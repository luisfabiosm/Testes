using Biro.Core.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Biro.Core.Domain.ValueObjects
{
    public record Document
    {
        public string Number { get; }
        public DocumentType Type { get; }

        public Document(string number, DocumentType type)
        {
            if (string.IsNullOrWhiteSpace(number))
                throw new ArgumentException("Document number cannot be empty", nameof(number));

            Number = CleanDocumentNumber(number);
            Type = type;

            if (!IsValid())
                throw new ArgumentException($"Invalid {type} number: {number}", nameof(number));
        }

        private string CleanDocumentNumber(string number)
        {
            return Regex.Replace(number, @"\D", "");
        }

        private bool IsValid()
        {
            return Type switch
            {
                DocumentType.CPF => ValidateCPF(Number),
                DocumentType.CNPJ => ValidateCNPJ(Number),
                _ => true
            };
        }

        private bool ValidateCPF(string cpf)
        {
            if (cpf.Length != 11)
                return false;

            // Check if all digits are the same
            if (Regex.IsMatch(cpf, @"^(\d)\1{10}$"))
                return false;

            // Validate check digits
            var digits = cpf.Select(c => int.Parse(c.ToString())).ToArray();

            // First check digit
            var sum = 0;
            for (int i = 0; i < 9; i++)
                sum += digits[i] * (10 - i);

            var checkDigit1 = 11 - (sum % 11);
            if (checkDigit1 >= 10)
                checkDigit1 = 0;

            if (digits[9] != checkDigit1)
                return false;

            // Second check digit
            sum = 0;
            for (int i = 0; i < 10; i++)
                sum += digits[i] * (11 - i);

            var checkDigit2 = 11 - (sum % 11);
            if (checkDigit2 >= 10)
                checkDigit2 = 0;

            return digits[10] == checkDigit2;
        }

        private bool ValidateCNPJ(string cnpj)
        {
            if (cnpj.Length != 14)
                return false;

            // Check if all digits are the same
            if (Regex.IsMatch(cnpj, @"^(\d)\1{13}$"))
                return false;

            var digits = cnpj.Select(c => int.Parse(c.ToString())).ToArray();
            var weights1 = new[] { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            var weights2 = new[] { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

            // First check digit
            var sum = 0;
            for (int i = 0; i < 12; i++)
                sum += digits[i] * weights1[i];

            var checkDigit1 = 11 - (sum % 11);
            if (checkDigit1 >= 10)
                checkDigit1 = 0;

            if (digits[12] != checkDigit1)
                return false;

            // Second check digit
            sum = 0;
            for (int i = 0; i < 13; i++)
                sum += digits[i] * weights2[i];

            var checkDigit2 = 11 - (sum % 11);
            if (checkDigit2 >= 10)
                checkDigit2 = 0;

            return digits[13] == checkDigit2;
        }

        public string GetMaskedNumber()
        {
            return Type switch
            {
                DocumentType.CPF => $"{Number.Substring(0, 3)}.{Number.Substring(3, 3)}.{Number.Substring(6, 3)}-{Number.Substring(9, 2)}",
                DocumentType.CNPJ => $"{Number.Substring(0, 2)}.{Number.Substring(2, 3)}.{Number.Substring(5, 3)}/{Number.Substring(8, 4)}-{Number.Substring(12, 2)}",
                _ => Number
            };
        }

        public override string ToString() => GetMaskedNumber();
    }


}
