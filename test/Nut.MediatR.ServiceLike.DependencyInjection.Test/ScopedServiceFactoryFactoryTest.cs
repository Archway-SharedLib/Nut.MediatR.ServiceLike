using System;
using Shouldly;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Nut.MediatR.ServiceLike.DependencyInjection.Test;

public class ScopedServiceFactoryFactoryTest
{
    [Fact]
    public void ctor_パラメーターがnullの場合は例外が発生する()
    {
        Should.Throw<ArgumentNullException>(() => new ScopedServiceFactoryFactory(null!));
    }

    [Fact]
    public void Create_Scopeが返ってくる()
    {
        var services = new ServiceCollection();
        var provider = services.BuildServiceProvider();
        var factory = new ScopedServiceFactoryFactory(provider.GetService<IServiceScopeFactory>());
        factory.Create().ShouldNotBeNull();
    }
}
