using CafeRMS.Api.Features.Companies;

namespace CafeRMS.Api.Shared.Entities;

public abstract class CompanyOwnedEntity : Entity, ICompanyOwned
{
    public Guid CompanyId { get; protected init; }
    public Company? Company { get; private init; }
}
