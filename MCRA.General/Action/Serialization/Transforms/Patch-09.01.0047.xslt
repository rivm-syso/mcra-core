<?xml version="1.0" encoding="utf-8"?>
<!--
Stylesheet for transforms from 9.1.39 to version 9.1.46 of MCRA
Issue: Change kinetic model codes.
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

  <!-- Set InternalModelType setting (if not already specified) -->
  <xsl:template match="/Project/KineticModelSettings">
    <!-- Copy the whole node -->
    <xsl:copy>
      <!-- Copy all existing data; this and child nodes -->
      <xsl:apply-templates select="@*|node()"/>
      <!-- Check whether property InternalModelType does not exists -->
      <xsl:if test="not(InternalModelType)">
        <xsl:element name="InternalModelType">
          <xsl:choose>
            <xsl:when test="UseKineticModel = 'true'">PBKModel</xsl:when>
            <xsl:otherwise>AbsorptionFactorModel</xsl:otherwise>
          </xsl:choose>
        </xsl:element>
      </xsl:if>
    </xsl:copy>
  </xsl:template>

  <!--Remove UseKineticModel element from XML-->
  <xsl:template match="/Project/KineticModelSettings/UseKineticModel" />

</xsl:stylesheet>
