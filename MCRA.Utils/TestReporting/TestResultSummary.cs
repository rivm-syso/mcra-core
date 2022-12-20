using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCRA.Utils.TestReporting {
    public sealed class TestResultSummary {
        public string StartDate { get; set; }
        public List<TestResultSummaryRecord> TestResultSummaryRecords { get; set; }
    }
}
