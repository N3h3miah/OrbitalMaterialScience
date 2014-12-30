using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP;
using Contracts;
using Contracts.Parameters;

namespace NE_Science.Contracts.Parameters
{
    public class OMSReturnExperimentParameter : ContractParameter
    {

        private CelestialBody targetBody = null;
        private AvailablePart experiment = null;

        static readonly Dictionary<string, string> experimentModulname =
          new Dictionary<string, string> {
              { "StnSciExperiment1", "1" },
              { "StnSciExperiment2", "2" },
              
          };

        public OMSReturnExperimentParameter()
        {
            //this.Enabled = true;
            //this.DisableOnStateChange = false;
        }

        public OMSReturnExperimentParameter(CelestialBody target, AvailablePart exp)
            : base()
        {
            targetBody = target;
            experiment = exp;
        }

        protected override string GetHashString()
        {
            return "return and recover experiment " + this.GetHashCode();
        }
        protected override string GetTitle()
        {
            return "Return and recover experiment at Kerbin";
        }

        protected override void OnRegister()
        {
            GameEvents.onVesselRecovered.Add(OnRecovered);
        }
        protected override void OnUnregister()
        {
            GameEvents.onVesselRecovered.Remove(OnRecovered);
        }

        private void OnRecovered(ProtoVessel pv)
        {
            if (targetBody == null || experiment == null)
            {
                Debug.Log("targetBody or experimentType is null");
                return;
            }
            foreach (ProtoPartSnapshot part in pv.protoPartSnapshots)
            {
                if (part.partName == experiment.name)
                {
                    foreach (ProtoPartModuleSnapshot module in part.modules)
                    {
                        NE_Helper.log("ProtoVessel recovery Modulename: " + module.moduleName);
                        //TODO
                    }
                }
            }
        }

        private void OnRecovery(Vessel vessel)
        {
            if (targetBody == null || experiment == null)
            {
                Debug.Log("targetBody or experimentType is null");
                return;
            }
            foreach (Part part in vessel.Parts)
            {
                if (part.name == experiment.name)
                {
                    OMSExperiment e = part.FindModuleImplementing<OMSExperiment>();
                    if (e != null)
                    {
                        if (e.launched >= this.Root.DateAccepted)
                        {
                            ScienceData[] data = e.GetData();
                            foreach (ScienceData sc in data)
                            {
                                if (sc.subjectID.ToLower().Contains("@" + targetBody.name.ToLower() + "inspace"))
                                {
                                    SetComplete();
                                    return;
                                }
                            }
                        }
                    }
                }
            }
        }

        private bool setTargetExperiment(string exp)
        {
            experiment = PartLoader.getPartInfoByName(exp);
            if (experiment == null)
            {
                Debug.LogError("Couldn't find experiment part: " + exp);
                return false;
            }
            return true;
        }

        protected override void OnLoad(ConfigNode node)
        {
            int bodyID = int.Parse(node.GetValue(OMSExperimentContract.TARGET_BODY));
            foreach (var body in FlightGlobals.Bodies)
            {
                if (body.flightGlobalsIndex == bodyID)
                    targetBody = body;
            }
            setTargetExperiment(node.GetValue(OMSExperimentContract.EXPERIMENT_STRING));
        }
        protected override void OnSave(ConfigNode node)
        {
            int bodyID = targetBody.flightGlobalsIndex;
            node.AddValue(OMSExperimentContract.TARGET_BODY, bodyID);

            node.AddValue(OMSExperimentContract.EXPERIMENT_STRING, experiment.name);
        }
    }
}
