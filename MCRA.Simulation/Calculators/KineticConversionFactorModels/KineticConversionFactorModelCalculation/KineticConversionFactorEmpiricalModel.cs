using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.KineticConversionFactorModels.KineticConversionFactorModelCalculation {

    public struct KineticConversionDataRecord {

        public double FromExposure { get; set; }

        public double ToExposure { get; set; }

        public KineticConversionDataRecord(double externalExposure, double targetExposure) {
            FromExposure = externalExposure;
            ToExposure = targetExposure;
        }

        public double Factor {
            get {
                return ToExposure / FromExposure;
            }
        }
    }

    public sealed class KineticConversionFactorEmpiricalModel : IKineticConversionFactorModel {
        
        public double Factor { get; set; }

        public List<KineticConversionDataRecord> KineticConversionDataRecords { get; set; }

        public Compound SubstanceTo { get; set; }

        public Compound SubstanceFrom { get; set; }

        public ExposureUnitTriple UnitTo { get; set; }

        public ExposureUnitTriple UnitFrom { get; set; }

        public ExposureTarget TargetFrom { get; set; }

        public ExposureTarget TargetTo { get; set; }

        public void CalculateParameters() {
            throw new NotImplementedException();
        }

        public void ResampleModelParameters(IRandom random) {
            throw new NotImplementedException();
        }

        public double GetConversionFactor(double? age, GenderType gender) {
            return Factor;
        }

        public bool HasCovariate(KineticConversionFactorCovariateType covariateType) {
            return false;
        }
    }
}
