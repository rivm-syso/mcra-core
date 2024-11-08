<?xml version="1.0" encoding="utf-8"?>
<!--
Stylesheet for transforms from version 10.0.3 to version 10.0.4 of MCRA Issues:
Removesetting DeriveFromAbsorptionFactors from module KineticConversionFactors

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
  <xsl:template match="ModuleConfigurations/ModuleConfiguration[@module='KineticConversionFactors']/Settings/Setting[
    @id='DeriveFromAbsorptionFactors']" />

</xsl:stylesheet>
