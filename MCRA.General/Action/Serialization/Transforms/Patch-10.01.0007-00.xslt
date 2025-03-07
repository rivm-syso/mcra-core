<?xml version="1.0" encoding="utf-8"?>
<!--
Stylesheet for transforms from version 10.1.6
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

  <!--Rename values of ExposureSources -->
  <xsl:template match=
                 "ModuleConfigurations/ModuleConfiguration[@module='TargetExposures']/Settings/Setting[@id='ExposureSources']/ExposureSource
                | ModuleConfigurations/ModuleConfiguration[@module='TargetExposures']/Settings/Setting[@id='IndividualReferenceSet']/ExposureSource
                | ModuleConfigurations/ModuleConfiguration[@module='TargetExposures']/Settings/Setting[@id='ExposureSources']/Value">
    <xsl:copy>
      <xsl:choose>
        <xsl:when test=". = 'DietaryExposures'">Diet</xsl:when>
        <xsl:when test=". = 'DustExposures'">Dust</xsl:when>
        <xsl:when test=". = 'SoilExposures'">Soil</xsl:when>
        <xsl:when test=". = 'OtherNonDietary'">OtherNonDiet</xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="text()" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:copy>
  </xsl:template>

  <!--Rename value of IndividualReferenceSet -->
  <xsl:template match="ModuleConfigurations/ModuleConfiguration[@module='TargetExposures']/Settings/Setting[@id='IndividualReferenceSet']">
    <Setting id="IndividualReferenceSet">
      <xsl:choose>
        <xsl:when test=". = 'DietaryExposures'">Diet</xsl:when>
        <xsl:when test=". = 'DustExposures'">Dust</xsl:when>
        <xsl:when test=". = 'SoilExposures'">Soil</xsl:when>
        <xsl:when test=". = 'OtherNonDietary'">OtherNonDiet</xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="text()" />
        </xsl:otherwise>
      </xsl:choose>
    </Setting>
  </xsl:template>
  
</xsl:stylesheet>
