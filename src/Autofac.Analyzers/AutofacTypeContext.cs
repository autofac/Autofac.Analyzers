using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Autofac.Analyzers
{
    public class AutofacTypeContext
    {
        private const string ContainerBuilderName = "Autofac.ContainerBuilder";
        private const string RegistrationExtensionsName = "Autofac.RegistrationExtensions";
        private const string RegistrationBuilderInterfaceName = "Autofac.Builder.IRegistrationBuilder`3";

        private class ExtensionMethodName
        {
            public ExtensionMethodName(string simple, Func<IMethodSymbol> predicate = null)
            {
                Simple = simple;
                Predicate = predicate;
            }

            public string Simple
            {
                get;
            }

            public Func<IMethodSymbol> Predicate
            {
                get;
            }
        }

        private readonly Lazy<INamedTypeSymbol> containerBuilderType;
        private readonly Lazy<INamedTypeSymbol> registrationExtensionsType;
        private readonly Lazy<INamedTypeSymbol> registrationBuilderInterface;
        private readonly Lazy<IModuleSymbol> autofacModule;

        public AutofacTypeContext(Compilation compileContext)
        {
            containerBuilderType = new Lazy<INamedTypeSymbol>(() => compileContext.GetTypeByMetadataName(ContainerBuilderName));
            registrationExtensionsType = new Lazy<INamedTypeSymbol>(() => compileContext.GetTypeByMetadataName(RegistrationExtensionsName));
            registrationBuilderInterface = new Lazy<INamedTypeSymbol>(() => compileContext.GetTypeByMetadataName(RegistrationBuilderInterfaceName));
            autofacModule = new Lazy<IModuleSymbol>(() => ContainerBuilder.ContainingModule);
        }

        public INamedTypeSymbol ContainerBuilder => containerBuilderType.Value;

        public INamedTypeSymbol RegistrationExtensions => registrationExtensionsType.Value;

        public INamedTypeSymbol RegistrationBuilderInterface => registrationBuilderInterface.Value;

        public IModuleSymbol AutofacModule => autofacModule.Value;
    }
}
