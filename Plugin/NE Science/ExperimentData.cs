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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NE_Science
{
    public enum ExperimentState
    {
        STORED, INSTALLED, RUNNING, FINISHED, FINALIZED
    }

    public class ExperimentData
    {
        public const string CONFIG_NODE_NAME = "NE_ExperimentData";
        public const string TYPE_VALUE = "Type";
        public const string STATE_VALUE = "State";
        private const string MASS_VALUE = "Mass";

        private string id;
        private string name;
        private string abb;
        private string type = "";
        private float mass = 0f;
        protected EquipmentRacks neededEquipment;
        internal ExperimentState state = ExperimentState.STORED;
        internal ExperimentDataStorage store;

        public ExperimentData(string id, string type, string name, string abb, EquipmentRacks eq, float mass)
        {
            this.id = id;
            this.type = type;
            this.name = name;
            this.abb = abb;
            this.mass = mass;
            neededEquipment = eq;
        }

        public string getId()
        {
            return id;
        }

        public string getName()
        {
            return name;
        }

        public string getAbbreviation()
        {
            return abb;
        }
        public float getMass()
        {
            return mass;
        }

        public EquipmentRacks getEquipmentNeeded()
        {
            return neededEquipment;
        }

        public virtual string getDescription()
        {
            string desc = "<b>" + name + "</b>\n";
            desc += getReqString();
            return desc;
        }

        private string getReqString()
        {
            
            string reqString = "Needs: ";
            switch (getEquipmentNeeded())
            {
                case EquipmentRacks.CIR:
                    reqString += "MSL-1000 with Combustion Integrated Rack (CIR)";
                    break;
                case EquipmentRacks.FIR:
                    reqString += "MSL-1000 with Fluid Integrated Rack (FIR)";
                    break;
                case EquipmentRacks.PRINTER:
                    reqString += "MSL-1000 with 3D-Printer (3PR)";
                    break;
                case EquipmentRacks.MSG:
                    reqString += "MPL-600 with Microgravity Science Glovebox (MSG)";
                    break;
                case EquipmentRacks.USU:
                    reqString += "MPL-600 with Ultrasound Unit (USU)";
                    break;
                case EquipmentRacks.KEMINI:
                    reqString += "Command Pod mk1";
                    break;
                case EquipmentRacks.EXPOSURE:
                    reqString += "MEP-825 and MPL-600 or MSL-1000";
                    break;
            }
            
            return reqString;
        }

        public virtual bool canFinalize()
        {
            return state == ExperimentState.FINISHED;
        }

        public string getType()
        {
            return type;
        }

        protected virtual void load(ConfigNode node)
        {
            state = getState(node.GetValue(STATE_VALUE));
        }

        private ExperimentState getState(string s)
        {
            switch (s)
            {
                case "STORED":
                    return ExperimentState.STORED;
                case "INSTALLED":
                    return ExperimentState.INSTALLED;
                case "RUNNING":
                    return ExperimentState.RUNNING;
                case "FINISHED":
                    return ExperimentState.FINISHED;
                case "FINALIZED":
                    return ExperimentState.FINALIZED;
                default:
                    return ExperimentState.STORED;
            }
        }

        public virtual ConfigNode getNode()
        {
            ConfigNode node = new ConfigNode(CONFIG_NODE_NAME);

            node.AddValue(TYPE_VALUE, getType());
            node.AddValue(STATE_VALUE, state);
            node.AddValue(MASS_VALUE, mass);

            return node;
        }

        public static ExperimentData getExperimentDataFromNode(ConfigNode node)
        {
            if (node.name != CONFIG_NODE_NAME)
            {
                NE_Helper.logError("getLabEquipmentFromNode: invalid Node: " + node.name);
                return getNullObject();
            }
            float mass = getMass(node.GetValue(MASS_VALUE));

            ExperimentData exp = ExperimentFactory.getExperiment(node.GetValue(TYPE_VALUE), mass);
            exp.load(node);
            return exp; ;
        }

        private static float getMass(string p)
        {
            if (p != null)
            {
                try
                {
                    return float.Parse(p);
                }catch(FormatException){
                    return 0f;
                }
            }
            return 0f;
        }



        public static ExperimentData getNullObject()
        {
            return new ExperimentData("", "", "null Experiment", "empty", EquipmentRacks.NONE, 0f);
        }

        public virtual bool canInstall(Vessel vessel)
        {
            return false;
        }

        public virtual bool canMove(Vessel vessel)
        {
            return id != "" && (state == ExperimentState.STORED || state == ExperimentState.INSTALLED || state == ExperimentState.FINISHED ) && getFreeExperimentContainers(vessel).Count > 0;
        }

        public List<ExperimentStorage> getFreeExperimentContainers(Vessel vessel)
        {
            List<ExperimentStorage> freeCont = new List<ExperimentStorage>();
            List<ExperimentStorage> allCont = new List<ExperimentStorage>(UnityFindObjectsOfType(typeof(ExperimentStorage)) as ExperimentStorage[]);
            foreach (ExperimentStorage c in allCont)
            {
                if (c.vessel == vessel && c.isEmpty())
                {
                    freeCont.Add(c);
                }
            }
            return freeCont;
        }

        public virtual List<Lab> getFreeLabsWithEquipment(Vessel vessel)
        {
            return new List<Lab>();
        }

        protected UnityEngine.Object[] UnityFindObjectsOfType(Type type)
        {
            return GameObject.FindObjectsOfType(type);
        }


        internal void installed(LabEquipment rack)
        {
            state = ExperimentState.INSTALLED;
            store = rack;
            rack.getLab().part.mass += getMass();
        }

        internal void finalize()
        {
            state = ExperimentState.FINALIZED;
        }

        internal void move(Vessel vessel)
        {
            List<ExperimentStorage> targets = getFreeExperimentContainers(vessel);
            if ((state == ExperimentState.STORED || state == ExperimentState.INSTALLED || state == ExperimentState.FINISHED) && targets.Count > 0)
            {
                ChooseMoveTarget t = getMoveGuiComponent();
                t.showDialog(targets, this);
            }
        }

        private ChooseMoveTarget getMoveGuiComponent()
        {
            ChooseMoveTarget t = store.getPartGo().GetComponent<ChooseMoveTarget>();
            if (t == null)
            {
                t = store.getPartGo().AddComponent<ChooseMoveTarget>();
            }
            return t;
        }

        public void moveTo(ExperimentStorage exp)
        {
            if (state == ExperimentState.INSTALLED)
            {
                state = ExperimentState.STORED;
            }
            store.removeExperimentData();
            exp.part.mass += getMass();
            exp.storeExperiment(this);
        }

        internal void setStorage(ExperimentDataStorage storage)
        {
            store = storage;
        }

        internal virtual bool canRunAction()
        {
            return false;
        }

        internal virtual string getActionString()
        {
            switch (state)
            {
                case ExperimentState.INSTALLED:
                    return "Start " + getAbbreviation();

                case ExperimentState.FINISHED:
                    return "End " + getAbbreviation();

                default:
                    return "";
            }
        }

        public virtual void runLabAction()
        {
            
        }

        internal virtual string getStateString()
        {
            switch (state)
            {
                case ExperimentState.STORED:
                    return "Stored";
                case ExperimentState.INSTALLED:
                    return "Installed";
                case ExperimentState.RUNNING:
                    return "Running";
                case ExperimentState.FINISHED:
                    return "Finished";
                case ExperimentState.FINALIZED:
                    return "Finalized";
                default:
                    return "NullState";
            }
        }

        internal void load(LabEquipment labEquipment)
        {
            store = labEquipment;
        }

        public virtual bool isExposureExperiment(){
            return false;
        }

        internal virtual void updateCheck()
        {
        }
    }

    public class StepExperimentData : ExperimentData
    {
        protected ExperimentStep step;

        protected StepExperimentData(string id, string type, string name, string abb, EquipmentRacks eq, float mass)
            : base(id, type, name, abb, eq, mass)
        {}

        public override ConfigNode getNode()
        {
            ConfigNode baseNode = base.getNode();
            if(step != null){
                baseNode.AddNode(step.getNode());
            }
            return baseNode;
        }

        protected override void load(ConfigNode node)
        {
            base.load(node);
            ConfigNode stepNode = node.GetNode(ExperimentStep.CONFIG_NODE_NAME);

            step = ExperimentStep.getExperimentStepFromConfigNode(stepNode, this);
        }

        internal override bool canRunAction()
        {
            switch (state)
            {
                case ExperimentState.INSTALLED:
                    return step.canStart();

                case ExperimentState.RUNNING:
                    return step.isResearchFinished();
                default:
                    return base.canRunAction();
            }
        }

        internal override string getActionString()
        {
            switch (state)
            {
                case ExperimentState.INSTALLED:
                    return "Start " + getAbbreviation();

                case ExperimentState.RUNNING:
                    if (step.isResearchFinished())
                    {
                        return "End " + getAbbreviation();
                    }
                    else
                    {
                        return "";
                    }
                default:
                    return "";
            }
        }

        public override void runLabAction()
        {
            switch (state)
            {
                case ExperimentState.INSTALLED:
                    if (step.canStart())
                    {
                        step.start(startCallback);
                    }
                    break;
                case ExperimentState.RUNNING:
                    if (step.isResearchFinished()) {
                        step.finishStep();
                        state = ExperimentState.FINISHED;
                    }
                    break;
            }
        }

        internal void startCallback(bool started)
        {
            if (started)
            {
                state = ExperimentState.RUNNING;
            }
        }

        public override bool isExposureExperiment()
        {
            return step.getNeededResource() == Resources.EXPOSURE_TIME;
        }
    }

    public class MultiStepExperimentData<T>  : ExperimentData where T : ExperimentStep
    {
        private const string ACTIVE_VALUE = "activeStep";
        protected T[] steps = new T[1];
        private int activeStep = 0;

        protected MultiStepExperimentData(string id, string type, string name, string abb, EquipmentRacks eq, float mass)
            : base(id, type, name, abb, eq, mass)
        { }

        public override ConfigNode getNode()
        {
            ConfigNode baseNode = base.getNode();
            baseNode.AddValue(ACTIVE_VALUE, activeStep);

            if (steps != null)
            {
                foreach (ExperimentStep es in steps)
                {
                    baseNode.AddNode(es.getNode());
                }
            }
            return baseNode;
        }

        protected override void load(ConfigNode node)
        {
            base.load(node);

            activeStep = int.Parse(node.GetValue(ACTIVE_VALUE));

            ConfigNode[] stepNodes = node.GetNodes(ExperimentStep.CONFIG_NODE_NAME);
            steps = new T[stepNodes.Length];
            foreach (ConfigNode stepNode in stepNodes)
            {
                T step = (T)ExperimentStep.getExperimentStepFromConfigNode(stepNode, this);
                steps[step.getIndex()] = step;
            }
        }

        internal override bool canRunAction()
        {
            switch (state)
            {
                case ExperimentState.INSTALLED:
                    return steps[activeStep].canStart();

                case ExperimentState.RUNNING:
                    return steps[activeStep].isResearchFinished();
                default:
                    return base.canRunAction();
            }
        }

        internal override string getActionString()
        {
            switch (state)
            {
                case ExperimentState.INSTALLED:
                    return "Start " + getAbbreviation() + " " + steps[activeStep].getName();

                case ExperimentState.RUNNING:
                    if (steps[activeStep].isResearchFinished())
                    {
                        return "End " + getAbbreviation() + " " + steps[activeStep].getName();
                    }
                    else
                    {
                        return "";
                    }
                default:
                    return "";
            }
        }

        internal T getActiveStep()
        {
            return steps[activeStep];
        }

        internal List<T> getExperimentSteps()
        {
            return new List<T>(steps);
        }

        internal int getActiveStepIndex()
        {
            return activeStep;
        }

        public override void runLabAction()
        {
            switch (state)
            {
                case ExperimentState.INSTALLED:
                    if(steps[activeStep].canStart()){
                        steps[activeStep].start(startCallback);
                    }
                    break;
                case ExperimentState.RUNNING:
                    if (steps[activeStep].isResearchFinished())
                    {
                        steps[activeStep].finishStep();
                        if (isLastStep())
                        {
                            state = ExperimentState.FINISHED;
                        }
                        else
                        {
                            state = ExperimentState.INSTALLED;
                            activeStep++;
                        }
                    }
                    break;
            }
        }

        internal void startCallback(bool started)
        {
            if (started)
            {
                state = ExperimentState.RUNNING;
            }
        }

        private bool isLastStep()
        {
            return activeStep == (steps.Length - 1);
        }

        public override bool isExposureExperiment()
        {
            return steps[activeStep].getNeededResource() == Resources.EXPOSURE_TIME;
        }
    }

    public class TestExperimentData : KerbalResearchExperimentData
    {
        public TestExperimentData(float mass)
            : base("NE_Test", "Test", "Test Experiment", "Test", EquipmentRacks.USU, mass, 4)
        {
            steps[0] = new KerbalResearchStep(this, Resources.ULTRASOUND_GEL, 0.5f, 0);
            steps[1] = new KerbalResearchStep(this, Resources.ULTRASOUND_GEL, 0.5f, 1);
            steps[2] = new KerbalResearchStep(this, Resources.ULTRASOUND_GEL, 0.5f, 2);
            steps[3] = new KerbalResearchStep(this, Resources.ULTRASOUND_GEL, 0.5f, 3);
        }

    }
}
