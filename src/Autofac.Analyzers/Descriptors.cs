// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.CodeAnalysis;

using static Autofac.Analyzers.Category;
using static Microsoft.CodeAnalysis.DiagnosticSeverity;

namespace Autofac.Analyzers;

public enum Category
{
    Registration,
}

internal static class Descriptors
{
    internal static DiagnosticDescriptor Autofac1000_DelegateRegistrationNeedsAs
    {
        get;
    }

    = Rule(
        "Autofac1000",
        nameof(Autofac1000_DelegateRegistrationNeedsAs),
        Registration,
        Warning);

    private static DiagnosticDescriptor Rule(string id, string textName, Category category, DiagnosticSeverity defaultSeverity)
    {
        var title = new LocalizableResourceString(textName + "_Title", Resources.ResourceManager, typeof(Resources));
        var description = new LocalizableResourceString(textName + "_Description", Resources.ResourceManager, typeof(Resources));

        // This documentation needs to be created.
        var helpLink = $"https://autofac.readthedocs.io/en/latest/rules/{id}";
        var isEnabledByDefault = true;
        return new DiagnosticDescriptor(id, title, title, category.ToString(), defaultSeverity, isEnabledByDefault, description, helpLink);
    }
}
