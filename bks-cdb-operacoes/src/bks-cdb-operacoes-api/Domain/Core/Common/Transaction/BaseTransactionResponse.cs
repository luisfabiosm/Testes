namespace Domain.Core.Common.Transaction
{
    public record BaseTransactionResponse
    {
        public string CorrelationId { get; set; }

        public string chvAutorizador { get; set; }


        protected DateTime ParseDate(string dateString)
        {
            if (string.IsNullOrWhiteSpace(dateString))
                return DateTime.MinValue;

            // Tenta diferentes formatos de data
            var formats = new[] { "dd/MM/yyyy", "yyyy-MM-dd", "MM/dd/yyyy" };

            foreach (var format in formats)
            {
                if (DateTime.TryParseExact(dateString.Trim(), format,
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out DateTime result))
                {
                    return result;
                }
            }

            // Fallback para parse padrão
            if (DateTime.TryParse(dateString.Trim(), out DateTime fallbackResult))
                return fallbackResult;

            return DateTime.MinValue;
        }

        protected decimal ParseDecimal(string decimalString)
        {
            if (string.IsNullOrWhiteSpace(decimalString))
                return 0m;

            // Remove espaços e trata formato brasileiro (vírgula como separador decimal)
            var cleanString = decimalString.Trim().Replace(".", "").Replace(",", ".");

            if (decimal.TryParse(cleanString,
                System.Globalization.NumberStyles.AllowDecimalPoint | System.Globalization.NumberStyles.AllowLeadingSign,
                System.Globalization.CultureInfo.InvariantCulture, out decimal result))
            {
                return result;
            }

            return 0m;
        }

        protected int ParseInt(string intString)
        {
            if (string.IsNullOrWhiteSpace(intString))
                return 0;

            if (int.TryParse(intString.Trim(), out int result))
                return result;

            return 0;
        }
    }
}
