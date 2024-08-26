using MCRA.Utils.Xml;
using MCRA.General.Action.Settings;
using MCRA.General.ModuleDefinitions.Settings;
using System.Data.Common;

namespace MCRA.General.Action.Serialization {
    public static class ProjectSettingsSerializer {

        /// <summary>
        /// Loads the project settings from the xml file and, if needed for old projects,
        /// applies certain corrections to the settings that depend on the data source
        /// configuration.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="dataSourceConfiguration"></param>
        /// <param name="oldStyle"></param>
        /// <param name="isModified"></param>
        /// <returns></returns>
        public static ProjectDto ImportFromXmlFile(
            string fileName,
            DataSourceConfiguration dataSourceConfiguration,
            bool oldStyle,
            out bool isModified
        ) {
            using (var fs = new StreamReader(fileName)) {
                var xmlString = fs.ReadToEnd();
                return ImportFromXmlString(xmlString, dataSourceConfiguration, oldStyle, out isModified);
            }
        }

        /// <summary>
        /// Loads the project settings from the compressed settings xml and, if needed for old
        /// projects, applies certain corrections to the settings that depend on the data source
        /// configuration.
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="dataSourceConfiguration"></param>
        /// <param name="oldStyle">Should be true when loading an MCRA 8 style settings xml.</param>
        /// <param name="isModified"></param>
        /// <returns></returns>
        public static ProjectDto ImportFromCompressedXml(
            byte[] bytes,
            DataSourceConfiguration dataSourceConfiguration,
            bool oldStyle,
            out bool isModified
        ) {
            var xml = XmlSerialization.UncompressBytes(bytes);
            return ImportFromXmlString(xml, dataSourceConfiguration, oldStyle, out isModified);
        }

        /// <summary>
        /// Loads the project settings from the settings xml and, if needed for old projects,
        /// applies certain corrections to the settings that depend on the data source
        /// configuration.
        /// </summary>
        /// <param name="settingsXml"></param>
        /// <param name="dataSourceConfiguration"></param>
        /// <param name="oldStyle">Should be true when loading an MCRA 8 style settings xml.</param>
        /// <param name="isModified"></param>
        /// <returns></returns>
        public static ProjectDto ImportFromXmlString(
            string settingsXml,
            DataSourceConfiguration dataSourceConfiguration,
            bool oldStyle,
            out bool isModified
        ) {
            isModified = false;

            // Deserialize project settings
            // load the settings XML string using the ProjectSettingsSerializer: during loading
            // the necessary XSLT transformations will be applied for backward compatibility of any
            // changes in the settings from previous versions
            var projectSettings = deserialize(settingsXml);

            // Apply MCRA 8 style projects corrections based on data source configuration.
            if (oldStyle) {
                applyOldStyleCorrections(dataSourceConfiguration, projectSettings);
                isModified = true;
            }

            // Do any necessary adjustments to the settings based on version of loaded settings here.
            if (applyPreviousVersionCorrections(dataSourceConfiguration, projectSettings)) {
                isModified = true;
            }

            projectSettings.McraVersion.SetCurrentVersionData();
            projectSettings.ProjectDataSourceVersions ??= dataSourceConfiguration?.ToVersionsDictionary();

            return projectSettings;
        }

        /// <summary>
        /// Exports the project settings to an xml string.
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static string ExportToXmlString(ProjectDto settings, bool format = false) {
            settings.McraVersion.SetCurrentVersionData();
            return settings.ToXml(format);
        }

        /// <summary>
        /// Exports the project settings to compressed xml.
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static byte[] ExportToCompressedXml(ProjectDto settings) {
            settings.McraVersion.SetCurrentVersionData();
            return settings.ToCompressedXml();
        }

        /// <summary>
        /// Exports the project settings to compressed xml.
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="filename"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static void ExportToXmlFile(ProjectDto settings, string filename, bool format = false) {
            settings.McraVersion.SetCurrentVersionData();
            var xml = ExportToXmlString(settings, format);
            File.WriteAllText(filename, xml);
        }

