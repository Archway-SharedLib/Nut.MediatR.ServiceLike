<img src="./assets/logo/logo.svg" alt="logo" height="192px" style="margin-bottom:2rem;" />

# Nut.MediatR.Behaviors

Nut.MediatR.Behaviorsは[MediatR]の、様々なアプリケーションで利用できる汎用のカスタム[Behavior](https://github.com/jbogard/MediatR/wiki/Behaviors)を提供します。

Nut.MediatR.Behaviorsには次のBehaviorが含まれます。

- [PerRequestBehavior](./docs/PerRequestBehavior.md)
- [AuthorizationBehavior](./docs/AuthorizationBehavior.md)
- [LoggingBehavior](./docs/LoggingBehavior.md)

```cs
[WithBehaviors(
    typeof(LoggingBehavior),
    typeof(AuthorizationBehavior)
)]
public class ProductQuery: IRequest<ProductQueryResult> {
}
```

詳細は各リンク先を参照してください。

[MediatR]:https://github.com/jbogard/MediatR