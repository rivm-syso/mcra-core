<?xml version="1.0" encoding="utf-8"?>
<!--
Stylesheet for transforms to version 9.2.7 and later of MCRA
Issue: old actions implicitly used specific gravity as the urine standardisation method. In this transform, this setting
       is explicitly specified in the xml file as part of the human monitoring settings.
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

  <!-- Add StandardiseUrine settings, with specific gravity applied -->
  <xsl:template match="/Project/HumanMonitoringSettings">
    <!-- Copy the whole node -->
    <xsl:copy>
      <!-- Copy all existing data; this and child nodes -->
      <xsl:apply-templates select="@*|node()"/>
      <!-- Add new elements -->
      <xsl:element name="StandardiseUrine">true</xsl:element>
      <xsl:element name="StandardiseUrineMethod">SpecificGravity</xsl:element>
    </xsl:copy>
  </xsl:template>

  <!-- Add  bool IsMcrAnalysis = true and  ExposureApproachType = McrExposureApproachType.RiskBased settings -->
  <xsl:template match="/Project/MixtureSelectionSettings">
    <!-- Copy the whole node -->
    <xsl:copy>
      <!-- Copy all existing data; this and child nodes -->
      <xsl:apply-templates select="@*|node()"/>
      <!-- Add new elements -->
      <xsl:element name="IsMcrAnalysis">true</xsl:element>
      <xsl:element name="McrExposureApproachType">RiskBased</xsl:element>
    </xsl:copy>
  </xsl:template>

</xsl:stylesheet>
