using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OMyEF
{
    public static class GeneratorExtensionMethods
    {
        const string GenerateODataAttributeName = "GenerateODataControllerAttribute";
        public static ITypeSymbol GetDbContextType(this InvocationExpressionSyntax expProperty, Compilation compilation)
        {
            SemanticModel model = compilation.GetSemanticModel(expProperty.SyntaxTree);
            var expPropertySymbolInfo = model.GetSymbolInfo(expProperty);
            if (expPropertySymbolInfo.Symbol is IMethodSymbol methSymbol
                && methSymbol.TypeArguments.Count() == 1)
            {
                return methSymbol.TypeArguments[0];
            }
            return null;
        }
        public static Dictionary<string, ITypeSymbol> GetControllerTypes(this ITypeSymbol typeSymbol)
        {
            Dictionary<string, ITypeSymbol> returnDictionary = new Dictionary<string, ITypeSymbol>();
            foreach(var m in typeSymbol.GetMembers())
            {
                if(m is IPropertySymbol propSymbol)
                {
                    var type = (INamedTypeSymbol)propSymbol.Type;
                    var genericType = type.TypeArguments.FirstOrDefault();
                    if(genericType == null) { continue; }
                    if(genericType.GetAttributes().Where(p => p.AttributeClass.Name == GenerateODataAttributeName).Any())
                    {
                        returnDictionary.Add(m.Name, genericType);
                    }
                }
            }
            return returnDictionary;
        }
        public static IPropertySymbol GetDbSetKey(this ITypeSymbol typeSymbol)
        {
            foreach(var member in typeSymbol.GetMembers())
            {
                if(member is IPropertySymbol pSymbol)
                {
                    if(pSymbol.GetAttributes().Where(p => p.AttributeClass.Name == "KeyAttribute").Any())
                    {
                        return pSymbol;
                    }
                }
            }
            return null;
        }
        public static T GetPropertyValue<T>(this AttributeData attributeData, string KeyName)
        {
            var namedArgsValue = attributeData.NamedArguments.SingleOrDefault(p => p.Key == KeyName).Value;
            if(namedArgsValue.Value != null)
            {
                if(namedArgsValue.Value is T)
                {
                    return (T)namedArgsValue.Value;
                }
                return (T)Convert.ChangeType(namedArgsValue.Value, typeof(T));
            }
            return default(T);
        }
    }
}