        /// <summary>
        /// Deserializes the project settings xml string and applies all the project
        /// settings transforms which have a version number that is higher than
        /// the version of the XML file
        /// The transforms should only contain settings transformations that are necessary to
        /// correctly load the DTO from xml, for example when settings have a new name or
        /// if they have been moved to another element
        /// </summary>
        /// <param name="xmlString"></param>
        /// <returns>a <see cref="ProjectDto"/> instance from the patched and deserialized settings</returns>
        private static ProjectDto deserialize(string xmlString) {
            //first get the version in the XML
            //use the simple base class
            var xmlVersion = XmlSerialization.FromXml<ProjectVersionInfo>(xmlString).McraVersion;

            //check whether we have a previous version, also check the revision level
            //in production environments this is always 0
            var revision = int.Parse(ThisAssembly.Git.Commits);
            if (xmlVersion.IsPreviousVersion || xmlVersion.Revision < revision) {
                var thisType = typeof(ProjectSettingsSerializer);
                //get the list of transforms in descending order, last one first
                //cast the version number of the file name into an integer for easy comparison
                //order the files with the highest version first (descending order)
                var transforms = thisType.Assembly.GetManifestResourceNames()
                    .Where(n => n.EndsWith(".xslt"))
                    .OrderByDescending(n => n)
                    .Select(n => (
                        Name: n,
                        //version part of the resource name string cast to an array of int
                        //first split by hyphen '-' and then by dot '.'
                        //Take first 3 because 2nd array contains also the extension 'xslt'
                        Ver: n.Split('-')[1].Split('.').Take(3)
                              .Select(v => int.Parse(v))
                              .ToArray()
                    ))
                    //pack the version parts into one 32bit integer (max supported version 255.255.65535)
                    .Select(v => (v.Name, VersionValue: v.Ver[0] << 24 | v.Ver[1] << 16 | v.Ver[2]))
                    .ToList();

                //also cast the version of the file to deserialize to an integer
                var xmlVersionValue = (uint)xmlVersion.Major << 24 | (uint)xmlVersion.Minor << 16 | (uint)xmlVersion.Build;

                //use a stack of transforms so last one added gets applied first
                var transformsToApply = new Stack<string>();
                foreach (var (Name, VersionValue) in transforms) {
                    //Check whether the transform should be applied
                    if (VersionValue > xmlVersionValue) {
                        transformsToApply.Push(Name);
                    } else {
                        //don't apply any more transforms when we reach the
                        //version of the xml file
                        break;
                    }
                }

                //now pop the transforms and apply them in chronological order
                while (transformsToApply.Count > 0) {
                    var xsltResName = transformsToApply.Pop();
                    string xslTransform = string.Empty;

                    // Load xsl transform file (embedded)
                    using (var stream = thisType.Assembly.GetManifestResourceStream(xsltResName)) {
                        using (var sr = new StreamReader(stream)) {
                            xslTransform = sr.ReadToEnd();
                        }
                    }
                    //Apply the transform and save to the xmlString variable
                    xmlString = XmlSerialization.TransformXmlStringWithXslString(xmlString, xslTransform);
                }
            }
            // The returned DTO should still contain the MCRA version info it was saved with previously
            // Load the DTO from the XML, using the transform file
            return XmlSerialization.FromXml<ProjectDto>(xmlString);
        }

        /// <summary>
        /// Apply corrections to the settings of previous versions.
        /// </summary>
        /// <param name="projectSettings"></param>
        private static bool applyPreviousVersionCorrections(
            DataSourceConfiguration dataSourceConfiguration,
            ProjectDto projectSettings
        ) {
            var changed = false;
            if (projectSettings.McraVersion.IsPreviousVersion) {
                if (!projectSettings.McraVersion.CheckMinimalVersionNumber(9, 1)) {
                    projectSettings.CalculationActionTypes = projectSettings.CalculationActionTypes ?? new HashSet<ActionType>();
                    projectSettings.CalculationActionTypes.Add(ActionType.HazardCharacterisations);
                    projectSettings.CalculationActionTypes.Add(ActionType.OccurrenceFrequencies);

                    var selectedSubstancesCount = projectSettings
                        .ScopeKeysFilters
                        ?.FirstOrDefault(r => r.ScopingType == ScopingType.Compounds)
                        ?.SelectedCodes
                        ?.Count ?? 0;

                    projectSettings.SubstancesSettings.MultipleSubstances = selectedSubstancesCount != 1;

                    changed = true;
                }
                if (!projectSettings.McraVersion.CheckMinimalVersionNumber(9, 1, 20)) {
                    if (projectSettings.DietaryExposuresSettings.TotalDietStudy) {
                        var tdsDataSource = dataSourceConfiguration?.DataSourceMappingRecords?
                            .FirstOrDefault(r => r.SourceTableGroup == SourceTableGroup.TotalDietStudy);
                        if (tdsDataSource != null
                            && !dataSourceConfiguration.HasDataGroup(SourceTableGroup.ConcentrationDistributions)
                        ) {
                            var concentrationDistributionsMapping = tdsDataSource.Clone();
                            concentrationDistributionsMapping.SourceTableGroup = SourceTableGroup.ConcentrationDistributions;
                            dataSourceConfiguration.DataSourceMappingRecords.Add(concentrationDistributionsMapping);
                        }
                    }
                }
                if (!projectSettings.McraVersion.CheckMinimalVersionNumber(9, 1, 32, 74)) {
                    // If old projects have no populations data source, then set it to compute
                    var hasPopulationsDataSource = dataSourceConfiguration?.DataSourceMappingRecords?
                        .Any(r => r.SourceTableGroup == SourceTableGroup.Populations) ?? false;

                    if (!hasPopulationsDataSource) {
                        projectSettings.CalculationActionTypes.Add(ActionType.Populations);
                        changed = true;
                    }
                }
                if (!projectSettings.McraVersion.CheckMinimalVersionNumber(10, 1, 1)) {
                    // For MCRA version < 10.1.1, the kinetic models data group was used for kinetic
                    // conversion factors and PBK models. Add the data source config for PBK models
                    // so that when the action was using PBK models, it can still find the instances.
                    var kineticModelsDataSource = dataSourceConfiguration?.DataSourceMappingRecords?
                        .FirstOrDefault(r => r.SourceTableGroup == SourceTableGroup.KineticModels);
                    if (kineticModelsDataSource != null
                        && !dataSourceConfiguration.HasDataGroup(SourceTableGroup.PbkModels)
                    ) {
                        var pbkModelsDataSourceMapping = kineticModelsDataSource.Clone();
                        pbkModelsDataSourceMapping.SourceTableGroup = SourceTableGroup.PbkModels;
                        dataSourceConfiguration.DataSourceMappingRecords.Add(pbkModelsDataSourceMapping);
                        changed = true;
                    }
                }
            }
            return changed;
        }

