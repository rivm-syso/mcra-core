using MCRA.Utils.ProgressReporting;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Objects;
using MCRA.Simulation.Calculators.CompoundResidueCollectionCalculation;
using MCRA.Simulation.Calculators.FoodConversionCalculation;

namespace MCRA.Simulation.Calculators.HighExposureFoodSubstanceCombinations {

    public abstract class ScreeningCalculator {

        /// <summary>
        /// Epsilon value for everything that should be zero, but can't be zero in practice;
        /// like standard deviations.
        /// </summary>
        protected static double _epsilon = 1e-10;

        /// <summary>
        /// The maximum number of selected scc records to limit the screening output.
        /// </summary>
        protected static int _maxNumberOfSelectedScreeningResultRecords = 1000;

        /// <summary>
        /// Importance factor of the Lor in the screening calculations.
        /// </summary>
        protected double _logLorImportanceFactor { get; set; }
        protected double _criticalExposurePercentage { get; set; }
        protected double _cumulativeSelectionPercentage { get; set; }
        protected double _importanceLor { get; set; }
        protected bool _isPerPerson { get; set; }

        /// <summary>
        /// The SCC or risk driver component table.
        /// </summary>
        protected List<ScreeningResultRecord> _screeningResultRecords { get; set; }

        public ScreeningCalculator(
            double criticalExposurePercentage,
            double cumulativeSelectionPercentage,
            double importanceLor,
            bool isPerPerson
        ) {
            _criticalExposurePercentage = criticalExposurePercentage;
            _cumulativeSelectionPercentage = cumulativeSelectionPercentage;
            _importanceLor = importanceLor;
            _isPerPerson = isPerPerson;
            _logLorImportanceFactor = importanceLor > 0
                                    ? Math.Log(importanceLor)
                                    : Math.Log(_epsilon);
        }

        /// <summary>
        /// Calculates the screening results from the provided list of screening result records. This implies
        /// that only the critical exposure contributions are to be re-calculated.
        /// </summary>
        /// <param name="screeningResultRecords"></param>
        public ScreeningResult Calculate(List<ScreeningResultRecord> screeningResultRecords) {
            _screeningResultRecords = screeningResultRecords;
            if (_screeningResultRecords == null || !_screeningResultRecords.Any()) {
                throw new Exception("No screening results available!");
            }
            calculateCriticalExposureContributions();
            var result = new ScreeningResult() {
                TotalNumberOfSccRecords = _screeningResultRecords.Count,
                RiskDriver = _screeningResultRecords.FirstOrDefault(),
                ScreeningResultsPerFoodCompound = getSelectedScreeningResultsPerFoodCompound(),
                SumCupAllSccRecords = _screeningResultRecords.Sum(r => r.Cup),
            };
            return result;
        }

        /// <summary>
        /// Performs intake screening of all relevant source-compound combinations.
        /// </summary>
        /// <param name="conversionResults"></param>
        /// <param name="simulatedIndividualDays"></param>
        /// <param name="foodConsumptions"></param>
        /// <param name="compoundResidueCollections"></param>
        /// <param name="compoundEffectCollection"></param>
        /// <param name="correctedRelativePotencyFactors"></param>
        /// <param name="progressReport"></param>
        /// <returns></returns>
        public abstract ScreeningResult Calculate(
            IEnumerable<FoodConversionResult> conversionResults,
            IEnumerable<IndividualDay> simulatedIndividualDays,
            IEnumerable<FoodConsumption> foodConsumptions,
            IEnumerable<FoodSubstanceResidueCollection> compoundResidueCollections,
            IDictionary<Compound, double> correctedRelativePotencyFactors,
            CompositeProgressState progressReport
        );

        /// <summary>
        /// Returns all scc records.
        /// </summary>
        /// <returns></returns>
        public List<ScreeningResultRecord> GetAllScreeningResultRecords() {
            return _screeningResultRecords;
        }

