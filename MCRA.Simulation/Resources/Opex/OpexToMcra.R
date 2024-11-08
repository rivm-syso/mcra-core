## Set variables.
selectedPersons <- c("operator", "worker", "resident", "bystander")

#' Create MCRA input tables based on OPEX calculations.
#'
#' Using input data identical to the input data in OPEX, create tables that
#' can be imported directly into MCRA.
#'
#' @param productData A data.frame with product data in the format exported
#' from OPEX.
#' @param cropData A data.frame with crop data in the format exported
#' from OPEX.
#' @param substanceData A data.frame with substance data in the format exported
#' from OPEX.
#' @param absorptionData A data.frame with absorption data in the format
#' exported from OPEX.
#' @param selectedPersons A character vector containing the persons for which
#' the OPEX calculations should be performed. One or more of "operator",
#' "worker", "resident", "bystander".
#'
#' @results A list of data frames in MCRA format. The list can be exported
#' directly to excel and imported into MCRA.
#'
#' @examples
#'
#' tempDir <- tempdir()
#'
#' newFiles <- lapply(X = list(product = "product.csv",
#'                             substance = "substance.csv",
#'                             crop = "crop.csv",
#'                             absorption = "absorption.csv"),
#'                     FUN = function(x) file.path(tempDir, x))
#'
#' unzip("data/Lego_20240415_12h48_opex1.0.1.zip", exdir = tempDir)
#'
#' ## Read data
#' productData <- read.csv(file = newFiles$product)
#' cropData <- read.csv(file = newFiles$crop)
#' substanceData <- read.csv(file = newFiles$substance)
#' absorptionData <- read.csv(file = newFiles$absorption)
#'
#' ## Define persons for which to compute risks.
#' selectedPersons <- c("operator", "worker", "resident", "bystander")
#'
#' ## Create MCRA tables.
#' opexMCRATables <- createOpexMCRATables(productData, cropData, substanceData,
#'                                       absorptionData, selectedPersons)
#'
#' @export
createOpexMCRATables <- function(productData,
                                 cropData,
                                 substanceData,
                                 absorptionData,
                                 selectedPersons = c("worker", "operator",
                                                     "resident", "bystander")) {
  ## Check input.
  selectedPersons <- match.arg(selectedPersons, several.ok = TRUE)
  ## Get all relevant scenarios fitting to the provided input data.
  opexScenarios <- getOpexScenarios(selectedPersons = selectedPersons,
                                    productData = productData,
                                    substanceData = substanceData,
                                    cropData = cropData,
                                    absorptionData = absorptionData)
  ## Create MCRA table with exposure scenarios.
  ExposureScenarios <- createExposureScenariosTable(opexScenarios)
  ## Create MCRA table with populations.
  Populations <- createPopulationsTable(opexScenarios)
  ## Create MCRA table with substances.
  Substances <- createSubstancesTable(opexScenarios)
  ## Compute risks for operator.
  operatorRisks <- computeRisksOperator(opexScenarios)
  ## Compute risks for worker.
  workerRisks <- computeRisksWorker(opexScenarios)
  ## Compute risks for resident.
  residentRisks <- computeRisksResident(opexScenarios)
  ## Compute risks for bystander.
  bystanderRisks <- computeRisksBystander(opexScenarios)
  ## Combine all computed risks.
  risksTot <- data.table::rbindlist(list(operatorRisks, workerRisks,
                                         residentRisks, bystanderRisks),
                                    fill = TRUE)
  ## Create MCRA table with exposure estimates.
  ExposureEstimates <- createExposureEstimatesTable(risksTot)
  ## Create MCRA table with exposure determinants.
  ExposureDeterminants <- createExposureDeterminantsTable()
  ## Create MCRA tables with exposure determinant combinations.
  # Both in long format (EDV) and wide format (EDC).
  ExpDetComb <- createExposureDeterminantCombinationsTable(risksTot)
  ExposureDeterminantCombinations <- ExpDetComb$EDC
  ExposureDeterminantValues <- ExpDetComb$EDV
  return(list(ExposureScenarios = ExposureScenarios,
              ExposureDeterminants = ExposureDeterminants,
              ExposureDeterminantCombinations = ExposureDeterminantCombinations,
              ExposureDeterminantValues = ExposureDeterminantValues,
              ExposureEstimates = ExposureEstimates,
              Substances = Substances,
              Populations = Populations))
}


