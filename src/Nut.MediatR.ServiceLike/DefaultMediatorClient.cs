using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Nut.MediatR.ServiceLike.Internals;
using SR = Nut.MediatR.ServiceLike.Resources.Strings;

namespace Nut.MediatR.ServiceLike;

/// <summary>
/// <see cref="IMediatorClient"/> のデフォルトの実装を定義します。
/// </summary>
public class DefaultMediatorClient : IMediatorClient
{
    private readonly IMediator _mediator;
    private readonly ServiceRegistry _serviceRegistry;
    private readonly ListenerRegistry _listenerRegistry;
    private readonly IServiceProvider _provider;
    private readonly IScopedServiceFactoryFactory _scopedServiceFactoryFactory;
    private readonly ServiceLikeLoggerWrapper _logger;

    /// <summary>
    /// インスタンスを初期化します。
    /// </summary>
    /// <param name="serviceRegistry">サービスが登録されている<see cref="ServiceRegistry"/></param>
    /// <param name="eventRegistry">イベントリスナーが登録されている<see cref="ListenerRegistry"/></param>
    /// <param name="serviceProvider">サービスを取得するための <see cref="IServiceProvider"/></param>
    /// <param name="scopedServiceFactoryFactory"><see cref="IScoepedServiceFactory"/>を作成する <see cref="IScopedServiceFactoryFactory"/></param>
    /// <param name="logger">ログ出力を行う <see cref="IServiceLikeLogger"/></param>
    public DefaultMediatorClient(ServiceRegistry serviceRegistry, ListenerRegistry eventRegistry,
        IServiceProvider serviceProvider, IScopedServiceFactoryFactory scopedServiceFactoryFactory,
        IServiceLikeLogger logger)
    {
        _provider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider)); ;
        _serviceRegistry = serviceRegistry ?? throw new ArgumentNullException(nameof(serviceRegistry));
        _listenerRegistry = eventRegistry ?? throw new ArgumentNullException(nameof(eventRegistry));
        _scopedServiceFactoryFactory = scopedServiceFactoryFactory ?? throw new ArgumentNullException(nameof(scopedServiceFactoryFactory));
        _logger = new ServiceLikeLoggerWrapper(logger);
        _mediator = new Mediator(serviceProvider);
    }

    /// <summary>
    /// 指定された <paramref name="path"/> で設定されている <see cref="IRequest{TResponse}"/> にメッセージを送信します。
    /// </summary>
    /// <param name="path">送信先のパス</param>
    /// <param name="request">リクエスト</param>
    /// <typeparam name="TResult">レスポンスの型</typeparam>
    /// <returns>レスポンスの値</returns>
    public async Task<TResult?> SendAsync<TResult>(string path, object request) where TResult : class
    {
        var result = await SendAsyncInternal(path, request, typeof(TResult));
        return TranslateType(result, typeof(TResult)) as TResult;
    }

    /// <summary>
    /// 指定された <paramref name="path"/> で設定されている <see cref="IRequest"/> にメッセージを送信します。
    /// </summary>
    /// <param name="path">送信先のパス</param>
    /// <param name="request">リクエスト</param>
    public async Task SendAsync(string path, object request)
        => await SendAsyncInternal(path, request, null);

    private async Task<object?> SendAsyncInternal(string path, object request, Type? resultType)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var mediatorRequest = _serviceRegistry.GetService(path);
        if (mediatorRequest is null)
        {
            throw new ReceiverNotFoundException(path);
        }
        var value = TranslateType(request, mediatorRequest.ServiceType);

        var context = new RequestContext(mediatorRequest.Path, mediatorRequest.ServiceType, _provider, resultType);

        return await ExecuteAsync(new Queue<Type>(mediatorRequest.Filters), value, context).ConfigureAwait(false);
    }

    private object? TranslateType(object? value, Type toType)
    {
        if (value is null or Unit) return null;
        var fromType = value.GetType();
        try
        {
            var json = JsonSerializer.Serialize(value, fromType, new JsonSerializerOptions());
            return JsonSerializer.Deserialize(json, toType);
        }
        catch (JsonException je)
        {
            _logger.HandleException(je);
            throw new TypeTranslationException(fromType, toType);
        }
    }

    private async Task<object?> ExecuteAsync(Queue<Type> filterTypes, object? parameter, RequestContext context)
    {
        if (filterTypes.TryDequeue(out var filterType))
        {
            var filter = filterType.Activate<IMediatorServiceFilter>();
            return await filter.HandleAsync(context, parameter, async (newParam) =>
                await ExecuteAsync(filterTypes, newParam, context).ConfigureAwait(false)
            ).ConfigureAwait(false);
        }
        return await _mediator.Send(parameter!).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task PublishAsync(string key, object eventData)
        => PublishAsync(key, eventData, new PublishOptions());

    /// <inheritdoc />
    public Task PublishAsync(string key, object @eventData, Action<PublishOptions> optionsAction)
    {
        if (optionsAction is null)
        {
            throw new ArgumentNullException(nameof(optionsAction));
        }
        var options = new PublishOptions();
        optionsAction(options);
        return PublishAsync(key, eventData, options);
    }

    /// <inheritdoc />
    public Task PublishAsync(string key, object eventData, PublishOptions options)
    {
        if (eventData is null)
        {
            throw new ArgumentNullException(nameof(eventData));
        }

        var mediatorNotifications = _listenerRegistry.GetListeners(key);
        PublishAndForget(mediatorNotifications, eventData, key, options);

        return Task.CompletedTask;
    }

    private void PublishAndForget(IEnumerable<MediatorListenerDescription> listeners, object notification, string key, PublishOptions options)
    {
        Task.Run(async () =>
        {
            var context = new ServiceLikeContext(key, options.Header);
            var scopeHolder = new List<IScoepedServiceFactory>();
            try
            {
                var listenersList = listeners.ToList();
                _logger.TraceStartPublishToListeners(key, listenersList);

                var publishTasks = new List<Task>();

                if (options.BeforePublishAsyncHandler is not null)
                {
                    await options.BeforePublishAsyncHandler.Invoke(notification, context).ConfigureAwait(false);
                }

                foreach (var listener in listenersList)
                {
                    try
                    {
                        var scope = _scopedServiceFactoryFactory.Create();
                        scopeHolder.Add(scope);

                        var contextAccessors = scope.Instance.GetServices<IServiceLikeContextAccessor>().ToList();
                        if (contextAccessors.Any())
                        {
                            contextAccessors.First().Context = context;
                        }

                        var serviceLikeMediator = new Mediator(scope.Instance, new ForeachAllAwaitPublisher());

                        var value = TranslateType(notification, listener.ListenerType);
                        _logger.TracePublishToListener(listener);
                        publishTasks.Add(FireEvent(listener, serviceLikeMediator, value!));
                        // scope.Dispose();
                    }
                    catch (Exception ex)
                    {
                        _logger.ErrorOnPublish(ex, listener);
                    }
                }

                // すべて投げたまでで完了とする。
                if (options.CompleteAsyncHandler is not null)
                {
                    await options.CompleteAsyncHandler.Invoke(notification, context).ConfigureAwait(false);
                }

                _logger.TraceFinishPublishToListeners(key);

                await Task.WhenAll(publishTasks);
            }
            catch (Exception e)
            {
                if (options.ErrorAsyncHandler is not null)
                {
                    await options.ErrorAsyncHandler.Invoke(e, notification, context).ConfigureAwait(false);
                }

                _logger.ErrorOnPublishEvents(e, key);
            }
            finally
            {
                foreach (var scope in scopeHolder)
                {
                    try
                    {
                        scope.Dispose();
                    }catch{}
                }
            }
        }).ConfigureAwait(false);
    }

    private Task FireEvent(MediatorListenerDescription description, Mediator serviceLikeMediator,
        object eventData)
        => description.MediateType == MediateType.Notification
            ? serviceLikeMediator.Publish(eventData)
            : serviceLikeMediator.Send(eventData);
}
