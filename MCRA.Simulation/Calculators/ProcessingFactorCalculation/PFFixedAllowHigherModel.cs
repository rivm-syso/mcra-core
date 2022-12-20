using MCRA.Utils;
using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Calculators.ProcessingFactorCalculation {

    /// <summary>
    /// Fixed processing factors (nominal value), only pf > 1
    /// </summary>
    public sealed class PFFixedAllowHigherModel : ProcessingFactorModel {

        private double? _uncertaintyMu;
        private double _factor;
        private double _mu;
        private double? _factorDrawn;
        private ProcessingDistributionType distributionType;

        public double Factor {
            get { return _factorDrawn ?? _factor; }
        }

        public override void CalculateParameters(ProcessingFactor pf) {
            _factor = pf.Nominal < 1 ? 1 : pf.Nominal;
            distributionType = pf.ProcessingType.DistributionType;
            if (pf.NominalUncertaintyUpper != null) {
                var nominalUncertainty = pf.NominalUncertaintyUpper.Value < _factor ? _factor : pf.NominalUncertaintyUpper.Value;
                if (distributionType == ProcessingDistributionType.LogisticNormal) {
                    _mu = UtilityFunctions.Logit(_factor);
                    _uncertaintyMu = (UtilityFunctions.Logit(nominalUncertainty) - _mu) / 1.645;
                } else if (distributionType == ProcessingDistributionType.LogNormal) {
                    _mu = UtilityFunctions.LogBound(_factor);
                    _uncertaintyMu = (UtilityFunctions.LogBound(nominalUncertainty) - _mu) / 1.645;
                }
            }
        }

        public override (double, bool) GetNominalValue() {
            return Factor > 1
                ? (Factor, true)
                : (1D, false);
        }

        public override (double, bool) DrawFromDistribution(IRandom random) {
            return GetNominalValue();
        }

        public override void Resample(IRandom random) {
            _isModellingUncertainty = true;
            if (_uncertaintyMu != null) {
                var draw = (double)_uncertaintyMu * NormalDistribution.InvCDF(0, 1, random.NextDouble()) + _mu;
                if (distributionType == ProcessingDistributionType.LogisticNormal) {
                    _factorDrawn = UtilityFunctions.ILogit(draw);
                } else if (distributionType == ProcessingDistributionType.LogNormal) {
                    _factorDrawn = UtilityFunctions.ExpBound(draw);
                }
            }
            _factorDrawn = _factorDrawn < 1 ? 1 : _factorDrawn;
        }

        public override void ResetNominal() {
            _isModellingUncertainty = false;
            _factorDrawn = null;
        }
    }
}
