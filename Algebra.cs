using System.Linq.Expressions;

namespace Reflection.Differentiation;

public static class Algebra
{
    public static Expression<Func<double, double>> Differentiate(Expression<Func<double, double>> function)
    {
        var visitor = new DifferentiationVisitor();

        var body = visitor.Visit(function.Body);

        return Expression.Lambda<Func<double, double>>(body, function.Parameters);
    }
}

public class DifferentiationVisitor : ExpressionVisitor
{
    protected override Expression VisitBinary(BinaryExpression expr)
    {
        var left = Visit(expr.Left);
        var right = Visit(expr.Right);

        switch (expr.NodeType)
        {
            case ExpressionType.Add:
                return Expression.Add(left, right);

            case ExpressionType.Multiply:
                return Expression.Add(
                    Expression.Multiply(left, expr.Right), 
                    Expression.Multiply(right, expr.Left)
                );

            default:
                throw new ArgumentException($"Binary operator '{expr.NodeType}' is not supported.");
        }
    }

    protected override Expression VisitMethodCall(MethodCallExpression expr)
    {
        switch (expr.Method.Name)
        {
            case "Sin":
                var cosMethod = typeof(Math).GetMethod("Cos");
                return Expression.Multiply(
                    Expression.Call(cosMethod, expr.Arguments),
                    Visit(expr.Arguments[0])
                );

            case "Cos":
                var sinMethod = typeof(Math).GetMethod("Sin");
                return Expression.Multiply(
                    Expression.Multiply(
                        Expression.Constant(-1.0),
                        Expression.Call(sinMethod, expr.Arguments)
                    ),
                    Visit(expr.Arguments[0])
                );
                    
            default:
                throw new ArgumentException($"Function '{expr.Method.Name}' is not supported.");
        }
    }

    protected override Expression VisitConstant(ConstantExpression expr)
        => Expression.Constant(0.0);

    protected override Expression VisitParameter(ParameterExpression expr)
        => Expression.Constant(1.0);
}