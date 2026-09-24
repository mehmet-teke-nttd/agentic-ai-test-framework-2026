using AgenticQa.Core.ElementRegistry;
using AgenticQa.Core.Enums;
using AgenticQa.Core.Execution.Commands;
using AgenticQa.Core.Execution.Runtime;
using AgenticQa.Core.Interfaces;
using AgenticQa.Core.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Playwright;
using NSubstitute;

namespace AgenticQa.Core.UnitTests.Execution.Commands;

public class CommandHandlerTests
{
    [Test]
    public async Task Navigate_KnownPage_ReturnsPassed()
    {
        var page = Substitute.For<IPage>();
        page.GotoAsync("https://example.com/login").Returns(Task.FromResult<IResponse?>(null));
        var context = CreateContext(page);
        var pageRegistry = Substitute.For<IPageRegistry>();
        pageRegistry.GetUrl("LoginPage").Returns("https://example.com/login");
        var handler = new NavigateCommandHandler(NullLogger<NavigateCommandHandler>.Instance, pageRegistry);

        var result = await handler.ExecuteAsync(
            new ExecutionStep { Step = 1, Keyword = ExecutionKeyword.Navigate, Target = "LoginPage" },
            context);

        Assert.That(result.Status, Is.EqualTo(StepStatus.Passed));
        Assert.That(context.CurrentPage, Is.EqualTo("LoginPage"));
    }

    [Test]
    public async Task Navigate_UnknownPage_ReturnsBlocked()
    {
        var context = CreateContext(Substitute.For<IPage>());
        var pageRegistry = Substitute.For<IPageRegistry>();
        pageRegistry.GetUrl("UnknownPage").Returns(_ => throw new PageRegistryException(
            RuntimeExecutionErrorCode.PageNotRegistered,
            "unknown"));
        var handler = new NavigateCommandHandler(NullLogger<NavigateCommandHandler>.Instance, pageRegistry);

        var result = await handler.ExecuteAsync(
            new ExecutionStep { Step = 1, Keyword = ExecutionKeyword.Navigate, Target = "UnknownPage" },
            context);

        Assert.That(result.Status, Is.EqualTo(StepStatus.Blocked));
        Assert.That(result.Error?.ErrorCode, Is.EqualTo("PAGE_NOT_REGISTERED"));
    }

    [Test]
    public async Task Click_ValidTarget_ReturnsPassed()
    {
        var setup = CreateTargetedSetup();
        setup.Locator.ClickAsync().Returns(Task.CompletedTask);
        var handler = new ClickCommandHandler(
            NullLogger<ClickCommandHandler>.Instance,
            setup.RegistryService,
            setup.Resolver,
            setup.Validator);

        var result = await handler.ExecuteAsync(
            new ExecutionStep { Step = 1, Keyword = ExecutionKeyword.Click, Target = "LoginButton" },
            CreateContext(setup.Page));

        Assert.That(result.Status, Is.EqualTo(StepStatus.Passed));
    }

    [Test]
    public async Task Click_MissingTarget_ReturnsBlocked()
    {
        var setup = CreateTargetedSetup();
        setup.RegistryService.GetElement("MissingTarget").Returns(_ =>
            throw new ElementLookupException(LocatorValidationErrorCode.TargetNotRegistered, "missing"));
        var handler = new ClickCommandHandler(
            NullLogger<ClickCommandHandler>.Instance,
            setup.RegistryService,
            setup.Resolver,
            setup.Validator);

        var result = await handler.ExecuteAsync(
            new ExecutionStep { Step = 1, Keyword = ExecutionKeyword.Click, Target = "MissingTarget" },
            CreateContext(setup.Page));

        Assert.That(result.Status, Is.EqualTo(StepStatus.Blocked));
        Assert.That(result.Error?.ErrorCode, Is.EqualTo("TARGET_NOT_REGISTERED"));
    }

    [Test]
    public async Task Fill_LiteralValue_ReturnsPassed()
    {
        var setup = CreateTargetedSetup();
        setup.Locator.FillAsync("literal-user").Returns(Task.CompletedTask);
        var handler = new FillCommandHandler(
            NullLogger<FillCommandHandler>.Instance,
            setup.RegistryService,
            setup.Resolver,
            setup.Validator,
            new TestDataResolver());

        var context = CreateContext(setup.Page);
        var result = await handler.ExecuteAsync(
            new ExecutionStep
            {
                Step = 1,
                Keyword = ExecutionKeyword.Fill,
                Target = "UsernameInput",
                Value = "literal-user"
            },
            context);

        Assert.That(result.Status, Is.EqualTo(StepStatus.Passed));
    }

