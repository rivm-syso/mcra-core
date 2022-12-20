using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Calculators.IntakeModelling {

    /// <summary>
    /// Represents a group of individuals with a common cofactor/covariable.
    /// </summary>
    public class CovariateGroup {
        public string Cofactor { get; set; }
        public double Covariable { get; set; }
        public int NumberOfIndividuals { get; set; }
        public double GroupSamplingWeight { get; set; }
        public override int GetHashCode() {
            return $"{Cofactor}/{Covariable}".GetChecksum();
        }
    }
}
