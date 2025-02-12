using MCRA.Data.Management.CompiledDataManagers;
using MCRA.Data.Management.RawDataProviders;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    [TestClass]
    public class CompiledTestsBase {

        protected CsvRawDataProvider _rawDataProvider;
        protected CompiledDataManager _compiledDataManager;

        [TestInitialize]
        public virtual void TestInitialize() {
            _rawDataProvider = new CsvRawDataProvider(@"Resources/Csv/");
            _compiledDataManager = new CompiledDataManager(_rawDataProvider);
        }
    }
}
