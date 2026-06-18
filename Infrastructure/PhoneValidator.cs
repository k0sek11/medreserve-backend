using System.Text.RegularExpressions;

namespace Medreserve.Infrastructure;

public static partial class PhoneValidator
{
    private static readonly HashSet<string> PolishMobilePrefixes = new()
    {
        "45", "50", "51", "53", "57", "60", "66", "69", "72", "73", "78", "79", "88",
    };

    private static readonly HashSet<string> PolishLandlinePrefixes = new()
    {
        "12", "13", "14", "15", "16", "17", "18", "22", "23", "24", "25",
        "29", "32", "33", "34", "41", "42", "43", "44", "46", "48",
        "52", "54", "55", "56", "58", "59", "61", "62", "63", "65",
        "67", "68", "71", "74", "75", "76", "77", "81", "82", "83",
        "84", "85", "86", "87", "89", "91", "94", "95",
    };

    private static readonly HashSet<string> AllPolishPrefixes = new(
        PolishMobilePrefixes.Concat(PolishLandlinePrefixes));

    private static readonly Regex DigitsOnlyRegex = DigitsOnly();

    public static string Normalize(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return string.Empty;

        return DigitsOnlyRegex.Replace(phone, "");
    }

    public static string? TrimToNull(string? phone)
    {
        var trimmed = phone?.Trim();
        return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
    }

    public static bool IsValidPolishPhone(string? phone)
    {
        var digits = Normalize(phone);
        if (string.IsNullOrEmpty(digits))
            return false;

        if (digits.StartsWith('+'))
            digits = digits[1..];

        if (digits.StartsWith("48") && digits.Length == 11)
            return AllPolishPrefixes.Contains(digits[2..4]);

        if (digits.Length == 9)
            return AllPolishPrefixes.Contains(digits[..2]);

        return false;
    }

    public static bool IsValidPhone(string? phone)
    {
        var digits = Normalize(phone);
        if (string.IsNullOrEmpty(digits))
            return false;

        if (digits.StartsWith('+'))
            digits = digits[1..];

        return digits.Length is >= 7 and <= 15 && digits.All(char.IsDigit);
    }

    public static string ToE164(string? phone)
    {
        var digits = Normalize(phone);
        if (string.IsNullOrEmpty(digits))
            return string.Empty;

        if (digits.StartsWith('+'))
            return digits;

        if (digits.StartsWith("48") && digits.Length == 11)
            return $"+{digits}";

        if (digits.Length == 9 && AllPolishPrefixes.Contains(digits[..2]))
            return $"+48{digits}";

        return $"+{digits}";
    }

    [GeneratedRegex(@"[^\d]")]
    private static partial Regex DigitsOnly();
}
