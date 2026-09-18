using System.Collections.Frozen;
using System.Reflection;

namespace DnsZoneRecordManager.Cqrs
{
    /// <summary>Marker for a command/query producing <typeparamref name="TResponse"/> (hand-rolled Mediator pattern, no library).</summary>
    /// <typeparam name="TResponse">Response type.</typeparam>
    public interface IRequest<TResponse>;

    /// <summary>Handles one request type end to end.</summary>
    /// <typeparam name="TRequest">Request type.</typeparam>
    /// <typeparam name="TResponse">Response type.</typeparam>
    public interface IRequestHandler<in TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        /// <summary>Handles the request.</summary>
        /// <param name="request">Request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Response.</returns>
        Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken);
    }

    /// <summary>Dispatches requests to their handlers via DI.</summary>
    public interface ISender
    {
        /// <summary>Sends a request to its handler.</summary>
        /// <typeparam name="TResponse">Response type.</typeparam>
        /// <param name="request">Request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Handler response.</returns>
        Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken);
    }

    /// <summary>DI-backed sender: one <see cref="FrozenDictionary{TKey,TValue}"/> lookup, one cast, one call.
    /// No per-call reflection (unlike <c>MakeGenericType</c> + <c>dynamic</c> dispatchers).</summary>
    public sealed class Sender : ISender
    {
        private readonly IServiceProvider _services;
        private readonly DispatcherRegistry _registry;

        /// <summary>Creates the sender.</summary>
        /// <param name="services">Service provider used to resolve handlers.</param>
        /// <param name="registry">Startup-built request-type registry.</param>
        public Sender(IServiceProvider services, DispatcherRegistry registry)
        {
            _services = services;
            _registry = registry;
        }

        /// <inheritdoc />
        public async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            if (!_registry.Handlers.TryGetValue(request.GetType(), out var wrapper))
            {
                throw new InvalidOperationException($"No handler registered for request type '{request.GetType().Name}'.");
            }

            return await ((RequestHandlerBase<TResponse>)wrapper).Handle(request, _services, cancellationToken);
        }
    }

    /// <summary>Non-generic wrapper root so one dictionary holds every request/response pair.</summary>
    public abstract class RequestHandlerBase;

    /// <summary>Typed wrapper: resolves the handler from DI and invokes it (built once at startup).</summary>
    /// <typeparam name="TResponse">Response type.</typeparam>
    public abstract class RequestHandlerBase<TResponse> : RequestHandlerBase
    {
        /// <summary>Handles a request of the matching type.</summary>
        /// <param name="request">Request.</param>
        /// <param name="services">Service provider.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Response.</returns>
        public abstract Task<TResponse> Handle(IRequest<TResponse> request, IServiceProvider services, CancellationToken cancellationToken);
    }

    /// <summary>Startup-built wrapper for one request/handler pair.</summary>
    /// <typeparam name="TRequest">Request type.</typeparam>
    /// <typeparam name="TResponse">Response type.</typeparam>
    public sealed class RequestHandlerWrapper<TRequest, TResponse> : RequestHandlerBase<TResponse>
        where TRequest : IRequest<TResponse>
    {
        /// <inheritdoc />
        public override async Task<TResponse> Handle(IRequest<TResponse> request, IServiceProvider services, CancellationToken cancellationToken)
        {
            var handler = services.GetRequiredService<IRequestHandler<TRequest, TResponse>>();
            return await handler.HandleAsync((TRequest)request, cancellationToken);
        }
    }

    /// <summary>Startup-frozen request-type → wrapper registry (read-only on the hot path).</summary>
    public sealed class DispatcherRegistry
    {
        /// <summary>Creates the registry.</summary>
        /// <param name="handlers">Request-type → wrapper map.</param>
        public DispatcherRegistry(FrozenDictionary<Type, RequestHandlerBase> handlers)
        {
            Handlers = handlers;
        }

        /// <summary>Request-type → wrapper map.</summary>
        public FrozenDictionary<Type, RequestHandlerBase> Handlers { get; }
    }

    /// <summary>One-time startup registration: scans an assembly for handlers, builds wrappers, freezes the registry.</summary>
    public static class CqrsRegistration
    {
        /// <summary>Registers every <see cref="IRequestHandler{TRequest,TResponse}"/> in the assembly plus the sender.</summary>
        /// <param name="services">Service collection.</param>
        /// <param name="assembly">Assembly to scan.</param>
        /// <returns>Service collection (fluent).</returns>
        public static IServiceCollection AddCqrsHandlers(this IServiceCollection services, Assembly assembly)
        {
            var wrappers = new Dictionary<Type, RequestHandlerBase>();
            foreach (var type in assembly.GetTypes())
            {
                if (type.IsAbstract || type.IsInterface)
                {
                    continue;
                }

                foreach (var service in type.GetInterfaces())
                {
                    if (!service.IsGenericType || service.GetGenericTypeDefinition() != typeof(IRequestHandler<,>))
                    {
                        continue;
                    }

                    var arguments = service.GetGenericArguments();
                    var wrapper = (RequestHandlerBase)Activator.CreateInstance(typeof(RequestHandlerWrapper<,>).MakeGenericType(arguments))!;
                    wrappers.TryAdd(arguments[0], wrapper);
                    services.AddScoped(service, type);
                }
            }

            services.AddSingleton(new DispatcherRegistry(wrappers.ToFrozenDictionary()));
            services.AddScoped<ISender, Sender>();
            return services;
        }
    }
}
