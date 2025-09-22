using Domain.Core.Models.Request;
using Domain.Core.SharedKernel.Validation;
using System.ComponentModel.DataAnnotations;


using Domain.UseCases.CreateUser;

public class CreateUserRequestValidator : BaseValidator<TransactionCreateUser>
{
    // Padrão para telefone brasileiro: (11) 99999-9999 ou (11) 9999-9999
    private const string PhonePattern = @"^\(\d{2}\)\s\d{4,5}-\d{4}$";
    private const string CpfPattern = @"^\d{3}\.\d{3}\.\d{3}-\d{2}$";


    protected override void ValidateInternal(TransactionCreateUser transaction)
    {
        // Validar Nome
        ValidateRequired(transaction.NewUser.Nome, nameof(transaction.NewUser.Nome), "Nome é obrigatório");
        ValidateMinLength(transaction.NewUser.Nome, 2, nameof(transaction.NewUser.Nome), "Nome deve ter pelo menos 2 caracteres");
        ValidateMaxLength(transaction.NewUser.Nome, 100, nameof(transaction.NewUser.Nome), "Nome deve ter no máximo 100 caracteres");

        // Validar Email
        ValidateRequired(transaction.NewUser.Email, nameof(transaction.NewUser.Email), "Email é obrigatório");
        ValidateEmail(transaction.NewUser.Email, nameof(transaction.NewUser.Email), "Email deve ter formato válido");
        ValidateMaxLength(transaction.NewUser.Email, 255, nameof(transaction.NewUser.Email), "Email deve ter no máximo 255 caracteres");

        // Validar Telefone
        ValidateRequired(transaction.NewUser.CPF.ToString(), nameof(transaction.NewUser.CPF), "CPF é obrigatório");
        ValidatePattern(transaction.NewUser.CPF.ToString(), CpfPattern, nameof(transaction.NewUser.CPF), "CPF deve ter formato válido (ex: 625.666.102-82)");       
    }
}
