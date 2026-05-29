using System;
using System.Collections.Generic;
using System.Text;

namespace Autofac.Analyzers.Test
{
    // Syntax experiments for browsing the syntax tree!
    internal class SyntaxExperiments
    {
        private interface ITestService
        {
        }

        private class TestClass
        {
        }

        private void Run()
        {
            var builder = new ContainerBuilder();
            var tracked = builder.Register(c => new TestClass()).As<ITestService>();

            tracked.SingleInstance();

            tracked = builder.Register(c => new TestClass());

            tracked.SingleInstance();
        }
    }
}
