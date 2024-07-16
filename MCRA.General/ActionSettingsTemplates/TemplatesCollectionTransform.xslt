<?xml version="1.0"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

    <xsl:output method="xml" indent="yes" />
    <xsl:key name="module" match="ModuleConfiguration" use="@module" />
    <!-- Create a new root element TemplateCollection to hold all templates per module
         This creates an inverted structure where the templates are grouped per module
         using Muenchian grouping for xslt version 1.0
    -->
    <xsl:template match="/">
        <TemplatesCollection>
          <xsl:apply-templates select="//ModuleConfiguration[generate-id(.)=generate-id(key('module',@module)[1])]"/>
        </TemplatesCollection>
    </xsl:template>

    <xsl:template match="ModuleConfiguration">
      <SettingsTemplates actionType="{@module}">
        <xsl:for-each select="key('module', @module)">
          <xsl:element name="SettingsTemplate">
            <xsl:if test="ancestor::SettingsTemplate/@deprecated">
              <xsl:attribute name="deprecated"><xsl:value-of select="ancestor::SettingsTemplate/@deprecated" /></xsl:attribute>
            </xsl:if>
            <ActionType><xsl:value-of select="@module" /></ActionType>
            <Id><xsl:value-of select="ancestor::SettingsTemplate/Id" /></Id>
            <Tier><xsl:value-of select="ancestor::SettingsTemplate/Tier" /></Tier>
            <Name><xsl:value-of select="ancestor::SettingsTemplate/Name" /></Name>
            <ShortName><xsl:value-of select="ancestor::SettingsTemplate/ShortName" /></ShortName>
            <xsl:if test="ancestor::SettingsTemplate/Description">
            <Description><xsl:value-of select="ancestor::SettingsTemplate/Description" /></Description>
            </xsl:if>
            <xsl:copy-of select="Settings" />
          </xsl:element>
        </xsl:for-each>
      </SettingsTemplates>
    </xsl:template>
</xsl:stylesheet>
