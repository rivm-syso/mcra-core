# Change Log

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
