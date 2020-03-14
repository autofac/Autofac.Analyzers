using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Text;

namespace Autofac.Analyzers
{
    public abstract class BaseRegistrationAnalyzer : DiagnosticAnalyzer
    {
        const string ContainerBuilderName = "Autofac.ContainerBuilder";

        public BaseRegistrationAnalyzer(DiagnosticDescriptor diagnostic)
        {
            SupportedDiagnostics = ImmutableArray.Create(diagnostic);
        }

        public sealed override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }

        private class AutofacTypeContext
        {
            private Lazy<INamedTypeSymbol> containerBuilderType;

            public AutofacTypeContext(Compilation compileContext)
            {
                containerBuilderType = new Lazy<INamedTypeSymbol>(() => compileContext.GetTypeByMetadataName(ContainerBuilderName));
            }

            public INamedTypeSymbol ContainerBuilder => containerBuilderType.Value;
        }

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

                            // We need to find the top-level expression that contains each of the parts.
                            // There are two types of 'RegisterX' expression. 
                            // One that is assigned to a variable, and one that is not.
                            // We need to find the root of the expression tree that we want to inspect.
                            
                            ExpressionSyntax registrationExpression = invocation.Expression;
                            var foundParent = invocation.Parent;

                            // Scan the parents until we get to a code block.
                            while (foundParent is object && !(foundParent is BlockSyntax))
                            {
                                if(foundParent is InvocationExpressionSyntax invocSyntax)
                                {
                                    registrationExpression = invocSyntax.Expression;
                                }

                                if(foundParent is EqualsValueClauseSyntax equalsSyntax)
                                {
                                    // Assignment to variable.
                                    // TODO: Find the variable and track it.
                                    break;
                                }
                                else if(foundParent is ExpressionStatementSyntax expressionSyntax)
                                {
                                    registrationExpression = expressionSyntax.Expression;
                                    // Just straight execution; the result of the registration 
                                    // doesn't go anywhere.
                                    break;
                                }

                                foundParent = foundParent.Parent;
                            }

                            Analyze(registrationExpression, methodSymbol);
                        }
                    }

                }, SyntaxKind.InvocationExpression);
            });
        }

        protected abstract void Analyze(ExpressionSyntax invocation, IMethodSymbol methodSymbol);
    }
}
