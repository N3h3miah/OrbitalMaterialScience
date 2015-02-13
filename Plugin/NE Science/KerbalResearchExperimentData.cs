using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NE_Science
{
    public class KerbalResearchExperimentData : MultiStepExperimentData
    {
        private const string TEST_SUBJECTS_NEEDED = "SubjectsNeeded";

        private int testSubjectsNeeded;

        protected KerbalResearchExperimentData(string id, string type, string name, string abb, EquipmentRacks eq, float mass, int testSubjectsNeeded)
            : base(id, type, name, abb, eq, mass)
        {
            this.testSubjectsNeeded = testSubjectsNeeded;
            steps = new KerbalResearchStep[testSubjectsNeeded];
        }

        public override ConfigNode getNode()
        {
            ConfigNode node =  base.getNode();

            node.AddValue(TEST_SUBJECTS_NEEDED, testSubjectsNeeded);
            return node;
        }

        protected override void load(ConfigNode node)
        {
            base.load(node);
            testSubjectsNeeded = int.Parse(node.GetValue(TEST_SUBJECTS_NEEDED));
        }

        public override bool canInstall(Vessel vessel)
        {
            List<Lab> labs = getFreeLabsWithEquipment(vessel);
            return labs.Count > 0 && state == ExperimentState.STORED;
        }

        internal override string getStateString()
        {
            string s = base.getStateString();
            switch(state){
                case ExperimentState.RUNNING:
                    s += " " + ((KerbalResearchStep) getActiveStep()).getSubjectName();
                    break;
                case ExperimentState.INSTALLED:
                    if (getAvailableCrewMembers().Count == 0)
                    {
                        s += " No test subjects";
                    }
                    break;
            }
            
            return s;
        }

        internal override string getActionString()
        {
            switch (state)
            {
                case ExperimentState.INSTALLED:
                    return "Start " + getAbbreviation();

                case ExperimentState.RUNNING:
                    if (getActiveStep().isResearchFinished())
                    {
                        return "End " + getAbbreviation() + " " + ((KerbalResearchStep)getActiveStep()).getSubjectName();
                    }
                    else
                    {
                        return "";
                    }
                default:
                    return "";
            }
        }

        internal override bool canRunAction()
        {
            switch (state)
            {
                case ExperimentState.INSTALLED:
                    return base.canRunAction() && isTestSubjectAvailable();

                case ExperimentState.RUNNING:
                    return base.canRunAction();;
                default:
                    return base.canRunAction();
            }
        }

        private bool isTestSubjectAvailable()
        {
            List<string> members = getAvailableCrewMembers();
            return members.Count > 0;
        }

        internal List<string> getAvailableCrewMembers()
        {
            List<string> members = new List<string>();
            if (state == ExperimentState.INSTALLED)
            {
                Lab lab = ((LabEquipment)store).getLab();
                foreach (ProtoCrewMember crewMember in lab.part.protoModuleCrew)
                {
                    bool foundInStep = false;
                    foreach (ExperimentStep s in steps)
                    {
                        if (((KerbalResearchStep)s).getSubjectName().Trim() == crewMember.name.Trim())
                        {
                            foundInStep = true;
                        }
                    }
                    if (!foundInStep) members.Add(crewMember.name);
                }
            }
            return members;
        }

        public override List<Lab> getFreeLabsWithEquipment(Vessel vessel)
        {
            List<Lab> ret = new List<Lab>();
            List<MPL_Module> allPhysicsLabs = new List<MPL_Module>(UnityFindObjectsOfType(typeof(MPL_Module)) as MPL_Module[]);
            foreach (MPL_Module lab in allPhysicsLabs)
            {
                if (lab.vessel == vessel && lab.hasEquipmentFreeExperimentSlot(neededEquipment))
                {
                    ret.Add(lab);
                }
            }
            return ret;
        }
    }

    public class KerbalResearchStep : ResourceExperimentStep
    {
        private const string SUBJECT_NAME = "subject";
        private string subject = "";

        internal KerbalResearchStep(ExperimentData exp, string resName, float amount, int index)
            : base(exp, resName, amount, "KerbalResStep",  "name", index)
        { 
        }

        public KerbalResearchStep(ExperimentData exp, string name, int index):base(exp, "KerbalResStep", name, index)
        { }

        protected override void load(ConfigNode node)
        {
            base.load(node);
            subject = node.GetValue(SUBJECT_NAME);
        }

        public override ConfigNode getNode()
        {
            ConfigNode node = base.getNode();

            node.AddValue(SUBJECT_NAME, subject);
            return node;
        }

        public string getSubjectName()
        {
            return subject;
        }

        public override bool start()
        {
            bool basRet = base.start();
            subject = getCrewMember();

            return basRet;
        }

        private string getCrewMember()
        {
            return ((KerbalResearchExperimentData)exp).getAvailableCrewMembers().FirstOrDefault();
        }
    }
}
