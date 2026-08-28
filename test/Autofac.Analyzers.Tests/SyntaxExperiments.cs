// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Analyzers.Test;

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
