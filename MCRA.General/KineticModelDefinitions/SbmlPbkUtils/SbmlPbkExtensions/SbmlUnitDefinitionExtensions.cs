using MCRA.Utils.Sbml.Objects;

namespace MCRA.General.Sbml {

    public static class SbmlUnitDefinitionExtensions {

        public static bool IsVolume(this SbmlUnitDefinition unit) {
            if (unit?.Units == null) {
                return false;
            }
            if (unit.Units.Count != 1) {
                return false;
            }
            var unitPart = unit.Units.Single();
            if (unitPart.Kind != SbmlUnitKind.Litre || unitPart.Exponent != 1) {
                return false;
            }
            return true;
        }

        public static bool IsMass(this SbmlUnitDefinition unit) {
            if (unit?.Units == null) {
                return false;
            }
            if (unit.Units.Count != 1) {
                return false;
            }
            var unitPart = unit.Units.Single();
            if (unitPart.Kind != SbmlUnitKind.Gram || unitPart.Exponent != 1) {
                return false;
            }
            return true;
        }

        public static bool IsAmountUnit(this SbmlUnitDefinition unit) {
            if (unit?.Units == null) {
                return false;
            }
            if (unit.Units.Count != 1) {
                return false;
            }
            var unitPart = unit.Units.Single();
            if ((unitPart.Kind != SbmlUnitKind.Gram && unitPart.Kind != SbmlUnitKind.Mole)
                || unitPart.Exponent != 1
            ) {
                return false;
            }
            return true;
        }

        public static bool IsTimeUnit(this SbmlUnitDefinition unit) {
            if (unit?.Units == null) {
                return false;
            }
            if (unit.Units.Count != 1) {
                return false;
            }
            var unitPart = unit.Units.Single();
            if (unitPart.Kind != SbmlUnitKind.Second || unitPart.Exponent != 1) {
                return false;
            }
            return true;
        }

        public static bool IsConcentrationUnit(this SbmlUnitDefinition unit) {
            if (unit?.Units == null) {
                return false;
            }
            if (unit.Units.Count != 2) {
                return false;
            }
            var unitParts = unit.Units.OrderByDescending(r => r.Exponent).ToList();
            if ((unitParts[0].Kind != SbmlUnitKind.Gram && unitParts[1].Kind != SbmlUnitKind.Mole)
                || unitParts[0].Exponent != 1
            ) {
                // First unit is not an amount unit (either gram or moles)
                return false;
            }
            if ((unitParts[1].Kind != SbmlUnitKind.Gram && unitParts[1].Kind != SbmlUnitKind.Litre)
                || unitParts[0].Exponent != -1
            ) {
                // Second unit is not an amount (gram) or volume (litre)
                return false;
            }
            return false;
        }

        public static TimeUnit ToTimeUnit(this SbmlUnitDefinition unit) {
            if (!IsTimeUnit(unit)) {
                throw new Exception($"Unit {unit.Id} is not a valid time unit");
            } else {
                var unitPart = unit.Units.Single();
                var result = unitPart.ToTimeUnit();
                return result;
            }
        }

        public static SubstanceAmountUnit ToSubstanceAmountUnit(this SbmlUnitDefinition unit) {
            if (!IsAmountUnit(unit)) {
                return SubstanceAmountUnit.Undefined;
            } else {
                var unitPart = unit.Units.Single();
                var result = unitPart.ToSubstanceAmountUnit();
                return result;
            }
        }

        public static ConcentrationMassUnit ToConcentrationMassUnit(this SbmlUnitDefinition unit) {
            if (unit.IsMass()) {
                var unitPart = unit.Units.Single();
                return unitPart.Scale switch {
                    3 => ConcentrationMassUnit.Kilograms,
                    0 => ConcentrationMassUnit.Grams,
                    _ => throw new NotImplementedException(),
                };
            } else if (unit.IsVolume()) {
                var unitPart = unit.Units.Single();
                return unitPart.Scale switch {
                    0 => ConcentrationMassUnit.Liter,
                    -3 => ConcentrationMassUnit.Milliliter,
                    _ => throw new NotImplementedException(),
                };
            }
            throw new NotImplementedException();
        }

        private static TimeUnit ToTimeUnit(this SbmlUnit unitPart) {
            if (unitPart == null) {
                throw new ArgumentNullException(nameof(unitPart));
            }

            // Only time-based units are valid for conversion
            if (unitPart.Kind != SbmlUnitKind.Second)
                return TimeUnit.NotSpecified;

            // Compute effective time multiplier
            var effectiveMultiplier = (double)unitPart.Multiplier * Math.Pow(10, (double)unitPart.Scale);

            // Match multiplier to TimeUnit
            return effectiveMultiplier switch {
                1 => TimeUnit.Seconds,
                60 => TimeUnit.Minutes,
                3600 => TimeUnit.Hours,
                86400 => TimeUnit.Days,
                _ => TimeUnit.NotSpecified // For unsupported multipliers (e.g., milliseconds, years, etc.)
            };
        }

        private static SubstanceAmountUnit ToSubstanceAmountUnit(this SbmlUnit unitPart) {
            if (unitPart.Multiplier != 1) {
                throw new NotImplementedException();
            }
            if (unitPart.Kind == SbmlUnitKind.Gram) {
                return unitPart.Scale switch {
                    3 => SubstanceAmountUnit.Kilograms,
                    0 => SubstanceAmountUnit.Grams,
                    -3 => SubstanceAmountUnit.Milligrams,
                    -6 => SubstanceAmountUnit.Micrograms,
                    -9 => SubstanceAmountUnit.Nanograms,
                    -12 => SubstanceAmountUnit.Picograms,
                    _ => throw new NotImplementedException(),
                };
            } else if (unitPart.Kind == SbmlUnitKind.Mole) {
                return unitPart.Scale switch {
                    0 => SubstanceAmountUnit.Moles,
                    -3 => SubstanceAmountUnit.Millimoles,
                    -6 => SubstanceAmountUnit.Micromoles,
                    -9 => SubstanceAmountUnit.Nanomoles,
                    _ => throw new NotImplementedException(),
                };
            }
            return SubstanceAmountUnit.Undefined;
        }
    }
}
