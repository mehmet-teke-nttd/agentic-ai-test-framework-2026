namespace AgenticQa.Core.Interfaces;

public interface IAiProvider
{
    Task<string> GenerateAsync(
        string systemPrompt,
        string userPrompt,
        CancellationToken cancellationToken = default);
}
