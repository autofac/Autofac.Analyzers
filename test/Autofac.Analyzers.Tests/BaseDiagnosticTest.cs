using Autofac.Analyzers.Tests.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using System.Threading.Tasks;

namespace Autofac.Analyzers.Tests
{
    /// <summary>
    /// Superclass of all Unit Tests for DiagnosticAnalyzers
    /// </summary>
    public class BaseDiagnosticTest<TAnalyzer>
        where TAnalyzer : DiagnosticAnalyzer, new()
    {
        protected DiagnosticResult Diagnostic()
        {
            return CSharpCodeFixVerifier<TAnalyzer, EmptyCodeFixProvider, XUnitVerifier>.Diagnostic();
        }

        protected DiagnosticResult Diagnostic(string diagnosticId)
        {
            return CSharpCodeFixVerifier<TAnalyzer, EmptyCodeFixProvider, XUnitVerifier>.Diagnostic(diagnosticId);
        }

        protected DiagnosticResult Diagnostic(DiagnosticDescriptor descriptor)
        {
            return new DiagnosticResult(descriptor);
        }

        public async Task Verify(string source, params DiagnosticResult[] diagnostics)
        {
            var test = new Test();
            test.TestCode = source;
            test.ExpectedDiagnostics.AddRange(diagnostics);

            await test.RunAsync();
        }

        private class Test : CSharpCodeFixTest<TAnalyzer, EmptyCodeFixProvider, XUnitVerifier>
        {
            public Test()
            {
                SolutionTransforms.Add((solution, projId) =>
                {
                    solution = solution.AddMetadataReferences(projId, new[] {
                        AssemblyReferenceHelpers.SystemRuntimeReference,
                        AssemblyReferenceHelpers.NetStandardReference,
                        //AssemblyReferenceHelpers.SystemRuntimeExtensionsReference,
                        AssemblyReferenceHelpers.AutofacReference
                    });

                    return solution;
                });

                // xunit diagnostics are reported in both normal and generated code
                TestBehaviors |= TestBehaviors.SkipGeneratedCodeCheck;
            }
        }
    }
}
