using MCRA.Utils;
using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Calculators.ProcessingFactorCalculation.ProcessingFactorModels {
    /// <summary>
    /// Distribution based processing factors using the logistic normal (0, 1),
    /// specified by a nominal and upper value.
    /// </summary>
    public sealed class PFLogisticModel(ProcessingFactor processingFactor)
        : ProcessingFactorModel(processingFactor), IDistributionProcessingFactorModel {

        private double _factor;
        private double _mu;
        private double _sigma;

        private double? _uncertaintyMu;
        private double? _degreesOfFreedom;

        private double? _muDrawn;
        private double? _sigmaDrawn;

        public ProcessingDistributionType DistributionType {
            get {
                return ProcessingDistributionType.LogisticNormal;
            }
        }

        public double Mu {
            get { return _muDrawn ?? _mu; }
        }

        public double Sigma {
            get { return _sigmaDrawn ?? _sigma; }
        }

        public double? DegreesOfFreedom {
            get { return _degreesOfFreedom; }
        }

        public override void CalculateParameters() {
            _factor = ProcessingFactor.Nominal;
            _mu = UtilityFunctions.Logit(_factor);
            _sigma = (UtilityFunctions.Logit(ProcessingFactor.Upper.Value) - _mu) / 1.645;
            if (ProcessingFactor.NominalUncertaintyUpper != null) {
                _uncertaintyMu = (UtilityFunctions.Logit(ProcessingFactor.NominalUncertaintyUpper.Value) - _mu) / 1.645;
                if (ProcessingFactor.UpperUncertaintyUpper != null) {
                    _degreesOfFreedom = StatisticalTests.GetDegreesOfFreedom(_factor, ProcessingFactor.Upper.Value, ProcessingFactor.NominalUncertaintyUpper.Value, ProcessingFactor.UpperUncertaintyUpper.Value, true);
                }
            }
        }
        public override bool GetApplyProcessingCorrectionFactor() {
            return true;
        }
        public override double GetNominalValue() {
            return _factor;
        }

        public override double DrawFromDistribution(IRandom random) {
            var factor = NormalDistribution.DrawInvCdf(random, Mu, Sigma);
            return UtilityFunctions.ILogit(factor);
        }

        public override void Resample(IRandom random) {
            _isModellingUncertainty = true;
            if (_uncertaintyMu.HasValue && _degreesOfFreedom != null) {
                _muDrawn = NormalDistribution.DrawInvCdf(random, _mu, _uncertaintyMu.Value);
                _sigmaDrawn = _sigma * Math.Sqrt(_degreesOfFreedom.Value / ChiSquaredDistribution.InvCDF(_degreesOfFreedom.Value, random.NextDouble()));
            }
        }

        public override void ResetNominal() {
            _isModellingUncertainty = false;
            _muDrawn = null;
            _sigmaDrawn = null;
        }
    }
}
