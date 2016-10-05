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
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Contracts;
using Contracts.Parameters;
using Contracts.Agents;
using KSP;
using KSPAchievements;

namespace NE_Science.Contracts
{
    public class KEESExperimentContract : Contract
    {

        public const string TARGET_BODY = "targetBody";
        public const string EXPERIMENT_STRING = "experiment";

        public const string KEES_PC = "NE.KEES.PC";
        public const string KEES_PEC = "NE.KEES.PEC";
        public const string KEES_PPMD = "NE.KEES.PPMD";
        public const string KEES_ODC = "NE.KEES.OCD";
        public const string KEES_POSAI = "NE.KEES.POSA1";
        public const string KEES_POSAII = "NE.KEES.POSA2";

        static readonly List<Experiment> experimentParts =
          new List<Experiment> { new Experiment(KEES_PPMD, "KEES Polished Plate Micrometeoroid and Debris (PPMD)", "KEES PPMD Experiment", "KEES PPMD"),
              new Experiment(KEES_POSAI, "KEES Passive Optical Sample Assemblies I (POSA I)", "KEES POSA I Experiment", "KEES POSA I"),
               new Experiment(KEES_ODC, "KEES Orbital Debris Collector", "KEES ODC Experiment (ODC)", "KEES ODC"),
               new Experiment(KEES_POSAII, "KEES Passive Optical Sample Assemblies II(POSA II)", "KEES POSA II Experiment", "KEES POSA II") };

        CelestialBody targetBody = null;
        Experiment experiment = null;

        protected override bool Generate()
        {
            if (!DependancyChecker.HasKIS)
            {
                return false;
            }

            NE_Helper.log("Generate Contract");
            if (activeContracts() >= getMaxKEESContracts())
            {
                NE_Helper.log("Generate Contract: Max Contracts reached: " + getMaxKEESContracts());
                return false;
            }
            targetBody = getTargetBody();
            if (targetBody == null)
            {
                NE_Helper.log("Generate Contract: Body null set Kerbin as Target");
                targetBody = Planetarium.fetch.Home;
            }
            else
            {
                NE_Helper.log("Generate Contract: Body: " + targetBody.name);
            }

            if (!setTargetExperiment(getTargetExperiment())) return false;

            NE_Helper.log("Generate Contract: Add Parameter");
            AddParameter(new Parameters.KEESDoExperimentParameter(targetBody, getPartForExperiment(experiment)));
            AddParameter(new Parameters.KEESReturnExperimentParameter(targetBody, getPartForExperiment(experiment)));

            NE_Helper.log("Generate Contract: set Values ");
            base.SetExpiry();
            base.SetScience(5f, targetBody);
            base.SetDeadlineYears(1f, targetBody);
            base.SetReputation(80f, 30f, targetBody);
            base.SetFunds(15000f, 30000f, 5000f, targetBody);
            agent = AgentList.Instance.GetAgent("Nehemiah Engineering");
            NE_Helper.log("Generate Contract: done Exp: " + experiment.getAbbreviation() + " Body: " + targetBody.name + " funds Adv: " + this.FundsAdvance);
            return true;
        }

        private int getMaxKEESContracts()
        {
            //int bonus = (int)(3f * ScenarioUpgradeableFacilities.GetFacilityLevel("SpaceCenter/MissionControl"));
            //return 3 + bonus;
            return 2;
        }

        private int activeContracts(String experimentPartName = null, CelestialBody body = null)
        {
            int ret = 0;
            if (ContractSystem.Instance == null)
            {
                return 0;
            }
            if (ContractSystem.Instance.Contracts == null)
            {
                return 0;
            }
            foreach (Contract con in ContractSystem.Instance.Contracts)
            {
                KEESExperimentContract keesCon = con as KEESExperimentContract;
                if (keesCon != null && (keesCon.ContractState == Contract.State.Active ||
                    keesCon.ContractState == Contract.State.Offered) &&
                  (experimentPartName == null || keesCon.experiment != null) &&
                  (body == null || keesCon.targetBody != null) &&
                  ((experimentPartName == null || experimentPartName == keesCon.experiment.getPartName()) &&
                   (body == null || body.theName == keesCon.targetBody.theName)))
                    ret += 1;
            }
            return ret;
        }

        private int activeAndDoneContracts(String experimentPartName = null, CelestialBody body = null)
        {
            int ret = 0;
            if (ContractSystem.Instance == null)
            {
                return 0;
            }
            if (ContractSystem.Instance.Contracts == null)
            {
                return 0;
            }
            foreach (Contract con in ContractSystem.Instance.Contracts)
            {
                KEESExperimentContract keesCon = con as KEESExperimentContract;
                if (keesCon != null && (keesCon.ContractState == Contract.State.Active ||
                    keesCon.ContractState == Contract.State.Offered ||
                    keesCon.ContractState == Contract.State.Completed) &&
                  (experimentPartName == null || keesCon.experiment != null) &&
                  (body == null || keesCon.targetBody != null) &&
                  ((experimentPartName == null || experimentPartName == keesCon.experiment.getPartName()) &&
                   (body == null || body.theName == keesCon.targetBody.theName)))
                    ret += 1;
            }
            return ret;
        }

