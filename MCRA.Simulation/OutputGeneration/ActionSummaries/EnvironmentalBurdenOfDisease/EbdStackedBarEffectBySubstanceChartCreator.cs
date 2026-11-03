using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration.ActionSummaries.EnvironmentalBurdenOfDisease {
    public class EbdStackedBarEffectBySubstanceChartCreator : ReportStackedBarChartWithErrorsCreatorBase {

        private string _sectionId;
        private readonly List<EnvironmentalBurdenOfDiseaseSummaryRecord> _records;
        private readonly bool _isUncertainty;
        public override string Title => "Total attributable burden by effect and substance.";

        public EbdStackedBarEffectBySubstanceChartCreator(
            string sectionId,
            List<EnvironmentalBurdenOfDiseaseSummaryRecord> records,
            bool isUncertainty
        ) {
            _records = records;
            _sectionId = sectionId;
            _isUncertainty = isUncertainty;
            Height = 150 + _records.Select(r => r.EffectCode).Distinct().Count();
            Width = 500;
        }

        public override string ChartId {
            get {
                var pictureId = "2fae0839-d5d6-4120-ac1f-6a844c5f4401";
                return StringExtensions.CreateFingerprint(_sectionId + pictureId);
            }
        }

        public override PlotModel Create() {
            var hasPopulationSize = !double.IsNaN(_records.FirstOrDefault().PopulationSize);

            var allEffects = _records
                .Select(r => r.EffectCode)
                .Distinct()
                .ToList();
            var stackedColumnWithErrorItems = new List<StackedColumnWithErrorItem>();
            foreach (var effect in allEffects) {
                var effectRecords = _records
                    .Where(r => r.EffectCode == effect);
                var subCategories = effectRecords
                    .ToDictionary(
                        r => r.SubstanceName,
                        r => _isUncertainty ?
                          (hasPopulationSize ? r.MedianStandardisedTotalAttributableBod : r.MedianTotalAttributableBod) :
                          (hasPopulationSize ? r.StandardisedTotalAttributableBod : r.TotalAttributableBod));

                var value = subCategories.Sum(r => r.Value);
                var lower = _isUncertainty ? effectRecords
                    .Sum(r => hasPopulationSize ? r.LowerStandardisedTotalAttributableBod : r.LowerTotalAttributableBod) :
                    double.NaN;
                var upper = _isUncertainty ? effectRecords
                    .Sum(r => hasPopulationSize ? r.UpperStandardisedTotalAttributableBod : r.UpperTotalAttributableBod) :
                    double.NaN;

                var stackedColumnWithErrorItem = new StackedColumnWithErrorItem() {
                    Label = effectRecords.FirstOrDefault().EffectName,
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

