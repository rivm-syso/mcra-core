namespace MCRA.Simulation.OutputGeneration {
    public class AcutePerPortionPerSubstanceDrilldownSection(
        OverallIndividualDayDrillDownRecord drilldownRecord,
        int drilldownIndex,
        bool isCumulative,
        bool isProcessing,
        bool isUnitVariability,
        string referenceCompoundName
    ) : AcuteDrilldownSectionBase(
        drilldownIndex,
        drilldownRecord,
        isCumulative,
        isProcessing,
        isUnitVariability,
        referenceCompoundName
    ) {
        public List<DetailedIndividualDayDrilldownRecord> DetailedIndividualDayDrillDownRecords { get; set; } = [];

        public void Summarize(
            IEnumerable<DietaryAcuteIntakePerFoodRecord> dietaryAcuteFoodRecords,
            double dietaryIntakePerBodyWeight
        ) {
            foreach (var ipf in dietaryAcuteFoodRecords) {
                var showRpf = dietaryAcuteFoodRecords.Any(r => r.AcuteIntakePerCompoundRecords.Any(ipc => !double.IsNaN(ipc.Rpf) && ipc.Rpf != 1D));
                var intakesPerCompounds = ipf.AcuteIntakePerCompoundRecords.Where(i => double.IsNaN(i.Concentration) || i.Concentration > 0);
                foreach (var ipc in intakesPerCompounds) {
                    foreach (var portion in ipc.UnitVariabilityPortions) {
                        var exposure = portion.Amount * portion.Concentration * ipc.ProcessingFactor / DrilldownRecord.BodyWeight / ipc.ProportionProcessing;
                        var detailedIndividualDrilldownRecord = new DetailedIndividualDayDrilldownRecord() {
                            FoodAsEaten = ipf.FoodAsEatenName,
                            Amount = ipf.FoodAsEatenAmount,
                            ModelledFood = ipf.FoodAsMeasuredName,
                            ConversionFactor = ipf.Translation,
                            PortionAmount = portion.Amount,
                            UnitWeight = ipc.UnitWeight,
                            UnitsInCompositeSample = ipc.UnitsInCompositeSample,
                            Substance = ipc.CompoundName,
                            ConcentrationInSample = ipc.Concentration,
                            VariabilityFactor = ipc.UnitVariabilityFactor,
                            StochasticVf = portion.Concentration / ipc.Concentration,
                            ConcentrationInPortionUV = portion.Concentration,
                            ProcessingFactor = ipc.ProcessingFactor,
                            ProcessingCorrectionFactor = ipc.ProportionProcessing,
                            ProcessingTypeDescription = ipc.ProcessingTypeDescription,
                            BatchProcessing = ipc.ProcessingTypeDescription,
                            Exposure = double.IsNaN(portion.Amount)
                                ? ipc.Intake
                                : exposure,
                            Rpf = ipc.Rpf,
                            EquivalentExposure = showRpf && !double.IsNaN(portion.Amount)
                                ? ipc.Rpf * exposure
                                : (showRpf ? ipc.Intake : double.NaN),
                            Percentage = showRpf && !double.IsNaN(portion.Amount)
                                ? ipc.Rpf * exposure / dietaryIntakePerBodyWeight
                                : (showRpf ? ipc.Intake / dietaryIntakePerBodyWeight : double.NaN)
                        };
                        DetailedIndividualDayDrillDownRecords.Add(detailedIndividualDrilldownRecord);
                    }
                }
            }
        }
    }
}
