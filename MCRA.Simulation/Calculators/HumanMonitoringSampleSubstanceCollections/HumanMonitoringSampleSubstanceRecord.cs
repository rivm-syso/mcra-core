using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;

namespace MCRA.Simulation.Calculators.HumanMonitoringSampleCompoundCollections {

    /// <summary>
    /// Contains information about a sample, such as the analytical method
    /// used for the sample, the concentration information per substance, and
    /// the (imputed) relative potency.
    /// </summary>
    public sealed class HumanMonitoringSampleSubstanceRecord {

        /// <summary>
        /// The monitoring sample.
        /// </summary>
        public HumanMonitoringSample HumanMonitoringSample { get; set; }

        /// <summary>
        /// The sample information per substance.
        /// </summary>
        public Dictionary<Compound, SampleCompound> HumanMonitoringSampleSubstances { get; set; }

        public int SimulatedIndividualId { get; set; }

        /// <summary>
        /// The individual.
        /// </summary>
        public Individual Individual {
            get {
                return HumanMonitoringSample.Individual;
            }
        }

        /// <summary>
        /// The survey day.
        /// </summary>
        public string Day {
            get {
                return HumanMonitoringSample.DayOfSurvey;
            }
        }

        /// <summary>
        /// The sampling method.
        /// </summary>
        public HumanMonitoringSamplingMethod SamplingMethod {
            get {
                return HumanMonitoringSample.SamplingMethod;
            }
        }

        /// <summary>
        /// The specific gravity correction factor.
        /// </summary>
        public double? SpecificGravityCorrectionFactor {
            get {
                return HumanMonitoringSample.SpecificGravityCorrectionFactor;
            }
        }

        /// <summary>
        /// Returns a copy/clone of the sample compound record.
        /// </summary>
        /// <returns></returns>
        public HumanMonitoringSampleSubstanceRecord Clone() {
            return new HumanMonitoringSampleSubstanceRecord() {
                HumanMonitoringSampleSubstances = HumanMonitoringSampleSubstances.Values
                    .ToDictionary(sc => sc.ActiveSubstance, sc => sc.Clone()),
                HumanMonitoringSample = this.HumanMonitoringSample,
            };
        }
    }
}
