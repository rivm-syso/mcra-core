using MCRA.Utils.ExtensionMethods;
using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Calculators.HazardCharacterisationCalculation.HazardDoseTypeConversion {
    public sealed class HazardDoseConverter : IHazardDoseConverter {

        private PointOfDepartureType _targetHazardDose;
        private ExposureUnitTriple _targetExposureUnit;

        public HazardDoseConverter(PointOfDepartureType targetHazardDoseType, ExposureUnitTriple targetExposureUnit) {
            _targetHazardDose = targetHazardDoseType;
            _targetExposureUnit = targetExposureUnit;
        }

        public double ConvertToTargetUnit(DoseUnit doseUnitSource, Compound compound, double dose) {
            if (double.IsNaN(compound.MolecularMass) && (doseUnitSource.GetSubstanceAmountUnit().IsInMoles() ^ _targetExposureUnit.SubstanceAmountUnit.IsInMoles())) {
                throw new Exception($"Cannot convert dose unit {doseUnitSource.GetShortDisplayName()} to target unit for substance {compound.Name} ({compound.Code}) due to missing molar mass.");
            }
            return doseUnitSource.GetDoseAlignmentFactor(_targetExposureUnit, compound.MolecularMass) * dose;
        }

        public double GetExpressionTypeConversionFactor(PointOfDepartureType sourceType) {
            switch (_targetHazardDose) {
                case PointOfDepartureType.Bmd:
                    return toBmdFactor(sourceType);
                case PointOfDepartureType.Bmdl01:
                case PointOfDepartureType.Bmdl10:
                    throw new Exception(message: $"No conversion from {sourceType} to {_targetHazardDose}");
                case PointOfDepartureType.Noael:
                    return toNoaelFactor(sourceType);
                case PointOfDepartureType.Loael:
                    throw new Exception(message: $"No conversion from {sourceType} to LOAEL");
                case PointOfDepartureType.Unspecified:
                    return 1D;
                default:
                    throw new Exception(message: $"No conversion from {sourceType} to {_targetHazardDose}");
            }
        }

        private double toBmdFactor(PointOfDepartureType sourceType) {
            switch (sourceType) {
                case PointOfDepartureType.Bmd:
                    return 1D;
                case PointOfDepartureType.Bmdl01:
                case PointOfDepartureType.Bmdl10:
                    throw new Exception(message: $"No conversion from {sourceType} to benchmark dose");
                case PointOfDepartureType.Loael:
                    return toNoaelFactor(PointOfDepartureType.Loael) * toBmdFactor(PointOfDepartureType.Noael);
                case PointOfDepartureType.Noael:
                    return 3D;
                default:
                    throw new Exception(message: $"No conversion from {sourceType} to benchmark dose");
            }
        }

        private double toNoaelFactor(PointOfDepartureType sourceType) {
            switch (sourceType) {
                case PointOfDepartureType.Bmd:
                    return 1D/3;
                case PointOfDepartureType.Bmdl01:
                case PointOfDepartureType.Bmdl10:
                    throw new Exception(message: $"No conversion from {sourceType} to NOAEL");
                case PointOfDepartureType.Noael:
                    return 1;
                case PointOfDepartureType.Loael:
                    return 1D/3;
                default:
                    throw new Exception(message: $"No conversion from {sourceType} to NOAEL");
            }
        }
    }
}
