<?xml version="1.0" encoding="utf-8"?>
<!--
Stylesheet for transforms to version 9.2.6 and later of MCRA
Issue: new setting introduced, CumulativeRisk, set to true. Is replacing old setting 'IsCumulative'
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

  <!-- Add CumulativeRisk setting to EffectModelSettings -->
  <xsl:template match="/Project/EffectModelSettings">
    <!-- Copy the whole node -->
    <xsl:copy>
      <!-- Copy all existing data; this and child nodes -->
      <xsl:apply-templates select="@*|node()"/>
      <!-- Add new elements -->
      <xsl:element name="CumulativeRisk">true</xsl:element>
    </xsl:copy>
  </xsl:template>

</xsl:stylesheet>
