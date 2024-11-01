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

  <xsl:variable name="matchSpecificIndividuals"
    select="/Project/ModuleConfigurations/ModuleConfiguration[@module='NonDietaryExposures']/Settings/Setting[@id='MatchSpecificIndividuals']" />

  <xsl:variable name="isCorrelationBetweenIndividuals"
    select="/Project/ModuleConfigurations/ModuleConfiguration[@module='NonDietaryExposures']/Settings/Setting[@id='IsCorrelationBetweenIndividuals']" />

  <!-- remove setting MatchSpecificIndividuals from module NonDietaryExposures settings -->
  <xsl:template match="ModuleConfigurations/ModuleConfiguration[@module='NonDietaryExposures']/Settings/Setting[@id='MatchSpecificIndividuals']" />

  <xsl:template match="ModuleConfigurations/ModuleConfiguration[@module='TargetExposures']/Settings">
    <xsl:copy>
      <!-- Copy all existing data; this and child nodes -->
      <xsl:apply-templates select="@*|node()"/>
      <!-- Add NonDietaryPopulationAlignmentMethod setting if not already there -->
      <xsl:if test="not(Setting[@id='NonDietaryPopulationAlignmentMethod'])">
        <xsl:choose>
          <xsl:when test="not(Setting[@id='NonDietaryPopulationAlignmentMethod']) and $matchSpecificIndividuals = 'true'">
            <Setting id="NonDietaryPopulationAlignmentMethod">MatchIndividualId</Setting>
          </xsl:when>
          <xsl:otherwise>
            <Setting id="NonDietaryPopulationAlignmentMethod">MatchCofactors</Setting>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:if>
    </xsl:copy>
  </xsl:template>
</xsl:stylesheet>
