using System.Collections.Concurrent;

namespace Domain.Core.Common.Mediator;


public class BSMediator
{
    private readonly IServiceProvider _serviceProvider;

    // Cache de tipos para evitar reflection repetitiva
    private static readonly ConcurrentDictionary<Type, Type> HandlerTypeCache = new();

    public BSMediator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<TResponse> Send<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : IBSRequest<TResponse>
    {
        var requestType = typeof(TRequest);

        // Cache lookup para evitar reflection custosa
        var handlerType = HandlerTypeCache.GetOrAdd(requestType,
            _ => typeof(IBSRequestHandler<TRequest, TResponse>));

        var handler = (IBSRequestHandler<TRequest, TResponse>)_serviceProvider.GetService(handlerType);

        if (handler == null)
        {
            throw new InvalidOperationException(
                $"Nenhum handler encontrado para {requestType.Name}. " +
                $"Verifique se {handlerType.Name} está registrado no DI container.");
        }

        return await handler.Handle(request, cancellationToken);
    }
}