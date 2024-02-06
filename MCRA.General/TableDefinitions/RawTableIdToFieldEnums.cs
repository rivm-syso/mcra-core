using MCRA.General.TableDefinitions.RawTableFieldEnums;
using MCRA.General;

namespace MCRA.Data.Raw.Constants {
    public static partial class RawTableIdToFieldEnums {
        private static Dictionary<Type, RawDataSourceTableID> _enumToIdMap;
        private static Dictionary<RawDataSourceTableID, Type> _idToEnumMap;

        /// <summary>
        /// IdToEnumMap
        /// </summary>
        public static IReadOnlyDictionary<RawDataSourceTableID, Type> IdToEnumMap {
            get {
                if (_idToEnumMap == null) {
                    initializeMaps();
                }
                return _idToEnumMap;
            }
        }

        /// <summary>
        /// EnumToIdMap
        /// </summary>
        public static IReadOnlyDictionary<Type, RawDataSourceTableID> EnumToIdMap {
            get {
                if (_enumToIdMap == null) {
                    initializeMaps();
                }
                return _enumToIdMap;
            }
        }

        private static void initializeMaps() {
            //initialize the dictionary by using the code generated method
            initializeDictionary();
            //Types not translated to raw tables:
            _enumToIdMap.Add(typeof(RawSampleYears), RawDataSourceTableID.Unknown);
            _enumToIdMap.Add(typeof(RawSampleLocations), RawDataSourceTableID.Unknown);
            _enumToIdMap.Add(typeof(RawSampleRegions), RawDataSourceTableID.Unknown);
            _enumToIdMap.Add(typeof(RawSampleProductionMethods), RawDataSourceTableID.Unknown);
            _enumToIdMap.Add(typeof(RawTwoWayTableData), RawDataSourceTableID.Unknown);

            //reverse map, exclude Unknown table id's
            _idToEnumMap = _enumToIdMap.Where(kvp => kvp.Value != RawDataSourceTableID.Unknown)
                .ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
        }
    }
}
