namespace MCRA.General.ScopingTypeDefinitions {
    public sealed class ScopeReference {
        public string FieldId { get; set; }
        public ScopingType SourceScopingType { get; set; }
        public SourceTableGroup SourceTableGroup { get; set; }
        public RawDataSourceTableID SourceTable { get; set; }
        public ScopingType TargetScopingType { get; set; }
        public RawDataSourceTableID TargetTable { get; set; }
        public bool IsKeysList { get; set; }
        public bool MatchAny { get; set; }
        public char ListSeparator { get; set; }

        public ScopeReference(char sep = ',') {
            ListSeparator = sep;
        }
    }
}
