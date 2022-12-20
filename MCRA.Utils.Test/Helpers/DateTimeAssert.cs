using System;

namespace MCRA.Utils.Test.Helpers {
    public static class DateTimeAssert {
        public static void AreEqual(DateTime? expectedDate, DateTime? actualDate, TimeSpan maximumDelta) {
            if (expectedDate == null && actualDate == null) {
                return;
            } else if (expectedDate == null) {
                throw new NullReferenceException("The expected date was null");
            } else if (actualDate == null) {
                throw new NullReferenceException("The actual date was null");
            }
            double totalSecondsDifference = Math.Abs(((DateTime)actualDate - (DateTime)expectedDate).TotalSeconds);
            if (totalSecondsDifference > maximumDelta.TotalSeconds) {
                throw new Exception($"Expected Date: {expectedDate}, Actual Date: {actualDate}\n" +
                    $"Expected Delta: {maximumDelta}, Actual Delta in seconds- {totalSecondsDifference}");
            }
        }
    }
}
