using System;
using System.Linq;
using System.Linq.Expressions;

namespace ImpalaToLinq.Translator {
  internal class LinqExpressionVisitor: ExpressionVisitor {
    private readonly Expression _expression;
    private readonly int _depth;
    private QueryExpression _queryExpression;

    public LinqExpressionVisitor(Expression expression):this(expression, 0) {
    }
    public LinqExpressionVisitor(Expression expression, int depth)
    {
      _expression = expression;
      _depth = depth;
    }

    public QueryExpression TranslateExpression() {
      _queryExpression = new QueryExpression(GetTableAlias());
      Visit(_expression);
      return _queryExpression;
    }

    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
      if (node.Method.Name == "Where") {
        Visit(node.Arguments[0]);
        var lambda = (LambdaExpression)StripQuotes(node.Arguments[1]);
        _queryExpression.AddWhereExpression(lambda);
        return node;
      }

      if (node.Method.Name == "OrderBy" || node.Method.Name == "ThenBy")
      {
        Visit(node.Arguments[0]);
        var lambda = (LambdaExpression)StripQuotes(node.Arguments[1]);
        _queryExpression.AddOrderByExpression(lambda);
        return node;
      }

      if (node.Method.Name == "OrderByDescending" || node.Method.Name == "ThenByDescending")
      {
        Visit(node.Arguments[0]);
        var lambda = (LambdaExpression)StripQuotes(node.Arguments[1]);
        _queryExpression.AddOrderByExpressionDescending(lambda);
        return node;
      }
      throw new NotSupportedException(string.Format("The method call '{0}' is not supported", node.NodeType));
    }


    protected override Expression VisitConstant(ConstantExpression node)
    {
      var q = node.Value as IQueryable;
      if (q != null)
      {
        _queryExpression.AddTableExpression(q);
      }
      return base.VisitConstant(node);
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