namespace MCRA.Data.Compiled.Objects {
    public sealed class FoodSample {
        public string Code;
        public Food Food { get; set; }
        public string Location { get; set; }
        public string Region { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ProductionMethod { get; set; }
        public DateTime? DateSampling { get; set; }
        public List<SampleAnalysis> SampleAnalyses { get; set; }


        public IDictionary<SampleProperty, SamplePropertyValue> SampleProperties { get; set; }
            = new Dictionary<SampleProperty, SamplePropertyValue>();
    }
}
