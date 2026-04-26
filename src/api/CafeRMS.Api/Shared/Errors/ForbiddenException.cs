namespace CafeRMS.Api.Shared.Errors;

public class ForbiddenException(string message) : DomainException(message);
