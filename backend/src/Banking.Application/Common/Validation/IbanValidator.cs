using System.Numerics;
using System.Text;

namespace Banking.Application.Common.Validation;

public static class IbanValidator
{
    public static bool IsValid(string? iban)
    {
        if (string.IsNullOrWhiteSpace(iban)) return false;
        var v = iban.Trim().ToUpperInvariant().Replace(" ", "");
        if (v.Length < 15 || v.Length > 34) return false;
        if (!char.IsLetter(v[0]) || !char.IsLetter(v[1]) || !char.IsDigit(v[2]) || !char.IsDigit(v[3])) return false;

        var reordered = v[4..] + v[..4];
        var sb = new StringBuilder(reordered.Length * 2);

        foreach (var ch in reordered)
        {
            if (char.IsDigit(ch)) sb.Append(ch);
            else if (char.IsLetter(ch)) sb.Append((ch - 'A' + 10).ToString());
            else return false;
        }

        var num = BigInteger.Parse(sb.ToString());
        return num % 97 == 1;
    }
}
