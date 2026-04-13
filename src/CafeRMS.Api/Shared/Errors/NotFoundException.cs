namespace CafeRMS.Api.Shared.Errors;

public class NotFoundException(string message) : DomainException(message);
