using System;
using System.Collections.Generic;
using System.Linq;
using LMMPrototype.Models.Canonical;

namespace LMMPrototype.Models.Aggregates;

public sealed class CustomerBillingDto
{
    public CustomerRecordDto Customer { get; }
    public List<BillingRecordDto> BillingRecords { get; }

    // domain alias: the address used by this domain
    public PostalAddress Address => Customer.BillingAddress;

    public bool HasChanges =>
        Customer.HasChanges || BillingRecords.Any(b => b.HasChanges);

    public CustomerBillingDto(CustomerRecordDto customer, IEnumerable<BillingRecordDto> billingRecords)
    {
        Customer = customer ?? throw new ArgumentNullException(nameof(customer));
        BillingRecords = billingRecords?.ToList()
            ?? throw new ArgumentNullException(nameof(billingRecords));

        Validate();
    }

    // --- Accessors / Computed values ---
    public decimal GetOpenBalance() =>
        BillingRecords.Where(b => b.IsActive).Sum(b => b.Balance);

    public IEnumerable<BillingRecordDto> GetActiveInvoices() =>
        BillingRecords.Where(b => b.IsActive);

    public IEnumerable<BillingRecordDto> GetInactiveInvoices() =>
        BillingRecords.Where(b => !b.IsActive);

    public BillingRecordDto? GetInvoice(string invoiceId) =>
        BillingRecords.FirstOrDefault(b => b.InvoiceId == invoiceId);

    // --- Modification helpers ---
    public void UpdateAddress(string street, string city, string state, string postalCode)
    {
        Customer.UpdateBillingAddress(street, city, state, postalCode);
    }


    public void ApplyPayment(string invoiceId, decimal amount)
    {
        var record = GetInvoice(invoiceId)
            ?? throw new InvalidOperationException($"Invoice {invoiceId} not found.");

        if (amount <= 0)
            throw new ArgumentException("Payment amount must be positive.");

        var newBalance = Math.Max(0, record.Balance - amount);
        record.UpdateBalance(newBalance);

        if (newBalance == 0)
            record.Deactivate();
    }

    public void ReactivateInvoice(string invoiceId)
    {
        var record = GetInvoice(invoiceId)
            ?? throw new InvalidOperationException($"Invoice {invoiceId} not found.");
        record.Activate();
    }

    // --- Validation ---
    public void Validate()
    {
        Customer.ValidateAddresses();
        foreach (var record in BillingRecords)
        {
            if (string.IsNullOrWhiteSpace(record.CustomerId))
                throw new InvalidOperationException("Billing record missing CustomerId.");
            if (record.CustomerId != Customer.Id)
                throw new InvalidOperationException($"Billing record {record.InvoiceId} does not belong to customer {Customer.Id}.");
        }
    }

    // --- Summary output ---
    public override string ToString()
    {
        var total = GetOpenBalance();
        return $"{Customer.Name} ({Customer.Email}) | " +
               $"{BillingRecords.Count} invoices, open balance {total:C}";
    }
}
