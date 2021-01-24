using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Text;

namespace OMyEF
{
    internal class SyntaxReceiver : ISyntaxReceiver
    {
        public List<PropertyDeclarationSyntax> CandidateProperties { get; } = new List<PropertyDeclarationSyntax>();
        public List<InvocationExpressionSyntax> CandidateExpressionProperties { get; } = new List<InvocationExpressionSyntax>();
        /// <summary>
        /// Called for every syntax node in the compilation, we can inspect the nodes and save any information useful for generation
        /// </summary>
        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if( syntaxNode is InvocationExpressionSyntax iExpSyn)
            {
                if(iExpSyn.Expression is MemberAccessExpressionSyntax memberAccessExpression)
                {
                    if(memberAccessExpression.Name.Identifier.Text == "AddOMyEFRoute")
                    {
                        CandidateExpressionProperties.Add(iExpSyn);
                    }
                }
            }
            // any field with at least one attribute is a candidate for property generation
            if (syntaxNode is PropertyDeclarationSyntax propertyDeclarationSyntax
                && propertyDeclarationSyntax.AttributeLists.Count > 0)
            {
                CandidateProperties.Add(propertyDeclarationSyntax);
            }
        }
    }

}
