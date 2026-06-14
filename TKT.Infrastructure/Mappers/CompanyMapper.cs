using TKT.Core.Domain.Entities;
using TKT.Infrastructure.Models;

namespace TKT.Infrastructure.Mappers;

public static class CompanyMapper
{
    public static CompanyRow ToRow(this Company company)
    {
        return new CompanyRow
        {
            CompanyId = company.CompanyId,
            CompanyName = company.Name,
            CompanySlug = company.Slug,
        };
    }
}
