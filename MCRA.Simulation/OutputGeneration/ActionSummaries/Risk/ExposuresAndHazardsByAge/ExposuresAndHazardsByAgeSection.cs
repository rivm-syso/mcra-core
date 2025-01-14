using MCRA.General;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation;
using MCRA.Simulation.Calculators.RiskCalculation;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ExposuresAndHazardsByAgeSection : SummarySection {

        public bool IsCumulative { get; set; }
        public TargetUnit TargetUnit { get; set; }
        public double NominalHazardCharacterisationValue { get; set; }
        public List<ExposuresAndHazardsByAgeRecord> HazardExposureByAgeRecords { get; set; }
        public List<HCSubgroupPlotRecord> HazardCharacterisationRecords { get; set; }

        /// <summary>
        /// Summarizes exposures and hazards by age section.
        /// </summary>
        public void Summarize(
            List<IndividualEffect> individualEffects,
            RiskMetricType riskMetricType,
            TargetUnit targetUnit,
            IHazardCharacterisationModel referenceDose,
            bool isCumulative
        ) {
            var records = individualEffects
                .Select(r => new ExposuresAndHazardsByAgeRecord() {
                    Age = r.SimulatedIndividual.Age.Value,
                    Exposure = r.Exposure,
                    RiskRatio = riskMetricType == RiskMetricType.HazardExposureRatio
                        ? r.HazardExposureRatio
                        : r.ExposureHazardRatio
                })
                .ToList();
            IsCumulative = isCumulative;
            HazardExposureByAgeRecords = records;
            NominalHazardCharacterisationValue = referenceDose.Value;
            TargetUnit = targetUnit;
            HazardCharacterisationRecords = referenceDose.HCSubgroups?
                .Select(c => new HCSubgroupPlotRecord() {
                    HazardCharacterisationValue = c.Value,
                    Age = (double)c.AgeLower,
                    UncertaintyValues = c.HCSubgroupsUncertains?.Select(u => u.Value).ToList()
                })
                .ToList();
        }
    }
}
