using MCRA.Data.Management.RawDataProviders;
using MCRA.General;
using MCRA.General.Action.Settings;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;

namespace MCRA.Data.Management.Test.UnitTests.RawDataProviders {
    [TestClass]
    public class ActionRawDataProviderTests {

        /// <summary>
        /// Tests whether the project's scope keys filter is properly picked up by the
        /// action raw data provider. Scenario: no specified keys filter. Expected null.
        /// </summary>
        [TestMethod]
        public void ActionRawDataProvider_TestGetFilterCodes_NoSelection() {
            var project = new ProjectDto();
            var linkedDataSources = new Dictionary<SourceTableGroup, List<int>>();
            var rawDataProvider = new ActionRawDataProvider(
                project,
                linkedDataSources,
                new MockDataManagerFactory()
            );

            var filterCodes = rawDataProvider.GetFilterCodes(ScopingType.Populations);
            Assert.IsNull(filterCodes);
        }

        /// <summary>
        /// Tests whether the project's scope keys filter is properly picked up by the
        /// action raw data provider for all scoping types. Scenario: a defined scope 
        /// keys filter in the project. Expected: the same filter codes in the action 
        /// raw data provider.
        /// </summary>
        [TestMethod]
        public void ActionRawDataProvider_TestGetFilterCodes_Selection() {

            var scopingTypes = Enum.GetValues(typeof(ScopingType))
                .Cast<ScopingType>()
                .ToList();
            foreach (var scopingType in scopingTypes) {
                var project = new ProjectDto() {
                    ScopeKeysFilters = new List<ScopeKeysFilter>() {
                        new ScopeKeysFilter() {
                            ScopingType = scopingType,
                            SelectedCodes = new HashSet<string>() { "A", "B" }
                        }
                    }
                };
                var linkedDataSources = new Dictionary<SourceTableGroup, List<int>>();
                var rawDataProvider = new ActionRawDataProvider(
                    project,
                    linkedDataSources,
                new MockDataManagerFactory()
                );

                var filterCodes = rawDataProvider.GetFilterCodes(scopingType).ToArray();
                CollectionAssert.AreEquivalent(filterCodes, new[] { "A", "B" });
            }
        }

        /// <summary>
        /// Tests whether the scope selection is omitted for scopes of compute action types.
        /// </summary>
        [TestMethod]
        public void ActionRawDataProvider_TestGetFilterCodes_SkippedSelection() {
            var project = new ProjectDto() {
                ScopeKeysFilters = new List<ScopeKeysFilter>() {
                    new ScopeKeysFilter() {
                        ScopingType = ScopingType.Populations,
                        SelectedCodes = new HashSet<string>() { "A", "B" }
                    }
                }
            };
            var linkedDataSources = new Dictionary<SourceTableGroup, List<int>>();
            var rawDataProvider = new ActionRawDataProvider(
                project,
                linkedDataSources,
                new MockDataManagerFactory()
            );

            var filterCodes = rawDataProvider.GetFilterCodes(ScopingType.Populations).ToArray();
            CollectionAssert.AreEquivalent(filterCodes, new[] { "A", "B" });
        }
    }
}
