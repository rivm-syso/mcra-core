# Change Log

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
