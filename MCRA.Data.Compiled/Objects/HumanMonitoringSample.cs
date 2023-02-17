using System;
using System.Collections.Generic;

namespace MCRA.Data.Compiled.Objects {
    public sealed class HumanMonitoringSample {
        public HumanMonitoringSample() {
            SampleAnalyses = new List<SampleAnalysis>();
        }

        public string Code { get; set; }
        public Individual Individual { get; set; }
        public DateTime? DateSampling { get; set; }
        public string DayOfSurvey { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string TimeOfSampling { get; set; }
        public double? SpecificGravity { get; set; }
        public double? SpecificGravityCorrectionFactor { get; set; }

        public HumanMonitoringSamplingMethod SamplingMethod { get; set; }
        public ICollection<SampleAnalysis> SampleAnalyses { get; set; }

        public string ExposureRoute {
            get {
                return SamplingMethod.ExposureRoute;
            }
        }

        public string Compartment {
            get {
                return SamplingMethod.BiologicalMatrixCode;
            }
        }

        public override string ToString() {
            return $"[{GetHashCode():X8}] {Code}";
        }
    }
}
