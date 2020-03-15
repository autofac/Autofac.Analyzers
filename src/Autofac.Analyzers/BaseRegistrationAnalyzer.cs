using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Autofac.Analyzers
{

    public abstract class BaseRegistrationAnalyzer : DiagnosticAnalyzer
    {
        public BaseRegistrationAnalyzer(DiagnosticDescriptor diagnostic)
        {
            SupportedDiagnostics = ImmutableArray.Create(diagnostic);
        }

        public sealed override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.EnableConcurrentExecution();

            context.RegisterCompilationStartAction(compilationStartCtxt =>
            {
                var autofacTypeContext = new AutofacTypeContext(compilationStartCtxt.Compilation);

                compilationStartCtxt.RegisterSyntaxNodeAction(nodeContext =>
                {
                    var invocation = (InvocationExpressionSyntax) nodeContext.Node;

                    var symbolInfo = nodeContext.SemanticModel.GetSymbolInfo(invocation, nodeContext.CancellationToken);
                    if (symbolInfo.Symbol?.Kind != SymbolKind.Method)
                        return;

                    // We're looking for any methods where the first argument is a ContainerBuilder,
                    // or the ReducedFrom first argument. 
                   
                    var methodSymbol = (IMethodSymbol)symbolInfo.Symbol;

                    if(methodSymbol.ReducedFrom is object)
                    {
                        methodSymbol = methodSymbol.ReducedFrom;
                    }

                    if(methodSymbol.Parameters.Length > 0)
                    {
                        var firstParam = methodSymbol.Parameters[0];

                        if(firstParam.Type is INamedTypeSymbol namedSymbol &&
                           SymbolEqualityComparer.Default.Equals(namedSymbol, autofacTypeContext.ContainerBuilder))
                        {
                            // This is a registration method that jumps directly off
                            // the container builder, so we can start from here.

                            // This call is likely to be the lowest point in the call tree.
                            // The parents of this node contain the follow-on calls, and eventually
                            // the decision as to whether the result is assigned to something, or thrown away.

                            // Let's create our own context object for the registration.
                            // We will use this to allow scanning of the invocation tree.

                            var registrationContext = new RegistrationSyntaxContext(nodeContext, methodSymbol, invocation, autofacTypeContext);

                            Analyze(registrationContext);
                        }
                    }

                }, SyntaxKind.InvocationExpression);
            });
        }

        protected abstract void Analyze(RegistrationSyntaxContext registrationContext);
    }
}
