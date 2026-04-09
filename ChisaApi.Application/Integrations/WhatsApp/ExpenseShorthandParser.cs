using System.Globalization;
using System.Text.RegularExpressions;

namespace ChisaApi.Application.Integrations.WhatsApp;

public static partial class ExpenseShorthandParser
{
    [GeneratedRegex(@"^\s*(\d+(?:[.,]\d+)?)\s+(.+)$", RegexOptions.CultureInvariant)]
    private static partial Regex ShorthandPattern();

    public static bool TryParse(string text, out decimal amount, out string categoryHint)
    {
        amount = 0;
        categoryHint = "";

        if (string.IsNullOrWhiteSpace(text))
            return false;

        Match m = ShorthandPattern().Match(text.Trim());
        if (!m.Success)
            return false;

        string numRaw = m.Groups[1].Value.Replace(',', '.');
        if (!decimal.TryParse(numRaw, NumberStyles.Number, CultureInfo.InvariantCulture, out amount) || amount <= 0)
            return false;

        categoryHint = m.Groups[2].Value.Trim();
        return categoryHint.Length > 0;
    }
}