#' Helper function for creating OPEX scenarios
#'
#' Create a table of OPEX scenarios based on the persons for which the
#' OPEX calculations should be performed and the default OPEX input tables.
#'
#' The scenarios are imported from OPEX. Unique Ids for the scenario, population
#' and substance are added.
#'
#' @inheritParams createOpexMCRATables
#'
#' @return A data.frame containing the OPEX scenarios for the selected persons.
#' If "resident" and/or "bystander are selected", their scenarios are split in
#' "resident - adult" and "resident - child", and "bystander - adult" and
#' "bystander - child" respectively.
#'
#' @keywords internal
getOpexScenarios <- function(selectedPersons= c("worker", "operator",
                                                "resident", "bystander"),
                             productData,
                             substanceData,
                             cropData,
                             absorptionData) {
  ## Check input.
  selectedPersons <- match.arg(selectedPersons, several.ok = TRUE)
  ## Load defaults.
  allData <- loadData()
  ## Define columns containing scenario information.
  scenarioColumns <- c("crop", "person")
  scenarioColumnsoperator <- c("LCTM_equipment", "density", "applicationEquipment")
  scenarioColumnsworker <- "activity"
  scenarioColumnsresident <- "season"
  scenarioColumnsbystander <- "season"
  ## Get scenarios.
  scenariosPersons <- lapply(selectedPersons, FUN = function(selectedPerson) {
    scenariosPerson <- getScenarios(name = productData$name,
                                    formulation = productData$formulation,
                                    wps = productData$wps,
                                    category = productData$category,
                                    substanceInput = substanceData,
                                    cropInput = cropData,
                                    absorptionInput = absorptionData,
                                    person = selectedPerson,
                                    crops = allData$crops,
                                    dataDefault = allData$default)
    scenariosPersonTot <- do.call(rbind, scenariosPerson)
    scenariosPersonTot <- scenariosPersonTot[, !duplicated(colnames(scenariosPersonTot))]
    nScenarios <- nrow(scenariosPersonTot)
    if (selectedPerson %in% c("resident", "bystander")) {
      scenariosPersonTot <- rbind(scenariosPersonTot, scenariosPersonTot)
      scenariosPersonTot$person <- rep(paste(selectedPerson, "-", c("adult", "child")),
                                       each = nScenarios)
    } else {
      scenariosPersonTot$person <- selectedPerson
    }
    ## Add description based on scenario information columns.
    ## Used for identifying unique scenarios.
    scenarioColumnsPerson <- c(scenarioColumns,
                               get(paste0("scenarioColumns", selectedPerson)))
    scenariosPersonTot$Description <-
      do.call(paste, c(scenariosPersonTot[scenarioColumnsPerson], list(sep = "-")))
    scenariosPersonTot
  })
  scenariosTot <- as.data.frame(data.table::rbindlist(scenariosPersons,
                                                      fill = TRUE))
  ## Add scenarioId.
  scenariosTot$idExposureScenario <- factor(scenariosTot$Description,
                                            levels = unique(scenariosTot$Description))
  nScenarios <- nlevels(scenariosTot$idExposureScenario)
  scenariosTot$idExposureScenario <- as.numeric(scenariosTot$idExposureScenario)
  scenariosTot$idExposureScenario <-
    paste0("SC", formatC(scenariosTot$idExposureScenario,
                         width = ceiling(log10(nScenarios)),
                         format = "d", flag = "0"))
  ## Add populationId.
  scenariosTot$idPopulation <- factor(scenariosTot$person,
                                      levels = unique(scenariosTot$person))
  scenariosTot$idPopulation <- as.numeric(scenariosTot$idPopulation)
  scenariosTot$idPopulation <-
    paste0("POP", formatC(scenariosTot$idPopulation,
                          width = ceiling(log10(length(selectedPersons))),
                          format = "d", flag = "0"))
  ## Add substanceId.
  substances <-
    data.frame(substance = unique(scenariosTot$substance),
               idSubstance = make.unique(substring(unique(scenariosTot$substance), 1, 3)))
  scenariosTot$idSubstance <-
    substances$idSubstance[match(scenariosTot$substance, substances$substance)]
  return(scenariosTot)
}


