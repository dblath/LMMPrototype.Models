using System;
using System.Collections.Generic;
using System.Linq;
using LMMPrototype.Models.Extensions;

namespace LMMPrototype.Models.Canonical;

public sealed class CustomerRecordDto : ICanonicalModel
{
    // --- Meta ---
    public bool IsActive { get; private set; } = true;
    // --- Properties ---
    public string Id { get; init; }
    public string Name { get; private set; }
    public string Email { get; private set; }
    public PostalAddress BillingAddress { get; private set; }
    public PostalAddress ShippingAddress { get; private set; }
    public LocationAccessExtension? DeliveryAccess { get; private set; }
    public string ModelVersion => "customer-record/1.0";


    // --- Change tracking ---
    private readonly List<string> _changedFields = new();
    public bool HasChanges => _changedFields.Count > 0;
    public IReadOnlyList<string> GetChanges() => _changedFields.AsReadOnly();
    public void ClearChanges() => _changedFields.Clear();

    // --- Constructors ---
    // for JSON deserialization or creation when no ID is provided
    public CustomerRecordDto()
    {
        Id = $"C{DateTime.UtcNow.Ticks % 10000:D3}";
        Name = string.Empty;
        Email = string.Empty;

        // provide minimal valid defaults
        BillingAddress = new PostalAddress("TBD", "TBD", "NA", "00000");
        ShippingAddress = new PostalAddress("TBD", "TBD", "NA", "00000");

        DeliveryAccess = new LocationAccessExtension(Id);
    }
    // Primary constructor
    [System.Text.Json.Serialization.JsonConstructor] 
    public CustomerRecordDto(
        string id,
        string name,
        string email,
        PostalAddress billingAddress,
        PostalAddress shippingAddress,
        LocationAccessExtension deliveryAccess)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Id cannot be null or empty.");
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be null or empty.");
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be null or empty.");

        Id = id;
        Name = name;
        Email = email;
        BillingAddress = billingAddress ?? throw new ArgumentNullException(nameof(billingAddress));
        ShippingAddress = shippingAddress ?? throw new ArgumentNullException(nameof(shippingAddress));
        DeliveryAccess = deliveryAccess ?? throw new ArgumentNullException(nameof(deliveryAccess));

        ValidateAddresses();
    }

    // --- Mutators ---
    public void ChangeName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Name cannot be empty.");
        if (newName != Name)
        {
            Name = newName;
            TrackChange(nameof(Name));
        }
    }

    public void ChangeEmail(string newEmail)
    {
        if (string.IsNullOrWhiteSpace(newEmail))
            throw new ArgumentException("Email cannot be empty.");
        if (newEmail != Email)
        {
            Email = newEmail;
            TrackChange(nameof(Email));
        }
    }

    public void UpdateBillingAddress(string street, string city, string state, string postalCode)
    {
        var newAddr = new PostalAddress(street, city, state, postalCode); // self-validates
        if (!newAddr.Equals(BillingAddress))
        {
            BillingAddress = newAddr;
            TrackChange(nameof(BillingAddress));
        }
    }

    public void UpdateShippingAddress(string street, string city, string state, string postalCode)
    {
        var newAddr = new PostalAddress(street, city, state, postalCode); // self-validates
        if (!newAddr.Equals(ShippingAddress))
        {
            ShippingAddress = newAddr;
            TrackChange(nameof(ShippingAddress));
        }
    }

    public void AttachDeliveryAccess(LocationAccessExtension? access)
    {
        DeliveryAccess = access;
    }

    public void UpdateDeliveryAccess(
        string parentId,
        string? instructions,
        string? accessCode,
        string? windowStart,
        string? windowEnd,
        string? contactName,
        string? contactPhone)
    {
        // Create a new candidate instance for comparison
        var newAccess = new LocationAccessExtension(parentId);
        newAccess.UpdateInstructions(instructions);
        newAccess.UpdateAccessCode(accessCode);
        newAccess.UpdateWindow(windowStart, windowEnd);
        newAccess.UpdateContact(contactName, contactPhone);

        // Only replace and track if data actually changed
        if (DeliveryAccess == null || !DeliveryAccess.Equals(newAccess))
        {
            DeliveryAccess = newAccess;
            TrackChange(nameof(DeliveryAccess));
        }
    }

    // --- Helper methods ---
    private void TrackChange(string field)
    {
        if (!_changedFields.Contains(field))
            _changedFields.Add(field);
    }

    public void ValidateAddresses()
    {
        if (BillingAddress == null)
            throw new InvalidOperationException("Billing address missing.");
        if (ShippingAddress == null)
            throw new InvalidOperationException("Shipping address missing.");
        BillingAddress.Validate();
        ShippingAddress.Validate();
    }

     // --- Lifecycle control ---
    public void Deactivate()
    {
        if (IsActive)
        {
            IsActive = false;
            TrackChange(nameof(IsActive));
        }
    }

    public void Activate()
    {
        if (!IsActive)
        {
            IsActive = true;
            TrackChange(nameof(IsActive));
        }
    }


    public override string ToString() =>
        $"{Name} ({Email}) | Billing: {BillingAddress} | Shipping: {ShippingAddress}";
}
