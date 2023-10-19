using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation;
using MCRA.Simulation.Calculators.KineticModelCalculation.PbpkModelCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.HazardCharacterisations {

    public enum HazardCharacterisationsSections {
        HazardCharacterisationsFromPoDsBMDsSection,
        ImputedHazardCharacterisationsSection,
        HazardCharacterisationImputationRecordsSection,
        HazardCharacterisationsFromIVIVESection,
        KineticModelsSection
    }

    public sealed class HazardCharacterisationsSummarizer : ActionResultsSummarizerBase<HazardCharacterisationsActionResult> {

        public override ActionType ActionType => ActionType.HazardCharacterisations;

        public override void Summarize(ProjectDto project, HazardCharacterisationsActionResult result, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<HazardCharacterisationsSections>(project, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }
            var subHeader = summarizeSelectedHazardCharacterisations(
                project,
                data.BodyWeightUnit,
                data.SelectedEffect,
                data.ActiveSubstances,
                data.AllCompounds,
                data.HazardCharacterisationModelsCollections,
                header,
                order
            );

            if (result != null && outputSettings.ShouldSummarize(HazardCharacterisationsSections.HazardCharacterisationsFromPoDsBMDsSection)) {
                summarizeAvailableHazardCharacterisations(
                    data.SelectedEffect,
                    result,
                    subHeader,
                    order++
                 );
            }

            if (result?.HazardCharacterisationsFromIvive?.Any() ?? false && outputSettings.ShouldSummarize(HazardCharacterisationsSections.HazardCharacterisationsFromIVIVESection)) {
                summarizeIviveHazardCharacterisations(
                    result,
                    data.SelectedEffect,
                    project.EffectSettings.TargetDoseLevelType,
                    subHeader,
                    order++
                );
            }

            if (result?.ImputedHazardCharacterisations?.Any() ?? false && outputSettings.ShouldSummarize(HazardCharacterisationsSections.HazardCharacterisationImputationRecordsSection)) {
                summarizeImputatedHazardCharacterisations(
                    data.SelectedEffect,
                    result,
                    subHeader,
                    order++
                );
            }

            if (result != null && outputSettings.ShouldSummarize(HazardCharacterisationsSections.KineticModelsSection)) {
                summarizeAvailableTargetDosesTimeCourses(
                    result,
                    data.KineticModelInstances,
                    project.AssessmentSettings.ExposureType == ExposureType.Acute,
                    subHeader,
                    order++
                );
            }
        }

        public void SummarizeUncertain(
            ProjectDto project, 
            HazardCharacterisationsActionResult result, 
            ActionData data, 
            SectionHeader header
        ) {
            summarizeSelectedHazardCharacterisationsUncertain(data, header);
        }

        private static List<ActionSummaryUnitRecord> collectUnits(
            ProjectDto project,
            TargetUnit hazardCharacterisationsUnit,
            BodyWeightUnit bodyWeightUnit
        ) {
            var result = new List<ActionSummaryUnitRecord>();
            var targetConcentrationUnit = new TargetUnit(
                ExposureTarget.DefaultInternalExposureTarget,
                hazardCharacterisationsUnit.SubstanceAmountUnit, 
                hazardCharacterisationsUnit.ConcentrationMassUnit
            );
            result.Add(new ActionSummaryUnitRecord("LowerBound", $"p{project.UncertaintyAnalysisSettings.UncertaintyLowerBound}"));
            result.Add(new ActionSummaryUnitRecord("UpperBound", $"p{project.UncertaintyAnalysisSettings.UncertaintyUpperBound}"));
            // TODO: the units below seem to be used by kinetic model time course view
            // check whether this works for multiple targets.
            result.Add(new ActionSummaryUnitRecord("TargetAmountUnit", hazardCharacterisationsUnit.SubstanceAmountUnit.GetShortDisplayName()));
            result.Add(new ActionSummaryUnitRecord("TargetConcentrationUnit", targetConcentrationUnit.GetShortDisplayName()));
            result.Add(new ActionSummaryUnitRecord("BodyWeightUnit", bodyWeightUnit.GetShortDisplayName()));
            return result;
        }

        private SectionHeader summarizeSelectedHazardCharacterisations(
            ProjectDto project,
            BodyWeightUnit bodyWeightUnit,
            Effect selectedEffect,
            ICollection<Compound> activeSubstances,
            ICollection<Compound> allCompounds,
            ICollection<HazardCharacterisationModelCompoundsCollection> hazardCharacterisationModelsCollections,
            SectionHeader header,
            int order
        ) {
            var section = new HazardCharacterisationsSummarySection();
            var subHeader = header.AddSubSectionHeaderFor(section, ActionType.GetDisplayName(), order);
            section.Summarize(
                selectedEffect,
                activeSubstances ?? allCompounds,
                hazardCharacterisationModelsCollections,
                project.EffectSettings.TargetDoseLevelType,
                project.AssessmentSettings.ExposureType,
                project.EffectSettings.TargetDosesCalculationMethod,
                project.EffectSettings.UseDoseResponseModels,
                project.EffectSettings.UseAdditionalAssessmentFactor,
                project.EffectSettings.AdditionalAssessmentFactor,
                project.EffectSettings.HazardCharacterisationsConvertToSingleTargetMatrix,
                project.CalculationActionTypes.Contains(ActionType)
            );
            subHeader.Units = collectUnits(
                project,
                hazardCharacterisationModelsCollections.First().TargetUnit, // TODO: check use of first
                bodyWeightUnit
            );
            subHeader.SaveSummarySection(section);
            return subHeader;
        }

        private static void summarizeSelectedHazardCharacterisationsUncertain(
            ActionData data,
            SectionHeader header
        ) {
            var subHeader = header.GetSubSectionHeader<HazardCharacterisationsSummarySection>();
            if (subHeader != null) {
                var section = subHeader.GetSummarySection() as HazardCharacterisationsSummarySection;
                section.SummarizeUncertain(data.HazardCharacterisationModelsCollections);
                subHeader.SaveSummarySection(section);
            }
        }

        private SectionHeader summarizeAvailableHazardCharacterisations(
            Effect selectedEffect,
            HazardCharacterisationsActionResult result,
            SectionHeader header,
            int order
        ) {
            var section = new AvailableHazardCharacterisationsSummarySection() {
                SectionLabel = getSectionLabel(HazardCharacterisationsSections.HazardCharacterisationsFromPoDsBMDsSection)
            };
            var subHeader = header.AddSubSectionHeaderFor(
                section,
                "Hazard characterisations from PoDs/BMDs",
                order
            );
            section.Summarize(selectedEffect, result.HazardCharacterisationsFromPodAndBmd);
            subHeader.SaveSummarySection(section);
            return subHeader;
        }

        private SectionHeader summarizeImputatedHazardCharacterisations(
            Effect selectedEffect,
            HazardCharacterisationsActionResult result,
            SectionHeader header,
            int order
        ) {
            var section = new ImputedHazardCharacterisationsSummarySection() {
                SectionLabel = getSectionLabel(HazardCharacterisationsSections.ImputedHazardCharacterisationsSection)
            };  
            var subHeader = header.AddSubSectionHeaderFor(
                section,
                "Imputed hazard characterisations",
                order
            );
            section.Summarize(selectedEffect, result.ImputedHazardCharacterisations);
            subHeader.SaveSummarySection(section);
            var subOrder = 1;
            summarizeHazardCharacterisationImputationRecords(selectedEffect, result, subHeader, subOrder);
            return subHeader;
        }

        private SectionHeader summarizeHazardCharacterisationImputationRecords(
            Effect selectedEffect,
            HazardCharacterisationsActionResult result,
            SectionHeader header,
            int order
        ) {
            var section = new HazardCharacterisationImputationCandidatesSection() {
                SectionLabel = getSectionLabel(HazardCharacterisationsSections.HazardCharacterisationImputationRecordsSection)
            };
            var subHeader = header.AddSubSectionHeaderFor(
                section,
                "Hazard characterisation imputation records",
                order
            );
            section.Summarize(selectedEffect, result.HazardCharacterisationImputationRecords);
            subHeader.SaveSummarySection(section);
            return subHeader;
        }

        private SectionHeader summarizeIviveHazardCharacterisations(
                HazardCharacterisationsActionResult result,
                Effect selectedEffect,
                TargetLevelType targetDoseLevel,
                SectionHeader header,
                int order
            ) {
            if (result.HazardCharacterisationsFromIvive != null) {
                var section = new IviveHazardCharacterisationsSummarySection() {
                    SectionLabel = getSectionLabel(HazardCharacterisationsSections.HazardCharacterisationsFromIVIVESection)
                };
                var subHeader = header.AddSubSectionHeaderFor(
                    section,
                    "Hazard characterisations from IVIVE",
                    order
                );
                section.Summarize(
                    selectedEffect,
                    result.ReferenceSubstance,
                    result.HazardCharacterisationsFromIvive,
                    targetDoseLevel
                );
                subHeader.SaveSummarySection(section);
                return subHeader;
            }
            return null;
        }

        private void summarizeAvailableTargetDosesTimeCourses(
            HazardCharacterisationsActionResult result,
            ICollection<KineticModelInstance> kineticModelInstances,
            bool isAcute,
            SectionHeader header,
            int order
        ) {
            if (result.KineticModelDrilldownRecords?.Any() ?? false) {
                var subHeader = header.AddEmptySubSectionHeader(
                    "Kinetic models",
                    order++,
                    getSectionLabel(HazardCharacterisationsSections.KineticModelsSection)
                );
                var subOrder = 0;
                var linearModelCompounds = new HashSet<Compound>();
                foreach (var item in result.KineticModelDrilldownRecords) {
                    foreach (var substance in item.TargetExposuresBySubstance.Keys) {
                        var isPBPKModel = item.TargetExposuresBySubstance[substance] is SubstanceTargetExposurePattern;
                        if (isPBPKModel) {
                            var section = new KineticModelTimeCourseSection();
                            var kineticModelInstance = kineticModelInstances.Single(c => c.Substances.Contains(substance));
                            var subHeader1 = subHeader.AddSubSectionHeaderFor(section, $"Hazard characterisations drilldown PBPK model {substance.Name}", subOrder++);
                            section.SummarizeIndividualDrillDown(
                                new List<ITargetIndividualExposure>() { item },
                                item.ExposuresPerRouteSubstance.Keys,
                                substance,
                                kineticModelInstance,
                                isAcute
                            );
                            subHeader1.SaveSummarySection(section);
                        } else if (!linearModelCompounds.Contains(substance)) {
                            linearModelCompounds.Add(substance);
                        }
                    }
                }
                if (linearModelCompounds.Any()) {
                    var section = new LinearModelSection();
                    var subHeader2 = subHeader.AddSubSectionHeaderFor(section, $"Hazard characterisations absorption factor models", subOrder++);
                    section.Summarize(linearModelCompounds.ToList());
                    subHeader2.SaveSummarySection(section);
                }
            }
        }
    }
}
