using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;

namespace MCRA.General.ScopingTypeDefinitions {

    public class McraScopingTypeDefinitions {

        private static McraScopingTypeDefinitions _instance;

        private static IDictionary<ScopingType, ScopingTypeDefinition> _scopingTypes = null;
        private static IDictionary<SourceTableGroup, List<ScopingTypeDefinition>> _tableGroupScopingTypesLookup = null;

        private static IList<ScopeReference> _scopeReferences;
        private static IDictionary<ScopingType, List<ScopeReference>> _childScopingTypesLookup = null;
        private static IDictionary<ScopingType, List<ScopeReference>> _parentScopingTypesLookup = null;

        private static HashSet<ScopingType> _autoScopeTypes = null;
        private static HashSet<ScopingType> _strongEntityTypes = null;
        private static HashSet<ScopingType> _userSelectionTypes = null;

        /// <summary>
        /// Singleton accessor.
        /// </summary>
        public static McraScopingTypeDefinitions Instance {
            get { return _instance ?? (_instance = new McraScopingTypeDefinitions()); }
        }

        /// <summary>
        /// Singleton constructor.
        /// </summary>
        private McraScopingTypeDefinitions() {
            _scopingTypes = loadDefinitions();
        }

        /// <summary>
        /// Returns all settings definitions.
        /// </summary>
        public IDictionary<ScopingType, ScopingTypeDefinition> ScopingDefinitions {
            get {
                return _scopingTypes;
            }
        }

        public IDictionary<SourceTableGroup, List<ScopingTypeDefinition>> TableGroupScopingTypesLookup {
            get {
                if (_tableGroupScopingTypesLookup == null) {
                    _tableGroupScopingTypesLookup = ScopingDefinitions.Values
                        .GroupBy(r => r.TableGroup)
                        .ToDictionary(r => r.Key, r => r.ToList());
                }
                return _tableGroupScopingTypesLookup;
            }
        }

        public IList<ScopeReference> ScopeReferences {
            get {
                if (_scopeReferences == null) {
                    _scopeReferences = ScopingDefinitions.Values
                        .SelectMany(r => r.ParentScopeReferences, (r, s) => {
                            var sourceDefinition = ScopingDefinitions[r.Id];
                            var targetDefinition = ScopingDefinitions[s.ReferencedScope];
                            return new ScopeReference() {
                                FieldId = s.IdField,
                                SourceScopingType = r.Id,
                                SourceTable = sourceDefinition.RawTableId ?? RawDataSourceTableID.Unknown,
                                SourceTableGroup = sourceDefinition.TableGroup,
                                TargetScopingType = s.ReferencedScope,
                                TargetTable = targetDefinition.RawTableId ?? RawDataSourceTableID.Unknown,
                                IsKeysList = s.IsKeysList,
                                MatchAny = s.MatchAny,
                            };
                        })
                        .ToList();
                }
                return _scopeReferences;
            }
        }

        /// <summary>
        /// Returns a dictionary with for each scoping type, the list of scoping types that
        /// reference this scoping type.
        /// </summary>
        public IDictionary<ScopingType, List<ScopeReference>> ChildScopingTypesLookup {
            get {
                if (_childScopingTypesLookup == null) {
                    _childScopingTypesLookup = ScopeReferences
                        .GroupBy(r => r.TargetScopingType)
                        .ToDictionary(r => r.Key, r => r.ToList());
                }
                return _childScopingTypesLookup;
            }
        }

        /// <summary>
        /// Returns a dictionary with for each scoping type, the list of scoping types that
        /// reference this scoping type.
        /// </summary>
        public IDictionary<ScopingType, List<ScopeReference>> ParentScopingTypesLookup {
            get {
                if (_parentScopingTypesLookup == null) {
                    _parentScopingTypesLookup = ScopeReferences
                        .GroupBy(r => r.SourceScopingType)
                        .ToDictionary(r => r.Key, r => r.ToList());
                }
                return _parentScopingTypesLookup;
            }
        }

        /// <summary>
        /// Returns the list of raw datasource table ids for which the scope is automatically
        /// set when the scope is not explicitly defined (= empty)
        /// In this case the scope is set to all records in this table, and any related tables which contain
        /// a relation. For example, when foods are read from the RawFoods table, the scope is defined
        /// by all Food codes in this table, but also all food ids from other related food tables
        /// for example when consumption data is read, all food ids that were not previously read are added.
        /// </summary>
        public HashSet<ScopingType> AutoScopeTypes {
            get {
                if (_autoScopeTypes == null) {
                    _autoScopeTypes = ScopingDefinitions.Values
                        .Where(r => r.IsAutoScope)
                        .Select(r => r.Id)
                        .ToHashSet();
                }
                return _autoScopeTypes;
            }
        }

        public HashSet<ScopingType> StrongEntityTypes {
            get {
                if (_strongEntityTypes == null) {
                    _strongEntityTypes = ScopingDefinitions.Values
                        .Where(r => r.IsStrongEntity)
                        .Select(r => r.Id)
                        .ToHashSet();
                }
                return _strongEntityTypes;
            }
        }

        public HashSet<ScopingType> UserSelectionTypes {
            get {
                if (_userSelectionTypes == null) {
                    _userSelectionTypes = ScopingDefinitions.Values
                        .Where(r => r.IsStrongEntity && r.AllowUserSelection)
                        .Select(r => r.Id)
                        .ToHashSet();
                }
                return _userSelectionTypes;
            }
        }

        public IList<ScopingTypeDefinition> GetTableGroupUserSelectionTypes(SourceTableGroup sourceTableGroup) {
            if (TableGroupScopingTypesLookup.TryGetValue(sourceTableGroup, out var scopingTypes)) {
                return scopingTypes.Where(r => IsUserSelectionType(r.Id)).ToList();
            }
            return new List<ScopingTypeDefinition>();
        }

        public bool IsStrongEntityType(ScopingType scopingType) {
            return StrongEntityTypes.Contains(scopingType);
        }

        public bool IsUserSelectionType(ScopingType scopingType) {
            return UserSelectionTypes.Contains(scopingType);
        }

        /// <summary>
        /// Returns the table group of the specified scoping type.
        /// </summary>
        public SourceTableGroup RawTableGroupLookup(ScopingType scopingType) {
            var definition = ScopingDefinitions[scopingType];
            var result = definition.TableGroup;
            return result;
        }

        /// <summary>
        /// Returns all table group definitions (including the nested table definitions) as defined in
        /// the internal xml.
        /// </summary>
        private static IDictionary<ScopingType, ScopingTypeDefinition> loadDefinitions() {
            var assembly = Assembly.Load("MCRA.General");
            using (var stream = assembly.GetManifestResourceStream("MCRA.General.ScopingTypeDefinitions.ScopeTypeDefinitions.xml")) {
                var xs = new XmlSerializer(typeof(ScopingTypeCollection));
                var definition = (ScopingTypeCollection)xs.Deserialize(stream);
                var result = definition.ToDictionary(r => r.Id);
                return result;
            }
        }
    }
}
