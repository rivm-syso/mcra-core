using MCRA.Data.Compiled.Objects;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class HbmSingleValueExposuresSummarySection : SummarySection {

        public HbmSingleValueExposureSurveySummaryRecord Record { get; set; }
        public List<HbmSingleValueExposureSetRecord> Percentiles { get; set; }

        public void Summarize(
            ICollection<HbmSingleValueExposureSet> hbmSingleValueExposureSets
        ) {
            //Only one survey allowed 
            var survey = hbmSingleValueExposureSets
                .Select(c => c.Survey)
                .First();

            Record = new HbmSingleValueExposureSurveySummaryRecord() {
                Name = survey.Name,
                Description = survey.Description,
                Country = survey.Country
            };

            Percentiles = [.. hbmSingleValueExposureSets
                .Select(c => new HbmSingleValueExposureSetRecord() {
                    SubstanceCode = c.Substance.Code,
                    SubstanceName = c.Substance.Name,
                    BiologicalMatrix = c.BiologicalMatrix.GetDisplayName(),
                    DoseUnit = c.DoseUnit.GetShortDisplayName(),
                    PercentileRecords = [.. c.HbmSingleValueExposures
                        .Select(r => new HbmSingleValueExposurePercentileRecord() {
                            Percentage = $"p{r.Percentage}",
                            Percentile = r.Value
                        })]
                })];
        }
    }
}
