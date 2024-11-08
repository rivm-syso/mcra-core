<?xml version="1.0" encoding="utf-8"?>
<!--
Stylesheet for transforms from version 10.0.3 to version 10.0.4 of MCRA
Issues:
A. Rename RiskMetricType values MOE and HI to HazardExposureRatio and ExposureHazardRatio (#1705)

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

  <!--Rename values of RiskMetricType -->
  <xsl:template match="InternalConcentrationType">
    <xsl:element name="ExposureCalculationMethod">
      <xsl:apply-templates select="@*|node()"/>
    </xsl:element>
  </xsl:template>

</xsl:stylesheet>
