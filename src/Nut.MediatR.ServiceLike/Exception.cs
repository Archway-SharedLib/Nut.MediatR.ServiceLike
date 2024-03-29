using System;
using MediatR;

namespace Nut.MediatR.ServiceLike;

/// <summary>
/// <see cref="ServiceLike"/> で発生した例外のベースクラスを定義します。
/// </summary>
public class MediatRServiceLikeException : Exception
{
    /// <summary>
    /// インスタンスを初期化します。
    /// </summary>
    public MediatRServiceLikeException() : base()
    {
    }

    /// <summary>
    /// インスタンスを初期化します。
    /// </summary>
    /// <param name="message">エラーに関するメッセージ</param>
    public MediatRServiceLikeException(string message) : base(message)
    {
    }
}

/// <summary>
/// 指定されたパスに紐づいた<see cref="IRequest"/>が見つからない場合に発生します。
/// </summary>
public class ReceiverNotFoundException: MediatRServiceLikeException
{
    /// <summary>
    /// インスタンスを初期化します。
    /// </summary>
    /// <param name="requestPath">送信先のパス</param>
    public ReceiverNotFoundException(string requestPath) : base(Resources.Strings.MediatorRequestNotFound(requestPath))
    {
        RequestPath = requestPath;
    }

    /// <summary>
    /// インスタンスを初期化します。
    /// </summary>
    /// <param name="requestPath">送信先のパス</param>
    /// <param name="message">エラーに関するメッセージ</param>
    public ReceiverNotFoundException(string requestPath, string message) : base(message)
    {
        RequestPath = requestPath;
    }

    /// <summary>
    /// 送信先のパスを取得します。
    /// </summary>
    public string RequestPath { get; }
}

/// <summary>
/// 指定されたパスに紐づいた<see cref="IRequest"/>が見つからない場合に発生します。
/// </summary>
public class TypeTranslationException: MediatRServiceLikeException
{
    /// <summary>
    /// インスタンスを初期化します。
    /// </summary>
    /// <param name="fromType">変換元の<see cref="Type"/></param>
    /// <param name="toType">変換先の<see cref="Type"/></param>
    public TypeTranslationException(Type fromType, Type toType) : base(Resources.Strings.CannotTypeTranslation(fromType, toType))
    {
        FromType = fromType;
        ToType = toType;
    }

    /// <summary>
    /// インスタンスを初期化します。
    /// </summary>
    /// <param name="fromType">変換元の<see cref="Type"/></param>
    /// <param name="toType">変換先の<see cref="Type"/></param>
    /// <param name="message">エラーに関するメッセージ</param>
    public TypeTranslationException(Type fromType, Type toType, string message) : base(message)
    {
        FromType = fromType;
        ToType = toType;
    }

    /// <summary>
    /// 変換元の型を取得します。
    /// </summary>
    /// <value>変換元の型</value>
    public Type FromType { get; }

    /// <summary>
    /// 変換先の型を取得します。
    /// </summary>
    /// <value>変換先の型</value>
    public Type ToType { get; }
}
