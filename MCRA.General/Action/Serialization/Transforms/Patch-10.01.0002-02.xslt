<?xml version="1.0" encoding="utf-8"?>
<!--
Stylesheet for transforms from version 10.1.1 to version 10.1.2 of MCRA Issues:
Remove setting AbsorptionFactorModel from Enum InternalModelType.
Relevant for HazardCharacterisations and TargetExposures
See unit test Patch_10_01_0002_01_Test_TargetLevelType
-->
<xsl:stylesheet version="2.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">
  <xsl:output method="xml" indent="yes" />

  <!-- copy all nodes and attributes, applying the templates hereafter -->
  <xsl:template match="@*|node()">
    <xsl:copy>
      <xsl:apply-templates select="@*|node()" />
    </xsl:copy>
  </xsl:template>

  <xsl:variable name="actionType" select="/Project/ActionType" />
  <xsl:variable name="targetExpIntModelType"
    select="/Project/ModuleConfigurations/ModuleConfiguration[@module='TargetExposures']/Settings/Setting[@id='InternalModelType']" />
  <xsl:variable name="hazardCharDoseLevelType"
    select="/Project/ModuleConfigurations/ModuleConfiguration[@module='HazardCharacterisations']/Settings/Setting[@id='TargetDoseLevelType']" />
  <xsl:variable name="exposureCalculationMethod"
    select="/Project/ModuleConfigurations/ModuleConfiguration[@module='Risks']/Settings/Setting[@id='ExposureCalculationMethod']" />


  <!-- remove setting InternalModelType from module KineticModels settings -->
  <xsl:template match="ModuleConfigurations/ModuleConfiguration[@module='KineticModels' or @module='PbkModels']/Settings/Setting[@id='InternalModelType']" />

  <!-- Replace values 'AbsorptionFactorModel' with 'ConversionFactorModel'
       for setting InternalModelType in modules TargetExposures and HazardCharacterisations -->
  <xsl:template match="ModuleConfigurations/ModuleConfiguration[@module='TargetExposures' or @module='HazardCharacterisations']/Settings/Setting[@id='InternalModelType']">
    <xsl:choose>
      <xsl:when test=". = 'AbsorptionFactorModel'">
        <Setting id="InternalModelType">ConversionFactorModel</Setting>
      </xsl:when>
      <xsl:otherwise>
        <xsl:copy-of select="." />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- Set TargetDoseLevelType in module HazardCharacterisations to 'Systemic'
    when model type AbsorptionFactorModel was selected in HazardCharacterisations AND TargetExposures
    and the level was Internal -->
  <xsl:template
    match="ModuleConfigurations/ModuleConfiguration[@module='HazardCharacterisations']/Settings/Setting[@id='TargetDoseLevelType']">
    <xsl:choose>
      <xsl:when test="($actionType = 'TargetExposures'
             or $actionType = 'Risks'
             and $exposureCalculationMethod = 'ModelledConcentration')
          and $targetExpIntModelType = 'AbsorptionFactorModel'
          and $hazardCharDoseLevelType = 'Internal'">
        <Setting id="TargetDoseLevelType">Systemic</Setting>
      </xsl:when>
      <xsl:otherwise>
        <xsl:copy-of select="." />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
</xsl:stylesheet>
