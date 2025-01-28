using MCRA.Utils;
using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Calculators.ProcessingFactorCalculation.ProcessingFactorModels {

    /// <summary>
    /// Fixed processing factors (nominal value), only pf > 1
    /// </summary>
    public sealed class PFFixedAllowHigherModel(ProcessingFactor processingFactor)
        : ProcessingFactorModel(processingFactor) {

        private double? _uncertaintyMu;
        private double _factor;
        private double _mu;
        private double? _factorDrawn;
        private ProcessingDistributionType distributionType;

        public override void CalculateParameters() {
            _factor = ProcessingFactor.Nominal < 1 ? 1 : ProcessingFactor.Nominal;
            distributionType = ProcessingFactor.ProcessingType.DistributionType;
            if (ProcessingFactor.NominalUncertaintyUpper != null) {
                var nominalUncertainty = ProcessingFactor.NominalUncertaintyUpper.Value < _factor ? _factor : ProcessingFactor.NominalUncertaintyUpper.Value;
                if (distributionType == ProcessingDistributionType.LogisticNormal) {
                    _mu = UtilityFunctions.Logit(_factor);
                    _uncertaintyMu = (UtilityFunctions.Logit(nominalUncertainty) - _mu) / 1.645;
                } else if (distributionType == ProcessingDistributionType.LogNormal) {
                    _mu = UtilityFunctions.LogBound(_factor);
                    _uncertaintyMu = (UtilityFunctions.LogBound(nominalUncertainty) - _mu) / 1.645;
                }
            }
        }

        public override double GetNominalValue() {
            var factor = _factorDrawn ?? _factor;
            return factor > 1 ? factor : 1D;
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
            _factorDrawn = _factorDrawn < 1 ? 1 : _factorDrawn;
        }

        public override void ResetNominal() {
            _isModellingUncertainty = false;
            _factorDrawn = null;
        }
    }
}
