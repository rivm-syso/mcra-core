using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.IntakeModelling {
    public sealed class TransformationCalculationResult {

        public List<double> Z400 { get; set; }
        public List<double> Z { get; set; }
        public List<double> GZ { get; set; }
        public List<double> ZHat { get; set; }
        public List<double> GZ400 { get; set; }

        public List<double> YI400 { get; set; }
        public int NumberOfKnots { get; set; }
        public AndersonDarlingResults AndersonDarlingResults { get; set; }
        public double Power { get; set; }

        /// <summary>
        /// P-value test Statistic
        /// </summary>
        public double PMA4 { get; set; }

        /// <summary>
        /// Test statistic
        /// </summary>
        public double MA4 { get; set; }

        public double VarianceBetweenUnit { get; set; }
        public double VarianceWithinUnit { get;  set; }
    }
}
