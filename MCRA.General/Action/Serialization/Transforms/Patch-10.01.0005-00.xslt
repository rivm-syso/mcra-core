<?xml version="1.0" encoding="utf-8"?>
<!--
Stylesheet for transforms from version 10.1.4 to version 10.1.5 of MCRA
Issues:
A. Refactor: make 'IsCompute' an attribute of module configuration (#xxxx)
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

  <!-- remove CalculationActionTypes item from module settings -->
  <xsl:template match="CalculationActionTypes" />
  <!-- create a key list of calculation module action types -->
  <xsl:key name="computeModules" match="CalculationActionTypes" use="ActionType" />
  <!-- create a key list of existing module configurations in the config -->
  <xsl:key name="moduleConfigs" match="ModuleConfiguration" use="@module" />

  <!-- copy ModuleConfigurations -->
  <xsl:template match="ModuleConfiguration">
    <xsl:copy>
      <!-- add compute='true' where module id exists in the defined key list -->
      <xsl:if test="key('computeModules', @module)">
        <xsl:attribute name="compute">true</xsl:attribute>
      </xsl:if>
      <xsl:apply-templates select="@*|node()"/>
    </xsl:copy>
  </xsl:template>

  <xsl:template match="ModuleConfigurations">
    <xsl:copy>
      <xsl:apply-templates select="@*|node()"/>
      <!-- Add module configurations for CalculationActionTypes that don't
        have a configuration block in the ModuleConfigurations collection -->
      <xsl:for-each select="/Project/CalculationActionTypes/ActionType">
        <xsl:if test="not(key('moduleConfigs', .))">
          <xsl:element name="ModuleConfiguration">
            <xsl:attribute name="module">
              <xsl:value-of select="."/>
            </xsl:attribute>
            <xsl:attribute name="compute">true</xsl:attribute>
          </xsl:element>
        </xsl:if>
      </xsl:for-each>
    </xsl:copy>
  </xsl:template>
</xsl:stylesheet>
