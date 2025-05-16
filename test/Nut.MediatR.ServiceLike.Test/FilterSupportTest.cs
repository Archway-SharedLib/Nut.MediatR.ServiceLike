using System;
using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace Nut.MediatR.ServiceLike.Test;

public class FilterSupportTest
{
    [Fact]
    public void IsValidFilterTypeAllCore_filterTypesがnullの場合は例外が発生する()
    {
        Should.Throw<ArgumentNullException>(() => FilterSupport.IsValidFilterTypeAllCore(null));
    }

    [Fact]
    public void IsValidFilterTypeAllCore_filterTypesにnullが含まれる場合はfalseが返る()
    {
        var result = FilterSupport.IsValidFilterTypeAllCore(
            new Type[] { typeof(Filter1), null, typeof(Filter2) });
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsValidFilterTypeAllCore_filterTypesにIFilterを継承していない型が含まれる場合はfalseが返る()
    {
        var result = FilterSupport.IsValidFilterTypeAllCore(
            new Type[] { typeof(Filter1), typeof(string), typeof(Filter2) });
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsValidFilterTypeAllCore_filterTypesにデフォルトコンストラクターがない型が含まれる場合はfalseが返る()
    {
        var result = FilterSupport.IsValidFilterTypeAllCore(
            new Type[] { typeof(Filter1), typeof(Filter3), typeof(Filter2) });
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsValidFilterTypeAllCore_filterTypesが全てIFilterを継承してデフォルトコンストラクターがある型の場合はtrueが返る()
    {
        var result = FilterSupport.IsValidFilterTypeAllCore(
            new Type[] { typeof(Filter1), typeof(Filter2) });
        result.ShouldBeTrue();
    }

    [Fact]
    public void ThrowIfInvalidFilterTypeAllWith_filterTypesがnullの場合は例外が発生する()
    {
        Should.Throw<ArgumentNullException>(() => FilterSupport.ThrowIfInvalidFilterTypeAllWith(null));
    }

    [Fact]
    public void ThrowIfInvalidFilterTypeAllWith_filterTypesにnullが含まれる場合はfalseが返る()
    {
        Should.Throw<ArgumentException>(() => FilterSupport.ThrowIfInvalidFilterTypeAllWith(
            new Type[] { typeof(Filter1), null, typeof(Filter2) }));
    }

    [Fact]
    public void ThrowIfInvalidFilterTypeAllWith_filterTypesにIFilterを継承していない型が含まれる場合はfalseが返る()
    {
        Should.Throw<ArgumentException>(() => FilterSupport.ThrowIfInvalidFilterTypeAllWith(
            new Type[] { typeof(Filter1), typeof(string), typeof(Filter2) }));
    }

    [Fact]
    public void ThrowIfInvalidFilterTypeAllWith_filterTypesにデフォルトコンストラクターがない型が含まれる場合はfalseが返る()
    {
        Should.Throw<ArgumentException>(() => FilterSupport.ThrowIfInvalidFilterTypeAllWith(
                new Type[] { typeof(Filter1), typeof(Filter3), typeof(Filter2) }));
    }

    [Fact]
    public void ThrowIfInvalidFilterTypeAllWith_filterTypesが全てIFilterを継承してデフォルトコンストラクターがある型の場合はtrueが返る()
    {
        FilterSupport.ThrowIfInvalidFilterTypeAllWith(
            new Type[] { typeof(Filter1), typeof(Filter2) });
    }

    public class Filter1 : IMediatorServiceFilter
    {
        public Task<object> HandleAsync(RequestContext context, object parameter, Func<object, Task<object>> next)
        {
            throw new NotImplementedException();
        }
    }

    public class Filter2 : IMediatorServiceFilter
    {
        public Task<object> HandleAsync(RequestContext context, object parameter, Func<object, Task<object>> next)
        {
            throw new NotImplementedException();
        }
    }

    public class Filter3 : IMediatorServiceFilter
    {
        public Filter3(string value)
        {

        }
        public Task<object> HandleAsync(RequestContext context, object parameter, Func<object, Task<object>> next)
        {
            throw new NotImplementedException();
        }
    }
}
