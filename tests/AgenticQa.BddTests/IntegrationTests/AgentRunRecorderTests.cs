using AgenticQa.BddTests.Support;
using AgenticQa.Core.Enums;
using AgenticQa.Core.Serialization;

namespace AgenticQa.BddTests.IntegrationTests;

public class AgentRunRecorderTests
{
    [Test]
    public async Task SaveAsync_WritesAgentRunJson()
    {
        var outputDirectory = Path.Combine(Path.GetTempPath(), $"agent-runs-{Guid.NewGuid():N}");
        Directory.CreateDirectory(outputDirectory);

        try
        {
            var record = new AgentRunRecord
            {
                RunId = "run-001",
                ScenarioName = "Sample scenario",
                Tags = ["ui", "smoke"],
                TestId = "TC-1",
                TestStatus = TestStatus.Failed,
                StartedAt = DateTimeOffset.UtcNow.AddSeconds(-10),
                CompletedAt = DateTimeOffset.UtcNow,
                Outcome = "FAILED",
                Summary = "Locator failure",
                Confidence = 0.91,
                EscalationRequired = true,
                EscalationLevel = FailureEscalationLevel.High,
                EscalationReason = "High confidence defect.",
                RecommendedAction = "InvestigateAutomation"
            };

            var path = await AgentRunRecorder.SaveAsync(outputDirectory, record);

            Assert.That(File.Exists(path), Is.True);
            var payload = await File.ReadAllTextAsync(path);
            var parsed = AgenticJsonSerializer.Deserialize<AgentRunRecord>(payload);
            Assert.That(parsed, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(parsed!.RunId, Is.EqualTo("run-001"));
                Assert.That(parsed.EscalationRequired, Is.True);
                Assert.That(parsed.EscalationLevel, Is.EqualTo(FailureEscalationLevel.High));
            });
        }
        finally
        {
            if (Directory.Exists(outputDirectory))
            {
                Directory.Delete(outputDirectory, true);
            }
        }
    }
}
