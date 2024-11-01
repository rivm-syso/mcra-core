<?xml version="1.0" encoding="utf-8"?>
<!--
Stylesheet for transforms from version 10.1.1 to version 10.1.2:
Change UseConsumptions to UseDietaryExposures for DustExposuresIndividualGenerationMethod value.
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

  <!--Rename Consumptions to DietaryExposures for DustExposuresIndividualGenerationMethod value -->
  <xsl:template match="ModuleConfigurations/ModuleConfiguration[@module='DustExposures']/Settings/Setting[@id='DustExposuresIndividualGenerationMethod']">
    <Setting id="DustExposuresIndividualGenerationMethod">
      <xsl:choose>
        <xsl:when test=". = 'UseConsumptions'">UseDietaryExposures</xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="text()" />
        </xsl:otherwise>
      </xsl:choose>
    </Setting>
  </xsl:template>
</xsl:stylesheet>
