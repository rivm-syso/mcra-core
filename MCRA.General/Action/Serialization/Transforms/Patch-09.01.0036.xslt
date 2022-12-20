<?xml version="1.0" encoding="utf-8"?>
<!--
Stylesheet for transforms from 9.1.23 to version 9.1.36 of MCRA
Issue: Rename boolean properties of settings classes which have redundant "Is" prefix.
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

  <!-- AgriculturalUseSettings: Rename IsRestrictOccurencePatternScalingToAuthorisedUses to RestrictOccurencePatternScalingToAuthorisedUses -->
  <xsl:template match="IsRestrictOccurencePatternScalingToAuthorisedUses">
    <xsl:element name="RestrictOccurencePatternScalingToAuthorisedUses">
      <xsl:apply-templates select="@*|node()"/>
    </xsl:element>
  </xsl:template>

  <!-- AgriculturalUseSettings: Rename IsScaleUpOccurencePatterns to ScaleUpOccurencePatterns -->
  <xsl:template match="IsScaleUpOccurencePatterns">
    <xsl:element name="ScaleUpOccurencePatterns">
      <xsl:apply-templates select="@*|node()"/>
    </xsl:element>
  </xsl:template>

  <!-- AgriculturalUseSettings: Rename IsUseAgriculturalUsePercentage to UseAgriculturalUsePercentage -->
  <xsl:template match="IsUseAgriculturalUsePercentage">
    <xsl:element name="UseAgriculturalUsePercentage">
      <xsl:apply-templates select="@*|node()"/>
    </xsl:element>
  </xsl:template>

  <!-- AgriculturalUseSettings: Rename IsUseAgriculturalUseTable to UseAgriculturalUseTable -->
  <xsl:template match="IsUseAgriculturalUseTable">
    <xsl:element name="UseAgriculturalUseTable">
      <xsl:apply-templates select="@*|node()"/>
    </xsl:element>
  </xsl:template>

  <!-- AgriculturalUseSettings: Rename IsUseOccurrenceFrequencies to UseOccurrenceFrequencies -->
  <xsl:template match="IsUseOccurrenceFrequencies">
    <xsl:element name="UseOccurrenceFrequencies">
      <xsl:apply-templates select="@*|node()"/>
    </xsl:element>
  </xsl:template>

  <!-- AssessmentSettings: Rename IsMultipleSubstances to MultipleSubstances -->
  <xsl:template match="IsMultipleSubstances">
    <xsl:element name="MultipleSubstances">
      <xsl:apply-templates select="@*|node()"/>
    </xsl:element>
  </xsl:template>

  <!-- ConcentrationModelSettings: Rename IsConsiderAuthorisationsForExtrapolations to ConsiderAuthorisationsForExtrapolations -->
  <xsl:template match="IsConsiderAuthorisationsForExtrapolations">
    <xsl:element name="ConsiderAuthorisationsForExtrapolations">
      <xsl:apply-templates select="@*|node()"/>
    </xsl:element>
  </xsl:template>

  <!-- ConcentrationModelSettings: Rename IsConsiderAuthorisationsForSubstanceConversion to ConsiderAuthorisationsForSubstanceConversion -->
  <xsl:template match="IsConsiderAuthorisationsForSubstanceConversion">
    <xsl:element name="ConsiderAuthorisationsForSubstanceConversion">
      <xsl:apply-templates select="@*|node()"/>
    </xsl:element>
  </xsl:template>

  <!-- ConcentrationModelSettings: Rename IsConsiderMrlForExtrapolations to ConsiderMrlForExtrapolations -->
  <xsl:template match="IsConsiderMrlForExtrapolations">
    <xsl:element name="ConsiderMrlForExtrapolations">
      <xsl:apply-templates select="@*|node()"/>
    </xsl:element>
  </xsl:template>

  <!-- ConcentrationModelSettings: Rename IsCorrelateImputedValueWithSamplePotency to CorrelateImputedValueWithSamplePotency -->
  <xsl:template match="IsCorrelateImputedValueWithSamplePotency">
    <xsl:element name="CorrelateImputedValueWithSamplePotency">
      <xsl:apply-templates select="@*|node()"/>
    </xsl:element>
  </xsl:template>

  <!-- ConcentrationModelSettings: Rename IsExtrapolateConcentrations to ExtrapolateConcentrations -->
  <xsl:template match="IsExtrapolateConcentrations">
    <xsl:element name="ExtrapolateConcentrations">
      <xsl:apply-templates select="@*|node()"/>
    </xsl:element>
  </xsl:template>

  <!-- ConcentrationModelSettings: Rename IsImputeWaterConcentrations to ImputeWaterConcentrations -->
  <xsl:template match="IsImputeWaterConcentrations">
    <xsl:element name="ImputeWaterConcentrations">
      <xsl:apply-templates select="@*|node()"/>
    </xsl:element>
  </xsl:template>

  <!-- ConcentrationModelSettings: Rename IsMissingValueImputation to MissingValueImputation -->
  <xsl:template match="IsMissingValueImputation">
    <xsl:element name="ImputeMissingValues">
      <xsl:apply-templates select="@*|node()"/>
    </xsl:element>
  </xsl:template>

  <!-- ConcentrationModelSettings: Rename IsRestrictLorImputationToAuthorisedUses to RestrictLorImputationToAuthorisedUses -->
  <xsl:template match="IsRestrictLorImputationToAuthorisedUses">
    <xsl:element name="RestrictLorImputationToAuthorisedUses">
      <xsl:apply-templates select="@*|node()"/>
    </xsl:element>
  </xsl:template>

  <!-- ConcentrationModelSettings: Rename IsRestrictWaterImputationToAuthorisedUses to RestrictWaterImputationToAuthorisedUses -->
  <xsl:template match="IsRestrictWaterImputationToAuthorisedUses">
    <xsl:element name="RestrictWaterImputationToAuthorisedUses">
      <xsl:apply-templates select="@*|node()"/>
    </xsl:element>
  </xsl:template>

  <!-- ConcentrationModelSettings: Rename IsRestrictWaterImputationToMostPotentSubstances to RestrictWaterImputationToMostPotentSubstances -->
  <xsl:template match="IsRestrictWaterImputationToMostPotentSubstances">
    <xsl:element name="RestrictWaterImputationToMostPotentSubstances">
      <xsl:apply-templates select="@*|node()"/>
    </xsl:element>
  </xsl:template>

  <!-- ConcentrationModelSettings: Rename IsRetainAllAllocatedSubstancesAfterAllocation to RetainAllAllocatedSubstancesAfterAllocation -->
  <xsl:template match="IsRetainAllAllocatedSubstancesAfterAllocation">
    <xsl:element name="RetainAllAllocatedSubstancesAfterAllocation">
      <xsl:apply-templates select="@*|node()"/>
    </xsl:element>
  </xsl:template>

  <!-- ConcentrationModelSettings: Rename IsUseComplexResidueDefinitions to UseComplexResidueDefinitions -->
  <xsl:template match="IsUseComplexResidueDefinitions">
    <xsl:element name="UseComplexResidueDefinitions">
      <xsl:apply-templates select="@*|node()"/>
    </xsl:element>
  </xsl:template>

  <!-- ConcentrationModelSettings: Rename IsUseDeterministicConversionFactors to UseDeterministicConversionFactors -->
  <xsl:template match="IsUseDeterministicConversionFactors">
    <xsl:element name="UseDeterministicConversionFactors">
      <xsl:apply-templates select="@*|node()"/>
    </xsl:element>
  </xsl:template>

  <!-- ConversionSettings: Rename IsDeriveModelledFoodsFromSampleBasedConcentrations to DeriveModelledFoodsFromSampleBasedConcentrations -->
  <xsl:template match="IsDeriveModelledFoodsFromSampleBasedConcentrations">
    <xsl:element name="DeriveModelledFoodsFromSampleBasedConcentrations">
      <xsl:apply-templates select="@*|node()"/>
    </xsl:element>
  </xsl:template>

  <!-- ConversionSettings: Rename IsDeriveModelledFoodsFromSingleValueConcentrations to DeriveModelledFoodsFromSingleValueConcentrations -->
  <xsl:template match="IsDeriveModelledFoodsFromSingleValueConcentrations">
    <xsl:element name="DeriveModelledFoodsFromSingleValueConcentrations">
      <xsl:apply-templates select="@*|node()"/>
    </xsl:element>
  </xsl:template>

  <!-- ConversionSettings: Rename IsSubstanceIndependent to SubstanceIndependent -->
  <xsl:template match="IsSubstanceIndependent">
    <xsl:element name="SubstanceIndependent">
      <xsl:apply-templates select="@*|node()"/>
    </xsl:element>
  </xsl:template>

  <!-- EffectSettings: Rename IsIncludeSubstancesWithUnknowMemberships to IncludeSubstancesWithUnknowMemberships -->
  <xsl:template match="IsIncludeSubstancesWithUnknowMemberships">
    <xsl:element name="IncludeSubstancesWithUnknowMemberships">
      <xsl:apply-templates select="@*|node()"/>
    </xsl:element>
  </xsl:template>

  <!-- EffectSettings: Rename IsMergeDoseResponseExperimentsData to MergeDoseResponseExperimentsData -->
  <xsl:template match="IsMergeDoseResponseExperimentsData">
    <xsl:element name="MergeDoseResponseExperimentsData">
      <xsl:apply-templates select="@*|node()"/>
    </xsl:element>
  </xsl:template>

  <!-- EffectSettings: Rename IsMultipleEffects to MultipleEffects -->
  <xsl:template match="IsMultipleEffects">
    <xsl:element name="MultipleEffects">
      <xsl:apply-templates select="@*|node()"/>
    </xsl:element>
  </xsl:template>

  <!-- EffectSettings: Rename IsRestrictAopByFocalUpstreamEffect to RestrictAopByFocalUpstreamEffect -->
  <xsl:template match="IsRestrictAopByFocalUpstreamEffect">
    <xsl:element name="RestrictAopByFocalUpstreamEffect">
      <xsl:apply-templates select="@*|node()"/>
    </xsl:element>
  </xsl:template>

  <!-- EffectSettings: Rename IsRestrictToAvailableHazardCharacterisations to RestrictToAvailableHazardCharacterisations -->
  <xsl:template match="IsRestrictToAvailableHazardCharacterisations">
    <xsl:element name="RestrictToAvailableHazardCharacterisations">
      <xsl:apply-templates select="@*|node()"/>
    </xsl:element>
  </xsl:template>

  <!-- EffectSettings: Rename IsRestrictToAvailableHazardDose to RestrictToAvailableHazardDoses -->
  <xsl:template match="IsRestrictToAvailableHazardDose">
    <xsl:element name="RestrictToAvailableHazardDoses">
      <xsl:apply-templates select="@*|node()"/>
    </xsl:element>
  </xsl:template>

  <!-- EffectSettings: Rename IsRestrictToAvailableRpfs to RestrictToAvailableRpfs -->
  <xsl:template match="IsRestrictToAvailableRpfs">
    <xsl:element name="RestrictToAvailableRpfs">
      <xsl:apply-templates select="@*|node()"/>
    </xsl:element>
  </xsl:template>

  <!-- EffectSettings: Rename IsRestrictToCertainMembership to RestrictToCertainMembership -->
  <xsl:template match="IsRestrictToCertainMembership">
    <xsl:element name="RestrictToCertainMembership">
      <xsl:apply-templates select="@*|node()"/>
    </xsl:element>
  </xsl:template>

  <!-- EffectSettings: Rename IsUseMolecularDockingModels to UseMolecularDockingModels -->
  <xsl:template match="IsUseMolecularDockingModels">
    <xsl:element name="UseMolecularDockingModels">
      <xsl:apply-templates select="@*|node()"/>
    </xsl:element>
  </xsl:template>

  <!-- EffectSettings: Rename IsUseProbabilisticMemberships to UseProbabilisticMemberships -->
  <xsl:template match="IsUseProbabilisticMemberships">
    <xsl:element name="UseProbabilisticMemberships">
      <xsl:apply-templates select="@*|node()"/>
    </xsl:element>
  </xsl:template>

  <!-- EffectSettings: Rename IsUseQsarModels to UseQsarModels -->
  <xsl:template match="IsUseQsarModels">
    <xsl:element name="UseQsarModels">
      <xsl:apply-templates select="@*|node()"/>
    </xsl:element>
  </xsl:template>

  <!-- HumanMonitoringSettings: Rename IsCorrelateTargetExposures to CorrelateTargetExposures -->
  <xsl:template match="IsCorrelateTargetExposures">
    <xsl:element name="CorrelateTargetExposures">
      <xsl:apply-templates select="@*|node()"/>
    </xsl:element>
  </xsl:template>

  <!-- OutputDetailSettings: Rename IsStoreIndividualDayIntakes to StoreIndividualDayIntakes -->
  <xsl:template match="IsStoreIndividualDayIntakes">
    <xsl:element name="StoreIndividualDayIntakes">
      <xsl:apply-templates select="@*|node()"/>
    </xsl:element>
  </xsl:template>

  <!-- OutputDetailSettings: Rename IsSummarizeSimulatedData to SummarizeSimulatedData -->
  <xsl:template match="IsSummarizeSimulatedData">
    <xsl:element name="SummarizeSimulatedData">
      <xsl:apply-templates select="@*|node()"/>
    </xsl:element>
  </xsl:template>

  <!-- SubsetSettings: Rename IsExpressSingleValueConsumptionsPerPerson to ExpressSingleValueConsumptionsPerPerson -->
  <xsl:template match="IsExpressSingleValueConsumptionsPerPerson">
    <xsl:element name="ExpressSingleValueConsumptionsPerPerson">
      <xsl:apply-templates select="@*|node()"/>
    </xsl:element>
  </xsl:template>

  <!-- SubsetSettings: Rename IsRestrictConsumptionsByFoodAsEatenSubset to RestrictConsumptionsByFoodAsEatenSubset -->
  <xsl:template match="IsRestrictConsumptionsByFoodAsEatenSubset">
    <xsl:element name="RestrictConsumptionsByFoodAsEatenSubset">
      <xsl:apply-templates select="@*|node()"/>
    </xsl:element>
  </xsl:template>

  <!-- SubsetSettings: Rename IsRestrictPopulationByFoodAsEatenSubset to RestrictPopulationByFoodAsEatenSubset -->
  <xsl:template match="IsRestrictPopulationByFoodAsEatenSubset">
    <xsl:element name="RestrictPopulationByFoodAsEatenSubset">
      <xsl:apply-templates select="@*|node()"/>
    </xsl:element>
  </xsl:template>

  <!-- SubsetSettings: Rename IsRestrictPopulationByModelledFoodSubset to RestrictPopulationByModelledFoodSubset -->
  <xsl:template match="IsRestrictPopulationByModelledFoodSubset">
    <xsl:element name="RestrictPopulationByModelledFoodSubset">
      <xsl:apply-templates select="@*|node()"/>
    </xsl:element>
  </xsl:template>

  <!-- SubsetSettings: Rename IsRestrictToModelledFoodSubset to RestrictToModelledFoodSubset -->
  <xsl:template match="IsRestrictToModelledFoodSubset">
    <xsl:element name="RestrictToModelledFoodSubset">
      <xsl:apply-templates select="@*|node()"/>
    </xsl:element>
  </xsl:template>

</xsl:stylesheet>