    [Test]
    public async Task Fill_PlaceholderValue_ReturnsPassed()
    {
        var setup = CreateTargetedSetup();
        setup.Locator.FillAsync("validUser").Returns(Task.CompletedTask);
        var handler = new FillCommandHandler(
            NullLogger<FillCommandHandler>.Instance,
            setup.RegistryService,
            setup.Resolver,
            setup.Validator,
            new TestDataResolver());

        var context = CreateContext(setup.Page, new Dictionary<string, object?> { ["username"] = "validUser" });
        var result = await handler.ExecuteAsync(
            new ExecutionStep
            {
                Step = 1,
                Keyword = ExecutionKeyword.Fill,
                Target = "UsernameInput",
                Value = "{{username}}"
            },
            context);

        Assert.That(result.Status, Is.EqualTo(StepStatus.Passed));
    }

    [Test]
    public async Task Fill_MissingPlaceholder_ReturnsBlocked()
    {
        var setup = CreateTargetedSetup();
        var handler = new FillCommandHandler(
            NullLogger<FillCommandHandler>.Instance,
            setup.RegistryService,
            setup.Resolver,
            setup.Validator,
            new TestDataResolver());

        var context = CreateContext(setup.Page, new Dictionary<string, object?>());
        var result = await handler.ExecuteAsync(
            new ExecutionStep
            {
                Step = 1,
                Keyword = ExecutionKeyword.Fill,
                Target = "UsernameInput",
                Value = "{{username}}"
            },
            context);

        Assert.That(result.Status, Is.EqualTo(StepStatus.Blocked));
        Assert.That(result.Error?.ErrorCode, Is.EqualTo("TEST_DATA_REFERENCE_NOT_FOUND"));
    }

    [Test]
    public async Task Select_ValidOption_ReturnsPassed()
    {
        var setup = CreateTargetedSetup();
        setup.Locator.SelectOptionAsync("US").Returns(Task.FromResult<IReadOnlyList<string>>(["US"]));
        var handler = new SelectCommandHandler(
            NullLogger<SelectCommandHandler>.Instance,
            setup.RegistryService,
            setup.Resolver,
            setup.Validator,
            new TestDataResolver());

        var result = await handler.ExecuteAsync(
            new ExecutionStep
            {
                Step = 1,
                Keyword = ExecutionKeyword.Select,
                Target = "CountrySelect",
                Value = "US"
            },
            CreateContext(setup.Page));

        Assert.That(result.Status, Is.EqualTo(StepStatus.Passed));
    }

    [Test]
    public async Task Check_ValidCheckbox_ReturnsPassed()
    {
        var setup = CreateTargetedSetup();
        setup.Locator.CheckAsync().Returns(Task.CompletedTask);
        var handler = new CheckCommandHandler(
            NullLogger<CheckCommandHandler>.Instance,
            setup.RegistryService,
            setup.Resolver,
            setup.Validator);

        var result = await handler.ExecuteAsync(
            new ExecutionStep
            {
                Step = 1,
                Keyword = ExecutionKeyword.Check,
                Target = "TermsCheckbox"
            },
            CreateContext(setup.Page));

        Assert.That(result.Status, Is.EqualTo(StepStatus.Passed));
    }

    [Test]
    public async Task VerifyVisible_VisibleElement_ReturnsPassed()
    {
        var setup = CreateTargetedSetup();
        var assertionService = Substitute.For<IPlaywrightAssertionService>();
        assertionService.AssertVisibleAsync(setup.Locator).Returns(Task.CompletedTask);
        var handler = new VerifyVisibleCommandHandler(
            NullLogger<VerifyVisibleCommandHandler>.Instance,
            setup.RegistryService,
            setup.Resolver,
            setup.Validator,
            assertionService);

        var result = await handler.ExecuteAsync(
            new ExecutionStep { Step = 1, Keyword = ExecutionKeyword.VerifyVisible, Target = "WelcomeMessage" },
            CreateContext(setup.Page));

        Assert.That(result.Status, Is.EqualTo(StepStatus.Passed));
    }

