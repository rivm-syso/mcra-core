# Change Log

## Version 10.2.10 (2025-12-12)

### Added

- Implement uncertainty in BoD module and EBD calculations (uniform and triangular) (#2339)
- Add population size uncertainty to populations module (#2340)
- Implement ERF uncertainty in EBD calculations (#2240,#2348)
- Dust concentration and dust concentration distributions modules (#2342)

## Changed

- Export frequency and amount model fit summary tables (#2365)
- Refactor BoD indicator distribution models and fix derived BoDs calculation (#2376)

## Version 10.2.9 (2025-12-01)

### Added

- Implement option to compute EBDs for cumulative exposures in EBDs module (#2336)
- Implementation triangular distribution for counterfactualvalue; fix for cfv uncertainty (#2240)
- Individuals as data (#2341)

### Fixed

- Error when reading DateTime value from an (Excel) OLE Automation Date (#2335)
- Display correct EBD standardisation in charts (#2346)

## Version 10.2.8 (2025-11-14)

### Added

- Safety plot in loop risk output of SRA2025 (#2279)
- Add new MCR plot according to Kortenkamp (#2323)
- Violin plots for dietary exposure percentiles of combined loop outputs (#2326)
- Add PBK model diagram

### Changed

- Update SBML file reader and SBML PBK model parsing

### Fixed

- Unclosed tag in SVG output of ReportRChartCreatorBase.ToSvgString using new R svglite package (#2322)
- MCRA.Utils data readers are not disposed properly (#2332)

## Version 10.2.7 (2025-11-02)

### Added

- Add option for defining within bin exposure (#2127)
- Add substances exclude list to active substance filtering, sets membership to 0 (#2254)
- Add initial implementation of dynamic age/BW scaling in lifetime PBK simulations

### Fixed

- Risks module: exposure percentiles visible in nominal, but invisible in uncertainty run (#2290)
- No metadata for individual loop task output reports (#2292)
- Correct loading and usage of substance-generic processing factors (#2293)
- Update concentrations action to also treat substance translations of the focal food/substance combinations as authorised (#2295)

## Version 10.2.6 (2025-10-10)

### Added

- Add samples by food summary section and refactor sample origins section (#2289)

### Changed

- Update modelled foods module with additional output section to summarize modelled foods only (#2289)
- Foods with facets: use name of processing type when (any of) the facet name(s) not available

## Version 10.2.5 (2025-10-03)

### Added

- Add option in PBK model simulation to read-across between arterial/venous blood/plasma and blood/plasma (#2286)
- Add functionality to use surrogate matrix (for target) for PBK models in internal exposures (#2287)

### Fixed

- Update data link check for RPF module to allow missing RPFs for substances (can happen for non-active substances)

## Version 10.2.4 (2025-09-26)

### Added

- Implement HBM single value exposures module and use in EBD calculations (#2242)
- Refactor EBD calculation and add functionality to translate between indicators (DALY <-> Costs) (#2243)
- Implementation of uncertainty of counterfactual value in exposure response functions (#2240)
- Add active substance allocation option in concentrations module to allocate to least potent substance (#2256)
- Add enum for specifying standardised population size in EBD calculations (#2258)

### Fixed

- ConsumerProducts internal exposure fails after filtering for consumer products (#2274)

## Version 10.2.3 (2025-09-05)

### Added

- Refactor reverse dose calculation and implement use of PBKmodels in HBM analysis (#2235)
- Implementation bootstrap concentrations for indoor/outdoor air, dust and soil (#2237)
- Implement support for SSD2 sampAnId and anPortSeq fields in concentrations bulk copier (#2267)

### Changed

- Don't allow uploading an SSD concentrations file with invalid concentration units

### Fixed

- Correct individual subset selection on sex/gender (#2262)
- Tier-defined settings are editable in some front end settings panels (#2266)

## Version 10.2.2 (2025-08-21)

### Added

- Add unit tests for air, dust, soil and consumer products (#2238)
- Add command line option in MCRA ClI to save data files as Excel or CSV(default) (#2246)

### Changed

- Rename 'standardised attributable bod' to 'EBD per 100,000' (#2211)

### Fixed

- MCRA output CSV tables always truncate floating point numbers to 5 significant digits (#1964)
- Loop calculation throws 'Exception has been thrown by the target of an invocation' (#2260)
- Loop calculation fails with 'idSubstance column is required' (#2261)

## Version 10.2.1 (2025-07-25)

### Added

- Implementation of bootstrap of individuals (uncertainty) in Individuals module for air, dust and soil (#2056)
- Add functionality to specify output resolution for PBK model simulations (#2117)
- Implementation ConsumerProductConcentrationDistributions module (#2223)

### Changed

- Remove DistributionType, CvVariability, rename OccurrencePercentage from table ConsumerProductConcentrations (#2229)
- Updated target .NET framework of MCRA Core to .NET 9.0
- Updated R version for MCRA to 4.5.1 (#2234)

### Fixed

- Specified routes in InternalExposures conflict with routes of input modules (#2227)
- Missing ModuleConfig for calculator module without configuration settings (#2228)
- Food hierarchy is not shown in output

## Version 10.2.0 (2025-07-07)

### Added

- Implement ConsumerProducts module (#2148)
- Add biological matrix term for feces (#2205)
- Show uncertainty in exposure response function plot (#2206)

### Changed

- Rename BaselineBodIndicators to BurdensOfDisease (#2209)
- Rename baseline to counterfactualValue in erf data (#2210)

### Fixed

- Correct conversion of erf evaluations (#2208)
- Throw exception with more specific information when no individuals are left after subset selection (#2230)

## Version 10.1.10 (2025-05-08)

### Added

- New setting EbdApproach in ebd module (#2120)
- Implement external exposures, exposures by route toc (#2193)
- New functionality for erfs defined as population shifts (#2194)
- Add info about sampling weights for HBM data (#2195)
- Add Excel as output format for MCRA CLI file conversion (#2200)
- Add uncertainty in erf (#2204)

### Fixed

- Implement correct alignment (to amounts) of nondietary exposures target exposures module (#2197)
- Update individuals module to use population properties when generating individuals (#2202)

## Version 10.1.9 (2025-04-28)

### Added

- New columns for uncertainty in exposure response functions data (#2149)
- Add Standardized and Exposed Attributable plots to Environmental Burden of Disease module (#2171)
- New table population characteristics (#2176)
- New column 'population size' in populations table (#2185)
- New module 'Individuals' (#2188)
- Allow for definition of exposure response functions as shifts (#2191)

### Fixed

- Total attributable BoD does not correspond to total in table when uncertainty is applied, add uncertainy bounds (#2189)

## Version 10.1.8 (2025-04-08)

### Added

- Add bar plot of attributable burden together with cumulative percentage (#2171)
- Implement environmental burden of disease with uncertainty (#2179)
- Implement TOC Exposures by Source and Substance in Internal Exposures module (#2184)

### Changed

- Rename exposure effect functions to exposure response functions (#2174)
- Align non-dietary expore module with other external exposure modules (dust, soil, etc) (#2183)

### Fixed

- Excel data templates table name too long for sheet name (#2134)
- Core and Web have different outputs for AIR (and probably DUST and SOIL) (#2177)
- Align non-dietary expore module with other external exposure modules (dust, soil, etc) (#2183)

## Version 10.1.7 (2025-03-24)

### Added

- New class SimulatedIndividual, refactor controlled Individual properties (#2089)
- Add PBK model simulation settings to internal exposures module (#2118)
- Add setting BoD indicator selection for EBD and allow for multiple exposure effect functions (#2122)
- Add option targetLevel in ebd module (#2123)
- Enable dietary exposures as input in ebd module (#2124)
- Add option for selecting custom bins in ebd module (#2125)
- Add exposure by source section to toc in internal exposure module (#2145)
- Implement toc exposure by source and route for internal exposures module (#2151)
- Implementation RS Expo Air Exposures module (#2167)
- Add separate random generator for individuals in soil exposures module (#2170)

### Changed

- Update HBM data reader to ignore DW, AS, IAIR, and EBC matrices and accept codebooks other than basic codebooks (#2146)

### Fixed

- Add missing units to single value risk tables (#2161)

## Version 10.1.6 (2025-03-04)

### Added

- Add initial version of PBK model definitions details summary sections for SBML models
- Extended exposure effect functions input format with erType and Baseline columns (#2128)
- New data module burden of disease infos (#2129)
- Implement external exposure by source summary section in internal exposures module (#2136)

### Changed

- Remove non-dietary drilldowns (#2111)

### Fixed

- Correct units for external exposures sub-sections in internal exposures module

## Version 10.1.5 (2025-02-10)

### Added

- Update SRA EU prospective dietary CRA (2024) to use the option to apply conversion of processed derivatives (#2105)
- Implement method to run chronic PBK model simulations using single repeated exposures (#2106)
- New module Soil exposures

### Changed

- Refactor 'IsCompute' functionality, move to ModuleConfiguration in project settings XML (#2101)
- All individual drilldowns are custom (html) code and not sortable and downloadable (#2108)

### Fixed

- Add uncertainty bounds to cumulative risk plot (instead of only nominal) (#2104)
- Focal Concentrations sample subset selection is not summarized

## Version 10.1.4 (2025-01-31)

### Added

- Focal food/substance functionality for MRL replacement using a settings input field for the proposed MRL (#2085)
- Implement option 'Restrict LOR imputation to authorised uses' (#2088)
- Filter focal food concentrations/samples based on fieldTrialType (#2094)
- Implement default processing factor for missing processing factors (#2095)
- Add option to replace measurements of processed derivatives of the focal food, including correction for processing (#2102)

### Changed

- Update SSD concentrations bulk copier to parse progSampStrategy and fieldTrialType (#2065)
- Update test projects to fetch settings from appsettings json instead of settings config to enable user-specific settings (#2099)

### Fixed

- Imputed bodyweights are not correctly picked up in dietary exposures calculations (#2054)
- Clicking on empty sub-headers doesn't navigate to anchor in content of download html reports (#2083)
- Concentration Limits table in output shows kg/kg in table header (#2091)

## Version 10.1.3 (2024-12-13)

### Added
- Update dust exposures module to support chronic and acute exposure calculations (#2045)
- Initial implementation of uploadable PBK models via data module (#2061)
- Implement uploading of SBML models (#2072)

### Changed
- Add start and end date of sampling to hbm survey information (#2074)

### Fixed
- Inline SVG charts in short report HTML output are not rendered correctly (#2063)

## Version 10.1.2 (2024-11-08)

### Added
- Implement internal concentration and internal dose (systemic) (#1891)
- Modular design diagram based on action mappings (#2012)
- Implement expression type for kinetic conversion factors (#2019)
- Implement dust exposures module (experimental)
- Implement population alignment methods (#2040)

### Changed
- Rename modules: "Human monitoring data" to "HBM occurences" and "Human monitoring analysis" to "HBM analysis" (#2001)
- Remove enum value AbsorptionFactorModel from InternalModelType, add TargetDoseLevelType.Systemic (#2026)
- Rename HBM Occurences module (previously named human monitoring data) to HBM concentrations (#2039)
- Create risks action: rename exposure calculation method "Human monitoring concentrations" to "Human biomonitoring concentrations (#2058)

### Fixed
- Uncertainty runs of Cosmos (PBK) fail (#1999)
- Exposure mixtures: return output "Analysis of acute exposures is currently not supported" for network analysis when exposure type is acute (#2005)
- Implementation targetExposureUnit for internal dose and concentration unit (#2018)
- Oxyplots uppertails crash and end with error (#2028)
- Risk value in 'Contribution for individuals at risk' is nominal value (instead of median of all bootstrap runs) (#2031)
- Correction for bodyweight in BiologicalMatrixComparisons (#2035)
- Do not hide columns non-detects and non-quantifications in hbm occurences when data is zero (#2037)
- X-axis of distribution chart of cumulative Risks shows wrong hazard/exposures while the risk metric is set to exposure/hazard (#2038)
- Incorrect reading of exposure route for single value non-dietary exposure data (#2042)
- Compute action for OPEX fails with undefined unit for exposure route (#2050)
- Account for undefined sex property in Busgang method to estimate specific gravity from creatinine (#2051)
- Hazard characterisations output not correctly rendered for systemic (#2052)

## Version 10.1.1 (2024-09-20)

### Added

- Implement PbkModels module
- Kinetic conversion: implement inverse uniform distribution for conversion factors and test for PARC T6.2.3. pesticides case study (#1992)
- Implement new modules for dust exposure assessment (#2009)
- Add tables and charts for individuals at risk, noy only based on a percentage for the upper tail (#2010)

### Changed

- Move compartment to target exposures module
- Exposures module, rename to internal exposures (#1972)
- Separate settings (and behaviour) for kinetic conversion from calculation method settings in hazard characterisations module (#2008)
- Split kinetic models into AbsorptionFactors (KineticModels) and KineticConversionFactors module

### Fixed

- Use deterministic substance conversion factors only for replace measurements of focal food/substance scenarios (#1994)
- Correct and refactor load PBK models function of compiled data manager to correctly load PBK model instance records based on scope filter (#2000)
- Correct loading of additional sample properties when loading samples (#2006)

## Version 10.1.0 (2024-08-15)

### Added

- Create PBK modelng mechanism to allow selection of outputs from multiple compartments (#1927)
- Add install scripts for Python 3.12 to support roadrunner SBML (#1965)
- Update PARC-HBM data format reader to support codebook v2.4 (#1970)

### Changed

- Restructure MCRA settings XML format to group settings per module (#581)
- Migrate code base from .NET 6 to .NET 8 (#1833)
- Refactor PBK models and their use in internal exposures and hazard characterisations module (#1933)
- Upgrade R to version 4.4.1 and rtools to 4.4 (#1966)

### Fixed

- Cannot convert to target matrix blood when blood is not present as sampling method in HBM data (#1960)
- Output sections are still visible when suppressed in the action settings file (#1969)
- Data formats templates readme text contains invalid URLs (#1979)
- Co-exposure tables fails for aggregate (#1983)
- Human monitoring individuals statistics does not reflect individuals in the further analysis (#1986)

## Version 10.0.15 (2024-06-21)

### Fixed

- Percentages in risks are not correct (#1962)
- Nominal value used in pies contributions, also in uncertainty run (#1963)

## Version 10.0.14 (2024-06-18)

### Fixed

- SA Risk steatosis from imazalil seems broken (#1948)
- Show legend MCR plot when exclude privacy sensitive data is selected (#1949)
- Wrong unit for hair in hazard characterisations (#1957)
- Kinetic conversion to blood when blood is not present as sampling method in HBM data (#1959)
- Fix resampling of kinetic conversion factors in uncertainty run

## Version 10.0.13 (2024-05-28)

### Added

- Add settings tiers for Prospective Dietary CRA (EFSA 2023) (#1924)

### Fixed

- Order of exposures in risks percentile table (#1945)
- HBM data gives sequence contains no elements when all body weights are missing (#1946)

## Version 10.0.12 (2024-05-21)

### Added

- Kinetic conversion for specific biomarkers in hbm analysis (#1827)
- Implementation of SBML generic model (#1910)
- Implement beta distribution for exposure biomarker conversion (#1911)
- Add lod and loq columns to table in hbm data (#1912)
- Adding opex worker-bystander-resident results (#1919)
- Risks, add contributions to risks for individuals for upper distribution (#1936)

### Changed

- Combine different urine sampling types (#1880)
- Decouple MCR settings / analysis in different modules (e.g. MCR in Risks and HBM analysis) (#1931)

### Fixed

- Output standard action should not report on age dependent hc subgroups when there are no subgroups (#1913)
- PARC T6.2.3 CS Pesticides: fixed uncertainty analysis demonstration (#1916)
- Exclude substances in output of hbm analysis that were not measured (#1917)
- Do not show substances from other matrices in hbm concentrations by substance before conversion (#1922)
- Set settings up-to-date in settings tiers for acute/chronic (#1929)
- MCR plot PFAS Risk action fails (#1930)

## Version 10.0.11 (2024-04-12)

### Added

- Filtering of hbm data on time points (#1782)
- Running two opex products for compute action single value non-dietary exposures (#1796)
- Specific gravity from creatinine urine adjustment method by Busgang et al. (#1874)
- Option in to skip privacy sensitive outputs in report (#1882)
- New expression type for specific gravity (#1896)
- Add legend for stacked histogram (#1901)
- Implement new option nondetects handling (#1905)

### Changed

- Split Busgang urine specific gravity normalisation into age and non-age dependent methods (#1903)
- Draw from censored lognormal using nondetect and nonquantification information (#1904)

### Fixed

- Remove hbm analysis option to convert using default value 1 (#1829)
- Limit hbm analysis to active substances only (#1865)

## Version 10.0.10 (2024-03-07)

### Added

- Data tables for single value non-dietary exposures (#1860)
- Add warning for missing substances in CAG (#1864)

### Changed

- MCRA boxplots: adapt for substances with only a few positive measurements (#1888, #1881)

### Fixed

- Correct random seed for drawing market shares in market shares calculator
- Use market shares calculated by food conversion instead of the raw market shares
- Hide module points of departure when use data is selected for hazard characterisations (#1887)
- Missing uncertainty results in several Risk exposure output sections
- Apply the complete-cases filter before kinetic conversion and biomarker conversion

## Version 10.0.9 (2024-02-27)

### Added

- New module single value non-dietary exposures (#1733)
- Implement 'use kinetic conversion factor subgroup' setting (#1868)
- Add exposure and hazard by age section with chart for age dependent HC analyses (#1873)
- Add specific gravity from creatinine adjusted method Carrieri 2001 (#1874)

### Changed

- HBM analysis: update biomarker conversion to allow for biomarker conversion subgroups (age/sex) (#1844)
- Move kinetic conversion factors and exposure biomarker conversion to calculator, repair reproducability (#1870)

### Fixed

- Kernel calculation in R for violin plots fails when vector contains infinities (#1869)
- Human monitoring analysis settings: order of settings is incorrect (#1876)
- Do not support multiple urines in analysis, but do allow multiple urines in HBM data (#1877)
- Fix market shares calculation for chronic exposure assessments (#1878)

## Version 10.0.8 (2024-02-12)

### Added

- Implement data format and use of kinetic conversion factors sub-groups (#1805)
- Extend HBM biomarker conversion calculator with possibility for summation of multiple measured substances (#1837)

### Changed

- HBM analysis: change order of settings in output summary (#1839)
- Change all caption upper summaries, use specified percentage (#1859)

### Fixed

- ExposurePathway Oral enum was used instead of Dietary enum when running the reverse kinetic model (from internal to external) (#1850)
- Upper MCR plot fix, MCR plots for RPF weighted and sum of ratios corrected x-axes (#1854)
- Update food conversion calculator to be a bit more permissive for market shares not adding up to 100% (allow for minor differences)
- Oxyplot individuals don't render, add uncertainty substances (#1862, #1866, #1867)

## Version 10.0.7 (2024-02-02)

### Added

- Implement uncertainty distribution kinetic conversion factors (#1845)

### Changed

- Move body weight imputation to hbm analysis (#1807)

### Fixed

- HBM complete samples should filter on individual day (#1835)
- Violin plots are missing (#1847)

## Version 10.0.6 (2024-01-26)

### Added

- Add a first implementation of the exposure biomarker conversion module and its use in the HBM analysis action (#1743)
- Summary section on substance contributions for rpf-weighted cumulative risks (#1769)
- Add boxplots for contributions to substances of individuals (#1774)

### Changed

- Update risks and HBM analysis calculator to allow risks on external exposures derived from HBM (#1820)
- More insightful sample columns for hbm data (#1821)
- Removed analytical methods data section from report (too large) (#1624)

### Fixed

- Improve loading speed frontend for large concentration datasets (#1542)
- Wrong percentiles cause exception message in boxplot of hbm data (#1825)
- PDF fails to render due to large AnalyticalMethods section (#1831)

## Version 10.0.5 (2023-12-21)

### Changed

- Additional changes for MCR analysis in risks module (#1784)

### Fixed

- Description for boxplots for full and positives only of hbm samples per substance (#1812)
- Total number non-analysed hbm samples reported incorrectly (#1813)
- Flip description individual - individual days  for hbm samples table (#1818)

## Version 10.0.4 (2023-12-18)

### Added

- Add general information columns (publication title e.o.) to RPF table (#1772)
- Allow for hazard-characterizations to vary as function of a covariate: example age-dependent HBM-GV for PFAS (#1778)
- Implement MCR analysis in risks module (#1784)

### Changed

- Rename MarginOfExposure and HazardIndex of type-definition/enum RiskMetricType (#1705)
- Update HBM kinetic conversion to automatically express external exposures per kg bodyweight (#1804)

### Fixed

- Update load data method in hazard characterisations module to filter on active substances when specified (#1799)
- Empty column header in sample time point sheets of HBM basic codebook cause nullref exception (#1800)
- Remove not-null/required constraint for sampling dates in HBM data (#1806)

## Version 10.0.3 (2023-12-01)

### Added

- Kinetic conversion models for metabolites (#1683)
- Add support of reverse metabolite to parent (external oral exposure) conversion in HBM analysis module (#1727)
- Add option to derive hazard characterisations from BMDLs and a switch option between parameteric and bootstrapped BMD uncertainty bounds (#1788)
- HBM data exclude substance - sampling method combinations (#1795)
- Show number of non-analysed samples in hbm data samples summary (#1797)

### Changed

- Update risk calculation to explicitly align exposure and hazard units (#1761)
- Hide uncertainty columns in hazard and rpf output when resamplerpf is not selected (#1773)
- Restructure risk section (#1785)

### Fixed

- Correct units display in BiologicalMatrixComparisons and add check for single matrix (#1760)
- Correct units administration in exposure mixtures module (#1763)
- Only include complete analysed samples and skip blanks (#1792)
- Default target surface level for hbm kinetic conversion set to internal

## Version 10.0.2 (2023-10-26)

### Added

- New table risk ratios, improved output (descriptions and view)
- Add number of individuals field to risks safety chart and risks by substance overview section

### Changed

- Include expression type (if relevant) when rendering axis title of exposure versus hazard plots
- Update HBM analysis module to filter individual/days with missing substance concentrations (also accounting for multiple matrices)

### Fixed

- Stack overflow exception reading an Excel file with many empty rows (#1767)
- Key not found exception: add check on Hazard characterisation ID when loading HC-uncertain data

## Version 10.0.1 (2023-10-23)

### Added

- MCRA commander must be able to run action loops (#1348)
- Bootstrap hbm monitoring data (#1405)
- Add support for PARC-HBM data for occupational exposure (#1578)
- Implement PARC-HBM data format reader to support codebook v2.3 (#1649)
- Basic implementation kinetic conversion factors (#1658, #1660)
- Implement new HBM units (#1678)
- Implement kinetic conversion factors (#1695)
- Exclude substances from urine or blood normalisation - standardisation (#1659, #1703)
- Add biological matrix and expression type to points of departure (POD) data table (#1724)
- Update HBM analysis module with univariate outlier detection based on interquartile range (#1728)
- Implement risks calculation for multiple targets (#1740)
- Add median of assessment factors distribution to output (#1741)
- Hazard characterisations uncertain (#1742)
- Add HBM risk-drivers summary sections for cumulative (RPF-weighted) HBM analyses (#1745)

### Changed

- Apply mixed HBM units logic (normal, normalised or standardised) to biological matrix concentrations (#1524)
- Update names of risk metric type options in risks module (#1573)
- Remove action tiers from module definitions to tier/template files per tier (#1673)
- Create separate matrix selection settings for kinetic models and HBM analysis (#1684)
- Generalize exposure matrix to allow substance-target combinations as rows (#1706)
- Consistent short concentration units in output of HBM (#1752)

### Fixed

- CLI and Web interface give different results for HBM sampling method (#1605)
- Wrong thousands separator in number of Monte Carlo iterations (#1645)
- Use correct hazard units for HBM data (#1662)
- Sort order in some CSV output tables differs between runs (#1682)
- Import HBM data should read in correct biological matrix (#1685)
- Crammed lines in boxplot for concentration by substances (#1690)
- Convert to single target for hazard characterisations (#1696)
- When the actiontype is changed a null reference exception occurs (#1707)
- Echo additional assessment factor in output (#1711)
- Use of POD with uncertainty sets returns "Faulted" error (#1712)
- Recalculate stddevs in ExposureMixtures when a subset is taken from the original exposure matrix (based on exposure limit) (#1720)
- Exclude substances from creatinine standardisation (#1722)
- Update resampling mechanism in RPF module to draw RPFs using uncertainty sets (#1751)

## Version 10.0.0 (2023-06-23)

Public release of Open MCRA Core source code on GitHub.

### Added

- Add boxplot to mixture exposure output (#1641)
- Create a .zenodo.json file with the default metadata for Zenodo GitHub releases (#1620)

### Changed

- Change name and split acute chronic EFSA/EC 2022-2018 tiers (#1640)
- Removed binary serialization option (BinaryFormatter is insecure and obsolete)

### Fixed

- Correct various errors in simulation run progress percentage (#1322)
- Inconsistencies in setting visibility and selected module tiers (#1632)
- Populations bulkcopier crashes when only individual properties table (as part of food survey) is available (#1637)
- Correctly show or hide two settings in occurence pattern settings summariser (scale up and restrict to authorised uses) (#1639)
- EuHbmImportDataCopier, the wrong ages are imported (#1644)
- Improve error reporting when XML serialization fails (#1646)
- Population subset ends with no reference (#1648)
- Tiers contain irrelevant settings like Left or Right Margin plots (#1651)
- MCR section not showing in SA demo acute cumulative risk assessment (#1652)
- Incorrect number of total samples analysed reported in concentration limit exceedances by food table (#1654)

## Version 9.2.10 (2023-06-01)

### Changed

- Download loop task output (html, tables, charts) creates a zip file with all sub-task outputs (#1530)

### Fixed

- Loop calculation output collection step fails for active substances
- Correct EFSA 2022 CRA Tier settings
- Bug in EUHbmImportDataCopier: LOD and LOQ are interchanged (#1636)

## Version 9.2.9 (2023-05-24)

### Added

- Add csv output of percentiles of bootstrap runs to percentiles sections of MOE and HI (#1535)
- Create settings tiers for EFSA 2022 regulatory method (#1541)
- Implement support for missing bodyweights in MCRA (#1585)
- Implement ug/L and other per L units as possible dose unit(s) for hazard characterisations (#1590)
- Add 'compare sub tasks' to single value risk module (#1606)
- Add bar charts to heatplots exposure mixture action (#1608)

### Changed

- Clean up the contents of the downloadable HTML zip report: move CSV, SVG and metadata files into subfolders (#1361)
- Update create action wizard of risks module (and single value risks) to include risk metric choice (#1574)
- Exposure mixtures: put pie charts for substance contributions to components in a grid layout (#1622)
- Update dietary exposures action calculator to not include RPF input requirement in case of substance loops (#1631)

### Fixed

- Human monitoring data allows to run without specifying survey (#1593)
- Exposure axis should be omitted in safety plots for the sum of risk ratios method (#1596)
- HBM analysis fails for chronic (#1601)
- Standard actions for prospective cumulative risk assessment shows same result for all scenarios (#1615)

## Version 9.2.8 (2023-04-25)

### Added

- Update HTML report in zip download with toc-functionality to make it easier to browse through (#1409)
- Risks: Implement cumulative as sum of single-substance ratios (Exposure/Hazard) (#1528)
- Update PARC-HBM data format reader to support codebook v2.2 (#1540)

### Changed

- Move MCR settings of output panel to MCR section in HBM analysis/dietary exposures/exposures panel (#1526)

### Fixed

- Data sources view is not updated/refreshed after upload new data source version (#1512)
- Bug in PFAS run for 6 substances (#1571)
- Parameters PBK Euromix generic model are not echoed (#1579)
- Bug HBM data after Biological matrix conversion (#1587)
- Empty density chart (# 1577)

## Version 9.2.7 (2023-04-03)

### Added

- Add blood and urine standardisation methods (#1359, #1477)
- Add method for "unstandardised" substance-weighing for MCR calculations and mixtures analysis (#1500)
- Update EU HBM data copier for support of codebook version 2.1 (#1538)
- Add 'IsMcrAnalysis' and 'McrExposureApproachType' settings and defaults to MixtureSelectionSettings (#1539)

### Changed

- Separate substance-weighing option for MCR and mixtures analysis and add checkbox to compute or not compute MCR in HBM analysis actions (#1499)
- Update exposure mixtures module with option to log-transform data before network analysis (#1531)
- Update AFDensity chart creator; remove incorrect red line that was supposed to mark the maximum of the pdf (#1532)

### Fixed

- Deterministic imputation for hbm (random per substance) and fix of vito regression test (#1522)

## Version 9.2.6 (2023-03-10)

### Added

- Add compartments including cumulative amounts like urine (#1272)
- Allow hbm as input for risks (#1394)
- Water imputation by approved substances only (#1460)
- Add new module substance approvals (#1461)

### Changed

- Update substance allocation calculators to also check for authorisation of base foods of processed foods when accounting for authorisations (#1473)
- Update HBM individual day concentrations calculator to compute HBM concentrations only for the selected active substances instead of all substances (#1493)

### Fixed

- Update HBM samples per sampling method and substance table to not include records with only missing values (#1375)
- Download action + data (data as zipped csv) fails for populations (#1451)
- Correct download U and V matrix in Mixture exposure action (#1490)
- Duplicate labels in settings xml, or missing in kinetic model xml (#1495)
- Value cannot be null exception in risk action for the cases that RPFs are available for some other action, but calculation of cumulative results is not set and no reference substance has been set (#1508)
- Fix analytical methods report summary table to print missing LODs/LOQs as "-" instead of NaN and unit
- SSD data uploads save all concentration records to database, LOQ records (default) should be omitted (#1519)
- Fix duplicate creation of dummy records (once as hierarchy record and once as normal record) in exposures by food as measured section summarizer (#1521)

## Version 9.2.5 (2023-02-10)

### Changed

- Update third party packages (NuGet) for MCRA Core

### Fixed

- Cannot immediate delete data source file after upload (#1456)
- Safety chart shows in greyscale instead of colored (PNG image) (#1471)

## Version 9.2.4 (2023-02-03)

### Added

- Implement violin plot for risk distribution plots for HI and MOE loop overview report (#1433)
- Add population characteristics Real Life Mixtures (#1475)

### Changed

- Update HBM data/analysis module to allow for analysis of concentrations of multiple sampling methods/matrices (#1398)
- Update xml structure kinetic models (#1428, #1429)
- Move internalconcentrationtype to assessment settings

### Fixed

- Download action + data (data as zipped csv) fails (#1451)
- Fix SSD concentrations importer to also allow zero concentrations as resType VAL

## Version 9.2.3 (2023-01-16)

### Added

- Implement kinetic model with metabolites (#1281)
- Implement chlorpyrifos kinetic model with metabolites (#1285)
- Implement violin plot for risk distribution plots for HI and MOE loop overview report (#1433)
- Use CLI tool to create an action template for a chosen action type (#1437)
- Use action folder directly, without extra zipping step, for CLI (#1445)

### Changed

- Removed test reports (trx) parsing functions from MCRA.Utils (#1441)

### Fixed

- Fix concentrations action to use substances collection for extrapolation when active substance collection is not specified (#1447)

## Version 9.2.2 (2022-12-20)

### Added

- Documentation: Add PARC Real-life mixture guidance documents (pdf) and related data to the User guide - Examples section (#1366)

### Changed

- Update HBM analysis module with non-detects imputation method according to method established within HBM4EU/PARC RLM (#1396)

### Fixed

- Fix reference to dietary exposures section in risks output section (#1331)
- MCRA Build date-time not displayed correctly (#1410)
- Settings in overview only show headers and no settings (#1442)
- Mixtures crashes when no concentrations for HBM data are available (#1443)
- Output collection of task with subtask fails (#1444)
- MCRA settings loading fails: allow floating point literals (NaN, Inf etc) in Json serialization
