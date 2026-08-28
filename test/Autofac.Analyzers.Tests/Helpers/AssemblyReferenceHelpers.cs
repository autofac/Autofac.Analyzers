// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Reflection;
using Microsoft.CodeAnalysis;

namespace Autofac.Analyzers.Tests.Helpers;

internal class AssemblyReferenceHelpers
{
#if NETCOREAPP
    internal static readonly MetadataReference SystemRuntimeReference = GetAssemblyReference(typeof(AssemblyReferenceHelpers).Assembly.GetReferencedAssemblies(), "System.Runtime");
    internal static readonly MetadataReference NetStandardReference = GetAssemblyReference(typeof(ContainerBuilder).Assembly.GetReferencedAssemblies(), "netstandard");
#endif

    internal static readonly MetadataReference AutofacReference = MetadataReference.CreateFromFile(typeof(ContainerBuilder).Assembly.Location);

    private static PortableExecutableReference GetAssemblyReference(IEnumerable<AssemblyName> assemblies, string name)
    {
        return MetadataReference.CreateFromFile(Assembly.Load(assemblies.First(n => n.Name == name)).Location);
    }
}
