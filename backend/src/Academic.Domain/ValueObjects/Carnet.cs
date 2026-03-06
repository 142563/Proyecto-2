using System.Text.RegularExpressions;

namespace Academic.Domain.ValueObjects;

public sealed class Carnet
{
    private static readonly Regex Format = new(@"^(?<prefix>\d{4})-(?<year>\d{2})-(?<seq>\d{4,5})$", RegexOptions.Compiled);

    public string Value { get; }
    public string Prefix { get; }
    public short EntryYearTwoDigits { get; }
    public string Sequence { get; }

    private Carnet(string value, string prefix, short entryYearTwoDigits, string sequence)
    {
        Value = value;
        Prefix = prefix;
        EntryYearTwoDigits = entryYearTwoDigits;
        Sequence = sequence;
    }

    public static bool TryCreate(string input, out Carnet? carnet)
    {
        carnet = null;
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        var normalized = input.Trim();
        var match = Format.Match(normalized);
        if (!match.Success)
        {
            return false;
        }

        var prefix = match.Groups["prefix"].Value;
        var year = short.Parse(match.Groups["year"].Value);
        var seq = match.Groups["seq"].Value;
        carnet = new Carnet($"{prefix}-{year:00}-{seq}", prefix, year, seq);
        return true;
    }
}
