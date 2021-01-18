using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace OMyEF
{
    [Generator]
    public class ODataGenerator : ISourceGenerator
    {
        
        private const string attributeText = @"
        using System;
        namespace OMyEF
        {
            [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
            public class GenerateODataControllerAttribute : Attribute
            {
                public GenerateODataControllerAttribute()
                {
                }
                public string KeyName { get; set; }
                public string KeyType { get; set; }
            }
        }
        ";
        
        public void Execute(GeneratorExecutionContext context)
        {
            context.AddSource("GenerateODataControllerAttribute", SourceText.From(attributeText, Encoding.UTF8));
            
            if (!(context.SyntaxReceiver is SyntaxReceiver receiver))
                return;

            CSharpParseOptions options = (context.Compilation as CSharpCompilation).SyntaxTrees[0].Options as CSharpParseOptions;
            Compilation compilation = context.Compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(SourceText.From(attributeText, Encoding.UTF8), options));

            INamedTypeSymbol attributeSymbol = compilation.GetTypeByMetadataName("OMyEF.GenerateODataControllerAttribute");

            // loop over the candidate fields, and keep the ones that are actually annotated
            List<IPropertySymbol> propertySymbols = new List<IPropertySymbol>();
            foreach (var property in receiver.CandidateProperties)
            {
                SemanticModel model = compilation.GetSemanticModel(property.SyntaxTree);
                var dSymbol = model.GetDeclaredSymbol(property);
                if (dSymbol.GetAttributes().Any(ad => ad.AttributeClass.Equals(attributeSymbol, SymbolEqualityComparer.Default)))
                {
                    propertySymbols.Add(dSymbol);
                }
            }


#pragma warning disable RS1024 // Compare symbols correctly
            foreach (IGrouping<INamedTypeSymbol, IPropertySymbol> group in propertySymbols.GroupBy(f => f.ContainingType))
#pragma warning restore RS1024 // Compare symbols correctly
            {
                string classSource = ProcessClass(group.Key, group.ToList(), attributeSymbol, context);
                context.AddSource($"{group.Key.Name}_ODataGenerated.cs", SourceText.From(classSource, Encoding.UTF8));
            }
        }
        private string ProcessClass(INamedTypeSymbol classSymbol, List<IPropertySymbol> properties, ISymbol attributeSymbol, GeneratorExecutionContext context)
        {

            if (!classSymbol.ContainingSymbol.Equals(classSymbol.ContainingNamespace, SymbolEqualityComparer.Default))
            {
                return null;
            }

            string namespaceName = classSymbol.ContainingNamespace.ToDisplayString();
            StringBuilder source = new StringBuilder($@"
                using Microsoft.AspNetCore.Mvc;
                using System;
                using System.Linq;
                using Microsoft.AspNet.OData;
                using System.Collections.Generic;
                using System.Text;
                using {namespaceName};

                namespace OMyEF
                {{

            ");

            foreach (IPropertySymbol propSymbol in properties)
            {
                ProcessProperty(source, classSymbol, propSymbol, attributeSymbol);
            }

            source.Append("}");
            return source.ToString();
        }
        private void ProcessProperty(StringBuilder source, INamedTypeSymbol classSymbol, IPropertySymbol propSymbol, ISymbol attributeSymbol)
        {
            string fieldName = propSymbol.Name;
            ITypeSymbol fieldType = propSymbol.Type;
            AttributeData attributeData = propSymbol.GetAttributes().Single(ad => ad.AttributeClass.Equals(attributeSymbol, SymbolEqualityComparer.Default));
            TypedConstant keyNameConstant = attributeData.NamedArguments.SingleOrDefault(kvp => kvp.Key == "KeyName").Value;
            TypedConstant keyTypeConstant = attributeData.NamedArguments.SingleOrDefault(kvp => kvp.Key == "KeyType").Value;
            source.Append($@"
                [ApiController]
                public class {fieldName}Controller : ControllerBase
                {{
                    private {classSymbol.Name} _dbContext;
                    public {fieldName}Controller({classSymbol.Name} dbContext){{
                        _dbContext = dbContext;
                    }}

                    [HttpGet]
                    [EnableQuery]
                    [Route(""odata/[controller]"")]
                    public IQueryable<{fieldName}> Get()
                    {{
                        return _dbContext.{fieldName};
                    }}
            ");
            if (keyNameConstant.Value != null && keyTypeConstant.Value != null)
            {
                string keyName = keyNameConstant.Value.ToString();
                string keyType = keyTypeConstant.Value.ToString();
                source.Append($@"
                    [HttpGet]
                    [EnableQuery]
                    [Route(""odata/[controller]"")]
                    public SingleResult<{fieldName}> Get([FromODataUri] {keyType} key)
                    {{
                        return SingleResult.Create(_dbContext.{fieldName}.Where(p => p.{keyName} == key));
                    }}
                ");
            }

            source.Append("}");
        }
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }
    }

}
