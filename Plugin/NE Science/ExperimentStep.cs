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

        protected ExperimentStep(ExperimentData exp)
        {
            this.exp = exp;
        }

        public virtual bool ready()
        {
            return false;
        }

        public virtual bool isResearchFinished()
        {
            return false;
        }

        public virtual void start(){
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
                return new ExperimentStep(exp);
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
                default:
                    return new ExperimentStep(exp);
            }
        }

        protected virtual string getType()
        {
            return "";
        }
    }

    public class ResourceExperimentStep : ExperimentStep
    {
        private const string RES_VALUE = "Res";
        private const string AMOUNT_VALUE = "Amount";

        protected string res;
        protected float amount;

        internal ResourceExperimentStep(ExperimentData exp)
            : base(exp)
        { }

        public ResourceExperimentStep(ExperimentData exp, string res, float amount)
            : base(exp)
        {
            this.res = res;
            this.amount = amount;
        }

        protected override string getType()
        {
            return "ResStep";
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

        public override void start()
        {
            if(exp.state == ExperimentState.INSTALLED){
                ((LabEquipment)exp.store).createResourceInLab(res, amount);
            }
        }

        public override void finishStep()
        {
            if (exp.state == ExperimentState.RUNNING && isResearchFinished())
            {
                ((LabEquipment)exp.store).setResourceMaxAmount(res, 0f); ;
            }
        }

    }
}
