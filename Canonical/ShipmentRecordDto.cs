using LMMPrototype.Models.Extensions;

namespace LMMPrototype.Models.Canonical;

public sealed class ShipmentRecordDto : ICanonicalModel
{
    public string CustomerId { get; init; }
    public string ShipmentId { get; init; }
    public string TrackingNumber { get; private set; }
    public string Carrier { get; private set; }
    public DateTime ShipDate { get; private set; }
    public PostalAddress? DeliveryAddress { get; private set; }
    public LocationAccessExtension? DeliveryAccess { get; private set; }

    public bool Delivered { get; private set; }
    public bool IsActive { get; private set; } = true;

    private readonly List<string> _changedFields = new();
    public bool HasChanges => _changedFields.Count > 0;
    public string ModelVersion => "shipment-record/1.0";

    public ShipmentRecordDto(
        string customerId,
        string shipmentId,
        string trackingNumber,
        string carrier,
        DateTime shipDate,
        PostalAddress? deliveryAddress = null,
        LocationAccessExtension? deliveryAccess = null,
        bool delivered = false)
    {
        if (string.IsNullOrWhiteSpace(customerId))
            throw new ArgumentException("CustomerId required", nameof(customerId));
        if (string.IsNullOrWhiteSpace(shipmentId))
            throw new ArgumentException("ShipmentId required", nameof(shipmentId));
        if (string.IsNullOrWhiteSpace(trackingNumber))
            throw new ArgumentException("Tracking number required", nameof(trackingNumber));
        if (string.IsNullOrWhiteSpace(carrier))
            throw new ArgumentException("Carrier required", nameof(carrier));

        CustomerId = customerId;
        ShipmentId = shipmentId;
        TrackingNumber = trackingNumber;
        Carrier = carrier;
        ShipDate = shipDate;
        Delivered = delivered;

        // assign optional extensions
        DeliveryAddress = deliveryAddress ?? new PostalAddress("", "", "", "");
        DeliveryAccess  = deliveryAccess;
    }

    // --- Mutators ---
    public void UpdateCarrier(string newCarrier)
    {
        if (string.IsNullOrWhiteSpace(newCarrier))
            throw new ArgumentException("Carrier cannot be empty.", nameof(newCarrier));

        if (newCarrier != Carrier)
        {
            Carrier = newCarrier;
            TrackChange(nameof(Carrier));
        }
    }

    public void UpdateDeliveryAddress(string street, string city, string state, string postalCode)
    {
        var newAddr = new PostalAddress(street, city, state, postalCode);
        if (!newAddr.Equals(DeliveryAddress))
        {
            DeliveryAddress = newAddr;
            TrackChange(nameof(DeliveryAddress));
        }
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
        var newAccess = new LocationAccessExtension(parentId);
        newAccess.UpdateInstructions(instructions);
        newAccess.UpdateAccessCode(accessCode);
        newAccess.UpdateWindow(windowStart, windowEnd);
        newAccess.UpdateContact(contactName, contactPhone);

        if (!newAccess.Equals(DeliveryAccess))
        {
            DeliveryAccess = newAccess;
            TrackChange(nameof(DeliveryAccess));
        }
    }

    public void UpdateTracking(string newTracking)
    {
        if (newTracking != TrackingNumber)
        {
            TrackingNumber = newTracking;
            TrackChange(nameof(TrackingNumber));
        }
    }

    public void MarkDelivered()
    {
        if (!Delivered)
        {
            Delivered = true;
            TrackChange(nameof(Delivered));
        }
    }

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

    public void ClearChanges() => _changedFields.Clear();

    private void TrackChange(string field)
    {
        if (!_changedFields.Contains(field))
            _changedFields.Add(field);
    }

    public override string ToString() =>
        $"{ShipmentId} | {Carrier} | {TrackingNumber} | Delivered: {Delivered} | Active: {IsActive}";
}
