using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NE_Science
{
    public class KerbalResearchExperimentData : MultiStepExperimentData<KerbalResearchStep>
    {
        private const string TEST_SUBJECTS_NEEDED = "SubjectsNeeded";

        private int testSubjectsNeeded;

        private Guid cachedVesselID;
        private int partCount;
        private List<MPL_Module> physicsLabCache = null;

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

        public override string getDescription(string linePrefix = "")
        {
            string desc = base.getDescription(linePrefix);
            desc += "\n" + linePrefix + "Test subjects needed: " + getTestSubjectsNeeded();
            return desc;
        }

        public int getTestSubjectsNeeded()
        {
            return testSubjectsNeeded;
        }

        internal override string getStateString()
        {
            string s = base.getStateString();
            switch(state){
                case ExperimentState.RUNNING:
                    s += " " + getActiveStep().getSubjectName();
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
                    if (isTestSubjectAvailable())
                    {
                        return "Start " + getAbbreviation();
                    }
                    else
                    {
                        return "Show " + getAbbreviation() + " Status";
                    }

                case ExperimentState.RUNNING:
                    if (getActiveStep().isResearchFinished())
                    {
                        return "End " + getAbbreviation() + " " + getActiveStep().getSubjectName();
                    }
                    else
                    {
                        return "";
                    }
                default:
                    return "";
            }
        }

        internal override void updateCheck()
        {
            base.updateCheck();
            if (state == ExperimentState.RUNNING)
            {
                List<string> crewMembers = getAllLabCrewMembers();
                if (!crewMembers.Contains(getActiveStep().getSubjectName()))
                {
                    string debug = "";
                        foreach(string member in crewMembers){
                            debug += member + ", ";
                        }
                    NE_Helper.log("Aborting Crew Members: " + debug + "; Subject; " + getActiveStep().getSubjectName());
                    getActiveStep().abortStep();
                    state = ExperimentState.INSTALLED;
                }
            }
        }

        internal ChooseTestSubject getTestSubjectGuiComponent()
        {
            ChooseTestSubject t = store.getPartGo().GetComponent<ChooseTestSubject>();
            if (t == null)
            {
                t = store.getPartGo().AddComponent<ChooseTestSubject>();
            }
            return t;
        }

        internal bool isTestSubjectAvailable()
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
                try {
                    if(store==null) {
                        NE_Helper.logError("getAllLabCrewMembers: store is null!");
                    }
                    Lab lab = ((LabEquipment)store).getLab();
                    if(lab==null) {
                        NE_Helper.logError("getAllLabCrewMembers: lab is null!");
                    }
                    if(lab.part==null) {
                        NE_Helper.logError("getAllLabCrewMembers: lab.part is null!");
                    }
                    if(lab.part.protoModuleCrew==null) {
                        NE_Helper.logError("getAllLabCrewMembers: lab.part.protoModuleCrew is null!");
                    }
                    foreach (ProtoCrewMember crewMember in lab.part.protoModuleCrew)
                    {
                        members.Add(crewMember.name.Trim());
                    }
                } catch(NullReferenceException nre) {
                    NE_Helper.logError ("getAllLabCrewMembers: nullref!\n" + nre.StackTrace);
                }
            }
            return members;
        }

        public override List<Lab> getFreeLabsWithEquipment(Vessel vessel)
        {
            List<Lab> ret = new List<Lab>();
            List<MPL_Module> allPhysicsLabs;
            if (cachedVesselID == vessel.id && partCount == vessel.parts.Count && physicsLabCache != null)
            {
                allPhysicsLabs = physicsLabCache;
            }
            else
            {
                allPhysicsLabs = new List<MPL_Module>(UnityFindObjectsOfType(typeof(MPL_Module)) as MPL_Module[]);
                physicsLabCache = allPhysicsLabs;
                cachedVesselID = vessel.id;
                partCount = vessel.parts.Count;
                NE_Helper.log("Lab Cache refresh");
            }
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

        public override void start(startCallback cbMethod)
        {
            ChooseTestSubject gui = ((KerbalResearchExperimentData)exp).getTestSubjectGuiComponent();
            gui.showDialog(((KerbalResearchExperimentData)exp).getAvailableLabCrewMembers(), (KerbalResearchExperimentData)exp, cbMethod);
        }

        internal void start(string crewMember, startCallback cbMethod)
        {
            base.start(cbMethod);
            subject = crewMember;
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
