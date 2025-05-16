using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Shouldly;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace Nut.MediatR.ServiceLike.Test;

public class DefaultMediatorClientTest
{

    private IServiceProvider CreateMockServiceProvider<T>(Func<Type, T> getService)
    {
        var provider = Substitute.For<IServiceProvider>();
        provider.GetService(Arg.Any<Type>())
            .Returns(getService);
        return provider;
    }

    [Fact]
    public void ctor_requestRegistryがnullの場合は例外が発生する()
    {
        var serviceFactory = CreateMockServiceProvider<object>(_ => null);
        Should.Throw<ArgumentNullException>(() => new DefaultMediatorClient(null, new ListenerRegistry(),
            serviceFactory, new InternalScopedServiceFactoryFactory(serviceFactory), new TestLogger()));
    }

    [Fact]
    public void ctor_notificationRegistryがnullの場合は例外が発生する()
    {
        var serviceFactory = CreateMockServiceProvider<object>(_ => null);
        Should.Throw<ArgumentNullException>(() => new DefaultMediatorClient(new ServiceRegistry(), null!,
            serviceFactory, new InternalScopedServiceFactoryFactory(serviceFactory), new TestLogger()));
    }

    [Fact]
    public void ctor_serviceFactoryがnullの場合は例外が発生する()
    {
        var serviceFactory = CreateMockServiceProvider<object>(_ => null);
        Should.Throw<ArgumentNullException>(() => new DefaultMediatorClient(new ServiceRegistry(), new ListenerRegistry(),
            null!, new InternalScopedServiceFactoryFactory(serviceFactory), new TestLogger()));
    }

    [Fact]
    public void ctor_ScopedServiceFactoryFactoryがnullの場合は例外が発生する()
    {
        var serviceFactory = CreateMockServiceProvider<object>(_ => null);
        Should.Throw<ArgumentNullException>(() => new DefaultMediatorClient(new ServiceRegistry(), new ListenerRegistry(),
            serviceFactory, null!, new TestLogger()));
    }

    [Fact]
    public async Task T_SendAsync_requestがnullの場合は例外が発生する()
    {
        var serviceFactory = CreateMockServiceProvider<object>(_ => null);
        var client = new DefaultMediatorClient(new ServiceRegistry(), new ListenerRegistry(),
            serviceFactory, new InternalScopedServiceFactoryFactory(serviceFactory), new TestLogger());

        await Should.ThrowAsync<ArgumentNullException>(() => client.SendAsync<Pong>("/path", null));
    }

    [Fact]
    public async Task T_SendAsync_pathに一致するリクエストが見つからない場合は例外が発生する()
    {
        var serviceFactory = CreateMockServiceProvider<object>(_ => null);
        var client = new DefaultMediatorClient(new ServiceRegistry(), new ListenerRegistry(),
            serviceFactory, new InternalScopedServiceFactoryFactory(serviceFactory), new TestLogger());

        var ex = await Should.ThrowAsync<ReceiverNotFoundException>(() => client.SendAsync<Pong>("/path", new ServicePing()));
        ex.RequestPath.ShouldBe("/path");
    }

