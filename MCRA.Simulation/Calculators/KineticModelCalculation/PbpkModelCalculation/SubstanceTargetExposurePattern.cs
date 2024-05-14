using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.KineticModelCalculation.PbpkModelCalculation {
    public sealed class SubstanceTargetExposurePattern : ISubstanceTargetExposure {

        public Compound Substance { get; set; }

        public string Compartment { get; set; }

        public ExposureTarget Target { get; set; }

        /// <summary>
        /// The relative weight of the compartment (compared to the bodyweight).
        /// TODO: currently the compartment weight is a single value and the assumption
        /// is that the compartment weight does not change over the time course of simulation.
        /// This needs to be changed in the future when working with PBK simulations spanning
        /// multiple years (e.g., from childhood to adulthood) or varying compartment sizes
        /// (e.g., urine).
        /// </summary>
        public double RelativeCompartmentWeight { get; set; }

        /// <summary>
        /// The exposure type (acute/chronic).
        /// </summary>
        public ExposureType ExposureType { get; set; }

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
                if (TargetExposuresPerTimeUnit.Any()) {
                    var period = 0;
                    if (NonStationaryPeriod < TargetExposuresPerTimeUnit.Count) {
                        period = NonStationaryPeriod;
                    }

                    // Computed as the mean of internal doses (at target)
                    return TargetExposuresPerTimeUnit
                        .Skip(period)
                        .Average(r => r.Exposure);
                } else {
                    return 0;
                }
            }
        }

        /// <summary>
        /// The computed peak target exposure = internal dose
        /// </summary>
        public double PeakTargetExposure {
            get {
                if (TargetExposuresPerTimeUnit.Any()) {
                    var period = 0;
                    if (NonStationaryPeriod < TargetExposuresPerTimeUnit.Count - 1) {
                        period = NonStationaryPeriod;
                    }
                    var stationaryTargetExposures = TargetExposuresPerTimeUnit
                        .Skip(period)
                        .Select(r => r.Exposure)
                        .ToList();
                    var n = stationaryTargetExposures.Count / TimeUnitMultiplier;
                    if (n < 1) {
                        n = 1;
                    }
                    var peaks = new List<double>();
                    for (int i = 0; i < n; i++) {
                        var bag = stationaryTargetExposures
                            .Skip(i * TimeUnitMultiplier)
                            .Take(TimeUnitMultiplier)
                            .ToList();
                        peaks.Add(bag.Max());
                    }
                    return peaks.Average();
                } else {
                    return 0;
                }
            }
        }

        /// <summary>
        /// Maximum of internal doses (at target).
        /// </summary>
        public double MaximumTargetExposure {
            get {
                return TargetExposuresPerTimeUnit.Any()
                    ? TargetExposuresPerTimeUnit.Max(r => r.Exposure)
                    : 0D;
            }
        }

        /// <summary>
        /// Implementation of exposure: should be peak for acute and steady state for chronic.
        /// </summary>
        public double Exposure {
            get {
                if (ExposureType == ExposureType.Acute) {
                    return PeakTargetExposure;
                }
                return SteadyStateTargetExposure;
            }
        }

        public double EquivalentSubstanceExposure(double rpf, double membershipProbability) {
            return Exposure * rpf * membershipProbability;
        }
    }
}
