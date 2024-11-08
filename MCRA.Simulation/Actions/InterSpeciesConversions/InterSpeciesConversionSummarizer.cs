using MCRA.Utils.ExtensionMethods;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.InterSpeciesConversion;
using MCRA.Simulation.OutputGeneration;

namespace MCRA.Simulation.Actions.InterSpeciesConversions {
    public enum InterSpeciesConversionsSections {
        //No sub-sections
    }
    public sealed class InterSpeciesConversionSummarizer : ActionResultsSummarizerBase<IInterSpeciesConversionActionResult> {

        public override ActionType ActionType => ActionType.InterSpeciesConversions;

        public override void Summarize(ActionModuleConfig sectionConfig, IInterSpeciesConversionActionResult actionResult, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<InterSpeciesConversionsSections>(sectionConfig, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }
            summarizeInterSpeciesConversionModels(
                data.InterSpeciesFactorModels,
                header,
                order
            );
        }

        public void SummarizeUncertain(ProjectDto project, IInterSpeciesConversionActionResult actionResult, ActionData data, SectionHeader header) {
            summarizeInterSpeciesConversionModelsUncertain(
                data.ActiveSubstances,
                data.AllEffects,
                data.InterSpeciesFactorModels,
                header
            );
        }

        private static void summarizeInterSpeciesConversionModels(
            IDictionary<(string species, Compound substance, Effect effect), InterSpeciesFactorModel> interSpeciesFactorModels,
            SectionHeader header,
            int order
        ) {
            var section = new InterSpeciesConversionModelsSummarySection();
            var subHeader = header.AddSubSectionHeaderFor(section, "Inter-species conversions", order);
            section.Records = interSpeciesFactorModels
                .Where(r => !r.Value.InterSpeciesFactor.IsDefault)
                .Select(r => new InterSpeciesConversionModelSummaryRecord {
                    EffectCode = r.Value.InterSpeciesFactor.Effect?.Code,
                    EffectName = r.Value.InterSpeciesFactor.Effect?.Name,
                    CompoundCode = r.Value.InterSpeciesFactor.Compound?.Code,
                    CompoundName = r.Value.InterSpeciesFactor.Compound?.Name,
                    Species = r.Value.InterSpeciesFactor.Species,
                    GeometricMean = r.Value.InterSpeciesFactor.InterSpeciesFactorGeometricMean,
                    GeometricStDev = r.Value.InterSpeciesFactor.InterSpeciesFactorGeometricStandardDeviation,
                    StandardHumanBodyWeight = r.Value.InterSpeciesFactor.StandardHumanBodyWeight,
                    HumanBodyWeightUnit = r.Value.InterSpeciesFactor.HumanBodyWeightUnit.GetShortDisplayName(),
                    StandardAnimalBodyWeight = r.Value.InterSpeciesFactor.StandardAnimalBodyWeight,
                    AnimalBodyWeightUnit = r.Value.InterSpeciesFactor.AnimalBodyWeightUnit.GetShortDisplayName(),
                    InterSpeciesFactorUncertaintyValues = [],
                })
            .ToList();

            var defaultFactor = interSpeciesFactorModels
                .Select(r => r.Value?.InterSpeciesFactor)
                .FirstOrDefault(r => r != null && r.IsDefault);
            if (defaultFactor != null) {
                section.DefaultInterSpeciesFactor = new InterSpeciesConversionModelSummaryRecord() {
                    GeometricMean = defaultFactor.InterSpeciesFactorGeometricMean,
                    GeometricStDev = defaultFactor.InterSpeciesFactorGeometricStandardDeviation,
                    StandardHumanBodyWeight = defaultFactor.StandardHumanBodyWeight,
                    HumanBodyWeightUnit = defaultFactor.HumanBodyWeightUnit.GetShortDisplayName(),
                    StandardAnimalBodyWeight = defaultFactor.StandardAnimalBodyWeight,
                    AnimalBodyWeightUnit = defaultFactor.AnimalBodyWeightUnit.GetShortDisplayName(),
                };
            }

            subHeader.SaveSummarySection(section);
        }

        private static void summarizeInterSpeciesConversionModelsUncertain(
            ICollection<Compound> activeSubstances,
            ICollection<Effect> effects,
            IDictionary<(string species, Compound substance, Effect effect), InterSpeciesFactorModel> interSpeciesFactorModels,
            SectionHeader header
        ) {
            var subHeader = header.GetSubSectionHeader<InterSpeciesConversionModelsSummarySection>();
            if (subHeader != null) {
                var section = subHeader.GetSummarySection() as InterSpeciesConversionModelsSummarySection;
                var substancesLookup = activeSubstances.ToDictionary(r => r.Code);
                var effectsLookup = effects.ToDictionary(r => r.Code);
                foreach (var record in section.Records) {
                    var substance = !string.IsNullOrEmpty(record.CompoundCode) ? substancesLookup[record.CompoundCode] : null;
                    var effect = !string.IsNullOrEmpty(record.EffectCode) ? effectsLookup[record.EffectCode] : null;
                    var model = InterSpeciesFactorModelsBuilder
                        .GetInterSpeciesFactor(interSpeciesFactorModels, effect, record.Species, substance);
                    record.InterSpeciesFactorUncertaintyValues.Add(model);
                }
                subHeader.SaveSummarySection(section);
            }
        }
    }
}
