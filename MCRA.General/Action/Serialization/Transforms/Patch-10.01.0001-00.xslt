<?xml version="1.0" encoding="utf-8"?>
<!--
Stylesheet for transforms from version 10.0.3 to version 10.0.4 of MCRA
Issues: 
A. Rename RiskMetricType values MOE and HI to HazardExposureRatio and ExposureHazardRatio (#1705)

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
    @id='CodeKineticModel' or
    @id='InternalModelType' or
    @id='NonStationaryPeriod' or
    @id='NumberOfDays' or
    @id='NumberOfDosesPerDay' or
    @id='NumberOfDosesPerDayNonDietaryDermal' or
    @id='NumberOfDosesPerDayNonDietaryInhalation' or
    @id='NumberOfDosesPerDayNonDietaryOral' or
    @id='SelectedEvents' or
    @id='SpecifyEvents' or
    @id='UseParameterVariability']" />

  <xsl:template match="ModuleConfigurations">
    <xsl:copy>
      <xsl:apply-templates select="@*|node()"/>

      <!--Move values from KineticModels to PbkModels module only if module KineticModels exists -->
      <xsl:if test="ModuleConfiguration[@module='KineticModels'] and not (ModuleConfiguration[@module='PbkModels'])">
        <ModuleConfiguration module="PbkModels">
          <Settings>
            <xsl:copy-of select="ModuleConfiguration[@module='KineticModels']/Settings/Setting[
              @id='CodeKineticModel' or
              @id='InternalModelType' or
              @id='NonStationaryPeriod' or
              @id='NumberOfDays' or
              @id='NumberOfDosesPerDay' or
              @id='NumberOfDosesPerDayNonDietaryDermal' or
              @id='NumberOfDosesPerDayNonDietaryInhalation' or
              @id='NumberOfDosesPerDayNonDietaryOral' or
              @id='SelectedEvents' or
              @id='SpecifyEvents' or
              @id='UseParameterVariability']" />
            <!-- Check if the ResampleKineticModelParameters exists in KineticModels settings -->
            <xsl:if test="ModuleConfiguration[@module='KineticModels']/Settings/Setting[@id='ResampleKineticModelParameters']">
              <Setting id="ResamplePbkModelParameters">
                <xsl:value-of select="ModuleConfiguration[@module='KineticModels']/Settings/Setting[@id='ResampleKineticModelParameters']" />
              </Setting>
            </xsl:if>
          </Settings>
        </ModuleConfiguration>
        <xsl:if test ="ModuleConfiguration[@module='KineticModels']/Settings/Setting[@id='InternalModelType']">
          <xsl:if test="not (ModuleConfiguration[@module='TargetExposures'])">
            <ModuleConfiguration module="TargetExposures">
              <Settings>
                <Setting id="InternalModelType">
                  <xsl:value-of select="ModuleConfiguration[@module='KineticModels']/Settings/Setting[@id='InternalModelType']" />
                </Setting>
              </Settings>
            </ModuleConfiguration>
          </xsl:if>
          <xsl:if test="not (ModuleConfiguration[@module='HazardCharacterisations'])">
            <ModuleConfiguration module="HazardCharacterisations">
              <Settings>
                <Setting id="InternalModelType">
                  <xsl:value-of select="ModuleConfiguration[@module='KineticModels']/Settings/Setting[@id='InternalModelType']" />
                </Setting>
              </Settings>
            </ModuleConfiguration>
          </xsl:if>
        </xsl:if>
      </xsl:if>
    </xsl:copy>
  </xsl:template>
  <!-- when module configuration for TargetExposures or HazardCharacterisations exists, add the InternalModelType setting from KineticModels -->
  <xsl:template match="ModuleConfigurations/ModuleConfiguration[@module='TargetExposures' or @module='HazardCharacterisations']/Settings">
    <xsl:copy>
      <xsl:apply-templates select="@*|node()"/>
      <!-- (try to) copy the setting from KineticModels -->
      <xsl:copy-of select="../../ModuleConfiguration[@module='KineticModels']/Settings/Setting[@id='InternalModelType']" />
    </xsl:copy>
  </xsl:template>
</xsl:stylesheet>
