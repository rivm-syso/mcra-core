<?xml version="1.0" encoding="utf-8"?>
<!--
Stylesheet for transforms to version 9.2.4 and later of MCRA
Issue: Move InternalConcentrationType from MixtureSelectionSettings to AssessmentSettings.
-->
<xsl:stylesheet version="2.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl"
>
  <xsl:output method="xml" indent="yes"/>

  <!-- copy all nodes and attributes, applying the template changes hereafter -->
  <xsl:template match="@*|node()">
    <xsl:copy>
      <xsl:apply-templates select="@*|node()"/>
    </xsl:copy>
  </xsl:template>

  <!-- Ignore/remove the InternalConcentrationType from the old position at MixtureSelectionSettings -->
  <xsl:template match="/Project/MixtureSelectionSettings/InternalConcentrationType" />

  <!-- Copy InternalConcentrationType from MixtureSelectionSettings to AssessmentSettings-->
  <xsl:template match="/Project/AssessmentSettings">
    <xsl:copy>
      <!-- Copy all existing data; this and child nodes -->
      <xsl:apply-templates select="@*|node()"/>
      <xsl:copy-of select="/Project/MixtureSelectionSettings/InternalConcentrationType"/>
    </xsl:copy>
  </xsl:template>

</xsl:stylesheet>
