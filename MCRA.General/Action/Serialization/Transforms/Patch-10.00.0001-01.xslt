<?xml version="1.0" encoding="utf-8"?>
<!--
Stylesheet for transforms from 10.0.0 to version 10.0.1 of MCRA
Issues:
A. Create separate matrix selection settings for kinetic models and HBM analysis (#1684)
B. New names for biological matrices and sampling method codes (#1685)
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

  <!--Rename EffectModelSettings to RisksSettings -->
  <xsl:template match="EffectModelSettings">
    <xsl:element name="RisksSettings">
      <!-- Copy all existing data; this and child nodes -->
      <xsl:apply-templates select="@*|node()"/>
    </xsl:element>
  </xsl:template>

  <!--Rename IntakeModelPerCategoryDto to IntakeModelPerCategory -->
  <xsl:template match="IntakeModelPerCategoryDto">
    <xsl:element name="IntakeModelPerCategory">
      <!-- Copy all existing data; this and child nodes -->
      <xsl:apply-templates select="@*|node()"/>
    </xsl:element>
  </xsl:template>

  <!--Rename IndividualsSubsetDefinitionDto to IndividualsSubsetDefinition -->
  <xsl:template match="IndividualsSubsetDefinitionDto">
    <xsl:element name="IndividualsSubsetDefinition">
      <!-- Copy all existing data; this and child nodes -->
      <xsl:apply-templates select="@*|node()"/>
    </xsl:element>
  </xsl:template>

  <!--Rename SamplesSubsetDefinitionDto to SamplesSubsetDefinitionDto -->
  <xsl:template match="SamplesSubsetDefinitionDto">
    <xsl:element name="SamplesSubsetDefinition">
      <!-- Copy all existing data; this and child nodes -->
      <xsl:apply-templates select="@*|node()"/>
    </xsl:element>
  </xsl:template>

  <!--Rename ConcentrationModelTypesPerFoodCompound element to ConcentrationModelTypesFoodSubstance -->
  <xsl:template match="ConcentrationModelTypesPerFoodCompound">
    <xsl:element name="ConcentrationModelTypesFoodSubstance">
      <!-- Copy all existing data; this and child nodes -->
      <xsl:apply-templates select="@*|node()"/>
    </xsl:element>
  </xsl:template>

  <!--Rename ConcentrationModelTypePerFoodCompoundDto to ConcentrationModelTypeFoodSubstance -->
  <xsl:template match="ConcentrationModelTypePerFoodCompoundDto">
    <ConcentrationModelTypeFoodSubstance>
      <FoodCode><xsl:value-of select="CodeFood"/></FoodCode>
      <SubstanceCode><xsl:value-of select="CodeCompound"/></SubstanceCode>
      <ModelType><xsl:value-of select="ConcentrationModelType"/></ModelType>
    </ConcentrationModelTypeFoodSubstance>
  </xsl:template>

  <!-- rename FocalFoodDto element -->
  <xsl:template match="FocalFoodDto">
    <xsl:element name="FocalFood">
      <xsl:apply-templates select="@*|node()"/>
    </xsl:element>
  </xsl:template>
</xsl:stylesheet>
