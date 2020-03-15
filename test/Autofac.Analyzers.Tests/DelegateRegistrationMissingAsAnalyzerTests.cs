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
                                      .WithLocation(14, 17);

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

                tracked = tracked.SingleInstance();

                tracked.As<ITestService>();
            }
        }
    }";
            await Verify(test);
        }

        [Fact]
        public async Task CanRaiseIssueWithFirstReAssignedBuilder()
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

                tracked = tracked.SingleInstance();

                tracked = builder.Register(c => new TestClass());

                tracked.SingleInstance().As<ITestService>();
            }
        }
    }";

            var expected = Diagnostic(Descriptors.Autofac1000_DelegateRegistrationNeedsAs)
                                      .WithLocation(15, 31);

            await Verify(test, expected);
        }

        [Fact]
        public async Task CanRaiseIssueWithSecondReAssignedBuilder()
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
                var tracked = builder.Register(c => new TestClass()).As<ITestService>();

                tracked = tracked.SingleInstance();

                tracked = builder.Register(c => new TestClass());

                tracked.SingleInstance();
            }
        }
    }";

            var expected = Diagnostic(Descriptors.Autofac1000_DelegateRegistrationNeedsAs)
                                      .WithLocation(19, 27);

            await Verify(test, expected);
        }

        [Fact]
        public async Task CanTrackReAssignedBuilder()
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
                var tracked = builder.Register(c => new TestClass()).As<ITestService>();

                tracked = builder.Register(c => new TestClass());

                tracked.As<ITestService>();
            }
        }
    }";
            await Verify(test);
        }
    }
}
