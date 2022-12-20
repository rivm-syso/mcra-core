using MCRA.Data.Compiled.Objects;
using MCRA.Utils.ExtensionMethods;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class PopulationsSummarySection : ActionSummaryBase {
        public List<PopulationPropertySummaryRecord> Records { get; set; }

        [Description("Body weight (BodyWeightUnit).")]
        [Display(Name = "Body weight (BodyWeightUnit)")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double NominalPopulationBodyWeight { get; set; }

        [DisplayName("Population name")]
        public string PopulationName { get; set; }

        [DisplayName("Population code")]
        public string PopulationCode { get; set; }

        public void Summarize(Population population) {
            Records = new List<PopulationPropertySummaryRecord>();
            PopulationCode = population?.Code ?? string.Empty;
            PopulationName = population?.Name ?? string.Empty;
            NominalPopulationBodyWeight = population?.NominalBodyWeight ?? double.NaN;
            if (population?.PopulationIndividualPropertyValues?.Any() ?? false) {
                var results = population.PopulationIndividualPropertyValues
                    .Select(c => new PopulationPropertySummaryRecord() {
                         PopulationCode = population?.Code ?? string.Empty,
                         PopulationName = population?.Name ?? string.Empty,
                         PropertyCode = c.Key,
                         PropertyName = c.Key,
                         Description = c.Value.IndividualProperty?.Description ?? string.Empty,
                         Location = population?.Location ?? string.Empty,
                         Value = c.Value.Value,
                         MinValue = c.Value.MinValue,
                         MaxValue = c.Value.MaxValue,
                         StartDate = c.Value.StartDate?.ToString("MM/dd/yyyy") ?? string.Empty,
                         EndDate = c.Value.EndDate?.ToString("MM/dd/yyyy") ?? string.Empty,
                         PropertyLevel = c.Value.IndividualProperty?.PropertyLevelString ?? string.Empty,
                         Type = c.Value.IndividualProperty?.PropertyType.GetShortDisplayName() ?? string.Empty,
                     });
                Records.AddRange(results);
            }
        }
    }
}

