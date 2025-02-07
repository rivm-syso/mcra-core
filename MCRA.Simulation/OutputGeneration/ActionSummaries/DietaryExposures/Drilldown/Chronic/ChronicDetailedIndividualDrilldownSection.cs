
namespace MCRA.Simulation.OutputGeneration {
    public class ChronicDetailedIndividualDrilldownSection(
        int drilldownIndex,
        OverallIndividualDrillDownRecord drilldownRecord,
        bool isCumulative,
        bool isProcessing,
        string referenceCompoundName,
        double bodyWeight,
        double individualMean
    ) : ChronicDrilldownSectionBase(
        drilldownRecord,
        drilldownIndex,
        isCumulative,
        isProcessing,
        referenceCompoundName,
        bodyWeight
    ) {
        public List<DetailedIndividualDrillDownRecord> DetailedIndividualDrillDownRecords { get; } = [];
        public double IndividualMean { get; } = individualMean;

        public void Summarize(IList<DietaryDayDrillDownRecord> drillDownRecords) {
            var showRpf = drillDownRecords.Any(r => r.ChronicIntakePerFoodRecords
                .Any(ipf => ipf.ChronicIntakePerCompoundRecords
                    .Any(ipc => !double.IsNaN(ipc.Rpf) && ipc.Rpf != 1D)));
            foreach (var dayDrillDown in drillDownRecords) {
                var numberOfDays = drillDownRecords.Count;
                var detailedIntakePerFoodRecord = dayDrillDown.ChronicIntakePerFoodRecords;
                foreach (var ipf in detailedIntakePerFoodRecord) {
                    foreach (var ipc in ipf.ChronicIntakePerCompoundRecords) {
                        if (ipc.Concentration > 0 || double.IsNaN(ipc.Concentration)) {
                            var exposure = ipf.FoodAsMeasuredAmount * ipc.Concentration * ipc.ProcessingFactor / BodyWeight / ipc.ProportionProcessing;
                            var detailedIndividualDrilldownRecord = new DetailedIndividualDrillDownRecord() {
                                Day = dayDrillDown.Day,
                                FoodAsEaten = ipf.FoodAsEatenName,
                                Amount = ipf.FoodAsEatenAmount,
                                ModelledFood = ipf.FoodAsMeasuredName,
                                ConversionFactor = ipf.Translation,
                                PortionAmount = ipf.FoodAsMeasuredAmount,
                                Substance = ipc.CompoundName,
                                ConcentrationInSample = ipc.Concentration,
                                ProcessingFactor = ipc.ProcessingFactor,
                                ProcessingCorrectionFactor = ipc.ProportionProcessing,
                                Exposure = double.IsNaN(ipf.FoodAsMeasuredAmount)
                                    ? ipc.Intake
                                    : exposure,
                                Rpf = ipc.Rpf,
                                EquivalentExposure = showRpf && !double.IsNaN(ipf.FoodAsMeasuredAmount)
                                    ? ipc.Rpf * exposure
                                    : (showRpf ? ipc.Intake : double.NaN),
                                Percentage = showRpf && !double.IsNaN(ipf.FoodAsMeasuredAmount)
                                    ? ipc.Rpf * exposure / IndividualMean / numberOfDays
                                    : (showRpf ? ipc.Intake / IndividualMean / numberOfDays : double.NaN)
                            };
                            DetailedIndividualDrillDownRecords.Add(detailedIndividualDrilldownRecord);
                        }
                    }
                }
            }
        }
    }
}
