using System;
using Shouldly;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Nut.MediatR.ServiceLike.DependencyInjection.Test;

public class ScopedServiceFactoryTest
{
    [Fact]
    public void ctor_パラメーターがない場合は例外が発生する()
    {
        Should.Throw<ArgumentNullException>(() => new ScopedServiceFactory(null!));
    }

    [Fact]
    public void ServiceFactory_スコープで区切られたServiceFactoryが取得できる()
    {
        var services = new ServiceCollection();
        services.AddScoped<Test>();
        var provider = services.BuildServiceProvider();
        using var outerScope = provider.CreateScope();
        var outerTest1 = outerScope.ServiceProvider.GetService<Test>();
        var outerTest2 = outerScope.ServiceProvider.GetService<Test>();

        outerTest1.ShouldBe(outerTest2);

        Test innerTest1 = null;
        Test innerTest2 = null;

        using (var scope = new ScopedServiceFactory(provider.GetService<IServiceScopeFactory>()))
        {
            innerTest1 = scope.Instance.GetService<Test>();
            innerTest2 = scope.Instance.GetService<Test>();

            innerTest1.ShouldBe(innerTest2);
            innerTest1.ShouldNotBe(outerTest1);
            innerTest1.Disposed.ShouldBeFalse();
            innerTest2.Disposed.ShouldBeFalse();
        }

        innerTest1.Disposed.ShouldBeTrue();
        innerTest2.Disposed.ShouldBeTrue();
    }

    private class Test : IDisposable
    {
        public bool Disposed { get; private set; } = false;
        public void Dispose()
        {
            Disposed = true;
        }
    }
}
