using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace MCRA.Simulation.Calculators.IntakeModelling.ModelThenAddIntakeModelCalculation {

    /// <summary>
    /// A summary of the exposure of an individual, with the exposure amounts categorized
    /// in some fashion.
    /// </summary>
    [XmlRoot("CategorizedIndividualExposures")]
    public class CategorizedIndividualExposure {

        /// <summary>
        /// The id of the simulated individual.
        /// </summary>
        public int SimulatedIndividualId { get; set; }

        /// <summary>
        /// The sampling weight of the individual.
        /// </summary>
        public double SamplingWeight { get; set; }

        /// <summary>
        /// Collection of the category exposures.
        /// </summary>
        public List<CategoryExposure> CategoryExposures { get; set; }

        /// <summary>
        /// Amount (or OIM).
        /// </summary>
        public double TotalExposure {
            get {
                return CategoryExposures
                    .Select(a => double.IsNaN(a.Exposure) ? 0 : a.Exposure)
                    .Sum();
            }
        }
    }


}
