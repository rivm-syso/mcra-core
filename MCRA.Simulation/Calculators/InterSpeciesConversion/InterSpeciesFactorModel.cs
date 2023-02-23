using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Calculators.InterSpeciesConversion {
    public sealed class InterSpeciesFactorModel {

        private double _geometricMean;
        private double _uncertaintyFactor = 1;
        
        public InterSpeciesFactor InterSpeciesFactor { get; set; }

        public InterSpeciesFactorModel(InterSpeciesFactor interSpeciesFactor) {
            InterSpeciesFactor = interSpeciesFactor;
            GeometricMean = interSpeciesFactor.InterSpeciesFactorGeometricMean;
        }

        public double GeometricMean {
            get { return _geometricMean * _uncertaintyFactor; }
            set { _geometricMean = value; }
        }

        /// <summary>
        /// Turns on the uncertainty modelling for interspecies factor with a given random number generator
        /// </summary>
        /// <param name="random">A Troschuetz random number generator</param>
        public void ResampleUncertainty(double draw) {
            _uncertaintyFactor = Math.Exp(Math.Log(InterSpeciesFactor.InterSpeciesFactorGeometricStandardDeviation) * NormalDistribution.InvCDF(0, 1, draw));
        }

        /// <summary>
        /// Turns off the uncertainty modelling, resets the geometric mean
        /// </summary>
        public void StopModelingUncertainty() {
            _uncertaintyFactor = 1;
        }
    }
}
