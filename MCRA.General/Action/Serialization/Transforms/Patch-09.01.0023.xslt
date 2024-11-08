<?xml version="1.0" encoding="utf-8"?>
<!--
Stylesheet for transforms up to version 9.1.23 of MCRA
This stylesheet contains the aggregated changes that were made before the incremental
transform system was introduced in version 9.1.36 (The latest changes to this file were for MCRA Version 9.1.23)

This file transforms MCRA project XML files from MCRA version 8(8.1) and later.
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

  <!-- Create ActionType element if one doesn't exist and take the value from
        The AssessmentSettings/AssessmentType if it is specified -->
  <xsl:template match="/Project/AssessmentSettings">
    <xsl:if test="not(/Project/ActionType) or /Project/ActionType='Unknown'">
      <xsl:element name="ActionType">
        <xsl:choose>
          <xsl:when test="AssessmentType = 'Exposure'">
            <xsl:choose>
              <xsl:when test="Aggregate = 'true'">TargetExposures</xsl:when>
              <xsl:otherwise>DietaryExposures</xsl:otherwise>
            </xsl:choose>
          </xsl:when>
          <xsl:when test="AssessmentType = 'HealthImpact'">Risks</xsl:when>
          <xsl:when test="AssessmentType = 'DoseResponseModelling'">PointsOfDeparture</xsl:when>
          <xsl:when test="AssessmentType = 'ConcentrationModelling'">ConcentrationModels</xsl:when>
          <xsl:when test="AssessmentType = 'Consumption'">Consumptions</xsl:when>
          <xsl:otherwise>Unknown</xsl:otherwise>
        </xsl:choose>
      </xsl:element>
    </xsl:if>
    <xsl:copy>
      <xsl:apply-templates select="@*|node()"/>
    </xsl:copy>
  </xsl:template>
  <!--Remove AssessmentType element from XML-->
  <xsl:template match="/Project/AssessmentSettings/AssessmentType" />
  <!--Remove any original ActionType=Unknown from XML-->
  <xsl:template match="/Project/ActionType[.='Unknown']" />

  <xsl:variable name="hasOldScreening" select="/Project/ScreeningSettings/IsScreening = 'true'" />

  <!-- Add any new single elements that should be added under /Project here -->
  <xsl:template match="/Project">
    <xsl:copy>
      <!-- Add ScopeKeysFilters Element when it does not exist -->
      <xsl:if test="not(ScopeKeysFilters)">
        <xsl:element name="ScopeKeysFilters">
          <xsl:call-template name="AddNewScopeKeysFiltersTemplate" />
        </xsl:element>
      </xsl:if>
      <xsl:if test="$hasOldScreening and not(DietaryIntakeCalculationSettings)">
        <xsl:element name="DietaryIntakeCalculationSettings">
          <xsl:element name="DietaryExposuresDetailsLevel">
            OnlyRiskDrivers
          </xsl:element>
        </xsl:element>
      </xsl:if>
      <xsl:apply-templates select="@*|node()"/>
    </xsl:copy>
  </xsl:template>

  <!-- Existing scope keys filter node -->
  <xsl:template match="/Project/ScopeKeysFilters">
    <xsl:copy>
      <xsl:apply-templates select="@*|node()"/>
      <!-- Add new nodes, call the template for new scope keys filters below -->
      <xsl:call-template name="AddNewScopeKeysFiltersTemplate" />
    </xsl:copy>
  </xsl:template>

  <!-- Remove any existing ScopeKeysFilter elements which have no selected codes
       by copying only elements which do have selected codes  -->
  <xsl:template match="/Project/ScopeKeysFilters/ScopeKeysFilter">
    <xsl:if test="count(SelectedCodes/string)&gt;0">
      <xsl:copy>
        <xsl:apply-templates select="@*|node()"/>
      </xsl:copy>
    </xsl:if>
  </xsl:template>

  <xsl:template name="AddNewScopeKeysFiltersTemplate">
    <!-- Add selected compounds with a scope keys filter for these compounds-->
    <xsl:if test="count(/Project/SelectedCompounds/SelectedCompoundDto/CodeCompound)&gt;0">
      <xsl:call-template name="ScopeKeysFilterTemplate">
        <xsl:with-param name="ScopingType">Compounds</xsl:with-param>
        <xsl:with-param name="Codes" select="/Project/SelectedCompounds/SelectedCompoundDto/CodeCompound" />
      </xsl:call-template>
    </xsl:if>
    <!-- Add a scope keys filter for the selected food survey setting-->
    <xsl:if test="/Project/FoodSurveySettings/CodeFoodSurvey">
      <xsl:call-template name="ScopeKeysFilterTemplate">
        <xsl:with-param name="ScopingType">FoodSurveys</xsl:with-param>
        <xsl:with-param name="Codes" select="/Project/FoodSurveySettings/CodeFoodSurvey" />
      </xsl:call-template>
    </xsl:if>

    <!-- Add a scope keys filter for the selected effect setting-->
    <xsl:variable name="allowMultipleEffects"
      select="/Project/EffectSettings/IncludeAopNetwork = 'true' or /Project/EffectSettings/RestrictToCriticalEffect = 'true'" />
    <xsl:if test="/Project/EffectSettings/CodeEffect and not($allowMultipleEffects)">
      <xsl:call-template name="ScopeKeysFilterTemplate">
        <xsl:with-param name="ScopingType">Effects</xsl:with-param>
        <xsl:with-param name="Codes" select="/Project/EffectSettings/CodeEffect" />
      </xsl:call-template>
    </xsl:if>
  </xsl:template>

  <!-- Remove obsolete selected compounds element -->
  <xsl:template match="/Project/SelectedCompounds" />
  <!-- TODO: Remove obsolete selected food survey code
       Keep it while mcra 9.0 and 9.1 still coexist
       <xsl:template match="/Project/FoodSurveySettings" />
  -->

  <xsl:template match="/Project/DietaryIntakeCalculationSettings">
    <xsl:copy>
      <!-- Copy all existing data; this and child nodes -->
      <xsl:apply-templates select="@*|node()"/>
      <!-- Add DietaryExposuresDetailsLevel -->
      <xsl:if test="$hasOldScreening and not(/Project/DietaryIntakeCalculationSettings/DietaryExposuresDetailsLevel)">
        <xsl:element name="DietaryExposuresDetailsLevel">OnlyRiskDrivers</xsl:element>
      </xsl:if>
    </xsl:copy>
  </xsl:template>

  <!-- Remove old IsScreening setting -->
  <xsl:template match="/Project/ScreeningSettings/IsScreening" />

  <!-- Convert old FoodSubsetSelection -->
  <!-- Get all decision selection variables into xsl boolean types -->
  <xsl:variable name="hasOldSubsetSettings"
    select="/Project/SubsetSettings/FoodSubsetSelection or /Project/SubsetSettings/TreatFoodSubsetAsFocalFoodEatersConcept" />
  <xsl:variable name="consumerDaysOnly" select="/Project/SubsetSettings/ConsumerDaysOnly = 'true'" />
  <xsl:variable name="isFoodSubsetSelection" select="/Project/SubsetSettings/FoodSubsetSelection = 'true'" />
  <xsl:variable name="hasEatenFoods" select="count(/Project/FoodAsEatenSubset/*) &gt; 0" />
  <xsl:variable name="hasMeasuredFoods" select="count(/Project/FoodAsMeasuredSubset/*) &gt; 0" />
  <xsl:variable name="restrictPopulation" select="/Project/SubsetSettings/TreatFoodSubsetAsFocalFoodEatersConcept = 'true'" />

  <!-- Special case for ConsumerDaysOnly:  -->
  <xsl:template match="/Project/SubsetSettings/ConsumerDaysOnly">
    <xsl:element name="ConsumerDaysOnly">
      <xsl:value-of select="$consumerDaysOnly or ($hasOldSubsetSettings and $isFoodSubsetSelection and $hasEatenFoods)"/>
    </xsl:element>
  </xsl:template>

  <xsl:template match="/Project/SubsetSettings">
    <!-- Copy the whole node -->
    <xsl:copy>
      <!-- Copy all existing data; this and child nodes -->
      <xsl:apply-templates select="@*|node()"/>
      <!-- Add the new elements, only if old subset settings were found -->
      <xsl:if test="$hasOldSubsetSettings">
        <!-- Only if food subset selection was true -->
        <xsl:if test="$isFoodSubsetSelection">
          <xsl:element name="IsRestrictPopulationByFoodAsEatenSubset">
            <xsl:value-of select="$restrictPopulation and $hasEatenFoods"/>
          </xsl:element>
          <xsl:element name="IsRestrictConsumptionsByFoodAsEatenSubset">
            <xsl:value-of select="not($restrictPopulation) and $hasEatenFoods"/>
          </xsl:element>
          <xsl:element name="IsRestrictPopulationByModelledFoodSubset">
            <xsl:value-of select="$restrictPopulation and $hasMeasuredFoods"/>
          </xsl:element>
          <xsl:element name="IsRestrictToModelledFoodSubset">
            <xsl:value-of select="not($restrictPopulation) and $hasMeasuredFoods" />
          </xsl:element>
        </xsl:if>
        <!-- Add boolean property ModelledFoodsConsumerDaysOnly -->
        <xsl:element name="ModelledFoodsConsumerDaysOnly">
          <xsl:value-of select="$consumerDaysOnly or ($restrictPopulation and $hasMeasuredFoods)" />
        </xsl:element>
      </xsl:if>
    </xsl:copy>
  </xsl:template>
  <!--Remove FoodSubsetSelection element from XML-->
  <xsl:template match="/Project/SubsetSettings/FoodSubsetSelection" />
  <!--Remove TreatFoodSubsetAsFocalFoodEatersConcept element from XML-->
  <xsl:template match="/Project/SubsetSettings/TreatFoodSubsetAsFocalFoodEatersConcept" />


  <!--BEGIN 20200824 issue #610: Refactor FoodsAsMeasured to ModelledFoods -->
  <!--Rename IsDeriveFoodsAsMeasuredFromSampleBasedConcentrations to IsDeriveModelledFoodsFromSampleBasedConcentrations -->
  <xsl:template match="IsDeriveFoodsAsMeasuredFromSampleBasedConcentrations">
    <xsl:element name="IsDeriveModelledFoodsFromSampleBasedConcentrations">
      <xsl:apply-templates select="@*|node()"/>
    </xsl:element>
  </xsl:template>
  <!--Rename IsDeriveFoodsAsMeasuredFromSingleValueConcentrations to IsDeriveModelledFoodsFromSingleValueConcentrations -->
  <xsl:template match="IsDeriveFoodsAsMeasuredFromSingleValueConcentrations">
    <xsl:element name="IsDeriveModelledFoodsFromSingleValueConcentrations">
      <xsl:apply-templates select="@*|node()"/>
    </xsl:element>
  </xsl:template>

  <!--Rename FoodAsMeasuredSubsetDto to ModelledFoodSubsetDto -->
  <xsl:template match="FoodAsMeasuredSubsetDto">
    <xsl:element name="ModelledFoodSubsetDto">
      <xsl:apply-templates select="@*|node()"/>
    </xsl:element>
  </xsl:template>

  <!--Rename FoodsAsMeasuredConsumerDaysOnly to ModelledFoodsConsumerDaysOnly -->
  <xsl:template match="FoodsAsMeasuredConsumerDaysOnly">
    <xsl:element name="ModelledFoodsConsumerDaysOnly">
      <xsl:apply-templates select="@*|node()"/>
    </xsl:element>
  </xsl:template>
  <!--Rename IsRestrictPopulationByFoodAsMeasuredSubset to IsRestrictPopulationByModelledFoodSubset -->
  <xsl:template match="IsRestrictPopulationByFoodAsMeasuredSubset">
    <xsl:element name="IsRestrictPopulationByModelledFoodSubset">
      <xsl:apply-templates select="@*|node()"/>
    </xsl:element>
  </xsl:template>
  <!--Rename IsRestrictToFoodAsMeasuredSubset to IsRestrictToModelledFoodSubset -->
  <xsl:template match="IsRestrictToFoodAsMeasuredSubset">
    <xsl:element name="IsRestrictToModelledFoodSubset">
      <xsl:apply-templates select="@*|node()"/>
    </xsl:element>
  </xsl:template>

  <!--Rename ActionType 'FoodsAsMeasured' to 'ModelledFoods' -->
  <xsl:template match="ActionType[.='FoodsAsMeasured']">
    <ActionType>ModelledFoods</ActionType>
  </xsl:template>
  <!--END 20200824 issue #610: Refactor FoodsAsMeasured to ModelledFoods -->

  <!--BEGIN 20200825 issue #605: Refactor AgriculturalUses to OccurrencePatterns -->
  <!--Rename ActionType 'AgriculturalUses' to 'OccurrencePatterns' -->
  <xsl:template match="ActionType[.='AgriculturalUses']">
    <ActionType>OccurrencePatterns</ActionType>
  </xsl:template>
  <xsl:template match="ScopingType[.='AgriculturalUses']">
    <ScopingType>OccurrencePatterns</ScopingType>
  </xsl:template>
  <xsl:template match="ScopingType[.='AgriculturalUsesHasCompounds']">
    <ScopingType>OccurrencePatternsHasCompounds</ScopingType>
  </xsl:template>
  <!--END 20200825 issue #605: Refactor AgriculturalUses to OccurrencePatterns -->

  <!--BEGIN 20200825 issue #609: Refactor AssessmentGroupMemberships to ActiveSubstances -->
  <!--Rename ActionType 'AssessmentGroupMemberships' to 'ActiveSubstances' -->
  <xsl:template match="ActionType[.='AssessmentGroupMemberships']">
    <ActionType>ActiveSubstances</ActionType>
  </xsl:template>
  <xsl:template match="ScopingType[.='AssessmentGroupMemberships']">
    <ScopingType>ActiveSubstances</ScopingType>
  </xsl:template>
  <xsl:template match="ScopingType[.='AssessmentGroupMembershipModels']">
    <ScopingType>ActiveSubstancesModels</ScopingType>
  </xsl:template>
  <!--END 20200825 issue #609: Refactor AssessmentGroupMemberships to ActiveSubstances -->

  <!--BEGIN 20200826 issue #611: Refactor DietaryExposureScreening to HighExposureFoodSubstanceCombinations -->
  <!--Rename ActionType 'DietaryExposureScreening' to 'HighExposureFoodSubstanceCombinations' -->
  <xsl:template match="ActionType[.='DietaryExposureScreening']">
    <ActionType>HighExposureFoodSubstanceCombinations</ActionType>
  </xsl:template>
  <!--END 20200826 issue #611: Refactor DietaryExposureScreening to HighExposureFoodSubstanceCombinations -->

  <!--BEGIN 20200826 issue #607: Refactor HazardDoses to PointsOfDeparture -->
  <!--Rename ActionType 'HazardDoses' to 'PointsOfDeparture' -->
  <xsl:template match="ActionType[.='HazardDoses']">
    <ActionType>PointsOfDeparture</ActionType>
  </xsl:template>
  <xsl:template match="ScopingType[.='HazardDoses']">
    <ScopingType>PointsOfDeparture</ScopingType>
  </xsl:template>
  <!--END 20200826 issue #607: Refactor HazardDoses to PointsOfDeparture -->

  <!--BEGIN 20200826 issue #613: Refactor ResidueDefinitions to SubstanceConversions -->
  <!--Rename ActionType 'ResidueDefinitions' to 'SubstanceConversions' -->
  <xsl:template match="ActionType[.='ResidueDefinitions']">
    <ActionType>SubstanceConversions</ActionType>
  </xsl:template>
  <xsl:template match="ScopingType[.='ResidueDefinitions']">
    <ScopingType>SubstanceConversions</ScopingType>
  </xsl:template>
  <!--END 20200826 issue #613: Refactor ResidueDefinitions to SubstanceConversions -->

  <!--BEGIN 20200826 issue #606: Refactor TargetHazardDoses to HazardCharacterisations -->
  <!--Rename ActionType 'TargetHazardDoses' to 'HazardCharacterisations' -->
  <xsl:template match="ActionType[.='TargetHazardDoses']">
    <ActionType>HazardCharacterisations</ActionType>
  </xsl:template>
  <!--END 20200826 issue #606: Refactor TargetHazardDoses to HazardCharacterisations -->

  <!--BEGIN 20200826 issue #608: Refactor ResidueLimits to ConcentrationLimits -->
  <!--Rename ActionType 'ResidueLimits' to 'ConcentrationLimits' -->
  <xsl:template match="ActionType[.='ResidueLimits']">
    <ActionType>ConcentrationLimits</ActionType>
  </xsl:template>
  <xsl:template match="ScopingType[.='MaximumResidueLimits']">
    <ScopingType>ConcentrationLimits</ScopingType>
  </xsl:template>
  <!--END 20200826 issue #608: Refactor ResidueLimits to ConcentrationLimits -->

  <!--BEGIN 20200826 issue #612: Refactor FocalFoods to FocalFoodConcentrations -->
  <!--Rename ActionType 'FocalFoods' to 'FocalFoodConcentrations' -->
  <xsl:template match="ActionType[.='FocalFoods']">
    <ActionType>FocalFoodConcentrations</ActionType>
  </xsl:template>
  <!--END 20200826 issue #612: Refactor FocalFoods to FocalFoodConcentrations -->

  <!--BEGIN 20200826 issue #614: Refactor AuthorisedUses to SubstanceAuthorisations -->
  <!--Rename ActionType 'AuthorisedUses' to 'SubstanceAuthorisations' -->
  <xsl:template match="ActionType[.='AuthorisedUses']">
    <ActionType>SubstanceAuthorisations</ActionType>
  </xsl:template>
  <xsl:template match="ScopingType[.='AuthorisedUses']">
    <ScopingType>SubstanceAuthorisations</ScopingType>
  </xsl:template>
  <!--END 20200826 issue #614: Refactor AuthorisedUses to SubstanceAuthorisations -->

  <!--BEGIN 20201202 issue #724: Refactor ConsumptionsPerFoodAsMeasured to ConsumptionsByModelledFood -->
  <!--Rename ActionType 'ConsumptionsPerFoodAsMeasured' to 'ConsumptionsByModelledFood' -->
  <xsl:template match="ActionType[.='ConsumptionsPerFoodAsMeasured']">
    <ActionType>ConsumptionsByModelledFood</ActionType>
  </xsl:template>
  <!--END 220201202 issue #724: Refactor ConsumptionsPerFoodAsMeasured to ConsumptionsByModelledFood -->

  <!-- Concentration model settings: Replace ProcessingFactorModel with booleans -->
  <xsl:variable name="oldProcessingFactorModel" select="/Project/ConcentrationModelSettings/ProcessingFactorModel" />
  <xsl:template match="/Project/ConcentrationModelSettings">
    <!-- Copy the whole node -->
    <xsl:copy>
      <!-- Copy all existing data; this and child nodes -->
      <xsl:apply-templates select="@*|node()"/>
      <!-- Add the new elements, only if old subset settings were found -->
      <xsl:if test="$oldProcessingFactorModel">
        <!-- Add boolean property isProcessing -->
        <xsl:element name="IsProcessing">
          <xsl:value-of select="$oldProcessingFactorModel != 'None'" />
        </xsl:element>
        <xsl:element name="IsDistribution">
          <xsl:value-of select="contains($oldProcessingFactorModel, 'DistributionBased')" />
        </xsl:element>
        <xsl:element name="AllowHigherThanOne">
          <xsl:value-of select="contains($oldProcessingFactorModel, 'AllowHigher')" />
        </xsl:element>
      </xsl:if>
    </xsl:copy>
  </xsl:template>
  <!--Remove ProcessingFactorModel element from XML-->
  <xsl:template match="/Project/ConcentrationModelSettings/ProcessingFactorModel" />


  <!-- Concentration model settings: Replace ProcessingFactorModel with booleans -->
  <xsl:variable name="varUnitVariabilityModel" select="/Project/UnitVariabilitySettings/UnitVariabilityModel" />
  <xsl:template match="/Project/UnitVariabilitySettings">
    <!-- Copy the whole node -->
    <xsl:copy>
      <!-- Copy all existing data; this and child nodes -->
      <xsl:apply-templates select="@*|node()"/>
      <!-- Add boolean property UseUnitVariability -->
      <xsl:element name="UseUnitVariability">
        <xsl:value-of select="$varUnitVariabilityModel != 'NoUnitVariability'" />
      </xsl:element>
    </xsl:copy>
  </xsl:template>
  <!--Remove ProcessingFactorModel element from XML-->
  <xsl:template match="/Project/UnitVariabilitySettings/UnitVariabilityModel[.='NoUnitVariability']" />


  <!-- Agricultural use settings: add occurrence pattern settings -->
  <xsl:template match="/Project/AgriculturalUseSettings">
    <!-- Copy the whole node -->
    <xsl:copy>
      <!-- Copy all existing data; this and child nodes -->
      <xsl:apply-templates select="@*|node()"/>
      <!-- Check whether property IsUseOccurrenceFrequencies doesn't exist -->
      <xsl:if test="not(IsUseOccurrenceFrequencies)">
        <!-- Add boolean property IsUseOccurrenceFrequencies and set equal to IsUseAgriculturalUseTable -->
        <xsl:element name="IsUseOccurrenceFrequencies">
          <xsl:value-of select="IsUseAgriculturalUseTable = 'true'" />
        </xsl:element>
      </xsl:if>
      <xsl:if test="not(UseOccurrencePatternsForResidueGeneration)">
        <!-- Add boolean property UseOccurrencePatternsForResidueGeneration and set equal to IsUseAgriculturalUseTable -->
        <xsl:element name="UseOccurrencePatternsForResidueGeneration">
          <xsl:value-of select="IsUseAgriculturalUseTable = 'true'" />
        </xsl:element>
      </xsl:if>
    </xsl:copy>
  </xsl:template>

  <!-- Optionally add FoodAsEatenSubset collection to focal food as eaten collection -->
  <xsl:template match="/Project/FoodAsEatenSubset">
    <xsl:copy>
      <!-- Copy all existing data; this and child nodes -->
      <xsl:apply-templates select="@*|node()"/>
    </xsl:copy>
    <!-- Conditionally add all foods as eaten codes to the focal food as eaten subset -->
    <xsl:if test="$hasOldSubsetSettings and $isFoodSubsetSelection and $restrictPopulation and $hasEatenFoods">
      <xsl:element name="FocalFoodAsEatenSubset">
        <xsl:call-template name="CodesAsStringsTemplate">
          <xsl:with-param name="Codes" select="FoodAsEatenSubsetDto/CodeFood" />
        </xsl:call-template>
      </xsl:element>
    </xsl:if>
  </xsl:template>

  <!-- Optionally add FoodAsMeasured collection to focal food as measured collection -->
  <xsl:template match="/Project/FoodAsMeasuredSubset">
    <!-- Create new element 'ModelledFoodSubset' and copy all content -->
    <xsl:element name="ModelledFoodSubset">
      <xsl:apply-templates select="@*|node()"/>
    </xsl:element>
    <!-- Conditionally add all foods as measured codes to the focal food as measured subset -->
    <xsl:if test="$hasOldSubsetSettings and $isFoodSubsetSelection and $restrictPopulation and $hasMeasuredFoods">
      <xsl:element name="FocalFoodAsMeasuredSubset">
        <xsl:call-template name="CodesAsStringsTemplate">
          <xsl:with-param name="Codes" select="FoodAsMeasuredSubsetDto/CodeFood" />
        </xsl:call-template>
      </xsl:element>
    </xsl:if>
  </xsl:template>

  <!-- Effect settings: add is-multiple effects setting -->
  <xsl:template match="/Project/EffectSettings">
    <!-- Copy the whole node -->
    <xsl:copy>
      <!-- Copy all existing data; this and child nodes -->
      <xsl:apply-templates select="@*|node()"/>
      <!-- Check whether property IsMultipleEffects doesn't exist -->
      <xsl:if test="RestrictToCriticalEffect and not(IsMultipleEffects)">
        <!-- Add boolean property IsMultipleEffects and set equal to RestrictToCriticalEffect -->
        <xsl:element name="IsMultipleEffects">
          <xsl:value-of select="RestrictToCriticalEffect = 'true'" />
        </xsl:element>
      </xsl:if>
      <!-- Check whether property CodeFocalEffect doesn't exist -->
      <xsl:if test="not(CodeFocalEffect)">
        <xsl:element name="CodeFocalEffect">
          <xsl:value-of select="CodeEffect" />
        </xsl:element>
      </xsl:if>
    </xsl:copy>
  </xsl:template>

  <!-- Remove CodeEffect setting from xml -->
  <xsl:template match="/Project/EffectSettings/CodeEffect" />

  <!-- Refactor EffectModelSettings.IsHazardIndex to Enum RiskMetricType -->
  <xsl:template match="/Project/EffectModelSettings">
    <xsl:copy>
      <!-- Copy all existing data; this and child nodes -->
      <xsl:apply-templates select="@*|node()"/>
      <xsl:if test="not(RiskMetricType)">
        <xsl:element name="RiskMetricType">
          <xsl:choose>
            <xsl:when test="IsHazardIndex = 'true'">HazardIndex</xsl:when>
            <xsl:otherwise>MarginOfExposure</xsl:otherwise>
          </xsl:choose>
        </xsl:element>
      </xsl:if>
    </xsl:copy>
  </xsl:template>

  <!-- Remove IsHazardIndex setting from xml -->
  <xsl:template match="/Project/EffectModelSettings/IsHazardIndex" />

  <!-- Creates 'string' elements for each item in the input list
       The Codes parameter should contain the text values for this list -->
  <xsl:template name="CodesAsStringsTemplate">
    <xsl:param name="Codes" />
    <xsl:for-each select="$Codes">
      <xsl:element name="string">
        <xsl:value-of select="."/>
      </xsl:element>
    </xsl:for-each>
  </xsl:template>

  <!-- Creates 'ScopeKeysFilter' elements for each item in the input list
       The Codes parameter should contain the text values for this list -->
  <xsl:template name="ScopeKeysFilterTemplate">
    <xsl:param name="ScopingType" />
    <xsl:param name="Codes" />
    <!-- Check that there are no SelectedCodes/string elements for this ScopingType
      This means there is no element at all, or the element exists, but with no SelectedCodes
      Any existing element with no SelectedCodes is removed by another template -->
    <xsl:if test="count(/Project/ScopeKeysFilters/ScopeKeysFilter[ScopingType=$ScopingType]/SelectedCodes/string)=0">
      <xsl:element name="ScopeKeysFilter">
        <xsl:element name="ScopingType">
          <xsl:value-of select="$ScopingType"/>
        </xsl:element>
        <xsl:element name="SelectedCodes">
          <xsl:call-template name="CodesAsStringsTemplate">
            <xsl:with-param name="Codes" select="$Codes" />
          </xsl:call-template>
        </xsl:element>
      </xsl:element>
    </xsl:if>
  </xsl:template>
</xsl:stylesheet>