        /// <summary>
        /// Updates the contributions to the critical exposure level of all screening records.
        /// </summary>
        /// <param name="settings"></param>
        protected abstract void calculateCriticalExposureContributions(CompositeProgressState progressState = null);

        /// <summary>
        /// Calculate critical exposure for supplied critical exposure percentage
        /// </summary>
        /// <param name="screeningResult"></param>
        protected abstract void calculateCriticalExposureContribution(ScreeningResultRecord screeningResult);

        /// <summary>
        /// Returns the screening results grouped by food-as-measured/compound.
        /// </summary>
        /// <returns></returns>
        protected List<GroupedScreeningResultRecord> getAllScreeningResultsPerFoodCompound() {
            var records = _screeningResultRecords
                .GroupBy(c => (c.Compound, c.FoodAsMeasured))
                .Select(c => new GroupedScreeningResultRecord {
                    Compound = c.Key.Compound,
                    FoodAsMeasured = c.Key.FoodAsMeasured,
                    Contribution = c.Sum(s => s.CupPercentage),
                    CumulativeContributionFraction = double.NaN,
                    ScreeningRecords = c.ToList(),
                })
                .OrderByDescending(o => o.Contribution)
                .ToList();
            var sum = 0D;
            foreach (var item in records) {
                sum += item.Contribution;
                item.CumulativeContributionFraction = sum;
            }
            return records.ToList();
        }

        /// <summary>
        /// Calculate the selected number of records in the screenings set
        /// </summary>
        /// <returns></returns>
        protected List<GroupedScreeningResultRecord> getSelectedScreeningResultsPerFoodCompound() {
            var sum = 0D;
            foreach (var item in _screeningResultRecords) {
                sum += item.CupPercentage;
                item.CumCupFraction = sum;
            }
            // optimistic imputation
            var resultRecords = _screeningResultRecords;
            if (_importanceLor == 0) {
                resultRecords = _screeningResultRecords
                    .Where(c => c.ConcentrationParameters.FractionPositives > 0)
                    .ToList();
            }
            var selectedRecords = cumulativeSelect(
                resultRecords,
                r => r.CumCupFraction,
                _cumulativeSelectionPercentage / 100,
                _maxNumberOfSelectedScreeningResultRecords
            ).ToList();

            var groupedScreeningRecords = selectedRecords.GroupBy(c => (c.Compound, c.FoodAsMeasured)).ToList();
            var records = groupedScreeningRecords
                .Select(c => new GroupedScreeningResultRecord {
                    Compound = c.Key.Compound,
                    FoodAsMeasured = c.Key.FoodAsMeasured,
                    Contribution = c.Sum(s => s.CupPercentage),
                    CumulativeContributionFraction = double.NaN,
                    ScreeningRecords = c.ToList(),
                })
                .OrderByDescending(o => o.Contribution)
                .ToList();
            sum = 0D;
            foreach (var item in records) {
                sum += item.Contribution;
                item.CumulativeContributionFraction = sum;
            }
            return records;
        }

        /// <summary>
        /// Selects the top contributing records based on the cumulative contribution factor.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="records"></param>
        /// <param name="cumulativeContributionValueExtractor"></param>
        /// <param name="cumulativeSelectionFraction"></param>
        /// <param name="maximumNumberOfRecords"></param>
        /// <returns></returns>
        private static IEnumerable<T> cumulativeSelect<T>(
            List<T> records,
            Func<T, double> cumulativeContributionValueExtractor,
            double cumulativeSelectionFraction,
            int maximumNumberOfRecords
        ) {
            if (cumulativeSelectionFraction == 1) {
                return records.Take(maximumNumberOfRecords);
            }
            var selectedCount = 0;
            while (selectedCount < records.Count) {
                if (cumulativeContributionValueExtractor(records[selectedCount]) > cumulativeSelectionFraction) {
                    break;
                }
                selectedCount++;
            }
            selectedCount = Math.Min(selectedCount + 1, maximumNumberOfRecords);
            return records.Take(selectedCount);
        }
    }
}
