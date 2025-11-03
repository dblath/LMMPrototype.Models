using System;
using System.Collections.Generic;

namespace LMMPrototype.Models.Canonical;

public sealed class BillingRecordDto : ICanonicalModel
{
    // --- Meta ---
    public bool IsActive { get; private set; } = true;
    public string ModelVersion => "billing-record/1.0";

    // --- Properties ---
    public string CustomerId { get; init; }
    public string InvoiceId { get; init; }
    public decimal Balance { get; private set; }

    // --- Change tracking ---
    private readonly List<string> _changedFields = new();
    public bool HasChanges => _changedFields.Count > 0;
    public IReadOnlyList<string> GetChanges() => _changedFields.AsReadOnly();
    public void ClearChanges() => _changedFields.Clear();

    // --- Constructors ---
    public BillingRecordDto()
    {
        CustomerId = "C000";
        InvoiceId = $"INV-{DateTime.UtcNow.Ticks % 10000:D4}";
        Balance = 0m;
    }

    public BillingRecordDto(
        string customerId,
        string invoiceId,
        decimal balance,
        bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(customerId))
            throw new ArgumentException("CustomerId cannot be null or empty.");
        if (string.IsNullOrWhiteSpace(invoiceId))
            throw new ArgumentException("InvoiceId cannot be null or empty.");

        CustomerId = customerId;
        InvoiceId = invoiceId;
        Balance = balance;
        IsActive = isActive;
    }

    // --- Behavior ---
    public void UpdateBalance(decimal newBalance)
    {
        if (newBalance < 0)
            throw new ArgumentException("Balance cannot be negative.");

        if (newBalance != Balance)
        {
            Balance = newBalance;
            TrackChange(nameof(Balance));
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

    // --- Helpers ---
    private void TrackChange(string field)
    {
        if (!_changedFields.Contains(field))
            _changedFields.Add(field);
    }

    public override string ToString()
        => $"{InvoiceId} | Customer: {CustomerId} | Balance: {Balance:C} | Active: {IsActive}";
}
