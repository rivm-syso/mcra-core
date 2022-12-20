using MCRA.Data.Compiled.Objects;
using System.Collections.Generic;

namespace MCRA.Simulation.Calculators.HumanMonitoringSampleCompoundCollections {

    /// <summary>
    /// This class holds a collection of sample compound records for a specified food.
    /// </summary>
    public sealed class HumanMonitoringSampleSubstanceCollection {

        /// <summary>
        /// The sampling method.
        /// </summary>
        public HumanMonitoringSamplingMethod SamplingMethod { get; set; }

        /// <summary>
        /// The sample substance records.
        /// </summary>
        public List<HumanMonitoringSampleSubstanceRecord> HumanMonitoringSampleSubstanceRecords { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="HumanMonitoringSampleSubstanceCollection" /> class.
        /// </summary>
        /// <param name="hbmSamplingMethod"></param>
        /// <param name="hbmSampleSubstanceRecords"></param>
        public HumanMonitoringSampleSubstanceCollection(
            HumanMonitoringSamplingMethod hbmSamplingMethod,
            List<HumanMonitoringSampleSubstanceRecord> hbmSampleSubstanceRecords
        ) {
            HumanMonitoringSampleSubstanceRecords = hbmSampleSubstanceRecords;
            SamplingMethod = hbmSamplingMethod;
        }
    }
}
