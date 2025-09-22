

//namespace Domain.UseCases.CreateUser;


//// ===== VALIDADOR PARA UpdateUserRequest =====

//using Domain.UseCases.UpdateUserInfo;

//namespace Domain.UseCases.UpdateUserInfo;

//public class UpdateUserInfoRequestValidator : BaseValidator<UpdateUserInfoRequest>
//{
//    private const string PhonePattern = @"^\(\d{2}\)\s\d{4,5}-\d{4}$";

//    protected override void ValidateInternal(UpdateUserInfoRequest request)
//    {
//        // Validar ID
//        ValidateGuid(request.Id, nameof(request.Id), "ID do usuário é obrigatório");

//        // Validar Nome
//        ValidateRequired(request.Name, nameof(request.Name), "Nome é obrigatório");
//        ValidateMinLength(request.Name, 2, nameof(request.Name), "Nome deve ter pelo menos 2 caracteres");
//        ValidateMaxLength(request.Name, 100, nameof(request.Name), "Nome deve ter no máximo 100 caracteres");

//        // Validar Email
//        ValidateRequired(request.Email, nameof(request.Email), "Email é obrigatório");
//        ValidateEmail(request.Email, nameof(request.Email), "Email deve ter formato válido");
//        ValidateMaxLength(request.Email, 255, nameof(request.Email), "Email deve ter no máximo 255 caracteres");

//        // Validar Telefone
//        ValidateRequired(request.Phone, nameof(request.Phone), "Telefone é obrigatório");
//        ValidatePattern(request.Phone, PhonePattern, nameof(request.Phone),
//            "Telefone deve ter formato válido: (11) 99999-9999");
//    }
//}

//// ===== VALIDADOR PARA GetUserByIdRequest =====

//using Domain.UseCases.GetUserById;

//namespace Domain.UseCases.GetUserById;

//public class GetUserByIdRequestValidator : BaseValidator<GetUserByIdRequest>
//{
//    protected override void ValidateInternal(GetUserByIdRequest request)
//    {
//        ValidateGuid(request.Id, nameof(request.Id), "ID do usuário é obrigatório");
//    }
//}

//// ===== VALIDADOR PARA DeleteUserRequest =====

//using Domain.UseCases.DeleteUser;

//namespace Domain.UseCases.DeleteUser;

//public class DeleteUserRequestValidator : BaseValidator<DeleteUserRequest>
//{
//    protected override void ValidateInternal(DeleteUserRequest request)
//    {
//        ValidateGuid(request.Id, nameof(request.Id), "ID do usuário é obrigatório");
//    }
//}

//// ===== VALIDADOR PARA ListUsersRequest =====

//using Domain.UseCases.ListUsers;

//namespace Domain.UseCases.ListUsers;

//public class ListUsersRequestValidator : BaseValidator<ListUsersRequest>
//{
//    protected override void ValidateInternal(ListUsersRequest request)
//    {
//        ValidateGreaterThan(request.Page, 0, nameof(request.Page), "Página deve ser maior que zero");
//        ValidateGreaterThan(request.Size, 0, nameof(request.Size), "Tamanho da página deve ser maior que zero");
//        ValidateRange(request.Size, 1, 100, nameof(request.Size), "Tamanho da página deve estar entre 1 e 100");
//    }
//}

//// ===== EXEMPLO DE VALIDAÇÃO COMPLEXA =====

//namespace Domain.Core.SharedKernel.Validation;

//// Validador mais complexo com validações assíncronas
//public class ComplexUserValidator : BaseValidator<CreateUserRequest>
//{
//    private readonly IUserRepository _userRepository;

//    public ComplexUserValidator(IUserRepository userRepository)
//    {
//        _userRepository = userRepository;
//    }

//    protected override void ValidateInternal(CreateUserRequest request)
//    {
//        // Validações síncronas básicas
//        ValidateRequired(request.Name, nameof(request.Name));
//        ValidateEmail(request.Email, nameof(request.Email));
//        ValidatePattern(request.Phone, @"^\(\d{2}\)\s\d{4,5}-\d{4}$", nameof(request.Phone));

