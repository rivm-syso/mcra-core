using MCRA.Utils;
using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Calculators.ProcessingFactorCalculation {
    /// <summary>
    /// Distribution based processing factors using the lognormal (0, infinity),
    /// specified by a nominal and upper value.
    /// </summary>
    public sealed class PFLogNormalModel : ProcessingFactorModel, IDistributionProcessingFactorModel {

        private double _factor;
        private double _mu;
        private double _sigma;

        private double? _uncertaintyMu;
        private double? _degreesOfFreedom;

        private double? _muDrawn;
        private double? _sigmaDrawn;

        public ProcessingDistributionType DistributionType {
            get {
                return ProcessingDistributionType.LogNormal;
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
            _factor = pf.Nominal;
            _mu = UtilityFunctions.LogBound(_factor);
            _sigma = (UtilityFunctions.LogBound((double)pf.Upper) - _mu) / 1.645;
            if (pf.NominalUncertaintyUpper != null) {
                _uncertaintyMu = (UtilityFunctions.LogBound(pf.NominalUncertaintyUpper.Value) - _mu) / 1.645;
            }
            if (pf.UpperUncertaintyUpper != null) {
                _degreesOfFreedom = StatisticalTests.GetDegreesOfFreedom(_factor, pf.Upper.Value, pf.NominalUncertaintyUpper.Value, pf.UpperUncertaintyUpper.Value, false);
            }
        }

        public override (double, bool) GetNominalValue() {
            return (_factor, true);
        }

        public override (double, bool) DrawFromDistribution(IRandom random) {
            var factor = NormalDistribution.DrawInvCdf(random, Mu, Sigma);
            return (UtilityFunctions.ExpBound(factor), true);
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
