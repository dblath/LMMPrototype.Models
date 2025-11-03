namespace LMMPrototype.Models.Canonical;

public static class CanonicalVersionRegistry
{
    private static readonly Dictionary<string, string> CurrentVersions = new()
    {
        { "customer-record", "1.0" },
        { "shipment-record", "1.0" },
        { "billing-record", "1.0" },
        { "location-access-extension", "1.0" }
    };

    public static bool IsCurrent(ICanonicalModel model)
    {
        var parts = model.ModelVersion.Split('/');
        var key = parts[0];
        var version = parts.Length > 1 ? parts[1] : "1.0";
        return CurrentVersions.TryGetValue(key, out var cur) && cur == version;
    }

    public static void EnsureCurrent(ICanonicalModel model)
    {
        if (!IsCurrent(model))
            throw new InvalidOperationException(
                $"Version mismatch for {model.GetType().Name}: expected {model.ModelVersion}, current is {GetCurrent(model)}");
    }

    public static string GetCurrent(ICanonicalModel model)
    {
        var key = model.ModelVersion.Split('/')[0];
        return CurrentVersions.TryGetValue(key, out var cur) ? cur : "unknown";
    }
}
