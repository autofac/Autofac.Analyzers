using System.Threading.Tasks;
using Xunit;

namespace Autofac.Analyzers.Tests
{
    public class DelegateRegistrationMissingAsAnalyzerTests : BaseDiagnosticTest<DelegateRegistrationMissingAsAnalyzer>
    {

        //No diagnostics expected to show up
        [Fact]
        public async Task EmptyContentNoDiagnostics()
        {
            var test = @"";

            await Verify(test);
        }

        //Diagnostic and CodeFix both triggered and checked for
        [Fact]
        public async Task RaisesDiagnosticForMissingCall()
        {
            var test = @"
    using Autofac;

    namespace MyAutofacApp
    {
        class Test
        {
            interface ITestService {}
            class TestClass {}

            void Run()
            {
                var builder = new ContainerBuilder();
                builder.Register(c => new TestClass());
            }
        }
    }";
            var expected = Diagnostic(Descriptors.Autofac1000_DelegateRegistrationNeedsAs)
                                      .WithLocation(11, 15);

            await Verify(test, expected);
        }

        [Fact]
        public async Task NoDiagnosticIfAsCallIsPresent()
        {
            var test = @"
    using Autofac;

    namespace MyAutofacApp
    {
        class Test
        {
            interface ITestService {}

            class TestClass {}

            void Run()
            {
                var builder = new ContainerBuilder();
                builder.Register(c => new TestClass()).As<ITestService>();
            }
        }
    }";
            await Verify(test);
        }


        [Fact]
        public async Task CanTrackRegistrationObject()
        {
            var test = @"
    using Autofac;

    namespace MyAutofacApp
    {
        class Test
        {
            interface ITestService {}

            class TestClass {}

            void Run()
            {
                var builder = new ContainerBuilder();
                var tracked = builder.Register(c => new TestClass());

                tracked.As<ITestService>();
            }
        }
    }";
            await Verify(test);
        }
    }
}
