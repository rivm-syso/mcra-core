namespace MCRA.Data.Compiled.Objects {
    public sealed class HCSubgroup {
        public string IdHazardCharacterisation { get; set; }
        public string IdSubgroup { get; set; }
        public Compound Substance { get; set; }
        public double? AgeLower { get; set; }
        public string Gender { get; set; }
        public double Value { get; set; }
        public ICollection<HCSubgroupsUncertain> HCSubgroupsUncertains { get; set; }

        /// <summary>
        /// Creates a shallow copy of this object.
        /// </summary>
        public HCSubgroup Clone() {
            return new HCSubgroup() {
                Substance = this.Substance,
                Value = this.Value,
                IdSubgroup = this.IdSubgroup,
                IdHazardCharacterisation = this.IdHazardCharacterisation,
                AgeLower = this.AgeLower,
                Gender = this.Gender,
            };
        }
    }
}
