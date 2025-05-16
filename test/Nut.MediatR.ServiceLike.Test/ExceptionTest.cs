using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace Nut.MediatR.ServiceLike.Test;

public class ExceptionTest
{
    // for coverage
    [Fact]
    public void MediatRServiceLikeException_ctor()
    {
        var exception = new MediatRServiceLikeException();
        exception.ShouldNotBeNull();
    }

    [Fact]
    public void MediatRServiceLikeException_メッセージが設定される()
    {
        var message = "testmessage";
        var exception = new MediatRServiceLikeException(message);
        exception.Message.ShouldBe(message);
    }

    [Fact]
    public void RequestNotFoundException_RequestPathが設定される()
    {
        var requestPath = "/path";
        var exception = new ReceiverNotFoundException(requestPath);
        exception.RequestPath.ShouldBe(requestPath);
    }

    [Fact]
    public void RequestNotFoundException_メッセージはデフォルトの値が設定される()
    {
        var requestPath = "/path";
        var exception = new ReceiverNotFoundException(requestPath);
        exception.Message.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public void RequestNotFoundException_メッセージが設定される()
    {
        var requestPath = "/path";
        var message = "testmessage";
        var exception = new ReceiverNotFoundException(requestPath, message);
        exception.Message.ShouldBe(message);
    }

    [Fact]
    public void TypeTranslationException_FromToが設定される()
    {
        var from = typeof(string);
        var to = typeof(int);
        var exception = new TypeTranslationException(from, to);
        exception.FromType.ShouldBe(from);
        exception.ToType.ShouldBe(to);
        exception.Message.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public void TypeTranslationException_メッセージが設定される()
    {
        var from = typeof(string);
        var to = typeof(int);
        var message = "testmessage";
        var exception = new TypeTranslationException(from, to, message);
        exception.FromType.ShouldBe(from);
        exception.ToType.ShouldBe(to);
        exception.Message.ShouldBe(message);
    }
}
