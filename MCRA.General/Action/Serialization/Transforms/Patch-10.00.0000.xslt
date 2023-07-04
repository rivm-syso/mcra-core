<?xml version="1.0" encoding="utf-8"?>
<!--
Stylesheet for transforms from 9.2.xx to version 10.0.0 of MCRA
Issue: Change Tiers to custom for deprecated tier codes
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

  <!--Deprecate module tiers Efsa2022DietaryCraTier1: rename to custom -->
  <xsl:template match="ConcentrationModelChoice[.='Efsa2022DietaryCraTier1']">
    <ConcentrationModelChoice>Custom</ConcentrationModelChoice>
  </xsl:template>
  <xsl:template match="ConcentrationsTier[.='Efsa2022DietaryCraTier1']">
    <ConcentrationsTier>Custom</ConcentrationsTier>
  </xsl:template>
  <xsl:template match="DietaryIntakeCalculationTier[.='Efsa2022DietaryCraTier1']">
    <DietaryIntakeCalculationTier>Custom</DietaryIntakeCalculationTier>
  </xsl:template>
  <xsl:template match="OccurrencePatternsTier[.='Efsa2022DietaryCraTier1']">
    <OccurrencePatternsTier>Custom</OccurrencePatternsTier>
  </xsl:template>
  <xsl:template match="ConsumptionsTier[.='Efsa2022DietaryCraTier1']">
    <ConsumptionsTier>Custom</ConsumptionsTier>
  </xsl:template>
  <xsl:template match="RiskCalculationTier[.='Efsa2022DietaryCraTier1']">
    <RiskCalculationTier>Custom</RiskCalculationTier>
  </xsl:template>
  <xsl:template match="SingleValueRisksCalculationTier[.='Efsa2022DietaryCraTier1']">
    <SingleValueRisksCalculationTier>Custom</SingleValueRisksCalculationTier>
  </xsl:template>

  <!--Deprecate module tiers Efsa2022DietaryCraTier2: rename to custom -->
  <xsl:template match="ConcentrationModelChoice[.='Efsa2022DietaryCraTier2']">
    <ConcentrationModelChoice>Custom</ConcentrationModelChoice>
  </xsl:template>
  <xsl:template match="ConcentrationsTier[.='Efsa2022DietaryCraTier2']">
    <ConcentrationsTier>Custom</ConcentrationsTier>
  </xsl:template>
  <xsl:template match="DietaryIntakeCalculationTier[.='Efsa2022DietaryCraTier2']">
    <DietaryIntakeCalculationTier>Custom</DietaryIntakeCalculationTier>
  </xsl:template>
  <xsl:template match="OccurrencePatternsTier[.='Efsa2022DietaryCraTier2']">
    <OccurrencePatternsTier>Custom</OccurrencePatternsTier>
  </xsl:template>
  <xsl:template match="ConsumptionsTier[.='Efsa2022DietaryCraTier2']">
    <ConsumptionsTier>Custom</ConsumptionsTier>
  </xsl:template>
  <xsl:template match="RiskCalculationTier[.='Efsa2022DietaryCraTier2']">
    <RiskCalculationTier>Custom</RiskCalculationTier>
  </xsl:template>
  <xsl:template match="SingleValueRisksCalculationTier[.='Efsa2022DietaryCraTier2']">
    <SingleValueRisksCalculationTier>Custom</SingleValueRisksCalculationTier>
  </xsl:template>

</xsl:stylesheet>
