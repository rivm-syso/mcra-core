using MCRA.General;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    [TestClass]
    public class DataLinkingActiveSubstancesTests : LinkTestsBase {
        [TestMethod]
        public void DataLinkingActiveSubstancesSimpleTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.ActiveSubstancesModels, @"AssessmentGroupMembershipsTests/AssessmentGroupMembershipModels")
            );

            _compiledLinkManager.LoadScope(SourceTableGroup.AssessmentGroupMemberships);

            var scope = _compiledLinkManager.GetCodesInScope(ScopingType.ActiveSubstancesModels);
            CollectionAssert.AreEqual(new[] { "Agm1", "Agm2" }, scope.ToArray());

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.AssessmentGroupMemberships);
            AssertDataReadingSummaryRecord(report, ScopingType.ActiveSubstancesModels, 2, "Agm1,Agm2", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.ActiveSubstancesModels, ScopingType.Effects, 2, "eff1,eff2", "", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Effects);
            AssertDataReadingSummaryRecord(report, ScopingType.Effects, 0, "", "", "eff1,eff2");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Compounds);
        }


        [TestMethod]
        public void DataLinkingActiveSubstancesSimpleEffectsFilterTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.ActiveSubstancesModels, @"AssessmentGroupMembershipsTests/AssessmentGroupMembershipModels")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Effects, ["eff2"]);

            _compiledLinkManager.LoadScope(SourceTableGroup.AssessmentGroupMemberships);

            var scope = _compiledLinkManager.GetCodesInScope(ScopingType.ActiveSubstancesModels);
            CollectionAssert.AreEqual(new[] { "Agm2" }, scope.ToArray());

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.AssessmentGroupMemberships);
            AssertDataReadingSummaryRecord(report, ScopingType.ActiveSubstancesModels, 2, "Agm2", "Agm1", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.ActiveSubstancesModels, ScopingType.Effects, 2, "eff2", "eff1", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Effects);
            AssertDataReadingSummaryRecord(report, ScopingType.Effects, 0, "", "", "eff2");
        }

        [TestMethod]
        public void DataLinkingActiveSubstancesSimpleTest1() {
            _rawDataProvider.SetDataTables(
                (ScopingType.ActiveSubstancesModels, @"AssessmentGroupMembershipsTests/AssessmentGroupMembershipModels"),
                (ScopingType.ActiveSubstances, @"AssessmentGroupMembershipsTests/AssessmentGroupMemberships")
            );

            _compiledLinkManager.LoadScope(SourceTableGroup.AssessmentGroupMemberships);

            var scope = _compiledLinkManager.GetCodesInScope(ScopingType.ActiveSubstancesModels);
            var allEntities = _compiledLinkManager.GetAllSourceEntities(ScopingType.ActiveSubstancesModels);
            CollectionAssert.AreEqual(new[] { "Agm1", "Agm2" }, scope.ToArray());

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.AssessmentGroupMemberships);
            AssertDataReadingSummaryRecord(report, ScopingType.ActiveSubstancesModels, 2, "Agm1,Agm2", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.ActiveSubstancesModels, ScopingType.Effects, 2, "eff1,eff2", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.ActiveSubstances, ScopingType.ActiveSubstancesModels, 4, "Agm1,Agm2", "Agm3,Agm4", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.ActiveSubstances, ScopingType.Compounds, 5, "a,b,c,d,e", "", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Effects);
            AssertDataReadingSummaryRecord(report, ScopingType.Effects, 0, "", "", "eff1,eff2");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Compounds);
            AssertDataReadingSummaryRecord(report, ScopingType.Compounds, 0, "", "", "a,b,c,d,e");
        }

        [TestMethod]
        public void DataLinkingActiveSubstancesEffectsFilterTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.ActiveSubstancesModels, @"AssessmentGroupMembershipsTests/AssessmentGroupMembershipModels"),
                (ScopingType.ActiveSubstances, @"AssessmentGroupMembershipsTests/AssessmentGroupMemberships")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Effects, ["eff2"]);

            _compiledLinkManager.LoadScope(SourceTableGroup.AssessmentGroupMemberships);

            var scope = _compiledLinkManager.GetCodesInScope(ScopingType.ActiveSubstancesModels);
            CollectionAssert.AreEqual(new[] { "Agm2" }, scope.ToArray());

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.AssessmentGroupMemberships);
            AssertDataReadingSummaryRecord(report, ScopingType.ActiveSubstancesModels, 2, "Agm2", "Agm1", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.ActiveSubstancesModels, ScopingType.Effects, 2, "eff2", "eff1", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.ActiveSubstances, ScopingType.ActiveSubstancesModels, 4, "Agm2", "Agm1,Agm3,Agm4", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.ActiveSubstances, ScopingType.Compounds, 5, "a,b,c,e", "d", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Effects);
            AssertDataReadingSummaryRecord(report, ScopingType.Effects, 0, "", "", "eff2");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Compounds);
            AssertDataReadingSummaryRecord(report, ScopingType.Compounds, 0, "", "", "a,b,c,e");
        }

        [TestMethod]
        public void DataLinkingActiveSubstancesEffectsCompoundsFilterTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.ActiveSubstancesModels, @"AssessmentGroupMembershipsTests/AssessmentGroupMembershipModels"),
                (ScopingType.ActiveSubstances, @"AssessmentGroupMembershipsTests/AssessmentGroupMemberships")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Effects, ["eff2"]);
            _rawDataProvider.SetFilterCodes(ScopingType.Compounds, ["b", "c"]);

            _compiledLinkManager.LoadScope(SourceTableGroup.AssessmentGroupMemberships);

            var scope = _compiledLinkManager.GetCodesInScope(ScopingType.ActiveSubstancesModels);
            CollectionAssert.AreEqual(new[] { "Agm2" }, scope.ToArray());

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.AssessmentGroupMemberships);
            AssertDataReadingSummaryRecord(report, ScopingType.ActiveSubstancesModels, 2, "Agm2", "Agm1", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.ActiveSubstancesModels, ScopingType.Effects, 2, "eff2", "eff1", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.ActiveSubstances, ScopingType.ActiveSubstancesModels, 4, "Agm2", "Agm1,Agm3,Agm4", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.ActiveSubstances, ScopingType.Compounds, 5, "b,c", "a,d,e", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Effects);
            AssertDataReadingSummaryRecord(report, ScopingType.Effects, 0, "", "", "eff2");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Compounds);
            AssertDataReadingSummaryRecord(report, ScopingType.Compounds, 0, "", "", "b,c");
        }

        [TestMethod]
        public void DataLinkingActiveSubstancesCompoundsFilterTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.ActiveSubstancesModels, @"AssessmentGroupMembershipsTests/AssessmentGroupMembershipModels"),
                (ScopingType.ActiveSubstances, @"AssessmentGroupMembershipsTests/AssessmentGroupMemberships")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Compounds, ["b", "c"]);

            _compiledLinkManager.LoadScope(SourceTableGroup.AssessmentGroupMemberships);

            var scope = _compiledLinkManager.GetCodesInScope(ScopingType.ActiveSubstancesModels);
            CollectionAssert.AreEqual(new[] { "Agm1", "Agm2" }, scope.ToArray());

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.AssessmentGroupMemberships);
            AssertDataReadingSummaryRecord(report, ScopingType.ActiveSubstancesModels, 2, "Agm1,Agm2", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.ActiveSubstancesModels, ScopingType.Effects, 2, "eff1,eff2", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.ActiveSubstances, ScopingType.ActiveSubstancesModels, 4, "Agm1,Agm2", "Agm3,Agm4", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.ActiveSubstances, ScopingType.Compounds, 5, "b,c", "a,d,e", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Effects);
            AssertDataReadingSummaryRecord(report, ScopingType.Effects, 0, "", "", "eff1,eff2");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Compounds);
            AssertDataReadingSummaryRecord(report, ScopingType.Compounds, 0, "", "", "b,c");
        }
    }
}
