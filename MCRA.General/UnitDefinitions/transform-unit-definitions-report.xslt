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

  <xsl:template match="UnitDefinition">
    <xsl:param name="level" />
    <xsl:element name="h{$level}">
      <xsl:attribute name="id">
        <xsl:value-of select="Id"/>
      </xsl:attribute>
      <xsl:value-of select="Name"/>
    </xsl:element>
    <xsl:if test="Units">
      <table>
        <thead>
          <tr>
            <th width="100px">Id</th>
            <th width="180px">Name</th>
            <th>Accepted formats</th>
          </tr>
        </thead>
        <tbody>
          <xsl:for-each select="Units/Unit">
            <tr>
              <td>
                <xsl:value-of select="Id"/>
              </td>
              <td>
                <xsl:if test="Name">
                  <xsl:value-of select="Name"/>
                </xsl:if>
                <xsl:if test="not(Name)">
                  <xsl:value-of select="Id"/>
                </xsl:if>
              </td>
              <td>
                <xsl:for-each select="Aliases/Alias">
                  <p>
                    <xsl:value-of select="."/>
                  </p>
                </xsl:for-each>
              </td>
            </tr>
          </xsl:for-each>
        </tbody>
      </table>
    </xsl:if>
  </xsl:template>

  <xsl:template match="/">
    <html>
      <head>
        <title>EuroMix toolbox unit definitions</title>
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
        <xsl:apply-templates select="UnitDefinitions/*">
          <xsl:with-param name="level" select="1"/>
        </xsl:apply-templates>
        <!-- <xsl:apply-templates /> -->
      </body>
    </html>
  </xsl:template>

</xsl:stylesheet>
