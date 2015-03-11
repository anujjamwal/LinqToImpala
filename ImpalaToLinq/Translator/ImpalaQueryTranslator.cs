using System.Linq.Expressions;

namespace ImpalaToLinq.Translator
{
  internal class ImpalaQueryTranslator
  {
    internal ImpalaQueryTranslator()
    {
    }

    internal string Translate(Expression expression)
    {
      return new ImpalaExpressionVisitor().GetQuery(Evaluator.PartialEval(expression));
    }

  }
}
