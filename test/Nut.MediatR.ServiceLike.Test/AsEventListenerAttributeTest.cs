using System;
using Shouldly;
using Xunit;

namespace Nut.MediatR.ServiceLike.Test;

public class AsEventListenerAttributeTest
{
    [Fact]
    public void ctor_パラメーターがnullの場合は例外が発生する()
    {
        Should.Throw<ArgumentException>(() => new AsEventListenerAttribute(null));
    }

    [Fact]
    public void ctor_パラメーターが空文字の場合は例外が発生する()
    {
        Should.Throw<ArgumentException>(() => new AsEventListenerAttribute(""));
    }

    [Fact]
    public void ctor_パラメーターがホワイトスペースの場合は例外が発生する()
    {
        Should.Throw<ArgumentException>(() => new AsEventListenerAttribute(" "));
    }

    [Fact]
    public void ctor_パラメーターで指定したPathが取得できる()
    {
        var expect = "/this/is/service/path";
        var attr = new AsEventListenerAttribute(expect);
        attr.Key.ShouldBe(expect);
    }
}
