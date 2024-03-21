using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;

namespace MCRA.Simulation.Calculators.KineticModelCalculation.PbpkModelCalculation {
    public sealed class SubstanceTargetExposurePattern : ISubstanceTargetExposure {

        public SubstanceTargetExposurePattern() { }

        /// <summary>
        /// The substance.
        /// </summary>
        public Compound Substance { get; set; }

        public string Compartment { get; set; }

        // NOTE: the relative compartment weight was added when the SBML version of the kinetic models was introduced.
        //       SBML models produce the relative compartment weight as calculation output, whereas the legacy desolve model
        //       needs the relative compartment weight as input. This property has been put here as a shortcut/workaround, but
        //       should be moved to a higher level in the call hierarchy.
        public double RelativeCompartmentWeight { get; set; }

        /// <summary>
        /// The exposure type (acute/chronic).
        /// </summary>
        public ExposureType ExposureType { get; set; }

        public TargetUnit Unit { get; set; }

        /// <summary>
        /// The target exposure per time bin.
        /// </summary>
        public List<TargetExposurePerTimeUnit> TargetExposuresPerTimeUnit { get; set; }

        /// <summary>
        /// Non-stationary period.
        /// </summary>
        public int NonStationaryPeriod { get; set; }

        /// <summary>
        /// Time unit multiplication factor.
        /// </summary>
        public int TimeUnitMultiplier { get; set; }

        /// <summary>
        /// The computed average target exposure = internal dose
        /// </summary>
        public double SteadyStateTargetExposure {
            get {
                var period = 0;
                if (NonStationaryPeriod < TargetExposuresPerTimeUnit.Count) {
                    period = NonStationaryPeriod;
                }

                // Computed as the mean of internal doses (at target)
                return TargetExposuresPerTimeUnit.Skip(period).Average(r => r.Exposure) + OtherRouteExposures;
            }
        }

        /// <summary>
        /// The computed peak target exposure = internal dose
        /// </summary>
        public double PeakTargetExposure {
            get {
                var period = 0;
                if (NonStationaryPeriod < TargetExposuresPerTimeUnit.Count - 1) {
                    period = NonStationaryPeriod;
                }
                var stationaryTargetExposures = TargetExposuresPerTimeUnit.Skip(period).Select(r => r.Exposure).ToList();
                var n = stationaryTargetExposures.Count / TimeUnitMultiplier;
                if (n < 1) {
                    n = 1;
                }
                var peaks = new List<double>();
                for (int i = 0; i < n; i++) {
                    var bag = stationaryTargetExposures.Skip(i * TimeUnitMultiplier).Take(TimeUnitMultiplier).ToList();
                    peaks.Add(bag.Max());
                }
                return peaks.Average() + OtherRouteExposures;
            }
        }

        /// <summary>
        /// Contains exposures for routes that are not in the kinetic model
        /// </summary>
        public double OtherRouteExposures { get; set; }

        /// <summary>
        /// Maximum of internal doses (at target).
        /// </summary>
        public double MaximumTargetExposure {
            get {
                return TargetExposuresPerTimeUnit.Max(r => r.Exposure) + OtherRouteExposures;
            }
        }

        /// <summary>
        /// Implementation of exposure: should be peak for acute and steady state for chronic.
        /// </summary>
        public double SubstanceAmount {
            get {
                if (ExposureType == ExposureType.Acute) {
                    return PeakTargetExposure;
                }
                return SteadyStateTargetExposure;
            }
        }

        public double EquivalentSubstanceAmount(double rpf, double membershipProbability) {
            return SubstanceAmount * rpf * membershipProbability;
        }
    }
}
