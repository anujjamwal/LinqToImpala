using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace ImpalaToLinq.Translator
{
  internal class ImpalaExpressionVisitor: ExpressionVisitor {
    private readonly Expression _expression;
    private readonly StringBuilder _sb;

    public ImpalaExpressionVisitor(Expression expression) {
      _expression = expression;
      _sb = new StringBuilder();
    }

    public string GetQuery() {
      Visit(_expression);
      return _sb.ToString();
    }

    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
      if (node.Method.Name == "Where") {
        _sb.Append("SELECT * FROM ( ");
        _sb.Append(new ImpalaExpressionVisitor(node.Arguments[0]).GetQuery());
        _sb.Append(") WHERE ");
        var lambda = (LambdaExpression)StripQuotes(node.Arguments[1]);
        _sb.Append(new ImpalaExpressionVisitor(lambda.Body).GetQuery());
        return node;
      }
      throw new NotSupportedException(string.Format("The method call '{0}' is not supported", node.NodeType));
    }

    protected override Expression VisitBinary(BinaryExpression b) {
      _sb.Append("(");
      _sb.Append(new ImpalaExpressionVisitor(b.Left).GetQuery());
        
      switch (b.NodeType)
      {
        case ExpressionType.Equal:
          _sb.Append(" = ");
          break;
        default:
          throw new NotSupportedException(string.Format("The binary operator '{0}' is not supported", b.NodeType));
      }
      _sb.Append(new ImpalaExpressionVisitor(b.Right).GetQuery());
      _sb.Append(")");
      return b;
    }

    protected override Expression VisitConstant(ConstantExpression node)
    {
      var q = node.Value as IQueryable;
      if (q != null)
      {
        // assume constant nodes w/ IQueryables are table references
        _sb.Append("SELECT * FROM ");
        _sb.Append(q.ElementType.Name);
      }
      else if (node.Value == null)
      {
        _sb.Append("NULL");
      }
      else {
        switch (Type.GetTypeCode(node.Value.GetType()))
        {
          case TypeCode.Boolean:
            _sb.Append(((bool)node.Value) ? 1 : 0);
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
        _sb.Append(node.Member.Name);
        return node;
      }
      throw new NotSupportedException(string.Format("The member '{0}' is not supported", node.Member.Name));
    }
    private static Expression StripQuotes(Expression e)
    {
      while (e.NodeType == ExpressionType.Quote)
      {
        e = ((UnaryExpression)e).Operand;
      }
      return e;
    }
  }
}
