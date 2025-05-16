using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Shouldly;
using Xunit;

namespace Nut.MediatR.ServiceLike.Test;

public class TypeExtensionsTest
{
    [Fact]
    public void IsOpenGeneric_型パラメータが省略されている型はtrueになる()
        => TypeExtensions.IsOpenGeneric(typeof(List<>)).ShouldBeTrue();

    [Fact]
    public void IsOpenGeneric_型パラメータが省略されている型を持っているGeneric型もtrueになる()
        => TypeExtensions.IsOpenGeneric(typeof(List<>).MakeGenericType(typeof(List<>))).ShouldBeTrue();

    [Fact]
    public void IsOpenGeneric_型パラメータが埋まっている場合はfalseになる()
        => TypeExtensions.IsOpenGeneric(typeof(List<>).MakeGenericType(typeof(List<string>))).ShouldBeFalse();

    [Fact]
    public void IsOpenGeneric_型パラメータがない場合はfalseになる()
        => TypeExtensions.IsOpenGeneric(typeof(TypeExtensionsTest)).ShouldBeFalse();

    [Fact]
    public void IsConcrete_abstractクラスはfalseになる()
        => TypeExtensions.IsConcrete(typeof(IsConcreteAbstract)).ShouldBeFalse();

    [Fact]
    public void IsConcrete_interfaceはfalseになる()
        => TypeExtensions.IsConcrete(typeof(IsConcreteInterface)).ShouldBeFalse();

    [Fact]
    public void IsConcrete_実装クラスはtrueになる()
        => TypeExtensions.IsConcrete(typeof(TypeExtensionsTest)).ShouldBeTrue();

    [Fact]
    public void GetAttribute_属性が取得できる()
        => TypeExtensions.GetAttribute<GetAttrAttribute>(typeof(GetAttrDefualt)).ShouldBeOfType<GetAttrAttribute>();

    [Fact]
    public void GetAttribute_属性が設定されていないときはnullが返る()
        => TypeExtensions.GetAttribute<GetAttrAttribute>(typeof(GetAttrNull)).ShouldBeNull();

    [Fact]
    public void GetAttribute_複数の属性が設定されているときはいずれかの値が取得できる()
        => TypeExtensions.GetAttribute<GetAttrAttribute>(typeof(GetAttrMulti)).Value.ShouldMatch("^[12]$");

    [Fact]
    public void GetAttribute_継承元の場合も取得する()
        => TypeExtensions.GetAttribute<GetAttrAttribute>(typeof(GetAttrInherit)).ShouldBeOfType<GetAttrAttribute>();

    [Fact]
    public void GetAttribute_inheritにfalseを設定すると継承元の場合は取得しない()
        => TypeExtensions.GetAttribute<GetAttrAttribute>(typeof(GetAttrInherit), false).ShouldBeNull();

    [Fact]
    public void GetAttribute_inheritにtrueを設定すると継承元の場合は取得する()
        => TypeExtensions.GetAttribute<GetAttrAttribute>(typeof(GetAttrInherit), true).ShouldBeOfType<GetAttrAttribute>();

    [Fact]
    public void GetAttributes_すべての属性が取得できる()
    {
        var attrs = TypeExtensions.GetAttributes<GetAttrAttribute>(typeof(GetAttrMulti));
        attrs.Count().ShouldBe(2);
        var attrList = attrs.OrderBy(a => a.Value).ToList();
        attrList[0].Value.ShouldBe("1");
        attrList[1].Value.ShouldBe("2");
    }

    [Fact]
    public void GetAttributes_属性が設定されていない場合は空になる()
        => TypeExtensions.GetAttributes<GetAttrAttribute>(typeof(GetAttrNull)).ShouldBeEmpty();

    [Fact]
    public void GetAttributes_継承元の場合も取得する()
    {
        var attrs = TypeExtensions.GetAttributes<GetAttrAttribute>(typeof(GetAttrInheritMulti));
        attrs.Count().ShouldBe(2);
        var attrList = attrs.OrderBy(a => a.Value).ToList();
        attrList[0].Value.ShouldBe("1");
        attrList[1].Value.ShouldBe("2");
    }

    [Fact]
    public void GetAttributes_inheritにfalseを設定すると継承元の場合は取得しない()
        => TypeExtensions.GetAttributes<GetAttrAttribute>(typeof(GetAttrInheritMulti), false).ShouldBeEmpty();

    [Fact]
    public void GetAttributes_inheritにtrueを設定すると継承元の場合は取得する()
    {
        var attrs = TypeExtensions.GetAttributes<GetAttrAttribute>(typeof(GetAttrInheritMulti), true);
        attrs.Count().ShouldBe(2);
        var attrList = attrs.OrderBy(a => a.Value).ToList();
        attrList[0].Value.ShouldBe("1");
        attrList[1].Value.ShouldBe("2");
    }

    [Fact]
    public void IsImplemented_interfaceTypeパラメーターがインターフェイスではない場合はfalse()
        => TypeExtensions.IsImplemented(typeof(GetAttrInherit), typeof(GetAttrDefualt)).ShouldBeFalse();

