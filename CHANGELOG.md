# Change Log

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
