<?xml version="1.0" encoding="utf-8"?>
<!--
Stylesheet for transforms from version 10.1.4 to version 10.1.5 of MCRA
Issues:
Add ExposureEventsGenerationMethod with value that differs from default for older projects
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

  <xsl:template match="ModuleConfigurations/ModuleConfiguration[@module='PbkModels']/Settings">
    <xsl:copy>
      <!-- Copy all existing data; this and child nodes -->
      <xsl:apply-templates select="@*|node()"/>
      <!-- Add ExposureEventsGenerationMethod setting if not already there -->
      <xsl:if test="not(Setting[@id='ExposureEventsGenerationMethod'])">
        <Setting id="ExposureEventsGenerationMethod">RandomDailyEvents</Setting>
      </xsl:if>
    </xsl:copy>
  </xsl:template>
</xsl:stylesheet>
