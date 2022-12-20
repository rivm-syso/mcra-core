using MCRA.Data.Management.RawDataWriters;
using MCRA.General;

namespace MCRA.Simulation.OutputManagement {
    public interface IOutputManager {

        IOutput CreateOutput(int taskId);

        string GetOutputTempFolder(IOutput output);

        IRawDataWriter CreateRawDataWriter(IOutput output);

        ISectionManager CreateSectionManager(IOutput output);

        void SaveOutput(
            IOutput output,
            ISectionManager sectionManager
        );
    }
}
