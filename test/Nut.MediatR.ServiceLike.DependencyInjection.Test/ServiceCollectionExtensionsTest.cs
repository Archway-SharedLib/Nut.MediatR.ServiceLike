using System;
using Shouldly;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace Nut.MediatR.ServiceLike.DependencyInjection.Test;

public class ServiceCollectionExtensionsTest
{
    [Fact]
    public void AddMediatRServiceLike_アセンブリを探索してAsServiceがついたIRequestが自動的に登録される()
    {
        var services = new ServiceCollection();
        services.AddMediatRServiceLike(typeof(ServicePing).Assembly);
        var provider = services.BuildServiceProvider();

        var registry = provider.GetService<ServiceRegistry>();
        registry.GetService("/ping").ShouldNotBeNull();
        registry.GetService("/ping/void").ShouldNotBeNull();
    }

    [Fact]
    public void AddMediatRServiceLike_アセンブリを探索してAsEventがついたINotificationが自動的に登録される()
    {
        var services = new ServiceCollection();
        services.AddMediatRServiceLike(typeof(ServicePing).Assembly);
        var provider = services.BuildServiceProvider();

        var registry = provider.GetService<ListenerRegistry>();
        registry.GetListeners("pang").ShouldNotBeNull();
        registry.GetListeners("pang2").ShouldNotBeNull();
    }

    [Fact]
    public void AddMediatRServiceLike_RequerstRegistryが先に登録されている場合はそのインスタンスが利用される()
    {
        var services = new ServiceCollection();
        var registry = new ServiceRegistry();
        services.AddSingleton(registry);

        services.AddMediatRServiceLike(typeof(ServicePing).Assembly);
        var provider = services.BuildServiceProvider();
        var registryFromService = provider.GetService<ServiceRegistry>();

        registryFromService.ShouldBeSameAs(registry);
        registry.GetService("/ping").ShouldNotBeNull();
        registry.GetService("/ping/void").ShouldNotBeNull();
    }

    [Fact]
    public void AddMediatRServiceLike_EventRegistryが先に登録されている場合はそのインスタンスが利用される()
    {
        var services = new ServiceCollection();
        var registry = new ListenerRegistry();
        services.AddSingleton(registry);

        services.AddMediatRServiceLike(typeof(ServicePing).Assembly);
        var provider = services.BuildServiceProvider();
        var registryFromService = provider.GetService<ListenerRegistry>();

        registryFromService.ShouldBeSameAs(registry);
        registry.GetListeners("pang").ShouldNotBeNull();
        registry.GetListeners("pang2").ShouldNotBeNull();
    }

    [Fact]
    public void AddMediatRServiceLike_IMediatorClientはDefaultMediatorClientでIMediatorが無い方のコンストラクタが利用される()
    {
        var services = new ServiceCollection();
        var serviceRegistry = new ServiceRegistry();
        services.AddSingleton(serviceRegistry);
        var listenerRegistry = new ListenerRegistry();
        services.AddSingleton(listenerRegistry);

        services.AddMediatRServiceLike(typeof(ServicePing).Assembly);

        var client = services.BuildServiceProvider().GetService<IMediatorClient>();
        client.ShouldNotBeNull();
        client.ShouldBeOfType<DefaultMediatorClient>();
    }
}