#' Helper function for creating the MCRA exposure scenarios table
#'
#' From the data.frame with OPEX scenarios, create a data.frame in MCRA format
#' containing the exposure scenarios.
#'
#' @param opexScenarios A data.frame containing the OPEX scenarios.
#'
#' @return A data.frame containing the exposure scenarios in MCRA format.
#'
#' @keywords internal
createExposureScenariosTable <- function(opexScenarios) {
  ES <- data.frame(idExposureScenario = opexScenarios$idExposureScenario,
                   Name = opexScenarios$Description,
                   Description = opexScenarios$Description,
                   idPopulation = opexScenarios$idPopulation,
                   ExposureType = "Acute",
                   ExposureLevel = "Systemic",
                   ExposureUnit = "mg/kg bw/day")
  ES <- unique(ES)
  return(ES)
}


#' Helper function for creating the MCRA populations table
#'
#' From the data.frame with OPEX scenarios, create a data.frame in MCRA format
#' containing the populations.
#'
#' @inheritParams createExposureScenariosTable
#'
#' @return A data.frame containing the populations in MCRA format.
#'
#' @keywords internal
createPopulationsTable <- function(opexScenarios) {
  ageMin <- ifelse(grepl("child", opexScenarios$person), 1,
                   ifelse(grepl("adult", opexScenarios$person), 18, NA_real_))
  ageMax <- ifelse(grepl("child", opexScenarios$person), 17,
                   ifelse(grepl("adult", opexScenarios$person), 99, NA_real_))
  populations <- data.frame(idPopulation = opexScenarios$idPopulation,
                            Name = opexScenarios$person,
                            Description = "",
                            AgeMin = ageMin,
                            AgeMax = ageMax,
                            Gender = "",
                            Location = "",
                            Class = opexScenarios$person)
  populations <- unique(populations)
  return(populations)
}


#' Helper function for creating the MCRA substances table
#'
#' From the data.frame with OPEX scenarios, create a data.frame in MCRA format
#' containing the substances.
#'
#' @inheritParams createExposureScenariosTable
#'
#' @return A data.frame containing the substances in MCRA format.
#'
#' @keywords internal
createSubstancesTable <- function(opexScenarios) {
  substances <- unique(opexScenarios[c("idSubstance", "substance", "name")])
  colnames(substances) <- c("idSubstance", "Name", "Description")
  return(substances)
}


#' Helper function for computing the risks for the operator
#'
#' From the data.frame with OPEX scenarios, compute the risks for the operator.
#'
#' @inheritParams createExposureScenariosTable
#'
#' @return A data.frame in long format containing the risks for the operator.
#' A row in the output contains the exposure for the operator for a specific
#' scenario with a specific combination of exposure determinants. A unique Id
#' is added for each combination of exposure determinants.
#'
#' @keywords internal
computeRisksOperator <- function(opexScenarios) {
  ## Get scenarios for operator.
  operatorScenarios <- opexScenarios[opexScenarios$person == "operator", ]
  ## Compute risks.
  riskScenarios <- lapply(X = 1:nrow(operatorScenarios), FUN = function(i) {
    scenarioDat <- operatorScenarios[i, ]
    riskScenario <- calculateRisks(scenarioDat)
    ## Get columns containing information about protection.
    protectionCols <- colnames(riskScenario)[startsWith(colnames(riskScenario), "ml") |
                                               startsWith(colnames(riskScenario), "ap")]
    ## Convert to long format.
    riskScenarioLong <- reshape2::melt(riskScenario,
                                       id = protectionCols,
                                       variable.name = "scenarioExposure",
                                       value.name = "risk")
    riskScenarioLong$idExposureScenario <- scenarioDat$idExposureScenario
    riskScenarioLong$idSubstance <- scenarioDat$idSubstance
    return(riskScenarioLong)
  })
  riskScenarioTot <- do.call(rbind, riskScenarios)
  ## Only keep total exposure.
  riskScenarioTot$scenarioExposure <- as.character(riskScenarioTot$scenarioExposure)
  riskScenarioTot$exposure <- sapply(X = strsplit(riskScenarioTot$scenarioExposure, "_"),
                                     FUN = `[[`, 2)
  riskScenarioTot <- riskScenarioTot[startsWith(riskScenarioTot$exposure, "total"), ]
  riskScenarioTot <- riskScenarioTot[!is.na(riskScenarioTot$risk), ]
  ## Combine with scenarios to get a complete data frame with all information.
  riskScenarioTot <- merge(riskScenarioTot, operatorScenarios,
                           by = c("idExposureScenario", "idSubstance"))
  ## Convert risk value from percentage to hazard dose.
  riskScenarioTot$exposureEstimate <- riskScenarioTot$risk / 100 *
    ifelse(startsWith(riskScenarioTot$scenario, "75"),  riskScenarioTot$aoel,
           riskScenarioTot$aaoel)
  riskScenarioTot$ExposureSource <- riskScenarioTot$applicationMethod
  riskScenarioTot$EstimateType <- paste0("P", gsub("\\D", "", riskScenarioTot$exposure))
  ## Add exposure determinant combination.
  protectionsColsTot <-
    c("applicationEquipment",
      unique(as.vector(
        sapply(X = riskScenarios, FUN = function(riskScenario) {
          colnames(riskScenario)[startsWith(colnames(riskScenario), "ml") |
                                   startsWith(colnames(riskScenario), "ap")]
        }))))
  riskScenarioTot$exposureDeterminantCombination <-
    do.call(paste, c(riskScenarioTot[protectionsColsTot], list(sep = "-")))
  riskScenarioTot$exposureDeterminantCombination <-
    factor(riskScenarioTot$exposureDeterminantCombination,
           levels = unique(riskScenarioTot$exposureDeterminantCombination))
  riskScenarioTot$idExposureDeterminantCombination <-
    paste0("EDC-OP-",
           formatC(as.numeric(riskScenarioTot$exposureDeterminantCombination),
                   width = ceiling(log10(nlevels(riskScenarioTot$exposureDeterminantCombination))),
                   format = "d", flag = "0"))
  return(riskScenarioTot)
}