    [Test]
    public async Task VerifyVisible_AssertionFailure_ReturnsFailed()
    {
        var setup = CreateTargetedSetup();
        var assertionService = Substitute.For<IPlaywrightAssertionService>();
        assertionService.AssertVisibleAsync(setup.Locator).Returns(_ => throw new AssertionFailedException("fail", new Exception()));
        var handler = new VerifyVisibleCommandHandler(
            NullLogger<VerifyVisibleCommandHandler>.Instance,
            setup.RegistryService,
            setup.Resolver,
            setup.Validator,
            assertionService);

        var result = await handler.ExecuteAsync(
            new ExecutionStep { Step = 1, Keyword = ExecutionKeyword.VerifyVisible, Target = "WelcomeMessage" },
            CreateContext(setup.Page));

        Assert.That(result.Status, Is.EqualTo(StepStatus.Failed));
        Assert.That(result.Error?.ErrorCode, Is.EqualTo("ASSERTION_FAILED"));
    }

    [Test]
    public async Task VerifyText_Match_ReturnsPassed()
    {
        var setup = CreateTargetedSetup();
        var assertionService = Substitute.For<IPlaywrightAssertionService>();
        assertionService.AssertTextAsync(setup.Locator, "Welcome").Returns(Task.CompletedTask);
        var handler = new VerifyTextCommandHandler(
            NullLogger<VerifyTextCommandHandler>.Instance,
            setup.RegistryService,
            setup.Resolver,
            setup.Validator,
            assertionService,
            new TestDataResolver());

        var result = await handler.ExecuteAsync(
            new ExecutionStep
            {
                Step = 1,
                Keyword = ExecutionKeyword.VerifyText,
                Target = "WelcomeMessage",
                Expected = "Welcome"
            },
            CreateContext(setup.Page));

        Assert.That(result.Status, Is.EqualTo(StepStatus.Passed));
    }

    [Test]
    public async Task VerifyText_Mismatch_ReturnsFailed()
    {
        var setup = CreateTargetedSetup();
        var assertionService = Substitute.For<IPlaywrightAssertionService>();
        assertionService.AssertTextAsync(setup.Locator, "Welcome")
            .Returns(_ => throw new AssertionFailedException("fail", new Exception()));
        var handler = new VerifyTextCommandHandler(
            NullLogger<VerifyTextCommandHandler>.Instance,
            setup.RegistryService,
            setup.Resolver,
            setup.Validator,
            assertionService,
            new TestDataResolver());

        var result = await handler.ExecuteAsync(
            new ExecutionStep
            {
                Step = 1,
                Keyword = ExecutionKeyword.VerifyText,
                Target = "WelcomeMessage",
                Expected = "Welcome"
            },
            CreateContext(setup.Page));

        Assert.That(result.Status, Is.EqualTo(StepStatus.Failed));
        Assert.That(result.Error?.ErrorCode, Is.EqualTo("ASSERTION_FAILED"));
    }

    [Test]
    public async Task VerifyUrl_Match_ReturnsPassed()
    {
        var page = Substitute.For<IPage>();
        var assertionService = Substitute.For<IPlaywrightAssertionService>();
        assertionService.AssertUrlAsync(page, "/dashboard").Returns(Task.CompletedTask);
        var handler = new VerifyUrlCommandHandler(
            NullLogger<VerifyUrlCommandHandler>.Instance,
            assertionService,
            new TestDataResolver());

        var result = await handler.ExecuteAsync(
            new ExecutionStep { Step = 1, Keyword = ExecutionKeyword.VerifyUrl, Expected = "/dashboard" },
            CreateContext(page));

        Assert.That(result.Status, Is.EqualTo(StepStatus.Passed));
    }

    [Test]
    public async Task VerifyUrl_Mismatch_ReturnsFailed()
    {
        var page = Substitute.For<IPage>();
        var assertionService = Substitute.For<IPlaywrightAssertionService>();
        assertionService.AssertUrlAsync(page, "/dashboard")
            .Returns(_ => throw new AssertionFailedException("fail", new Exception()));
        var handler = new VerifyUrlCommandHandler(
            NullLogger<VerifyUrlCommandHandler>.Instance,
            assertionService,
            new TestDataResolver());

        var result = await handler.ExecuteAsync(
            new ExecutionStep { Step = 1, Keyword = ExecutionKeyword.VerifyUrl, Expected = "/dashboard" },
            CreateContext(page));

        Assert.That(result.Status, Is.EqualTo(StepStatus.Failed));
    }

