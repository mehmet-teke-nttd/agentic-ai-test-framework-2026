using AgenticQa.Core.Enums;
using AgenticQa.Core.Execution.Runtime;
using AgenticQa.Core.Models;
using AgenticQa.Core.Serialization;
using Microsoft.Extensions.Logging.Abstractions;

namespace AgenticQa.Core.UnitTests.Execution.Runtime;

public class JsonTestResultWriterTests
{
    [Test]
    public async Task Writer_CreatesExpectedFile_AndSerializablePayload()
    {
        var outputDir = Path.Combine(Path.GetTempPath(), $"agentic-qa-results-{Guid.NewGuid():N}");
        var writer = new JsonTestResultWriter(outputDir, NullLogger<JsonTestResultWriter>.Instance);

        try
        {
            var result = new TestExecutionResult
            {
                TestId = "TC-001",
                Status = TestStatus.Passed,
                StartedAt = DateTimeOffset.Parse("2026-09-23T20:00:00Z"),
                CompletedAt = DateTimeOffset.Parse("2026-09-23T20:00:02Z"),
                DurationMs = 2000,
                TotalSteps = 1,
                PassedSteps = 1,
                FailedSteps = 0,
                BlockedSteps = 0,
                SkippedSteps = 0,
                StepResults =
                [
                    new StepExecutionResult
                    {
                        TestId = "TC-001",
                        Step = 1,
                        Keyword = ExecutionKeyword.Navigate,
                        Status = StepStatus.Passed,
                        StartedAt = DateTimeOffset.Parse("2026-09-23T20:00:00Z"),
                        DurationMs = 100
                    }
                ]
            };

            var filePath = await writer.WriteAsync(result);
            Assert.That(File.Exists(filePath), Is.True);
            Assert.That(Path.GetFileName(filePath), Is.EqualTo("TC-001-result.json"));

            var payload = await File.ReadAllTextAsync(filePath);
            var roundTrip = AgenticJsonSerializer.Deserialize<TestExecutionResult>(payload);
            Assert.That(roundTrip, Is.Not.Null);
            Assert.That(roundTrip!.TestId, Is.EqualTo("TC-001"));
            Assert.That(roundTrip.Status, Is.EqualTo(TestStatus.Passed));
        }
        finally
        {
            if (Directory.Exists(outputDir))
            {
                Directory.Delete(outputDir, true);
            }
        }
    }
}
