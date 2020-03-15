using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections;
using System.Collections.Generic;

namespace Autofac.Analyzers
{
    public class RegistrationBuilderInvocationContext
    {
        public RegistrationBuilderInvocationContext(IMethodSymbol invokedMethod, InvocationExpressionSyntax invocation)
        {
            InvokedMethod = invokedMethod;
            Invocation = invocation;
        }

        public IMethodSymbol InvokedMethod { get; }
        public InvocationExpressionSyntax Invocation { get; }
    }


    class RegistrationBuilderExpressionEnumerator : IEnumerator<RegistrationBuilderInvocationContext>
    {
        private readonly RegistrationSyntaxContext registrationContext;

        private InvocationExpressionSyntax currentInvocationExpression;
        private RegistrationBuilderInvocationContext currentInvocationContext;
        private ILocalSymbol trackingSymbol;
        private IEnumerator<SyntaxNode> blockWalkingEnumerator;

        public RegistrationBuilderExpressionEnumerator(RegistrationSyntaxContext registrationContext)
        {
            this.registrationContext = registrationContext;
        }

        public RegistrationBuilderInvocationContext Current => currentInvocationContext;

        object IEnumerator.Current => currentInvocationContext;

        public bool MoveNext()
        {
            // Moving to the next registration builder call involves:
            //  - Looking at the parent expression.
            if(currentInvocationExpression == null)
            {
                currentInvocationExpression = registrationContext.RootInvocationSyntax;
            }

            var nextParent = currentInvocationExpression.Parent;

            // We want to walk up the expression tree, and depending on the parent, we will do different things.
            while (nextParent is object)
            {
                // If we've hit an invocation expression, lets inspect the method.
                // A method that takes an IRegistrationBuilder of some form will be considered.
                if (nextParent is InvocationExpressionSyntax invocExpr)
                {
                    var symbolInfo = registrationContext.SemanticModel.GetSymbolInfo(invocExpr, registrationContext.CancellationToken);

                    // We're calling a method. Check if it takes a registration builder as the first parameter.
                    if (symbolInfo.Symbol?.Kind == SymbolKind.Method)
                    {
                        var methodSymbol = (IMethodSymbol)symbolInfo.Symbol;

                        if (TestForRegistrationBuilder(invocExpr, methodSymbol))
                        {
                            currentInvocationExpression = invocExpr;
                            currentInvocationContext = new RegistrationBuilderInvocationContext(methodSymbol, invocExpr);
                            return true;
                        }
                    }
                }
                else if(nextParent is LocalDeclarationStatementSyntax localDeclareSyntax)
                {
                    // The registration has been assigned to a variable.
                    // If the variable is a registration builder, then follow it.
                    var variableAssignment = localDeclareSyntax.Declaration.Variables.FirstOrDefault();
                    var declaredSymbol = registrationContext.SemanticModel.GetDeclaredSymbol(variableAssignment) as ILocalSymbol;
                    
                    // Remember the tracking symbol.
                    trackingSymbol = declaredSymbol;

                    PopulateCodeBlockWalker(nextParent);

                    // Next parent.
                    nextParent = GetNextStartSearchNode();
                }
                else if(nextParent is AssignmentExpressionSyntax assignment)
                {
                    // If we assign the value to a variable, we need to make that target variable the tracking target.
                    var assignToSymbol = registrationContext.SemanticModel.GetSymbolInfo(assignment.Left);

                    if(assignToSymbol.Symbol is ILocalSymbol newLocal)
                    {
                        // Track it.
                        trackingSymbol = newLocal;

                        PopulateCodeBlockWalker(nextParent);

                        nextParent = GetNextStartSearchNode();
                    }
                    else
                    {
                        // The registration builder is being assigned to something other than a
                        // local variable. We can't track it anymore.
                        break;
                    }
                }
                else if(nextParent is ExpressionStatementSyntax)
                {
                    if (trackingSymbol is object)
                    {
                        // We're tracking something; we can keep going.
                        nextParent = GetNextStartSearchNode();
                    }
                    else
                    {
                        // Reached a standalone expression statement. We are done.
                        break;
                    }
                }
                else if(nextParent is BlockSyntax)
                {
                    // Reached the code block.
                    // Nothing to do.
                    break;
                }

                if(nextParent is null)
                {
                    break;
                }

                nextParent = nextParent.Parent;
            }

            return false;
        }

        private void PopulateCodeBlockWalker(SyntaxNode nextParent)
        {
            if(blockWalkingEnumerator is null)
            {
                // Get the containing code block.
                var codeBlock = nextParent.FirstAncestorOrSelf<BlockSyntax>();

                // All descendants in the code block after the declaration.
                blockWalkingEnumerator = codeBlock.DescendantNodes().GetEnumerator();
            }

            // Move it ahead until we get to the current block.
            while (blockWalkingEnumerator.MoveNext() && blockWalkingEnumerator.Current.SpanStart < nextParent.Span.End)
            {
            }
        }

        private SyntaxNode GetNextStartSearchNode()
        {
            var assigningToTracker = false;

            while(blockWalkingEnumerator.MoveNext())
            {
                var current = blockWalkingEnumerator.Current;

                if (current is AssignmentExpressionSyntax assignExpr)
                {
                    var accessSymbolInfo = registrationContext.SemanticModel.GetSymbolInfo(assignExpr.Left);

                    if (trackingSymbol.Equals(accessSymbolInfo.Symbol, SymbolEqualityComparer.Default))
                    {
                        // We are assigning to this tracking variable before anything else.
                        assigningToTracker = true;
                    }
                }
                else if (current is MemberAccessExpressionSyntax accessExpr)
                {
                    var accessSymbolInfo = registrationContext.SemanticModel.GetSymbolInfo(accessExpr.Expression);

                    if (trackingSymbol.Equals(accessSymbolInfo.Symbol, SymbolEqualityComparer.Default))
                    {
                        // We are in an expression that is accessing our value.
                        // This access expression now becomes the next parent.
                        // We can start searching from here.
                        return accessExpr;
                    }
                    else if(assigningToTracker)
                    {
                        return null;
                    }
                }
            }

            return null;
        }

        private bool TestForRegistrationBuilder(InvocationExpressionSyntax invocExpr, IMethodSymbol methodSymbol)
        {
            // Any method that functions as an extension method on IRegistrationBuilder<>
            // will be on a type constructed from that interface.
            var constructedFrom = methodSymbol.ContainingType.ConstructedFrom;

            if(constructedFrom.Equals(registrationContext.AutofacTypes.RegistrationBuilderInterface,
                                      SymbolEqualityComparer.Default))
            {
                return true;
            }

            return false;
        }

        public void Reset()
        {
            currentInvocationExpression = null;
        }

        public void Dispose()
        {
        }
    }
}
