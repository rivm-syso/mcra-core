## Set variables.
selectedCropId <- cropData$id[1]
selectedPerson <- "operator"

computeRisks <- function(productData,
                         cropData,
                         substanceData,
                         absorptionData,
                         selectedCropId,
                         selectedPerson) {
  ## Load data loads default values for some inputs.
  allData <- loadData()
  
  ## Get scenarios.
  summaryScenarios <- getScenarios(name = productData$name, 
                                   formulation = productData$formulation, 
                                   wps = productData$wps, 
                                   category = productData$category, 
                                   substanceInput = substanceData, 
                                   cropInput = cropData, 
                                   absorptionInput = absorptionData, 
                                   person = selectedPerson, 
                                   crops = allData$crops, 
                                   dataDefault = allData$default)
  scenarios <- summaryScenarios[sapply(summaryScenarios, nrow) > 0]
  
  ## Select scenario for current crop.
  scenariosForCrop <- scenarios[[selectedCropId]]
  ## Remove duplicated columns.
  scenariosForCrop <- scenariosForCrop[, !duplicated(colnames(scenariosForCrop))]
  
  # Add Id.
  scenariosUnique <- unique(scenariosForCrop[c("crop", "LCTM_equipment", 
                                               "density", "applicationEquipment")])
  scenariosUnique$idExposureScenario <- 
    paste0("SC", formatC(1:nrow(scenariosUnique), 
                         width = ceiling(log10(nrow(scenariosUnique))), 
                         format = "d", flag = "0"))
  scenariosForCrop <- merge(scenariosUnique, scenariosForCrop, sort = FALSE)
  
  ## Compute risks.
  riskList <- lapply(scenarios, calculateRisks)
  risksForCrop <- riskList[[selectedCropId]]
  risksForCrop$person <- selectedPerson
  
  attr(risksForCrop, which = "scenarios") <- scenariosForCrop
  
  return(risksForCrop)
}

## Convert to more convenient format.
convertRisksToLong <- function(risksCrop, 
                               scenariosCrop) {
  ## Get columns containing information about protection.
  protectionCols <- colnames(risksCrop)[startsWith(colnames(risksCrop), "ml") |
                                          startsWith(colnames(risksCrop), "ap")]
  
  ## Convert to long format.
  riskCropLong <- reshape2::melt(risksCrop, 
                                 id = c(protectionCols, "person"),
                                 variable.name = "scenarioExposure",
                                 value.name = "risk")
  
  ## Add relevant variables.
  riskCropLong$scenarioExposure <- as.character(riskCropLong$scenarioExposure)
  riskCropLong$scenario <- sapply(X = strsplit(riskCropLong$scenarioExposure, "_"),
                                  FUN = `[[`, 1)
  riskCropLong$exposure <- sapply(X = strsplit(riskCropLong$scenarioExposure, "_"),
                                  FUN = `[[`, 2)
  
  ## Only keep total exposure.
  riskCropLong <- riskCropLong[startsWith(riskCropLong$exposure, "total"), ]
  riskCropLong <- riskCropLong[!is.na(riskCropLong$risk), ]
  
  ## Combine with scenarios to get a complete data frame with all information.
  riskCropLong <- merge(riskCropLong, scenariosCrop, 
                        by.x = "scenario", by.y = "row.names")
  
  ## Convert risk value from percentage to hazard dose.
  riskCropLong$exposureEstimate <- riskCropLong$risk / 100 * 
    ifelse(as.numeric(gsub("\\D", "", riskCropLong$exposure)) == 75 , 
           riskCropLong$aoel,
           riskCropLong$aaoel)
  
  ## Add exposure determinant combination.
  riskCropLong$ExposureDeterminantCombination <- 
    paste0(riskCropLong$applicationEquipment, 
           riskCropLong$mlBody,
           riskCropLong$mlHands,
           riskCropLong$mlHead,
           riskCropLong$apBody,
           riskCropLong$apHands,
           riskCropLong$apHead)
  riskCropLong$ExposureDeterminantCombination <- 
    factor(riskCropLong$ExposureDeterminantCombination,
           levels = unique(riskCropLong$ExposureDeterminantCombination))
  riskCropLong$idExposureDeterminantCombination <- 
    paste0("EDC", 
           formatC(as.numeric(riskCropLong$ExposureDeterminantCombination), 
                   width = ceiling(log10(nlevels(riskCropLong$ExposureDeterminantCombination))), 
                   format = "d", flag = "0"))
  return(riskCropLong)
}

