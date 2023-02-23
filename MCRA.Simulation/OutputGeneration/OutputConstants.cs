namespace MCRA.Simulation.OutputGeneration {
    public static class OutputConstants {
        /// <summary>
        /// Guid id for the action settings header, which is itself an empty header, but needs an id to be able to find it.
        /// Important: Note that fixed section guids should only be used for section headers without section data. This is
        /// because section data is stored with the section GUID as primary key.
        /// </summary>
        public static readonly Guid ActionSettingsSectionGuid = new Guid("{E7A53368-A268-4308-AB85-655ABDD96A90}");
    }
}
