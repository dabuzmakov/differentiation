using NUnit.Framework;
using System.Linq.Expressions;
using Assert = NUnit.Framework.Legacy.ClassicAssert;

namespace Reflection.Differentiation;

[TestFixture]
public class Algebra_should
{
	void AssertDerivativeEqualToNumericDerivative(Expression<Func<double, double>> function)
	{
		var f = function.Compile();
		var eps = 1e-7;
		var dfExpression = Algebra.Differentiate(function);
		var df = dfExpression.Compile();
		for (double x = 0; x < 5; x += 0.1)
		{
			Assert.AreEqual((f(x + eps) - f(x)) / eps, df(x), 1e-5, $"Error on function {function.Body}");
		}
	}

	[Test]
	public void DifferentiateConstant()
		=> AssertDerivativeEqualToNumericDerivative(z => 42);

	[Test]
	public void DifferentiateParameter()
		=> AssertDerivativeEqualToNumericDerivative(z => z);

	[Test]
	public void DifferentiateLinearFunction()
		=> AssertDerivativeEqualToNumericDerivative(z => z * 5);
	

	[Test]
	public void DifferentiateQuadraticFunction()
		=> AssertDerivativeEqualToNumericDerivative(z => z * 5 * z);

	[Test]
	public void DifferentiateSum()
		=> AssertDerivativeEqualToNumericDerivative(z => z + z);

	[Test]
	public void DifferentiateSumAndProduct()
		=> AssertDerivativeEqualToNumericDerivative(z => 5 * z + z * z);

	[Test]
	public void DifferentiateSin1()
		=> AssertDerivativeEqualToNumericDerivative(z => Math.Sin(z));

	[Test]
	public void DifferentiateSin2()
		=> AssertDerivativeEqualToNumericDerivative(z => Math.Sin(z * z + z));

	[Test]
	public void DifferentiateCos1()
		=> AssertDerivativeEqualToNumericDerivative(z => Math.Cos(z));

	[Test]
	public void DifferentiateCos2()
		=> AssertDerivativeEqualToNumericDerivative(z => Math.Cos(z * z + z));

	[Test]
	public void DifferentiateComplexExpression()
		=> AssertDerivativeEqualToNumericDerivative(z => Math.Cos(2 * z + z) + 2 * Math.Sin(3 * z + z) + Math.Sin(z + 1) * Math.Cos(z + 2) * 3);

    [Test] public void DifferentiateSubtraction() 
		=> AssertDerivativeEqualToNumericDerivative(z => z - 3 * z);

    [Test] public void DifferentiateUnaryMinus() 
		=> AssertDerivativeEqualToNumericDerivative(z => -z * z);

    [Test] public void DifferentiateDivision() 
		=> AssertDerivativeEqualToNumericDerivative(z => (3 * z + 1) / (z * z + 2));

    [Test] public void DifferentiatePower() 
		=> AssertDerivativeEqualToNumericDerivative(z => Math.Pow(z, 4));

    [Test] public void DifferentiatePowerWithConstantBase() 
		=> AssertDerivativeEqualToNumericDerivative(z => Math.Pow(2, z));

    [Test] public void DifferentiateSqrt() 
		=> AssertDerivativeEqualToNumericDerivative(z => Math.Sqrt(z + 1));

    [Test] public void DifferentiateExp() 
		=> AssertDerivativeEqualToNumericDerivative(z => Math.Exp(2 * z + 1));

    [Test] public void DifferentiateLog() 
		=> AssertDerivativeEqualToNumericDerivative(z => Math.Log(z * z + 1));

    [Test] public void DifferentiateTan() 
		=> AssertDerivativeEqualToNumericDerivative(z => Math.Tan(z));

    [Test] public void DifferentiateTanOfComplex() 
		=> AssertDerivativeEqualToNumericDerivative(z => Math.Tan(z * z + Math.Sin(z)));

    [Test]
    public void DifferentiateSumAndProductWithSubtraction()
        => AssertDerivativeEqualToNumericDerivative(z => 5 * z - z * z + 3);

    [Test]
    public void DifferentiateComplexTrigAndExp()
        => AssertDerivativeEqualToNumericDerivative(z =>
            Math.Cos(2 * z) * Math.Sin(z) +
            Math.Exp(-z) * Math.Pow(z, 2) +
            3 / (z + 1));

    [Test]
    public void DifferentiateNestedFunctions()
        => AssertDerivativeEqualToNumericDerivative(z =>
            Math.Sin(Math.Cos(Math.Exp(-z * z))));

    [Test]
    public void DifferentiateOriginalComplexExpression()
        => AssertDerivativeEqualToNumericDerivative(z =>
            Math.Cos(2 * z + z) + 2 * Math.Sin(3 * z + z) + Math.Sin(z + 1) * Math.Cos(z + 2) * 3);

}