    [Fact]
    public void IsImplemented_typeパラメーターがインターフェイスの場合はfalse()
        => TypeExtensions.IsImplemented(typeof(IsConcreteInterfaceInherit), typeof(IsConcreteInterface)).ShouldBeFalse();

    [Theory()]
    [InlineData(typeof(GenericInterfaceImpl<>), typeof(GenericInterface<>))]
    [InlineData(typeof(GenericInterfaceImpl<string>), typeof(GenericInterface<string>))]
    [InlineData(typeof(GenericInterfaceImpl<string>), typeof(GenericInterface<>))]
    [InlineData(typeof(GenericInterfaceTypedImpl), typeof(GenericInterface<>))]
    [InlineData(typeof(GenericInterfaceTypedImpl), typeof(GenericInterface<string>))]
    [InlineData(typeof(NonGenericInterfaceImpl), typeof(NonGenericInterface))]
    [InlineData(typeof(GenericInterfaceImplNested<>), typeof(GenericInterface<>))]
    [InlineData(typeof(GenericInterfaceImplNested<string>), typeof(GenericInterface<>))]
    [InlineData(typeof(GenericInterfaceTypedImplNested), typeof(GenericInterface<>))]
    [InlineData(typeof(GenericInterfaceTypedImplNested), typeof(GenericInterface<string>))]
    [InlineData(typeof(NestedGenericInterfaceImpl<>), typeof(GenericInterface<>))]
    [InlineData(typeof(NestedGenericInterfaceImpl<string>), typeof(GenericInterface<string>))]
    public void IsImplementd_インターフェイスを継承している場合はtrue(Type type, Type interfaceType)
        => TypeExtensions.IsImplemented(type, interfaceType).ShouldBeTrue();

    [Theory()]
    [InlineData(typeof(NonGenericInterfaceImpl), typeof(GenericInterface<>))]
    [InlineData(typeof(GenericInterfaceImpl<bool>), typeof(GenericInterface<string>))]
    [InlineData(typeof(GenericInterfaceTypedImpl), typeof(GenericInterface<bool>))]
    [InlineData(typeof(GenericInterfaceTypedImpl), typeof(NonGenericInterface))]
    public void IsImplementd_インターフェイスを継承してない場合はtrue(Type type, Type interfaceType)
        => TypeExtensions.IsImplemented(type, interfaceType).ShouldBeFalse();

    [Fact]
    public void Activate_インスタンスを作成できる()
        => TypeExtensions.Activate<ActivateInterface>(typeof(ActivateClass)).ShouldNotBeNull();

    [Fact]
    public void HasDefaultConstructor_デフォルトコンストラクタがある場合はtrueが返る()
        => TypeExtensions.HasDefaultConstructor(typeof(ActivateClass)).ShouldBeTrue();

    [Fact]
    public void HasDefaultConstructor_デフォルトコンストラクタがない場合はfalseが返る()
        => TypeExtensions.HasDefaultConstructor(typeof(ActivateClass2)).ShouldBeFalse();

    [Fact]
    public void GetDefaultConstructor_デフォルトコンストラクタがある場合はConstructorInfoが返る()
        => TypeExtensions.GetDefaultConstructor(typeof(ActivateClass)).ShouldBeAssignableTo<ConstructorInfo>();

    [Fact]
    public void GetDefaultConstructor_デフォルトコンストラクタがない場合はnullが返る()
        => TypeExtensions.GetDefaultConstructor(typeof(ActivateClass2)).ShouldBeNull();

    //-----------------------------

    public interface ActivateInterface { }

    public class ActivateClass : ActivateInterface { }

    public class ActivateClass2
    {
        public ActivateClass2(string value)
        {

        }
    }


    public abstract class IsConcreteAbstract { }

    public interface IsConcreteInterface { }

    public interface IsConcreteInterfaceInherit : IsConcreteInterface { }

    public interface GenericInterface<T> { }

    public interface NonGenericInterface { };

    public class GenericInterfaceImpl<T> : GenericInterface<T> { }


    public class GenericInterfaceImplNested<T> : GenericInterfaceImpl<T> { }

    public class GenericInterfaceTypedImplNested : GenericInterfaceImpl<string> { }

    public class GenericInterfaceTypedImpl : GenericInterface<string> { }

    public class NonGenericInterfaceImpl : NonGenericInterface { }

    public interface NestedGenericInterface<T> : GenericInterface<T> { }

    public class NestedGenericInterfaceImpl<T> : NestedGenericInterface<T> { }

    [GetAttr]
    public class GetAttrDefualt { }

    public class GetAttrNull { }

    public class GetAttrInherit : GetAttrDefualt { }

    [GetAttr("1")]
    [GetAttr("2")]
    public class GetAttrMulti { }

    public class GetAttrInheritMulti : GetAttrMulti { }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class GetAttrAttribute : Attribute
    {
        public GetAttrAttribute()
        {

        }

        public GetAttrAttribute(string value)
        {
            Value = value;
        }

        public string Value { get; }
    }
}
