using MCRA.Utils;
using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Calculators.ProcessingFactorCalculation {
    /// <summary>
    /// Distribution based processing factors using the lognormal (0, infinity),
    /// specified by a nominal and upper value, only pf > 1
    /// </summary>
    public sealed class PFLogNormalAllowHigherModel : ProcessingFactorModel, IDistributionProcessingFactorModel {

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
            _factor = pf.Nominal < 1 ? 1 : pf.Nominal;
            _mu = UtilityFunctions.LogBound(_factor);
            var pfUpper = pf.Upper < _factor ? _factor : (double) pf.Upper;
            _sigma = (UtilityFunctions.LogBound(pfUpper) - _mu) / 1.645;
            if (pf.NominalUncertaintyUpper != null) {
                var nominalUncertainty = pf.NominalUncertaintyUpper < _factor ? _factor : (double) pf.NominalUncertaintyUpper;
                _uncertaintyMu = (UtilityFunctions.LogBound(nominalUncertainty) - _mu) / 1.645;
            }
            if (pf.UpperUncertaintyUpper != null) {
                var nominalUncertainty = pf.NominalUncertaintyUpper < _factor ? _factor : (double)pf.NominalUncertaintyUpper;
                var upperUncertainty = pf.UpperUncertaintyUpper < pfUpper ? pfUpper : (double)pf.UpperUncertaintyUpper;
                _degreesOfFreedom = StatisticalTests.GetDegreesOfFreedom(_factor, pfUpper, nominalUncertainty, upperUncertainty, false);
            }
        }

        public override (double, bool) GetNominalValue() {
            return (_factor, _factor > 1);
        }

        public override (double, bool) DrawFromDistribution(IRandom random) {
            var factor = UtilityFunctions.ExpBound(NormalDistribution.DrawInvCdf(random, Mu, Sigma)); 
            return factor > 1
                ? (factor, true)
                : (1D, false);
        }

        public override void Resample(IRandom random) {
            _isModellingUncertainty = true;
            if (_uncertaintyMu != null && _degreesOfFreedom != null) {
                _muDrawn = NormalDistribution.DrawInvCdf(random, _mu, (double)_uncertaintyMu);
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
