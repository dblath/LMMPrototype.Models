using System.Text.Json.Serialization;
using LMMPrototype.Models.Canonical;
using LMMPrototype.Models.Extensions;

namespace LMMPrototype.Models.Aggregates;

public sealed class CustomerShippingDto
{
    public CustomerRecordDto Customer { get; init; }
    public List<ShipmentRecordDto> Shipments { get; init; }

    // surface delivery access directly
    [JsonIgnore]
    public LocationAccessExtension? DeliveryAccess => Customer.DeliveryAccess;

    [JsonIgnore]
    public PostalAddress Address => Customer.ShippingAddress;

    [JsonIgnore]
    public bool HasChanges =>
        Customer.HasChanges || Shipments.Any(s => s.HasChanges);

    [JsonConstructor]
    public CustomerShippingDto(CustomerRecordDto customer, List<ShipmentRecordDto> shipments)
    {
        Customer = customer ?? throw new ArgumentNullException(nameof(customer));
        Shipments = shipments ?? new List<ShipmentRecordDto>();
    }

    public IEnumerable<ShipmentRecordDto> GetDelivered() =>
        Shipments.Where(s => s.Delivered);

    public IEnumerable<ShipmentRecordDto> GetInTransit() =>
        Shipments.Where(s => !s.Delivered);

    public void MarkShipmentDelivered(string shipmentId)
    {
        var shipment = Shipments.FirstOrDefault(s => s.ShipmentId == shipmentId)
            ?? throw new InvalidOperationException($"Shipment {shipmentId} not found.");
        shipment.MarkDelivered();
    }

    public void UpdateAddress(string street, string city, string state, string postalCode)
    {
        Customer.UpdateShippingAddress(street, city, state, postalCode);
    }

    public void UpdateCustomerName(string newName)
    {
        Customer.ChangeName(newName);
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
        Customer.UpdateDeliveryAccess(
            parentId,
            instructions,
            accessCode,
            windowStart,
            windowEnd,
            contactName,
            contactPhone);
    }

    public void DeactivateAccess() => Customer.DeliveryAccess?.Deactivate();
    public void ActivateAccess() => Customer.DeliveryAccess?.Activate();

    public override string ToString()
    {
        var delivered = GetDelivered().Count();
        var total = Shipments.Count;
        return $"{Customer.Name} ({Customer.Email}) | " +
               $"{delivered}/{total} delivered | Address: {Customer.ShippingAddress} | " +
               $"Access: {Customer.DeliveryAccess?.Instructions ?? "None"}";
    }
}
