namespace CafeRMS.Api.Shared.Errors;

public class DomainException(string message) : Exception(message);
