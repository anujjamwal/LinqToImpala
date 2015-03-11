using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace ImpalaToLinq.Translator
{
  internal abstract class SqlExpression: Expression {
    protected abstract new SqlExpressionType Type { get; }
  }
  internal enum SqlExpressionType {
    Table,
    Where,
    Query,
    OrderBy
  }

  internal enum Order {
    Asc, Desc
  }

  internal class TableExpression : SqlExpression
  {
    private IQueryable _table;

    public TableExpression(IQueryable table, string alias) {
      _table = table;
      Table = ModelUtils.TableName(_table);
      Alias = alias;
    }

    public string Alias { get; private set; }
    public string Table { get; private set; }

    protected override SqlExpressionType Type { get { return SqlExpressionType.Table; } }
  }

  internal class WhereExpression : SqlExpression
  {
    public LambdaExpression Expression { get; private set; }

    public WhereExpression(LambdaExpression expression) {
      Expression = expression;
    }

    protected override SqlExpressionType Type { get { return SqlExpressionType.Where; } }
  }

  internal class OrderByExpression : SqlExpression
  {
    public LambdaExpression Expression { get; private set; }
    public Order Order { get; private set; }

    public OrderByExpression(LambdaExpression expression, Order order) {
      Expression = expression;
      Order = order;
    }

    protected override SqlExpressionType Type { get { return SqlExpressionType.OrderBy; } }
  }

  internal class QueryExpression: SqlExpression
  {
    public QueryExpression(string alias) {
      Alias = alias;
      OrderBy = new List<OrderByExpression>();
    }

    public void AddTableExpression(IQueryable table)
    {
      Table = new TableExpression(table, Alias);
    }

    public void AddWhereExpression(LambdaExpression expression)
    {
      Where = new WhereExpression(expression);
    }

    public void AddOrderByExpression(LambdaExpression expression)
    {
      OrderBy.Add(new OrderByExpression(expression, Order.Asc));
    }
    public void AddOrderByExpressionDescending(LambdaExpression expression)
    {
      OrderBy.Add(new OrderByExpression(expression, Order.Desc));
    }


    public string Alias { get; private set; }
    public SqlExpression Table { get; private set; }
    public WhereExpression Where { get; private set; }
    public List<OrderByExpression> OrderBy { get; private set; }

    protected override SqlExpressionType Type { get { return SqlExpressionType.Query; } }
  }

  internal class ImpalaExpressionVisitor {
    private StringBuilder _sb;

    public string GetQuery(Expression expression) {
      _sb = new StringBuilder();
      var queryExpression = new LinqExpressionVisitor(expression).TranslateExpression();
      TableAlias = queryExpression.Alias;
      return Visit(queryExpression);
    }

    public string Visit(QueryExpression expression) {
      _sb.Append("SELECT ").Append(expression.Alias).Append(".* FROM ");
      _sb.Append(VisitTableExpression(expression.Table));
      _sb.Append(VisitWhereExpression(expression.Where));
      _sb.Append(VisitOrderByExpression(expression.OrderBy));

      return _sb.ToString().Trim();
    }

    private string VisitOrderByExpression(List<OrderByExpression> orderBy) {
      StringBuilder sb = new StringBuilder();

      foreach (var expression in orderBy) {
        sb.Append(new LambdaVisitor(expression.Expression, TableAlias).Translate());
        sb.Append(expression.Order == Order.Asc ? " " : " DESC ");
      }

      return sb.Length > 0 ? string.Format("ORDER BY {0}", sb) : "";
    }

    private string VisitWhereExpression(WhereExpression expression) {
      if (expression == null) return "";

      var whereStatement = new LambdaVisitor(expression.Expression, TableAlias).Translate();

      return whereStatement.Length > 0 ? string.Format("WHERE {0} ", whereStatement) : "";
    }

    protected  static string TableAlias { get; private set; }

    private string VisitTableExpression(SqlExpression expression)
    {
      return string.Format("({0}) {1} ", ((TableExpression)expression).Table, ((TableExpression)expression).Alias);
    }
  }

  internal class LambdaVisitor : ExpressionVisitor {
    private readonly Expression _expression;
    private readonly string _tableAlias;
    private StringBuilder _sb;

    public LambdaVisitor(Expression expression, string tableAlias) {
      _expression = expression;
      _tableAlias = tableAlias;
    }

    public String Translate()
    {
      _sb = new StringBuilder();
      Visit(_expression);
      return _sb.ToString();
    }

    protected override Expression VisitBinary(BinaryExpression b)
    {
      _sb.Append("(");
      _sb.Append(new LambdaVisitor(b.Left, _tableAlias).Translate());
      var rightExpression = new LambdaVisitor(b.Right, _tableAlias).Translate();

      switch (b.NodeType)
      {
        case ExpressionType.AndAlso:
        case ExpressionType.And:
          _sb.Append(" AND ");
          break;
        case ExpressionType.OrElse:
        case ExpressionType.Or:
          _sb.Append(" OR ");
          break;
        case ExpressionType.Equal:
          _sb.Append(rightExpression == "NULL" ? " IS " : " = ");
          break;
        case ExpressionType.NotEqual:
          _sb.Append(rightExpression == "NULL" ? " IS NOT " : " <> ");
          break;
        case ExpressionType.LessThan:
          _sb.Append(" < ");
          break;
        case ExpressionType.LessThanOrEqual:
          _sb.Append(" <= ");
          break;
        case ExpressionType.GreaterThan:
          _sb.Append(" > ");
          break;
        case ExpressionType.GreaterThanOrEqual:
          _sb.Append(" >= ");
          break;
        default:
          throw new NotSupportedException(string.Format("The binary operator '{0}' is not supported", b.NodeType));
      }
      _sb.Append(rightExpression);
      _sb.Append(")");
      return b;
    }


    protected override Expression VisitConstant(ConstantExpression node)
    {
      var q = node.Value as IQueryable;
      if (node.Value == null)
      {
        _sb.Append("NULL");
      }
      else
      {
        switch (Type.GetTypeCode(node.Value.GetType()))
        {
          case TypeCode.Boolean:
            _sb.Append("'");
            _sb.Append(((bool)node.Value) ? "true" : "false");
            _sb.Append("'");
            break;
          case TypeCode.String:
            _sb.Append("'");
            _sb.Append(node.Value);
            _sb.Append("'");
            break;
          case TypeCode.Object:
            throw new NotSupportedException(string.Format("The constant for '{0}' is not supported", node.Value));
          default:
            _sb.Append(node.Value);
            break;
        }
      }
      return base.VisitConstant(node);
    }

    protected override Expression VisitMember(MemberExpression node)
    {
      if (node.Expression != null && node.Expression.NodeType == ExpressionType.Parameter)
      {
        _sb.Append(_tableAlias).Append(".").Append(ModelUtils.ColumnNameForField(node.Member));
        return node;
      }
      throw new NotSupportedException(string.Format("The member '{0}' is not supported", node.Member.Name));
    }
  }
}
