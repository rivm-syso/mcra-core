using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.AdjustmentFactorCalculation {
    public class AFLogStudentTModel : AdjustmentFactorModelBase, IAdjustmentFactorModel {
        public double A { get; set; }

        public double B { get; set; }

        public double C { get; set; }

        public double D { get; set; }

        public AFLogStudentTModel(double a, double b, double c, double d) {
            A = a;
            B = b;
            C = c;
            D = d;
            if (B <= 0 || C <= 0 || D < 0) {
                throw new Exception($"Log Students t model: scale parameter B = {B} , df parameter C = {C}, offset parameter D = {D}. Restriction: B, C > 0 ; D >= 0.");
            }
        }

        public override double DrawFromDistribution(IRandom random) {
            var logStudentTdistribution = new LogStudentTScaledDistribution(A, B, C, D);
            return logStudentTdistribution.Draw(random);
        }

        public override double GetNominal() {
            var mean = Math.Exp(A) + D;
            return  mean;
        }
    }
}
