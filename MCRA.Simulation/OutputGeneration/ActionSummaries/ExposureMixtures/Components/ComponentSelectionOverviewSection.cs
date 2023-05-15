using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Utils;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ComponentSelectionOverviewSection : SummarySection {

        public List<ComponentRecord> Records { get; set; }
        public List<SubstanceComponentRecord> SortedSubstancesComponentRecords { get; set; }
        /// <summary>
        /// Contains for each component a list of nmf-values of all substances
        /// </summary>
        public List<List<SubstanceComponentRecord>> SubstanceComponentRecords { get; set; }
        public List<List<SubstanceComponentRecord>> SubstanceBarChartComponentRecords { get; set; }
        public ExposureApproachType ExposureApproach { get; set; }
        public InternalConcentrationType InternalConcentrationType { get; set; }
        public ExposureType ExposureType { get; set; }
        public double Sparseness { get; set; }
        public double RatioCutOff { get; set; }
        public double TotalExposureCutOff { get; set; }
        public double TotalExposureCutOffPercentile { get; set; }
        public int NumberOfDays { get; set; }
        public int NumberOfSelectedDays { get; set; }
        public int NumberOfIterations { get; set; }
        public int NumberOfCompounds { get; set; }

        #region Comparer class CompoundRecord

        /// <summary>
        /// Comparer class for sorting rows. Find the index of the highest contribution in a row and sort, not the max value itself is important.
        /// </summary>
        internal class CompoundRecordComparer : IComparer<List<SubstanceComponentRecord>> {
            public int Compare(List<SubstanceComponentRecord> x, List<SubstanceComponentRecord> y) {
                if (x.Count != y.Count) {
                    throw new System.ArgumentException("Array lengths must equal.");
                }
                var maxX = x[0].NmfValue;
                var maxY = y[0].NmfValue;
                var indexX = 0;
                var indexY = 0;
                for (int i = 1; i < x.Count; i++) {
                    maxY = maxY > y[i].NmfValue ? maxY : y[i].NmfValue;
                    maxX = maxX > x[i].NmfValue ? maxX : x[i].NmfValue;
                    indexY = maxY > y[i].NmfValue ? indexY : i;
                    indexX = maxX > x[i].NmfValue ? indexX : i;
                }
                if (indexX < indexY) {
                    return 1;
                } else if (indexX > indexY) {
                    return -1;
                }
                if (maxX < maxY) {
                    return -1;
                } else if (maxX > maxY) {
                    return 1;
                }
                return 0;
            }
        }

        #endregion

        public void Summarize(
            List<Compound> substances,
            List<ComponentRecord> componentRecords,
            List<double> rmse,
            GeneralMatrix uMatrix,
            IDictionary<Compound, string> substanceSamplingMethods,
            ExposureApproachType exposureApproachType,
            InternalConcentrationType internalConcentrationType,
            ExposureType exposureType,
            double totalExposureCutOffPercentile,
            double sparseness,
            double ratioCutoff,
            double totalExposureCutoff,
            int numberOfDays,
            int numberOfSelectedDays,
            int numberOfIterations,
            bool removeZeros,
            SectionHeader header
        ) {
            Records = componentRecords;
            Sparseness = sparseness;
            NumberOfIterations = numberOfIterations;
            RatioCutOff = ratioCutoff;
            TotalExposureCutOff = totalExposureCutoff;
            ExposureApproach = exposureApproachType;
            NumberOfDays = numberOfDays;
            NumberOfSelectedDays = numberOfSelectedDays;
            TotalExposureCutOffPercentile = totalExposureCutOffPercentile;
            NumberOfCompounds = substances.Count;
            Records = componentRecords;
            ExposureType = exposureType;
            InternalConcentrationType = internalConcentrationType;

            var sorted = uMatrix.NormalizeColumns().Array.Select((r, i) => {
                var listCompoundRecords = r.Select(c => {
                    var samplingMethod = substanceSamplingMethods != null
                        ? (substanceSamplingMethods.TryGetValue(substances[i], out var record) ? $" ({record})" : null)
                        : null;
                    return new SubstanceComponentRecord() {
                        SubstanceCode = substances[i].Code,
                        SubstanceName = substances[i].Name + samplingMethod,
                        NmfValue = c,
                    };
                }).ToList();
                return listCompoundRecords;
            })
            .ToList();

            List<List<SubstanceComponentRecord>> sortedComponents = null;
            if (removeZeros) {
                var sortedTrimmed = sorted.Where(c => c.Select(s => s.NmfValue).Sum() > 0).ToList();
                sortedComponents = sortedTrimmed.OrderBy(x => x, new CompoundRecordComparer()).ToList();
            } else {
                sortedComponents = sorted.OrderBy(x => x, new CompoundRecordComparer()).ToList();
            }

            SortedSubstancesComponentRecords = sortedComponents.Select(c => c.First()).ToList();
            var rawSubstanceComponentRecords = new List<List<SubstanceComponentRecord>>();
            var numberOfCompounds = SortedSubstancesComponentRecords.Count;
            for (int i = 0; i < uMatrix.ColumnDimension; i++) {
                var components = new List<SubstanceComponentRecord>();
                foreach (var item in sortedComponents) {
                    components.Add(new SubstanceComponentRecord() {
                        SubstanceName = item[i].SubstanceName,
                        SubstanceCode = item[i].SubstanceCode,
                        NmfValue = item[i].NmfValue,
                    });
                }
                rawSubstanceComponentRecords.Add(components.OrderByDescending(c => c.NmfValue).ToList());
            }

            SubstanceComponentRecords = rawSubstanceComponentRecords.Select(c => c.Where(r => r.NmfValue > 0).ToList()).ToList();

            SubstanceBarChartComponentRecords = convertToBarChartRecords(rawSubstanceComponentRecords);

            var count = 0;
            var section = new ComponentSelectionSection();
            var subHeader = header.AddSubSectionHeaderFor(section, $"Details substance contributions to components", count++);
            for (int componentId = 0; componentId < uMatrix.ColumnDimension; componentId++) {
                section.SummarizePerComponent(
                    componentRecords,
                    SubstanceComponentRecords,
                    substances,
                    true
                );
            }
            subHeader.SaveSummarySection(section);

            var diagnosticsSection = new ComponentDiagnosticsSection();
            var subHeader1 = header.AddSubSectionHeaderFor(diagnosticsSection, "Additional details", count++);
            diagnosticsSection.Summarize(
                componentRecords,
                exposureApproachType,
                uMatrix,
                substances,
                rmse
            );
            subHeader1.SaveSummarySection(diagnosticsSection);
        }

        /// <summary>
        /// Convert SubstanceComponentRecords to bar chart records and collect all substances in other category
        /// where nmf-value smaller than others criterium
        /// </summary>
        /// <returns></returns>
        private List<List<SubstanceComponentRecord>> convertToBarChartRecords(List<List<SubstanceComponentRecord>> rawSubstanceComponentRecords) {
            var componentRecords = rawSubstanceComponentRecords.Select(c => c.OrderBy(r => r.SubstanceName).ToList()).ToList();
            var substanceNames = componentRecords.First().Select(c => c.SubstanceName).ToList();
            var otherCrit = 0.1;

            var otherSubstances = new List<string>();
            for (int i = 0; i < substanceNames.Count; i++) {
                if (componentRecords.All(c => c[i].NmfValue < otherCrit)) {
                    otherSubstances.Add(substanceNames[i]);
                }
            }

            //prune records for bar chart
            var prunedSubstanceComponentRecords = new List<List<SubstanceComponentRecord>>();
            if (otherSubstances.Any() && otherSubstances.Count > 1) {
                foreach (var records in componentRecords) {
                    var results = new List<SubstanceComponentRecord>() {
                        new SubstanceComponentRecord() {
                            NmfValue = records.Where(c => otherSubstances.Contains(c.SubstanceName)).Sum(c => c.NmfValue),
                            SubstanceName = "others",
                            SubstanceCode = "others"
                        }
                    };
                    results.AddRange(records.Where(c => !otherSubstances.Contains(c.SubstanceName)).Select(c => c).OrderBy(c => c.SubstanceName).ToList());
                    prunedSubstanceComponentRecords.Add(results);
                }
                return prunedSubstanceComponentRecords;
            } else {
                return componentRecords;
            }
        }
    }
}
