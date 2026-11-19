using MathNet.Numerics.Statistics;
using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration.ActionSummaries.EnvironmentalBurdenOfDisease {
    public class EbdCategorizedBarEffectBySubstanceChartCreator : ReportCategorizedBarChartWithErrorsCreatorBase {

        private string _sectionId;
        private readonly List<EnvironmentalBurdenOfDiseaseSummaryRecord> _records;
        private readonly bool _isUncertainty;
        private double _uncertaintyLowerBound;
        private double _uncertaintyUpperBound;
        public override string Title => "Total attributable burden by effect and substance.";

        public EbdCategorizedBarEffectBySubstanceChartCreator(
            string sectionId,
            List<EnvironmentalBurdenOfDiseaseSummaryRecord> records,
            bool isUncertainty,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound
        ) {
            _records = records;
            _sectionId = sectionId;
            _isUncertainty = isUncertainty;
            _uncertaintyLowerBound = uncertaintyLowerBound;
            _uncertaintyUpperBound = uncertaintyUpperBound;
            Height = 150 + _records.Select(r => r.EffectCode).Distinct().Count() * 25;
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
            var stackedColumnWithErrorItems = new List<CategorizedColumnWithErrorItem>();
            foreach (var effect in allEffects) {
                var effectRecords = _records
                    .Where(r => r.EffectCode == effect);

                var substanceUncertaintySums = effectRecords
                    .GroupBy(r => r.SubstanceCode)
                    .Select(g => new {
                        SubstanceCode = g.Key,
                        Values = g
                            .Select(r => hasPopulationSize
                                ? [.. r.TotalAttributableBodsStandardised
                                    .Select(c => c * r.StandardisedPopulationSize)]
                                : r.TotalAttributableBods
                            )
                            .ToList()
                    })
                    .ToDictionary(
                        c => c.SubstanceCode,
                        c => Enumerable.Range(0, c.Values[0].Count)
                                .Select(i => c.Values.Sum(v => v[i]))
                                .ToList()
                    );

                var n = substanceUncertaintySums.Values.First().Count;
                var totalUncertaintySums = new double[n];
                foreach (var sums in substanceUncertaintySums.Values) {
                    for (var i = 0; i < n; i++) {
                        totalUncertaintySums[i] += sums[i];
                    }
                }

                var value = _isUncertainty
                    ? totalUncertaintySums.Percentile(50.0)
                    : effectRecords
                        .Sum(r => hasPopulationSize
                            ? r.StandardisedTotalAttributableBod
                            : r.TotalAttributableBod
                        );
                var lower = _isUncertainty
                    ? totalUncertaintySums.Percentile(_uncertaintyLowerBound)
                    : double.NaN;
                var upper = _isUncertainty
                    ? totalUncertaintySums.Percentile(_uncertaintyUpperBound)
                    : double.NaN;

                var subCategories = effectRecords
                    .ToDictionary(
                        r => r.SubstanceName,
                        r => (_isUncertainty
                            ? substanceUncertaintySums[r.SubstanceCode].Mean()
                            : (hasPopulationSize
                                ? r.StandardisedTotalAttributableBod
                                : r.TotalAttributableBod
                             )
                        ) / value
                    );

                var stackedColumnWithErrorItem = new CategorizedColumnWithErrorItem() {
                    Label = effectRecords.FirstOrDefault().EffectName,
                    Value = value,
                    Lower = lower,
                    Upper = upper,
                    Subcategories = subCategories
                };
                stackedColumnWithErrorItems.Add(stackedColumnWithErrorItem);
            }
            return create(stackedColumnWithErrorItems, uncertainty: _isUncertainty, xTitle: _records.First().BodIndicator);
        }
    }
}

