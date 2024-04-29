<?xml version="1.0" encoding="utf-8"?>
<!--
Stylesheet for transforms from version 10.0.11 to version 10.0.12 of MCRA
Issues: 
A. Add new setting ApplyKineticConversions when in projects the convert to single target was selected (#1827)

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

  <xsl:variable name="isConvertToSingleTarget" select="/Project/HumanMonitoringSettings/HbmConvertToSingleTargetMatrix = 'true'" />
  <xsl:template match="/Project/HumanMonitoringSettings">
    <!-- Copy the whole node -->
    <xsl:copy>
      <!-- Copy all existing data; this and child nodes -->
      <xsl:apply-templates select="@*|node()"/>
      <!-- Add new element, if convert to single target is selected -->
      <xsl:if test="$isConvertToSingleTarget">
        <xsl:element name="ApplyKineticConversions">true</xsl:element>
      </xsl:if>
    </xsl:copy>
  </xsl:template>

  <xsl:variable name="codeCompartment" select="/Project/KineticModelSettings/CodeCompartment != ''" />
  <xsl:template match="/Project/KineticModelSettings/CodeCompartment">
    <!-- If code compartment not empty -->
    <xsl:if test="$codeCompartment">
      <!-- Create list node and fill with single element -->
      <xsl:element name="CompartmentCodes">
        <xsl:element name="string">
          <xsl:apply-templates select="@*|node()"/>
        </xsl:element>
      </xsl:element>
    </xsl:if>
  </xsl:template>

</xsl:stylesheet>