        private Experiment getTargetExperiment()
        {
            List<Experiment> unlockedExperiments = getUnlockedKEESExperiments();
            List<Experiment> unlockedNoContract = new List<Experiment>();
            foreach (Experiment exp in unlockedExperiments)
            {
                if (activeAndDoneContracts(exp.getPartName(), targetBody) == 0)
                {
                    unlockedNoContract.Add(exp);
                }
            }
            if (unlockedNoContract.Count == 0)
            {
                return null;
            }
            else
            {
                return unlockedNoContract[UnityEngine.Random.Range(0, unlockedNoContract.Count)];
            }
        }

        private List<Experiment> getUnlockedKEESExperiments()
        {
            List<Experiment> unlockedParts = new List<Experiment>();
            foreach (Experiment exp in experimentParts)
            {
                if (isPartUnlocked(exp.getPartName()))
                {
                    unlockedParts.Add(exp);
                }
            }
            return unlockedParts;
        }

        private bool setTargetExperiment(Experiment exp)
        {
            if (exp == null) {
                NE_Helper.log("Generate Contract: Experiment null");
                return false; }
            AvailablePart experiment = PartLoader.getPartInfoByName(exp.getPartName());
            if (experiment == null)
            {
                NE_Helper.log("Generate Contract: Experiment Part null");
                return false;
            }
            NE_Helper.log("Generate Contract: Experiment: " + exp.getAbbreviation());
            this.experiment = exp;
            return true;
        }

        private AvailablePart getPartForExperiment(Experiment exp)
        {
            return PartLoader.getPartInfoByName(exp.getPartName());
        }

        private CelestialBody getTargetBody()
        {
            List<CelestialBody> bodies = Contract.GetBodies_Reached(true, false);
            return bodies[UnityEngine.Random.Range(0, bodies.Count)];

        }

        public override bool CanBeCancelled()
        {
            return true;
        }
        public override bool CanBeDeclined()
        {
            return true;
        }

        protected override string GetHashString()
        {
            return (targetBody.bodyName + experiment.getPartName());
        }
        protected override string GetTitle()
        {
            return "Run " + experiment.getShortName() +" in orbit around " + targetBody.theName + " and return it to Kerbin";
        }
        protected override string GetDescription()
        {
            //those 3 strings appear to do nothing
            return TextGen.GenerateBackStories(Agent.Name, Agent.GetMindsetString(), "science", "station", "expand knowledge", new System.Random().Next());
        }
        protected override string GetSynopsys()
        {
            return "Run " + experiment.getName() + " in orbit around " + targetBody.theName;
        }
        protected override string MessageCompleted()
        {
            return "You have succesfully run " + experiment.getShortName() +" in orbit around " + targetBody.theName;
        }

        protected override void OnLoad(ConfigNode node)
        {
            int bodyID = NE_Helper.GetValueAsInt(node, TARGET_BODY);
            foreach (var body in FlightGlobals.Bodies)
            {
                if (body.flightGlobalsIndex == bodyID)
                    targetBody = body;
            }
            setTargetExperiment(getExperimentByPartName(node.GetValue(EXPERIMENT_STRING)));
        }
        protected override void OnSave(ConfigNode node)
        {
            int bodyID = targetBody.flightGlobalsIndex;
            node.AddValue(TARGET_BODY, bodyID);

            node.AddValue(EXPERIMENT_STRING, experiment.getPartName());
        }

        private Experiment getExperimentByPartName(string partName)
        {
            foreach(Experiment exp in experimentParts){
                if (exp.getPartName() == partName)
                {
                    return exp;
                }
            }
            return new Experiment("", "", "", ""); //Nullobject;
        }

        bool isPartUnlocked(string name)
        {
            AvailablePart part = PartLoader.getPartInfoByName(name);
            if (part != null && ResearchAndDevelopment.PartModelPurchased(part))
            {
                return true;
            }
            return false;
        }

        public override bool MeetRequirements()
        {
            CelestialBodySubtree kerbinProgress = null;
            foreach (var node in ProgressTracking.Instance.celestialBodyNodes)
            {
                if (node.Body == Planetarium.fetch.Home)
                    kerbinProgress = node;
            }
            if (kerbinProgress == null)
            {
                return false;
            }

            return (kerbinProgress.orbit.IsComplete &&
                  isPartUnlocked(KEES_PC) &&
                  isPartUnlocked(KEES_PEC) &&
                  isPartUnlocked(KEES_PPMD)) &&
                  ScenarioUpgradeableFacilities.GetFacilityLevel("SpaceCenter/AstronautComplex") > 0.1f;
        }
    }

        public class Experiment
        {

            private string name;
            private string shortName;
            private string abbrev;
            private string partName;

            public Experiment(string partName, string name, string shortName, string abbrev)
            {
                this.partName = partName;
                this.name = name;
                this.shortName = shortName;
                this.abbrev = abbrev;
            }
            public string getPartName()
            {
                return partName;
            }

            public string getName()
            {
                return name;
            }

            public string getShortName()
            {
                return shortName;
            }

            public string getAbbreviation(){
                return abbrev;
        }
    }
}