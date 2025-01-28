using MCRA.Utils;
using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Calculators.ProcessingFactorCalculation.ProcessingFactorModels {
    /// <summary>
    /// Distribution based processing factors using the lognormal (0, infinity),
    /// specified by a nominal and upper value, only pf > 1
    /// </summary>
    public sealed class PFLogNormalAllowHigherModel(ProcessingFactor processingFactor)
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

        public override void CalculateParameters() {
            _factor = ProcessingFactor.Nominal < 1 ? 1 : ProcessingFactor.Nominal;
            _mu = UtilityFunctions.LogBound(_factor);
            var pfUpper = ProcessingFactor.Upper < _factor ? _factor : ProcessingFactor.Upper.Value;
            _sigma = (UtilityFunctions.LogBound(pfUpper) - _mu) / 1.645;
            if (ProcessingFactor.NominalUncertaintyUpper != null) {
                var nominalUncertainty = ProcessingFactor.NominalUncertaintyUpper < _factor ? _factor : ProcessingFactor.NominalUncertaintyUpper.Value;
                _uncertaintyMu = (UtilityFunctions.LogBound(nominalUncertainty) - _mu) / 1.645;
            }
            if (ProcessingFactor.UpperUncertaintyUpper != null) {
                var nominalUncertainty = ProcessingFactor.NominalUncertaintyUpper < _factor ? _factor : ProcessingFactor.NominalUncertaintyUpper.Value;
                var upperUncertainty = ProcessingFactor.UpperUncertaintyUpper < pfUpper ? pfUpper : ProcessingFactor.UpperUncertaintyUpper.Value;
                _degreesOfFreedom = StatisticalTests.GetDegreesOfFreedom(_factor, pfUpper, nominalUncertainty, upperUncertainty, false);
            }
        }

        public override double GetNominalValue() {
            return _factor;
        }

        public override double DrawFromDistribution(IRandom random) {
            var factor = UtilityFunctions.ExpBound(NormalDistribution.DrawInvCdf(random, Mu, Sigma));
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
