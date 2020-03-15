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
        private const string DelegateRegistrationMethodNames = "Register";

        public DelegateRegistrationMissingAsAnalyzer() 
            : base(Descriptors.Autofac1000_DelegateRegistrationNeedsAs)
        {
        }

        protected override void Analyze(RegistrationSyntaxContext registrationContext)
        {
            // Check if the method is a delegate register method.

            if(registrationContext.RootRegistrationMethod.Name == DelegateRegistrationMethodNames)
            {
                // This is a delegate registration; now we need to walk back up the expression tree
                // to find any As<> methods.
                if(!registrationContext.BuilderCalls.Any(c => c.InvokedMethod.Name == "As"))
                {
                    // No 'As' method in the registration.
                    registrationContext.ReportDiagnostic(
                        Diagnostic.Create(
                            Descriptors.Autofac1000_DelegateRegistrationNeedsAs,
                            registrationContext.GetRegistrationLocation()));
                }
            }
        }
    }
}
