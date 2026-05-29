using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Autofac.Analyzers.Tests.Helpers
{
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
}
