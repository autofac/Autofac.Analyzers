using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Autofac.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DelegateRegistrationMissingAsAnalyzer : BaseRegistrationAnalyzer
    {
        public DelegateRegistrationMissingAsAnalyzer() 
            : base(Descriptors.Autofac1000_DelegateRegistrationNeedsAs)
        {
        }

        protected override void Analyze(ExpressionSyntax invocation, IMethodSymbol methodSymbol)
        {

        }
    }
}
