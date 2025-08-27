using MCRA.Utils.ProgressReporting;

namespace MCRA.Utils.Test.UnitTests {
    [TestClass]
    public class ProgressReportTests {

        /// <summary>
        /// Test single progress state as substate of the progress report.
        /// </summary>
        [TestMethod]
        public void ProgressReportTest1() {
            var progressReport = new ProgressReport();

            Assert.AreEqual(progressReport.CurrentActivity, string.Empty);
            Assert.AreEqual(0, progressReport.Progress);

            var subState1 = progressReport.NewProgressState(100);

            subState1.Update("State1", 10);
            Assert.AreEqual("State1", progressReport.CurrentActivity);
            Assert.AreEqual(10, progressReport.Progress);

            subState1.Update(20);
            Assert.AreEqual("State1", progressReport.CurrentActivity);
            Assert.AreEqual(20, progressReport.Progress);

            subState1.Increment(20);
            Assert.AreEqual("State1", progressReport.CurrentActivity);
            Assert.AreEqual(40, progressReport.Progress);
        }

        /// <summary>
        /// Test two progress states as substates of the progress report.
        /// </summary>
        [TestMethod]
        public void ProgressReportTest2() {
            var progressReport = new ProgressReport();

            Assert.AreEqual(string.Empty, progressReport.CurrentActivity);
            Assert.AreEqual(0, progressReport.Progress);

            var subState1 = progressReport.NewProgressState(50);
            subState1.Update("State1", 100);
            Assert.AreEqual("State1", progressReport.CurrentActivity);
            Assert.AreEqual(50, progressReport.Progress);

            var subState2 = progressReport.NewProgressState(50);
            subState2.Update("State2", 100);
            Assert.AreEqual("State2", progressReport.CurrentActivity);
            Assert.AreEqual(100, progressReport.Progress);
        }

        /// <summary>
        /// Test composite progress state as composite substate of the progress report.
        /// </summary>
        [TestMethod]
        public void ProgressReportTest3() {
            var progressReport = new ProgressReport();

            Assert.AreEqual(string.Empty, progressReport.CurrentActivity);
            Assert.AreEqual(0, progressReport.Progress);

            var subState1 = progressReport.NewProgressState(50);
            subState1.Update("State1", 100);
            Assert.AreEqual("State1", progressReport.CurrentActivity);
            Assert.AreEqual(50, progressReport.Progress);

            var compositeSubState2 = progressReport.NewCompositeState(50);
            var subState2 = compositeSubState2.NewProgressState(50);
            subState2.Update("State2", 100);
            Assert.AreEqual("State2", progressReport.CurrentActivity);
            Assert.AreEqual(75, progressReport.Progress);

            var subState3 = compositeSubState2.NewProgressState(50);
            subState3.Update("State3", 100);
            Assert.AreEqual("State3", progressReport.CurrentActivity);
            Assert.AreEqual(100, progressReport.Progress);
        }

        /// <summary>
        /// Test high number of progress states as substates, check for 100% progress after the loop.
        /// </summary>
        [TestMethod]
        public void ProgressReportTest4() {
            for (int j = 0; j < 20; j++) {
                var progressReport = new ProgressReport();

                Assert.AreEqual(string.Empty, progressReport.CurrentActivity);
                Assert.AreEqual(0, progressReport.Progress);

                var numberOfStates = 20000;
                var incrementAmount = 100D / numberOfStates;
                for (int i = 0; i < numberOfStates; i++) {
                    var subState = progressReport.NewProgressState(100);
                    subState.Increment($"State {i}", incrementAmount);
                }
                Assert.AreEqual(100, progressReport.Progress, 0.0001);
            }
        }

        /// <summary>
        /// Test cancellation token.
        /// </summary>
        [TestMethod]
        public void ProgressReportTest5() {
            var cancelSource = new CancellationTokenSource();

            var progressReport = new ProgressReport(cancelSource.Token);

            Assert.AreEqual(string.Empty, progressReport.CurrentActivity);
            Assert.AreEqual(0, progressReport.Progress);

            var subState1 = progressReport.NewProgressState(50);
            subState1.Update("State1", 50);
            Assert.AreEqual("State1", progressReport.CurrentActivity);
            Assert.AreEqual(25, progressReport.Progress);

            cancelSource.Cancel();

            Assert.ThrowsExactly<OperationCanceledException>(() => subState1.Update("State1", 50));
        }

        /// <summary>
        /// Test high number of progress states as substates with cancel action of forelast update.
        /// </summary>
        [TestMethod]
        [TestCategory("Sandbox Tests")]
        public void ProgressReportTest6() {
            Assert.ThrowsExactly<OperationCanceledException>(() => {
                for (int j = 0; j < 100; j++) {
                    var cancelSource = new CancellationTokenSource();
                    var progressReport = new ProgressReport(cancelSource.Token);
                    var numberOfStates = 500000;
                    var incrementAmount = 100D / numberOfStates;
                    for (int i = 0; i < numberOfStates; i++) {
                        var subState = progressReport.NewProgressState(100);
                        subState.Increment($"State {i}", incrementAmount);
                        if (i == numberOfStates - 2) {
                            cancelSource.Cancel();
                        }
                    }
                }
            });
        }

        /// <summary>
        /// Test progress on incomplete substate.
        /// </summary>
        [TestMethod]
        public void ProgressReportTest7() {
            var progressReport = new ProgressReport();

            Assert.AreEqual(string.Empty, progressReport.CurrentActivity);
            Assert.AreEqual(0, progressReport.Progress);

            var subState1 = progressReport.NewProgressState(50);
            subState1.Update("State1", 50);
            Assert.AreEqual("State1", progressReport.CurrentActivity);
            Assert.AreEqual(25, progressReport.Progress);

            var subState2 = progressReport.NewProgressState(50);
            subState2.Update("State2", 100);
            Assert.AreEqual("State2", progressReport.CurrentActivity);
            Assert.AreEqual(75, progressReport.Progress);
        }

        /// <summary>
        /// Test only initialized progress report; progress should be 0.
        /// </summary>
        [TestMethod]
        public void ProgressReportTest8() {
            var progressReport = new ProgressReport();
            Assert.AreEqual(0D, progressReport.Progress);
        }

        /// <summary>
        /// Test progress report with only one composite state with no progress recorded; progress should be 0.
        /// </summary>
        [TestMethod]
        public void ProgressReportTest9() {
            var progressReport = new ProgressReport();
            _ = progressReport.NewCompositeState(50);
            Assert.AreEqual(0D, progressReport.Progress);
        }

        /// <summary>
        /// Test progress report with only one local progress state with no progress recorded; progress should be 0.
        /// </summary>
        [TestMethod]
        public void ProgressReportTest10() {
            var progressReport = new ProgressReport();
            _ = progressReport.NewProgressState(100);
            Assert.AreEqual(0D, progressReport.Progress);
        }
    }
}
