using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Tests.Shared;

// Helpers for asserting the ProblemDetails-shaped error body the API emits via
// AddProblemDetails() + GlobalExceptionHandler. Pull-as-needed: tests that only
// check the status code keep using `response.StatusCode.Should().Be(...)`.
// Reach for these when the test cares about the Detail / Title / extension fields.
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
