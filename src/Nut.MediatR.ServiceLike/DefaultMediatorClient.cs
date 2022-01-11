using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using MediatR;
using Nut.MediatR.ServiceLike.Internals;
using SR = Nut.MediatR.ServiceLike.Resources.Strings;

namespace Nut.MediatR.ServiceLike;

public class DefaultMediatorClient : IMediatorClient
{
    private readonly IMediator _mediator;
    private readonly ServiceRegistry _serviceRegistry;
    private readonly ListenerRegistry _listenerRegistry;
    private readonly ServiceFactory _factory;
    private readonly IScopedServiceFactoryFactory _scopedServiceFactoryFactory;
    private readonly ServiceLikeLoggerWrapper _logger;

    public DefaultMediatorClient(ServiceRegistry serviceRegistry, ListenerRegistry eventRegistry,
        ServiceFactory serviceFactory, IScopedServiceFactoryFactory scopedServiceFactoryFactory,
        IServiceLikeLogger logger)
    {
        _factory = serviceFactory ?? throw new ArgumentNullException(nameof(serviceFactory)); ;
        _serviceRegistry = serviceRegistry ?? throw new ArgumentNullException(nameof(serviceRegistry));
        _listenerRegistry = eventRegistry ?? throw new ArgumentNullException(nameof(eventRegistry));
        _scopedServiceFactoryFactory = scopedServiceFactoryFactory ?? throw new ArgumentNullException(nameof(scopedServiceFactoryFactory));
        _logger = new ServiceLikeLoggerWrapper(logger);
        _mediator = new Mediator(serviceFactory);
    }

    public async Task<TResult?> SendAsync<TResult>(string path, object request) where TResult : class
    {
        var result = await SendAsyncInternal(path, request, typeof(TResult));
        return TranslateType(result, typeof(TResult)) as TResult;
    }

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
            throw new InvalidOperationException(SR.MediatorRequestNotFound(path));
        }
        var value = TranslateType(request, mediatorRequest.ServiceType);

        var context = new RequestContext(mediatorRequest.Path, mediatorRequest.ServiceType, _factory, resultType);

        return await ExecuteAsync(new Queue<Type>(mediatorRequest.Filters), value, context).ConfigureAwait(false);
    }

    private object? TranslateType(object? value, Type type)
    {
        if (value is null or Unit) return null;
        var json = JsonSerializer.Serialize(value, value.GetType(), new JsonSerializerOptions());
        return JsonSerializer.Deserialize(json, type);
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

    public Task PublishAsync(string key, object eventData)
        => PublishAsync(key, eventData, new PublishOptions());

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

            try
            {
                var listenersList = listeners.ToList();
                _logger.TraceStartPublishToListeners(key, listenersList);

                using var scope = _scopedServiceFactoryFactory.Create();

                var contextAccessors = scope.Instance.GetInstances<IServiceLikeContextAccessor>();
                if (contextAccessors.Any())
                {
                    contextAccessors.First().Context = context;
                }

                var publishTasks = new List<Task>();
                var serviceLikeMediator = new ServiceLikeMediator(scope.Instance);

                if (options.BeforePublishAsyncHandler is not null)
                {
                    await options.BeforePublishAsyncHandler.Invoke(notification, context).ConfigureAwait(false);
                }

                foreach (var listener in listenersList)
                {
                    try
                    {
                        var value = TranslateType(notification, listener.ListenerType);
                        _logger.TracePublishToListener(listener);
                        publishTasks.Add(FireEvent(listener, serviceLikeMediator, value!));
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
        }).ConfigureAwait(false);
    }

    private Task FireEvent(MediatorListenerDescription description, ServiceLikeMediator serviceLikeMediator,
        object eventData)
        => description.MediateType == MediateType.Notification
            ? serviceLikeMediator.Publish(eventData)
            : serviceLikeMediator.Send(eventData);
}