#' Helper function for computing the risks for the worker
#'
#' From the data.frame with OPEX scenarios, compute the risks for the worker
#'
#' @inheritParams createExposureScenariosTable
#'
#' @return A data.frame in long format containing the risks for the worker.
#' A row in the output contains the exposure for the worker for a specific
#' scenario with a specific combination of exposure determinants. A unique Id
#' is added for each combination of exposure determinants.
#'
#' @keywords internal
computeRisksWorker <- function(opexScenarios) {
  ## Get scenarios for worker.
  workerScenarios <- opexScenarios[opexScenarios$person == "worker", ]
  ## Compute risks.
  riskScenarios <- lapply(X = 1:nrow(workerScenarios), FUN = function(i) {
    scenarioDat <- workerScenarios[i, ]
    riskScenario <- workerRisk(loadData(), scenarioDat, scenarioDat$aoel)
    riskScenario$idExposureScenario <- scenarioDat$idExposureScenario
    riskScenario$idSubstance <- scenarioDat$idSubstance
    return(riskScenario)
  })
  riskScenarioTot <- do.call(rbind, riskScenarios)
  riskScenarioTot$exposureEstimate <- riskScenarioTot$`Total exposure (mg/kg bw/day)`
  riskScenarioTot <- riskScenarioTot[!is.na(riskScenarioTot$exposureEstimate), ]
  ## Combine with scenarios to get a complete data frame with all information.
  riskScenarioTot <- merge(riskScenarioTot, workerScenarios,
                           by = c("idExposureScenario", "idSubstance"))
  riskScenarioTot$ExposureSource <- "Undefined"
  riskScenarioTot$EstimateType <- "P75"
  ## Add exposure determinant combination.
  riskScenarioTot$exposureDeterminantCombination <-
    factor(riskScenarioTot$PPE, levels = unique(riskScenarioTot$PPE))
  riskScenarioTot$idExposureDeterminantCombination <-
    paste0("EDC-WK-",
           formatC(as.numeric(riskScenarioTot$exposureDeterminantCombination),
                   width = ceiling(log10(nlevels(riskScenarioTot$exposureDeterminantCombination))),
                   format = "d", flag = "0"))
  return(riskScenarioTot)
}


