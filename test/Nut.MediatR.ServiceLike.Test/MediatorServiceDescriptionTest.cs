using System;
using System.Linq;
using Shouldly;
using MediatR;
using Xunit;

namespace Nut.MediatR.ServiceLike.Test;

public class MediatorServiceDescriptionTest
{
    [Fact]
    public void Create_引数がnullの場合は例外が発行される()
    {
        Should.Throw<ArgumentNullException>(() => MediatorServiceDescription.Create(null));
    }

    [Fact]
    public void Create_引数の型のGenericがオープンしてたら例外が発行される()
    {
        Should.Throw<ArgumentException>(() => MediatorServiceDescription.Create(typeof(TestOpenGenericRequest<>)));
    }

    [Fact]
    public void Create_引数の型が実装型じゃない場合例外が発行される()
    {
        Should.Throw<ArgumentException>(() => MediatorServiceDescription.Create(typeof(TestAbstractRequest)));
    }

    [Fact]
    public void Create_引数の型がIRequestを継承していない場合例外が発行される()
    {
        Should.Throw<ArgumentException>(() => MediatorServiceDescription.Create(typeof(TestPlainRequest)));
    }

    [Fact]
    public void Create_引数の型にAsServiceが付加されていない場合例外が発行される()
    {
        Should.Throw<ArgumentException>(() => MediatorServiceDescription.Create(typeof(TestNotServicRequest)));
    }

    [Fact]
    public void Create_引数の型にIRequestを実装したクローズドでAsServiceが付加されている場合はMediatorRequestが返される()
    {
        var requests = MediatorServiceDescription.Create(typeof(TestServiceRequest));
        requests.Count().ShouldBe(1);
        var request = requests.First();
        request.Path.ShouldBe("/path");
        request.ServiceType.ShouldBe(typeof(TestServiceRequest));
    }

    [Fact]
    public void Create_引数の型にIRequestTを実装したクローズドでAsServiceが付加されている場合はMediatorRequestが返される()
    {
        var requests = MediatorServiceDescription.Create(typeof(TestServiceRequestT));
        requests.Count().ShouldBe(1);
        var request = requests.First();
        request.Path.ShouldBe("/path");
        request.ServiceType.ShouldBe(typeof(TestServiceRequestT));
    }

    [Fact]
    public void Create_引数の型に複数のAsServiceが付加されている場合は複数のRequestが返される()
    {
        var requests = MediatorServiceDescription.Create(typeof(TestMultipleRouteRequest));
        requests.Count().ShouldBe(2);
        var requestList = requests.OrderBy(r => r.Path).ToList();
        requestList[0].Path.ShouldBe("/path1");
        requestList[0].ServiceType.ShouldBe(typeof(TestMultipleRouteRequest));
        requestList[1].Path.ShouldBe("/path2");
        requestList[1].ServiceType.ShouldBe(typeof(TestMultipleRouteRequest));
    }

    [AsService("/path")]
    public class TestOpenGenericRequest<T> : IRequest<T> { }

    [AsService("/path")]
    public abstract class TestAbstractRequest : IRequest<Unit> { }

    [AsService("/path")]
    public class TestPlainRequest { }

    public abstract class TestNotServicRequest : IRequest<Unit> { }

    [AsService("/path")]
    public class TestServiceRequest : IRequest { }

    [AsService("/path")]
    public class TestServiceRequestT : IRequest<Unit> { }

    [AsService("/path1")]
    [AsService("/path2")]
    public class TestMultipleRouteRequest : IRequest<Unit> { }
}
