using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Utils.Test.UnitTests {

    [TestClass]
    public class PolynomialTests {

        [TestMethod]
        public void PolynomialTest1() {
            int n = 2;

            var x = new List<double>() { 4.32, 1.34, 6.73, 3.56, 6.9, 6.34, 9.32, 19.2, 21.23, 1.231, 5 };
            var wt = new List<double> () { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 5 };

            //var op = new MCRA.Utils.OrthogonalPolynomial();
            //var pol = op.CalculateOrthPol(x, 4, wt);
            var testvalue = new double[1];
            testvalue[0] = x[n];

            Polynomial polynomial = null;
            var design = new List<double[]>();
            int maxDegree = 3;
            var orthPol = new OrthogonalPolynomial();
            polynomial = orthPol.CalculateOrthPol(x,maxDegree,wt);
            design.AddRange(polynomial.Result);

            var result = polynomial.CalculateComponentsPolynomial(testvalue, maxDegree).Select(p => p.First()).ToList();

            System.Diagnostics.Trace.WriteLine($"Testvalue {testvalue[0]} and corresponding line of orthogonal polynomial. ");
            System.Diagnostics.Trace.WriteLine("Orthogonal polynomial ");
            System.Diagnostics.Trace.WriteLine($"1e: {polynomial.Result[0][n]:F7}   2e: {polynomial.Result[1][n]:F7}   3e: {polynomial.Result[2][n]:F7} ");
            System.Diagnostics.Trace.WriteLine("TestValue " );
            System.Diagnostics.Trace.WriteLine($"1e: {result[0]:F7}   2e: {result[1]:F7}   3e: {result[2]:F7} ");
            Assert.IsTrue(polynomial.Result[0][n] == result[0]);
            Assert.IsTrue(polynomial.Result[1][n] == result[1]);
            Assert.IsTrue(polynomial.Result[2][n] == result[2]);
        }
    }
}
