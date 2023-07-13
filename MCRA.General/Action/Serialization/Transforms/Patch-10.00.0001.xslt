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

  <!-- Ignore/remove the NumberOfIndividuals from the old position at KineticModelSettings -->
  <xsl:template match="/Project/KineticModelSettings/NumberOfIndividuals" />

  <!-- Create separate biological matrix selection for HBM analysis and use new names for biological matrices  -->
  <xsl:variable name="codeCompartment" select="/Project/KineticModelSettings/CodeCompartment" />
  <xsl:variable name="isSerum" select="(/Project/HumanMonitoringSettings/SamplingMethodCodes/string='Blood_Serum')" />
  <xsl:variable name="isPlasma" select="(/Project/HumanMonitoringSettings/SamplingMethodCodes/string='Blood_Plasma')" />
  <xsl:template match="/Project/HumanMonitoringSettings">
    <xsl:copy>
      <!-- Copy all existing data; this and child nodes -->
      <xsl:apply-templates select="@*|node()"/>
      <xsl:choose>
        <xsl:when test="$codeCompartment = 'Blood' and ($isSerum)">
          <xsl:element name="HbmTargetMatrix">
            <xsl:value-of select="'BloodSerum'"/>
          </xsl:element>
        </xsl:when>
        <xsl:when test="$codeCompartment = 'Blood' and ($isPlasma)">
          <xsl:element name="HbmTargetMatrix">
            <xsl:value-of select="'BloodPlasma'"/>
          </xsl:element>
        </xsl:when>
        <xsl:otherwise>
          <xsl:element name="HbmTargetMatrix">
            <xsl:value-of select="$codeCompartment"/>
          </xsl:element>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:copy>
  </xsl:template>

  <xsl:template match="HumanMonitoringSettings/SamplingMethodCodes/string">
    <xsl:copy>
      <xsl:choose>
        <xsl:when test=". = 'Blood_Serum'">BloodSerum_Serum</xsl:when>
        <xsl:when test=". = 'Blood_Plasma'">BloodPlasma_Plasma</xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="text()" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:copy>
  </xsl:template>

  <xsl:template match="/Project/FoodAsEatenSubset/FoodAsEatenSubsetDto">
    <xsl:element name="FoodCode">
      <xsl:value-of select="CodeFood" />
    </xsl:element>
  </xsl:template>
  <xsl:template match="/Project/ModelledFoodSubset/ModelledFoodSubsetDto">
    <xsl:element name="FoodCode">
      <xsl:value-of select="CodeFood" />
    </xsl:element>
  </xsl:template>
  <xsl:template match="/Project/SelectedScenarioAnalysisFoods/SelectedScenarioAnalysisFoodDto">
    <xsl:element name="FoodCode">
      <xsl:value-of select="CodeFood" />
    </xsl:element>
  </xsl:template>
  <xsl:template match="/Project/IntakeModelSettings/IntakeModelsPerCategory/IntakeModelPerCategoryDto/FoodsAsMeasured/IntakeModelPerCategory_FoodAsMeasuredDto">
    <xsl:element name="FoodCode">
      <xsl:value-of select="CodeFood" />
    </xsl:element>
  </xsl:template>
</xsl:stylesheet>