//        // Validações de negócio customizadas
//        ValidateBusinessRules(request);
//    }

//    protected override async Task ValidateInternalAsync(CreateUserRequest request, CancellationToken cancellationToken)
//    {
//        // Executar validações síncronas primeiro
//        ValidateInternal(request);

//        // Validações assíncronas (banco de dados, APIs externas, etc.)
//        await ValidateEmailNotExists(request.Email, cancellationToken);
//        await ValidatePhoneNotExists(request.Phone, cancellationToken);
//    }

//    private void ValidateBusinessRules(CreateUserRequest request)
//    {
//        // Exemplo: Nome não pode conter números
//        if (!string.IsNullOrEmpty(request.Name) && request.Name.Any(char.IsDigit))
//        {
//            _errors.Add(new ValidationError(nameof(request.Name), "Nome não pode conter números", request.Name));
//        }

//        // Exemplo: Telefone deve ser de SP (código 11)
//        if (!string.IsNullOrEmpty(request.Phone) && !request.Phone.StartsWith("(11)"))
//        {
//            _errors.Add(new ValidationError(nameof(request.Phone), "Apenas telefones de São Paulo são aceitos", request.Phone));
//        }

//        // Exemplo: Email deve ser corporativo
//        if (!string.IsNullOrEmpty(request.Email) && !request.Email.EndsWith("@company.com"))
//        {
//            _errors.Add(new ValidationError(nameof(request.Email), "Apenas emails corporativos são aceitos", request.Email));
//        }
//    }

//    private async Task ValidateEmailNotExists(string email, CancellationToken cancellationToken)
//    {
//        if (string.IsNullOrEmpty(email)) return;

//        var result = await _userRepository.ExistsByEmailAsync(email, cancellationToken);
//        if (result.IsSuccess && result.Value)
//        {
//            _errors.Add(new ValidationError(nameof(CreateUserRequest.Email), "Email já está em uso", email));
//        }
//    }

//    private async Task ValidatePhoneNotExists(string phone, CancellationToken cancellationToken)
//    {
//        if (string.IsNullOrEmpty(phone)) return;

//        // Assumindo que existe um método para verificar telefone
//        // var result = await _userRepository.ExistsByPhoneAsync(phone, cancellationToken);
//        // if (result.IsSuccess && result.Value)
//        // {
//        //     _errors.Add(new ValidationError(nameof(CreateUserRequest.Phone), "Telefone já está em uso", phone));
//        // }
//    }
//}

//// ===== ATUALIZAÇÃO DO PROCESSADOR PARA USAR A VALIDAÇÃO CUSTOMIZADA =====

//// Exemplo de como usar no processador:
///*
//public class CreateUserProcessor
//{
//    private readonly IUserRepository _userRepository;
//    private readonly CreateUserRequestValidator _validator;
//    private readonly ILogger<CreateUserProcessor> _logger;

//    public CreateUserProcessor(
//        IUserRepository userRepository,
//        CreateUserRequestValidator validator,
//        ILogger<CreateUserProcessor> logger)
//    {
//        _userRepository = userRepository;
//        _validator = validator;
//        _logger = logger;
//    }

//    public async Task<Result<CreateUserResponse>> ProcessAsync(
//        CreateUserRequest request,
//        CancellationToken cancellationToken = default)
//    {
//        try
//        {
//            // Usar validação customizada
//            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
//            if (!validationResult.IsValid)
//            {
//                var errors = validationResult.GetErrorsAsString();
//                _logger.LogWarning("Dados inválidos para criação de usuário: {Errors}", errors);
//                return Result.Failure<CreateUserResponse>($"Dados inválidos: {errors}");
//            }

//            // ... resto da implementação
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Erro inesperado no processador");
//            return Result.Failure<CreateUserResponse>($"Erro interno: {ex.Message}");
//        }
//    }
//}
//*/