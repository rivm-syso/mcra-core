using MCRA.General;

namespace MCRA.Data.Raw.Objects.RawTableObjects {
    [RawDataSourceTableID(RawDataSourceTableID.HumanMonitoringSamples)]
    public sealed class RawHumanMonitoringSample : IRawDataTableRecord {
        public string idSample { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string idIndividual { get; set; }
        public DateTime? DateSampling { get; set; }
        public string DayOfSurvey { get; set; }
        public string TimeOfSampling { get; set; }
        public string SampleType { get; set; }
        public string Compartment { get; set; }
        public string ExposureRoute { get; set; }
        public double? SpecificGravity { get; set; }
        public double? SpecificGravityCorrectionFactor { get; set; }
        public double? LipidEnz { get; set; }
        public double? LipidGrav { get; set; }
        public double? Cholesterol { get; set; }
        public double? Triglycerides { get; set; }
        public double? Creatinine { get; set; }
        public double? OsmoticConcentration { get; set; }
        public double? UrineVolume { get; set; }
    }
}
