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
            builder.Register(c => new TestClass()).As<ITestService>().As<ITestService>();
        }
    }
}
