using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Tests.Shared;

public static class ProblemDetailsAssertions
{
    public static async Task<ProblemDetails> ShouldBeProblemDetailsAsync(
        this HttpResponseMessage response,
        HttpStatusCode expectedStatus)
    {
        response.StatusCode.Should().Be(expectedStatus);
        var body = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        body.Should().NotBeNull();
        body!.Status.Should().Be((int)expectedStatus);
        return body;
    }

    public static async Task ShouldBeProblemDetailsWithDetailAsync(
        this HttpResponseMessage response,
        HttpStatusCode expectedStatus,
        string expectedDetail)
    {
        var body = await response.ShouldBeProblemDetailsAsync(expectedStatus);
        body.Detail.Should().Be(expectedDetail);
    }

    public static async Task ShouldBeProblemDetailsContainingAsync(
        this HttpResponseMessage response,
        HttpStatusCode expectedStatus,
        string expectedSubstring)
    {
        var body = await response.ShouldBeProblemDetailsAsync(expectedStatus);
        body.Detail.Should().Contain(expectedSubstring);
    }
}
