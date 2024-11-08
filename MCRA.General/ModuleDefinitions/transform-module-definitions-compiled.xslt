<?xml version="1.0"?>

<xsl:stylesheet version="2.0"
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:xi="http://www.w3.org/2001/XInclude">

  <xsl:template match="xi:include[@href][@parse='xml' or not(@parse)]">
    <xsl:apply-templates select="document(@href)" />
  </xsl:template>

  <xsl:template match="@* | node()" priority="-1">
    <xsl:copy copy-namespaces="no">
      <xsl:apply-templates select="@* | node()"  />
    </xsl:copy>
  </xsl:template>

</xsl:stylesheet>
