using System.Data;
using System.IO.Compression;
using MCRA.Data.Compiled;
using MCRA.Data.Raw;
using MCRA.Utils.DataFileReading;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Data.Management.CompiledDataManagers {

    /// <summary>
    /// CompiledDataManager provides all raw data as 'compiled' data to the upper layers of the system
    /// This is the main class' source code file, the other files that constitute partial class are
    /// located in the 'CompiledDataManagerParts' subfolder, one file per table group
    /// </summary>
    public partial class CompiledDataManager : CompiledLinkManager, ICompiledDataManager {

        private const string _sep = "\a";

        private readonly CompiledData _data;

        /// <summary>
        /// Instantiate with a raw data provider.
        /// </summary>
        /// <param name="rawDataProvider"></param>
        public CompiledDataManager(IRawDataProvider rawDataProvider, IEnumerable<string> skipScopingTypes = null) : base(rawDataProvider, skipScopingTypes) {
            _data = new CompiledData();
        }

        /// <summary>
        /// Write the data of this instance to a zipped CSV file
        /// </summary>
        /// <param name="filename"></param>
        /// <returns>true if succeeded</returns>
        public bool WriteToZippedCsvFile(string filename) {
            try {
                WriteDataToZippedCsv(_data, filename);
                return true;
            } catch (Exception) {
                return false;
            }
        }

        /// <summary>
        /// Writes the compiled data to a zipped CSV file
        /// </summary>
        /// <param name="data"></param>
        /// <param name="fileName"></param>
        public static void WriteDataToZippedCsv(CompiledData data, string fileName) {
            try {
                var tmpCsvFolder = Path.Combine(Path.GetTempPath(), $"MCRACompiledData{DateTime.Now:yyyyMMddHHmmssff}");
                Directory.CreateDirectory(tmpCsvFolder);

                //add the files for the data in the collection
                WriteDataToCsvFiles(data, tmpCsvFolder);

                //create the zip file
                if (File.Exists(fileName)) {
                    File.Delete(fileName);
                }
                ZipFile.CreateFromDirectory(tmpCsvFolder, fileName, CompressionLevel.Optimal, false);

            } catch (Exception) {
                throw;
            }
        }

        /// <summary>
        /// Writes the compiled data to CSV files in the specified folder
        /// </summary>
        /// <param name="data"></param>
        /// <param name="folderName"></param>
        public static void WriteDataToCsvFiles(CompiledData data, string folderName) {
            try {
                if (data == null) {
                    return;
                }

                var dirInfo = new DirectoryInfo(folderName);
                if (!dirInfo.Exists) {
                    dirInfo.Create();
                }

                //add the files for the data in the collection
                writeAdverseOutcomePathwayNetworksDataToCsv(folderName, data.AllAdverseOutcomePathwayNetworks?.Values);
                writeOccurrencePatternDataToCsv(folderName, data.AllOccurrencePatterns);
                writeAdditionalSamplePropertiesToCsv(folderName, data.AllAdditionalSampleProperties?.Values);
                writeAnalyticalMethodsToCsv(folderName, data.AllAnalyticalMethods?.Values);
                writeActiveSubstanceDataToCsv(folderName, data.AllActiveSubstanceModels?.Values);
                writeCompoundsDataToCsv(folderName, data.AllSubstances?.Values);
                writeConcentrationDistributionsDataToCsv(folderName, data.AllConcentrationDistributions);
                writeConcentrationSingleValuesToCsv(folderName, data.AllConcentrationSingleValues);
                writeConsumptionDataToCsv(folderName, data.AllFoodConsumptions);
                writePopulationConsumptionSingleValuesToCsv(folderName, data.AllPopulationConsumptionSingleValues);
                writeDietaryExposureModelsToCsv(folderName, data.AllDietaryExposureModels?.Values);
                writeDoseResponseDataToCsv(folderName, data.AllDoseResponseExperiments?.Values);
                writeDoseResponseModelsToCsv(folderName, data.AllDoseResponseModels?.Values);
                writeEffectRepresentationsToCsv(folderName, data.AllEffectRepresentations);
                writeEffectsDataToCsv(folderName, data.AllEffects?.Values);
                writeFoodSampleCompositionsToCsv(folderName, data.AllTDSFoodSampleCompositions);
                writeFoodsDataToCsv(folderName, data.AllFoods?.Values);
                writeFacetDescriptorsDataToCsv(folderName, data.AllFacetDescriptors);
                writeFoodSurveyDataToCsv(folderName, data.AllFoodSurveys?.Values);
                writeFoodTranslationDataToCsv(folderName, data.AllFoodTranslations);
                writePointsOfDepartureDataToCsv(folderName, data.AllPointsOfDeparture);
                writeHazardCharacterisationsToCsv(folderName, data.AllHazardCharacterisations);
                writeHumanMonitoringSampleDataToCsv(folderName, data.AllHumanMonitoringSamples?.Values);
                writeHumanMonitoringSurveyDataToCsv(folderName, data.AllHumanMonitoringSurveys?.Values);
                writeIestiSpecialCasesDataToCsv(folderName, data.AllIestiSpecialCases);
                writeIndividualsDataToCsv(folderName, data.AllIndividuals?.Values);
                writeInterSpeciesFactorDataToCsv(folderName, data.AllInterSpeciesFactors);
                writeIntraSpeciesFactorDataToCsv(folderName, data.AllIntraSpeciesFactors);
                writeAbsorptionFactorDataToCsv(folderName, data.AllAbsorptionFactors);
                writeKineticConversionFactorDataToCsv(folderName, data.AllKineticConversionFactors);
                writePbkModelDataToCsv(folderName, data.AllKineticModelInstances);
                writeMaximumConcentrationLimitDataToCsv(folderName, data.AllMaximumConcentrationLimits);
                writeMolecularDockingModelDataToCsv(folderName, data.AllMolecularDockingModels?.Values);
                writeNonDietaryDataToCsv(folderName, data.NonDietaryExposureSets);
                writeOccurrenceFrequenciesDataToCsv(folderName, data.AllOccurrenceFrequencies);
                writeProcessingTypesDataToCsv(folderName, data.AllProcessingTypes?.Values);
                writeProcessingFactorsDataToCsv(folderName, data.AllProcessingFactors);
                writePopulationDataToCsv(folderName, data.AllPopulations?.Values);
                writeQsarMembershipModelDataToCsv(folderName, data.AllQsarMembershipModels?.Values);
                writeReadAcrossFoodTranslationsDataToCsv(folderName, data.AllFoodExtrapolations);
                writeRelativePotencyFactorsDataToCsv(folderName, data.AllRelativePotencyFactors?.SelectMany(f => f.Value));
                writeSubstanceConversionsDataToCsv(folderName, data.AllSubstanceConversions);
                writeDeterministicSubstanceConversionFactorsDataToCsv(folderName, data.AllDeterministicSubstanceConversionFactors);
                writeResponsesDataToCsv(folderName, data.AllResponses?.Values);
                writeRiskModelsToCsv(folderName, data.AllRiskModels?.Values);
                writeFoodSamplesToCsv(folderName, data.AllFoodSamples?.Values);
                writeTargetExposureModelsToCsv(folderName, data.AllTargetExposureModels?.Values);
                writeTestSystemsDataToCsv(folderName, data.AllTestSystems?.Values);
                writeUnitVariabilityFactorsDataToCsv(folderName, data.AllUnitVariabilityFactors);
                writeMarketSharesDataToCsv(folderName, data.AllMarketShares);
                writeSubstanceAuthorisationsDataToCsv(folderName, data.AllSubstanceAuthorisations);
                writeSubstanceApprovalsDataToCsv(folderName, data.AllSubstanceApprovals);
                writeExposureBiomarkerConversionsDataToCsv(folderName, data.AllExposureBiomarkerConversions);
                writeSingleValueNonDietaryExposuresToCsv(folderName, data.AllSingleValueNonDietaryExposureEstimates);
                writeDustConcentrationDistributionsToCsv(folderName, data.AllDustConcentrationDistributions);
                writeDustIngestionsToCsv(folderName, data.AllDustIngestions);
                writeDustBodyExposureFractionsToCsv(folderName, data.AllDustBodyExposureFractions);
                writeDustAdherenceAmountsToCsv(folderName, data.AllDustAdherenceAmounts);
                writeDustAvailabilityFractionsToCsv(folderName, data.AllDustAvailabilityFractions);
                writeExposureEffectFunctionsDataToCsv(folderName, data.AllExposureEffectFunctions);
                writePbkModelDefinitionDataToCsv(folderName, data.AllPbkModelDefinitions?.Values);
                writeSoilConcentrationDistributionsToCsv(folderName, data.AllSoilConcentrationDistributions);
                writeSoilIngestionsToCsv(folderName, data.AllSoilIngestions);
                writeBaselineBodIndicatorsDataToCsv(folderName, data.AllBaselineBodIndicators);
            } catch (Exception) {
                throw;
            }
        }

        private static void writeToCsv(string tempCsvFolder, TableDefinition tableDef, DataTable table, int[] columnValueCounts = null) {
            //don't write empty tables here
            if (table.Rows.Count == 0) {
                return;
            }

            //remove empty columns from DataTable, that is columns with a zero count
            if (columnValueCounts != null) {
                for (int i = columnValueCounts.Length - 1; i >= 0; i--) {
                    if (columnValueCounts[i] == 0) {
                        table.Columns.RemoveAt(i);
                    }
                }
            }

            var fileName = Path.Combine(tempCsvFolder, $"{tableDef.Id}.csv");
            table.ToCsv(fileName);
        }
    }
}
