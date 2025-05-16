using System;
using Shouldly;
using MediatR;
using NSubstitute;
using Xunit;

namespace Nut.MediatR.ServiceLike.Test;

public class RequestContextTest
{
    private IServiceProvider CreateMockServiceProvider<T>(Func<Type, T> getService)
    {
        var provider = Substitute.For<IServiceProvider>();
        provider.GetService(Arg.Any<Type>())
            .Returns(getService);
        return provider;
    }

    [Fact]
    public void ctor_pathがnullの場合は例外が発生する()
    {
        Should.Throw<ArgumentException>(() => new RequestContext(null, typeof(ServicePing), CreateMockServiceProvider<object>(_ => null), typeof(Pong)));
    }

    [Fact]
    public void ctor_pathが空文字の場合は例外が発生する()
    {
        Should.Throw<ArgumentException>(() => new RequestContext(string.Empty, typeof(ServicePing), CreateMockServiceProvider<object>(_ => null), typeof(Pong)));
    }

    [Fact]
    public void ctor_pathが空白文字の場合は例外が発生する()
    {
        Should.Throw<ArgumentException>(() => new RequestContext(" ", typeof(ServicePing), CreateMockServiceProvider<object>(_ => null), typeof(Pong)));
    }

    [Fact]
    public void ctor_mediatorParameterTypeがnullの場合は例外が発生する()
    {
        Should.Throw<ArgumentException>(() => new RequestContext("/this/is/path", null, CreateMockServiceProvider<object>(_ => null), typeof(Pong)));
    }

    [Fact]
    public void ctor_serviceFactoryがnullの場合は例外が発生する()
    {
        Should.Throw<ArgumentException>(() => new RequestContext("/this/is/path", typeof(ServicePing), null, typeof(Pong)));
    }

    [Fact]
    public void ctor_コンストラクタで設定した値がプロパティで取得できる()
    {
        var path = "/this/is/path";
        var mediatorParameterType = typeof(ServicePing);
        var clientResultType = typeof(Pong);
        var serviceFactory = CreateMockServiceProvider<object>(_ => null);

        var context = new RequestContext(path, mediatorParameterType, serviceFactory, clientResultType);

        context.Path.ShouldBe(path);
        context.MediatorParameterType.ShouldBe(mediatorParameterType);
        context.ClientResultType.ShouldBe(clientResultType);
        context.ServiceProvider.ShouldBe(serviceFactory);
    }

    [Fact]
    public void NeedClientResult_ClientResultTypeがnullの場合はfalseになる()
    {
        var path = "/this/is/path";
        var mediatorParameterType = typeof(ServicePing);
        var serviceFactory = CreateMockServiceProvider<object>(_ => null);

        var context = new RequestContext(path, mediatorParameterType, serviceFactory);
        context.ClientResultType.ShouldBeNull();
        context.NeedClientResult.ShouldBeFalse();
    }

    [Fact]
    public void NeedClientResult_ClientResultTypeがある場合はtrueになる()
    {
        var path = "/this/is/path";
        var mediatorParameterType = typeof(ServicePing);
        var serviceFactory = CreateMockServiceProvider<object>(_ => null);
        var clientResultType = typeof(Pong);

        var context = new RequestContext(path, mediatorParameterType, serviceFactory, clientResultType);
        context.ClientResultType.ShouldBe(clientResultType);
        context.NeedClientResult.ShouldBeTrue();
    }
}
