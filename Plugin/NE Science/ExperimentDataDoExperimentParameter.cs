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
    public class ExperimentDataDoExperimentParameter : ContractParameter
    {

        private CelestialBody targetBody = null;
        private ExperimentData experiment = null;

        public ExperimentDataDoExperimentParameter()
        {
            //this.Enabled = true;
            //this.DisableOnStateChange = false;
        }

        public ExperimentDataDoExperimentParameter(CelestialBody target, ExperimentData exp)
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
                return "Run experiment " + experiment.getAbbreviation() + " in orbit";
            else
                return "Run experiment " + experiment.getAbbreviation() + " in orbit around " + targetBody.theName;
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
                    ExperimentStorage[] ess = part.GetComponents<ExperimentStorage>();
                    if (ess.Length > 0)
                    {
                        foreach (ExperimentStorage es in ess)
                        {
                            ScienceData[] data = es.GetData();
                            foreach (ScienceData datum in data)
                            {
                                if (datum.subjectID.ToLower().Contains(experiment.getId().ToLower()+"@" + targetBody.name.ToLower() + "inspace"))
                                {
                                    SetComplete();
                                    return;
                                }
                            }
                        }
                        OMSExperiment e = part.FindModuleImplementing<OMSExperiment>();
                        if (e != null)
                        {
                            if (e.completed >= this.Root.DateAccepted)
                            {
                                
                            }
                        }
                    }
                }
            SetIncomplete();
        }

        protected override void OnLoad(ConfigNode node)
        {
            int bodyID = int.Parse(node.GetValue(KEESExperimentContract.TARGET_BODY));
            foreach (var body in FlightGlobals.Bodies)
            {
                if (body.flightGlobalsIndex == bodyID)
                    targetBody = body;
            }
            experiment = ExperimentData.getExperimentDataFromNode(node.GetNode(ExperimentData.CONFIG_NODE_NAME));
        }
        protected override void OnSave(ConfigNode node)
        {
            int bodyID = targetBody.flightGlobalsIndex;
            node.AddValue(KeminiExperimentContract.TARGET_BODY, bodyID);
            node.AddNode(experiment.getNode());
        }
    }
}
