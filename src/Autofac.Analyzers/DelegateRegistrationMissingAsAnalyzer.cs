// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Autofac.Analyzers;

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
        // This is a delegate registration; now we need to walk back up the expression tree
        // to find any As<> methods.
        if (registrationContext.RootRegistrationMethod.Name == DelegateRegistrationMethodNames && !registrationContext.BuilderCalls.Any(c => c.InvokedMethod.Name == "As"))
        {
            // No 'As' method in the registration.
            registrationContext.ReportDiagnostic(
                Diagnostic.Create(
                    Descriptors.Autofac1000_DelegateRegistrationNeedsAs,
                    registrationContext.GetRegistrationLocation()));
        }
    }
}
