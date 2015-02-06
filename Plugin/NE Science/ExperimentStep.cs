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

namespace NE_Science
{
    public class ExperimentStep
    {
        public const string CONFIG_NODE_NAME = "NE_ExperimentStep";
        private const string TYPE_VALUE = "Type";

        protected ExperimentData exp;

        private string type = "";

        protected ExperimentStep(ExperimentData exp, string type)
        {
            this.exp = exp;
            this.type = type;
        }

        public virtual bool ready()
        {
            return false;
        }

        public virtual bool isResearchFinished()
        {
            return false;
        }

        public virtual bool start(){
            return false;
        }

        public virtual void finishStep()
        {
        }


        public virtual ConfigNode getNode()
        {
            ConfigNode node = new ConfigNode(CONFIG_NODE_NAME);

            node.AddValue(TYPE_VALUE, getType());
            return node;
        }

        public static ExperimentStep getExperimentStepFromConfigNode(ConfigNode node, ExperimentData exp)
        {
            if (node.name != CONFIG_NODE_NAME)
            {
                NE_Helper.logError("getExperimentStepFromConfigNode: invalid Node: " + node.name);
                return new ExperimentStep(exp, "");
            }
            ExperimentStep step = createExperimentStep(node.GetValue(TYPE_VALUE), exp);
            step.load(node);
            return step;

        }

        protected virtual void load(ConfigNode node)
        {
        }

        private static ExperimentStep createExperimentStep(string p, ExperimentData exp)
        {
            switch (p)
            {
                case "ResStep":
                    return new ResourceExperimentStep(exp);
                case "MEPResStep":
                    return new MEPResourceExperimentStep(exp);
                default:
                    return new ExperimentStep(exp, "");
            }
        }

        protected string getType()
        {
            return type;
        }

        internal virtual bool canStart()
        {
            return exp.state == ExperimentState.INSTALLED;
        }
    }

    public class ResourceExperimentStep : ExperimentStep
    {
        private const string RES_VALUE = "Res";
        private const string AMOUNT_VALUE = "Amount";

        protected string res;
        protected float amount;

        internal ResourceExperimentStep(ExperimentData exp)
            : base(exp, "ResStep")
        { }

        internal ResourceExperimentStep(ExperimentData exp, string type)
            : base(exp, type)
        { }

        public ResourceExperimentStep(ExperimentData exp, string res, float amount)
            : base(exp, "ResStep")
        {
            this.res = res;
            this.amount = amount;
        }

        internal ResourceExperimentStep(ExperimentData exp, string res, float amount, string type)
            : base(exp, type)
        {
            this.res = res;
            this.amount = amount;
        }

        public override ConfigNode getNode()
        {
            ConfigNode node =  base.getNode();
            node.AddValue(RES_VALUE, res);
            node.AddValue(AMOUNT_VALUE, amount);
            return node;
        }

        protected override void load(ConfigNode node)
        {
            base.load(node);
            res = node.GetValue(RES_VALUE);
            amount = float.Parse(node.GetValue(AMOUNT_VALUE));
        }

        public override bool ready()
        {
            return exp.state == ExperimentState.INSTALLED;
        }

        public override bool isResearchFinished()
        {
            double numTestPoints = ((LabEquipment)exp.store).getResourceAmount(res);

            return Math.Round(numTestPoints, 2) >= amount;
        }

        public override bool start()
        {
            NE_Helper.log("ResExppStep.start()");
            if(canStart()){
                Lab lab = ((LabEquipment)exp.store).getLab();
                if (lab != null && !OMSExperiment.checkBoring(lab.vessel, true))
                {
                    NE_Helper.log("ResExppStep.start(): create Resource");
                    ((LabEquipment)exp.store).createResourceInLab(res, amount);
                    return true;
                }
                else
                {
                    NE_Helper.logError("ResExppStep.start(): Lab null or boring. Boring: " + OMSExperiment.checkBoring(lab.vessel, true));
                }
            }
            NE_Helper.log("ResExppStep.start(): can NOT start");
            return false;
        }

        public override void finishStep()
        {
            if (exp.state == ExperimentState.RUNNING && isResearchFinished())
            {
                ((LabEquipment)exp.store).setResourceMaxAmount(res, 0f); ;
            }
        }
    }

    public class MEPResourceExperimentStep : ResourceExperimentStep
    {
        internal MEPResourceExperimentStep(ExperimentData exp)
            : base(exp, "MEPResStep")
        { }

        public MEPResourceExperimentStep(ExperimentData exp, string res, float amount)
            : base(exp, res, amount, "MEPResStep")
        {
        }

        internal override bool canStart()
        {
            if(base.canStart()){
                return ((MEP_Module)((LabEquipment)exp.store).getLab()).MEPlabState == MEPLabStatus.READY;
            }

            return false; ;
        }
    }
}
