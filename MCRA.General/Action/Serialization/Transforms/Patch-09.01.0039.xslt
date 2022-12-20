<?xml version="1.0" encoding="utf-8"?>
<!--
Stylesheet for transforms from 9.1.36 to version 9.1.39 of MCRA
Issue: Rename boolean properties of settings classes which have redundant "Is" prefix.
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

  <xsl:variable name="isMatchIndividualsWithPopulation" select="/Project/SubsetSettings/MatchIndividualsWithPopulation = 'true'" />

  <xsl:template match="MatchIndividualsWithPopulation">
    <xsl:element name="MatchIndividualSubsetWithPopulation">
      <xsl:choose>
        <xsl:when test="$isMatchIndividualsWithPopulation">MatchToPopulationDefinition</xsl:when>
        <xsl:otherwise>IgnorePopulationDefinition</xsl:otherwise>
      </xsl:choose>
    </xsl:element>
  </xsl:template>

  <xsl:template match="MatchIndividualDaysWithPopulation">
    <!-- Just remove -->
  </xsl:template>

  <xsl:template match="IndividualDaySubsetSelection">
    <!-- Just remove -->
  </xsl:template>

</xsl:stylesheet>
