namespace AgenticQa.Core.Models;

public static class ContractSchemaVersions
{
    public const string Legacy = "0.9";
    public const string Current = "1.0";

    public static bool IsSupported(string? schemaVersion) =>
        string.IsNullOrWhiteSpace(schemaVersion)
        || schemaVersion.Equals(Current, StringComparison.OrdinalIgnoreCase)
        || schemaVersion.Equals(Legacy, StringComparison.OrdinalIgnoreCase);
}

public static class ContractSchemaMigration
{
    public static TestIntent Normalize(TestIntent intent)
    {
        ArgumentNullException.ThrowIfNull(intent);
        intent.SchemaVersion = NormalizeVersion(intent.SchemaVersion);
        return intent;
    }

    public static ExecutionContract Normalize(ExecutionContract contract)
    {
        ArgumentNullException.ThrowIfNull(contract);
        contract.SchemaVersion = NormalizeVersion(contract.SchemaVersion);
        return contract;
    }

    private static string NormalizeVersion(string? schemaVersion)
    {
        if (string.IsNullOrWhiteSpace(schemaVersion))
        {
            return ContractSchemaVersions.Current;
        }

        if (schemaVersion.Equals(ContractSchemaVersions.Legacy, StringComparison.OrdinalIgnoreCase))
        {
            return ContractSchemaVersions.Current;
        }

        return schemaVersion;
    }
}
