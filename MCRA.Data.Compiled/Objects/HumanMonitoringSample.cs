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
        /// <summary>
        /// Lipids concentration in sample measured by gravimetric analysis, unit see HumanMonitoringSurvey.LipidConcentrationUnitString, default in mg/dL
        /// </summary>
        public double? LipidGrav { get; set; }
        /// <summary>
        /// Lipids concentration in sample measured by enzymatic summation, unit see HumanMonitoringSurvey.LipidConcentrationUnitString, default in mg/dL
        /// </summary>
        public double? LipidEnz { get; set; }
        /// <summary>
        /// Triglycerides concentration in sample, unit see HumanMonitoringSurvey.TriglycConcentrationUnitString, default in mg/dL
        /// </summary>
        public double? Triglycerides { get; set; }
        /// <summary>
        /// Cholesterol concentration in sample, unit see HumanMonitoringSurvey.CholestConcentrationUnitString, default in mg/dL
        /// </summary>
        public double? Cholesterol { get; set; }
        /// <summary>
        /// Creatinine concentration in sample, unit see HumanMonitoringSurvey.CreatConcentrationUnitString, default in mg/dL
        /// </summary>
        public double? Creatinine { get; set; }
        public double? OsmoticConcentration { get; set; }
        public double? UrineVolume { get; set; }
        public HumanMonitoringSamplingMethod SamplingMethod { get; set; }
        public ICollection<SampleAnalysis> SampleAnalyses { get; set; }

        public override string ToString() {
            return $"[{GetHashCode():X8}] {Code}";
        }
    }
}
