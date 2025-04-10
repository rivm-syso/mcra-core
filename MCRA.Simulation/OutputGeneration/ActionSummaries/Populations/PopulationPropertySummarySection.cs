using MCRA.Data.Compiled.Objects;
using MCRA.Utils.ExtensionMethods;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class PopulationPropertySummarySection : ActionSummarySectionBase {
        public List<PopulationPropertySummaryRecord> Records { get; set; }

        [Description("Body weight (BodyWeightUnit).")]
        [Display(Name = "Body weight (BodyWeightUnit)")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double NominalPopulationBodyWeight { get; set; }

        [Description("Population size.")]
        [Display(Name = "Population size")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double PopulationSize { get; set; }

        [DisplayName("Population name")]
        public string PopulationName { get; set; }

        [DisplayName("Population code")]
        public string PopulationCode { get; set; }

        public void Summarize(Population population) {
            Records = [];
            PopulationCode = population?.Code ?? string.Empty;
            PopulationName = population?.Name ?? string.Empty;
            NominalPopulationBodyWeight = population?.NominalBodyWeight ?? double.NaN;
            PopulationSize = population?.Size ?? double.NaN;
            if (population?.PopulationIndividualPropertyValues?.Count > 0) {
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
                         PropertyLevel = c.Value.IndividualProperty?.PropertyLevel.ToString() ?? string.Empty,
                         Type = c.Value.IndividualProperty?.PropertyType.GetShortDisplayName() ?? string.Empty,
                     });
                Records.AddRange(results);
            }
        }
    }
}

