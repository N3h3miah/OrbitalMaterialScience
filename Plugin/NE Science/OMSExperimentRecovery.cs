/*
 *   This file is part of Orbital Material Science.
 *   
 *   Orbital Material Science is free software: you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation, either version 3 of the License, or
 *   (at your option) any later version.
 *
 *   Orbital Material Sciencee is distributed in the hope that it will be useful,
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *   GNU General Public License for more details.
 *
 *   You should have received a copy of the GNU General Public License
 *   along with Orbital Material Science.  If not, see <http://www.gnu.org/licenses/>.
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KSP;

namespace NE_Science.Contracts.Parameters
{
    /*
     * Strategy for finding the experiment in a recoverd vessel. 
     */
    public class OMSExperimentRecovery
    {
        protected static readonly Dictionary<string, string> experimentModulname =
          new Dictionary<string, string> {
              { "NE.KEES.PPMD", "KEESExperiment" },
              { "NE.KEES.POSA1", "KEESExperiment" },
              { "NE.KEES.POSA2", "KEESExperiment" },
              { "NE.KEES.ODC", "KEESExperiment" },
              
          };

        protected const string SCIENCE_DATA = "ScienceData";
        protected const string SUBJECT_ID = "subjectID";

        public virtual bool protovesselHasDoneExperiement(ProtoVessel pv, AvailablePart experiment, CelestialBody targetBody, double contractAccepted)
        {
            foreach (ProtoPartSnapshot part in pv.protoPartSnapshots)
            {
                if (part.partName == experiment.name)
                {
                    return experimentFound(part, experiment, targetBody, contractAccepted);
                }
            }
            return false;
        }

        protected bool experimentFound(ProtoPartSnapshot part, AvailablePart experiment, CelestialBody targetBody, double contractAccepted)
        {
            NE_Helper.log("ProtoVessel recovery: Experiment found");
            string moduleName = experimentModulname[experiment.name];
            foreach (ProtoPartModuleSnapshot module in part.modules)
            {
                NE_Helper.log("ProtoVessel recovery Modulename: " + module.moduleName);
                if (module.moduleName == moduleName)
                {
                    ConfigNode partConf = module.moduleValues;
                    float completed = getFloatValueFromConfigNode(partConf, OMSExperiment.COMPLETED);
                    if (completed >= contractAccepted)
                    {
                        return containsDoneExperimentData(partConf, targetBody);
                    }
                }
            }
            return false;
        }

        protected bool containsDoneExperimentData(ConfigNode partConf, CelestialBody targetBody)
        {
            foreach (ConfigNode scienceData in partConf.GetNodes(SCIENCE_DATA))
            {
                if (!scienceData.HasValue(SUBJECT_ID))
                    continue;
                string subjectID = scienceData.GetValue(SUBJECT_ID);
                NE_Helper.log("Science on Board SubjectID: " + subjectID);
                if (subjectID.ToLower().Contains("@" + targetBody.name.ToLower() + "inspace"))
                {
                    return true;
                }
            }
            return false;
        }

        protected float getFloatValueFromConfigNode(ConfigNode configNode, string valueName)
        {
            try
            {
                return float.Parse(configNode.GetValue(valueName));
            }
            catch (Exception)
            {
                return 0;
            }
        }
    }

    /*
     * Recovery strategy for KEES experiment. This experiments can be stored in a payload carrier.
     */
    public class KEESExperimentRecovery : OMSExperimentRecovery
    {
        protected const string KEES_PC = "NE.KEES.PC";
        protected const string KAS_CONTAINER = "KASModuleContainer";
        protected const string CONTENT_PART = "CONTENT_PART";

        public override bool protovesselHasDoneExperiement(ProtoVessel pv, AvailablePart experiment, CelestialBody targetBody, double contractAccepted)
        {
            NE_Helper.log("KEES-Experiement stategy");
            foreach (ProtoPartSnapshot part in pv.protoPartSnapshots)
            {
                NE_Helper.log("KEES-Experiement stategy, Part: " + part.partName);
                if (part.partName == experiment.name)
                {
                    if (experimentFound(part, experiment, targetBody, contractAccepted))
                        return true;
                }
                else if (part.partName == KEES_PC)
                {
                    if (payloadCarrierFound(part, experiment, targetBody, contractAccepted))
                        return true;
                }
            }
            return false;
        }

        private bool payloadCarrierFound(ProtoPartSnapshot payloadCarrier, AvailablePart experiment, CelestialBody targetBody, double contractAccepted)
        {

            NE_Helper.log("ProtoVessel recovery: payload carrier found");
            string experiementModuleName = experimentModulname[experiment.name];
            foreach (ProtoPartModuleSnapshot module in payloadCarrier.modules)
            {
                NE_Helper.log("ProtoVessel recovery Modulename: " + module.moduleName);
                if (module.moduleName == KAS_CONTAINER)
                {
                    NE_Helper.log("KAS container found");
                    ConfigNode partConf = findExperimentModulInPC(module, experiment);
                    if (partConf != null)
                    {
                        NE_Helper.log("Experiment module found");
                        float completed = getFloatValueFromConfigNode(partConf, OMSExperiment.COMPLETED);
                        if (completed >= contractAccepted)
                        {
                            return containsDoneExperimentData(partConf, targetBody);
                        }
                    }
                }
            }
            return false;
        }

        private ConfigNode findExperimentModulInPC(ProtoPartModuleSnapshot kasModule, AvailablePart experiment)
        {
            ConfigNode partConf = kasModule.moduleValues;
            foreach (ConfigNode contentPart in partConf.GetNodes(CONTENT_PART))
            {
                NE_Helper.log("ContentPart: " + contentPart.GetValue("name"));
                if (contentPart.GetValue("name") == experiment.name)
                {
                    foreach (ConfigNode module in contentPart.GetNodes("MODULE"))
                    {
                        if (module.GetValue("name") == experimentModulname[experiment.name])
                            return module;
                    }
                }
            }
            return null;
        }


    }
}
