using Domain.Core.Interfaces.Domain;
using Domain.Core.Interfaces.Outbound;
using Domain.Core.Models.Entities;
using Domain.Core.Models.Request;
using Domain.Core.Models.Response;
using Domain.Core.SharedKernel.Transactions;
using Domain.UseCases.CreateUser;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Adapters.Inbound.API.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users")
            .WithTags("Users")
            .WithOpenApi();

        // CREATE - Criar usuário
        group.MapPost("/", async (
            HttpContext httpContext,
            [FromBody]  CreateUserRequest request,
            [FromServices]  ITransactionFactory factory,
            [FromServices]  ICreateUserUseCase useCase,
            CancellationToken cancellationToken) =>
        {
            using var activity = Activity.Current?.Source.StartActivity("CreateUser.Endpoint");
            activity?.SetTag("endpoint", "POST /api/users");
            activity?.SetTag("request.email", request.Email);

            var correlationId = httpContext.Items["CorrelationId"]?.ToString() ?? "unknown";
            var _transaction = factory.CreateUserTransaction(httpContext, request, correlationId);
       
            var result = await useCase.ExecuteAsync(_transaction, cancellationToken);

            if (result.IsSuccess)
                return Results.Ok(ApiResponse<CreateUserResponse>.OnSuccess(result.Value, "Usuário criado com sucesso"));
          

            return Results.BadRequest(ApiResponse<object>.OnError(result.Error));
        })
        .WithName("CreateUser")
        .WithSummary("Criar novo usuário")
        .WithDescription("Cria um novo usuário no sistema")
        .Produces<ApiResponse<CreateUserResponse>>(StatusCodes.Status200OK)
        .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest);



        //// READ - Listar usuários (paginado)
        //group.MapGet("/", async (
        //    int page,
        //    int size,
        //    ListUsersUseCase useCase,
        //    CancellationToken cancellationToken) =>
        //{
        //    // Valores padrão
        //    page = page <= 0 ? 1 : page;
        //    size = size <= 0 ? 10 : Math.Min(size, 100); // Máximo 100 por página

        //    using var activity = Activity.Current?.Source.StartActivity("ListUsers.Endpoint");
        //    activity?.SetTag("endpoint", "GET /api/users");
        //    activity?.SetTag("query.page", page);
        //    activity?.SetTag("query.size", size);

        //    var useCaseRequest = new ListUsersRequest { Page = page, Size = size };

        //    var result = await useCase.ExecuteAsync(useCaseRequest, cancellationToken);

        //    if (result.IsSuccess)
        //    {
        //        var users = result.Value.Select(u => new UserResponseDto
        //        {
        //            Id = u.Id,
        //            Name = u.Name,
        //            Email = u.Email,
        //            Phone = u.Phone,
        //            Status = u.Status.ToString(),
        //            CreatedAt = u.CreatedAt,
        //            UpdatedAt = u.UpdatedAt
        //        });

        //        var paginatedResponse = new PaginatedResponseDto<UserResponseDto>
        //        {
        //            Data = users,
        //            Page = page,
        //            Size = size,
        //            HasNextPage = users.Count() == size // Simplificado - em produção calcular corretamente
        //        };

        //        return Results.Ok(
        //            ApiResponseDto<PaginatedResponseDto<UserResponseDto>>.Success(paginatedResponse));
        //    }

        //    return Results.BadRequest(
        //        ApiResponseDto<object>.Error(result.Error));
        //})
        //.WithName("ListUsers")
        //.WithSummary("Listar usuários")
        //.WithDescription("Lista usuários com paginação")
        //.Produces<ApiResponseDto<PaginatedResponseDto<UserResponseDto>>>(StatusCodes.Status200OK)
        //.Produces<ApiResponseDto<object>>(StatusCodes.Status400BadRequest);


        //// Endpoint de Health Check específico para usuários
        //group.MapGet("/health", async (
        //    IUserRepository userRepository,
        //    CancellationToken cancellationToken) =>
        //{
        //    try
        //    {
        //        // Tenta fazer uma consulta simples no repositório
        //        var healthCheckResult = await userRepository.ExistsAsync(Guid.NewGuid(), cancellationToken);

        //        return Results.Ok(new
        //        {
        //            Status = "Healthy",
        //            Timestamp = DateTime.UtcNow,
        //            Service = "Users",
        //            DatabaseConnection = healthCheckResult.IsSuccess ? "Connected" : "Disconnected"
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Results.StatusCode(503, new
        //        {
        //            Status = "Unhealthy",
        //            Timestamp = DateTime.UtcNow,
        //            Service = "Users",
        //            Error = ex.Message
        //        });
        //    }
        //})
        //.WithName("UsersHealthCheck")
        //.WithSummary("Health check do módulo de usuários")
        //.AllowAnonymous();
    }
}