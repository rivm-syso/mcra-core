using System;

namespace MCRA.General.UnitConversion {
    public static class SourceTableGroupConverter {
        public static SourceTableGroup FromString(string str) {
            return (SourceTableGroup)Enum.Parse(typeof(SourceTableGroup), str);
        }
    }
}
