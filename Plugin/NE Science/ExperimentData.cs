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
        private const string TYPE_VALUE = "Type";
        private const string STATE_VALUE = "State";

        private string id;
        private string name;
        private string abb;
        private string type = "";
        protected EquipmentRacks neededEquipment;
        internal ExperimentState state = ExperimentState.STORED;
        internal ExperimentDataStorage store;

        public ExperimentData(string id, string type, string name, string abb, EquipmentRacks eq)
        {
            this.id = id;
            this.type = type;
            this.name = name;
            this.abb = abb;
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

        public EquipmentRacks getEquipmentNeeded()
        {
            return neededEquipment;
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

            return node;
        }

        public static ExperimentData getExperimentDataFromNode(ConfigNode node)
        {
            if (node.name != CONFIG_NODE_NAME)
            {
                NE_Helper.logError("getLabEquipmentFromNode: invalid Node: " + node.name);
                return getNullObject();
            }

            ExperimentData exp = ExperimentFactory.getExperiment(node.GetValue(TYPE_VALUE));
            exp.load(node);
            return exp; ;
        }



        public static ExperimentData getNullObject()
        {
            return new ExperimentData("", "", "null Experiment", "empty", EquipmentRacks.NONE);
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
                ChooseMoveTarget t = getGuiComponent(store);
                t.showDialog(targets, this);
            }
        }

        private ChooseMoveTarget getGuiComponent(ExperimentDataStorage store)
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
            exp.storeExperiment(this);
        }

        internal void setStorage(ExperimentDataStorage storage)
        {
            store = storage;
        }

        internal virtual string getActionString()
        {
            switch (state)
            {
                case ExperimentState.INSTALLED:
                    return "Start " + getAbbreviation();

                case ExperimentState.FINISHED:
                    return "End " + getAbbreviation() + " Step";

                default:
                    return "";
            }
        }

        public virtual void runLabAction()
        {
            
        }

        internal string getStateString()
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
    }

    public class StepExperimentData : ExperimentData
    {
        protected ExperimentStep step;

        protected StepExperimentData(string id, string type, string name, string abb, EquipmentRacks eq)
            : base(id, type, name, abb, eq)
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

        internal override string getActionString()
        {
            switch (state)
            {
                case ExperimentState.INSTALLED:
                    return "Start " + getAbbreviation();

                case ExperimentState.RUNNING:
                    if (step.isResearchFinished())
                    {
                        return "End " + getAbbreviation() + " Step";
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
                    if (step.start())
                    {
                        state = ExperimentState.RUNNING;
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
    }



    public class TestExperimentData : MSLExperimentData
    {
        public TestExperimentData()
            : base("NE_Test", "Test", "Test Experiment", "Test", EquipmentRacks.FFR)
        {
            step = new ResourceExperimentStep(this, Resources.FFR_TEST_RUN, 2);
        }

    }
}
