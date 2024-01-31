namespace MCRA.Data.Raw.Copying {

    [Serializable]
    public class RawDataSourceBulkCopyException : Exception {

        /// <summary>
        /// Name of the source table that cause the exception.
        /// </summary>
        public string SourceTableName { get; set; }

        /// <summary>
        /// SQL error code that was mentioned in the inner exception
        /// </summary>
        public int SqlErrorCode { get; set; }

        public RawDataSourceBulkCopyException() {
        }

        public RawDataSourceBulkCopyException(string message) : base(message) {
        }

        public RawDataSourceBulkCopyException(string message, string sourceTableName) : base(message) {
            SourceTableName = sourceTableName;
        }
        
        public RawDataSourceBulkCopyException(string message, Exception inner) : base(message, inner) {
        }
    }
}