    [Fact]
    public async Task T_SendAsync_Mediatorが実行されて結果が返される()
    {
        var services = new ServiceCollection();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ServicePing).Assembly));
        var check = new ExecuteCheck();
        services.AddTransient(_ => check);
        var provider = services.BuildServiceProvider();

        var registry = new ServiceRegistry();
        registry.Add(typeof(ServicePing));

        var client = new DefaultMediatorClient(registry, new ListenerRegistry(),
            provider, new TestScopedServiceFactoryFactory(provider), new TestLogger());

        var pong = await client.SendAsync<Pong>("/ping", new ServicePing() { Value = "Ping" });
        pong!.Value.ShouldBe("Ping Pong");
        check.Executed.ShouldBeTrue();
    }

    [Fact]
    public async Task SendAsync_Mediatorが実行されるが結果は捨てられる()
    {
        var services = new ServiceCollection();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ServicePing).Assembly));
        var check = new ExecuteCheck();
        services.AddTransient(_ => check);
        var provider = services.BuildServiceProvider();

        var registry = new ServiceRegistry();
        registry.Add(typeof(ServicePing));

        var client = new DefaultMediatorClient(registry, new ListenerRegistry(),
            provider, new TestScopedServiceFactoryFactory(provider), new TestLogger());

        await client.SendAsync("/ping", new ServicePing() { Value = "Ping" });
        check.Executed.ShouldBeTrue();
    }

    [Fact]
    public async Task T_SendAsync_引数と戻り値は変換可能_Jsonシリアライズデシリアライズに依存()
    {
        var services = new ServiceCollection();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ServicePing).Assembly));
        var check = new ExecuteCheck();
        services.AddTransient(_ => check);
        var provider = services.BuildServiceProvider();

        var registry = new ServiceRegistry();
        registry.Add(typeof(ServicePing));

        var client = new DefaultMediatorClient(registry, new ListenerRegistry(),
            provider, new TestScopedServiceFactoryFactory(provider), new TestLogger());

        var pong = await client.SendAsync<LocalPong>("/ping", new { Value = "Ping" });
        pong!.Value.ShouldBe("Ping Pong");
        check.Executed.ShouldBeTrue();
    }

    [Fact]
    public async Task T_SendAsync_引数を変換できない場合は例外が発生する()
    {
        var services = new ServiceCollection();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ServicePing).Assembly));
        var check = new ExecuteCheck();
        services.AddTransient(_ => check);
        var provider = services.BuildServiceProvider();

        var registry = new ServiceRegistry();
        registry.Add(typeof(ServicePing));

        var client = new DefaultMediatorClient(registry, new ListenerRegistry(),
            provider, new TestScopedServiceFactoryFactory(provider), new TestLogger());

        var ex = await Should.ThrowAsync<TypeTranslationException>(() => client.SendAsync<Pong>("/ping", "ping"));
        ex.FromType.ShouldBe(typeof(string));
        ex.ToType.ShouldBe(typeof(ServicePing));
    }

    [Fact]
    public async Task T_SendAsync_戻り値を変換できない場合は例外が発生する()
    {
        var services = new ServiceCollection();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ServicePing).Assembly));
        var check = new ExecuteCheck();
        services.AddTransient(_ => check);
        var provider = services.BuildServiceProvider();

        var registry = new ServiceRegistry();
        registry.Add(typeof(ServicePing));

        var client = new DefaultMediatorClient(registry, new ListenerRegistry(),
            provider, new TestScopedServiceFactoryFactory(provider), new TestLogger());

        var ex = await Should.ThrowAsync<TypeTranslationException>(() => client.SendAsync<string>("/ping", new { Value = "Ping" }));
        ex.FromType.ShouldBe(typeof(Pong));
        ex.ToType.ShouldBe(typeof(string));
    }

    [Fact]
    public async Task T_SendAsync_戻り値がnullの場合はnullが返される()
    {
        var services = new ServiceCollection();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ServicePing).Assembly));
        var check = new ExecuteCheck();
        services.AddTransient(_ => check);
        var provider = services.BuildServiceProvider();

        var registry = new ServiceRegistry();
        registry.Add(typeof(ServiceNullPing));

        var client = new DefaultMediatorClient(registry, new ListenerRegistry(),
            provider, new TestScopedServiceFactoryFactory(provider), new TestLogger());

        var pong = await client.SendAsync<Pong>("/ping/null", new { Value = "Ping" });
        pong.ShouldBeNull();
        check.Executed.ShouldBeTrue();
    }

    [Fact]
    public async Task SendAsync_戻り値がnullの場合も結果が捨てられる()
    {
        var services = new ServiceCollection();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ServicePing).Assembly));
        var check = new ExecuteCheck();
        services.AddTransient(_ => check);
        var provider = services.BuildServiceProvider();

        var registry = new ServiceRegistry();
        registry.Add(typeof(ServiceNullPing));

        var client = new DefaultMediatorClient(registry, new ListenerRegistry(),
            provider, new TestScopedServiceFactoryFactory(provider), new TestLogger());

        await client.SendAsync("/ping/null", new { Value = "Ping" });
        check.Executed.ShouldBeTrue();
    }

    [Fact]
    public async Task T_SendAsync_戻り値がUnitの場合はnullで返される()
    {
        var services = new ServiceCollection();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ServicePing).Assembly));
        var check = new ExecuteCheck();
        services.AddTransient(_ => check);
        var provider = services.BuildServiceProvider();

        var registry = new ServiceRegistry();
        registry.Add(typeof(VoidServicePing));

        var client = new DefaultMediatorClient(registry, new ListenerRegistry(),
            provider, new TestScopedServiceFactoryFactory(provider), new TestLogger());

        var pong = await client.SendAsync<Pong>("/ping/void", new { Value = "Ping" });
        pong.ShouldBeNull();
        check.Executed.ShouldBeTrue();
    }

    [Fact]
    public async Task SendAsync_戻り値がUnitの場合も結果は捨てられる()
    {
        var services = new ServiceCollection();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ServicePing).Assembly));
        var check = new ExecuteCheck();
        services.AddTransient(_ => check);
        var provider = services.BuildServiceProvider();

        var registry = new ServiceRegistry();
        registry.Add(typeof(VoidServicePing));

        var client = new DefaultMediatorClient(registry, new ListenerRegistry(),
            provider, new TestScopedServiceFactoryFactory(provider), new TestLogger());

        await client.SendAsync("/ping/void", new { Value = "Ping" });
        check.Executed.ShouldBeTrue();
    }

    [Fact]
    public async Task T_SendAsync_Filterが設定されている場合は順番に実行される()
    {
        var services = new ServiceCollection();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ServicePing).Assembly));

        var check = new FilterExecutionCheck();
        services.AddSingleton(check);

        var handlerCheck = new ExecuteCheck();
        services.AddTransient(_ => handlerCheck);

        var provider = services.BuildServiceProvider();

        var registry = new ServiceRegistry();
        registry.Add(typeof(ServicePing), typeof(Filter1), typeof(Filter2));

        var client = new DefaultMediatorClient(registry, new ListenerRegistry(),
            provider, new TestScopedServiceFactoryFactory(provider), new TestLogger());

        var pong = await client.SendAsync<Pong>("/ping", new ServicePing() { Value = "Ping" });

        pong!.Value.ShouldBe("Ping Pong");
        check.Checks.Count.ShouldBe(2);
        check.Checks[0].ShouldBe("1");
        check.Checks[1].ShouldBe("2");
    }

    [Fact]
    public async Task PublishAsyncActionOption_actionがnullの場合は例外が発生する()
    {
        var serviceFactory = CreateMockServiceProvider<object>(_ => null);
        var client = new DefaultMediatorClient(new ServiceRegistry(), new ListenerRegistry(),
            serviceFactory, new InternalScopedServiceFactoryFactory(serviceFactory!), new TestLogger());

        await Should.ThrowAsync<ArgumentNullException>(() => client.PublishAsync("ev", new Pang(), (Action<PublishOptions>)null!));
    }

    [Fact]
    public async Task PublishAsync_requestがnullの場合は例外が発生する()
    {
        var serviceFactory = CreateMockServiceProvider<object>(_ => null);
        var client = new DefaultMediatorClient(new ServiceRegistry(), new ListenerRegistry(),
            serviceFactory, new InternalScopedServiceFactoryFactory(serviceFactory!), new TestLogger());

        await Should.ThrowAsync<ArgumentNullException>(() => client.PublishAsync("ev", null!));
    }

    [Fact]
    public void PublishAsync_keyに一致するイベントが見つからない場合はなにも実行されず終了する()
    {
        var serviceFactory = CreateMockServiceProvider<object>(_ => null);
        var client = new DefaultMediatorClient(new ServiceRegistry(), new ListenerRegistry(),
            serviceFactory, new InternalScopedServiceFactoryFactory(serviceFactory!), new TestLogger());

        client.PublishAsync("key", new Pang());

        //TODO: assertion
    }

    [Fact]
    public async Task PublishAsyncActionOption_Mediatorが実行される()
    {
        var services = new ServiceCollection();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ServicePing).Assembly));

        services.AddSingleton<TaskHolder>();
        var provider = services.BuildServiceProvider();

        var registry = new ListenerRegistry();
        registry.Add(typeof(MediatorClientTestPang));

        var client = new DefaultMediatorClient(new ServiceRegistry(), registry,
            provider, new TestScopedServiceFactoryFactory(provider), new TestLogger());

        var holder = provider.GetService<TaskHolder>();

        var pang = new MediatorClientTestPang();
        await client.PublishAsync(nameof(MediatorClientTestPang), pang, optionsAction: op => { });

        await Task.Delay(1000); //それぞれで10だけまたしているため、1000あれば終わっているはず。

        await Task.WhenAll(holder.Tasks);
        holder.Messages.Count.ShouldBe(3);
        holder.Messages.ShouldContain("1");
        holder.Messages.ShouldContain("2");
        holder.Messages.ShouldContain("3");
        holder.Pangs.Count.ShouldBe(3);

        var paramBang = holder.Pangs[0];
        foreach (var bangItem in holder.Pangs)
        {
            paramBang.ShouldBe(bangItem);
        }
    }

    [Fact]
    public async Task PublishAsync_Mediatorが実行される()
    {
        var services = new ServiceCollection();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ServicePing).Assembly));

        services.AddSingleton<TaskHolder>();
        var provider = services.BuildServiceProvider();

        var registry = new ListenerRegistry();
        registry.Add(typeof(MediatorClientTestPang));

        var client = new DefaultMediatorClient(new ServiceRegistry(), registry,
            provider, new TestScopedServiceFactoryFactory(provider), new TestLogger());

        var holder = provider.GetService<TaskHolder>();

        var pang = new MediatorClientTestPang();
        await client.PublishAsync(nameof(MediatorClientTestPang), pang);

        await Task.Delay(1000); //それぞれで10だけまたしているため、1000あれば終わっているはず。

        await Task.WhenAll(holder.Tasks);
        holder.Messages.Count.ShouldBe(3);
        holder.Messages.ShouldContain("1");
        holder.Messages.ShouldContain("2");
        holder.Messages.ShouldContain("3");
        holder.Pangs.Count.ShouldBe(3);

        var paramBang = holder.Pangs[0];
        foreach (var bangItem in holder.Pangs)
        {
            paramBang.ShouldBe(bangItem);
        }
    }

    [Fact]
    public async Task PublishAsync_Notification内で例外が発生しても続行される()
    {
        var services = new ServiceCollection();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ExceptionPang).Assembly));
        var provider = services.BuildServiceProvider();

        var registry = new ListenerRegistry();
        registry.Add(typeof(ExceptionPang));

        var logger = new TestLogger();

        var client = new DefaultMediatorClient(new ServiceRegistry(), registry,
            provider, new TestScopedServiceFactoryFactory(provider), logger);
        await client.PublishAsync(nameof(ExceptionPang), new { });

        // Fire and forgetのため一旦スリープ
        await Task.Delay(2000);

        logger.Errors.Count.ShouldBe(1);
    }

    [Fact]
    public async Task PublishAsync_RequestもNotificationも実行される()
    {
        var services = new ServiceCollection();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(MixedRequest).Assembly));
        services.AddSingleton<MixedTaskHolder>();
        services.AddScoped<ScopeIdProvider>();
        var provider = services.BuildServiceProvider();

        var registry = new ListenerRegistry();
        registry.Add(typeof(MixedRequest));
        registry.Add(typeof(MixedNotification));

        var holder = provider.GetService<MixedTaskHolder>();

        var client = new DefaultMediatorClient(new ServiceRegistry(), registry,
            provider, new TestScopedServiceFactoryFactory(provider), new TestLogger());

        await client.PublishAsync("mixed", new { });

        // Fire and forgetのため一旦スリープ
        await Task.Delay(1000);

        holder.Messages.Count.ShouldBe(3);
        holder.Messages.ShouldContain("request");
        holder.Messages.ShouldContain("notification");
        holder.Messages.ShouldContain("notification2");
    }

    [Fact()]
    public async Task PublishAsync_Listener実行前に例外が発生した場合はリスナーが実行されずにログが出力される()
    {
        var services = new ServiceCollection();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(MixedRequest).Assembly));
        services.AddSingleton<MixedTaskHolder>();
        services.AddScoped<ScopeIdProvider>();
        var provider = services.BuildServiceProvider();

        //var serviceFactory = provider.GetService<ServiceFactory>();
        var registry = new ListenerRegistry();
        registry.Add(typeof(MixedRequest));
        registry.Add(typeof(MixedNotification));

        var holder = provider.GetService<MixedTaskHolder>();

        var testLogger = new TestLogger();
        var client = new DefaultMediatorClient(new ServiceRegistry(), registry,
            provider!, new ExceptionTestScopedServiceFactoryFactory(), testLogger);

        await client.PublishAsync("mixed", new { });

        // Fire and forgetのため一旦スリープ
        await Task.Delay(1000);

        holder.Messages.Count.ShouldBe(0);
        testLogger.Errors.Count.ShouldBe(2);
    }

    [Fact()]
    public async Task PublishAsync_別のScopeで実行されるが同じINotificationは同じScope()
    {
        var services = new ServiceCollection();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(MixedRequest).Assembly));
        services.AddSingleton<MixedTaskHolder>();
        services.AddScoped<ScopeIdProvider>();
        var provider = services.BuildServiceProvider();

        var registry = new ListenerRegistry();
        registry.Add(typeof(MixedRequest));
        registry.Add(typeof(MixedNotification));

        var holder = provider.GetService<MixedTaskHolder>();

        var client = new DefaultMediatorClient(new ServiceRegistry(), registry,
            provider, new TestScopedServiceFactoryFactory(provider), new TestLogger());

        await client.PublishAsync("mixed", new { });

        // Fire and forgetのため一旦スリープ
        await Task.Delay(1000);

        holder.ScopeIds.Count.ShouldBe(3);
        holder.ScopeIds[typeof(MixedRequestHandler)].ShouldNotBe(holder.ScopeIds[typeof(MixedNotificationHandler)]);
        holder.ScopeIds[typeof(MixedNotificationHandler)].ShouldBe(holder.ScopeIds[typeof(MixedNotificationHandler2)]);

        holder.ScopeIdProviders.All(p => p.Disposed).ShouldBeTrue();
    }

    private class ExceptionTestScopedServiceFactoryFactory : IScopedServiceFactoryFactory
    {
        public IScoepedServiceFactory Create()
        {
            throw new NotImplementedException();
        }
    }

    [AsEventListener("mixed")]
    public record MixedRequest : IRequest;

    [AsEventListener("mixed")]
    public record MixedNotification : INotification;

    public class MixedTaskHolder
    {
        public List<string> Messages { get; } = new();

        public Dictionary<Type, string> ScopeIds { get; } = new();

        public List<ScopeIdProvider> ScopeIdProviders { get; } = new();
    }

    public class ScopeIdProvider: IDisposable
    {
        public ScopeIdProvider()
        {
            _value = Guid.NewGuid().ToString();
        }

        private string _value;
        public string Value
        {
            get
            {
                return Disposed ? throw new InvalidOperationException("Already disposed") : _value;
            }
        }

        public bool Disposed { get; private set; }
        public void Dispose()
        {
            Disposed = true;
        }
    }

    public class MixedRequestHandler : IRequestHandler<MixedRequest>
    {
        private readonly MixedTaskHolder _holder;
        private readonly ScopeIdProvider _scopeIdProvider;

        public MixedRequestHandler(MixedTaskHolder holder, ScopeIdProvider scopeIdProvider)
        {
            _holder = holder;
            _scopeIdProvider = scopeIdProvider;
            holder.ScopeIdProviders.Add(scopeIdProvider);
        }
        public Task Handle(MixedRequest request, CancellationToken cancellationToken)
        {
            _holder.Messages.Add("request");
            _holder.ScopeIds.Add(typeof(MixedRequestHandler), _scopeIdProvider.Value);
            return Task.FromResult(request);
            //return Unit.Task;
        }
    }

    public class MixedNotificationHandler : INotificationHandler<MixedNotification>
    {
        private readonly MixedTaskHolder _holder;
        private readonly ScopeIdProvider _scopeIdProvider;

        public MixedNotificationHandler(MixedTaskHolder holder, ScopeIdProvider scopeIdProvider)
        {
            _holder = holder;
            _scopeIdProvider = scopeIdProvider;
        }
        public Task Handle(MixedNotification request, CancellationToken cancellationToken)
        {
            _holder.Messages.Add("notification");
            _holder.ScopeIds.Add(typeof(MixedNotificationHandler), _scopeIdProvider.Value);
            return Task.CompletedTask;
        }
    }

    public class MixedNotificationHandler2 : INotificationHandler<MixedNotification>
    {
        private readonly MixedTaskHolder _holder;
        private readonly ScopeIdProvider _scopeIdProvider;

        public MixedNotificationHandler2(MixedTaskHolder holder, ScopeIdProvider scopeIdProvider)
        {
            _holder = holder;
            _scopeIdProvider = scopeIdProvider;
        }
        public Task Handle(MixedNotification request, CancellationToken cancellationToken)
        {
            _holder.Messages.Add("notification2");
            _holder.ScopeIds.Add(typeof(MixedNotificationHandler2), _scopeIdProvider.Value);
            return Task.CompletedTask;
        }
    }

    [AsEventListener(nameof(ExceptionPang))]
    public class ExceptionPang : INotification
    {
        public ExceptionPang(string value)
        {

        }
    }
    public class ExceptionPangHandler : INotificationHandler<ExceptionPang>
    {
        public Task Handle(ExceptionPang notification, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }

    public class LocalPong
    {
        public string Value { get; set; }
    }

    [AsEventListener(nameof(MediatorClientTestPang))]
    public class MediatorClientTestPang : INotification
    {
    }

    public class TaskHolder
    {
        public List<Task> Tasks { get; } = new();

        public List<string> Messages { get; } = new();

        public List<MediatorClientTestPang> Pangs { get; } = new();
    }

    public class MediatorClientTestHandlerBase
    {
        private readonly TaskHolder _holder;

        private readonly Task _task;

        protected MediatorClientTestHandlerBase(TaskHolder holder, string message)
        {
            _task = new Task(async () =>
            {
                await Task.Delay(10);
                holder.Messages.Add(message);
            });
            _holder = holder;
            holder.Tasks.Add(_task);
        }
        public Task Handle(MediatorClientTestPang notification, CancellationToken cancellationToken)
        {
            _holder.Pangs.Add(notification);
            _task.Start();
            // return Task.CompletedTask;
            return _task;
        }
    }

    public class MediatorClientTestHandler1 : MediatorClientTestHandlerBase, INotificationHandler<MediatorClientTestPang>
    {
        public MediatorClientTestHandler1(TaskHolder holder) : base(holder, "1")
        {
        }
    }
    public class MediatorClientTestHandler2 : MediatorClientTestHandlerBase, INotificationHandler<MediatorClientTestPang>
    {
        public MediatorClientTestHandler2(TaskHolder holder) : base(holder, "2")
        {
        }
    }

    public class MediatorClientTestHandler3 : MediatorClientTestHandlerBase, INotificationHandler<MediatorClientTestPang>
    {
        public MediatorClientTestHandler3(TaskHolder holder) : base(holder, "3")
        {
        }
    }

    private class TestLogger : IServiceLikeLogger
    {
        public void Info(string message, params object[] args)
        {
            Infos.Add(message);
        }

        public void Error(Exception ex, string message, params object[] args)
        {
            Errors.Add(message);
        }

        public void Trace(string message, params object[] args)
        {
            Traces.Add(message);
        }

        public bool IsTraceEnabled() => true;

        public bool IsInfoEnabled() => true;

        public bool IsErrorEnabled() => true;

        public List<string> Errors { get; } = new();

        public List<string> Infos { get; } = new();

        public List<string> Traces { get; } = new();
    }
}
