namespace Domain.Core.Common.Mediator
{
    public interface IBSRequestHandler<in TRequest, TResponse> where TRequest : IBSRequest<TResponse>
    {
        Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
    }
}
