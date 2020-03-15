using System;
using System.Collections.Generic;
using System.Text;

namespace Autofac.Analyzers.Test
{
    // Syntax experiments for browsing the syntax tree!
    class SyntaxExperiments
    {
        interface ITestService { }

        class TestClass { }

        void Run()
        {
            var builder = new ContainerBuilder();
            var tracked = builder.Register(c => new TestClass()).As<ITestService>();

            tracked = tracked.SingleInstance();

            tracked = builder.Register(c => new TestClass());


            tracked.SingleInstance();

        }
    }
}
