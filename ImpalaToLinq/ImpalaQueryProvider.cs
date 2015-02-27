using System;
using System.Data.Common;
using System.Linq.Expressions;
using System.Reflection;
using ImpalaToLinq.ImpalaToLinq;
using ImpalaToLinq.Translator;

namespace ImpalaToLinq
{

  public class ImpalaQueryProvider : QueryProvider
  {
    private readonly DbConnection _connection;

    public ImpalaQueryProvider(DbConnection connection)
    {
      _connection = connection;
    }

    public override string GetQuery(Expression expression)
    {
      return new ImpalaQueryTranslator().Translate(expression);
    }

    public override object Execute(Expression expression)
    {
      var dbCommand = _connection.CreateCommand();
      dbCommand.CommandText = GetQuery(expression);
      var reader = dbCommand.ExecuteReader();
      var elementType = TypeSystem.GetElementType(expression.Type);

      return Activator.CreateInstance(
            typeof(ObjectReader<>).MakeGenericType(elementType),
            BindingFlags.Instance | BindingFlags.NonPublic, null,
            new object[] { reader },
            null);
    }
  }
}
