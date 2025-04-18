﻿using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Objects;

namespace MCRA.Simulation.Calculators.ExternalExposureCalculation {
    public interface IExternalIndividualExposure {

        SimulatedIndividual SimulatedIndividual { get; }

        Dictionary<ExposurePath, List<IIntakePerCompound>> ExposuresPerPath { get; set; }

        /// <summary>
        /// Returns true if this individual exposure contains one or more positive amounts for a route and substance.
        /// </summary>
        bool HasPositives(ExposureRoute route, Compound substance);

        double GetExposure(ExposureRoute route, Compound substance, bool isPerPerson);

        double GetExposure(ExposurePath path, Compound substance, bool isPerPerson);

        List<IExternalIndividualDayExposure> ExternalIndividualDayExposures { get; set; }
    }
}
