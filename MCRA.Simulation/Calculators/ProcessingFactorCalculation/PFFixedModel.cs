using MCRA.Utils;
using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Calculators.ProcessingFactorCalculation {

    /// <summary>
    /// Fixed processing factors (nominal value).
    /// </summary>
    public sealed class PFFixedModel : ProcessingFactorModel {

        private double? _uncertaintyMu;
        private double _factor;
        private double _mu;
        private double? _factorDrawn;
        private ProcessingDistributionType distributionType;

        public double Factor {
            get { return _factorDrawn ?? _factor; }
        }

        public override void CalculateParameters(ProcessingFactor pf) {
            _factor = pf.Nominal;
            distributionType = pf.ProcessingType.DistributionType;
            if (pf.NominalUncertaintyUpper != null) {
                if (distributionType == ProcessingDistributionType.LogisticNormal) {
                    _mu = UtilityFunctions.Logit(_factor);
                    _uncertaintyMu = (UtilityFunctions.Logit(pf.NominalUncertaintyUpper.Value) - _mu) / 1.645;
                } else if (distributionType == ProcessingDistributionType.LogNormal) {
                    _mu = UtilityFunctions.LogBound(_factor);
                    _uncertaintyMu = (UtilityFunctions.LogBound(pf.NominalUncertaintyUpper.Value) - _mu) / 1.645;
                }
            }
        }

        public override double GetNominalValue() {
            return Factor;
        }

        public override double DrawFromDistribution(IRandom random) {
            return GetNominalValue();
        }

        public override void Resample(IRandom random) {
            _isModellingUncertainty = true;
            if (_uncertaintyMu != null) {
                var draw = _uncertaintyMu.Value * NormalDistribution.InvCDF(0, 1, random.NextDouble()) + _mu;
                if (distributionType == ProcessingDistributionType.LogisticNormal) {
                    _factorDrawn = UtilityFunctions.ILogit(draw);
                } else if (distributionType == ProcessingDistributionType.LogNormal) {
                    _factorDrawn = UtilityFunctions.ExpBound(draw);
                }
            }
        }

        public override void ResetNominal() {
            _isModellingUncertainty = false;
            _factorDrawn = null;
        }
    }
}
