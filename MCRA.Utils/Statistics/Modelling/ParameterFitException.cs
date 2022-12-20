using System;

namespace MCRA.Utils.Statistics.Modelling {
    public sealed class ParameterFitException : Exception {

        public ParameterFitException()
            : base() { }

        public ParameterFitException(string message)
            : base(message) { }

        public ParameterFitException(string message, Exception innerException)
            : base(message, innerException) { }

    }
}
