namespace MCRA.Data.Compiled.Objects {
    public sealed class PointOfDepartureUncertain {
        public Compound Compound { get; set; }
        public Effect Effect { get; set; }

        public string IdUncertaintySet { get; set; }
        public double LimitDose { get; set; }
        public string DoseResponseModelParameterValues { get; set; }
    }
}
