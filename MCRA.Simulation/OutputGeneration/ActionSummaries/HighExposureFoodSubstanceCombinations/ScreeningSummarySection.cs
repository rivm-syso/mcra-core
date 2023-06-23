using MCRA.Data.Compiled.Wrappers;
using System.ComponentModel;

namespace MCRA.Simulation.OutputGeneration {

    [DisplayName("Dietary exposure screening")]
    public sealed class ScreeningSummarySection : SummarySection {

        public double CumulativeSelectionPercentage { get; set; }
        public double CriticalExposurePercentage { get; set; }
        public double ImportanceFactorLor { get; set; }

        public ScreeningSummaryRecord RiskDriver { get; set; }
        public List<RestrictedSummaryRecord> GroupedScreeningSummaryRecords { get; set; }
        public List<ScreeningSummaryRecord> ScreeningSummaryRecords { get; set; }

        public void Summarize(ScreeningResult screeningResult, double criticalExposurePercentage, double cumulativeSelectionPercentage, double importanceLor) {
            CumulativeSelectionPercentage = cumulativeSelectionPercentage;
            CriticalExposurePercentage = criticalExposurePercentage;
            ImportanceFactorLor = importanceLor;

            GroupedScreeningSummaryRecords = screeningResult.ScreeningResultsPerFoodCompound
                .Select(c => new RestrictedSummaryRecord() {
                    CompoundName = c.Compound.Name,
                    CompoundCode = c.Compound.Code,
                    FoodAsMeasuredName = c.FoodAsMeasured.Name,
                    FoodAsMeasuredCode = c.FoodAsMeasured.Code,
                    NumberOfFoods = c.ScreeningRecords.Count,
                    Contribution = c.Contribution * 100D,
                    CumulativeContributionFraction = c.CumulativeContributionFraction * 100D,
                }).OrderByDescending(o => o.Contribution)
                .ToList();

            var sumOthers = 100 - 100 * screeningResult.ScreeningResultsPerFoodCompound.Sum(r => r.Contribution);
            sumOthers = sumOthers < 0 ? 0 : sumOthers;

            if (screeningResult.SelectedNumberOfSccRecords != screeningResult.TotalNumberOfSccRecords) {
                GroupedScreeningSummaryRecords.Add(new RestrictedSummaryRecord() {
                    CompoundName = "Others",
                    CompoundCode = "Others",
                    FoodAsMeasuredName = "Others",
                    FoodAsMeasuredCode = "Others",
                    NumberOfFoods = screeningResult.TotalNumberOfSccRecords - screeningResult.SelectedNumberOfSccRecords,
                    Contribution = sumOthers,
                    CumulativeContributionFraction = 100,
                });
            }

            //re-scale percentages
            ScreeningSummaryRecords = screeningResult.ScreeningResultsPerFoodCompound
                .SelectMany(c => c.ScreeningRecords, (c, fcr) => new ScreeningSummaryRecord() {
                    CompoundCode = fcr.Compound.Code,
                    CompoundName = fcr.Compound.Name,
                    FoodAsEatenCode = fcr.FoodAsEaten.Code,
                    FoodAsEatenName = fcr.FoodAsEaten.Name,
                    FoodAsMeasuredCode = fcr.FoodAsMeasured.Code,
                    FoodAsMeasuredName = fcr.FoodAsMeasured.Name,
                    MuCensoredValues = fcr.CensoredValueParameters.Mu,
                    SigmaCensoredValues = fcr.CensoredValueParameters.Sigma,
                    FractionCensoredValues = fcr.CensoredValueParameters.Fraction,
                    MuDetects = fcr.DetectParameters.Mu,
                    SigmaDetects = fcr.DetectParameters.Sigma,
                    FractionDetects = fcr.DetectParameters.Fraction,
                    WeightCensoredValues = fcr.WeightCensoredValue,
                    WeightDetect = fcr.WeightDetect,
                    Exposure = fcr.Exposure,
                    Cup = fcr.Cup * 100,
                    CupPercentage = fcr.Cup / screeningResult.SumCupAllSccRecords * 100,
                    CumCupPercentage = fcr.CumCupFraction * 100,
                })
                .OrderByDescending(c => c.Cup)
                .ToList();

            //sumOthers = 1 - ScreeningSummaryRecords.Sum(r => r.CupPercentage);
            if (sumOthers > 0) {
                var othersString = "Others";
                ScreeningSummaryRecords.Add(new ScreeningSummaryRecord() {
                    CompoundCode = othersString,
                    CompoundName = othersString,
                    FoodAsEatenCode = othersString,
                    FoodAsEatenName = othersString,
                    FoodAsMeasuredCode = othersString,
                    FoodAsMeasuredName = othersString,
                    MuCensoredValues = double.NaN,
                    SigmaCensoredValues = double.NaN,
                    FractionCensoredValues = 0,
                    MuDetects = double.NaN,
                    SigmaDetects = double.NaN,
                    FractionDetects = double.NaN,
                    WeightCensoredValues = double.NaN,
                    WeightDetect = double.NaN,
                    Exposure = double.NaN,
                    Cup = double.NaN,
                    CupPercentage = sumOthers,
                    CumCupPercentage = 100,
                });
            }

            GroupedScreeningSummaryRecords.TrimExcess();
            ScreeningSummaryRecords.TrimExcess();

            RiskDriver = ScreeningSummaryRecords.OrderByDescending(c => c.Cup).First();
        }
    }
}
