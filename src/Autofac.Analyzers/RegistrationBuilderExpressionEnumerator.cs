using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Autofac.Analyzers
{
    public class RegistrationBuilderInvocationContext
    {
        public RegistrationBuilderInvocationContext(IMethodSymbol invokedMethod, InvocationExpressionSyntax invocation)
        {
            InvokedMethod = invokedMethod;
            Invocation = invocation;
        }

        public IMethodSymbol InvokedMethod
        {
            get;
        }
        public InvocationExpressionSyntax Invocation
        {
            get;
        }
    }


    internal class RegistrationBuilderExpressionEnumerator : IEnumerator<RegistrationBuilderInvocationContext>
    {
        private readonly RegistrationSyntaxContext _registrationContext;

        private InvocationExpressionSyntax _currentInvocationExpression;
        private RegistrationBuilderInvocationContext _currentInvocationContext;
        private ILocalSymbol _trackingSymbol;
        private IEnumerator<SyntaxNode> _blockWalkingEnumerator;

        public RegistrationBuilderExpressionEnumerator(RegistrationSyntaxContext registrationContext)
        {
            this._registrationContext = registrationContext;
        }

        public RegistrationBuilderInvocationContext Current => _currentInvocationContext;

        object IEnumerator.Current => _currentInvocationContext;

        public bool MoveNext()
        {
            // Moving to the next registration builder call involves:
            //  - Looking at the parent expression.
            if (_currentInvocationExpression == null)
            {
                _currentInvocationExpression = _registrationContext.RootInvocationSyntax;
            }

            var nextParent = _currentInvocationExpression.Parent;

            // We want to walk up the expression tree, and depending on the parent, we will do different things.
            while (nextParent is object)
            {
                var result = ProcessParentNode(ref nextParent);

                if (result.HasValue)
                {
                    return result.Value;
                }

                if (nextParent is null)
                {
                    break;
                }

                nextParent = nextParent.Parent;
            }

            return false;
        }

        private bool? ProcessParentNode(ref SyntaxNode nextParent)
        {
            // If we've hit an invocation expression, lets inspect the method.
            // A method that takes an IRegistrationBuilder of some form will be considered.
            if (nextParent is InvocationExpressionSyntax invocExpr)
            {
                if (TryHandleInvocationExpression(invocExpr))
                {
                    return true;
                }
            }
            else if (nextParent is LocalDeclarationStatementSyntax localDeclareSyntax)
            {
                // The registration has been assigned to a variable.
                // If the variable is a registration builder, then follow it.
                HandleLocalDeclaration(localDeclareSyntax, ref nextParent);
            }
            else if (nextParent is AssignmentExpressionSyntax assignment)
            {
                // If we assign the value to a variable, we need to make that target variable the tracking target.
                if (!HandleAssignment(assignment, ref nextParent))
                {
                    // The registration builder is being assigned to something other than a
                    // local variable. We can't track it anymore.
                    return false;
                }
            }
            else if (nextParent is ExpressionStatementSyntax)
            {
                if (!HandleExpressionStatement(ref nextParent))
                {
                    // Reached a standalone expression statement. We are done.
                    return false;
                }
            }
            else if (nextParent is BlockSyntax)
            {
                // Reached the code block. Nothing to do.
                return false;
            }

            return null;
        }

        private bool TryHandleInvocationExpression(InvocationExpressionSyntax invocExpr)
        {
            var symbolInfo = _registrationContext.SemanticModel.GetSymbolInfo(invocExpr, _registrationContext.CancellationToken);

            // We're calling a method. Check if it takes a registration builder as the first parameter.
            if (symbolInfo.Symbol?.Kind != SymbolKind.Method)
            {
                return false;
            }

            var methodSymbol = (IMethodSymbol)symbolInfo.Symbol;

            if (!TestForRegistrationBuilder(methodSymbol))
            {
                return false;
            }

            _currentInvocationExpression = invocExpr;
            _currentInvocationContext = new RegistrationBuilderInvocationContext(methodSymbol, invocExpr);
            return true;
        }

        private void HandleLocalDeclaration(LocalDeclarationStatementSyntax localDeclareSyntax, ref SyntaxNode nextParent)
        {
            var variableAssignment = localDeclareSyntax.Declaration.Variables.FirstOrDefault();
            var declaredSymbol = _registrationContext.SemanticModel.GetDeclaredSymbol(variableAssignment) as ILocalSymbol;

            // Remember the tracking symbol.
            _trackingSymbol = declaredSymbol;

            PopulateCodeBlockWalker(localDeclareSyntax);

            // Next parent.
            nextParent = GetNextStartSearchNode();
        }

        private bool HandleAssignment(AssignmentExpressionSyntax assignment, ref SyntaxNode nextParent)
        {
            var assignToSymbol = _registrationContext.SemanticModel.GetSymbolInfo(assignment.Left);

            if (assignToSymbol.Symbol is ILocalSymbol newLocal)
            {
                // Track it.
                _trackingSymbol = newLocal;

                PopulateCodeBlockWalker(assignment);

                nextParent = GetNextStartSearchNode();
                return true;
            }

            return false;
        }

        private bool HandleExpressionStatement(ref SyntaxNode nextParent)
        {
            if (_trackingSymbol is object)
            {
                // We're tracking something; we can keep going.
                nextParent = GetNextStartSearchNode();
                return true;
            }

            return false;
        }

        private void PopulateCodeBlockWalker(SyntaxNode nextParent)
        {
            if (_blockWalkingEnumerator is null)
            {
                // Get the containing code block.
                var codeBlock = nextParent.FirstAncestorOrSelf<BlockSyntax>();

                // All descendants in the code block after the declaration.
                _blockWalkingEnumerator = codeBlock.DescendantNodes().GetEnumerator();
            }

            while (_blockWalkingEnumerator.MoveNext() && _blockWalkingEnumerator.Current.SpanStart < nextParent.Span.End)
            {
                // Move it ahead until we get to the current block.
            }
        }

        private SyntaxNode GetNextStartSearchNode()
        {
            var assigningToTracker = false;

            while (_blockWalkingEnumerator.MoveNext())
            {
                var current = _blockWalkingEnumerator.Current;

                if (current is AssignmentExpressionSyntax assignExpr)
                {
                    var accessSymbolInfo = _registrationContext.SemanticModel.GetSymbolInfo(assignExpr.Left);

                    if (_trackingSymbol.Equals(accessSymbolInfo.Symbol, SymbolEqualityComparer.Default))
                    {
                        // We are assigning to this tracking variable before anything else.
                        assigningToTracker = true;
                    }
                }
                else if (current is MemberAccessExpressionSyntax accessExpr)
                {
                    var accessSymbolInfo = _registrationContext.SemanticModel.GetSymbolInfo(accessExpr.Expression);

                    if (_trackingSymbol.Equals(accessSymbolInfo.Symbol, SymbolEqualityComparer.Default))
                    {
                        // We are in an expression that is accessing our value.
                        // This access expression now becomes the next parent.
                        // We can start searching from here.
                        return accessExpr;
                    }
                    else if (assigningToTracker)
                    {
                        return null;
                    }
                }
            }

            return null;
        }

        private bool TestForRegistrationBuilder(IMethodSymbol methodSymbol)
        {
            // Any method that functions as an extension method on IRegistrationBuilder<>
            // will be on a type constructed from that interface.
            var constructedFrom = methodSymbol.ContainingType.ConstructedFrom;

            if (constructedFrom.Equals(_registrationContext.AutofacTypes.RegistrationBuilderInterface,
                                      SymbolEqualityComparer.Default))
            {
                return true;
            }

            return false;
        }

        public void Reset()
        {
            _currentInvocationExpression = null;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _blockWalkingEnumerator?.Dispose();
            }
        }
    }
}
