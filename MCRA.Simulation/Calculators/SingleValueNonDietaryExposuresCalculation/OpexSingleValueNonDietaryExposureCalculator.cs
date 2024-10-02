using System.Data;
using System.Reflection;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.OpexProductDefinitions;
using MCRA.General.OpexProductDefinitions.Dto;
using MCRA.General.TableDefinitions;
using MCRA.General.TableDefinitions.RawTableObjects;
using MCRA.Utils.DataFileReading;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.R.REngines;

namespace MCRA.Simulation.Calculators.SingleValueNonDietaryExposuresCalculation {

    public class OpexSingleValueNonDietaryExposureCalculator : ISingleValueNonDietaryExposureCalculator {
        /// <summary>
        /// Computes single value internal exposures.
        /// </summary>
        public ISingleValueNonDietaryExposure Compute(
            ICollection<Compound> substances,
            string codeConfig
        ) {
            var definitions = OpexProductDefinitions.Definitions;
            var opexProductDefinition = definitions[codeConfig];

            var productTable = new List<Product> { opexProductDefinition.Product }.ToDataTable();
            var cropsTable = opexProductDefinition.Crops.ToDataTable();
            var substancesTable = opexProductDefinition.Substances.ToDataTable();
            var absorptionsTable = opexProductDefinition.Absorptions.ToDataTable();

            DataTable dataTablePopulations = null;
            DataTable dataTableExposureScenarios = null;
            DataTable dataTableExposureDeterminants = null;
            DataTable dataTableExposureDeterminantValues = null;
            DataTable dataTableExposureDeterminantCombinations = null;
            DataTable dataTableExposureEstimates = null;
            using (var R = new RDotNetEngine()) {
                R.LoadLibrary("opex");

                R.SetSymbol("productData", productTable, stringsAsFactors: false);
                R.SetSymbol("cropData", cropsTable, stringsAsFactors: false);
                R.SetSymbol("substanceData", substancesTable, stringsAsFactors: false);
                R.SetSymbol("absorptionData", absorptionsTable, stringsAsFactors: false);

                R.EvaluateNoReturn("source('" + getOpexRScriptPath() + "')");
                R.EvaluateNoReturn("OpexTables <- createOpexMCRATables(productData, cropData, substanceData, absorptionData, selectedPersons)");

                dataTablePopulations = R.EvaluateDataTable("OpexTables$Populations");
                dataTableExposureScenarios = R.EvaluateDataTable("OpexTables$ExposureScenarios");
                dataTableExposureDeterminants = R.EvaluateDataTable("OpexTables$ExposureDeterminants");
                dataTableExposureDeterminantValues = R.EvaluateDataTable("OpexTables$ExposureDeterminantValues");
                dataTableExposureDeterminantCombinations = R.EvaluateDataTable("OpexTables$ExposureDeterminantCombinations");
                dataTableExposureEstimates = R.EvaluateDataTable("OpexTables$ExposureEstimates");
            }

            // Populations
            var rawPopulations = dataTablePopulations
                .CreateDataReader()
                .ReadRecords<RawPopulation>(McraTableDefinitions.Instance.TableDefinitions[RawDataSourceTableID.Populations]);
            var populations = rawPopulations
                .Select(r => new Population() {
                    Code = r.idPopulation,
                    Name = r.Name,
                    Description = r.Description,
                    Location = r.Location
                })
                .ToDictionary(r => r.Code, StringComparer.OrdinalIgnoreCase);

            // Exposure scenarios
            var rawExposureScenarios = dataTableExposureScenarios
                .CreateDataReader()
                .ReadRecords<RawExposureScenario>(McraTableDefinitions.Instance.TableDefinitions[RawDataSourceTableID.ExposureScenarios]);
            var exposureScenarios = rawExposureScenarios
                .Select(r => new ExposureScenario() {
                    Code = r.idExposureScenario,
                    Name = r.Name,
                    Description = r.Description,
                    Population = populations[r.idPopulation],
                    ExposureLevel = TargetLevelTypeConverter.FromString(r.ExposureLevel),
                    ExposureRoutes = !string.Equals(r.ExposureRoutes, "Undefined", StringComparison.OrdinalIgnoreCase) ? r.ExposureRoutes : "",
                    ExposureType = ExposureTypeConverter.FromString(r.ExposureType),
                    ExposureUnit = ExternalExposureUnitConverter.FromString(r.ExposureUnit)
                })
                .ToDictionary(r => r.Code, StringComparer.OrdinalIgnoreCase);

            // Exposure determinants
            var rawExposureDeterminants = dataTableExposureDeterminants
                .CreateDataReader()
                .ReadRecords<RawExposureDeterminant>(McraTableDefinitions.Instance.TableDefinitions[RawDataSourceTableID.ExposureDeterminants]);
            var exposureDeterminants = rawExposureDeterminants
                .Select(r => new ExposureDeterminant() {
                    Code = r.idExposureDeterminant,
                    Name = r.Name,
                    Description = r.Description,
                    PropertyType = IndividualPropertyTypeConverter.FromString(r.Type)
                })
                .ToDictionary(r => r.Code, StringComparer.OrdinalIgnoreCase);

            // Exposure determinant values
            var rawExposureDeterminantValues = dataTableExposureDeterminantValues
                .CreateDataReader()
                .ReadRecords<RawExposureDeterminantValue>(McraTableDefinitions.Instance.TableDefinitions[RawDataSourceTableID.ExposureDeterminantValues]);
            var groupedExposureDeterminantValues = rawExposureDeterminantValues
                .ToLookup(r => r.idExposureDeterminantCombination, StringComparer.OrdinalIgnoreCase);

            // Exposure determinant combinations
            var rawExposureDeterminantCombinations = dataTableExposureDeterminantCombinations
                .CreateDataReader()
                .ReadRecords<RawExposureDeterminantCombination>(McraTableDefinitions.Instance.TableDefinitions[RawDataSourceTableID.ExposureDeterminantCombinations]);
            var exposureDeterminantCombinations = rawExposureDeterminantCombinations
                .Select(r => new ExposureDeterminantCombination() {
                    Code = r.idExposureDeterminantCombination,
                    Name = r.Name,
                    Description = r.Description,
                    Properties = groupedExposureDeterminantValues.Contains(r.idExposureDeterminantCombination)
                        ? groupedExposureDeterminantValues[r.idExposureDeterminantCombination]
                            .Select(v => new ExposureDeterminantValue() {
                                Property = exposureDeterminants[v.PropertyName],
                                DoubleValue = v.DoubleValue,
                                TextValue = v.TextValue
                            })
                            .ToDictionary(r => r.Property.Name, StringComparer.OrdinalIgnoreCase)
                        : new Dictionary<string, ExposureDeterminantValue>()
                })
                .ToDictionary(r => r.Code, StringComparer.OrdinalIgnoreCase);

            // Exposure determinant estimates
            var rawExposureEstimates = dataTableExposureEstimates
                .CreateDataReader()
                .ReadRecords<RawExposureEstimate>(McraTableDefinitions.Instance.TableDefinitions[RawDataSourceTableID.ExposureEstimates])
                .ToList();
            var exposureEstimates = rawExposureEstimates
                .Select(r => new ExposureEstimate() {
                    ExposureScenario = exposureScenarios[r.idExposureScenario],
                    ExposureDeterminantCombination = exposureDeterminantCombinations[r.idExposureDeterminantCombination],
                    ExposureSource = r.ExposureSource,
                    Substance = substances.First(),
                    ExposureRoute = ExposureRouteConverter.FromString(r.ExposureRoute),
                    Value = r.Value,
                    EstimateType = r.EstimateType,
                })
                .ToList();

            return new SingleValueNonDietaryExposure {
                SingleValueNonDietaryExposureScenarios = exposureScenarios,
                SingleValueNonDietaryExposureDeterminantCombinations = exposureDeterminantCombinations,
                SingleValueNonDietaryExposureEstimates = exposureEstimates
            };
        }

        private string getOpexRScriptPath() {
            var location = Assembly.GetExecutingAssembly().Location;
            var assemblyFolder = new FileInfo(location).Directory.FullName;
            var RPath = Path.Combine(assemblyFolder, "Resources/Opex/OpexToMcra.R");
            //convert backslashes to / explicitly, path is used in R script
            return RPath.Replace(@"\", "/");
        }
    }
}
