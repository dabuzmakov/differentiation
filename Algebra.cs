using System.Linq.Expressions;
using System.Reflection;

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
    private static readonly MethodInfo _sinMethod = typeof(Math).GetMethod("Sin", new[] { typeof(double) })!;
    private static readonly MethodInfo _cosMethod = typeof(Math).GetMethod("Cos", new[] { typeof(double) })!;
    private static readonly MethodInfo _tanMethod = typeof(Math).GetMethod("Tan", new[] { typeof(double) })!;
    private static readonly MethodInfo _powMethod = typeof(Math).GetMethod("Pow", new[] { typeof(double), typeof(double) })!;
    private static readonly MethodInfo _sqrtMethod = typeof(Math).GetMethod("Sqrt", new[] { typeof(double) })!;
    private static readonly MethodInfo _expMethod = typeof(Math).GetMethod("Exp", new[] { typeof(double) })!;
    private static readonly MethodInfo _logNatural = typeof(Math).GetMethod("Log", new[] { typeof(double) })!;
    private static readonly MethodInfo _signMethod = typeof(Math).GetMethod("Sign", new[] { typeof(double) })!;

    protected override Expression VisitBinary(BinaryExpression expr)
    {
        var left = Visit(expr.Left);
        var right = Visit(expr.Right);

        switch (expr.NodeType)
        {
            case ExpressionType.Add:
                return Expression.Add(left, right);

            case ExpressionType.Subtract:
                return Expression.Subtract(left, right);

            case ExpressionType.Multiply:
                return Expression.Add(
                    Expression.Multiply(left, expr.Right), 
                    Expression.Multiply(right, expr.Left)
                    );

            case ExpressionType.Divide:
                return Expression.Divide(
                    Expression.Subtract(
                        Expression.Multiply(left, expr.Right),
                        Expression.Multiply(right, expr.Left)
                        ),
                    Expression.Multiply(expr.Right, expr.Right)
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
                return Expression.Multiply(
                    Expression.Call(_cosMethod, expr.Arguments),
                    Visit(expr.Arguments[0])
                    );

            case "Cos":
                return Expression.Multiply(
                    Expression.Multiply(
                        Expression.Constant(-1.0),
                        Expression.Call(_sinMethod, expr.Arguments)
                        ),
                    Visit(expr.Arguments[0])
                    );

            case "Tan":
                return Expression.Multiply(
                    Visit(expr.Arguments[0]),
                    Expression.Divide(
                        Expression.Constant(1.0),
                        Expression.Multiply(
                            Expression.Call(_cosMethod, expr.Arguments[0]),
                            Expression.Call(_cosMethod, expr.Arguments[0])
                            )
                        )
                    );

            case "Pow":
                return expr.Arguments[0].NodeType == ExpressionType.Constant
                    ? Expression.Multiply(
                        Visit(expr.Arguments[1]),
                        Expression.Multiply(
                            Expression.Power(
                                expr.Arguments[0], 
                                expr.Arguments[1]
                                ),
                            Expression.Call(_logNatural, expr.Arguments[0])
                            )
                        )
                    : Expression.Multiply(
                        expr.Arguments[1],
                        Expression.Power(
                            expr.Arguments[0], 
                            Expression.Subtract(
                                expr.Arguments[1],
                                Expression.Constant(1.0)
                                )
                            )
                        );

            case "Sqrt":
                return Expression.Divide(
                    Visit(expr.Arguments[0]),
                    Expression.Multiply(
                        Expression.Constant(2.0), 
                        Expression.Call(_sqrtMethod, expr.Arguments[0])
                        )
                );

            case "Exp":
                return Expression.Multiply(
                    expr,
                    Visit(expr.Arguments[0])
                    );

            case "Log":
                if (expr.Arguments.Count == 1)
                {
                    return Expression.Divide(
                        Visit(expr.Arguments[0]),
                        expr.Arguments[0]
                        );
                }
                else
                {
                    return Expression.Divide(
                        Visit(expr.Arguments[0]), 
                        Expression.Multiply(
                            expr.Arguments[0], 
                            Expression.Call(_logNatural, expr.Arguments[1])
                            )
                        );
                }

            case "Abs":
                return Expression.Multiply(
                    Expression.Convert(
                        Expression.Call(_signMethod, expr.Arguments[0]),
                        typeof(double)
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