<?xml version="1.0" encoding="utf-8"?>
<!--
Stylesheet for transforms to version 9.2.6 and later of MCRA
Issue: new setting introduced, CumulativeRisk, set to true. Is replacing old setting 'IsCumulative'
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

  <!-- Add any new single elements that should be added under /Project here -->
  <xsl:template match="/Project">
    <xsl:copy>
      <!-- Add ScopeKeysFilters Element when it does not exist -->
      <xsl:if test="not(ScopeKeysFilters)">
        <xsl:element name="ScopeKeysFilters">
          <xsl:call-template name="AddNewScopeKeysFiltersTemplate" />
        </xsl:element>
      </xsl:if>
      <xsl:apply-templates select="@*|node()"/>
    </xsl:copy>
  </xsl:template>

  <xsl:template name="AddNewScopeKeysFiltersTemplate">
    <!-- Add a scope keys filter for the selected food survey setting-->
    <xsl:if test="count(/Project/HumanMonitoringSettings/SurveyCodes/string)&gt;0">
      <xsl:call-template name="ScopeKeysFilterTemplate">
        <xsl:with-param name="ScopingType">HumanMonitoringSurveys</xsl:with-param>
        <xsl:with-param name="Codes" select="/Project/HumanMonitoringSettings/SurveyCodes/string" />
      </xsl:call-template>
    </xsl:if>
  </xsl:template>

  <!-- Creates 'string' elements for each item in the input list
       The Codes parameter should contain the text values for this list -->
  <xsl:template name="CodesAsStringsTemplate">
    <xsl:param name="Codes" />
    <xsl:for-each select="$Codes">
      <xsl:element name="string">
        <xsl:value-of select="."/>
      </xsl:element>
    </xsl:for-each>
  </xsl:template>

  <!-- Creates 'ScopeKeysFilter' elements for each item in the input list
       The Codes parameter should contain the text values for this list -->
  <xsl:template name="ScopeKeysFilterTemplate">
    <xsl:param name="ScopingType" />
    <xsl:param name="Codes" />
    <!-- Check that there are no SelectedCodes/string elements for this ScopingType
      This means there is no element at all, or the element exists, but with no SelectedCodes
      Any existing element with no SelectedCodes is removed by another template -->
    <xsl:if test="count(/Project/ScopeKeysFilters/ScopeKeysFilter[ScopingType=$ScopingType]/SelectedCodes/string)=0">
      <xsl:element name="ScopeKeysFilter">
        <xsl:element name="ScopingType">
          <xsl:value-of select="$ScopingType"/>
        </xsl:element>
        <xsl:element name="SelectedCodes">
          <xsl:call-template name="CodesAsStringsTemplate">
            <xsl:with-param name="Codes" select="$Codes" />
          </xsl:call-template>
        </xsl:element>
      </xsl:element>
    </xsl:if>
  </xsl:template>
</xsl:stylesheet>