createPopulationsTable <- function(selectedPerson) {
  Populations <- 
    data.frame(id = "POP1",
               Name = selectedPerson,
               Description = "",
               AgeMin = "",
               AgeMax = "",
               StartDate = NA_character_,
               EndDate = NA_character_,
               Gender = "",
               Location = "",
               Class = selectedPerson)
  return(Populations)
}

createSubstancesTable <- function(riskCropLong,
                                  scenariosCrop) {
  Substances <- unique(scenariosCrop[c("idSubstance", "substance", "name")])
  colnames(Substances) <- c("idSubstance", "Name", "Description")
  Substances$idSubstanceNew <- make.unique(substring(Substances$Name, 1, 3))
  
  ## Add new substance Id to risk table.
  riskCropLong <- merge(riskCropLong, Substances[c("idSubstance", "idSubstanceNew")])
  
  Substances$idSubstance <- Substances$idSubstanceNew
  Substances$idSubstanceNew <- NULL
  
  return(list(riskCropLong = riskCropLong, Substances = Substances))
}

createExposureScenariosTable <- function(scenariosCrop, 
                                         selectedPerson, 
                                         Populations) {
  ES <- data.frame(idExposureScenario = scenariosCrop$idExposureScenario,
                   Name = paste(scenariosCrop$crop,
                                selectedPerson),
                   Description = paste0("density ",
                                        scenariosCrop$density,
                                        ", application ",
                                        tolower(scenariosCrop$applicationEquipment)),
                   idPopulation = Populations$id,
                   ExposureType = "Acute",
                   ExposureLevel = "Internal",
                   ExposureRoutes = "Undefined",
                   ExposureUnit = "mg/kg bw/day")
  ES$Name <- paste(ES$Name, "-", ES$Description)
  ES <- unique(ES)
  return(ES)
}

createExposureDeterminantsTable <- function() {
  ED <- data.frame(idExposureDeterminant = 
                     c("apEq", "mlBody", "mlHands", "mlHead",
                       "apBody", "apHands", "apHead"),
                   Name = c("applicationEquipment", 
                            "mlProtectionBody",
                            "mlProtectionHands",
                            "mlProtectionHead",
                            "apProtectionBody",
                            "apProtectionHands",
                            "apProtectionHead"),
                   Description = c("Equipment used for application",
                                   "Body protection mixing/loading",
                                   "Hands protection mixing/loading",
                                   "Head protection mixing/loading",
                                   "Body protection application",
                                   "Hands protection application",
                                   "Head protection application"),
                   Type = c("Categorical", "Boolean", "Boolean",
                            "Categorical", "Boolean", "Boolean",
                            "Categorical"))
  return(ED)
}

