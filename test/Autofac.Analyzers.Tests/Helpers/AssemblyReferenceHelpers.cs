using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Autofac.Analyzers.Tests.Helpers
{
    internal class AssemblyReferenceHelpers
    {
#if NETCOREAPP
        internal static readonly MetadataReference SystemRuntimeReference;
        internal static readonly MetadataReference NetStandardReference;
#endif

        internal static readonly MetadataReference AutofacReference = MetadataReference.CreateFromFile(typeof(ContainerBuilder).Assembly.Location);

        static AssemblyReferenceHelpers()
        {
            var testAssemblies = typeof(AssemblyReferenceHelpers).Assembly.GetReferencedAssemblies();
            var autofacAssemblies = typeof(ContainerBuilder).Assembly.GetReferencedAssemblies();
#if NETCOREAPP
            SystemRuntimeReference = GetAssemblyReference(testAssemblies, "System.Runtime");
            NetStandardReference = GetAssemblyReference(autofacAssemblies, "netstandard");
            //	SystemRuntimeExtensionsReference = GetAssemblyReference(referencedAssemblies, "System.Runtime.Extensions");
#endif
        }

        static PortableExecutableReference GetAssemblyReference(IEnumerable<AssemblyName> assemblies, string name)
        {
            return MetadataReference.CreateFromFile(Assembly.Load(assemblies.First(n => n.Name == name)).Location);
        }
    }

}
