using AgenticQa.Core.Enums;
using AgenticQa.Core.FailureAnalysis;

namespace AgenticQa.Core.UnitTests.FailureAnalysis;

public class FailureAnalysisResponseValidatorTests
{
    private FailureAnalysisResponseValidator _validator = null!;

    [SetUp]
    public void Setup()
    {
        _validator = new FailureAnalysisResponseValidator();
    }

    [Test]
    public void ValidJson_ParsesSuccessfully()
    {
        const string json =
            """
            {
              "classification": "PRODUCT_DEFECT",
              "confidence": 0.82,
              "summary": "Summary text",
              "evidence": ["E1", "E2"],
              "recommendedAction": "QA_REVIEW_REQUIRED",
              "escalationRequired": true,
              "escalationLevel": "HIGH",
              "escalationReason": "High-confidence product defect should be escalated."
            }
            """;

        var ok = _validator.TryParse("TC-1", json, out var result, out var error);

        Assert.That(ok, Is.True, error);
        Assert.That(result.Classification, Is.EqualTo(FailureClassification.ProductDefect));
        Assert.That(result.Confidence, Is.EqualTo(0.82));
        Assert.That(result.EscalationRequired, Is.True);
        Assert.That(result.EscalationLevel, Is.EqualTo(FailureEscalationLevel.High));
    }

    [Test]
    public void InvalidJson_ReturnsFalse()
    {
        var ok = _validator.TryParse("TC-1", "{ invalid", out _, out var error);

        Assert.That(ok, Is.False);
        Assert.That(error, Does.Contain("not valid JSON"));
    }

    [Test]
    public void InvalidConfidence_ReturnsFalse()
    {
        const string json =
            """
            {
              "classification": "PRODUCT_DEFECT",
              "confidence": 1.5,
              "summary": "Summary text",
              "evidence": ["E1"],
              "recommendedAction": "QA_REVIEW_REQUIRED"
            }
            """;

        var ok = _validator.TryParse("TC-1", json, out _, out var error);
        Assert.That(ok, Is.False);
        Assert.That(error, Does.Contain("between 0 and 1"));
    }

    [Test]
    public void UnsupportedClassification_ReturnsFalse()
    {
        const string json =
            """
            {
              "classification": "SOMETHING_ELSE",
              "confidence": 0.5,
              "summary": "Summary text",
              "evidence": ["E1"],
              "recommendedAction": "QA_REVIEW_REQUIRED"
            }
            """;

        var ok = _validator.TryParse("TC-1", json, out _, out var error);
        Assert.That(ok, Is.False);
        Assert.That(error, Does.Contain("unsupported"));
    }

    [Test]
    public void MissingEscalationFields_AreDerivedFromPolicy()
    {
        const string json =
            """
            {
              "classification": "UNKNOWN",
              "confidence": 0.10,
              "summary": "Not enough evidence",
              "evidence": ["E1"],
              "recommendedAction": "QA_REVIEW_REQUIRED"
            }
            """;

        var ok = _validator.TryParse("TC-1", json, out var result, out var error);

        Assert.That(ok, Is.True, error);
        Assert.That(result.EscalationRequired, Is.True);
        Assert.That(result.EscalationLevel, Is.EqualTo(FailureEscalationLevel.Medium));
    }
}
