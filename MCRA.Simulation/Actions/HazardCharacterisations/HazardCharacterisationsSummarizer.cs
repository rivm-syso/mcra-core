using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation;
using MCRA.Simulation.Calculators.KineticModelCalculation.PbpkModelCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.HazardCharacterisations {

    public enum HazardCharacterisationsSections {
        HazardCharacterisationsFromDataSummarySection,
        HazardCharacterisationsFromPoDsBMDsSection,
        ImputedHazardCharacterisationsSection,
        HazardCharacterisationImputationRecordsSection,
        HazardCharacterisationsFromIVIVESection,
        KineticModelsSection
    }

    public sealed class HazardCharacterisationsSummarizer : ActionModuleResultsSummarizer<HazardCharacterisationsModuleConfig, HazardCharacterisationsActionResult> {
        private readonly bool _isCompute;

        public HazardCharacterisationsSummarizer(bool isCompute, HazardCharacterisationsModuleConfig config) : base(config) {
            _isCompute = isCompute;
        }

        public override void Summarize(
            ActionModuleConfig sectionConfig,
            HazardCharacterisationsActionResult result,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var outputSettings = new ModuleOutputSectionsManager<HazardCharacterisationsSections>(sectionConfig, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }
            var subHeader = summarizeSelectedHazardCharacterisations(
                data.BodyWeightUnit,
                data.SelectedEffect,
                data.ActiveSubstances,
                data.AllCompounds,
                data.HazardCharacterisationModelsCollections,
                header,
                order
            );

            if (!_isCompute && outputSettings.ShouldSummarize(HazardCharacterisationsSections.HazardCharacterisationsFromDataSummarySection)
            ) {
                summarizeHazardCharacterisationsFromData(
                    data,
                    _configuration.HCSubgroupDependent,
                    subHeader,
                    order++
                 );
            }

            if (result != null
                && outputSettings.ShouldSummarize(HazardCharacterisationsSections.HazardCharacterisationsFromPoDsBMDsSection)
            ) {
                summarizeHazardCharacterisationsFromPodsBmds(
                    data.SelectedEffect,
                    result,
                    subHeader,
                    order++
                 );
            }

            if ((result?.HazardCharacterisationsFromIvive?.Any() ?? false)
                && outputSettings.ShouldSummarize(HazardCharacterisationsSections.HazardCharacterisationsFromIVIVESection)
            ) {
                summarizeIviveHazardCharacterisations(
                    result,
                    data.SelectedEffect,
                    _configuration.TargetDoseLevelType,
                    subHeader,
                    order++
                );
            }

            if ((result?.ImputedHazardCharacterisations?.Any() ?? false)
                && outputSettings.ShouldSummarize(HazardCharacterisationsSections.HazardCharacterisationImputationRecordsSection)
            ) {
                summarizeImputatedHazardCharacterisations(
                    data.SelectedEffect,
                    result,
                    subHeader,
                    order++
                );
            }

            if (result != null &&
                outputSettings.ShouldSummarize(HazardCharacterisationsSections.KineticModelsSection)
            ) {
                summarizeAvailableTargetDosesTimeCourses(
                    result,
                    data,
                    subHeader,
                    order++
                );
            }
        }

        public void SummarizeUncertain(
            HazardCharacterisationsActionResult result,
            ActionData data,
            SectionHeader header
        ) {
            if (_configuration.ReSampleRPFs) {
                summarizeSelectedHazardCharacterisationsUncertain(data, header);
            }
        }

        private List<ActionSummaryUnitRecord> collectUnits(
            TargetUnit hazardCharacterisationsUnit,
            BodyWeightUnit bodyWeightUnit
        ) {
            var targetConcentrationUnit = new TargetUnit(
                ExposureTarget.DefaultInternalExposureTarget,
                hazardCharacterisationsUnit.SubstanceAmountUnit,
                hazardCharacterisationsUnit.ConcentrationMassUnit
            );
            var result = new List<ActionSummaryUnitRecord> {
                new("LowerBound", $"p{_configuration.UncertaintyLowerBound}"),
                new("UpperBound", $"p{_configuration.UncertaintyUpperBound}"),
                // TODO: the units below seem to be used by kinetic model time course view
                // check whether this works for multiple targets.
                new("TargetAmountUnit", hazardCharacterisationsUnit.SubstanceAmountUnit.GetShortDisplayName()),
                new("TargetConcentrationUnit", targetConcentrationUnit.GetShortDisplayName()),
                new("BodyWeightUnit", bodyWeightUnit.GetShortDisplayName()),
                new("ExternalExposureUnit", hazardCharacterisationsUnit.GetShortDisplayName())
            };
            return result;
        }

        private SectionHeader summarizeSelectedHazardCharacterisations(
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
                _configuration.TargetDoseLevelType,
                _configuration.ExposureType,
                _configuration.TargetDosesCalculationMethod,
                _configuration.UseDoseResponseModels,
                _configuration.UseAdditionalAssessmentFactor,
                _configuration.AdditionalAssessmentFactor,
                _configuration.HazardCharacterisationsConvertToSingleTargetMatrix,
                _isCompute,
                _configuration.UseBMDL,
                _configuration.ReSampleRPFs
            );
            subHeader.Units = collectUnits(
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

        private SectionHeader summarizeHazardCharacterisationsFromData(
            ActionData data,
            bool hcSubgroupDependent,
            SectionHeader header,
            int order
        ) {
            var section = new HazardCharacterisationsFromDataSummarySection() {
                SectionLabel = getSectionLabel(HazardCharacterisationsSections.HazardCharacterisationsFromDataSummarySection)
            };
            var subHeader = header.AddSubSectionHeaderFor(
                section,
                "Hazard characterisations from data",
                order
            );
            section.Summarize(
                data.SelectedEffect,
                data.HazardCharacterisationModelsCollections,
                hcSubgroupDependent
            );
            subHeader.SaveSummarySection(section);
            return subHeader;
        }


        private SectionHeader summarizeHazardCharacterisationsFromPodsBmds(
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
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var kineticModelInstances = data.KineticModelInstances;
            var exposureType = _configuration.ExposureType;
            var activeSubstance = data.ActiveSubstances;
            var targetUnits = new List<TargetUnit>() { result.TargetDoseUnit };
            var exposurePathTypes = result.ExposureRoutes;
            if (result.KineticModelDrilldownRecords?.Any() ?? false) {
                var subHeader = header.AddEmptySubSectionHeader(
                    "Kinetic models",
                    order++,
                    getSectionLabel(HazardCharacterisationsSections.KineticModelsSection)
                );
                var subOrder = 0;
                var linearModelCompounds = new HashSet<Compound>();
                foreach (var record in result.KineticModelDrilldownRecords) {
                    foreach (var substance in activeSubstance) {
                        if (record.AggregateIndividualExposure.InternalTargetExposures.Values.Any(r => r.Values.Any(x => x is SubstanceTargetExposurePattern))) {
                            var section = new KineticModelTimeCourseSection();
                            var kineticModelInstance = kineticModelInstances.Single(c => c.Substances.Contains(substance));
                            var subHeader1 = subHeader.AddSubSectionHeaderFor(
                                section: section,
                                title: $"Hazard characterisations drilldown PBPK model {substance.Name}", 
                                order: subOrder++
                            );
                            var internalDoseUnit = TargetUnit.FromInternalDoseUnit(
                                record.HcModel.TestSystemHazardCharacterisation.DoseUnit,
                                BiologicalMatrixConverter.FromString(record.HcModel.TestSystemHazardCharacterisation.Organ)
                            );
                            section.Summarize(
                                new List<(AggregateIndividualExposure, IHazardCharacterisationModel)>() { record },
                                exposurePathTypes,
                                substance,
                                kineticModelInstance,
                                targetUnits,
                                internalDoseUnit.ExposureUnit,
                                exposureType
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
