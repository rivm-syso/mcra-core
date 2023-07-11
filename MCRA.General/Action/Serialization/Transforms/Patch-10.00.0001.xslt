<?xml version="1.0" encoding="utf-8"?>
<!--
Stylesheet for transforms from 10.0.0 to version 10.0.1 of MCRA
Issue: Create separate matrix selection settings for kinetic models and HBM analysis (#1684)
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

  <xsl:variable name="codeCompartment" select="/Project/KineticModelSettings/CodeCompartment" />

  <xsl:template match="/Project/HumanMonitoringSettings">
    <xsl:copy>
      <!-- Copy all existing data; this and child nodes -->
      <xsl:apply-templates select="@*|node()"/>
      <xsl:if test="$codeCompartment != ''">
        <xsl:element name="HbmTargetMatrix">
          <xsl:value-of select="$codeCompartment"/>
        </xsl:element>
      </xsl:if>
    </xsl:copy>
  </xsl:template>

</xsl:stylesheet>
