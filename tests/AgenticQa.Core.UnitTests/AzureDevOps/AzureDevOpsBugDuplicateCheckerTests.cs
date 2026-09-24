using System.Net;
using System.Text;
using AgenticQa.Core.AzureDevOps;
using AgenticQa.Core.Enums;
using AgenticQa.Core.Interfaces;
using AgenticQa.Core.Models;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace AgenticQa.Core.UnitTests.AzureDevOps;

public class AzureDevOpsBugDuplicateCheckerTests
{
    [Test]
    public async Task DuplicateFound_ReturnsDuplicateMatch()
    {
        var payload = """{"workItems":[{"id":123,"url":"https://dev.azure.com/myorg/_workitems/edit/123"}]}""";
        var handler = new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json")
        });
        var factory = Substitute.For<IHttpClientFactory>();
        factory.CreateClient("AzureDevOps").Returns(new HttpClient(handler));
        var auth = Substitute.For<IAzureDevOpsAuthenticationProvider>();
        auth.TryCreateAuthorizationHeader(out Arg.Any<string>(), out Arg.Any<string>())
            .Returns(call =>
            {
                call[0] = "Basic xxx";
                call[1] = string.Empty;
                return true;
            });

        var checker = new AzureDevOpsBugDuplicateChecker(
            factory,
            auth,
            Options.Create(new AzureDevOpsOptions
            {
                OrganizationUrl = "https://dev.azure.com/myorg",
                Project = "MyProject",
                ApiVersion = "7.1"
            }));

        var match = await checker.FindDuplicateAsync(CreateApprovedDraft());

        Assert.That(match.IsDuplicate, Is.True);
        Assert.That(match.ExistingBugId, Is.EqualTo("123"));
    }

    private static ApprovedBugDraft CreateApprovedDraft() =>
        new()
        {
            TestId = "TC-1",
            CreationKey = "TC-1:key",
            FailureClassification = FailureClassification.ProductDefect,
            Review = new BugDraftReview
            {
                TestId = "TC-1",
                Decision = BugDraftReviewDecision.Approved,
                ReviewedBy = "QA",
                Comment = "ok",
                ReviewedAt = DateTimeOffset.UtcNow
            },
            Draft = new BugDraft
            {
                TestId = "TC-1",
                Title = "Bug title",
                Summary = "Summary",
                ExpectedResult = "Expected",
                ActualResult = "Actual",
                Environment = "UI"
            }
        };

    private sealed class StubHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;

        public StubHandler(Func<HttpRequestMessage, HttpResponseMessage> handler)
        {
            _handler = handler;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(_handler(request));
    }
}
