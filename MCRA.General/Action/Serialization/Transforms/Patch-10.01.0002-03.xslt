<?xml version="1.0" encoding="utf-8"?>
<!--
Stylesheet for transforms from version 10.1.1 to version 10.1.2 of MCRA Issues:
Replace Aggregate setting with ExposureRoutes setting.
Add ExposureSources and IndividualReferenceSet selection setting to TargetExposures.
-->
<xsl:stylesheet version="2.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">
  <xsl:output method="xml" indent="yes" />

  <!-- copy all nodes and attributes, applying the templates hereafter -->
  <xsl:template match="@*|node()">
    <xsl:copy>
      <xsl:apply-templates select="@*|node()" />
    </xsl:copy>
  </xsl:template>

  <xsl:variable name="aggregate"
    select="/Project/ModuleConfigurations/ModuleConfiguration[@module='HazardCharacterisations']/Settings/Setting[@id='Aggregate']" />

  <!-- remove setting Aggregate from module HazardCharacterisations settings -->
  <xsl:template match="ModuleConfigurations/ModuleConfiguration[@module='HazardCharacterisations']/Settings/Setting[@id='Aggregate']" />

  <!-- Add ExposureRoutes setting to HazardCharacterisations settings -->
  <xsl:template match="ModuleConfigurations/ModuleConfiguration[@module='HazardCharacterisations']/Settings">
    <xsl:copy>
      <!-- Copy all existing data; this and child nodes -->
      <xsl:apply-templates select="@*|node()"/>
      <!-- Add ExposureRoutes setting if not already there -->
      <xsl:if test="not(Setting[@id='ExposureRoutes'])">
        <Setting id="ExposureRoutes">
          <xsl:choose>
            <xsl:when test="$aggregate = 'True'">
              <ExposureRoute>Oral</ExposureRoute>
              <ExposureRoute>Dermal</ExposureRoute>
              <ExposureRoute>Inhalation</ExposureRoute>
            </xsl:when>
            <xsl:otherwise>
              <ExposureRoute>Oral</ExposureRoute>
            </xsl:otherwise>
          </xsl:choose>
        </Setting>
      </xsl:if>
    </xsl:copy>
  </xsl:template>

  <!-- Add ExposureSources and IndividualReferenceSet settings to TargetExposures settings -->
  <xsl:template match="ModuleConfigurations/ModuleConfiguration[@module='TargetExposures']/Settings">
    <xsl:copy>
      <!-- Copy all existing data; this and child nodes -->
      <xsl:apply-templates select="@*|node()"/>
      <!-- Add ExposureSources setting if not already there -->
      <xsl:if test="not(Setting[@id='ExposureSources'])">
        <Setting id="ExposureSources">
          <xsl:choose>
            <xsl:when test="$aggregate = 'True'">
              <ExposureSource>DietaryExposures</ExposureSource>
              <ExposureSource>OtherNonDietary</ExposureSource>
          </xsl:when>
            <xsl:otherwise>
              <ExposureSource>DietaryExposures</ExposureSource>
            </xsl:otherwise>
          </xsl:choose>
        </Setting>
      </xsl:if>
      <!-- Add IndividualReferenceSet setting if not already there -->
      <xsl:if test="not(Setting[@id='IndividualReferenceSet'])">
        <Setting id="IndividualReferenceSet">
          <ExposureSource>DietaryExposures</ExposureSource>
        </Setting>
      </xsl:if>
    </xsl:copy>
  </xsl:template>
</xsl:stylesheet>