#' Helper function for computing the risks for the bystander
#'
#' From the data.frame with OPEX scenarios, compute the risks for the bystander
#'
#' @inheritParams createExposureScenariosTable
#'
#' @return A data.frame in long format containing the risks for the bystander.
#' A row in the output contains the exposure for the bystander for a specific
#' scenario from a specific exposure source. The exposure is split in an
#' exposure for adults and an exposure for children.
#'
#' @keywords internal
computeRisksBystander <- function(opexScenarios) {
  ## Get scenarios for worker.
  bystanderScenarios <- opexScenarios[grepl("bystander", opexScenarios$person), ]
  ## Compute risks.
  riskScenarios <- lapply(X = 1:nrow(bystanderScenarios), FUN = function(i) {
    scenarioDat <- bystanderScenarios[i, ]
    ageGroup <- if (grepl("child", scenarioDat$person)) "Children" else "Adults"
    riskScenario <- bystanderRisk(scenarioDat, scenarioDat$aaoel)
    riskScenario <- as.data.frame(riskScenario[[ageGroup]][2, , drop = FALSE])
    riskScenarioLong <- reshape2::melt(riskScenario,
                                       measure.vars = colnames(riskScenario),
                                       variable.name = "ExposureSource",
                                       value.name = "exposureEstimate")
    riskScenarioLong$idExposureScenario <- scenarioDat$idExposureScenario
    riskScenarioLong$idSubstance <- scenarioDat$idSubstance
    return(riskScenarioLong)
  })
  riskScenarioTot <- do.call(rbind, riskScenarios)
  riskScenarioTot <- riskScenarioTot[!is.na(riskScenarioTot$exposureEstimate), ]
  ## Combine with scenarios to get a complete data frame with all information.
  riskScenarioTot <- merge(riskScenarioTot, bystanderScenarios,
                           by = c("idExposureScenario", "idSubstance"))
  riskScenarioTot$EstimateType <- "P95"
  ## Add exposure determinant combination.
  riskScenarioTot$idExposureDeterminantCombination <- "EDC-RB-1"
  riskScenarioTot$exposureDeterminantCombination <- "None"
  return(riskScenarioTot)
}


#' Helper function for computing the risks for the resident
#'
#' From the data.frame with OPEX scenarios, compute the risks for the resident
#'
#' @inheritParams createExposureScenariosTable
#'
#' @return A data.frame in long format containing the risks for the resident.
#' A row in the output contains the exposure for the resident for a specific
#' scenario from a specific exposure source. The exposure is split in an
#' exposure for adults and an exposure for children.
#'
#' @keywords internal
computeRisksResident <- function(opexScenarios) {
  ## Get scenarios for worker.
  residentScenarios <- opexScenarios[grepl("resident", opexScenarios$person), ]
  ## Compute risks.
  riskScenarios <- lapply(X = 1:nrow(residentScenarios), FUN = function(i) {
    scenarioDat <- residentScenarios[i, ]
    ageGroup <- if (grepl("child", scenarioDat$person)) "Children" else "Adults"
    riskScenario <- residentRisk(scenarioDat, scenarioDat$aoel)
    riskScenario <- as.data.frame(riskScenario[[ageGroup]][2, , drop = FALSE])
    riskScenarioLong <- reshape2::melt(riskScenario,
                                       measure.vars = colnames(riskScenario),
                                       variable.name = "ExposureSource",
                                       value.name = "exposureEstimate")
    riskScenarioLong$idExposureScenario <- scenarioDat$idExposureScenario
    riskScenarioLong$idSubstance <- scenarioDat$idSubstance
    return(riskScenarioLong)
  })
  riskScenarioTot <- do.call(rbind, riskScenarios)
  riskScenarioTot <- riskScenarioTot[!is.na(riskScenarioTot$exposureEstimate), ]
  ## Combine with scenarios to get a complete data frame with all information.
  riskScenarioTot <- merge(riskScenarioTot, residentScenarios,
                           by = c("idExposureScenario", "idSubstance"))
  riskScenarioTot$EstimateType <-
    ifelse(riskScenarioTot$ExposureSource == "All pathways (mean)", "P50", "P75")
  ## Add exposure determinant combination.
  riskScenarioTot$idExposureDeterminantCombination <- "EDC-RB-1"
  riskScenarioTot$exposureDeterminantCombination <- "None"
  return(riskScenarioTot)
}


#' Helper function for creating the MCRA exposure estimates table
#'
#' From the data.frame with OPEX risk exposures, create a data.frame in MCRA
#' format containing the exposure estimates.
#'
#' @param riskData A data.frame in long format containing the risk data. The
#' (possibly combined) output from `computeRiskOperator`, `computeRiskWorker`,
#' `computeRisksBystander` and `computeRisksResident`.
#'
#' @return A data.frame containing the exposure estimates in MCRA format.
#'
#' @keywords internal
createExposureEstimatesTable <- function(riskData) {
  EE <- unique(
    data.frame(idExposureScenario = riskData$idExposureScenario,
               idExposureDeterminantCombination = riskData$idExposureDeterminantCombination,
               ExposureSource = riskData$ExposureSource,
               idSubstance = riskData$idSubstance,
               Value = riskData$exposureEstimate,
               EstimateType = riskData$EstimateType))
  EE <- EE[order(EE$idExposureScenario, EE$idExposureDeterminantCombination), ]
  return(EE)
}


