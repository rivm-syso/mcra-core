using MathNet.Numerics.Distributions;
using MathNet.Numerics.LinearAlgebra;


namespace MCRA.Utils.Statistics {

    /// <summary>
    ///  Continuous Multivariate Normal distribution.
    ///  </summary>
    public class MultiVariateNormalDistribution {

        public Vector<double> Mu { get; private set; }
        public  Matrix<double> Vcov { get; private set; }

        public MultiVariateNormalDistribution(
            List<double> mu, 
            double[,] vcov
        ) {
            Mu = Vector<double>.Build.Dense([.. mu]);
            Vcov = Matrix<double>.Build.DenseOfArray(vcov);
        }

        public double[] Draw(IRandom random) {
            var rnd = new RandomAsRandomWrapper(random);
            return Sample(rnd, Mu, Vcov);
        }

        public double[] Draw(IRandom random, Vector<double> mu, Matrix<double> vcov) {
            var rnd = new RandomAsRandomWrapper(random);
            return Sample(rnd, mu, vcov);
        }

        public List<double[]> Samples(IRandom random, int n) {
            var rnd = new RandomAsRandomWrapper(random);
            var x = new List<double[]>();
            for (int i = 0; i < n; i++) {
                x.Add(Sample(rnd, Mu, Vcov));
            }
            return x;
        }

        public  double[] Sample(Random random, Vector<double> mu, Matrix<double> vcov) {
            var dist = new MatrixNormal(
                mu.ToColumnMatrix(),
                vcov,
                Matrix<double>.Build.DenseIdentity(1),
                random
            );
            return [.. dist.Sample().Column(0)];
        }
    }
}
