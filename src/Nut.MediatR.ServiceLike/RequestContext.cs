using System;
using MediatR;
using SR = Nut.MediatR.ServiceLike.Resources.Strings;

namespace Nut.MediatR.ServiceLike;

/// <summary>
/// メッセージを送信する際のリクエスト情報を保持します。
/// </summary>
public class RequestContext
{
    /// <summary>
    /// インスタンスを初期化します。
    /// </summary>
    /// <param name="path">送信する先のサービスのパス</param>
    /// <param name="mediatorParameterType">呼び出し先のパラメーターの型</param>
    /// <param name="serviceProvider">送信先を取得するための <see cref="ServiceProvider"/></param>
    /// <param name="clientResultType">呼び出し元の戻り値の型</param>
    public RequestContext(string path, Type mediatorParameterType, IServiceProvider serviceProvider, Type? clientResultType = null)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException(SR.Argument_CanNotNullOrWhitespace(nameof(path)));
        }
        Path = path;
        MediatorParameterType = mediatorParameterType ?? throw new ArgumentNullException(nameof(mediatorParameterType));
        ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        ClientResultType = clientResultType;
    }

    /// <summary>
    /// 呼び出し先のサービスのパスを取得します。
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// 呼び出し先のパラメーターの型を取得します。
    /// </summary>
    public Type MediatorParameterType { get; }

    /// <summary>
    /// 呼び出し元の戻り値の型を取得します。
    /// </summary>
    public Type? ClientResultType { get; }

    /// <summary>
    /// クライアントに戻り値が必要かどうかを取得します。
    /// </summary>
    public bool NeedClientResult => ClientResultType is not null;

    /// <summary>
    /// <see cref="ServiceProvider"/> を取得します。
    /// </summary>
    public IServiceProvider ServiceProvider { get; }
}
