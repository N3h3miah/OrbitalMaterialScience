using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NE_Science
{
    /*
    *Module used to add Experiments to the Tech tree. 
    */
    public class NE_ExperimentModule : PartModule
    {

        [KSPField(isPersistant = false)]
        public string type = "";

    } 


    public class ExperimentFactory
    {
        static readonly List<string> expRegistry = new List<string>() { "NE.TEST", "NE.CCFE" };

        public static List<ExperimentData> getAvailableExperiments()
        {
            List<ExperimentData> list = new List<ExperimentData>();
            foreach (string pn in expRegistry)
            {
                AvailablePart part = PartLoader.getPartInfoByName(pn);
                if (part != null)
                {
                    if (ResearchAndDevelopment.PartTechAvailable(part))
                    {
                        Part pPf = part.partPrefab;
                        NE_ExperimentModule exp = pPf.GetComponent<NE_ExperimentModule>();
                        list.Add(getExperiment(exp.type));
                    }
                }
            }

            return list;
        }

        public static ExperimentData getExperiment(string type)
        {
            switch (type)
            {
                case "Test":
                    return new TestExperimentData();
                case "CCF":
                    return new CCFExperimentData();
                default:
                    return ExperimentData.getNullObject();

            }
        }
    }
}
