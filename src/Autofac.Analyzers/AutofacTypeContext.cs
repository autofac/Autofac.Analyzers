using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Autofac.Analyzers
{
    public sealed class AutofacTypeContext
    {
        private const string ContainerBuilderName = "Autofac.ContainerBuilder";
        private const string RegistrationExtensionsName = "Autofac.RegistrationExtensions";
        private const string RegistrationBuilderInterfaceName = "Autofac.Builder.IRegistrationBuilder`3";

        private readonly Lazy<INamedTypeSymbol> _containerBuilderType;
        private readonly Lazy<INamedTypeSymbol> _registrationExtensionsType;
        private readonly Lazy<INamedTypeSymbol> _registrationBuilderInterface;
        private readonly Lazy<IModuleSymbol> _autofacModule;

        public AutofacTypeContext(Compilation compileContext)
        {
            _containerBuilderType = new Lazy<INamedTypeSymbol>(() => compileContext.GetTypeByMetadataName(ContainerBuilderName));
            _registrationExtensionsType = new Lazy<INamedTypeSymbol>(() => compileContext.GetTypeByMetadataName(RegistrationExtensionsName));
            _registrationBuilderInterface = new Lazy<INamedTypeSymbol>(() => compileContext.GetTypeByMetadataName(RegistrationBuilderInterfaceName));
            _autofacModule = new Lazy<IModuleSymbol>(() => ContainerBuilder.ContainingModule);
        }

        public INamedTypeSymbol ContainerBuilder => _containerBuilderType.Value;

        public INamedTypeSymbol RegistrationExtensions => _registrationExtensionsType.Value;

        public INamedTypeSymbol RegistrationBuilderInterface => _registrationBuilderInterface.Value;

        public IModuleSymbol AutofacModule => _autofacModule.Value;
    }
}
