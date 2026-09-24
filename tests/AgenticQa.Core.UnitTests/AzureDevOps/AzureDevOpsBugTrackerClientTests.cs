using System.Net;
using System.Text;
using AgenticQa.Core.AzureDevOps;
using AgenticQa.Core.Enums;
using AgenticQa.Core.Interfaces;
using AgenticQa.Core.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace AgenticQa.Core.UnitTests.AzureDevOps;

public class AzureDevOpsBugTrackerClientTests
{
    [Test]
    public async Task DryRun_DoesNotCallCreateApi()
    {
        var handler = new StubHandler(_ => throw new Exception("Should not be called"));
        var clientFactory = Substitute.For<IHttpClientFactory>();
        clientFactory.CreateClient("AzureDevOps").Returns(new HttpClient(handler));

        var tracker = CreateClient(clientFactory, dryRun: true);
        var result = await tracker.CreateBugAsync(CreateApprovedDraft());

        Assert.That(result.Status, Is.EqualTo(BugCreationStatus.ReadyForCreation));
    }

    [Test]
    public async Task SuccessfulApiResponse_ReturnsCreated()
    {
        var responseJson = """{"id":12345,"url":"https://dev.azure.com/myorg/MyProject/_workitems/edit/12345"}""";
        var handler = new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
        });
        var clientFactory = Substitute.For<IHttpClientFactory>();
        clientFactory.CreateClient("AzureDevOps").Returns(new HttpClient(handler));

        Environment.SetEnvironmentVariable("AZURE_DEVOPS_PAT_TEST", "pat");
        var tracker = CreateClient(clientFactory, dryRun: false);
        var result = await tracker.CreateBugAsync(CreateApprovedDraft());

        Assert.That(result.Status, Is.EqualTo(BugCreationStatus.Created));
        Assert.That(result.ExternalId, Is.EqualTo("12345"));
    }

    [Test]
    public async Task FailedApiResponse_ReturnsFailed()
    {
        var handler = new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.BadRequest));
        var clientFactory = Substitute.For<IHttpClientFactory>();
        clientFactory.CreateClient("AzureDevOps").Returns(new HttpClient(handler));

        Environment.SetEnvironmentVariable("AZURE_DEVOPS_PAT_TEST", "pat");
        var tracker = CreateClient(clientFactory, dryRun: false);
        var result = await tracker.CreateBugAsync(CreateApprovedDraft());

        Assert.That(result.Status, Is.EqualTo(BugCreationStatus.Failed));
        Assert.That(result.Error?.ErrorCode, Is.EqualTo("AZURE_DEVOPS_REQUEST_FAILED"));
    }

    [Test]
    public async Task InvalidApiResponse_ReturnsFailed()
    {
        var handler = new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""{"url":"missing-id"}""", Encoding.UTF8, "application/json")
        });
        var clientFactory = Substitute.For<IHttpClientFactory>();
        clientFactory.CreateClient("AzureDevOps").Returns(new HttpClient(handler));

        Environment.SetEnvironmentVariable("AZURE_DEVOPS_PAT_TEST", "pat");
        var tracker = CreateClient(clientFactory, dryRun: false);
        var result = await tracker.CreateBugAsync(CreateApprovedDraft());

        Assert.That(result.Status, Is.EqualTo(BugCreationStatus.Failed));
        Assert.That(result.Error?.ErrorCode, Is.EqualTo("AZURE_DEVOPS_RESPONSE_INVALID"));
    }

    [Test]
    public async Task MissingPat_ReturnsAuthenticationError_WithoutTokenLeak()
    {
        Environment.SetEnvironmentVariable("AZURE_DEVOPS_PAT_TEST", null);
        var clientFactory = Substitute.For<IHttpClientFactory>();
        clientFactory.CreateClient("AzureDevOps").Returns(new HttpClient(new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.OK))));
        var tracker = CreateClient(clientFactory, dryRun: false);

        var result = await tracker.CreateBugAsync(CreateApprovedDraft());

        Assert.That(result.Status, Is.EqualTo(BugCreationStatus.Failed));
        Assert.That(result.Error?.ErrorCode, Is.EqualTo("AZURE_DEVOPS_AUTHENTICATION_FAILED"));
        Assert.That(result.Message, Does.Not.Contain("pat"));
    }

    [Test]
    public async Task InvalidProjectConfig_ReturnsConfigurationOrMappingFailure()
    {
        var options = Options.Create(new AzureDevOpsOptions
        {
            OrganizationUrl = "https://dev.azure.com/myorg",
            Project = "",
            ApiVersion = "7.1",
            PersonalAccessTokenEnvironmentVariable = "AZURE_DEVOPS_PAT_TEST",
            DryRun = false
        });
        var severityMapper = new AzureDevOpsSeverityMapper(options);
        var priorityMapper = new AzureDevOpsPriorityMapper(options);
        var requestBuilder = new AzureDevOpsBugRequestBuilder(options, severityMapper, priorityMapper);
        var auth = new AzureDevOpsAuthenticationProvider(options);

        var clientFactory = Substitute.For<IHttpClientFactory>();
        clientFactory.CreateClient("AzureDevOps").Returns(new HttpClient(new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.OK))));
        var tracker = new AzureDevOpsBugTrackerClient(
            NullLogger<AzureDevOpsBugTrackerClient>.Instance,
            clientFactory,
            auth,
            requestBuilder,
            options);

        var result = await tracker.CreateBugAsync(CreateApprovedDraft());

        Assert.That(result.Status, Is.EqualTo(BugCreationStatus.Failed));
        Assert.That(result.Error?.ErrorCode, Is.EqualTo("AZURE_DEVOPS_FIELD_MAPPING_FAILED"));
    }

    private static AzureDevOpsBugTrackerClient CreateClient(IHttpClientFactory clientFactory, bool dryRun)
    {
        var options = Options.Create(new AzureDevOpsOptions
        {
            OrganizationUrl = "https://dev.azure.com/myorg",
            Project = "MyProject",
            ApiVersion = "7.1",
            PersonalAccessTokenEnvironmentVariable = "AZURE_DEVOPS_PAT_TEST",
            DryRun = dryRun,
            SeverityMapping = new Dictionary<string, string>
            {
                ["LOW"] = "4 - Low",
                ["MEDIUM"] = "3 - Medium",
                ["HIGH"] = "2 - High",
                ["CRITICAL"] = "1 - Critical"
            },
            PriorityMapping = new Dictionary<string, int>
            {
                ["LOW"] = 3,
                ["MEDIUM"] = 2,
                ["HIGH"] = 1
            },
            DefaultPriority = 2
        });

        var severityMapper = new AzureDevOpsSeverityMapper(options);
        var priorityMapper = new AzureDevOpsPriorityMapper(options);
        var requestBuilder = new AzureDevOpsBugRequestBuilder(options, severityMapper, priorityMapper);
        var auth = new AzureDevOpsAuthenticationProvider(options);

        return new AzureDevOpsBugTrackerClient(
            NullLogger<AzureDevOpsBugTrackerClient>.Instance,
            clientFactory,
            auth,
            requestBuilder,
            options);
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
                Comment = "approved",
                ReviewedAt = DateTimeOffset.UtcNow
            },
            Draft = new BugDraft
            {
                TestId = "TC-1",
                Title = "Title",
                Summary = "Summary",
                StepsToReproduce = ["Step1"],
                ExpectedResult = "Expected",
                ActualResult = "Actual",
                Severity = BugSeverity.Medium,
                Priority = BugPriority.Medium,
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
