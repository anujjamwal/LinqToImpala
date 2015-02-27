using System.Linq;
using System.Reflection;

namespace ImpalaToLinq
{
  class ModelUtils
  {
    public static string ColumnNameForField(MemberInfo member) {
      return member.Name;
    }

    public static string TableName(IQueryable queryable) {
      return queryable.ElementType.Name;
    }
  }
}
