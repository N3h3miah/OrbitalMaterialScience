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
        protected EquipmentRacks neededEquipment;
        protected ExperimentState state = ExperimentState.STORED;
        private ExperimentDataStorage store;

        public ExperimentData(string id, string name, string abb, EquipmentRacks eq)
        {
            this.id = id;
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

        protected virtual string getType()
        {
            return "";
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
                    return ExperimentState.STORED;
                case "RUNNING":
                    return ExperimentState.STORED;
                case "FINISHED":
                    return ExperimentState.STORED;
                case "FINALIZED":
                    return ExperimentState.STORED;
                default:
                    return ExperimentState.STORED;
            }
        }

        public ConfigNode getNode()
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
            return new ExperimentData("", "null Experiment", "empty", EquipmentRacks.NONE);
        }

        public virtual bool canInstall(Vessel vessel)
        {
            return false;
        }

        public virtual bool canMove(Vessel vessel)
        {
            return id != "" && (state == ExperimentState.STORED || state == ExperimentState.INSTALLED || state == ExperimentState.FINISHED ) && getFreeExperimentContainers(vessel).Count > 0;
        }

        public List<MoveableExperiment> getFreeExperimentContainers(Vessel vessel)
        {
            List<MoveableExperiment> freeCont = new List<MoveableExperiment>();
            List<MoveableExperiment> allCont = new List<MoveableExperiment>(UnityFindObjectsOfType(typeof(MoveableExperiment)) as MoveableExperiment[]);
            foreach (MoveableExperiment c in allCont)
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
            List<MoveableExperiment> targets = getFreeExperimentContainers(vessel);
            if ((state == ExperimentState.STORED || state == ExperimentState.INSTALLED || state == ExperimentState.FINISHED) && targets.Count > 0)
            {
                //moveTo(targets.First());
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

        public void moveTo(MoveableExperiment exp)
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
    }

    /*
     * Experiments needing MSL-1000
     */
    public class MSLExperimentData : ExperimentData
    {
        public MSLExperimentData(string id, string name, string abb, EquipmentRacks eq) : base(id, name, abb, eq)
        {}
        
        public override List<Lab> getFreeLabsWithEquipment(Vessel vessel){
            List<Lab> ret = new List<Lab>();
            List<PhysicsMaterialsLab> allPhysicsLabs = new List<PhysicsMaterialsLab>(UnityFindObjectsOfType(typeof(PhysicsMaterialsLab)) as PhysicsMaterialsLab[]);
            foreach (PhysicsMaterialsLab lab in allPhysicsLabs)
            {
                if (lab.vessel == vessel && lab.hasEquipmentInstalled(neededEquipment) && lab.hasEquipmentFreeExperimentSlot(neededEquipment))
                {
                    ret.Add(lab);
                }
            }
            return ret;
        }
    }

    public class CCFExperimentData :MSLExperimentData
    {
        public CCFExperimentData()
            : base("NE_CCF", "Capillary Channel Flow Experiment", "CCF", EquipmentRacks.FFR)
        {
            
        }

        public override bool canInstall(Vessel vessel)
        {
            List<Lab> labs = getFreeLabsWithEquipment(vessel);
            return labs.Count > 0 && state == ExperimentState.STORED;
        }

        protected override string getType()
        {
            return "CCF";
        }

        protected override void load(ConfigNode node)
        {
            base.load(node);
        }

    }

    public class TestExperimentData : ExperimentData
    {
        public TestExperimentData()
            : base("NE_Test", "Test Experiment", "Test", EquipmentRacks.NONE)
        {

        }

        public override bool canFinalize()
        {
            return true;
        }

        protected override string getType()
        {
            return "Test";
        }

        protected override void load(ConfigNode node)
        {
            base.load(node);
        }
    }
}
