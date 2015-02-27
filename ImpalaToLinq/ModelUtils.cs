using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;

namespace ImpalaToLinq
{
  class ModelUtils
  {
    public static string ColumnNameForField(MemberInfo member) {
      var columnAttribute = member.GetCustomAttribute<ColumnAttribute>();
      return columnAttribute != null && columnAttribute.Name != null ? columnAttribute.Name : member.Name ;
    }

    public static string TableName(IQueryable queryable) {
      var tableAttribute = queryable.ElementType.GetCustomAttribute<TableAttribute>();
      return tableAttribute != null && tableAttribute.Name != null ? tableAttribute.Name : queryable.ElementType.Name;
    }
  }
}
