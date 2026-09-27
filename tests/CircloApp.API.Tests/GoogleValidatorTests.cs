using CircloApp.Application.Common.Interfaces;
using CircloApp.Infrastructure.Authentication;
using Microsoft.Extensions.Options;
namespace CircloApp.API.Tests.Authentication;
public class GoogleValidatorTests
{
    private static IGoogleTokenValidator Validator() => (IGoogleTokenValidator)Activator.CreateInstance(
        typeof(JwtTokenGenerator).Assembly.GetType("CircloApp.Infrastructure.Authentication.GoogleTokenValidator")!,
        Options.Create(new GoogleAuthOptions { ClientId="test.apps.googleusercontent.com" }))!;
    [Theory] [InlineData(null)] [InlineData("")] [InlineData("  ")]
    public async Task Missing_tokens_return_null_without_network(string? token) =>
        Assert.Null(await Validator().ValidateAsync(token!));
    [Fact] public async Task Nonempty_tokens_reach_validation_instead_of_being_rejected_by_the_empty_guard()
    {
        using var cts=new CancellationTokenSource();cts.Cancel();
        await Assert.ThrowsAsync<OperationCanceledException>(()=>Validator().ValidateAsync("nonempty-token",cts.Token));
    }
}
