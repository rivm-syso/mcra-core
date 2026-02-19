<?xml version="1.0" encoding="utf-8"?>
<!--
Stylesheet for transforms from version 10.2.22 to version 10.2.23:
Rename (PBK) setting LifetimeYears for LifetimeEndAge.
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

  <!-- Rename LifetimeYears to LifetimeEndAge -->
  <xsl:template match="Setting[@id='LifetimeYears']">
    <Setting>
      <xsl:copy-of select="@*[name() != 'id']"/>
      <xsl:attribute name="id">LifetimeEndAge</xsl:attribute>
      <xsl:apply-templates select="node()"/>
    </Setting>
  </xsl:template>

  <!--Remove NonStationaryPeriodInYears element from XML-->
  <xsl:template match="/Project/ModuleConfigurations/ModuleConfiguration[@module='PbkModels']/Settings/Setting[@id='NonStationaryPeriodInYears']" />

</xsl:stylesheet>
