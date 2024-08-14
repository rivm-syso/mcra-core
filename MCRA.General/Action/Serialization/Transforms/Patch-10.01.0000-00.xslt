<?xml version="1.0" encoding="utf-8"?>
<!--
Stylesheet for transforming to the new project settings configuration
-->
<xsl:stylesheet version="2.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl"
>
  <xsl:output method="xml" indent="yes"/>
  <!-- copy all nodes and attributes, applying the templates hereafter -->
  <xsl:template match="@*|node()">
    <xsl:copy>
      <xsl:apply-templates select="@*|node()"/>
    </xsl:copy>
  </xsl:template>

  <!-- Clear all current settings items -->
  <xsl:template match="SelectedCompounds" />
  <xsl:template match="FocalFoods" />
  <xsl:template match="FoodAsEatenSubset" />
  <xsl:template match="ModelledFoodSubset" />
  <xsl:template match="SelectedScenarioAnalysisFoods" />
  <xsl:template match="SamplesSubsetDefinitions" />
  <xsl:template match="IndividualsSubsetDefinitions" />
  <xsl:template match="FocalFoodAsEatenSubset" />
  <xsl:template match="FocalFoodAsMeasuredSubset" />
  <xsl:template match="SelectedFoodSurveySubsetProperties" />
  <xsl:template match="SelectedHbmSurveySubsetProperties" />
  <xsl:template match="AgriculturalUseSettings" />
  <xsl:template match="ConcentrationModelSettings" />
  <xsl:template match="ConversionSettings" />
  <xsl:template match="CovariatesSelectionSettings" />
  <xsl:template match="DietaryIntakeCalculationSettings" />
  <xsl:template match="DoseResponseModelsSettings" />
  <xsl:template match="EffectSettings" />
  <xsl:template match="ExposureBiomarkerConversionsSettings" />
  <xsl:template match="FoodSurveySettings" />
  <xsl:template match="HumanMonitoringSettings" />
  <xsl:template match="KineticModelSettings" />
  <xsl:template match="MixtureSelectionSettings" />
  <xsl:template match="NonDietarySettings" />
  <xsl:template match="PopulationSettings" />
  <xsl:template match="RisksSettings" />
  <xsl:template match="ScreeningSettings" />
  <xsl:template match="UnitVariabilitySettings" />
  <xsl:template match="AmountModelSettings" />
  <xsl:template match="AssessmentSettings" />
  <xsl:template match="FrequencyModelSettings" />
  <xsl:template match="IndividualDaySubsetDefinition" />
  <xsl:template match="IntakeModelSettings" />
  <xsl:template match="LocationSubsetDefinition" />
  <xsl:template match="MonteCarloSettings" />
  <xsl:template match="OutputDetailSettings" />
  <xsl:template match="PeriodSubsetDefinition" />
  <xsl:template match="ScenarioAnalysisSettings" />
  <xsl:template match="SubsetSettings" />
  <xsl:template match="UncertaintyAnalysisSettings" />

  <!-- Add any new single elements that should be added under /Project here -->
  <xsl:template match="/Project">
    <xsl:copy>
      <!-- copy all nodes and attributes, applying the templates hereafter -->
      <xsl:apply-templates select="@*|node()"/>
      <!-- Add new ModuleConfigurations element which will contain all settings as 'Setting' type items -->
      <!-- ONLY when it doesn't exist yet, to safeguard against reapplying this patch -->
      <xsl:if test="not(ModuleConfigurations)">
      <ModuleConfigurations>
        <!-- Add new Global ActionSettings ModuleConfiguration -->
        <ModuleConfiguration module="Action">
          <Settings>
            <xsl:if test="MonteCarloSettings/RandomSeed"><Setting id="RandomSeed"><xsl:value-of select="MonteCarloSettings/RandomSeed"/></Setting></xsl:if>
            <xsl:if test="AssessmentSettings/ExposureType"><Setting id="ExposureType"><xsl:value-of select="AssessmentSettings/ExposureType"/></Setting></xsl:if>
            <xsl:if test="UncertaintyAnalysisSettings/DoUncertaintyAnalysis"><Setting id="DoUncertaintyAnalysis"><xsl:value-of select="UncertaintyAnalysisSettings/DoUncertaintyAnalysis"/></Setting></xsl:if>
            <xsl:if test="UncertaintyAnalysisSettings/DoUncertaintyFactorial"><Setting id="DoUncertaintyFactorial"><xsl:value-of select="UncertaintyAnalysisSettings/DoUncertaintyFactorial"/></Setting></xsl:if>
            <xsl:if test="UncertaintyAnalysisSettings/UncertaintyLowerBound"><Setting id="UncertaintyLowerBound"><xsl:value-of select="UncertaintyAnalysisSettings/UncertaintyLowerBound"/></Setting></xsl:if>
            <xsl:if test="UncertaintyAnalysisSettings/UncertaintyUpperBound"><Setting id="UncertaintyUpperBound"><xsl:value-of select="UncertaintyAnalysisSettings/UncertaintyUpperBound"/></Setting></xsl:if>
            <xsl:if test="UncertaintyAnalysisSettings/NumberOfResampleCycles"><Setting id="UncertaintyAnalysisCycles"><xsl:value-of select="UncertaintyAnalysisSettings/NumberOfResampleCycles"/></Setting></xsl:if>
            <xsl:if test="UncertaintyAnalysisSettings/NumberOfIterationsPerResampledSet"><Setting id="UncertaintyIterationsPerResampledSet"><xsl:value-of select="UncertaintyAnalysisSettings/NumberOfIterationsPerResampledSet"/></Setting></xsl:if>
            <xsl:if test="OutputDetailSettings/LowerPercentage"><Setting id="VariabilityLowerPercentage"><xsl:value-of select="OutputDetailSettings/LowerPercentage"/></Setting></xsl:if>
            <xsl:if test="OutputDetailSettings/UpperPercentage"><Setting id="VariabilityUpperPercentage"><xsl:value-of select="OutputDetailSettings/UpperPercentage"/></Setting></xsl:if>
            <xsl:if test="OutputDetailSettings/PercentageForUpperTail"><Setting id="VariabilityUpperTailPercentage"><xsl:value-of select="OutputDetailSettings/PercentageForUpperTail"/></Setting></xsl:if>
            <xsl:if test="OutputDetailSettings/PercentageForDrilldown"><Setting id="VariabilityDrilldownPercentage"><xsl:value-of select="OutputDetailSettings/PercentageForDrilldown"/></Setting></xsl:if>
            <xsl:if test="OutputDetailSettings/SkipPrivacySensitiveOutputs"><Setting id="SkipPrivacySensitiveOutputs"><xsl:value-of select="OutputDetailSettings/SkipPrivacySensitiveOutputs"/></Setting></xsl:if>
            <xsl:if test="OutputDetailSettings/MaximumCumulativeRatioCutOff"><Setting id="McrPlotRatioCutOff"><xsl:value-of select="OutputDetailSettings/MaximumCumulativeRatioCutOff"/></Setting></xsl:if>
            <xsl:if test="OutputDetailSettings/MaximumCumulativeRatioMinimumPercentage"><Setting id="McrPlotMinimumPercentage"><xsl:value-of select="OutputDetailSettings/MaximumCumulativeRatioMinimumPercentage"/></Setting></xsl:if>
            <xsl:if test="MixtureSelectionSettings/RatioCutOff"><Setting id="McrCalculationRatioCutOff"><xsl:value-of select="MixtureSelectionSettings/RatioCutOff"/></Setting></xsl:if>
            <xsl:if test="MixtureSelectionSettings/TotalExposureCutOff"><Setting id="McrCalculationTotalExposureCutOff"><xsl:value-of select="MixtureSelectionSettings/TotalExposureCutOff"/></Setting></xsl:if>
            <xsl:if test="OutputDetailSettings/OutputSectionSelectionMethod"><Setting id="OutputSectionSelectionMethod"><xsl:value-of select="OutputDetailSettings/OutputSectionSelectionMethod"/></Setting></xsl:if>
            <xsl:if test="OutputDetailSettings/OutputSections">
              <Setting id="OutputSections">
                <xsl:call-template name="listElements"><xsl:with-param name="items" select="OutputDetailSettings/OutputSections/Label"/></xsl:call-template>
              </Setting>
            </xsl:if>
            <xsl:if test="OutputDetailSettings/SelectedPercentiles">
              <Setting id="SelectedPercentiles">
                <xsl:call-template name="listElements"><xsl:with-param name="items" select="OutputDetailSettings/SelectedPercentiles/double"/></xsl:call-template>
              </Setting>
            </xsl:if>
            <xsl:if test="OutputDetailSettings/MaximumCumulativeRatioPercentiles">
              <Setting id="McrPlotPercentiles">
                <xsl:call-template name="listElements"><xsl:with-param name="items" select="OutputDetailSettings/MaximumCumulativeRatioPercentiles/double"/></xsl:call-template>
              </Setting>
            </xsl:if>
            <xsl:choose>
              <xsl:when test="RisksSettings/SingleValueRisksCalculationTier"><Setting id="SelectedTier"><xsl:value-of select="RisksSettings/SingleValueRisksCalculationTier"/></Setting></xsl:when>
              <xsl:when test="RisksSettings/RiskCalculationTier"><Setting id="SelectedTier"><xsl:value-of select="RisksSettings/RiskCalculationTier"/></Setting></xsl:when>
              <xsl:when test="DietaryIntakeCalculationSettings/DietaryIntakeCalculationTier"><Setting id="SelectedTier"><xsl:value-of select="DietaryIntakeCalculationSettings/DietaryIntakeCalculationTier"/></Setting></xsl:when>
              <xsl:when test="ConcentrationModelSettings/ConcentrationModelChoice"><Setting id="SelectedTier"><xsl:value-of select="ConcentrationModelSettings/ConcentrationModelChoice"/></Setting></xsl:when>
              <xsl:when test="ConcentrationModelSettings/ConcentrationsTier"><Setting id="SelectedTier"><xsl:value-of select="ConcentrationModelSettings/ConcentrationsTier"/></Setting></xsl:when>
              <xsl:when test="AgriculturalUseSettings/OccurrencePatternsTier"><Setting id="SelectedTier"><xsl:value-of select="AgriculturalUseSettings/OccurrencePatternsTier"/></Setting></xsl:when>
              <xsl:when test="FoodSurveySettings/ConsumptionsTier"><Setting id="SelectedTier"><xsl:value-of select="FoodSurveySettings/ConsumptionsTier"/></Setting></xsl:when>
              <xsl:otherwise><Setting id="SelectedTier">Custom</Setting></xsl:otherwise>
            </xsl:choose>
          </Settings>
        </ModuleConfiguration>
        <!-- Add new ActiveSubstances ModuleConfiguration -->
        <ModuleConfiguration module="ActiveSubstances">
          <Settings>
            <xsl:if test="EffectSettings/PriorMembershipProbability"><Setting id="PriorMembershipProbability"><xsl:value-of select="EffectSettings/PriorMembershipProbability"/></Setting></xsl:if>
            <xsl:if test="EffectSettings/AssessmentGroupMembershipCalculationMethod"><Setting id="AssessmentGroupMembershipCalculationMethod"><xsl:value-of select="EffectSettings/AssessmentGroupMembershipCalculationMethod"/></Setting></xsl:if>
            <xsl:if test="EffectSettings/CombinationMethodMembershipInfoAndPodPresence"><Setting id="CombinationMethodMembershipInfoAndPodPresence"><xsl:value-of select="EffectSettings/CombinationMethodMembershipInfoAndPodPresence"/></Setting></xsl:if>
            <xsl:if test="EffectSettings/RestrictToAvailableHazardCharacterisations"><Setting id="FilterByAvailableHazardCharacterisation"><xsl:value-of select="EffectSettings/RestrictToAvailableHazardCharacterisations"/></Setting></xsl:if>
            <xsl:if test="EffectSettings/RestrictToAvailableHazardDoses"><Setting id="FilterByAvailableHazardDose"><xsl:value-of select="EffectSettings/RestrictToAvailableHazardDoses"/></Setting></xsl:if>
            <xsl:if test="EffectSettings/RestrictToCertainMembership"><Setting id="FilterByCertainAssessmentGroupMembership"><xsl:value-of select="EffectSettings/RestrictToCertainMembership"/></Setting></xsl:if>
            <xsl:if test="EffectSettings/IncludeSubstancesWithUnknowMemberships"><Setting id="IncludeSubstancesWithUnknowMemberships"><xsl:value-of select="EffectSettings/IncludeSubstancesWithUnknowMemberships"/></Setting></xsl:if>
            <xsl:if test="EffectSettings/UseMolecularDockingModels"><Setting id="UseMolecularDockingModels"><xsl:value-of select="EffectSettings/UseMolecularDockingModels"/></Setting></xsl:if>
            <xsl:if test="EffectSettings/UseProbabilisticMemberships"><Setting id="UseProbabilisticMemberships"><xsl:value-of select="EffectSettings/UseProbabilisticMemberships"/></Setting></xsl:if>
            <xsl:if test="EffectSettings/UseQsarModels"><Setting id="UseQsarModels"><xsl:value-of select="EffectSettings/UseQsarModels"/></Setting></xsl:if>
            <xsl:if test="UncertaintyAnalysisSettings/ReSampleAssessmentGroupMemberships"><Setting id="ResampleAssessmentGroupMemberships"><xsl:value-of select="UncertaintyAnalysisSettings/ReSampleAssessmentGroupMemberships"/></Setting></xsl:if>
          </Settings>
        </ModuleConfiguration>
        <!-- Add new Effects ModuleConfiguration -->
        <ModuleConfiguration module="Effects">
          <Settings>
            <xsl:if test="EffectSettings/CodeFocalEffect"><Setting id="CodeFocalEffect"><xsl:value-of select="EffectSettings/CodeFocalEffect"/></Setting></xsl:if>
            <xsl:if test="EffectSettings/IncludeAopNetwork"><Setting id="IncludeAopNetwork"><xsl:value-of select="EffectSettings/IncludeAopNetwork"/></Setting></xsl:if>
            <xsl:if test="EffectSettings/MultipleEffects"><Setting id="MultipleEffects"><xsl:value-of select="EffectSettings/MultipleEffects"/></Setting></xsl:if>
          </Settings>
        </ModuleConfiguration>
        <!-- Add new ExposureBiomarkerConversions ModuleConfiguration -->
        <ModuleConfiguration module="ExposureBiomarkerConversions">
          <Settings>
            <xsl:if test="ExposureBiomarkerConversionsSettings/EBCSubgroupDependent"><Setting id="EBCSubgroupDependent"><xsl:value-of select="ExposureBiomarkerConversionsSettings/EBCSubgroupDependent"/></Setting></xsl:if>
          </Settings>
        </ModuleConfiguration>
        <!-- Add new Substances ModuleConfiguration -->
        <ModuleConfiguration module="Substances">
          <Settings>
            <xsl:if test="AssessmentSettings/Cumulative"><Setting id="Cumulative"><xsl:value-of select="AssessmentSettings/Cumulative"/></Setting></xsl:if>
            <xsl:if test="EffectSettings/CodeReferenceCompound"><Setting id="CodeReferenceSubstance"><xsl:value-of select="EffectSettings/CodeReferenceCompound"/></Setting></xsl:if>
            <xsl:if test="AssessmentSettings/MultipleSubstances"><Setting id="MultipleSubstances"><xsl:value-of select="AssessmentSettings/MultipleSubstances"/></Setting></xsl:if>
          </Settings>
        </ModuleConfiguration>
        <!-- Add new AopNetworks ModuleConfiguration -->
        <ModuleConfiguration module="AOPNetworks">
          <Settings>
            <xsl:if test="EffectSettings/CodeAopNetwork"><Setting id="CodeAopNetwork"><xsl:value-of select="EffectSettings/CodeAopNetwork"/></Setting></xsl:if>
            <xsl:if test="EffectSettings/CodeFocalUpstreamEffect"><Setting id="CodeFocalUpstreamEffect"><xsl:value-of select="EffectSettings/CodeFocalUpstreamEffect"/></Setting></xsl:if>
            <xsl:if test="EffectSettings/RestrictAopByFocalUpstreamEffect"><Setting id="RestrictAopByFocalUpstreamEffect"><xsl:value-of select="EffectSettings/RestrictAopByFocalUpstreamEffect"/></Setting></xsl:if>
          </Settings>
        </ModuleConfiguration>
        <!-- Add new DoseResponseData ModuleConfiguration -->
        <ModuleConfiguration module="DoseResponseData">
          <Settings>
            <xsl:if test="EffectSettings/MergeDoseResponseExperimentsData"><Setting id="MergeDoseResponseExperimentsData"><xsl:value-of select="EffectSettings/MergeDoseResponseExperimentsData"/></Setting></xsl:if>
          </Settings>
        </ModuleConfiguration>
        <!-- Add new BiologicalMatrixConcentrationComparisons element -->
        <ModuleConfiguration module="BiologicalMatrixConcentrationComparisons">
          <Settings>
            <xsl:if test="HumanMonitoringSettings/CorrelateTargetConcentrations"><Setting id="CorrelateTargetConcentrations"><xsl:value-of select="HumanMonitoringSettings/CorrelateTargetConcentrations"/></Setting></xsl:if>
          </Settings>
        </ModuleConfiguration>
        <ModuleConfiguration module="ConcentrationModels">
          <Settings>
            <xsl:if test="ConcentrationModelSettings/DefaultConcentrationModel"><Setting id="DefaultConcentrationModel"><xsl:value-of select="ConcentrationModelSettings/DefaultConcentrationModel"/></Setting></xsl:if>
            <xsl:if test="ConcentrationModelSettings/IsFallbackMrl"><Setting id="IsFallbackMrl"><xsl:value-of select="ConcentrationModelSettings/IsFallbackMrl"/></Setting></xsl:if>
            <xsl:if test="ConcentrationModelSettings/RestrictLorImputationToAuthorisedUses"><Setting id="RestrictLorImputationToAuthorisedUses"><xsl:value-of select="ConcentrationModelSettings/RestrictLorImputationToAuthorisedUses"/></Setting></xsl:if>
            <xsl:if test="ConcentrationModelSettings/NonDetectsHandlingMethod"><Setting id="NonDetectsHandlingMethod"><xsl:value-of select="ConcentrationModelSettings/NonDetectsHandlingMethod"/></Setting></xsl:if>
            <xsl:if test="ConcentrationModelSettings/FractionOfLOR"><Setting id="FractionOfLor"><xsl:value-of select="ConcentrationModelSettings/FractionOfLOR"/></Setting></xsl:if>
            <xsl:if test="ConcentrationModelSettings/FractionOfMrl"><Setting id="FractionOfMrl"><xsl:value-of select="ConcentrationModelSettings/FractionOfMrl"/></Setting></xsl:if>
            <xsl:if test="ConcentrationModelSettings/IsSampleBased"><Setting id="IsSampleBased"><xsl:value-of select="ConcentrationModelSettings/IsSampleBased"/></Setting></xsl:if>
            <xsl:if test="ConcentrationModelSettings/ImputeMissingValues"><Setting id="ImputeMissingValues"><xsl:value-of select="ConcentrationModelSettings/ImputeMissingValues"/></Setting></xsl:if>
            <xsl:if test="ConcentrationModelSettings/CorrelateImputedValueWithSamplePotency"><Setting id="CorrelateImputedValueWithSamplePotency"><xsl:value-of select="ConcentrationModelSettings/CorrelateImputedValueWithSamplePotency"/></Setting></xsl:if>
            <xsl:if test="AgriculturalUseSettings/UseAgriculturalUseTable"><Setting id="UseAgriculturalUseTable"><xsl:value-of select="AgriculturalUseSettings/UseAgriculturalUseTable"/></Setting></xsl:if>
            <xsl:if test="UncertaintyAnalysisSettings/IsParametric"><Setting id="IsParametric"><xsl:value-of select="UncertaintyAnalysisSettings/IsParametric"/></Setting></xsl:if>

            <xsl:if test="ConcentrationModelSettings/ConcentrationModelTypesFoodSubstance">
              <Setting id="ConcentrationModelTypesFoodSubstance">
                <xsl:copy-of select="ConcentrationModelSettings/ConcentrationModelTypesFoodSubstance/ConcentrationModelTypeFoodSubstance"/>
              </Setting>
            </xsl:if>
          </Settings>
        </ModuleConfiguration>
        <!-- Add new Concentrations element -->
        <ModuleConfiguration module="Concentrations">
          <Settings>
            <xsl:if test="ConcentrationModelSettings/FilterConcentrationLimitExceedingSamples"><Setting id="FilterConcentrationLimitExceedingSamples"><xsl:value-of select="ConcentrationModelSettings/FilterConcentrationLimitExceedingSamples"/></Setting></xsl:if>
            <xsl:if test="ConcentrationModelSettings/ConcentrationLimitFilterFractionExceedanceThreshold"><Setting id="ConcentrationLimitFilterFractionExceedanceThreshold"><xsl:value-of select="ConcentrationModelSettings/ConcentrationLimitFilterFractionExceedanceThreshold"/></Setting></xsl:if>
            <xsl:if test="ConcentrationModelSettings/UseComplexResidueDefinitions"><Setting id="UseComplexResidueDefinitions"><xsl:value-of select="ConcentrationModelSettings/UseComplexResidueDefinitions"/></Setting></xsl:if>
            <xsl:if test="ConcentrationModelSettings/SubstanceTranslationAllocationMethod"><Setting id="SubstanceTranslationAllocationMethod"><xsl:value-of select="ConcentrationModelSettings/SubstanceTranslationAllocationMethod"/></Setting></xsl:if>
            <xsl:if test="ConcentrationModelSettings/RetainAllAllocatedSubstancesAfterAllocation"><Setting id="RetainAllAllocatedSubstancesAfterAllocation"><xsl:value-of select="ConcentrationModelSettings/RetainAllAllocatedSubstancesAfterAllocation"/></Setting></xsl:if>
            <xsl:if test="ConcentrationModelSettings/ConsiderAuthorisationsForSubstanceConversion"><Setting id="ConsiderAuthorisationsForSubstanceConversion"><xsl:value-of select="ConcentrationModelSettings/ConsiderAuthorisationsForSubstanceConversion"/></Setting></xsl:if>
            <xsl:if test="ConcentrationModelSettings/TryFixDuplicateAllocationInconsistencies"><Setting id="TryFixDuplicateAllocationInconsistencies"><xsl:value-of select="ConcentrationModelSettings/TryFixDuplicateAllocationInconsistencies"/></Setting></xsl:if>
            <xsl:if test="ConcentrationModelSettings/ExtrapolateConcentrations"><Setting id="ExtrapolateConcentrations"><xsl:value-of select="ConcentrationModelSettings/ExtrapolateConcentrations"/></Setting></xsl:if>
            <xsl:if test="ConcentrationModelSettings/ThresholdForExtrapolation"><Setting id="ThresholdForExtrapolation"><xsl:value-of select="ConcentrationModelSettings/ThresholdForExtrapolation"/></Setting></xsl:if>
            <xsl:if test="ConcentrationModelSettings/ConsiderMrlForExtrapolations"><Setting id="ConsiderMrlForExtrapolations"><xsl:value-of select="ConcentrationModelSettings/ConsiderMrlForExtrapolations"/></Setting></xsl:if>
            <xsl:if test="ConcentrationModelSettings/ConsiderAuthorisationsForExtrapolations"><Setting id="ConsiderAuthorisationsForExtrapolations"><xsl:value-of select="ConcentrationModelSettings/ConsiderAuthorisationsForExtrapolations"/></Setting></xsl:if>
            <xsl:if test="ConcentrationModelSettings/ImputeWaterConcentrations"><Setting id="ImputeWaterConcentrations"><xsl:value-of select="ConcentrationModelSettings/ImputeWaterConcentrations"/></Setting></xsl:if>
            <xsl:if test="ConcentrationModelSettings/CodeWater"><Setting id="CodeWater"><xsl:value-of select="ConcentrationModelSettings/CodeWater"/></Setting></xsl:if>
            <xsl:if test="ConcentrationModelSettings/WaterConcentrationValue"><Setting id="WaterConcentrationValue"><xsl:value-of select="ConcentrationModelSettings/WaterConcentrationValue"/></Setting></xsl:if>
            <xsl:if test="ConcentrationModelSettings/RestrictWaterImputationToMostPotentSubstances"><Setting id="RestrictWaterImputationToMostPotentSubstances"><xsl:value-of select="ConcentrationModelSettings/RestrictWaterImputationToMostPotentSubstances"/></Setting></xsl:if>
            <xsl:if test="ConcentrationModelSettings/RestrictWaterImputationToAuthorisedUses"><Setting id="RestrictWaterImputationToAuthorisedUses"><xsl:value-of select="ConcentrationModelSettings/RestrictWaterImputationToAuthorisedUses"/></Setting></xsl:if>
            <xsl:if test="ConcentrationModelSettings/RestrictWaterImputationToApprovedSubstances"><Setting id="RestrictWaterImputationToApprovedSubstances"><xsl:value-of select="ConcentrationModelSettings/RestrictWaterImputationToApprovedSubstances"/></Setting></xsl:if>
            <xsl:if test="ConcentrationModelSettings/FocalCommodityScenarioOccurrencePercentage"><Setting id="FocalCommodityScenarioOccurrencePercentage"><xsl:value-of select="ConcentrationModelSettings/FocalCommodityScenarioOccurrencePercentage"/></Setting></xsl:if>
            <xsl:if test="ConcentrationModelSettings/FocalCommodityConcentrationAdjustmentFactor"><Setting id="FocalCommodityConcentrationAdjustmentFactor"><xsl:value-of select="ConcentrationModelSettings/FocalCommodityConcentrationAdjustmentFactor"/></Setting></xsl:if>
            <xsl:if test="ConcentrationModelSettings/UseDeterministicSubstanceConversionsForFocalCommodity"><Setting id="UseDeterministicSubstanceConversionsForFocalCommodity"><xsl:value-of select="ConcentrationModelSettings/UseDeterministicSubstanceConversionsForFocalCommodity"/></Setting></xsl:if>
            <xsl:if test="AssessmentSettings/FocalCommodity"><Setting id="FocalCommodity"><xsl:value-of select="AssessmentSettings/FocalCommodity"/></Setting></xsl:if>
            <xsl:if test="SubsetSettings/SampleSubsetSelection"><Setting id="SampleSubsetSelection"><xsl:value-of select="SubsetSettings/SampleSubsetSelection"/></Setting></xsl:if>
            <xsl:if test="SubsetSettings/RestrictToModelledFoodSubset"><Setting id="RestrictToModelledFoodSubset"><xsl:value-of select="SubsetSettings/RestrictToModelledFoodSubset"/></Setting></xsl:if>
            <xsl:if test="LocationSubsetDefinition/AlignSubsetWithPopulation"><Setting id="AlignSampleLocationSubsetWithPopulation"><xsl:value-of select="LocationSubsetDefinition/AlignSubsetWithPopulation"/></Setting></xsl:if>
            <xsl:if test="LocationSubsetDefinition/IncludeMissingValueRecords"><Setting id="IncludeMissingLocationRecords"><xsl:value-of select="LocationSubsetDefinition/IncludeMissingValueRecords"/></Setting></xsl:if>
            <xsl:if test="PeriodSubsetDefinition/AlignSampleDateSubsetWithPopulation"><Setting id="AlignSampleDateSubsetWithPopulation"><xsl:value-of select="PeriodSubsetDefinition/AlignSampleDateSubsetWithPopulation"/></Setting></xsl:if>
            <xsl:if test="PeriodSubsetDefinition/AlignSampleSeasonSubsetWithPopulation"><Setting id="AlignSampleSeasonSubsetWithPopulation"><xsl:value-of select="PeriodSubsetDefinition/AlignSampleSeasonSubsetWithPopulation"/></Setting></xsl:if>
            <xsl:if test="PeriodSubsetDefinition/IncludeMissingValueRecords"><Setting id="IncludeMissingDateValueRecords"><xsl:value-of select="PeriodSubsetDefinition/IncludeMissingValueRecords"/></Setting></xsl:if>
            <xsl:if test="UncertaintyAnalysisSettings/ReSampleConcentrations"><Setting id="ResampleConcentrations"><xsl:value-of select="UncertaintyAnalysisSettings/ReSampleConcentrations"/></Setting></xsl:if>
            <xsl:if test="LocationSubsetDefinition/LocationSubset">
              <Setting id="FilterSamplesByLocation">true</Setting>
              <Setting id="LocationSubsetDefinition">
                <xsl:copy-of select="LocationSubsetDefinition" />
              </Setting>
            </xsl:if>
            <xsl:if test="PeriodSubsetDefinition/YearsSubset/*">
              <Setting id="FilterSamplesByYear">true</Setting>
            </xsl:if>
            <xsl:if test="PeriodSubsetDefinition/MonthsSubset/*">
              <Setting id="FilterSamplesByMonth">true</Setting>
            </xsl:if>
            <xsl:if test="(PeriodSubsetDefinition/YearsSubset/*) or (PeriodSubsetDefinition/MonthsSubset/*)">
              <Setting id="PeriodSubsetDefinition">
                <xsl:copy-of select="PeriodSubsetDefinition" />
              </Setting>
            </xsl:if>
            <xsl:if test="SamplesSubsetDefinitions/*">
              <Setting id="SamplesSubsetDefinitions">
                <xsl:copy-of select="SamplesSubsetDefinitions/*" />
              </Setting>
            </xsl:if>
          </Settings>
        </ModuleConfiguration>
        <!-- Add new Consumptions ModuleConfiguration -->
        <ModuleConfiguration module="Consumptions">
          <Settings>
            <xsl:if test="FoodSurveySettings/CodeFoodSurvey"><Setting id="CodeFoodSurvey"><xsl:value-of select="FoodSurveySettings/CodeFoodSurvey"/></Setting></xsl:if>
            <xsl:if test="SubsetSettings/ConsumerDaysOnly"><Setting id="ConsumerDaysOnly"><xsl:value-of select="SubsetSettings/ConsumerDaysOnly"/></Setting></xsl:if>
            <xsl:if test="SubsetSettings/RestrictPopulationByFoodAsEatenSubset"><Setting id="RestrictPopulationByFoodAsEatenSubset"><xsl:value-of select="SubsetSettings/RestrictPopulationByFoodAsEatenSubset"/></Setting></xsl:if>
            <xsl:if test="FocalFoodAsEatenSubset">
              <Setting id="FocalFoodAsEatenSubset">
                <xsl:call-template name="listElements"><xsl:with-param name="items" select="FocalFoodAsEatenSubset/string"/></xsl:call-template>
              </Setting>
            </xsl:if>
            <xsl:if test="SubsetSettings/RestrictConsumptionsByFoodAsEatenSubset"><Setting id="RestrictConsumptionsByFoodAsEatenSubset"><xsl:value-of select="SubsetSettings/RestrictConsumptionsByFoodAsEatenSubset"/></Setting></xsl:if>
            <xsl:if test="FoodAsEatenSubset">
              <Setting id="FoodAsEatenSubset">
                <xsl:call-template name="listElements"><xsl:with-param name="items" select="FoodAsEatenSubset/FoodCode"/></xsl:call-template>
              </Setting>
            </xsl:if>
            <xsl:if test="SubsetSettings/MatchIndividualSubsetWithPopulation"><Setting id="MatchIndividualSubsetWithPopulation"><xsl:value-of select="SubsetSettings/MatchIndividualSubsetWithPopulation"/></Setting></xsl:if>
            <xsl:if test="SelectedFoodSurveySubsetProperties">
              <Setting id="SelectedFoodSurveySubsetProperties">
                <xsl:call-template name="listElements"><xsl:with-param name="items" select="SelectedFoodSurveySubsetProperties/string"/></xsl:call-template>
              </Setting>
            </xsl:if>
            <xsl:if test="SubsetSettings/IsDefaultSamplingWeight"><Setting id="IsDefaultSamplingWeight"><xsl:value-of select="SubsetSettings/IsDefaultSamplingWeight"/></Setting></xsl:if>
            <xsl:if test="SubsetSettings/ExcludeIndividualsWithLessThanNDays"><Setting id="ExcludeIndividualsWithLessThanNDays"><xsl:value-of select="SubsetSettings/ExcludeIndividualsWithLessThanNDays"/></Setting></xsl:if>
            <xsl:if test="SubsetSettings/MinimumNumberOfDays"><Setting id="MinimumNumberOfDays"><xsl:value-of select="SubsetSettings/MinimumNumberOfDays"/></Setting></xsl:if>
            <xsl:if test="UncertaintyAnalysisSettings/ResampleIndividuals"><Setting id="ResampleIndividuals"><xsl:value-of select="UncertaintyAnalysisSettings/ResampleIndividuals"/></Setting></xsl:if>
            <xsl:if test="UncertaintyAnalysisSettings/ReSamplePortions"><Setting id="ResamplePortions"><xsl:value-of select="UncertaintyAnalysisSettings/ReSamplePortions"/></Setting></xsl:if>
            <xsl:if test="CovariatesSelectionSettings/NameCofactor"><Setting id="NameCofactor"><xsl:value-of select="CovariatesSelectionSettings/NameCofactor"/></Setting></xsl:if>
            <xsl:if test="CovariatesSelectionSettings/NameCovariable"><Setting id="NameCovariable"><xsl:value-of select="CovariatesSelectionSettings/NameCovariable"/></Setting></xsl:if>

          </Settings>
        </ModuleConfiguration>
        <!-- Add new ConsumptionsByModelledFood ModuleConfiguration -->
        <ModuleConfiguration module="ConsumptionsByModelledFood">
          <Settings>
            <xsl:if test="SubsetSettings/ModelledFoodsConsumerDaysOnly"><Setting id="ModelledFoodsConsumerDaysOnly"><xsl:value-of select="SubsetSettings/ModelledFoodsConsumerDaysOnly"/></Setting></xsl:if>
            <xsl:if test="SubsetSettings/RestrictPopulationByModelledFoodSubset"><Setting id="RestrictPopulationByModelledFoodSubset"><xsl:value-of select="SubsetSettings/RestrictPopulationByModelledFoodSubset"/></Setting></xsl:if>
            <xsl:if test="FocalFoodAsMeasuredSubset">
              <Setting id="FocalFoodAsMeasuredSubset">
                <xsl:call-template name="listElements"><xsl:with-param name="items" select="FocalFoodAsMeasuredSubset/string"/></xsl:call-template>
              </Setting>
            </xsl:if>
          </Settings>
        </ModuleConfiguration>
        <!-- Add new DietaryExposures ModuleConfiguration -->
        <ModuleConfiguration module="DietaryExposures">
          <Settings>
            <xsl:if test="DietaryIntakeCalculationSettings/ImputeExposureDistributions"><Setting id="ImputeExposureDistributions"><xsl:value-of select="DietaryIntakeCalculationSettings/ImputeExposureDistributions"/></Setting></xsl:if>
            <xsl:if test="DietaryIntakeCalculationSettings/DietaryExposuresDetailsLevel"><Setting id="DietaryExposuresDetailsLevel"><xsl:value-of select="DietaryIntakeCalculationSettings/DietaryExposuresDetailsLevel"/></Setting></xsl:if>
            <xsl:if test="DietaryIntakeCalculationSettings/VariabilityDiagnosticsAnalysis"><Setting id="VariabilityDiagnosticsAnalysis"><xsl:value-of select="DietaryIntakeCalculationSettings/VariabilityDiagnosticsAnalysis"/></Setting></xsl:if>
            <xsl:if test="SelectedScenarioAnalysisFoods">
              <Setting id="ScenarioAnalysisFoods">
                <xsl:call-template name="listElements"><xsl:with-param name="items" select="SelectedScenarioAnalysisFoods/FoodCode"/></xsl:call-template>
              </Setting>
            </xsl:if>
            <xsl:if test="ConcentrationModelSettings/IsSingleSamplePerDay"><Setting id="IsSingleSamplePerDay"><xsl:value-of select="ConcentrationModelSettings/IsSingleSamplePerDay"/></Setting></xsl:if>
            <xsl:if test="ConcentrationModelSettings/IsCorrelation"><Setting id="MaximiseCoOccurrenceHighResidues"><xsl:value-of select="ConcentrationModelSettings/IsCorrelation"/></Setting></xsl:if>
            <xsl:if test="MonteCarloSettings/NumberOfMonteCarloIterations"><Setting id="NumberOfMonteCarloIterations"><xsl:value-of select="MonteCarloSettings/NumberOfMonteCarloIterations"/></Setting></xsl:if>
            <xsl:if test="MonteCarloSettings/IsSurveySampling"><Setting id="IsSurveySampling"><xsl:value-of select="MonteCarloSettings/IsSurveySampling"/></Setting></xsl:if>
            <xsl:if test="AmountModelSettings/CovariateModelType"><Setting id="AmountModelCovariateModelType"><xsl:value-of select="AmountModelSettings/CovariateModelType"/></Setting></xsl:if>
            <xsl:if test="AmountModelSettings/FunctionType"><Setting id="AmountModelFunctionType"><xsl:value-of select="AmountModelSettings/FunctionType"/></Setting></xsl:if>
            <xsl:if test="AmountModelSettings/TestingLevel"><Setting id="AmountModelTestingLevel"><xsl:value-of select="AmountModelSettings/TestingLevel"/></Setting></xsl:if>
            <xsl:if test="AmountModelSettings/TestingMethod"><Setting id="AmountModelTestingMethod"><xsl:value-of select="AmountModelSettings/TestingMethod"/></Setting></xsl:if>
            <xsl:if test="AmountModelSettings/MinDegreesOfFreedom"><Setting id="AmountModelMinDegreesOfFreedom"><xsl:value-of select="AmountModelSettings/MinDegreesOfFreedom"/></Setting></xsl:if>
            <xsl:if test="AmountModelSettings/MaxDegreesOfFreedom"><Setting id="AmountModelMaxDegreesOfFreedom"><xsl:value-of select="AmountModelSettings/MaxDegreesOfFreedom"/></Setting></xsl:if>
            <xsl:if test="FrequencyModelSettings/CovariateModelType"><Setting id="FrequencyModelCovariateModelType"><xsl:value-of select="FrequencyModelSettings/CovariateModelType"/></Setting></xsl:if>
            <xsl:if test="FrequencyModelSettings/FunctionType"><Setting id="FrequencyModelFunctionType"><xsl:value-of select="FrequencyModelSettings/FunctionType"/></Setting></xsl:if>
            <xsl:if test="FrequencyModelSettings/TestingLevel"><Setting id="FrequencyModelTestingLevel"><xsl:value-of select="FrequencyModelSettings/TestingLevel"/></Setting></xsl:if>
            <xsl:if test="FrequencyModelSettings/TestingMethod"><Setting id="FrequencyModelTestingMethod"><xsl:value-of select="FrequencyModelSettings/TestingMethod"/></Setting></xsl:if>
            <xsl:if test="FrequencyModelSettings/MinDegreesOfFreedom"><Setting id="FrequencyModelMinDegreesOfFreedom"><xsl:value-of select="FrequencyModelSettings/MinDegreesOfFreedom"/></Setting></xsl:if>
            <xsl:if test="FrequencyModelSettings/MaxDegreesOfFreedom"><Setting id="FrequencyModelMaxDegreesOfFreedom"><xsl:value-of select="FrequencyModelSettings/MaxDegreesOfFreedom"/></Setting></xsl:if>
            <xsl:if test="IntakeModelSettings/IntakeModelType"><Setting id="IntakeModelType"><xsl:value-of select="IntakeModelSettings/IntakeModelType"/></Setting></xsl:if>
            <xsl:if test="IntakeModelSettings/FirstModelThenAdd"><Setting id="IntakeFirstModelThenAdd"><xsl:value-of select="IntakeModelSettings/FirstModelThenAdd"/></Setting></xsl:if>
            <xsl:if test="IntakeModelSettings/CovariateModelling"><Setting id="IntakeCovariateModelling"><xsl:value-of select="IntakeModelSettings/CovariateModelling"/></Setting></xsl:if>
            <xsl:if test="IntakeModelSettings/TransformType"><Setting id="AmountModelTransformType"><xsl:value-of select="IntakeModelSettings/TransformType"/></Setting></xsl:if>
            <xsl:if test="IntakeModelSettings/GridPrecision"><Setting id="IsufModelGridPrecision"><xsl:value-of select="IntakeModelSettings/GridPrecision"/></Setting></xsl:if>
            <xsl:if test="IntakeModelSettings/NumberOfIterations"><Setting id="IsufModelNumberOfIterations"><xsl:value-of select="IntakeModelSettings/NumberOfIterations"/></Setting></xsl:if>
            <xsl:if test="IntakeModelSettings/SplineFit"><Setting id="IsufModelSplineFit"><xsl:value-of select="IntakeModelSettings/SplineFit"/></Setting></xsl:if>
            <xsl:if test="IntakeModelSettings/Dispersion"><Setting id="FrequencyModelDispersion"><xsl:value-of select="IntakeModelSettings/Dispersion"/></Setting></xsl:if>
            <xsl:if test="IntakeModelSettings/VarianceRatio"><Setting id="AmountModelVarianceRatio"><xsl:value-of select="IntakeModelSettings/VarianceRatio"/></Setting></xsl:if>
            <xsl:if test="IntakeModelSettings/IntakeModelsPerCategory/*">
              <Setting id="IntakeModelsPerCategory">
                <xsl:copy-of select="IntakeModelSettings/IntakeModelsPerCategory/*"/>
              </Setting>
            </xsl:if>
            <xsl:if test="AgriculturalUseSettings/UseOccurrencePatternsForResidueGeneration"><Setting id="UseOccurrencePatternsForResidueGeneration"><xsl:value-of select="AgriculturalUseSettings/UseOccurrencePatternsForResidueGeneration"/></Setting></xsl:if>
            <xsl:if test="DietaryIntakeCalculationSettings/AnalyseMcr"><Setting id="McrAnalysis"><xsl:value-of select="DietaryIntakeCalculationSettings/AnalyseMcr"/></Setting></xsl:if>
            <xsl:if test="DietaryIntakeCalculationSettings/ExposureApproachType"><Setting id="McrExposureApproachType"><xsl:value-of select="DietaryIntakeCalculationSettings/ExposureApproachType"/></Setting></xsl:if>
            <xsl:if test="OutputDetailSettings/IsDetailedOutput"><Setting id="IsDetailedOutput"><xsl:value-of select="OutputDetailSettings/IsDetailedOutput"/></Setting></xsl:if>
            <xsl:if test="OutputDetailSettings/ExposureMethod"><Setting id="ExposureMethod"><xsl:value-of select="OutputDetailSettings/ExposureMethod"/></Setting></xsl:if>
            <xsl:if test="OutputDetailSettings/Intervals"><Setting id="IntakeModelPredictionIntervals"><xsl:value-of select="OutputDetailSettings/Intervals"/></Setting></xsl:if>
            <xsl:if test="OutputDetailSettings/ExposureLevels">
              <Setting id="ExposureLevels">
                <xsl:call-template name="listElements"><xsl:with-param name="items" select="OutputDetailSettings/ExposureLevels/double"/></xsl:call-template>
              </Setting>
            </xsl:if>
            <xsl:if test="OutputDetailSettings/ExtraPredictionLevels">
              <Setting id="IntakeExtraPredictionLevels">
                <xsl:call-template name="listElements"><xsl:with-param name="items" select="OutputDetailSettings/ExtraPredictionLevels/double"/></xsl:call-template>
              </Setting>
            </xsl:if>
            <xsl:if test="UncertaintyAnalysisSettings/ReSampleImputationExposureDistributions"><Setting id="ResampleImputationExposureDistributions"><xsl:value-of select="UncertaintyAnalysisSettings/ReSampleImputationExposureDistributions"/></Setting></xsl:if>
            <xsl:if test="SubsetSettings/IsPerPerson"><Setting id="IsPerPerson"><xsl:value-of select="SubsetSettings/IsPerPerson"/></Setting></xsl:if>
            <xsl:if test="ScenarioAnalysisSettings/UseScenario"><Setting id="ReductionToLimitScenario"><xsl:value-of select="ScenarioAnalysisSettings/UseScenario"/></Setting></xsl:if>
            <xsl:if test="UnitVariabilitySettings/UseUnitVariability"><Setting id="UseUnitVariability"><xsl:value-of select="UnitVariabilitySettings/UseUnitVariability"/></Setting></xsl:if>
          </Settings>
        </ModuleConfiguration>
        <!-- Add new DoseResponseModels ModuleConfiguration -->
        <ModuleConfiguration module="DoseResponseModels">
          <Settings>
            <xsl:if test="DoseResponseModelsSettings/CalculateParametricConfidenceInterval"><Setting id="CalculateParametricConfidenceInterval"><xsl:value-of select="DoseResponseModelsSettings/CalculateParametricConfidenceInterval"/></Setting></xsl:if>
          </Settings>
        </ModuleConfiguration>
        <!-- Add new FocalFoodConcentrations ModuleConfiguration -->
        <ModuleConfiguration module="FocalFoodConcentrations">
          <Settings>
            <xsl:if test="ConcentrationModelSettings/FocalCommodityReplacementMethod"><Setting id="FocalCommodityReplacementMethod"><xsl:value-of select="ConcentrationModelSettings/FocalCommodityReplacementMethod"/></Setting></xsl:if>
            <xsl:if test="FocalFoods">
              <Setting id="FocalFoods">
                <xsl:copy-of select="FocalFoods/FocalFood"/>
              </Setting>
            </xsl:if>
          </Settings>
        </ModuleConfiguration>
        <!-- Add new FoodConversions ModuleConfiguration -->
        <ModuleConfiguration module="FoodConversions">
          <Settings>
            <xsl:if test="AssessmentSettings/TotalDietStudy"><Setting id="TotalDietStudy"><xsl:value-of select="AssessmentSettings/TotalDietStudy"/></Setting></xsl:if>
            <xsl:if test="ConversionSettings/UseProcessing"><Setting id="UseProcessing"><xsl:value-of select="ConversionSettings/UseProcessing"/></Setting></xsl:if>
            <xsl:if test="ConversionSettings/UseComposition"><Setting id="UseComposition"><xsl:value-of select="ConversionSettings/UseComposition"/></Setting></xsl:if>
            <xsl:if test="ConversionSettings/UseReadAcrossFoodTranslations"><Setting id="UseReadAcrossFoodTranslations"><xsl:value-of select="ConversionSettings/UseReadAcrossFoodTranslations"/></Setting></xsl:if>
            <xsl:if test="ConversionSettings/UseMarketShares"><Setting id="UseMarketShares"><xsl:value-of select="ConversionSettings/UseMarketShares"/></Setting></xsl:if>
            <xsl:if test="ConversionSettings/UseSubTypes"><Setting id="UseSubTypes"><xsl:value-of select="ConversionSettings/UseSubTypes"/></Setting></xsl:if>
            <xsl:if test="ConversionSettings/UseSuperTypes"><Setting id="UseSuperTypes"><xsl:value-of select="ConversionSettings/UseSuperTypes"/></Setting></xsl:if>
            <xsl:if test="ConversionSettings/UseDefaultProcessingFactor"><Setting id="UseDefaultProcessingFactor"><xsl:value-of select="ConversionSettings/UseDefaultProcessingFactor"/></Setting></xsl:if>
            <xsl:if test="ConversionSettings/SubstanceIndependent"><Setting id="SubstanceIndependent"><xsl:value-of select="ConversionSettings/SubstanceIndependent"/></Setting></xsl:if>
          </Settings>
        </ModuleConfiguration>
        <!-- Add new HazardCharacterisations ModuleConfiguration -->
        <ModuleConfiguration module="HazardCharacterisations">
          <Settings>
            <xsl:if test="AssessmentSettings/Aggregate"><Setting id="Aggregate"><xsl:value-of select="AssessmentSettings/Aggregate"/></Setting></xsl:if>
            <xsl:if test="EffectSettings/TargetDoseLevelType"><Setting id="TargetDoseLevelType"><xsl:value-of select="EffectSettings/TargetDoseLevelType"/></Setting></xsl:if>
            <xsl:if test="EffectSettings/RestrictToCriticalEffect"><Setting id="RestrictToCriticalEffect"><xsl:value-of select="EffectSettings/RestrictToCriticalEffect"/></Setting></xsl:if>
            <xsl:if test="EffectSettings/TargetDosesCalculationMethod"><Setting id="TargetDosesCalculationMethod"><xsl:value-of select="EffectSettings/TargetDosesCalculationMethod"/></Setting></xsl:if>
            <xsl:if test="EffectSettings/PointOfDeparture"><Setting id="PointOfDeparture"><xsl:value-of select="EffectSettings/PointOfDeparture"/></Setting></xsl:if>
            <xsl:if test="EffectSettings/TargetDoseSelectionMethod"><Setting id="TargetDoseSelectionMethod"><xsl:value-of select="EffectSettings/TargetDoseSelectionMethod"/></Setting></xsl:if>
            <xsl:if test="EffectSettings/ImputeMissingHazardDoses"><Setting id="ImputeMissingHazardDoses"><xsl:value-of select="EffectSettings/ImputeMissingHazardDoses"/></Setting></xsl:if>
            <xsl:if test="EffectSettings/HazardDoseImputationMethod"><Setting id="HazardDoseImputationMethod"><xsl:value-of select="EffectSettings/HazardDoseImputationMethod"/></Setting></xsl:if>
            <xsl:if test="EffectSettings/UseDoseResponseModels"><Setting id="UseDoseResponseModels"><xsl:value-of select="EffectSettings/UseDoseResponseModels"/></Setting></xsl:if>
            <xsl:if test="EffectSettings/UseBMDL"><Setting id="UseBMDL"><xsl:value-of select="EffectSettings/UseBMDL"/></Setting></xsl:if>
            <xsl:if test="EffectSettings/UseIntraSpeciesConversionFactors"><Setting id="UseIntraSpeciesConversionFactors"><xsl:value-of select="EffectSettings/UseIntraSpeciesConversionFactors"/></Setting></xsl:if>
            <xsl:if test="EffectSettings/UseAdditionalAssessmentFactor"><Setting id="UseAdditionalAssessmentFactor"><xsl:value-of select="EffectSettings/UseAdditionalAssessmentFactor"/></Setting></xsl:if>
            <xsl:if test="EffectSettings/AdditionalAssessmentFactor"><Setting id="AdditionalAssessmentFactor"><xsl:value-of select="EffectSettings/AdditionalAssessmentFactor"/></Setting></xsl:if>
            <xsl:if test="EffectSettings/HazardCharacterisationsConvertToSingleTargetMatrix"><Setting id="HazardCharacterisationsConvertToSingleTargetMatrix"><xsl:value-of select="EffectSettings/HazardCharacterisationsConvertToSingleTargetMatrix"/></Setting></xsl:if>
            <xsl:if test="EffectSettings/TargetMatrix"><Setting id="TargetMatrix"><xsl:value-of select="EffectSettings/TargetMatrix"/></Setting></xsl:if>
            <xsl:if test="EffectSettings/HCSubgroupDependent"><Setting id="HCSubgroupDependent"><xsl:value-of select="EffectSettings/HCSubgroupDependent"/></Setting></xsl:if>
          </Settings>
        </ModuleConfiguration>
        <!-- Add new HighExposureFoodSubstanceCombinations ModuleConfiguration -->
        <ModuleConfiguration module="HighExposureFoodSubstanceCombinations">
          <Settings>
            <xsl:if test="ScreeningSettings/CriticalExposurePercentage"><Setting id="CriticalExposurePercentage"><xsl:value-of select="ScreeningSettings/CriticalExposurePercentage"/></Setting></xsl:if>
            <xsl:if test="ScreeningSettings/CumulativeSelectionPercentage"><Setting id="CumulativeSelectionPercentage"><xsl:value-of select="ScreeningSettings/CumulativeSelectionPercentage"/></Setting></xsl:if>
            <xsl:if test="ScreeningSettings/ImportanceLor"><Setting id="ImportanceLor"><xsl:value-of select="ScreeningSettings/ImportanceLor"/></Setting></xsl:if>
          </Settings>
        </ModuleConfiguration>
        <!-- Add new HumanMonitoringAnalysis ModuleConfiguration -->
        <ModuleConfiguration module="HumanMonitoringAnalysis">
          <Settings>
            <xsl:if test="HumanMonitoringSettings/NonDetectsHandlingMethod"><Setting id="HbmNonDetectsHandlingMethod"><xsl:value-of select="HumanMonitoringSettings/NonDetectsHandlingMethod"/></Setting></xsl:if>
            <xsl:if test="HumanMonitoringSettings/NonDetectImputationMethod"><Setting id="NonDetectImputationMethod"><xsl:value-of select="HumanMonitoringSettings/NonDetectImputationMethod"/></Setting></xsl:if>
            <xsl:if test="HumanMonitoringSettings/FractionOfLor"><Setting id="HbmFractionOfLor"><xsl:value-of select="HumanMonitoringSettings/FractionOfLor"/></Setting></xsl:if>
            <xsl:if test="HumanMonitoringSettings/MissingValueImputationMethod"><Setting id="MissingValueImputationMethod"><xsl:value-of select="HumanMonitoringSettings/MissingValueImputationMethod"/></Setting></xsl:if>
            <xsl:if test="HumanMonitoringSettings/MissingValueCutOff"><Setting id="MissingValueCutOff"><xsl:value-of select="HumanMonitoringSettings/MissingValueCutOff"/></Setting></xsl:if>
            <xsl:if test="HumanMonitoringSettings/ApplyKineticConversions"><Setting id="ApplyKineticConversions"><xsl:value-of select="HumanMonitoringSettings/ApplyKineticConversions"/></Setting></xsl:if>
            <xsl:if test="HumanMonitoringSettings/HbmConvertToSingleTargetMatrix"><Setting id="HbmConvertToSingleTargetMatrix"><xsl:value-of select="HumanMonitoringSettings/HbmConvertToSingleTargetMatrix"/></Setting></xsl:if>
            <xsl:if test="HumanMonitoringSettings/HbmTargetSurfaceLevel"><Setting id="HbmTargetSurfaceLevel"><xsl:value-of select="HumanMonitoringSettings/HbmTargetSurfaceLevel"/></Setting></xsl:if>
            <xsl:if test="HumanMonitoringSettings/HbmTargetMatrix"><Setting id="TargetMatrix"><xsl:value-of select="HumanMonitoringSettings/HbmTargetMatrix"/></Setting></xsl:if>
            <xsl:if test="HumanMonitoringSettings/SpecificGravityConversionFactor"><Setting id="SpecificGravityConversionFactor"><xsl:value-of select="HumanMonitoringSettings/SpecificGravityConversionFactor"/></Setting></xsl:if>
            <xsl:if test="HumanMonitoringSettings/StandardiseBlood"><Setting id="StandardiseBlood"><xsl:value-of select="HumanMonitoringSettings/StandardiseBlood"/></Setting></xsl:if>
            <xsl:if test="HumanMonitoringSettings/StandardiseBloodMethod"><Setting id="StandardiseBloodMethod"><xsl:value-of select="HumanMonitoringSettings/StandardiseBloodMethod"/></Setting></xsl:if>
            <xsl:if test="HumanMonitoringSettings/StandardiseBloodExcludeSubstances"><Setting id="StandardiseBloodExcludeSubstances"><xsl:value-of select="HumanMonitoringSettings/StandardiseBloodExcludeSubstances"/></Setting></xsl:if>
            <xsl:if test="HumanMonitoringSettings/StandardiseBloodExcludedSubstancesSubset">
              <Setting id="StandardiseBloodExcludedSubstancesSubset">
                <xsl:call-template name="listElements"><xsl:with-param name="items" select="HumanMonitoringSettings/StandardiseBloodExcludedSubstancesSubset/string"/></xsl:call-template>
              </Setting>
            </xsl:if>
            <xsl:if test="HumanMonitoringSettings/AnalyseMcr"><Setting id="McrAnalysis"><xsl:value-of select="HumanMonitoringSettings/AnalyseMcr"/></Setting></xsl:if>
            <xsl:if test="HumanMonitoringSettings/ExposureApproachType"><Setting id="McrExposureApproachType"><xsl:value-of select="HumanMonitoringSettings/ExposureApproachType"/></Setting></xsl:if>

            <xsl:if test="HumanMonitoringSettings/StandardiseUrine"><Setting id="StandardiseUrine"><xsl:value-of select="HumanMonitoringSettings/StandardiseUrine"/></Setting></xsl:if>
            <xsl:if test="HumanMonitoringSettings/StandardiseUrineMethod"><Setting id="StandardiseUrineMethod"><xsl:value-of select="HumanMonitoringSettings/StandardiseUrineMethod"/></Setting></xsl:if>
            <xsl:if test="HumanMonitoringSettings/StandardiseUrineExcludeSubstances"><Setting id="StandardiseUrineExcludeSubstances"><xsl:value-of select="HumanMonitoringSettings/StandardiseUrineExcludeSubstances"/></Setting></xsl:if>
            <xsl:if test="HumanMonitoringSettings/StandardiseUrineExcludedSubstancesSubset">
              <Setting id="StandardiseUrineExcludedSubstancesSubset">
                <xsl:call-template name="listElements"><xsl:with-param name="items" select="HumanMonitoringSettings/StandardiseUrineExcludedSubstancesSubset/string"/></xsl:call-template>
              </Setting>
            </xsl:if>

            <xsl:if test="HumanMonitoringSettings/ApplyExposureBiomarkerConversions"><Setting id="ApplyExposureBiomarkerConversions"><xsl:value-of select="HumanMonitoringSettings/ApplyExposureBiomarkerConversions"/></Setting></xsl:if>

            <xsl:if test="OutputDetailSettings/StoreIndividualDayIntakes"><Setting id="StoreIndividualDayIntakes"><xsl:value-of select="OutputDetailSettings/StoreIndividualDayIntakes"/></Setting></xsl:if>
          </Settings>
        </ModuleConfiguration>
        <!-- Add new HumanMonitoringData ModuleConfiguration -->
        <ModuleConfiguration module="HumanMonitoringData">
          <Settings>
            <xsl:if test="HumanMonitoringSettings/UseCompleteAnalysedSamples"><Setting id="UseCompleteAnalysedSamples"><xsl:value-of select="HumanMonitoringSettings/UseCompleteAnalysedSamples"/></Setting></xsl:if>
            <xsl:if test="HumanMonitoringSettings/FilterRepeatedMeasurements"><Setting id="FilterRepeatedMeasurements"><xsl:value-of select="HumanMonitoringSettings/FilterRepeatedMeasurements"/></Setting></xsl:if>
            <xsl:if test="HumanMonitoringSettings/ExcludeSubstancesFromSamplingMethod"><Setting id="ExcludeSubstancesFromSamplingMethod"><xsl:value-of select="HumanMonitoringSettings/ExcludeSubstancesFromSamplingMethod"/></Setting></xsl:if>
            <xsl:if test="HumanMonitoringSettings/SamplingMethodCodes">
              <Setting id="CodesHumanMonitoringSamplingMethods">
                <xsl:call-template name="listElements"><xsl:with-param name="items" select="HumanMonitoringSettings/SamplingMethodCodes/string"/></xsl:call-template>
              </Setting>
            </xsl:if>

            <xsl:if test="HumanMonitoringSettings/RepeatedMeasurementTimepointCodes">
              <Setting id="RepeatedMeasurementTimepointCodes">
                <xsl:call-template name="listElements"><xsl:with-param name="items" select="HumanMonitoringSettings/RepeatedMeasurementTimepointCodes/string"/></xsl:call-template>
              </Setting>
            </xsl:if>

            <xsl:if test="HumanMonitoringSettings/ExcludedSubstancesFromSamplingMethodSubset/*">
              <Setting id="ExcludedSubstancesFromSamplingMethodSubset">
                <xsl:copy-of select="HumanMonitoringSettings/ExcludedSubstancesFromSamplingMethodSubset/*" />
              </Setting>
            </xsl:if>

            <xsl:if test="SelectedHbmSurveySubsetProperties">
              <Setting id="SelectedHbmSurveySubsetProperties">
                <xsl:call-template name="listElements"><xsl:with-param name="items" select="SelectedHbmSurveySubsetProperties/string"/></xsl:call-template>
              </Setting>
            </xsl:if>

            <xsl:if test="SubsetSettings/MatchHbmIndividualSubsetWithPopulation"><Setting id="MatchHbmIndividualSubsetWithPopulation"><xsl:value-of select="SubsetSettings/MatchHbmIndividualSubsetWithPopulation"/></Setting></xsl:if>
            <xsl:if test="SubsetSettings/UseHbmSamplingWeights"><Setting id="UseHbmSamplingWeights"><xsl:value-of select="SubsetSettings/UseHbmSamplingWeights"/></Setting></xsl:if>

            <xsl:if test="UncertaintyAnalysisSettings/ResampleHBMIndividuals"><Setting id="ResampleHbmIndividuals"><xsl:value-of select="UncertaintyAnalysisSettings/ResampleHBMIndividuals"/></Setting></xsl:if>

          </Settings>
        </ModuleConfiguration>
        <!-- Add new InterSpeciesConversions ModuleConfiguration -->
        <ModuleConfiguration module="InterSpeciesConversions">
          <Settings>
            <xsl:if test="EffectSettings/UseInterSpeciesConversionFactors"><Setting id="UseInterSpeciesConversionFactors"><xsl:value-of select="EffectSettings/UseInterSpeciesConversionFactors"/></Setting></xsl:if>
            <xsl:if test="RisksSettings/DefaultInterSpeciesFactorGeometricMean"><Setting id="DefaultInterSpeciesFactorGeometricMean"><xsl:value-of select="RisksSettings/DefaultInterSpeciesFactorGeometricMean"/></Setting></xsl:if>
            <xsl:if test="RisksSettings/DefaultInterSpeciesFactorGeometricStandardDeviation"><Setting id="DefaultInterSpeciesFactorGeometricStandardDeviation"><xsl:value-of select="RisksSettings/DefaultInterSpeciesFactorGeometricStandardDeviation"/></Setting></xsl:if>
            <xsl:if test="UncertaintyAnalysisSettings/ReSampleInterspecies"><Setting id="ResampleInterspecies"><xsl:value-of select="UncertaintyAnalysisSettings/ReSampleInterspecies"/></Setting></xsl:if>
          </Settings>
        </ModuleConfiguration>
        <!-- Add new IntraSpeciesFactors ModuleConfiguration -->
        <ModuleConfiguration module="IntraSpeciesFactors">
          <Settings>
            <xsl:if test="RisksSettings/DefaultIntraSpeciesFactor"><Setting id="DefaultIntraSpeciesFactor"><xsl:value-of select="RisksSettings/DefaultIntraSpeciesFactor"/></Setting></xsl:if>
            <xsl:if test="UncertaintyAnalysisSettings/ReSampleIntraSpecies"><Setting id="ResampleIntraSpecies"><xsl:value-of select="UncertaintyAnalysisSettings/ReSampleIntraSpecies"/></Setting></xsl:if>
          </Settings>
        </ModuleConfiguration>
        <!-- Add new KineticModels ModuleConfiguration -->
        <ModuleConfiguration module="KineticModels">
          <Settings>
            <xsl:if test="KineticModelSettings/CodeModel"><Setting id="CodeKineticModel"><xsl:value-of select="KineticModelSettings/CodeModel"/></Setting></xsl:if>
            <xsl:if test="KineticModelSettings/NumberOfDays"><Setting id="NumberOfDays"><xsl:value-of select="KineticModelSettings/NumberOfDays"/></Setting></xsl:if>
            <xsl:if test="KineticModelSettings/NumberOfDosesPerDay"><Setting id="NumberOfDosesPerDay"><xsl:value-of select="KineticModelSettings/NumberOfDosesPerDay"/></Setting></xsl:if>
            <xsl:if test="KineticModelSettings/NumberOfDosesPerDayNonDietaryOral"><Setting id="NumberOfDosesPerDayNonDietaryOral"><xsl:value-of select="KineticModelSettings/NumberOfDosesPerDayNonDietaryOral"/></Setting></xsl:if>
            <xsl:if test="KineticModelSettings/NumberOfDosesPerDayNonDietaryDermal"><Setting id="NumberOfDosesPerDayNonDietaryDermal"><xsl:value-of select="KineticModelSettings/NumberOfDosesPerDayNonDietaryDermal"/></Setting></xsl:if>
            <xsl:if test="KineticModelSettings/NumberOfDosesPerDayNonDietaryInhalation"><Setting id="NumberOfDosesPerDayNonDietaryInhalation"><xsl:value-of select="KineticModelSettings/NumberOfDosesPerDayNonDietaryInhalation"/></Setting></xsl:if>
            <xsl:if test="KineticModelSettings/NonStationaryPeriod"><Setting id="NonStationaryPeriod"><xsl:value-of select="KineticModelSettings/NonStationaryPeriod"/></Setting></xsl:if>
            <xsl:if test="KineticModelSettings/UseParameterVariability"><Setting id="UseParameterVariability"><xsl:value-of select="KineticModelSettings/UseParameterVariability"/></Setting></xsl:if>
            <xsl:if test="KineticModelSettings/InternalModelType"><Setting id="InternalModelType"><xsl:value-of select="KineticModelSettings/InternalModelType"/></Setting></xsl:if>
            <xsl:if test="KineticModelSettings/KCFSubgroupDependent"><Setting id="KCFSubgroupDependent"><xsl:value-of select="KineticModelSettings/KCFSubgroupDependent"/></Setting></xsl:if>
            <xsl:if test="KineticModelSettings/SpecifyEvents"><Setting id="SpecifyEvents"><xsl:value-of select="KineticModelSettings/SpecifyEvents"/></Setting></xsl:if>
            <xsl:if test="KineticModelSettings/SelectedEvents">
              <Setting id="SelectedEvents">
                <xsl:call-template name="listElements"><xsl:with-param name="items" select="KineticModelSettings/SelectedEvents/int"/></xsl:call-template>
              </Setting>
            </xsl:if>
            <xsl:if test="NonDietarySettings/OralAbsorptionFactorForDietaryExposure"><Setting id="OralAbsorptionFactorForDietaryExposure"><xsl:value-of select="NonDietarySettings/OralAbsorptionFactorForDietaryExposure"/></Setting></xsl:if>
            <xsl:if test="NonDietarySettings/OralAbsorptionFactor"><Setting id="OralAbsorptionFactor"><xsl:value-of select="NonDietarySettings/OralAbsorptionFactor"/></Setting></xsl:if>
            <xsl:if test="NonDietarySettings/DermalAbsorptionFactor"><Setting id="DermalAbsorptionFactor"><xsl:value-of select="NonDietarySettings/DermalAbsorptionFactor"/></Setting></xsl:if>
            <xsl:if test="NonDietarySettings/InhalationAbsorptionFactor"><Setting id="InhalationAbsorptionFactor"><xsl:value-of select="NonDietarySettings/InhalationAbsorptionFactor"/></Setting></xsl:if>
            <xsl:if test="UncertaintyAnalysisSettings/ResampleKineticModelParameters"><Setting id="ResampleKineticModelParameters"><xsl:value-of select="UncertaintyAnalysisSettings/ResampleKineticModelParameters"/></Setting></xsl:if>
          </Settings>
        </ModuleConfiguration>
        <!-- Add new ExposureMixtures ModuleConfiguration -->
        <ModuleConfiguration module="ExposureMixtures">
          <Settings>
            <xsl:if test="MixtureSelectionSettings/ExposureApproachType"><Setting id="ExposureApproachType"><xsl:value-of select="MixtureSelectionSettings/ExposureApproachType"/></Setting></xsl:if>
            <xsl:if test="MixtureSelectionSettings/K"><Setting id="NumberOfMixtures"><xsl:value-of select="MixtureSelectionSettings/K"/></Setting></xsl:if>
            <xsl:if test="MixtureSelectionSettings/SW"><Setting id="MixtureSelectionSparsenessConstraint"><xsl:value-of select="MixtureSelectionSettings/SW"/></Setting></xsl:if>
            <xsl:if test="MixtureSelectionSettings/NumberOfIterations"><Setting id="MixtureSelectionIterations"><xsl:value-of select="MixtureSelectionSettings/NumberOfIterations"/></Setting></xsl:if>
            <xsl:if test="MixtureSelectionSettings/RandomSeed"><Setting id="MixtureSelectionRandomSeed"><xsl:value-of select="MixtureSelectionSettings/RandomSeed"/></Setting></xsl:if>
            <xsl:if test="MixtureSelectionSettings/Epsilon"><Setting id="MixtureSelectionConvergenceCriterium"><xsl:value-of select="MixtureSelectionSettings/Epsilon"/></Setting></xsl:if>
            <xsl:if test="MixtureSelectionSettings/AutomaticallyDeterminationOfClusters"><Setting id="AutomaticallyDeterminationOfClusters"><xsl:value-of select="MixtureSelectionSettings/AutomaticallyDeterminationOfClusters"/></Setting></xsl:if>
            <xsl:if test="MixtureSelectionSettings/NumberOfClusters"><Setting id="NumberOfClusters"><xsl:value-of select="MixtureSelectionSettings/NumberOfClusters"/></Setting></xsl:if>
            <xsl:if test="MixtureSelectionSettings/ClusterMethodType"><Setting id="ClusterMethodType"><xsl:value-of select="MixtureSelectionSettings/ClusterMethodType"/></Setting></xsl:if>
            <xsl:if test="MixtureSelectionSettings/NetworkAnalysisType"><Setting id="NetworkAnalysisType"><xsl:value-of select="MixtureSelectionSettings/NetworkAnalysisType"/></Setting></xsl:if>
            <xsl:if test="MixtureSelectionSettings/IsLogTransform"><Setting id="IsLogTransform"><xsl:value-of select="MixtureSelectionSettings/IsLogTransform"/></Setting></xsl:if>
            <xsl:if test="AssessmentSettings/ExposureCalculationMethod"><Setting id="ExposureCalculationMethod"><xsl:value-of select="AssessmentSettings/ExposureCalculationMethod"/></Setting></xsl:if>
          </Settings>
        </ModuleConfiguration>
        <!-- Add new ModelledFoods ModuleConfiguration -->
        <ModuleConfiguration module="ModelledFoods">
          <Settings>
            <xsl:if test="ConversionSettings/UseWorstCaseValues"><Setting id="UseWorstCaseValues"><xsl:value-of select="ConversionSettings/UseWorstCaseValues"/></Setting></xsl:if>
            <xsl:if test="ConversionSettings/DeriveModelledFoodsFromSampleBasedConcentrations"><Setting id="DeriveModelledFoodsFromSampleBasedConcentrations"><xsl:value-of select="ConversionSettings/DeriveModelledFoodsFromSampleBasedConcentrations"/></Setting></xsl:if>
            <xsl:if test="ConversionSettings/DeriveModelledFoodsFromSingleValueConcentrations"><Setting id="DeriveModelledFoodsFromSingleValueConcentrations"><xsl:value-of select="ConversionSettings/DeriveModelledFoodsFromSingleValueConcentrations"/></Setting></xsl:if>
            <xsl:if test="ConversionSettings/FoodIncludeNonDetects"><Setting id="FoodIncludeNonDetects"><xsl:value-of select="ConversionSettings/FoodIncludeNonDetects"/></Setting></xsl:if>
            <xsl:if test="ConversionSettings/CompoundIncludeNonDetects"><Setting id="SubstanceIncludeNonDetects"><xsl:value-of select="ConversionSettings/CompoundIncludeNonDetects"/></Setting></xsl:if>
            <xsl:if test="ConversionSettings/CompoundIncludeNoMeasurements"><Setting id="SubstanceIncludeNoMeasurements"><xsl:value-of select="ConversionSettings/CompoundIncludeNoMeasurements"/></Setting></xsl:if>
            <xsl:if test="ModelledFoodSubset">
              <Setting id="ModelledFoodSubset">
                <xsl:call-template name="listElements"><xsl:with-param name="items" select="ModelledFoodSubset/FoodCode"/></xsl:call-template>
              </Setting>
            </xsl:if>
          </Settings>
        </ModuleConfiguration>
        <!-- Add new NonDietaryExposures ModuleConfiguration -->
        <ModuleConfiguration module="NonDietaryExposures">
          <Settings>
            <xsl:if test="NonDietarySettings/MatchSpecificIndividuals"><Setting id="MatchSpecificIndividuals"><xsl:value-of select="NonDietarySettings/MatchSpecificIndividuals"/></Setting></xsl:if>
            <xsl:if test="UncertaintyAnalysisSettings/ReSampleNonDietaryExposures"><Setting id="ResampleNonDietaryExposures"><xsl:value-of select="UncertaintyAnalysisSettings/ReSampleNonDietaryExposures"/></Setting></xsl:if>
          </Settings>
        </ModuleConfiguration>
        <!-- Add new OccurrencePatterns ModuleConfiguration -->
        <ModuleConfiguration module="OccurrencePatterns">
          <Settings>
            <xsl:if test="AgriculturalUseSettings/SetMissingAgriculturalUseAsUnauthorized"><Setting id="SetMissingAgriculturalUseAsUnauthorized"><xsl:value-of select="AgriculturalUseSettings/SetMissingAgriculturalUseAsUnauthorized"/></Setting></xsl:if>
            <xsl:if test="AgriculturalUseSettings/UseAgriculturalUsePercentage"><Setting id="UseAgriculturalUsePercentage"><xsl:value-of select="AgriculturalUseSettings/UseAgriculturalUsePercentage"/></Setting></xsl:if>
            <xsl:if test="AgriculturalUseSettings/ScaleUpOccurencePatterns"><Setting id="ScaleUpOccurencePatterns"><xsl:value-of select="AgriculturalUseSettings/ScaleUpOccurencePatterns"/></Setting></xsl:if>
            <xsl:if test="AgriculturalUseSettings/RestrictOccurencePatternScalingToAuthorisedUses"><Setting id="RestrictOccurencePatternScalingToAuthorisedUses"><xsl:value-of select="AgriculturalUseSettings/RestrictOccurencePatternScalingToAuthorisedUses"/></Setting></xsl:if>
            <xsl:if test="UncertaintyAnalysisSettings/RecomputeOccurrencePatterns"><Setting id="RecomputeOccurrencePatterns"><xsl:value-of select="UncertaintyAnalysisSettings/RecomputeOccurrencePatterns"/></Setting></xsl:if>
          </Settings>
        </ModuleConfiguration>
        <!-- Add new Populations ModuleConfiguration -->
        <ModuleConfiguration module="Populations">
          <Settings>
            <xsl:if test="SubsetSettings/PopulationSubsetSelection"><Setting id="PopulationSubsetSelection"><xsl:value-of select="SubsetSettings/PopulationSubsetSelection"/></Setting></xsl:if>
            <xsl:if test="PopulationSettings/NominalPopulationBodyWeight"><Setting id="NominalPopulationBodyWeight"><xsl:value-of select="PopulationSettings/NominalPopulationBodyWeight"/></Setting></xsl:if>
            <xsl:if test="IndividualsSubsetDefinitions/*">
              <Setting id="IndividualsSubsetDefinitions">
                <xsl:copy-of select="IndividualsSubsetDefinitions/*" />
              </Setting>
            </xsl:if>
            <xsl:if test="IndividualDaySubsetDefinition">
              <Setting id="IndividualDaySubsetDefinition">
                <xsl:copy-of select="IndividualDaySubsetDefinition" />
              </Setting>
            </xsl:if>
          </Settings>
        </ModuleConfiguration>
        <!-- Add new ProcessingFactors ModuleConfiguration -->
        <ModuleConfiguration module="ProcessingFactors">
          <Settings>
            <xsl:if test="UncertaintyAnalysisSettings/ReSampleProcessingFactors"><Setting id="ResampleProcessingFactors"><xsl:value-of select="UncertaintyAnalysisSettings/ReSampleProcessingFactors"/></Setting></xsl:if>
            <xsl:if test="ConcentrationModelSettings/IsProcessing"><Setting id="IsProcessing"><xsl:value-of select="ConcentrationModelSettings/IsProcessing"/></Setting></xsl:if>
            <xsl:if test="ConcentrationModelSettings/IsDistribution"><Setting id="IsDistribution"><xsl:value-of select="ConcentrationModelSettings/IsDistribution"/></Setting></xsl:if>
            <xsl:if test="ConcentrationModelSettings/AllowHigherThanOne"><Setting id="AllowHigherThanOne"><xsl:value-of select="ConcentrationModelSettings/AllowHigherThanOne"/></Setting></xsl:if>
          </Settings>
        </ModuleConfiguration>
        <!-- Add new RelativePotencyFactors ModuleConfiguration -->
        <ModuleConfiguration module="RelativePotencyFactors">
          <Settings>
            <xsl:if test="UncertaintyAnalysisSettings/ReSampleRPFs"><Setting id="ResampleRPFs"><xsl:value-of select="UncertaintyAnalysisSettings/ReSampleRPFs"/></Setting></xsl:if>
          </Settings>
        </ModuleConfiguration>
        <!-- Add new Risks ModuleConfiguration -->
        <ModuleConfiguration module="Risks">
          <Settings>
            <xsl:if test="AssessmentSettings/ExposureCalculationMethod"><Setting id="ExposureCalculationMethod"><xsl:value-of select="AssessmentSettings/ExposureCalculationMethod"/></Setting></xsl:if>
            <xsl:if test="RisksSettings/CumulativeRisk"><Setting id="CumulativeRisk"><xsl:value-of select="RisksSettings/CumulativeRisk"/></Setting></xsl:if>
            <xsl:if test="RisksSettings/HealthEffectType"><Setting id="HealthEffectType"><xsl:value-of select="RisksSettings/HealthEffectType"/></Setting></xsl:if>
            <xsl:if test="RisksSettings/RiskMetricType"><Setting id="RiskMetricType"><xsl:value-of select="RisksSettings/RiskMetricType"/></Setting></xsl:if>
            <xsl:if test="RisksSettings/IsEAD"><Setting id="IsEAD"><xsl:value-of select="RisksSettings/IsEAD"/></Setting></xsl:if>
            <xsl:if test="RisksSettings/ThresholdMarginOfExposure"><Setting id="ThresholdMarginOfExposure"><xsl:value-of select="RisksSettings/ThresholdMarginOfExposure"/></Setting></xsl:if>
            <xsl:if test="RisksSettings/IsInverseDistribution"><Setting id="IsInverseDistribution"><xsl:value-of select="RisksSettings/IsInverseDistribution"/></Setting></xsl:if>
            <xsl:if test="RisksSettings/CalculateRisksByFood"><Setting id="CalculateRisksByFood"><xsl:value-of select="RisksSettings/CalculateRisksByFood"/></Setting></xsl:if>
            <xsl:if test="RisksSettings/RiskMetricCalculationType"><Setting id="RiskMetricCalculationType"><xsl:value-of select="RisksSettings/RiskMetricCalculationType"/></Setting></xsl:if>
            <xsl:if test="RisksSettings/LeftMargin"><Setting id="LeftMargin"><xsl:value-of select="RisksSettings/LeftMargin"/></Setting></xsl:if>
            <xsl:if test="RisksSettings/RightMargin"><Setting id="RightMargin"><xsl:value-of select="RisksSettings/RightMargin"/></Setting></xsl:if>
            <xsl:if test="RisksSettings/NumberOfLabels"><Setting id="NumberOfLabels"><xsl:value-of select="RisksSettings/NumberOfLabels"/></Setting></xsl:if>
            <xsl:if test="RisksSettings/NumberOfSubstances"><Setting id="NumberOfSubstances"><xsl:value-of select="RisksSettings/NumberOfSubstances"/></Setting></xsl:if>
            <xsl:if test="RisksSettings/ConfidenceInterval"><Setting id="ConfidenceInterval"><xsl:value-of select="RisksSettings/ConfidenceInterval"/></Setting></xsl:if>
            <xsl:if test="RisksSettings/AnalyseMcr"><Setting id="McrAnalysis"><xsl:value-of select="RisksSettings/AnalyseMcr"/></Setting></xsl:if>
            <xsl:if test="RisksSettings/ExposureApproachType"><Setting id="McrExposureApproachType"><xsl:value-of select="RisksSettings/ExposureApproachType"/></Setting></xsl:if>
        </Settings>
        </ModuleConfiguration>
        <!-- Add new SingleValueConcentrations ModuleConfiguration -->
        <ModuleConfiguration module="SingleValueConcentrations">
          <Settings>
            <xsl:if test="ConcentrationModelSettings/UseDeterministicConversionFactors"><Setting id="UseDeterministicConversionFactors"><xsl:value-of select="ConcentrationModelSettings/UseDeterministicConversionFactors"/></Setting></xsl:if>
          </Settings>
        </ModuleConfiguration>
        <!-- Add new SingleValueConsumptions ModuleConfiguration -->
        <ModuleConfiguration module="SingleValueConsumptions">
          <Settings>
            <xsl:if test="SubsetSettings/UseBodyWeightStandardisedConsumptionDistribution"><Setting id="UseBodyWeightStandardisedConsumptionDistribution"><xsl:value-of select="SubsetSettings/UseBodyWeightStandardisedConsumptionDistribution"/></Setting></xsl:if>
          </Settings>
        </ModuleConfiguration>
        <!-- Add new SingleValueDietaryExposures ModuleConfiguration -->
        <ModuleConfiguration module="SingleValueDietaryExposures">
          <Settings>
            <xsl:if test="AgriculturalUseSettings/UseOccurrenceFrequencies"><Setting id="UseOccurrenceFrequencies"><xsl:value-of select="AgriculturalUseSettings/UseOccurrenceFrequencies"/></Setting></xsl:if>
            <xsl:if test="DietaryIntakeCalculationSettings/SingleValueDietaryExposureCalculationMethod"><Setting id="SingleValueDietaryExposureCalculationMethod"><xsl:value-of select="DietaryIntakeCalculationSettings/SingleValueDietaryExposureCalculationMethod"/></Setting></xsl:if>
          </Settings>
        </ModuleConfiguration>
        <!-- Add new SingleValueNonDietaryExposures ModuleConfiguration -->
        <ModuleConfiguration module="SingleValueNonDietaryExposures">
          <Settings>
            <xsl:if test="NonDietarySettings/CodeConfiguration"><Setting id="CodeConfiguration"><xsl:value-of select="NonDietarySettings/CodeConfiguration"/></Setting></xsl:if>
          </Settings>
        </ModuleConfiguration>
        <!-- Add new SingleValueRisks ModuleConfiguration -->
        <ModuleConfiguration module="SingleValueRisks">
          <Settings>
            <xsl:if test="RisksSettings/SingleValueRiskCalculationMethod"><Setting id="SingleValueRiskCalculationMethod"><xsl:value-of select="RisksSettings/SingleValueRiskCalculationMethod"/></Setting></xsl:if>
            <xsl:if test="RisksSettings/Percentage"><Setting id="Percentage"><xsl:value-of select="RisksSettings/Percentage"/></Setting></xsl:if>
            <xsl:if test="RisksSettings/UseAdjustmentFactors"><Setting id="UseAdjustmentFactors"><xsl:value-of select="RisksSettings/UseAdjustmentFactors"/></Setting></xsl:if>
            <xsl:if test="RisksSettings/ExposureAdjustmentFactorDistributionMethod"><Setting id="ExposureAdjustmentFactorDistributionMethod"><xsl:value-of select="RisksSettings/ExposureAdjustmentFactorDistributionMethod"/></Setting></xsl:if>
            <xsl:if test="RisksSettings/HazardAdjustmentFactorDistributionMethod"><Setting id="HazardAdjustmentFactorDistributionMethod"><xsl:value-of select="RisksSettings/HazardAdjustmentFactorDistributionMethod"/></Setting></xsl:if>
            <xsl:if test="RisksSettings/ExposureParameterA"><Setting id="ExposureParameterA"><xsl:value-of select="RisksSettings/ExposureParameterA"/></Setting></xsl:if>
            <xsl:if test="RisksSettings/ExposureParameterB"><Setting id="ExposureParameterB"><xsl:value-of select="RisksSettings/ExposureParameterB"/></Setting></xsl:if>
            <xsl:if test="RisksSettings/ExposureParameterC"><Setting id="ExposureParameterC"><xsl:value-of select="RisksSettings/ExposureParameterC"/></Setting></xsl:if>
            <xsl:if test="RisksSettings/ExposureParameterD"><Setting id="ExposureParameterD"><xsl:value-of select="RisksSettings/ExposureParameterD"/></Setting></xsl:if>
            <xsl:if test="RisksSettings/HazardParameterA"><Setting id="HazardParameterA"><xsl:value-of select="RisksSettings/HazardParameterA"/></Setting></xsl:if>
            <xsl:if test="RisksSettings/HazardParameterB"><Setting id="HazardParameterB"><xsl:value-of select="RisksSettings/HazardParameterB"/></Setting></xsl:if>
            <xsl:if test="RisksSettings/HazardParameterC"><Setting id="HazardParameterC"><xsl:value-of select="RisksSettings/HazardParameterC"/></Setting></xsl:if>
            <xsl:if test="RisksSettings/HazardParameterD"><Setting id="HazardParameterD"><xsl:value-of select="RisksSettings/HazardParameterD"/></Setting></xsl:if>
            <xsl:if test="RisksSettings/UseBackgroundAdjustmentFactor"><Setting id="UseBackgroundAdjustmentFactor"><xsl:value-of select="RisksSettings/UseBackgroundAdjustmentFactor"/></Setting></xsl:if>
          </Settings>
        </ModuleConfiguration>
        <!-- Add new TargetExposures ModuleConfiguration -->
        <ModuleConfiguration module="TargetExposures">
          <Settings>
            <xsl:if test="NonDietarySettings/IsCorrelationBetweenIndividuals"><Setting id="IsCorrelationBetweenIndividuals"><xsl:value-of select="NonDietarySettings/IsCorrelationBetweenIndividuals"/></Setting></xsl:if>
            <xsl:if test="OutputDetailSettings/StoreIndividualDayIntakes"><Setting id="StoreIndividualDayIntakes"><xsl:value-of select="OutputDetailSettings/StoreIndividualDayIntakes"/></Setting></xsl:if>
            <xsl:if test="EffectSettings/AnalyseMcr"><Setting id="McrAnalysis"><xsl:value-of select="EffectSettings/AnalyseMcr"/></Setting></xsl:if>
            <xsl:if test="EffectSettings/ExposureApproachType"><Setting id="McrExposureApproachType"><xsl:value-of select="EffectSettings/ExposureApproachType"/></Setting></xsl:if>
            <xsl:if test="KineticModelSettings/CodeCompartment"><Setting id="CodeCompartment"><xsl:value-of select="KineticModelSettings/CodeCompartment"/></Setting></xsl:if>
          </Settings>
        </ModuleConfiguration>
        <!-- Add new UnitVariabilityFactors ModuleConfiguration -->
        <ModuleConfiguration module="UnitVariabilityFactors">
          <Settings>
            <xsl:if test="UnitVariabilitySettings/CorrelationType"><Setting id="CorrelationType"><xsl:value-of select="UnitVariabilitySettings/CorrelationType"/></Setting></xsl:if>
            <xsl:if test="UnitVariabilitySettings/UnitVariabilityModel"><Setting id="UnitVariabilityModel"><xsl:value-of select="UnitVariabilitySettings/UnitVariabilityModel"/></Setting></xsl:if>
            <xsl:if test="UnitVariabilitySettings/EstimatesNature"><Setting id="EstimatesNature"><xsl:value-of select="UnitVariabilitySettings/EstimatesNature"/></Setting></xsl:if>
            <xsl:if test="UnitVariabilitySettings/UnitVariabilityType"><Setting id="UnitVariabilityType"><xsl:value-of select="UnitVariabilitySettings/UnitVariabilityType"/></Setting></xsl:if>
            <xsl:if test="UnitVariabilitySettings/MeanValueCorrectionType"><Setting id="MeanValueCorrectionType"><xsl:value-of select="UnitVariabilitySettings/MeanValueCorrectionType"/></Setting></xsl:if>
            <xsl:if test="UnitVariabilitySettings/DefaultFactorLow"><Setting id="DefaultFactorLow"><xsl:value-of select="UnitVariabilitySettings/DefaultFactorLow"/></Setting></xsl:if>
            <xsl:if test="UnitVariabilitySettings/DefaultFactorMid"><Setting id="DefaultFactorMid"><xsl:value-of select="UnitVariabilitySettings/DefaultFactorMid"/></Setting></xsl:if>
          </Settings>
        </ModuleConfiguration>
      </ModuleConfigurations>
      </xsl:if>
    </xsl:copy>
  </xsl:template>

  <!-- create generic value list from elements -->
  <xsl:template name="listElements">
    <xsl:param name="items" />
    <xsl:for-each select="$items">
      <Value><xsl:value-of select="."/></Value>
    </xsl:for-each>
  </xsl:template>
  <!-- rename list element -->
  <xsl:template name="renameElements">
    <xsl:param name="newname" />
    <xsl:param name="items" />
    <xsl:for-each select="$items">
      <xsl:element name="{$newname}">
        <xsl:value-of select="."/>
      </xsl:element>
    </xsl:for-each>
  </xsl:template>
</xsl:stylesheet>
