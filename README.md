<img src="./assets/logo/logo.svg" alt="logo" height="192px" style="margin-bottom:2rem;" />

[![CI](https://github.com/Archway-SharedLib/Nut.MediatR.ServiceLike/workflows/CI/badge.svg)](https://github.com/Archway-SharedLib/Nut.MediatR.ServiceLike/actions)

## Nut.MediatR.ServiceLike

[![NuGet](https://img.shields.io/nuget/vpre/Nut.MediatR.ServiceLike.svg)](https://www.nuget.org/packages/Nut.MediatR.ServiceLike) 
[![NuGet](https://img.shields.io/nuget/dt/Nut.MediatR.ServiceLike.svg)](https://www.nuget.org/packages/Nut.MediatR.ServiceLike)

Nut.MediatR.ServiceLikeは[MediatR]のハンドラを、文字列で指定して実行できるようにするライブラリです。
Nut.MediatR.ServiceLikeを利用することで、`IRequest`および`INotification`の実装自体への依存も無くせます。

詳細は[ドキュメント](./docs/serviceLike/ServiceLike.md)を参照してください。

```cs
[AsService("/users/detail")]
public record UserQuery(string UserId): IRequest<UserDetail>;

public class UserService 
{
    private readonly IMediatorClient client;

    public GetUserService(IMediatorClient client)
    {
        this.client = client;
    }

    public async Task<User> Get(string id)
    {
        var result = await client.Send<User>("/users/detail", new {UserId = id});
        return result;
    }
}
```

## Nut.MediatR.ServiceLike.DependencyInjection

[![NuGet](https://img.shields.io/nuget/vpre/Nut.MediatR.ServiceLike.DependencyInjection.svg)](https://www.nuget.org/packages/Nut.MediatR.ServiceLike.DependencyInjection) 
[![NuGet](https://img.shields.io/nuget/dt/Nut.MediatR.ServiceLike.DependencyInjection.svg)](https://www.nuget.org/packages/Nut.MediatR.ServiceLike.DependencyInjection)

Nut.MediatR.ServiceLike.DependencyInjectionは[Microsoft.Extensions.DependencyInjection](https://docs.microsoft.com/ja-jp/dotnet/core/extensions/dependency-injection)の`IServiceCollection`を通して、Nut.MediatR.ServiceLikeを設定します。

詳細は[ドキュメント](./docs/serviceLike/DependencyInjection.md)を参照してください。

```cs
services
    .AddMediatR(typeof(Startup))
    .AddMediatRServiceLike(typeof(Startup).Assembly);
```

[MediatR]:https://github.com/jbogard/MediatR
[Behavior]:https://github.com/jbogard/MediatR/wiki/Behaviors
[FluentValidation]:https://fluentvalidation.net/
