using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Autofac.Analyzers
{


    public class RegistrationSyntaxContext
    {
        private readonly SyntaxNodeAnalysisContext analysisContext;

        public RegistrationSyntaxContext(SyntaxNodeAnalysisContext nodeContext, IMethodSymbol methodSymbol, InvocationExpressionSyntax invocation, AutofacTypeContext types)
        {
            this.analysisContext = nodeContext;
            this.RootRegistrationMethod = methodSymbol;
            RootInvocationSyntax = invocation;
            AutofacTypes = types;
            BuilderCalls = new EnumerableRegistrationCalls(this);
        }

        public SemanticModel SemanticModel => analysisContext.SemanticModel;

        public CancellationToken CancellationToken => analysisContext.CancellationToken;

        public IMethodSymbol RootRegistrationMethod { get; }

        public InvocationExpressionSyntax RootInvocationSyntax { get; }

        public AutofacTypeContext AutofacTypes { get; }

        public IEnumerable<RegistrationBuilderInvocationContext> BuilderCalls { get; }

        public void ReportDiagnostic(Diagnostic diagnostic)
        {
            analysisContext.ReportDiagnostic(diagnostic);
        }

        public Location GetRegistrationLocation()
        {
            return RootInvocationSyntax.GetLocation();
        }

        private class EnumerableRegistrationCalls : IEnumerable<RegistrationBuilderInvocationContext>
        {
            private readonly RegistrationSyntaxContext registrationSyntaxContext;

            public EnumerableRegistrationCalls(RegistrationSyntaxContext registrationSyntaxContext)
            {
                this.registrationSyntaxContext = registrationSyntaxContext;
            }

            public IEnumerator<RegistrationBuilderInvocationContext> GetEnumerator()
            {
                return new RegistrationBuilderExpressionEnumerator(registrationSyntaxContext);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return new RegistrationBuilderExpressionEnumerator(registrationSyntaxContext);
            }
        }
    }
}
