using MCRA.General;
using MCRA.Simulation.TaskExecution;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.TaskExecution {
    [TestClass]
    public class TaskExecuterFactoryTests {

        /// <summary>
        /// Creates job executer for a complete simulation.
        /// </summary>
        [TestMethod]
        public void TaskExecuterFactory_TestCreateSimulationTaskExecuter() {
            var executer = TaskExecuterFactory.CreateTaskExecuter(MCRATaskType.Simulation, null, null, string.Empty);
            Assert.IsNotNull(executer);
        }

        /// <summary>
        /// Creates job executer for a complete simulation.
        /// </summary>
        [TestMethod]
        public void TaskExecuterFactory_TestCreateLoopCalculationTaskExecuter() {
            var executer = TaskExecuterFactory.CreateTaskExecuter(MCRATaskType.LoopCalculation, null, null, string.Empty);
            Assert.IsNotNull(executer);
        }

        /// <summary>
        /// Creates job executer for a data source compilation.
        /// Expect fail because this type is deprecated.
        /// </summary>
        [TestMethod]
        public void TaskExecuterFactory_TestRunDataSourceCompilationTest() {
            Assert.ThrowsExactly<Exception>(() => TaskExecuterFactory.CreateTaskExecuter(MCRATaskType.DataSourceCompilation, null, null, string.Empty));
        }

        /// <summary>
        /// Creates job executer for a complete simulation.
        /// Expect fail because this type is deprecated.
        /// </summary>
        [TestMethod]
        public void TaskExecuterFactory_TestRunFoodConversionTest() {
            Assert.ThrowsExactly<Exception>(() => TaskExecuterFactory.CreateTaskExecuter(MCRATaskType.FoodConversion, null, null, string.Empty));
        }

        /// <summary>
        /// Creates job executer for a complete simulation.
        /// Expect fail because this type is deprecated.
        /// </summary>
        [TestMethod]
        public void TaskExecuterFactory_TestRunConcentrationModellingTest() {
            Assert.ThrowsExactly<Exception>(() => TaskExecuterFactory.CreateTaskExecuter(MCRATaskType.ConcentrationModelling, null, null, string.Empty));
        }

        /// <summary>
        /// Creates job executer for a exposure screening run.
        /// Expect fail because this type is deprecated.
        /// </summary>
        [TestMethod]
        public void TaskExecuterFactory_TestRunExposureScreeningTest() {
            Assert.ThrowsExactly<Exception>(() => TaskExecuterFactory.CreateTaskExecuter(MCRATaskType.ExposureScreening, null, null, string.Empty));
        }

        /// <summary>
        /// Creates job executer for a data source compilation.
        /// Expect fail because this type is deprecated.
        /// </summary>
        [TestMethod]
        public void TaskExecuterFactory_TestRunIntakeCalculationTest() {
            Assert.ThrowsExactly<Exception>(() => TaskExecuterFactory.CreateTaskExecuter(MCRATaskType.IntakeCalculation, null, null, string.Empty));
        }
    }
}
