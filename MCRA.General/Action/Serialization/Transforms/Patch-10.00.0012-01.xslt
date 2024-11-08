<?xml version="1.0" encoding="utf-8"?>
<!--
Stylesheet for transforms from version 10.0.11 to version 10.0.12 of MCRA
Issues:
A. Refactor MCR options: IsMcrAnalysis and McrExposureApproachType
Dietary, for HumanMonitoring, for Risks, for TargetExposures
Remove from MixtureSelectionSettings

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

  <!-- Remove obsolete MixtureSelectionSettings.McrExposureApproachType and IsMcrAnalysis elements -->
  <xsl:template match="/Project/MixtureSelectionSettings/McrExposureApproachType" />
  <xsl:template match="/Project/MixtureSelectionSettings/IsMcrAnalysis" />

  <!-- Get values for MixtureSelectionSettings -->
  <xsl:variable name="exposureApprTypeValue">
    <xsl:choose>
      <xsl:when test="not(/Project/MixtureSelectionSettings/McrExposureApproachType)">RiskBased</xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="/Project/MixtureSelectionSettings/McrExposureApproachType" />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <xsl:variable name="analyseMcrValue">
     <xsl:value-of select="/Project/MixtureSelectionSettings/IsMcrAnalysis = 'true'" />
  </xsl:variable>

  <xsl:template match="/Project">
    <xsl:copy>
      <!-- Copy all existing data; this and child nodes -->
      <xsl:apply-templates select="@*|node()"/>
      <xsl:if test="not(DietaryIntakeCalculationSettings)">
        <xsl:element name="DietaryIntakeCalculationSettings">
          <xsl:call-template name="AddMcrElementValues" />
        </xsl:element>
      </xsl:if>
      <xsl:if test="not(EffectSettings)">
        <xsl:element name="EffectSettings">
          <xsl:call-template name="AddMcrElementValues" />
        </xsl:element>
      </xsl:if>
      <xsl:if test="not(HumanMonitoringSettings)">
        <xsl:element name="HumanMonitoringSettings">
          <xsl:call-template name="AddMcrElementValues" />
        </xsl:element>
      </xsl:if>
      <xsl:if test="not(RisksSettings)">
        <xsl:element name="RisksSettings">
          <xsl:call-template name="AddMcrElementValues" />
        </xsl:element>
      </xsl:if>
    </xsl:copy>
  </xsl:template>

  <xsl:template match="/Project/DietaryIntakeCalculationSettings">
    <xsl:copy>
      <xsl:apply-templates select="@*|node()"/>
      <xsl:call-template name="AddMcrElementValues" />
    </xsl:copy>
  </xsl:template>

  <xsl:template match="/Project/RisksSettings">
    <xsl:copy>
      <xsl:apply-templates select="@*|node()"/>
      <xsl:call-template name="AddMcrElementValues" />
    </xsl:copy>
  </xsl:template>

  <xsl:template match="/Project/EffectSettings">
    <xsl:copy>
      <xsl:apply-templates select="@*|node()"/>
      <xsl:call-template name="AddMcrElementValues" />
    </xsl:copy>
  </xsl:template>

  <xsl:template match="/Project/HumanMonitoringSettings">
    <xsl:copy>
      <xsl:apply-templates select="@*|node()"/>
      <xsl:call-template name="AddMcrElementValues" />
    </xsl:copy>
  </xsl:template>

  <!-- Creates 'AnalyseMcr' and 'ExposureApproachType' elements -->
  <xsl:template name="AddMcrElementValues">
    <xsl:element name="AnalyseMcr">
      <xsl:value-of select="$analyseMcrValue"/>
    </xsl:element>
    <xsl:element name="ExposureApproachType">
      <xsl:value-of select="$exposureApprTypeValue"/>
    </xsl:element>
  </xsl:template>
</xsl:stylesheet>
