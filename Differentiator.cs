using System.Linq.Expressions;

namespace Reflection.Differentiation
{
    public static class Differentiator
    {
        public static Expression<Func<double, double>> Differentiate(Expression<Func<double, double>> function)
        {
            var visitor = new DifferentiationVisitor();

            var body = visitor.Visit(function.Body);

            return Expression.Lambda<Func<double, double>>(body, function.Parameters);
        }
    }
}
