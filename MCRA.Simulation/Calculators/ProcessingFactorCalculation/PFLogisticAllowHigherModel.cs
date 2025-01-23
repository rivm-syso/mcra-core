using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Utils;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.ProcessingFactorCalculation {
    /// <summary>
    /// Distribution based processing factors using the logistic normal (0, 1),
    /// specified by a nominal and upper value, only pf > 1
    /// </summary>
    public sealed class PFLogisticAllowHigherModel : ProcessingFactorModel, IDistributionProcessingFactorModel {

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

        public override void CalculateParameters(ProcessingFactor pf) {
            _factor = pf.Nominal < 1 ? 1 : pf.Nominal;
            _mu = UtilityFunctions.Logit(_factor);
            var pfUpper = pf.Upper < _factor ? _factor : pf.Upper.Value;
            var logUpper = UtilityFunctions.Logit(pfUpper);
            _sigma = (logUpper - _mu) / 1.645;
            if (pf.NominalUncertaintyUpper != null) {
                var nominalUncertainty = pf.NominalUncertaintyUpper < _factor ? _factor : pf.NominalUncertaintyUpper.Value;
                _uncertaintyMu = (UtilityFunctions.Logit(nominalUncertainty) - _mu) / 1.645;
                if (pf.UpperUncertaintyUpper != null) {
                    var upperUncertainty = pf.UpperUncertaintyUpper < pfUpper ? pfUpper : pf.UpperUncertaintyUpper.Value;
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
