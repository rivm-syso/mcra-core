using MCRA.General;
using MCRA.General.Action.Settings.Dto;
using MCRA.Simulation.Calculators.ActiveSubstancesCalculators.AggregateMembershipModelCalculation;
using MCRA.Simulation.Calculators.ActiveSubstancesCalculators.MembershipsFromInSilicoCalculation;
using MCRA.Simulation.Calculators.ActiveSubstancesCalculators.MembershipsFromPodCalculation;

namespace MCRA.Simulation.Actions.ActiveSubstances {
    public sealed class ActiveSubstancesModuleSettings :
        IMembershipsFromInSilicoCalculatorSettings,
        IAggregateMembershipModelCalculatorSettings,
        IMembershipsFromPodCalculatorSettings
    {

        private readonly EffectSettingsDto _effectSettings;

        public ActiveSubstancesModuleSettings(
            EffectSettingsDto effectSettings,
            bool isCompute
        ) {
            IsCompute = isCompute;
            _effectSettings = effectSettings;
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

        public bool RestrictToAvailableHazardCharacterisations => _effectSettings.RestrictToAvailableHazardCharacterisations;

        public bool RestrictToAvailableHazardDoses => _effectSettings.RestrictToAvailableHazardDoses;

        public bool UseQsarModels => _effectSettings.UseQsarModels;

        public bool UseMolecularDockingModels => _effectSettings.UseMolecularDockingModels;

        public bool FilterByCertainAssessmentGroupMembership {
            get {
                if (!IsCompute || UseProbabilisticMemberships) {
                    return _effectSettings.RestrictToCertainMembership;
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
                        return _effectSettings.IncludeSubstancesWithUnknowMemberships;
                    } else if (!IsCompute) {
                        return _effectSettings.IncludeSubstancesWithUnknowMemberships;
                    } else {
                        return !DeriveFromHazardData;
                    }
                }
            }
        }

        public bool UseProbabilisticMemberships {
            get {
                if (!IsCompute || IsComputeFromQsarOrDocking) {
                    return _effectSettings.UseProbabilisticMemberships;
                } else {
                    return false;
                }
            }
        }

        public double PriorMembershipProbability {
            get {
                return UseProbabilisticMemberships
                    ? _effectSettings.PriorMembershipProbability
                    : IncludeSubstancesWithUnknowMemberships ? 1 : 0;
            }
        }

        public AssessmentGroupMembershipCalculationMethod AssessmentGroupMembershipCalculationMethod {
            get {
                return IsComputeFromQsarOrDocking
                    ? _effectSettings.AssessmentGroupMembershipCalculationMethod
                    : UseProbabilisticMemberships
                        ? AssessmentGroupMembershipCalculationMethod.CrispMax
                        : AssessmentGroupMembershipCalculationMethod.ProbabilisticRatio;
            }
        }

        public CombinationMethodMembershipInfoAndPodPresence CombinationMethodMembershipInfoAndPodPresence {
            get {
                if (IsComputeFromQsarOrDocking && DeriveFromHazardData) {
                    return _effectSettings.CombinationMethodMembershipInfoAndPodPresence;
                } else if (!IsCompute && DeriveFromHazardData) {
                    return _effectSettings.CombinationMethodMembershipInfoAndPodPresence;
                } else {
                    return CombinationMethodMembershipInfoAndPodPresence.Union;
                }
            }
        }

        public bool BubbleMembershipsThroughAop => false;
    }
}

