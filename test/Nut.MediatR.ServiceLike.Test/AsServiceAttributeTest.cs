using System;
using Shouldly;
using Xunit;

namespace Nut.MediatR.ServiceLike.Test;

public class AsServiceAttributeTest
{
    [Fact]
    public void ctor_パラメーターがnullの場合は例外が発生する()
    {
        Should.Throw<ArgumentException>(() => new AsServiceAttribute(null));
    }

    [Fact]
    public void ctor_パラメーターが空文字の場合は例外が発生する()
    {
        Should.Throw<ArgumentException>(() => new AsServiceAttribute(""));
    }

    [Fact]
    public void ctor_パラメーターがホワイトスペースの場合は例外が発生する()
    {
        Should.Throw<ArgumentException>(() => new AsServiceAttribute(" "));
    }

    [Fact]
    public void ctor_パラメーターで指定したPathが取得できる()
    {
        var expect = "/this/is/service/path";
        var attr = new AsServiceAttribute(expect);
        attr.Path.ShouldBe(expect);
    }

    [Fact]
    public void ctor_filterTypesがnullの場合は例外が発生する()
    {
        Should.Throw<ArgumentException>(() => new AsServiceAttribute("/this/is/service/path", null));
    }
}
