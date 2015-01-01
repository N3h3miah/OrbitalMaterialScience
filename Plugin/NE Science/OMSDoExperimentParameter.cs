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
    public class OMSDoExperimentParameter : ContractParameter
    {

        private CelestialBody targetBody = null;
        private AvailablePart experiment = null;

        public OMSDoExperimentParameter()
        {
            //this.Enabled = true;
            //this.DisableOnStateChange = false;
        }

        public OMSDoExperimentParameter(CelestialBody target, AvailablePart exp)
            : base()
        {
            targetBody = target;
            experiment = exp;
        }

        protected override string GetHashString()
        {
            return "do experiment " + this.GetHashCode();
        }

        protected override string GetTitle()
        {
            if (targetBody == null)
                return "Run experiment in orbit";
            else
                return "Run experiment in orbit around " + targetBody.theName;
        }

        private float lastUpdate = 0;

        protected override void OnUpdate()
        {
            base.OnUpdate();
            if (lastUpdate > UnityEngine.Time.realtimeSinceStartup + 1)
                return;

            if (targetBody == null || experiment == null)
            {
                NE_Helper.log("targetBody or experimentType is null");
                return;
            }
            lastUpdate = UnityEngine.Time.realtimeSinceStartup;
            Vessel vessel = FlightGlobals.ActiveVessel;
            if (vessel != null)
                foreach (Part part in vessel.Parts)
                {
                    if (part.name == experiment.name)
                    {
                        OMSExperiment e = part.FindModuleImplementing<OMSExperiment>();
                        if (e != null)
                        {
                            if (e.completed >= this.Root.DateAccepted)
                            {
                                ScienceData[] data = e.GetData();
                                foreach (ScienceData datum in data)
                                {
                                    if (datum.subjectID.ToLower().Contains("@" + targetBody.name.ToLower() + "inspace"))
                                    {
                                        SetComplete();
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
            SetIncomplete();
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
            int bodyID = int.Parse(node.GetValue(KEESExperimentContract.TARGET_BODY));
            foreach (var body in FlightGlobals.Bodies)
            {
                if (body.flightGlobalsIndex == bodyID)
                    targetBody = body;
            }
            setTargetExperiment(node.GetValue(KEESExperimentContract.EXPERIMENT_STRING));
        }
        protected override void OnSave(ConfigNode node)
        {
            int bodyID = targetBody.flightGlobalsIndex;
            node.AddValue(KEESExperimentContract.TARGET_BODY, bodyID);

            node.AddValue(KEESExperimentContract.EXPERIMENT_STRING, experiment.name);
        }
    }
}
