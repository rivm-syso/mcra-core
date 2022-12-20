using MCRA.Utils.Collections;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Constants;
using System.Collections.Generic;

namespace MCRA.Simulation.Calculators.KineticModelCalculation.AbsorptionFactorsGeneration {
    public sealed class AbsorptionFactorsCollectionBuilder {
        private readonly IAbsorptionFactorsCollectionBuilderSettings _settings;
        public AbsorptionFactorsCollectionBuilder(IAbsorptionFactorsCollectionBuilderSettings settings) {
            _settings = settings;
        }

        /// <summary>
        /// https://www.epa.gov/expobox/exposure-assessment-tools-routes-ingestion
        /// Ingestion exposure can also occur via the intentional or inadvertent non-dietary ingestion of soil, dust, or chemical residues
        /// on surfaces or objects that are contacted via hand-to-mouth or object-to-mouth activity (especially for young children).  
        /// Soil can become contaminated as a result of direct discharges to soil, atmospheric deposition, or transport from other media(e.g., water). 
        /// Contaminated soil can also be tracked indoors and contribute to contaminated house dust.
        /// Estimating exposure from ingestion requires information on the concentration of the contaminant in the medium that is ingested, ingestion rate, 
        /// and the timeframe of exposure.Estimating exposure from non-dietary ingestion may also require information on the frequency of hand-to-mouth or
        /// object-to-mouth contact.
        /// https://ofmpub.epa.gov/eims/eimscomm.getfile?p_download_id=526164
        /// https://doi.org/10.6027/9789289330206-11-en
        /// Adults and children have the potential for exposure to toxic substances through non-dietary ingestion pathways other than soil and dust 
        /// ingestion, e.g., ingesting pesticide residues that have been transferred from treated surfaces to the hands or objects that are mouthed. 
        /// Adult’s mouth objects such as cigarettes, pens/pencils, or their hands. Young children mouth objects, surfaces or their fingers as they 
        /// explore their environment. Mouthing behaviour includes all activities in which objects, including fingers, are touched by the mouth or 
        /// put into the mouth except for eating and drinking, and includes licking, sucking, chewing, and biting (US-EPA, 2009).
        /// 
        ///  On 11/5/2018 Marc Kennedy wrote:
        ///  "
        ///  Dear Waldo and Hilko,
        ///  As suggested in Berlin, I checked what values I used for the absorption factors. In Acropolis I tried 2 alternatives for dermal absorption 
        ///  (75% was the conservative default listed in the old EFSA (2012) guidance or 10% as suggested in de Heer et al, 1999 for the German model). 
        ///  It was interesting to note that the 10% value led to a closer match of exposures to the urine based values when we did the comparisons. 
        ///  In my examples for Euromix I simply used a factor of 1 (100%) for each route(dermal, oral and inhalation) but explained in the manuscript 
        ///  that this is a worst case assumption.
        ///  Section 5.6 of the guidance on operators, workers, residents and bystanders (EFSA, 2015) refers to 100% for Oral and Inhalation, and then 
        ///  cites EFSA (2012) for dermal factors.EFSA(2015) also says to use the higher of the values for undiluted and in-use dilution(this used to 
        ///  be 75% but as explained below it is now reduced to 50%).
        ///  Since our original modelling there has been an update to the dermal absorption guidance – see EFSA(2017). On P18(section 5.10) it says that 
        ///  until better data are obtained we should use a default from the higher of the values for concentrated or in-use diluted products. Then on 
        ///  Table 2 it has various values depending on the formulation.For water-based formulations it gives a highest absorption factors of 50%. So you 
        ///  might want to use this value? This would also go some way to explain why my example non-dietary exposures are more conservative than they 
        ///  need to be
        ///  Hope this is useful!
        ///  Marc
        ///  "
        /// </summary>
        /// <param name="substances"></param>
        /// <param name="defaultFactorDietary"></param>
        /// <param name="defaultFactorDermalNonDietary"></param>
        /// <param name="defaultFactorOralNonDietary"></param>
        /// <param name="defaultFactorInhalationNonDietary"></param>
        /// <param name="substanceAbsorptionFactors"></param>
        /// <returns></returns>
        public TwoKeyDictionary<ExposureRouteType, Compound, double> Create(
            ICollection<Compound> substances,
            ICollection<KineticAbsorptionFactor> substanceAbsorptionFactors = null
        ) {
            var kineticAbsorptionFactors = new TwoKeyDictionary<ExposureRouteType, Compound, double> {
                [ExposureRouteType.Dietary, SimulationConstants.NullSubstance] = _settings.DefaultFactorDietary,
                [ExposureRouteType.Dermal, SimulationConstants.NullSubstance] = _settings.DefaultFactorDermalNonDietary,
                [ExposureRouteType.Oral, SimulationConstants.NullSubstance] = _settings.DefaultFactorOralNonDietary,
                [ExposureRouteType.Inhalation, SimulationConstants.NullSubstance] = _settings.DefaultFactorInhalationNonDietary
            };
            if (substances != null) {
                if (substanceAbsorptionFactors != null) {
                    foreach (var item in substanceAbsorptionFactors) {
                        kineticAbsorptionFactors[item.ExposureRoute, item.Compound] = item.AbsorptionFactor;
                    }
                }
            }
            return kineticAbsorptionFactors;
        }
    }
}
