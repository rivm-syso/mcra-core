<?xml version="1.0" encoding="utf-8"?>
<!--
Stylesheet for transforms from version 10.3.2
- Rename values of enum ExposureSources
-->
<xsl:stylesheet version="2.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">
  <xsl:output method="xml" indent="yes" />

  <!-- Copy all nodes and attributes, applying the templates hereafter -->
  <xsl:template match="@*|node()">
    <xsl:copy>
      <xsl:apply-templates select="@*|node()" />
    </xsl:copy>
  </xsl:template>

  <!--Rename values of WithinBinExposureRepresentationMethod -->
  <xsl:template match="ModuleConfigurations/ModuleConfiguration[@module='EnvironmentalBurdenOfDisease']/Settings/Setting[@id='WithinBinExposureRepresentationMethod']">
    <Setting id="WithinBinExposureRepresentationMethod">
      <xsl:choose>
        <xsl:when test=". = 'MinimumInBin'">BinLowerLimit</xsl:when>
        <xsl:when test=". = 'MaximumInBin'">BinUpperLimit</xsl:when>
        <xsl:when test=". = 'AverageInBin'">BinMidpoint</xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="text()" />
        </xsl:otherwise>
      </xsl:choose>
    </Setting>
  </xsl:template>

</xsl:stylesheet>
