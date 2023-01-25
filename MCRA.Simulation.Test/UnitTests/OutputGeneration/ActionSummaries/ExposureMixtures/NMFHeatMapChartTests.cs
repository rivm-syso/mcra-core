using System.Collections.Generic;
using System.Linq;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.OutputGeneration.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.ExposureMixtures {
    /// <summary>
    /// OutputGeneration, ActionSummaries, ExposureMixtures
    /// </summary>
    [TestClass]
    public class NMFHeatMapChartTests : ChartCreatorTestBase {
        /// <summary>
        /// Create charts and test MixtureExposureSection view
        /// </summary>
        [TestMethod]
        public void NMFHeatMapChart_Test1() {

            var components = new List<List<SubstanceComponentRecord>>() { };
            components.Add(new List<SubstanceComponentRecord>(){
                new SubstanceComponentRecord(){SubstanceCode = "a", SubstanceName = "A", NmfValue = .6},
                new SubstanceComponentRecord(){SubstanceCode = "b", SubstanceName = "B", NmfValue = .3},
                new SubstanceComponentRecord(){SubstanceCode = "c", SubstanceName = "C", NmfValue = .1},
                new SubstanceComponentRecord(){SubstanceCode = "d", SubstanceName = "D", NmfValue = 0},
                new SubstanceComponentRecord(){SubstanceCode = "e", SubstanceName = "E", NmfValue = 0},
                new SubstanceComponentRecord(){SubstanceCode = "f", SubstanceName = "F", NmfValue = 0},
                new SubstanceComponentRecord(){SubstanceCode = "g", SubstanceName = "G", NmfValue = 0},
            });
            components.Add(new List<SubstanceComponentRecord>(){
                new SubstanceComponentRecord(){SubstanceCode = "a", SubstanceName = "A", NmfValue = 0},
                new SubstanceComponentRecord(){SubstanceCode = "b", SubstanceName = "B", NmfValue = 0},
                new SubstanceComponentRecord(){SubstanceCode = "c", SubstanceName = "C", NmfValue = .5},
                new SubstanceComponentRecord(){SubstanceCode = "d", SubstanceName = "D", NmfValue = .3},
                new SubstanceComponentRecord(){SubstanceCode = "e", SubstanceName = "E", NmfValue = .2},
                new SubstanceComponentRecord(){SubstanceCode = "f", SubstanceName = "F", NmfValue = 0},
                new SubstanceComponentRecord(){SubstanceCode = "g", SubstanceName = "G", NmfValue = 0},
            });
            components.Add(new List<SubstanceComponentRecord>(){
                new SubstanceComponentRecord(){SubstanceCode = "a", SubstanceName = "A", NmfValue = 0},
                new SubstanceComponentRecord(){SubstanceCode = "b", SubstanceName = "B", NmfValue = 0},
                new SubstanceComponentRecord(){SubstanceCode = "c", SubstanceName = "C", NmfValue = 0},
                new SubstanceComponentRecord(){SubstanceCode = "d", SubstanceName = "D", NmfValue = 0},
                new SubstanceComponentRecord(){SubstanceCode = "e", SubstanceName = "E", NmfValue = .4},
                new SubstanceComponentRecord(){SubstanceCode = "f", SubstanceName = "F", NmfValue = .31},
                new SubstanceComponentRecord(){SubstanceCode = "g", SubstanceName = "G", NmfValue = .29},
            });


            var section = new ComponentSelectionOverviewSection() {
                SubstanceComponentRecords = components,
                SortedSubstancesComponentRecords = components.First().ToList(),
                Records = new List<ComponentRecord>() {
                    new ComponentRecord() { ComponentNumber = 1, Variance = 40 },
                    new ComponentRecord() { ComponentNumber = 2, Variance = 30 },
                    new ComponentRecord() { ComponentNumber = 3, Variance = 20 },
                    new ComponentRecord() { ComponentNumber = 4, Variance = 10 }
                },
            };

            var chart = new NMFHeatMapChartCreator(section);
            TestRender(chart, $"TestCreate", ChartFileType.Png);
            AssertIsValidView(section);
        }

        /// <summary>
        /// Create charts and test MixtureExposureSection view
        /// </summary>
        [TestMethod]
        public void NMFHeatMapChart_Test2() {

            var components = new List<List<SubstanceComponentRecord>>() { };
            components.Add(new List<SubstanceComponentRecord>(){
                new SubstanceComponentRecord(){SubstanceCode = "a", SubstanceName = "A", NmfValue = .4},
                new SubstanceComponentRecord(){SubstanceCode = "b", SubstanceName = "B", NmfValue = .3},
                new SubstanceComponentRecord(){SubstanceCode = "c", SubstanceName = "C", NmfValue = .2},
                new SubstanceComponentRecord(){SubstanceCode = "d", SubstanceName = "D", NmfValue = .1},
            });

            var section = new ComponentSelectionOverviewSection() {
                SubstanceComponentRecords = components,
                SortedSubstancesComponentRecords = components.First().ToList(),
                Records = new List<ComponentRecord>() {
                    new ComponentRecord() { ComponentNumber = 1, Variance = 40 },
                },
            };

            var chart = new NMFSingleHeatMapChartCreator(section);
            TestRender(chart, $"TestCreate2", ChartFileType.Png);
        }
    }
}
