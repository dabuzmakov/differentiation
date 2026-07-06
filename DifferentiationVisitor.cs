using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;

namespace Reflection.Differentiation;

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

    private readonly Dictionary<string, Func<ReadOnlyCollection<Expression>, Expression>> _handlers;
    
    public DifferentiationVisitor()
    {
        _handlers = new()
        {
            ["Sin"] = DifferentiateSin,
            ["Cos"] = DifferentiateCos,
            ["Tan"] = DifferentiateTan,
            ["Pow"] = DifferentiatePow,
            ["Sqrt"] = DifferentiateSqrt,
            ["Exp"] = DifferentiateExp,
            ["Log"] = DifferentiateLog,
            ["Abs"] = DifferentiateAbs,
            ["Asin"] = DifferentiateAsin,
            ["Acos"] = DifferentiateAcos,
            ["Atan"] = DifferentiateAtan,
        };
    }

    protected override Expression VisitBinary(BinaryExpression expr)
    {
        var dLeft = Visit(expr.Left);
        var dRight = Visit(expr.Right);

        return expr.NodeType switch
        {
            ExpressionType.Add => Expression.Add(dLeft, dRight),
            ExpressionType.Subtract => Expression.Subtract(dLeft, dRight),
            ExpressionType.Multiply => Expression.Add(
                Expression.Multiply(dLeft, expr.Right),
                Expression.Multiply(dRight, expr.Left)
                ),
            ExpressionType.Divide => Expression.Divide(
                Expression.Subtract(
                    Expression.Multiply(dLeft, expr.Right),
                    Expression.Multiply(dRight, expr.Left)
                    ),
                Expression.Multiply(expr.Right, expr.Right)
                ),
            _ => throw new ArgumentException($"Binary operator '{expr.NodeType}' is not supported.")
        };
    }

    protected override Expression VisitMethodCall(MethodCallExpression expr)
        => _handlers.TryGetValue(expr.Method.Name, out var handler)
            ? handler(expr.Arguments)
            : throw new ArgumentException($"Function '{expr.Method.Name}' is not supported.");

    protected override Expression VisitConstant(ConstantExpression expr)
        => Expression.Constant(0.0);

    protected override Expression VisitParameter(ParameterExpression expr)
        => Expression.Constant(1.0);

    private Expression DifferentiateAtan(ReadOnlyCollection<Expression> args)
        => Expression.Multiply(
            Visit(args[0]),
            Expression.Divide(
                Expression.Constant(1.0),
                Expression.Add(
                    Expression.Constant(1.0),
                    Expression.Call(
                        _powMethod,
                        args[0],
                        Expression.Constant(2.0)
                        )
                    )
                )
            );

    private Expression DifferentiateAcos(ReadOnlyCollection<Expression> args)
        => Expression.Multiply(
            Visit(args[0]),
            Expression.Divide(
                Expression.Constant(-1.0),
                Expression.Call(
                    _powMethod,
                    Expression.Subtract(
                        Expression.Constant(1.0),
                        Expression.Call(
                            _powMethod,
                            args[0],
                            Expression.Constant(2.0)
                            )
                        ),
                    Expression.Constant(1.0 / 2)
                    )
                )
            );

    private Expression DifferentiateAsin(ReadOnlyCollection<Expression> args)
        => Expression.Multiply(
            Visit(args[0]),
            Expression.Divide(
                Expression.Constant(1.0),
                Expression.Call(
                    _powMethod,
                    Expression.Subtract(
                        Expression.Constant(1.0),
                        Expression.Call(
                            _powMethod,
                            args[0],
                            Expression.Constant(2.0)
                            )
                        ),
                    Expression.Constant(1.0 / 2)
                    )
                )
            );

    private Expression DifferentiateAbs(ReadOnlyCollection<Expression> args)
        => Expression.Multiply(
            Expression.Convert(
                Expression.Call(_signMethod, args[0]),
                typeof(double)
                ),
            Visit(args[0])
            );

    private Expression DifferentiateLog(ReadOnlyCollection<Expression> args)
    {
        if (args.Count == 1)
        {
            return Expression.Divide(
                Visit(args[0]),
                args[0]
                );
        }
        else
        {
            return Expression.Divide(
                Visit(args[0]),
                Expression.Multiply(
                    args[0],
                    Expression.Call(_logNatural, args[1])
                    )
                );
        }
    }

    private Expression DifferentiateExp(ReadOnlyCollection<Expression> args)
        => Expression.Multiply(
            Expression.Call(_expMethod, args),
            Visit(args[0])
            );

    private Expression DifferentiateSqrt(ReadOnlyCollection<Expression> args)
        => Expression.Divide(
            Visit(args[0]),
            Expression.Multiply(
                Expression.Constant(2.0),
                Expression.Call(_sqrtMethod, args[0])
                )
            );

    private Expression DifferentiatePow(ReadOnlyCollection<Expression> args)
    {
        var dFirstArg = Visit(args[0]);
        var dSecondArg = Visit(args[1]);

        if (args[0].NodeType == ExpressionType.Constant)
            return Expression.Multiply(
                dSecondArg!,
                Expression.Multiply(
                    Expression.Call(
                        _powMethod,
                        args[0],
                        args[1]
                    ),
                    Expression.Call(_logNatural, args[0])
                )
            );
        else if (args[1].NodeType == ExpressionType.Constant)
            return Expression.Multiply(
                dFirstArg,
                Expression.Multiply(
                    args[1],
                    Expression.Call(
                        _powMethod,
                        args[0],
                        Expression.Subtract(
                            args[1],
                            Expression.Constant(1.0)
                        )
                    )
                )
            );
        else return Expression.Multiply(
            Expression.Call(
                _powMethod,
                args[0],
                args[1]),
            Expression.Add(
                Expression.Multiply(
                    dSecondArg!,
                    Expression.Call(_logNatural, args[0])
                ),
                Expression.Divide(
                    Expression.Multiply(
                        args[1],
                        dFirstArg),
                    args[0]
                )
            )
        );
    }

    private Expression DifferentiateTan(ReadOnlyCollection<Expression> args)
        => Expression.Multiply(
            Visit(args[0]),
            Expression.Divide(
                Expression.Constant(1.0),
                Expression.Multiply(
                    Expression.Call(_cosMethod, args[0]),
                    Expression.Call(_cosMethod, args[0])
                    )
                )
            );

    private Expression DifferentiateCos(ReadOnlyCollection<Expression> args)
        => Expression.Multiply(
            Expression.Multiply(
                Expression.Constant(-1.0),
                Expression.Call(_sinMethod, args)
                ),
            Visit(args[0])
            );

    private Expression DifferentiateSin(ReadOnlyCollection<Expression> args)
        => Expression.Multiply(
            Expression.Call(_cosMethod, args),
            Visit(args[0])
            );
}