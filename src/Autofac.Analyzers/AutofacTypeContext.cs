using Microsoft.CodeAnalysis;
using System;
using System.Linq;

namespace Autofac.Analyzers
{
    public class AutofacTypeContext
    {
        const string ContainerBuilderName = "Autofac.ContainerBuilder";
        const string RegistrationExtensionsName = "Autofac.RegistrationExtensions";
        const string RegistrationBuilderInterfaceName = "Autofac.Builder.IRegistrationBuilder`3";

        private class ExtensionMethodName
        {
            public ExtensionMethodName(string simple, Func<IMethodSymbol> predicate = null)
            {
                Simple = simple;
                Predicate = predicate;
            }

            public string Simple { get; }

            public Func<IMethodSymbol> Predicate { get; }
        }

        private Lazy<INamedTypeSymbol> containerBuilderType;
        private Lazy<INamedTypeSymbol> registrationExtensionsType;
        private Lazy<INamedTypeSymbol> registrationBuilderInterface;
        private Lazy<IModuleSymbol> autofacModule;

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
