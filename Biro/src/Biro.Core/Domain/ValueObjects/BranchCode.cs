using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Biro.Core.Domain.ValueObjects
{
    public record BranchCode
    {
        private static readonly Regex BranchCodeRegex = new(@"^\d{4}$", RegexOptions.Compiled);

        public string Value { get; }

        public BranchCode(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Branch code cannot be empty", nameof(value));

            if (!BranchCodeRegex.IsMatch(value))
                throw new ArgumentException("Branch code must be exactly 4 digits", nameof(value));

            Value = value;
        }

        public override string ToString() => Value;

        public static implicit operator string(BranchCode branchCode) => branchCode.Value;
    }

}
