using System.ComponentModel;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ResponseSummaryRecord {

        [Description("The name of the response.")]
        [DisplayName("Name")]
        public string ResponseName { get; set; }

        [Description("The resonse identification code.")]
        [DisplayName("Code")]
        public string ResponseCode { get; set; }

        [Description("Description of the response.")]
        [DisplayName("Description")]
        public string Description { get; set; }

        [Description("The test system for which this response is defined.")]
        [DisplayName("Test system")]
        public string IdSystem { get; set; }

        [Description("The response type")]
        [DisplayName("Response type")]
        public string ResponseType { get; set; }

        [Description("Reference to the test method guideline, e.g., standaridised assay kit.")]
        [DisplayName("Guideline method")]
        public string GuidelineMethod { get; set; }

        [Description("The unit of measurement of this response (if applicable)")]
        [DisplayName("Unit")]
        public string ResponseUnit { get; set; }
    }
}
