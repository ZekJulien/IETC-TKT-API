using System.Data;
using System.Net;
using Dapper;

namespace TKT.Infrastructure.Persistence;

public sealed class IPAddressTypeHandler : SqlMapper.TypeHandler<IPAddress>
{
    public override void SetValue(IDbDataParameter parameter, IPAddress? value)
        => parameter.Value = (object?)value ?? DBNull.Value;

    public override IPAddress? Parse(object value)
        => value as IPAddress ?? IPAddress.Parse(value.ToString()!);
}