        /// <summary>
        /// MCRA 8 style projects corrections based on data source configuration:
        /// NOTE: ONLY hard code any necessary settings conversions for MCRA 8 - style
        /// settings which can only be derived from the data source configuration file
        /// Any other settings transformations MUST be added to the XSLT transform file
        /// MCRA.General.Action.Settings.Serialization.ProjectSettingsTransform.xslt
        /// which is applied to both old and new style (8 and 9) project settings
        /// </summary>
        /// <param name="dataSourceConfiguration"></param>
        /// <param name="projectSettings"></param>
        private static void applyOldStyleCorrections(
            DataSourceConfiguration dataSourceConfiguration,
            ProjectDto projectSettings
        ) {
            if (projectSettings.CalculationActionTypes == null) {
                projectSettings.CalculationActionTypes = new HashSet<ActionType>();
            }
            // Correct IsProcessing default set in project that has no processing data
            if (!dataSourceConfiguration.HasDataGroup(SourceTableGroup.Processing)) {
                projectSettings.ProcessingFactorsSettings.IsProcessing = false;
                projectSettings.FoodConversionsSettings.UseProcessing = false;
            }
            // Correct unit variability default set in project that has no unit variability data
            if (!dataSourceConfiguration.HasDataGroup(SourceTableGroup.UnitVariabilityFactors)) {
                projectSettings.DietaryExposuresSettings.UseUnitVariability = false;
                projectSettings.SingleValueDietaryExposuresSettings.UseUnitVariability = false;
            }
            // When HazardDoses table is available, use RPF calculation
            if (dataSourceConfiguration.HasDataGroup(SourceTableGroup.HazardDoses)) {
                projectSettings.CalculationActionTypes.Add(ActionType.RelativePotencyFactors);
                projectSettings.CalculationActionTypes.Add(ActionType.ActiveSubstances);
                projectSettings.ActiveSubstancesSettings.FilterByAvailableHazardDose = true;
            }
            // When there is no food recipes table available, uncheck the food conversion step 3a
            if (!dataSourceConfiguration.HasDataGroup(SourceTableGroup.FoodTranslations) &&
                projectSettings.FoodConversionsSettings.UseComposition
            ) {
                projectSettings.FoodConversionsSettings.UseComposition = false;
            }
            // When there is no agricultural use table available, and we want to use occurrence patterns
            // add the calculation action type
            if (!dataSourceConfiguration.HasDataGroup(SourceTableGroup.AgriculturalUse) &&
                projectSettings.ConcentrationModelsSettings.UseAgriculturalUseTable
            ) {
                projectSettings.CalculationActionTypes.Add(ActionType.OccurrencePatterns);
            }
            // When there are market shares, check the use market shares option
            projectSettings.FoodConversionsSettings.UseMarketShares = dataSourceConfiguration.HasDataGroup(SourceTableGroup.MarketShares);

            // when there are Relative Potency Factors available, but no points of departure (hazard doses)
            if (dataSourceConfiguration.HasDataGroup(SourceTableGroup.RelativePotencyFactors)
                && !dataSourceConfiguration.HasDataGroup(SourceTableGroup.HazardDoses)) {
                projectSettings.CalculationActionTypes.Add(ActionType.ActiveSubstances);
                projectSettings.ActiveSubstancesSettings.FilterByAvailableHazardDose = false;
            }
        }
    }
}
