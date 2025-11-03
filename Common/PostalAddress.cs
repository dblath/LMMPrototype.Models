namespace LMMPrototype.Models;

public sealed class PostalAddress
{
    public string Street { get; }
    public string City { get; }
    public string State { get; }
    public string PostalCode { get; }

    public PostalAddress(string street, string city, string state, string postalCode)
    {
        Street = street?.Trim() ?? throw new ArgumentNullException(nameof(street));
        City = city?.Trim() ?? throw new ArgumentNullException(nameof(city));
        State = state?.Trim() ?? throw new ArgumentNullException(nameof(state));
        PostalCode = postalCode?.Trim() ?? throw new ArgumentNullException(nameof(postalCode));

        Validate();
    }

    public void Validate()
    {
        if (Street.Length < 3)
            throw new ArgumentException("Street must have at least 3 characters");
        if (City.Length < 2)
            throw new ArgumentException("City must have at least 2 characters");
        if (State.Length != 2)
            throw new ArgumentException("State must be a 2-letter code");
        if (!PostalCode.All(char.IsDigit) || PostalCode.Length < 5)
            throw new ArgumentException("Postal code must be numeric and at least 5 digits");
    }

    // 'With' pattern creates a new immutable address with modifications
    public PostalAddress With(
        string? street = null,
        string? city = null,
        string? state = null,
        string? postalCode = null)
    {
        return new PostalAddress(
            street ?? Street,
            city ?? City,
            state ?? State,
            postalCode ?? PostalCode
        );
    }

    public override string ToString() => $"{Street}, {City}, {State} {PostalCode}";

    public override bool Equals(object? obj)
    {
        if (obj is not PostalAddress other) return false;
        return Street == other.Street &&
               City == other.City &&
               State == other.State &&
               PostalCode == other.PostalCode;
    }

    public override int GetHashCode() =>
        HashCode.Combine(Street, City, State, PostalCode);
}
