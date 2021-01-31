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
                var odataAttribute = GetGenerateODataAttribute(dbContextType);

                var controllers = dbContextType.GetControllerTypes();
                
                foreach(var controller in controllers)
                {
                    var dbSetKey = controller.Value.GetDbSetKey();
                    var controllerOdataAttribute = GetGenerateODataControllerAttribute(controller.Value);
                    string setName = "";
                    setName = controllerOdataAttribute?.GetPropertyValue<string>("SetName");
                    if (String.IsNullOrEmpty(setName))
                    {
                        setName = controller.Key;
                    }
                    var controllerSetting = new OMyEFControllerBuilder()
                    {
                        ODataSetName = setName,
                        DbSetPropertyType = controller.Value.Name,
                        BaseRoute = GetBaseRoute(odataAttribute),
                        DbContextType = dbContextType.Name,
                        DbContextNamespace = dbContextType.ContainingNamespace.ToString(),
                        DbSetNamespace = controller.Value.ContainingNamespace.ToString(),
                        KeyName = dbSetKey?.Name,
                        KeyType = dbSetKey?.Type.Name,
                        Authorize = GetAuthorizeAttribute(odataAttribute, controllerOdataAttribute),
                        AuthorizePolicy = GetAuthorizePolicy(odataAttribute, controllerOdataAttribute, controller.Key),
                        AuthorizeRoles = GetAuthorizeRoles(odataAttribute, controllerOdataAttribute, controller.Key)
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
        private AttributeData GetGenerateODataAttribute(ITypeSymbol typeSymbol)
        {
            foreach (var a in typeSymbol.GetAttributes())
            {
                if (a.AttributeClass.Name == "GenerateODataAttribute")
                {
                    return a;
                }
            }
            return null;
        }
        private AttributeData GetGenerateODataControllerAttribute(ITypeSymbol typeSymbol)
        {
            foreach (var a in typeSymbol.GetAttributes())
            {
                if (a.AttributeClass.Name == "GenerateODataControllerAttribute")
                {
                    return a;
                }
            }
            return null;
        }
        private string GetBaseRoute(AttributeData a)
        {
            var attValue = a?.GetPropertyValue<string>("BaseRoute");
            if (!string.IsNullOrEmpty(attValue))
            {
                return attValue;
            }
            return "odata";
        }
        private bool GetAuthorizeAttribute(AttributeData dbContextAttribute, AttributeData controllerAttribute)
        {
            // If the DbSet specifies Authorize policies - return true
            var authPolicy = controllerAttribute?.GetPropertyValue<string>("AuthorizePolicy");
            var authRoles = controllerAttribute?.GetPropertyValue<string>("AuthorizeRoles");
            if (!String.IsNullOrEmpty(authPolicy)) { return true; }
            if (!String.IsNullOrEmpty(authRoles)) { return true; }
            
            // If the DbContext specifies Authorize policies - return true
            authPolicy = dbContextAttribute?.GetPropertyValue<string>("AuthorizePolicy");
            authRoles = dbContextAttribute?.GetPropertyValue<string>("AuthorizeRoles");
            if (!String.IsNullOrEmpty(authPolicy)) { return true; }
            if (!String.IsNullOrEmpty(authRoles)) { return true; }

            var controllerAttValue = controllerAttribute?.GetPropertyValue<bool>("Authorize");
            if (controllerAttValue.HasValue && controllerAttValue.Value)
            {
                return controllerAttValue.Value;
            }
            var attValue = dbContextAttribute?.GetPropertyValue<bool>("Authorize");
            if (attValue.HasValue && attValue.Value)
            {
                return attValue.Value;
            }
            
            return false;
        }
        private string GetAuthorizePolicy(AttributeData dbContextAttribute, AttributeData controllerAttribute, string controllerName)
        {
            // If the DbSet specifies Authorize policies - return true
            var authPolicy = controllerAttribute?.GetPropertyValue<string>("AuthorizePolicy");
            if (String.IsNullOrEmpty(authPolicy)) 
            {
                authPolicy = dbContextAttribute?.GetPropertyValue<string>("AuthorizePolicy");
            }
            if (!String.IsNullOrEmpty(authPolicy)) 
            {
                return authPolicy.Replace("[controller]", controllerName);
            }
            return null;
        }
        private string GetAuthorizeRoles(AttributeData dbContextAttribute, AttributeData controllerAttribute, string controllerName)
        {
            // If the DbSet specifies Authorize policies - return true
            var authRoles = controllerAttribute?.GetPropertyValue<string>("AuthorizeRoles");
            if (String.IsNullOrEmpty(authRoles)) 
            {
                authRoles = dbContextAttribute?.GetPropertyValue<string>("AuthorizeRoles");
            }

            // If the DbContext specifies Authorize policies - return true
            
            if (!String.IsNullOrEmpty(authRoles)) {
                return authRoles.Replace("[controller]",controllerName); 
            }
            return null;
        }
    }
}
