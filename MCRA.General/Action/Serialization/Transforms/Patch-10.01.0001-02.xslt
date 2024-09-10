<?xml version="1.0" encoding="utf-8"?>
<!--
Stylesheet for transforms from version 10.0.3 to version 10.0.4 of MCRA
Issues: 
Implementation of KineticConversionFactors from KineticModels

-->
<xsl:stylesheet version="2.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">
  <xsl:output method="xml" indent="yes"/>

  <!-- copy all nodes and attributes, applying the templates hereafter -->
  <xsl:template match="@*|node()">
    <xsl:copy>
      <xsl:apply-templates select="@*|node()"/>
    </xsl:copy>
  </xsl:template>

  <!-- remove settings item from module settings -->
  <xsl:template match="ModuleConfigurations/ModuleConfiguration[@module='PbkModels']/Settings/Setting[
    @id='CodeKineticModel']" />

  <!-- Copy InternalConcentrationType from MixtureSelectionSettings to AssessmentSettings-->
  <xsl:template match="ModuleConfigurations/ModuleConfiguration[@module='HazardCharacterisations']/Settings">
    <xsl:copy>
      <!-- Copy all existing data; this and child nodes -->
      <xsl:apply-templates select="@*|node()"/>
      <xsl:if test="not(Setting[@id='ApplyKineticConversions'])">
        <Setting id="ApplyKineticConversions">
          <xsl:choose>
            <xsl:when test="Setting[@id='TargetDosesCalculationMethod'] = 'CombineInVivoPodInVitroDrms'">true</xsl:when>
            <xsl:when test="Setting[@id='TargetDosesCalculationMethod'] = 'InVitroBmds'">true</xsl:when>
            <xsl:when test="Setting[@id='TargetDosesCalculationMethod'] = 'InVivoPods'
                      and Setting[@id='TargetDoseLevelType'] = 'Internal'">true</xsl:when>
            <xsl:otherwise>false</xsl:otherwise>
          </xsl:choose>
        </Setting>
      </xsl:if>
    </xsl:copy>
  </xsl:template>

  <!-- remove settings item from module settings -->
  <xsl:template match="ModuleConfigurations/ModuleConfiguration[@module='HazardCharacterisations']/Settings/Setting[
    @id='UseDoseResponseModels']" />

</xsl:stylesheet>
