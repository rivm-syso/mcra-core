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

  <xsl:template match="CodeModel[.='CosmosV4']">
    <CodeModel>EuroMix_Generic_PBTK_model_V5</CodeModel>
  </xsl:template>
  <xsl:template match="CodeModel[.='CosmosV6']">
    <CodeModel>EuroMix_Generic_PBTK_model_V6</CodeModel>
  </xsl:template>
  <xsl:template match="CodeModel[.='PBPKModel_BPA']">
    <CodeModel>EuroMix_Bisphenols_PBPK_model_V1</CodeModel>
  </xsl:template>
  <xsl:template match="CodeModel[.='PBPKModel_BPA_Reimplementation']">
    <CodeModel>EuroMix_Bisphenols_PBPK_model_V2</CodeModel>
  </xsl:template>

</xsl:stylesheet>
