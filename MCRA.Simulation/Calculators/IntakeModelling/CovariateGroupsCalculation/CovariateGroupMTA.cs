using MCRA.Data.Compiled.Objects;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.Calculators.IntakeModelling {

    public sealed class CovariateGroupMTA {

        private List<double> _sumOfModelBasedIntakes;

        public Individual Individual { get; set; }

        /// <summary>
        /// The id of the covariate group.
        /// </summary>
        public int SimulatedIndividualId { get; set; }

        /// <summary>
        /// Covariate group for which the amounts are calculated.
        /// </summary>
        public CovariateGroup CovariateGroup { get; set; }

        /// <summary>
        /// Added usual exposures for OIM exposure models
        /// </summary>
        public double ObservedIndividualMeanRemainingCategory { get; set; }

        /// <summary>
        /// Collector for usual exposures for BNN/LNN0 exposure models
        /// </summary>
        public List<ModelBasedIntakeResult> ModelBasedIntakeResults { get; set; }

        /// <summary>
        /// Added usual exposures for BNN/LNN0 exposure models and OIM exposure model
        /// </summary>
        public List<double> ModelBasedIntakes {
            get {
                if (_sumOfModelBasedIntakes == null) {
                    // aggregate the rows of all available lists and add oim amount
                    _sumOfModelBasedIntakes = new List<double>();
                    if (ModelBasedIntakeResults.Count > 0) {
                        var ui = ModelBasedIntakeResults.Select(c => c.ModelBasedIntakes).ToList();
                        for (int ix = 0; ix < ui.First().Count; ix++) {
                            _sumOfModelBasedIntakes.Add((ui.Sum(c => c.ElementAt(ix)) + ObservedIndividualMeanRemainingCategory));
                        }
                    } else {
                        //only OIM
                        _sumOfModelBasedIntakes.Add(ObservedIndividualMeanRemainingCategory);
                    }
                }
                return _sumOfModelBasedIntakes;
            }
        }
    }
}