    [Test]
    public async Task LocatorValidationFailure_ReturnsBlocked()
    {
        var setup = CreateTargetedSetup();
        setup.Validator.ValidateAsync(Arg.Any<ILocator>(), Arg.Any<ElementRegistryEntry>(), Arg.Any<string>())
            .Returns(new LocatorValidationResult
            {
                Status = LocatorValidationStatus.Invalid,
                Target = "LoginButton",
                ExpectedPage = "LoginPage",
                MatchCount = 0,
                Error = new ExecutionError
                {
                    ErrorCode = "LOCATOR_NOT_FOUND",
                    Message = "missing"
                }
            });
        var handler = new ClickCommandHandler(
            NullLogger<ClickCommandHandler>.Instance,
            setup.RegistryService,
            setup.Resolver,
            setup.Validator);

        var result = await handler.ExecuteAsync(
            new ExecutionStep { Step = 1, Keyword = ExecutionKeyword.Click, Target = "LoginButton" },
            CreateContext(setup.Page));

        Assert.That(result.Status, Is.EqualTo(StepStatus.Blocked));
        Assert.That(result.Error?.ErrorCode, Is.EqualTo("LOCATOR_NOT_FOUND"));
    }

    [Test]
    public async Task PlaywrightActionException_ReturnsBlocked()
    {
        var setup = CreateTargetedSetup();
        setup.Locator.ClickAsync().Returns(_ => throw new PlaywrightException("boom"));
        var handler = new ClickCommandHandler(
            NullLogger<ClickCommandHandler>.Instance,
            setup.RegistryService,
            setup.Resolver,
            setup.Validator);

        var result = await handler.ExecuteAsync(
            new ExecutionStep { Step = 1, Keyword = ExecutionKeyword.Click, Target = "LoginButton" },
            CreateContext(setup.Page));

        Assert.That(result.Status, Is.EqualTo(StepStatus.Blocked));
        Assert.That(result.Error?.ErrorCode, Is.EqualTo("PLAYWRIGHT_ACTION_FAILED"));
    }

    private static ExecutionContext CreateContext(IPage page, Dictionary<string, object?>? testData = null) =>
        new()
        {
            Page = page,
            CurrentPage = "LoginPage",
            TestIntent = new TestIntent
            {
                TestId = "TC-001",
                Title = "Login",
                TestType = TestType.UiFunctionalPositive,
                Preconditions = ["User is on login page"],
                TestData = testData ?? new Dictionary<string, object?> { ["username"] = "validUser", ["password"] = "validPassword" },
                Actions = [new TestAction { Step = 1, Action = "Click Login button" }],
                ExpectedResults = ["Dashboard visible"]
            }
        };

    private static TargetedSetup CreateTargetedSetup()
    {
        var page = Substitute.For<IPage>();
        var locator = Substitute.For<ILocator>();
        var registryService = Substitute.For<IElementRegistryService>();
        var resolver = Substitute.For<ILocatorResolver>();
        var validator = Substitute.For<ILocatorValidator>();

        var entry = new ElementRegistryEntry
        {
            Target = "LoginButton",
            Page = "LoginPage",
            LocatorType = LocatorType.GetByRole,
            LocatorValue = "button",
            Name = "Login"
        };

        registryService.GetElement(Arg.Any<string>()).Returns(call =>
        {
            var target = call.Arg<string>();
            return new ElementRegistryEntry
            {
                Target = target,
                Page = "LoginPage",
                LocatorType = LocatorType.GetByRole,
                LocatorValue = "button",
                Name = "Login"
            };
        });
        resolver.Resolve(page, Arg.Any<ElementRegistryEntry>()).Returns(locator);
        validator.ValidateAsync(Arg.Any<ILocator>(), Arg.Any<ElementRegistryEntry>(), Arg.Any<string>())
            .Returns(new LocatorValidationResult
            {
                Status = LocatorValidationStatus.Valid,
                Target = entry.Target,
                ExpectedPage = entry.Page,
                MatchCount = 1,
                Error = null
            });

        return new TargetedSetup(page, locator, registryService, resolver, validator);
    }

    private sealed record TargetedSetup(
        IPage Page,
        ILocator Locator,
        IElementRegistryService RegistryService,
        ILocatorResolver Resolver,
        ILocatorValidator Validator);
}
