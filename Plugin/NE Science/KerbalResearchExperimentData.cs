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
                    if (getAvailableLabCrewMembers().Count == 0)
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

        internal override void updateCheck()
        {
            base.updateCheck();
            if (state == ExperimentState.RUNNING)
            {
                List<string> crewMembers = getAllLabCrewMembers();
                if (!crewMembers.Contains(((KerbalResearchStep)getActiveStep()).getSubjectName()))
                {
                    string debug = "";
                        foreach(string member in crewMembers){
                            debug += member + ", ";
                        }
                    NE_Helper.log("Aborting Crew Members: " + debug + "; Subject; " + ((KerbalResearchStep)getActiveStep()).getSubjectName());
                    ((KerbalResearchStep)getActiveStep()).abortStep();
                    state = ExperimentState.INSTALLED;
                }
            }
        }

        private bool isTestSubjectAvailable()
        {
            List<string> members = getAvailableLabCrewMembers();
            return members.Count > 0;
        }

        internal List<string> getAvailableLabCrewMembers()
        {
            List<string> members = new List<string>();
            if (state == ExperimentState.INSTALLED)
            {
                List<string> labCrew = getAllLabCrewMembers();
                foreach (string crewMember in labCrew)
                {
                    bool foundInStep = false;
                    foreach (ExperimentStep s in steps)
                    {
                        if (((KerbalResearchStep)s).getSubjectName() == crewMember)
                        {
                            foundInStep = true;
                        }
                    }
                    if (!foundInStep) members.Add(crewMember);
                }
            }
            return members;
        }

        internal List<string> getAllLabCrewMembers()
        {
            List<string> members = new List<string>();
            if (state == ExperimentState.INSTALLED || state == ExperimentState.RUNNING)
            {
                Lab lab = ((LabEquipment)store).getLab();
                foreach (ProtoCrewMember crewMember in lab.part.protoModuleCrew)
                {
                    members.Add(crewMember.name.Trim());
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
            return ((KerbalResearchExperimentData)exp).getAvailableLabCrewMembers().FirstOrDefault();
        }

        internal void abortStep()
        {
            NE_Helper.log("Abort Research");
            ScreenMessages.PostScreenMessage("Test subject left lab. Research aborted!", 6, ScreenMessageStyle.UPPER_CENTER);
            subject = "";
            ((LabEquipment)exp.store).setResourceMaxAmount(res, 0f);
        }
    }
}
