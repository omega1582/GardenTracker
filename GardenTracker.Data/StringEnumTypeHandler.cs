using Dapper;
using System.Data;

namespace GardenTracker.Data;

public class StringEnumTypeHandler<T> : SqlMapper.TypeHandler<T> where T : struct, Enum
{
    public override void SetValue(IDbDataParameter parameter, T value) =>
        parameter.Value = value.ToString();

    public override T Parse(object value) =>
        Enum.Parse<T>((string)value);
}
