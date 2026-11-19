using MathNet.Numerics.Statistics;
using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration.ActionSummaries.EnvironmentalBurdenOfDisease {
    public class EbdCategorizedBarSubstanceByEffectChartCreator : ReportCategorizedBarChartWithErrorsCreatorBase {

        private string _sectionId;
        private readonly List<EnvironmentalBurdenOfDiseaseSummaryRecord> _records;
        private readonly bool _isUncertainty;
        private double _uncertaintyLowerBound;
        private double _uncertaintyUpperBound;
        public override string Title => "Total attributable burden by substance and effect.";

        public EbdCategorizedBarSubstanceByEffectChartCreator(
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
            Height = 150 + _records.Select(r => r.SubstanceCode).Distinct().Count() * 25;
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
            var stackedColumnWithErrorItems = new List<CategorizedColumnWithErrorItem>();
            foreach (var substance in allSubstances) {
                var substanceRecords = _records
                    .Where(r => r.SubstanceCode == substance);
                var effectUncertaintySums = substanceRecords
                    .GroupBy(r => r.EffectCode)
                    .Select(g => new {
                        EffectCode = g.Key,
                        Values = g
                            .Select(r => hasPopulationSize
                                ? [.. r.TotalAttributableBodsStandardised
                                    .Select(c => c * r.StandardisedPopulationSize)]
                                : r.TotalAttributableBods
                            )
                            .ToList()
                    })
                    .ToDictionary(
                        c => c.EffectCode,
                        c => Enumerable.Range(0, c.Values[0].Count)
                                .Select(i => c.Values.Sum(v => v[i]))
                                .ToList()
                    );

                var n = effectUncertaintySums.Values.First().Count;
                var totalUncertaintySums = new double[n];
                foreach (var sums in effectUncertaintySums.Values) {
                    for (var i = 0; i < n; i++) {
                        totalUncertaintySums[i] += sums[i];
                    }
                }

                var value = _isUncertainty
                    ? totalUncertaintySums.Percentile(50.0)
                    : substanceRecords
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

                var subCategories = substanceRecords
                    .ToDictionary(
                        r => r.EffectName,
                        r => (_isUncertainty
                            ? effectUncertaintySums[r.EffectCode].Mean()
                            : (hasPopulationSize
                                ? r.StandardisedTotalAttributableBod
                                : r.TotalAttributableBod
                             )
                        ) / value
                    );

                var stackedColumnWithErrorItem = new CategorizedColumnWithErrorItem() {
                    Label = substanceRecords.FirstOrDefault().SubstanceName,
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

