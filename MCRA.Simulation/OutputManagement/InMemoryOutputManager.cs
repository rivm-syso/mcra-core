using MCRA.Data.Management.RawDataWriters;
using MCRA.General;
using MCRA.Simulation.OutputGeneration;

namespace MCRA.Simulation.OutputManagement {

    /// <summary>
    /// In-memory output manager.
    /// </summary>
    public class InMemoryOutputManager : IOutputManager {

        public IOutput Output { get; private set; }

        public ISectionManager SectionManager { get; private set; }

        /// <summary>
        /// Creates a new <see cref="InMemoryOutputManager"/> instance.
        /// </summary>
        public InMemoryOutputManager() {
        }

        public IOutput CreateOutput(int taskId) {
            Output = new OutputData() {
                idTask = taskId,
                StartExecution = DateTime.Now,
                BuildDate = Simulation.RetrieveLinkerTimestamp(),
                BuildVersion = Simulation.RetrieveVersion(),
                DateCreated = DateTime.Now,
            };
            return Output;
        }

        public IRawDataWriter CreateRawDataWriter(IOutput output) {
            return null;
        }

        public ISectionManager CreateSectionManager(IOutput output) {
            SectionManager = new InMemorySectionManager();
            return SectionManager;
        }

        public void SaveOutput(
            IOutput output,
            ISectionManager sectionManager
        ) {
        }

        public string GetOutputTempFolder(IOutput output) {
            return null;
        }
    }
}
