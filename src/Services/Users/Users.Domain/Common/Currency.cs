namespace Users.Domain.Common;

public sealed record Currency
{
    public static readonly Currency Cop = new("COP");
    public static readonly Currency Usd = new("USD");

    private static readonly Dictionary<string, Currency> Supported = new() { [Cop.Code] = Cop, [Usd.Code] = Usd };

    private Currency(string code) => Code = code;

    public string Code { get; }

    public static Currency FromCode(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new DomainException("Currency code is required.");
        }

        var normalizedCode = code.Trim().ToUpperInvariant();

        return Supported.TryGetValue(normalizedCode, out var currency)
            ? currency
            : throw new DomainException($"Currency '{normalizedCode}' is not supported.");
    }

    public override string ToString() => Code;
}