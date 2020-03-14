using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Autofac.Analyzers
{
	using static Category;
	using static DiagnosticSeverity;

	public enum Category
	{
		Registration
	}

    internal static class Descriptors
    {
		internal static DiagnosticDescriptor Autofac1000_DelegateRegistrationNeedsAs { get; } = Rule(
			"Autofac1000",
			nameof(Autofac1000_DelegateRegistrationNeedsAs),
			Registration,
			Warning
			);

		static DiagnosticDescriptor Rule(string id, string textName, Category category, DiagnosticSeverity defaultSeverity)
		{
			var title = new LocalizableResourceString(textName + "_Title", Resources.ResourceManager, typeof(Resources));
			var description = new LocalizableResourceString(textName + "_Description", Resources.ResourceManager, typeof(Resources));

			// TODO: Needs to be created...
			var helpLink = $"https://autofac.readthedocs.io/en/latest/rules/{id}";
			var isEnabledByDefault = true;
			return new DiagnosticDescriptor(id, title, title, category.ToString(), defaultSeverity, isEnabledByDefault, description, helpLink);
		}
	}
}
