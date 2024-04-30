using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Calculators.ActiveSubstancesCalculators.AggregateMembershipModelCalculation;
using MCRA.Simulation.Calculators.ActiveSubstancesCalculators.MembershipsFromInSilicoCalculation;
using MCRA.Simulation.Calculators.ActiveSubstancesCalculators.MembershipsFromPodCalculation;

namespace MCRA.Simulation.Actions.ActiveSubstances {
    public sealed class ActiveSubstancesModuleSettings :
        IMembershipsFromInSilicoCalculatorSettings,
        IAggregateMembershipModelCalculatorSettings,
        IMembershipsFromPodCalculatorSettings
    {

        private readonly ActiveSubstancesModuleConfig _configuration;

        public ActiveSubstancesModuleSettings(
            ActiveSubstancesModuleConfig config,
            bool isCompute
        ) {
            IsCompute = isCompute;
            _configuration = config;
        }

        public bool IsCompute { get; private set; }

        public bool IsComputeFromQsarOrDocking {
            get {
                return IsCompute && (UseQsarModels || UseMolecularDockingModels);
            }
        }

        public bool DeriveFromHazardData {
            get {
                return RestrictToAvailableHazardDoses
                    || RestrictToAvailableHazardCharacterisations;
            }
        }

        public bool RestrictToAvailableHazardCharacterisations => _configuration.FilterByAvailableHazardCharacterisation;

        public bool RestrictToAvailableHazardDoses => _configuration.FilterByAvailableHazardDose;

        public bool UseQsarModels => _configuration.UseQsarModels;

        public bool UseMolecularDockingModels => _configuration.UseMolecularDockingModels;

        public bool FilterByCertainAssessmentGroupMembership {
            get {
                if (!IsCompute || UseProbabilisticMemberships) {
                    return _configuration.FilterByCertainAssessmentGroupMembership;
                } else {
                    return true;
                }
            }
        }

        public bool IncludeSubstancesWithUnknowMemberships {
            get {
                if (UseProbabilisticMemberships) {
                    return PriorMembershipProbability > 0;
                } else {
                    if (IsComputeFromQsarOrDocking) {
                        return _configuration.IncludeSubstancesWithUnknowMemberships;
                    } else if (!IsCompute) {
                        return _configuration.IncludeSubstancesWithUnknowMemberships;
                    } else {
                        return !DeriveFromHazardData;
                    }
                }
            }
        }

        public bool UseProbabilisticMemberships {
            get {
                if (!IsCompute || IsComputeFromQsarOrDocking) {
                    return _configuration.UseProbabilisticMemberships;
                } else {
                    return false;
                }
            }
        }

        public double PriorMembershipProbability {
            get {
                return UseProbabilisticMemberships
                    ? _configuration.PriorMembershipProbability
                    : IncludeSubstancesWithUnknowMemberships ? 1 : 0;
            }
        }

        public AssessmentGroupMembershipCalculationMethod AssessmentGroupMembershipCalculationMethod {
            get {
                return IsComputeFromQsarOrDocking
                    ? _configuration.AssessmentGroupMembershipCalculationMethod
                    : UseProbabilisticMemberships
                        ? AssessmentGroupMembershipCalculationMethod.CrispMax
                        : AssessmentGroupMembershipCalculationMethod.ProbabilisticRatio;
            }
        }

        public CombinationMethodMembershipInfoAndPodPresence CombinationMethodMembershipInfoAndPodPresence {
            get {
                if (IsComputeFromQsarOrDocking && DeriveFromHazardData) {
                    return _configuration.CombinationMethodMembershipInfoAndPodPresence;
                } else if (!IsCompute && DeriveFromHazardData) {
                    return _configuration.CombinationMethodMembershipInfoAndPodPresence;
                } else {
                    return CombinationMethodMembershipInfoAndPodPresence.Union;
                }
            }
        }

        public bool BubbleMembershipsThroughAop => false;
    }
}

