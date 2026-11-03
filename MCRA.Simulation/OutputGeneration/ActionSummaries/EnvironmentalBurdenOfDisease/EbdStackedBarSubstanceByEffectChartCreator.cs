using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration.ActionSummaries.EnvironmentalBurdenOfDisease {
    public class EbdStackedBarSubstanceByEffectChartCreator : ReportStackedBarChartWithErrorsCreatorBase {

        private string _sectionId;
        private readonly List<EnvironmentalBurdenOfDiseaseSummaryRecord> _records;
        private readonly bool _isUncertainty;
        public override string Title => "Total attributable burden by substance and effect.";

        public EbdStackedBarSubstanceByEffectChartCreator(
            string sectionId,
            List<EnvironmentalBurdenOfDiseaseSummaryRecord> records,
            bool isUncertainty
        ) {
            _records = records;
            _sectionId = sectionId;
            _isUncertainty = isUncertainty;
            Height = 150 + _records.Select(r => r.SubstanceCode).Distinct().Count();
            Width = 500;
        }

        public override string ChartId {
            get {
                var pictureId = "6b96ab4b-cb18-496d-924b-4bf471b9cac0";
                return StringExtensions.CreateFingerprint(_sectionId + pictureId);
            }
        }

        public override PlotModel Create() {
            var hasPopulationSize = !double.IsNaN(_records.FirstOrDefault().PopulationSize);
            var allSubstances = _records
                .Select(r => r.SubstanceCode)
                .Distinct()
                .ToList();
            var stackedColumnWithErrorItems = new List<StackedColumnWithErrorItem>();
            foreach (var substance in allSubstances) {
                var substanceRecords = _records
                    .Where(r => r.SubstanceCode == substance);
                var subCategories = substanceRecords
                    .ToDictionary(
                        r => r.EffectName,
                        r => _isUncertainty
                            ? (hasPopulationSize
                                ? r.MedianStandardisedTotalAttributableBod
                                : r.MedianTotalAttributableBod
                            )
                            : (hasPopulationSize
                                ? r.StandardisedTotalAttributableBod
                                : r.TotalAttributableBod)
                    );

                var value = subCategories.Sum(r => r.Value);
                var lower = _isUncertainty
                    ? substanceRecords.Sum(r => hasPopulationSize
                        ? r.LowerStandardisedTotalAttributableBod
                        : r.LowerTotalAttributableBod
                    )
                    : double.NaN;
                var upper = _isUncertainty
                    ? substanceRecords.Sum(r => hasPopulationSize
                        ? r.UpperStandardisedTotalAttributableBod
                        : r.UpperTotalAttributableBod
                    )
                    : double.NaN;

                var stackedColumnWithErrorItem = new StackedColumnWithErrorItem() {
                    Label = substanceRecords.FirstOrDefault().SubstanceName,
                    Value = value,
                    Lower = lower,
                    Upper = upper,
                    Subcategories = subCategories
                };
                stackedColumnWithErrorItems.Add(stackedColumnWithErrorItem);
            }
            return create(stackedColumnWithErrorItems, uncertainty: _isUncertainty);
        }
    }
}

