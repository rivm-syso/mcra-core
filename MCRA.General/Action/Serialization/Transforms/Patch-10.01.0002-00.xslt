<?xml version="1.0" encoding="utf-8"?>
<!--
Stylesheet for transforms from version 10.1.1 to version 10.1.2 of MCRA Issues:
Rename setting HbmTargetSurfaceLevel from module HumanMonitoringAnalysis to TargetDoseLevelType
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

  <xsl:variable name="ActionType" select="/Project/ActionType" />
  <xsl:variable name="hbmTargetDoseLevel" select="/Project/ModuleConfigurations/ModuleConfiguration[@module='HumanMonitoringAnalysis']/Settings/Setting[@id='HbmTargetSurfaceLevel']" />

  <!-- remove settings HbmTargetSurfaceLevel from module settings -->
  <xsl:template match="ModuleConfigurations/ModuleConfiguration[@module='HumanMonitoringAnalysis']/Settings/Setting[@id='HbmTargetSurfaceLevel']" />

  <!-- Set TargetDoseLevelType in module HazardCharacterisations to the value of HumanMonitoringAnalysis.HbmTargetSurfaceLevel
    when the ActionType is HumanMonitoringAnalysis only -->
  <xsl:template
    match="ModuleConfigurations/ModuleConfiguration[@module='HazardCharacterisations']/Settings/Setting[@id='TargetDoseLevelType']">
    <xsl:choose>
      <xsl:when test="$ActionType = 'HumanMonitoringAnalysis'">
        <Setting id="TargetDoseLevelType">
          <xsl:value-of select="$hbmTargetDoseLevel"/>
        </Setting>
      </xsl:when>
      <xsl:otherwise>
        <xsl:copy-of select="." />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- Add TargetDoseLevelType in module HazardCharacterisations with the value of HumanMonitoringAnalysis.HbmTargetSurfaceLevel
    when the ActionType is HumanMonitoringAnalysis and HumanMonitoringAnalysis settings exists -->
  <xsl:template match="ModuleConfigurations">
    <xsl:copy>
      <xsl:apply-templates select="@*|node()"/>
      <!-- Add module settings for HazardCharacterisations only in the following case -->
      <xsl:if test="$ActionType = 'HumanMonitoringAnalysis'
           and (ModuleConfiguration[@module='HumanMonitoringAnalysis'])
           and not (ModuleConfiguration[@module='HazardCharacterisations'])">
        <ModuleConfiguration module="HazardCharacterisations">
          <Settings>
            <Setting id="TargetDoseLevelType">
              <xsl:value-of select="$hbmTargetDoseLevel"/>
            </Setting>
          </Settings>
        </ModuleConfiguration>
      </xsl:if>
    </xsl:copy>
  </xsl:template>
</xsl:stylesheet>
