using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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


        public void Execute(GeneratorExecutionContext context)
        {
            if (!System.Diagnostics.Debugger.IsAttached)
            {
 //System.Diagnostics.Debugger.Launch();
            }

            if (!(context.SyntaxReceiver is SyntaxReceiver receiver))
                return;
            
            foreach (InvocationExpressionSyntax expProperty in receiver.CandidateExpressionProperties)
            {
                var dbContextType = expProperty.GetDbContextType(context.Compilation);
                
                var controllers = dbContextType.GetControllerTypes();
                
                foreach(var controller in controllers)
                {
                    var dbSetKey = controller.Value.GetDbSetKey();
                    var controllerSetting = new OMyEFControllerBuilder()
                    {
                        DbSetPropertyName = controller.Key,
                        DbSetPropertyType = controller.Value.Name,
                        BaseRoute = GetBaseRoute(dbContextType),
                        DbContextType = dbContextType.Name,
                        DbContextNamespace = dbContextType.ContainingNamespace.ToString(),
                        DbSetNamespace = controller.Value.ContainingNamespace.ToString(),
                        KeyName = dbSetKey?.Name,
                        KeyType = dbSetKey?.Type.Name
                    };
                    var source = controllerSetting.Build();
                    context.AddSource(controller.Key + "_ODataGenerated.cs", SourceText.From(source, Encoding.UTF8));
                }
            }

        }
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }
        private string GetBaseRoute(ITypeSymbol typeSymbol)
        {
            foreach(var a in typeSymbol.GetAttributes())
            {
                if(a.AttributeClass.Name == "GenerateODataRouteAttribute")
                {
                    var attValue = a.GetPropertyValue<string>("BaseRoute");
                    if (!string.IsNullOrEmpty(attValue))
                    {
                        return attValue;
                    }
                }
            }

            return "odata";
        }
    }
}
