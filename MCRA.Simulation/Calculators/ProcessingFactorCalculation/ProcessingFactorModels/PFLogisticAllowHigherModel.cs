using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Utils;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.ProcessingFactorCalculation.ProcessingFactorModels {
    /// <summary>
    /// Distribution based processing factors using the logistic normal (0, 1),
    /// specified by a nominal and upper value, only pf > 1
    /// </summary>
    public sealed class PFLogisticAllowHigherModel(ProcessingFactor processingFactor)
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
            _factor = ProcessingFactor.Nominal < 1 ? 1 : ProcessingFactor.Nominal;
            _mu = UtilityFunctions.Logit(_factor);
            var pfUpper = ProcessingFactor.Upper < _factor ? _factor : ProcessingFactor.Upper.Value;
            var logUpper = UtilityFunctions.Logit(pfUpper);
            _sigma = (logUpper - _mu) / 1.645;
            if (ProcessingFactor.NominalUncertaintyUpper != null) {
                var nominalUncertainty = ProcessingFactor.NominalUncertaintyUpper < _factor ? _factor : ProcessingFactor.NominalUncertaintyUpper.Value;
                _uncertaintyMu = (UtilityFunctions.Logit(nominalUncertainty) - _mu) / 1.645;
                if (ProcessingFactor.UpperUncertaintyUpper != null) {
                    var upperUncertainty = ProcessingFactor.UpperUncertaintyUpper < pfUpper ? pfUpper : ProcessingFactor.UpperUncertaintyUpper.Value;
                    _degreesOfFreedom = StatisticalTests.GetDegreesOfFreedom(_factor, pfUpper, nominalUncertainty, upperUncertainty, false);
                }
            }
        }

        public override double GetNominalValue() {
            return _factor;
        }

        public override double DrawFromDistribution(IRandom random) {
            var factor = UtilityFunctions.ILogit(Sigma * NormalDistribution.InvCDF(0, 1, random.NextDouble()) + Mu);
            return factor > 1
                ? factor
                : 1D;
        }

        public override void Resample(IRandom random) {
            _isModellingUncertainty = true;
            if (_uncertaintyMu != null && _degreesOfFreedom != null) {
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
