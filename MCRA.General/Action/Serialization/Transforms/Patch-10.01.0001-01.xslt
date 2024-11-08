<?xml version="1.0" encoding="utf-8"?>
<!--
Stylesheet for transforms from version 10.0.3 to version 10.0.4 of MCRA
Issues:
Implementation of KineticConversionFactors from KineticModels

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

  <!-- remove settings item from module settings -->
  <xsl:template match="ModuleConfigurations/ModuleConfiguration[@module='KineticModels']/Settings/Setting[
    @id='KCFSubgroupDependent']" />

  <xsl:template match="ModuleConfigurations">
    <xsl:copy>
      <xsl:apply-templates select="@*|node()"/>

      <!--Move values from KineticModels to KineticConversionFactors module only if module KineticModels exists -->
      <xsl:if test="ModuleConfiguration[@module='KineticModels'] and not (ModuleConfiguration[@module='KineticConversionFactors'])">
        <ModuleConfiguration module="KineticConversionFactors">
          <Settings>
            <xsl:copy-of select="ModuleConfiguration[@module='KineticModels']/Settings/Setting[
              @id='KCFSubgroupDependent']" />
            <!-- Check if the ResampleKineticModelParameters exists in KineticModels settings -->
            <xsl:if test="ModuleConfiguration[@module='KineticModels']/Settings/Setting[@id='ResampleKineticModelParameters']">
              <Setting id="ResampleKineticConversionFactors">
                <xsl:value-of select="ModuleConfiguration[@module='KineticModels']/Settings/Setting[@id='ResampleKineticModelParameters']" />
              </Setting>
            </xsl:if>
          </Settings>
        </ModuleConfiguration>
      </xsl:if>
    </xsl:copy>
  </xsl:template>
</xsl:stylesheet>
