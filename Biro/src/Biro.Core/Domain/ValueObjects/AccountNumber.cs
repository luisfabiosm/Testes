using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Biro.Core.Domain.ValueObjects
 public record AccountNumber
{
    private static readonly Regex AccountNumberRegex = new(@"^\d{8}\d{5}-\d$", RegexOptions.Compiled);

    public string Value { get; }

    public AccountNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Account number cannot be empty", nameof(value));

        Value = value;
    }

    public static AccountNumber Generate()
    {
        var random = new Random();
        var date = DateTime.Now.ToString("yyyyMMdd");
        var sequence = random.Next(10000, 99999);
        var checkDigit = random.Next(0, 9);

        return new AccountNumber($"{date}{sequence}-{checkDigit}");
    }

    public override string ToString() => Value;

    public static implicit operator string(AccountNumber accountNumber) => accountNumber.Value;
}

}
