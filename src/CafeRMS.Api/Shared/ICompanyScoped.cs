namespace CafeRMS.Api.Shared;

public interface ICompanyScoped
{
    Guid CompanyId { get; }
}
