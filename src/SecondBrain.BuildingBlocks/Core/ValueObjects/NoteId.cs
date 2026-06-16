// BuildingBlocks/Core/ValueObjects/NoteId.cs
using System;

namespace SecondBrain.BuildingBlocks.Core.ValueObjects;

public sealed class NoteId : ValueObject
{
    public Guid Value { get; }
    
    private NoteId(Guid value) => Value = value;

    public static NoteId Create() => new(Guid.NewGuid());
    public static NoteId From(Guid value) => new(value);
    public static NoteId FromString(string value) => new(Guid.Parse(value));

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value; 
    }

    public static implicit operator Guid(NoteId id) => id.Value;
    public static explicit operator NoteId(Guid value) => new(value);
    
    public override string ToString() => Value.ToString();
}