#' Helper function for creating the MCRA exposure determinants table
#'
#' Create a data.frame in MCRA format containing the exposure determinants.
#'
#' @return A data.frame containing the exposure estimates in MCRA format.
#'
#' @keywords internal
createExposureDeterminantsTable <- function() {
  ED <- data.frame(idExposureDeterminant =
                     c("apEq", "mlBody", "mlHands", "mlHead",
                       "apBody", "apHands", "apHead",
                       "PPE"),
                   Name = c("applicationEquipment",
                            "mlProtectionBody",
                            "mlProtectionHands",
                            "mlProtectionHead",
                            "apProtectionBody",
                            "apProtectionHands",
                            "apProtectionHead",
                            "workerProtection"),
                   Description = c("Equipment used for application",
                                   "Body protection mixing/loading",
                                   "Hands protection mixing/loading",
                                   "Head protection mixing/loading",
                                   "Body protection application",
                                   "Hands protection application",
                                   "Head protection application",
                                   "Worker protection"),
                   Type = c("Categorical", "Boolean", "Boolean",
                            "Categorical", "Boolean", "Boolean",
                            "Categorical", "Categorical"))
  return(ED)
}


#' Helper function for creating the MCRA exposure determinant combinations table
#'
#' From the data.frame with OPEX risk exposures, create a data.frame in MCRA
#' format containing the exposure determinant combinations.
#'
#' @inheritParams createExposureEstimatesTable
#'
#' @return A list of two data.frames containing the exposure determinant
#' combinations in both long (EDV) and wide (EDC) format. Both formats can be
#' used in MCRA.
#'
#' @keywords internal
createExposureDeterminantCombinationsTable <- function(riskData) {
  riskData <- riskData[!riskData$idExposureDeterminantCombination == "", ]
  riskDataOperator <- riskData[riskData$person == "operator", ]
  EDCOperator <-
    unique(data.frame(idExposureDeterminantCombination = riskDataOperator$idExposureDeterminantCombination,
                      Name = substring(riskDataOperator$exposureDeterminantCombination, 1, 100),
                      Description = riskDataOperator$exposureDeterminantCombination,
                      apEq = riskDataOperator$applicationEquipment,
                      mlBody = riskDataOperator$mlBody != "None",
                      mlHands = riskDataOperator$mlHands != "None",
                      mlHead = riskDataOperator$mlHead,
                      apBody = riskDataOperator$apBody != "None",
                      apHands = riskDataOperator$apHands != "None",
                      apHead = riskDataOperator$apHead))
  riskDataWorker <- riskData[riskData$person == "worker", ]
  EDCWorker <-
    unique(data.frame(idExposureDeterminantCombination = riskDataWorker$idExposureDeterminantCombination,
                      Name = substring(riskDataWorker$exposureDeterminantCombination, 1, 100),
                      Description = riskDataWorker$exposureDeterminantCombination,
                      PPE = riskDataWorker$PPE))
  riskDataResByst <- riskData[riskData$person %in% c("resident - adult", "resident - child",
                                                     "bystander - adult", "bystander - child"), ]
  EDCResByst <-
    unique(data.frame(idExposureDeterminantCombination = riskDataResByst$idExposureDeterminantCombination,
                      Name = substring(riskDataResByst$exposureDeterminantCombination, 1, 100),
                      Description = riskDataResByst$exposureDeterminantCombination))
  EDC <- as.data.frame(data.table::rbindlist(list(EDCOperator, EDCWorker, EDCResByst),
                                             fill = TRUE))
  EDC <- EDC[order(EDC$idExposureDeterminantCombination), ]
  EDV <- reshape2::melt(EDC,
                        id.vars = c("idExposureDeterminantCombination", "Name", "Description"),
                        variable.name = "PropertyName",
                        value.name = "TextValue")
  EDV <- EDV[!is.na(EDV$TextValue) & !EDV$TextValue %in% c("FALSE", "None"), ]
  return(list(EDC = EDC, EDV = EDV))
}