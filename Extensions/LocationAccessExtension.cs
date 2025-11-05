using LMMPrototype.Models.Canonical;
using System.Text.Json.Serialization;

namespace LMMPrototype.Models.Extensions;

public sealed class LocationAccessExtension : ICanonicalModel
{
    // --- Meta ---
    public bool IsActive { get; private set; } = true;
    public bool HasChanges { get; private set; } = false;
    public string ModelVersion => "location-access-extension/1.0";

    // --- Properties ---
    public string ParentId { get; }
    public string? Instructions { get; private set; }
    public string? AccessCode { get; private set; }
    public string? WindowStart { get; private set; }
    public string? WindowEnd { get; private set; }
    public string? ContactName { get; private set; }
    public string? ContactPhone { get; private set; }

    // --- Constructor ---
    public LocationAccessExtension(string? parentId = null)
    {
        ParentId = string.IsNullOrWhiteSpace(parentId)
            ? "unassigned"   // temporary until assembler sets it
            : parentId;

        IsActive = true;
        HasChanges = false;
    }

    // JSON constructor to round-trip state over transport
    [JsonConstructor]
    public LocationAccessExtension(
        string parentId,
        string? instructions,
        string? accessCode,
        string? windowStart,
        string? windowEnd,
        string? contactName,
        string? contactPhone,
        bool isActive)
    {
        ParentId = string.IsNullOrWhiteSpace(parentId) ? "unassigned" : parentId;

        // Apply full state using existing mutation helpers
        WithState(
            instructions,
            accessCode,
            windowStart,
            windowEnd,
            contactName,
            contactPhone,
            isActive);
    }

    // --- Mutators ---
    public void UpdateInstructions(string? newInstructions)
    {
        if (newInstructions != Instructions)
        {
            Instructions = newInstructions;
            HasChanges = true;
        }
    }

    public void UpdateAccessCode(string? newCode)
    {
        if (newCode != AccessCode)
        {
            AccessCode = newCode;
            HasChanges = true;
        }
    }

    public void UpdateWindow(string? start, string? end)
    {
        if (start != WindowStart || end != WindowEnd)
        {
            WindowStart = start;
            WindowEnd = end;
            HasChanges = true;
        }
    }

    public void UpdateContact(string? name, string? phone)
    {
        if (name != ContactName || phone != ContactPhone)
        {
            ContactName = name;
            ContactPhone = phone;
            HasChanges = true;
        }
    }

    // --- Lifecycle control ---
    public void Deactivate()
    {
        if (IsActive)
        {
            IsActive = false;
            HasChanges = true;
        }
    }

    public void Activate()
    {
        if (!IsActive)
        {
            IsActive = true;
            HasChanges = true;
        }
    }

    public void ClearChanges() => HasChanges = false;


    public LocationAccessExtension WithState(
        string? instructions,
        string? accessCode,
        string? windowStart,
        string? windowEnd,
        string? contactName,
        string? contactPhone,
        bool isActive)
    {
        UpdateInstructions(instructions);
        UpdateAccessCode(accessCode);
        UpdateWindow(windowStart, windowEnd);
        UpdateContact(contactName, contactPhone);

        if (!isActive)
            Deactivate();

        ClearChanges();
        return this;
    }

    // --- Equality helpers ---
    public override bool Equals(object? obj)
    {
        if (obj is not LocationAccessExtension other) return false;
        return string.Equals(ParentId, other.ParentId, StringComparison.OrdinalIgnoreCase)
            && string.Equals(Instructions, other.Instructions)
            && string.Equals(AccessCode, other.AccessCode)
            && string.Equals(WindowStart, other.WindowStart)
            && string.Equals(WindowEnd, other.WindowEnd)
            && string.Equals(ContactName, other.ContactName)
            && string.Equals(ContactPhone, other.ContactPhone)
            && IsActive == other.IsActive;
    }

    public override int GetHashCode() =>
        HashCode.Combine(
            ParentId?.ToLowerInvariant(),
            Instructions,
            AccessCode,
            WindowStart,
            WindowEnd,
            ContactName,
            ContactPhone,
            IsActive);

    public override string ToString() =>
        $"Instructions: \"{Instructions}\" | Code: \"{AccessCode}\" | Active: {IsActive}";
}
