using System;
using System.Collections.Generic;
using System.Linq;
using Shouldly;
using Xunit;

namespace Nut.MediatR.ServiceLike.Test;

public class ServiceLikeContextTest
{
    [Fact]
    public void ctor_keyにnullまたは空文字または空白が指定された場合は例外が発生する()
    {
        Should.Throw<ArgumentException>(() => new ServiceLikeContext(null));
        Should.Throw<ArgumentException>(() => new ServiceLikeContext(""));
        Should.Throw<ArgumentException>(() => new ServiceLikeContext("  "));
    }

    [Fact]
    public void ctor_Headerにnullを設定してもからのHeaderがプロパティから取得できる()
    {
        var instance = new ServiceLikeContext("key", null);
        instance.Header.Count().ShouldBe(0);
    }

    [Fact]
    public void Id_IdにはGuidベースの値が設定される()
    {
        var instance = new ServiceLikeContext("key");
        Guid.TryParseExact(instance.Id, "N", out var _).ShouldBeTrue();
    }

    [Fact]
    public void Header_コンストラクタで指定した内容が設定されている()
    {
        var header = new Dictionary<string, object>()
            {
                { "key1", "123" },
                { "key2", 456 },
            };
        var instance = new ServiceLikeContext("key", header);

        instance.Header.Count().ShouldBe(header.Count());
        instance.Header["key1"].ShouldBe(header["key1"]);
        instance.Header["key2"].ShouldBe(header["key2"]);
    }

    [Fact]
    public void Key_ctorで指定した値が設定される()
    {
        var expect = "alksjfolawjef";
        var instance = new ServiceLikeContext(expect);
        instance.Key.ShouldBe(expect);
    }

    [Fact]
    public void Timestamp_現在日時が設定される()
    {
        var now = DateTimeOffset.Now.Ticks;
        var instance = new ServiceLikeContext("foo");
        instance.Timestamp.ShouldBeInRange(now - 30000, now + 30000);
    }
}
