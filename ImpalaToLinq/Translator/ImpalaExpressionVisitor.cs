using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace ImpalaToLinq.Translator
{
  internal class ImpalaExpressionVisitor: ExpressionVisitor {
    private readonly Expression _expression;
    private readonly int _depth;
    private readonly StringBuilder _sb;

    public ImpalaExpressionVisitor(Expression expression):this(expression, 0) {
    }
    public ImpalaExpressionVisitor(Expression expression, int depth)
    {
      _expression = expression;
      _depth = depth;
      _sb = new StringBuilder();
    }

    public string GetQuery() {
      Visit(_expression);
      return _sb.ToString();
    }

    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
      if (node.Method.Name == "Where") {
        _sb.Append("SELECT ").Append(GetTableAlias()).Append(".* FROM ( ");
        _sb.Append(new ImpalaExpressionVisitor(node.Arguments[0], _depth+1).GetQuery());
        _sb.Append(") ").Append(GetTableAlias()).Append(" WHERE ");
        var lambda = (LambdaExpression)StripQuotes(node.Arguments[1]);
        _sb.Append(new ImpalaExpressionVisitor(lambda.Body, _depth).GetQuery());
        return node;
      }
      throw new NotSupportedException(string.Format("The method call '{0}' is not supported", node.NodeType));
    }

    protected override Expression VisitBinary(BinaryExpression b) {
      _sb.Append("(");
      _sb.Append(new ImpalaExpressionVisitor(b.Left, _depth).GetQuery());
      var rightExpression = new ImpalaExpressionVisitor(b.Right, _depth).GetQuery();
 
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
      if (q != null)
      {
        // assume constant nodes w/ IQueryables are table references
        _sb.Append("SELECT ").Append(GetTableAlias()).Append(".* FROM ");
        _sb.Append(ModelUtils.TableName(q));
        _sb.Append(" ").Append(GetTableAlias());
      }
      else if (node.Value == null)
      {
        _sb.Append("NULL");
      }
      else {
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

    protected override Expression VisitParameter(ParameterExpression node)
    {
      return base.VisitParameter(node);
    }

    protected override Expression VisitMember(MemberExpression node)
    {
      if (node.Expression != null && node.Expression.NodeType == ExpressionType.Parameter)
      {
        _sb.Append(GetTableAlias()).Append(".").Append(ModelUtils.ColumnNameForField(node.Member));
        return node;
      }
      throw new NotSupportedException(string.Format("The member '{0}' is not supported", node.Member.Name));
    }
    private Expression StripQuotes(Expression e)
    {
      while (e.NodeType == ExpressionType.Quote)
      {
        e = ((UnaryExpression)e).Operand;
      }
      return e;
    }

    private string GetTableAlias() {
      return "T" + _depth;
    }
  }
}
