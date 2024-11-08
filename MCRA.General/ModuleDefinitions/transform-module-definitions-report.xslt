<?xml version="1.0"?>
<xsl:stylesheet version="2.0"
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:xi="http://www.w3.org/2001/XInclude">

<xsl:template match="xi:include[@href][@parse='xml' or not(@parse)]">
  <xsl:param name="level" />
  <xsl:apply-templates select="document(@href)/*">
    <xsl:with-param name="level" select="$level"/>
  </xsl:apply-templates>
</xsl:template>

<xsl:template match="ModuleGroupDefinition[not(@Deprecated)]">
  <xsl:param name="level" />
  <xsl:element name="h{$level}">
    <xsl:attribute name="id">
      <xsl:value-of select="Id"/>
    </xsl:attribute>
    <xsl:value-of select="Name"/>
  </xsl:element>
  <xsl:if test="Description">
    <p>
      <xsl:value-of select="Description"/>
    </p>
  </xsl:if>
  <xsl:apply-templates select="Modules">
      <xsl:with-param name="level" select="$level + 1"/>
  </xsl:apply-templates>
</xsl:template>

<xsl:template match="Module[not(@Deprecated)]">
  <xsl:param name="level" />
  <xsl:param name="settings" />
  <xsl:element name="h{$level}">
    <xsl:attribute name="id">
      <xsl:value-of select="Id"/>
    </xsl:attribute>
    <xsl:value-of select="Name"/>
  </xsl:element>
  <p>
    Module type: <xsl:value-of select="ModuleType"/>
  </p>
  <xsl:if test="Description">
    <p>
      <xsl:value-of select="Description"/>
    </p>
  </xsl:if>
  <xsl:if test="SelectionInputs">
    <table>
      <tbody>
        <tr>
          <th colspan="2">Calculator input data</th>
        </tr>
        <xsl:apply-templates select="SelectionInputs" />
      </tbody>
    </table>
  </xsl:if>
  <xsl:if test="CalculationDescription">
    <xsl:element name="h{$level}">
      Calculation
    </xsl:element>
    <p>
      <xsl:value-of select="CalculationDescription"/>
    </p>
    <xsl:if test="CalculationInputs">
      <table>
        <tbody>
          <tr>
            <th colspan="2">Calculator input data</th>
          </tr>
          <xsl:apply-templates select="CalculationInputs" />
        </tbody>
      </table>
    </xsl:if>
  </xsl:if>
</xsl:template>

<xsl:template match="Entities">
  <xsl:for-each select="Entity[not(@Deprecated)]">
    <tr>
      <td width="190px">
        <xsl:value-of select="."/>
      </td>
      <td>
      </td>
    </tr>
  </xsl:for-each>
</xsl:template>

<xsl:template match="Tiers">
  <xsl:for-each select="Tier[not(@Deprecated)]">
    <tr>
      <td width="190px">
        <xsl:value-of select="."/>
      </td>
      <td>
      </td>
    </tr>
  </xsl:for-each>
</xsl:template>

  <xsl:template match="CalculationInputs">
  <xsl:for-each select="Input[not(@Deprecated)]">
    <tr>
      <td width="190px">
          <xsl:value-of select="."/>
      </td>
      <td>
      </td>
    </tr>
  </xsl:for-each>
</xsl:template>

<xsl:template match="SelectionInputs">
  <xsl:for-each select="Input[not(@Deprecated)]">
    <tr>
      <td width="190px">
        <xsl:value-of select="."/>
      </td>
      <td>
      </td>
    </tr>
  </xsl:for-each>
</xsl:template>

<xsl:template match="/">
  <html>
    <head>
      <title>EuroMix toolbox modules</title>
      <meta charset="UTF-8" />
      <meta name="viewport" content="width=device-width, initial-scale=1" />
      <meta http-equiv="Cache-Control" content="no-cache, no-store, must-revalidate" />
      <meta http-equiv="Pragma" content="no-cache" />
      <meta http-equiv="Expires" content="0" />
      <style>
        body
        {
            font-size: 10pt;
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
        }

        h1
        {
            font-size: 12pt;
        }

        h2
        {
            font-size: 12pt;
        }

        h3, h4, h5, h6
        {
            font-size: 12pt;
        }

        h1, h2, h3, h4, h5, h6
        {
            margin-bottom: 4px;
        }

        a {
            text-decoration: none;
            color: #000;
        }

        p
        {
            margin: 2px 0;
        }

        .center {
            text-align: center;
        }

        pre {
            margin: 2px 0;
            padding: 5px;
            background: #eee;
            border: #ddd;
            white-space: -moz-pre-wrap;
            white-space: -o-pre-wrap;
            word-wrap: break-word;
        }

        img {
            display: block;
            max-width: 100%;
        }

        figcaption {
            font-weight: bold;
            text-align: center;
        }

        ul {
          list-style-position: outside;
          padding-left: 20px;
          margin: 0;
        }

        table {
            align-self: center;
            font-size: 9pt;
            vertical-align: top;
            border-collapse: collapse;
            margin: 4px 0 8px 0;
            width: 100%;
        }

        table, tr, td, th, tbody, thead, tfoot {
          page-break-inside: avoid !important;
        }

        caption {
            display: table-caption;
            font-weight: bold;
            align-content: center;
            margin-bottom: 3px;
        }

        table td, table th {
            border: #bbb 1px solid;
            padding: 0px 8px 0px 4px;
            vertical-align: top;
        }

        table th
        {
            background-color: #eee;
            text-align: left;
            font-weight: bold;
            padding: 3px 8px 3px 4px;
        }

        table td
        {
            border: #bbb 1px solid;
            min-height: 1em;
            max-width: 800px;
        }
      </style>
    </head>
    <body>
      <xsl:apply-templates select="ModuleDefinitions/ModuleGroupDefinition">
        <xsl:with-param name="level" select="1"/>
      </xsl:apply-templates>
    </body>
  </html>
</xsl:template>

</xsl:stylesheet>
