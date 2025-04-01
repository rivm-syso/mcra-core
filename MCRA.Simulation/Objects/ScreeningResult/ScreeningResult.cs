namespace MCRA.Simulation.Objects {
    public sealed class ScreeningResult {

        /// <summary>
        /// The total number of scc records.
        /// </summary>
        public int TotalNumberOfSccRecords { get; set; }

        /// <summary>
        /// Sum of all cups.
        /// </summary>
        public double SumCupAllSccRecords { get; set; }

        /// <summary>
        /// The risk driver of the screening.
        /// </summary>
        public ScreeningResultRecord RiskDriver { get; set; }

        /// <summary>
        /// The screening results grouped per modelled food and substance.
        /// </summary>
        public List<GroupedScreeningResultRecord> ScreeningResultsPerFoodCompound { get; set; }

        /// <summary>
        /// The selected number of scc records.
        /// </summary>
        public int SelectedNumberOfSccRecords {
            get {
                if (ScreeningResultsPerFoodCompound == null) {
                    return TotalNumberOfSccRecords;
                }
                return ScreeningResultsPerFoodCompound.Sum(r => r.ScreeningRecords.Count);
            }
        }

        /// <summary>
        /// Returns the effective critical exposure percentage of the selected MSCCs.
        /// </summary>
        public double EffectiveCumulativeSelectionPercentage => 100 * ScreeningResultsPerFoodCompound.Last().CumulativeContributionFraction;
    }
}