createExposureDeterminantCombinationsTable <- function(riskCropLong) {
  EDC <- 
    unique(data.frame(idExposureDeterminantCombination = riskCropLong$idExposureDeterminantCombination,
                      Name = "A name",
                      Description = "A description",
                      apEq = riskCropLong$applicationEquipment,
                      mlBody = riskCropLong$mlBody != "None",
                      mlHands = riskCropLong$mlHands != "None",
                      mlHead = riskCropLong$mlHead,
                      apBody = riskCropLong$apBody != "None",
                      apHands = riskCropLong$apHands != "None",
                      apHead = riskCropLong$apHead
    ))
  EDC$Name <- paste0(EDC$apEq, 
                     ifelse(EDC$mlBody, ", ml protected body", ""),
                     ifelse(EDC$Hands, ", ml protected hands", ""),
                     ", ml ", EDC$mlHead,
                     ifelse(EDC$apBody, ", ap protected body", ""),
                     ifelse(EDC$apHands, ", ap protected hands", ""),
                     ", ap ", EDC$apHead)
  EDC$Description <- EDC$Name
  EDC <- EDC[order(EDC$idExposureDeterminantCombination), ]
  
  EDV <- reshape2::melt(EDC,
                        id.vars = c("idExposureDeterminantCombination", "Name", "Description"),
                        variable.name = "PropertyName",
                        value.name = "TextValue")
  EDV <- EDV[!EDV$TextValue %in% c("FALSE", "None"), ]
  return(list(EDC = EDC, EDV = EDV))
}

createExposureEstimatesTable <- function(riskCropLong) {
  EE <- unique(
    data.frame(idExposureScenario = riskCropLong$idExposureScenario,
               idExposureDeterminantCombination = riskCropLong$idExposureDeterminantCombination,
               ExposureSource = riskCropLong$applicationMethod,
               idSubstance = riskCropLong$idSubstanceNew,
               ExposureRoute = "Undefined",
               Value = riskCropLong$exposureEstimate,
               EstimateType = paste0("P", gsub("\\D", "", riskCropLong$exposure))))
  EE <- EE[order(EE$idExposureScenario, EE$idExposureDeterminantCombination), ]
  return(EE)
}

exportToExcel <- function(ExposureScenarios, 
                          ExposureDeterminants, 
                          ExposureDeterminantCombinations, 
                          ExposureEstimates, 
                          Substances, 
                          Populations,
                          file = "outputs/Single value non-dietary exposures OPEX.xlsx") {
  openxlsx::write.xlsx(list(ExposureScenarios = ExposureScenarios,
                            ExposureDeterminants = ExposureDeterminants,
                            ExposureDeterminantCombinations = ExposureDeterminantCombinations,
                            ExposureEstimates = ExposureEstimates,
                            Substances = Substances,
                            Populations = Populations),
                       file = file)
}

createOpexMCRATables <- function(productData, 
                                 cropData, 
                                 substanceData, 
                                 absorptionData, 
                                 selectedCropId, 
                                 selectedPerson) {
  risksCrop <- computeRisks(productData = productData,
                            cropData = cropData,
                            substanceData = substanceData,
                            absorptionData = absorptionData,
                            selectedCropId = selectedCropId,
                            selectedPerson = selectedPerson)
  scenariosCrop <- attr(risksCrop, which = "scenarios")
  
  riskCropLong <- convertRisksToLong(risksCrop = risksCrop,
                                     scenariosCrop = scenariosCrop)
  
  ## Create output tables for import in MCRA.
  Populations <- createPopulationsTable(selectedPerson = selectedPerson)
  Subst <- createSubstancesTable(riskCropLong = riskCropLong,
                                 scenariosCrop = scenariosCrop)
  riskCropLong <- Subst$riskCropLong
  Substances <- Subst$Substances
  
  ExposureScenarios <- createExposureScenariosTable(scenariosCrop = scenariosCrop,
                                                    selectedPerson = selectedPerson,
                                                    Populations = Populations)
  ExposureDeterminants <- createExposureDeterminantsTable()
  
  ExpDetComb <- createExposureDeterminantCombinationsTable(riskCropLong = riskCropLong)
  ExposureDeterminantCombinations <- ExpDetComb$EDC
  ExposureDeterminantValues <- ExpDetComb$EDV
  
  ExposureEstimates <- createExposureEstimatesTable(riskCropLong = riskCropLong)
  
  return(list(ExposureScenarios = ExposureScenarios,
              ExposureDeterminants = ExposureDeterminants,
              ExposureDeterminantCombinations = ExposureDeterminantCombinations,
              ExposureDeterminantValues = ExposureDeterminantValues,
              ExposureEstimates = ExposureEstimates))
}