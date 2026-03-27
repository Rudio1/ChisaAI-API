using System.Text;

namespace ChisaApi.Application.Common;

public static class PhoneNumberNormalizer
{
    public static string ToBrazilE164(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentException("Telefone é obrigatório.");

        StringBuilder sb = new();
        foreach (char c in input)
        {
            if (char.IsDigit(c))
                sb.Append(c);
        }

        string d = sb.ToString();
        if (d.Length == 0)
            throw new ArgumentException("Telefone inválido.");

        if (d.StartsWith("55", StringComparison.Ordinal) && d.Length >= 12)
            return "+" + d;

        if (d.Length is >= 10 and <= 11)
            return "+55" + d;

        if (d.Length == 13 && d.StartsWith("55", StringComparison.Ordinal))
            return "+" + d;

        return "+" + d;
    }
}
