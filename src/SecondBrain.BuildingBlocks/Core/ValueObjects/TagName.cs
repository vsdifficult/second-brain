namespace SecondBrain.BuildingBlocks.Core.ValueObjects;

public sealed class TagName : ValueObject
{
    public string Value { get; }
    
    private TagName(string value) => Value = value;

    public static TagName Create(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentException("Tag cannot be empty", nameof(input));
            
        var normalized = input
            .Trim()
            .ToLowerInvariant()
            .Replace(' ', '_')
            .Replace("#", "sharp")
            .Replace("+", "plus");
            
        if (normalized.Length > 50)
            normalized = normalized[..50];
            
        return new TagName(normalized);
    }

    public static TagName FromString(string value) => new(value);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
    
    public override string ToString() => Value;
}