namespace CafeRMS.Api.Shared.Errors;

public class ConflictException(string message) : DomainException(message);
