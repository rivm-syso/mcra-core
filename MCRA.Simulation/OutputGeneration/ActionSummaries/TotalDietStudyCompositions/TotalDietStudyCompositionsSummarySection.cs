using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class TotalDietStudyCompositionsSummarySection : SummarySection {

        public List<TotalDietStudyCompositionsSummaryRecord> Records { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="recipes"></param>
        /// <param name="foodAsEaten"></param>
        public void Summarize(SectionHeader header, ILookup<Food, TDSFoodSampleComposition> tdsCompositionsLookup) {
            var numbers = tdsCompositionsLookup.Select(c => c.Count()).GroupBy(c => c).ToList();
            Records = numbers
                .Select(c => new TotalDietStudyCompositionsSummaryRecord() {
                    Number = c.Count(),
                    NumberOfSubFoods = c.Key,
                })
                .OrderBy(r => r.NumberOfSubFoods)
                .ToList();

            if (tdsCompositionsLookup != null) {
                var TDSConversionsSection = new TDSConversionsSection();
                var subHeader = header.AddSubSectionHeaderFor(TDSConversionsSection, "Total diet studies: compositions", 2);
                TDSConversionsSection.Summarize(tdsCompositionsLookup);
                subHeader.SaveSummarySection(TDSConversionsSection);
            }
        }
    }
}
