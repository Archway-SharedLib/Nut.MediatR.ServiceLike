using System;
using System.Linq;
using Shouldly;
using Xunit;

namespace Nut.MediatR.ServiceLike.Test;

public class MediatorListenerDescriptionTest
{
    [Fact]
    public void Create_引数がnullの場合は例外が発行される()
    {
        Should.Throw<ArgumentNullException>(() => MediatorListenerDescription.Create(null));
    }

    [Fact]
    public void Create_引数の型のGenericがオープンしてたら例外が発行される()
    {
        Should.Throw<ArgumentException>(() => MediatorListenerDescription.Create(typeof(OpenGenericPang<>)));
    }

    [Fact]
    public void Create_引数の型が実装型じゃない場合例外が発行される()
    {
        Should.Throw<ArgumentException>(() => MediatorListenerDescription.Create(typeof(AbstractPang)));
    }

    [Fact]
    public void Create_引数の型がINotificationを継承していない場合例外が発行される()
    {
        Should.Throw<ArgumentException>(() => MediatorListenerDescription.Create(typeof(PlainPang)));
    }

    [Fact]
    public void Create_引数の型にAsEventListenerが付加されていない場合例外が発行される()
    {
        Should.Throw<ArgumentException>(() => MediatorListenerDescription.Create(typeof(OnlyPang)));
    }

    [Fact]
    public void Create_引数の型にINotificationを実装したクローズドでAsEventListenerが付加されている場合はMediatorListenerが返される()
    {
        var listeners = MediatorListenerDescription.Create(typeof(Pang));
        listeners.Count().ShouldBe(1);
        var listener = listeners.First();
        listener.Key.ShouldBe("pang");
        listener.ListenerType.ShouldBe(typeof(Pang));
    }

    [Fact]
    public void Create_引数の型にINotificationを実装している場合はMediateTypeがNotificationのMediatorListenerが返される()
    {
        var listeners = MediatorListenerDescription.Create(typeof(Pang));
        listeners.Count().ShouldBe(1);
        listeners.Select(l => l.MediateType)
            .All(m => m == MediateType.Notification).ShouldBeTrue();
    }

    [Fact]
    public void Create_引数の型にIRequestを実装したクローズドでAsEventListenerが付加されている場合はMediatorListenerが返される()
    {
        var listeners = MediatorListenerDescription.Create(typeof(RequestPang));
        listeners.Count().ShouldBe(1);
        var listener = listeners.First();
        listener.Key.ShouldBe("pang.request");
        listener.ListenerType.ShouldBe(typeof(RequestPang));
    }

    [Fact]
    public void Create_引数の型にIRequestTを実装したクローズドでAsEventListenerが付加されている場合はMediatorListenerが返される()
    {
        var listeners = MediatorListenerDescription.Create(typeof(RequestTPang));
        listeners.Count().ShouldBe(1);
        var listener = listeners.First();
        listener.Key.ShouldBe("pang.requestT");
        listener.ListenerType.ShouldBe(typeof(RequestTPang));
    }

    [Fact]
    public void Create_引数の型にIRequestを実装している場合はMediateTypeがRequestのMediatorListenerが返される()
    {
        var listeners = MediatorListenerDescription.Create(typeof(RequestPang));
        listeners.Count().ShouldBe(1);
        listeners.Select(l => l.MediateType)
            .All(m => m == MediateType.Request).ShouldBeTrue();
    }

    [Fact]
    public void Create_引数の型にIRequestTを実装している場合はMediateTypeがRequestのMediatorListenerが返される()
    {
        var listeners = MediatorListenerDescription.Create(typeof(RequestTPang));
        listeners.Count().ShouldBe(1);
        listeners.Select(l => l.MediateType)
            .All(m => m == MediateType.Request).ShouldBeTrue();
    }

    [Fact]
    public void Create_引数の型に複数のAsEventListenerが付加されている場合は複数のEventが返される()
    {
        var listeners = MediatorListenerDescription.Create(typeof(MultiPang));
        listeners.Count().ShouldBe(2);
        var listenerList = listeners.OrderBy(r => r.Key).ToList();
        listenerList[0].Key.ShouldBe("pang.1");
        listenerList[0].ListenerType.ShouldBe(typeof(MultiPang));
        listenerList[1].Key.ShouldBe("pang.2");
        listenerList[1].ListenerType.ShouldBe(typeof(MultiPang));
    }
}
