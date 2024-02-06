using MCRA.General.TableDefinitions.RawTableFieldEnums;
using MCRA.General;

namespace MCRA.Data.Raw.Constants {
    public static class RawTableIdToFieldEnums {
        private static Dictionary<Type, RawDataSourceTableID> _enumToIdMap;
        private static Dictionary<RawDataSourceTableID, Type> _idToEnumMap;

        /// <summary>
        /// IdToEnumMap
        /// </summary>
        public static IReadOnlyDictionary<RawDataSourceTableID, Type> IdToEnumMap {
            get {
                if (_idToEnumMap == null) {
                    initializeMaps();
                }
                return _idToEnumMap;
            }
        }

        /// <summary>
        /// EnumToIdMap
        /// </summary>
        public static IReadOnlyDictionary<Type, RawDataSourceTableID> EnumToIdMap {
            get {
                if (_enumToIdMap == null) {
                    initializeMaps();
                }
                return _enumToIdMap;
            }
        }

        private static void initializeMaps() {
            var enumType = typeof(RawAgriculturalUses);

            _enumToIdMap = new Dictionary<Type, RawDataSourceTableID> {
                {typeof(RawAnalysisSamples), RawDataSourceTableID.AnalysisSamples},
                {typeof(RawAnalyticalMethodCompounds), RawDataSourceTableID.AnalyticalMethodCompounds },
                {typeof(RawAnalyticalMethods), RawDataSourceTableID.AnalyticalMethods },
                {typeof(RawAuthorisedUses), RawDataSourceTableID.AuthorisedUses },
                {typeof(RawCompounds), RawDataSourceTableID.Compounds },
                {typeof(RawConcentrationsPerSample), RawDataSourceTableID.ConcentrationsPerSample },
                {typeof(RawConcentrationSingleValues), RawDataSourceTableID.ConcentrationSingleValues },
                {typeof(RawFoodConsumptions), RawDataSourceTableID.Consumptions },
                {typeof(RawPopulationConsumptionSingleValues), RawDataSourceTableID.PopulationConsumptionSingleValues },
                {typeof(RawFacets), RawDataSourceTableID.Facets },
                {typeof(RawFacetDescriptors), RawDataSourceTableID.FacetDescriptors },
                {typeof(RawFoodConsumptionQuantifications), RawDataSourceTableID.FoodConsumptionQuantifications },
                {typeof(RawFoodConsumptionUncertainties), RawDataSourceTableID.FoodConsumptionUncertainties },
                {typeof(RawReadAcrossFoodTranslations), RawDataSourceTableID.FoodExtrapolations },
                {typeof(RawFoodHierarchies), RawDataSourceTableID.FoodHierarchies },
                {typeof(RawFoodOrigins), RawDataSourceTableID.FoodOrigins },
                {typeof(RawFoodProperties), RawDataSourceTableID.FoodProperties},
                {typeof(RawFoodUnitWeights), RawDataSourceTableID.FoodUnitWeights },
                {typeof(RawFoods), RawDataSourceTableID.Foods },
                {typeof(RawFoodSamples), RawDataSourceTableID.FoodSamples },
                {typeof(RawSampleProperties), RawDataSourceTableID.SampleProperties },
                {typeof(RawSamplePropertyValues), RawDataSourceTableID.SamplePropertyValues },
                {typeof(RawFoodSurveys), RawDataSourceTableID.FoodSurveys },
                {typeof(RawFoodTranslations), RawDataSourceTableID.FoodTranslations },
                {typeof(RawIndividuals), RawDataSourceTableID.Individuals },
                {typeof(RawIndividualDays), RawDataSourceTableID.IndividualDays },
                {typeof(RawIndividualProperties), RawDataSourceTableID.IndividualProperties },
                {typeof(RawIndividualPropertyValues), RawDataSourceTableID.IndividualPropertyValues },
                {typeof(RawMarketShares), RawDataSourceTableID.MarketShares },
                {typeof(RawConcentrationDistributions), RawDataSourceTableID.ConcentrationDistributions },
                {typeof(RawOccurrenceFrequencies), RawDataSourceTableID.OccurrenceFrequencies },
                {typeof(RawProcessingTypes), RawDataSourceTableID.ProcessingTypes },
                {typeof(RawProcessingFactors), RawDataSourceTableID.ProcessingFactors },
                {typeof(RawIestiSpecialCases), RawDataSourceTableID.IestiSpecialCases },
                {typeof(RawUnitVariabilityFactors), RawDataSourceTableID.UnitVariabilityFactors },
                {typeof(RawAgriculturalUses), RawDataSourceTableID.AgriculturalUses },
                {typeof(RawAgriculturalUses_has_Compounds), RawDataSourceTableID.AgriculturalUsesHasCompounds },
                {typeof(RawMaximumResidueLimits), RawDataSourceTableID.MaximumResidueLimits },
                {typeof(RawEffects), RawDataSourceTableID.Effects },
                {typeof(RawTestSystems), RawDataSourceTableID.TestSystems },
                {typeof(RawResponses), RawDataSourceTableID.Responses },
                {typeof(RawRelativePotencyFactors), RawDataSourceTableID.RelativePotencyFactors },
                {typeof(RawRelativePotencyFactorsUncertain), RawDataSourceTableID.RelativePotencyFactorsUncertain },
                {typeof(RawHazardDoses), RawDataSourceTableID.HazardDoses },
                {typeof(RawHazardDosesUncertain), RawDataSourceTableID.HazardDosesUncertain },
                {typeof(RawAssessmentGroupMemberships), RawDataSourceTableID.AssessmentGroupMemberships },
                {typeof(RawAssessmentGroupMembershipModels), RawDataSourceTableID.AssessmentGroupMembershipModels },
                {typeof(RawMolecularDockingModels), RawDataSourceTableID.MolecularDockingModels },
                {typeof(RawMolecularBindingEnergies), RawDataSourceTableID.MolecularBindingEnergies },
                {typeof(RawQSARMembershipModels), RawDataSourceTableID.QsarMembershipModels },
                {typeof(RawQSARMembershipScores), RawDataSourceTableID.QsarMembershipScores },
                {typeof(RawInterSpeciesModelParameters), RawDataSourceTableID.InterSpeciesModelParameters },
                {typeof(RawIntraSpeciesModelParameters), RawDataSourceTableID.IntraSpeciesModelParameters },
                {typeof(RawDietaryExposureModels), RawDataSourceTableID.DietaryExposureModels },
                {typeof(RawDietaryExposurePercentiles), RawDataSourceTableID.DietaryExposurePercentiles },
                {typeof(RawDietaryExposurePercentileUncertains), RawDataSourceTableID.DietaryExposurePercentilesUncertain },
                {typeof(RawNonDietaryExposureSources), RawDataSourceTableID.NonDietaryExposureSources },
                {typeof(RawNonDietaryExposures), RawDataSourceTableID.NonDietaryExposures },
                {typeof(RawNonDietaryExposuresUncertain), RawDataSourceTableID.NonDietaryExposuresUncertain },
                {typeof(RawNonDietarySurveys), RawDataSourceTableID.NonDietarySurveys },
                {typeof(RawNonDietarySurveyProperties), RawDataSourceTableID.NonDietarySurveyProperties },
                {typeof(RawTDSFoodSampleCompositions), RawDataSourceTableID.TdsFoodSampleCompositions },
                {typeof(RawDoseResponseExperiments), RawDataSourceTableID.DoseResponseExperiments },
                {typeof(RawDoseResponseExperimentDoses), RawDataSourceTableID.DoseResponseExperimentDoses },
                {typeof(RawExperimentalUnitProperties), RawDataSourceTableID.ExperimentalUnitProperties },
                {typeof(RawDoseResponseExperimentMeasurements), RawDataSourceTableID.DoseResponseExperimentMeasurements },
                {typeof(RawKineticModelInstanceParameters), RawDataSourceTableID.KineticModelInstanceParameters },
                {typeof(RawKineticModelInstances), RawDataSourceTableID.KineticModelInstances },
                {typeof(RawKineticAbsorptionFactors), RawDataSourceTableID.KineticAbsorptionFactors },
                {typeof(RawKineticConversionFactors), RawDataSourceTableID.KineticConversionFactors },
                {typeof(RawKineticConversionFactorSGs), RawDataSourceTableID.KineticConversionFactorSGs },

                {typeof(RawDoseResponseModels), RawDataSourceTableID.DoseResponseModels },
                {typeof(RawDoseResponseModelBenchmarkDoses), RawDataSourceTableID.DoseResponseModelBenchmarkDoses },
                {typeof(RawDoseResponseModelBenchmarkDosesUncertain), RawDataSourceTableID.DoseResponseModelBenchmarkDosesUncertain },
                {typeof(RawEffectRepresentations), RawDataSourceTableID.EffectRepresentations },
                {typeof(RawAdverseOutcomePathwayNetworks), RawDataSourceTableID.AdverseOutcomePathwayNetworks },
                {typeof(RawEffectRelations), RawDataSourceTableID.EffectRelations },
                {typeof(RawResidueDefinitions), RawDataSourceTableID.ResidueDefinitions },
                {typeof(RawDeterministicSubstanceConversionFactors), RawDataSourceTableID.DeterministicSubstanceConversionFactors },
                {typeof(RawSubstanceApprovals), RawDataSourceTableID.SubstanceApprovals },

                {typeof(RawHumanMonitoringSurveys), RawDataSourceTableID.HumanMonitoringSurveys },
                {typeof(RawHumanMonitoringSamples), RawDataSourceTableID.HumanMonitoringSamples },
                {typeof(RawHumanMonitoringSampleAnalyses), RawDataSourceTableID.HumanMonitoringSampleAnalyses },
                {typeof(RawHumanMonitoringSampleConcentrations), RawDataSourceTableID.HumanMonitoringSampleConcentrations },

                {typeof(RawPopulations), RawDataSourceTableID.Populations },
                {typeof(RawPopulationIndividualPropertyValues), RawDataSourceTableID.PopulationIndividualPropertyValues },

                {typeof(RawHazardCharacterisations), RawDataSourceTableID.HazardCharacterisations },
                {typeof(RawHazardCharacterisationsUncertain), RawDataSourceTableID.HazardCharacterisationsUncertain },
                {typeof(RawHCSubgroups), RawDataSourceTableID.HCSubgroups },
                {typeof(RawHCSubgroupsUncertain), RawDataSourceTableID.HCSubgroupsUncertain },

                {typeof(RawTargetExposureModels), RawDataSourceTableID.TargetExposureModels },
                {typeof(RawTargetExposurePercentiles), RawDataSourceTableID.TargetExposurePercentiles },
                {typeof(RawTargetExposurePercentileUncertains), RawDataSourceTableID.TargetExposurePercentilesUncertain },

                {typeof(RawRiskModels), RawDataSourceTableID.RiskModels },
                {typeof(RawRiskPercentiles), RawDataSourceTableID.RiskPercentiles },
                {typeof(RawRiskPercentileUncertains), RawDataSourceTableID.RiskPercentilesUncertain },
                {typeof(RawExposureBiomarkerConversions), RawDataSourceTableID.ExposureBiomarkerConversions },


                //Types not translated to raw tables:
                {typeof(RawSampleYears), RawDataSourceTableID.Unknown },
                {typeof(RawSampleLocations), RawDataSourceTableID.Unknown },
                {typeof(RawSampleRegions), RawDataSourceTableID.Unknown },
                {typeof(RawSampleProductionMethods), RawDataSourceTableID.Unknown },
                {typeof(RawTwoWayTableData), RawDataSourceTableID.Unknown },
            };

            //reverse map, exclude Unknown table id's
            _idToEnumMap = _enumToIdMap.Where(kvp => kvp.Value != RawDataSourceTableID.Unknown)
                .ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
        }
    }
}
