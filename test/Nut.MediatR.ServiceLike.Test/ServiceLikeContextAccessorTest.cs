using System;
using System.Threading;
using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace Nut.MediatR.ServiceLike.Test;

public class ServiceLikeContextAccessorTest
{
    [Fact]
    public void ctor_インスタンスを作成した時点ではコンテキストはnull()
    {
        var accessor = new ServiceLikeContextAccessor();
        accessor.Context.ShouldBeNull();
    }

    [Fact]
    public async Task Context_設定したコンテキストが取得できる()
    {
        var accessor = new ServiceLikeContextAccessor();
        var context = new ServiceLikeContext("foo");

        accessor.Context = context;

        await Task.Delay(100);

        context.ShouldBeSameAs(accessor.Context);
    }

    [Fact]
    public async Task Context_親のAsyncContextでnullに設定されたら子のコンテキストでもnullになる()
    {
        var accessor = new ServiceLikeContextAccessor();
        var context = new ServiceLikeContext("foo");
        accessor.Context = context;

        var checkAsyncFlowTcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
        var waitForNullTcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
        var afterNullCheckTcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);

        ThreadPool.QueueUserWorkItem(async _ =>
        {
            context.ShouldBeSameAs(accessor.Context);

            checkAsyncFlowTcs.SetResult(null);
            await waitForNullTcs.Task;

            try
            {
                accessor.Context.ShouldBeNull();
                afterNullCheckTcs.SetResult(null);
            }
            catch (Exception ex)
            {
                afterNullCheckTcs.SetException(ex);
            }
        });

        await checkAsyncFlowTcs.Task;
        accessor.Context = null;

        waitForNullTcs.SetResult(null);

        accessor.Context.ShouldBeNull();

        await afterNullCheckTcs.Task;
    }

    [Fact]
    public async Task Context_親のAsyncContextで別のインスタンスが設定されたら子のコンテキストはnullになる()
    {
        var accessor = new ServiceLikeContextAccessor();
        var context = new ServiceLikeContext("foo");
        accessor.Context = context;

        var checkAsyncFlowTcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
        var waitForNullTcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
        var afterNullCheckTcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);

        ThreadPool.QueueUserWorkItem(async _ =>
        {
            context.ShouldBeSameAs(accessor.Context);
            checkAsyncFlowTcs.SetResult(null);

            await waitForNullTcs.Task;

            try
            {
                accessor.Context.ShouldBeNull();
                afterNullCheckTcs.SetResult(null);
            }
            catch (Exception ex)
            {
                afterNullCheckTcs.SetException(ex);
            }
        });

        await checkAsyncFlowTcs.Task;

        var context2 = new ServiceLikeContext("bar");
        accessor.Context = context2;

        waitForNullTcs.SetResult(null);

        context2.ShouldBeSameAs(accessor.Context);

        await afterNullCheckTcs.Task;
    }

    [Fact]
    public async Task Context_親のAsyncContextにつながっていない場合は値は設定されない()
    {
        var accessor = new ServiceLikeContextAccessor();
        var context = new ServiceLikeContext("foo");
        accessor.Context = context;

        var checkAsyncFlowTcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);

        ThreadPool.UnsafeQueueUserWorkItem(_ =>
        {
            try
            {
                accessor.Context.ShouldBeNull();
                checkAsyncFlowTcs.SetResult(null);
            }
            catch (Exception ex)
            {
                checkAsyncFlowTcs.SetException(ex);
            }
        }, null);

        await checkAsyncFlowTcs.Task;
    }
}
