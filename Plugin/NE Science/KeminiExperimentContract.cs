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
using KSP;
using KSPAchievements;

namespace NE_Science.Contracts
{
    public class KeminiExperimentContract : Contract
    {

        public const string TARGET_BODY = "targetBody";
        public const string EXPERIMENT_STRING = "experiment";

        CelestialBody targetBody = null;
        ExperimentData experiment = null;

        protected override bool Generate()
        {
            NE_Helper.log("Generate Contract");
            if (activeContracts() >= getMaxContracts())
            {
                NE_Helper.log("Generate Contract: Max Contracts reached: " + getMaxContracts());
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
            AddParameter(new Parameters.ExperimentDataDoExperimentParameter(targetBody, experiment));
            AddParameter(new Parameters.ExperimentDataReturnExperimentParameter(targetBody, experiment));

            NE_Helper.log("Generate Contract: set Values ");
            base.SetExpiry(10, 100);
            base.SetScience(5f, targetBody);
            base.SetDeadlineYears(1f, targetBody);
            base.SetReputation(80f, 30f, targetBody);
            base.SetFunds(15000f, 30000f, 5000f, targetBody);

            NE_Helper.log("Generate Contract: done Exp: " + experiment.getAbbreviation() + " Body: " + targetBody.name + " funds Adv: " + this.FundsAdvance);
            return true;
        }

        private int getMaxContracts()
        {
            int bonus = (int)(3f * ScenarioUpgradeableFacilities.GetFacilityLevel("SpaceCenter/MissionControl"));
            return 3 + bonus;
        }

        private int activeContracts(ExperimentData expData = null, CelestialBody body = null)
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
                KeminiExperimentContract keminiContract = con as KeminiExperimentContract;
                if (keminiContract != null && (keminiContract.ContractState == Contract.State.Active ||
                    keminiContract.ContractState == Contract.State.Offered) &&
                  (expData == null || keminiContract.experiment != null) &&
                  (body == null || keminiContract.targetBody != null) &&
                  ((expData == null || expData.getId() == keminiContract.experiment.getId()) &&
                   (body == null || body.theName == keminiContract.targetBody.theName)))
                    ret += 1;
            }
            return ret;
        }

        private int activeAndDoneContracts(ExperimentData expData = null, CelestialBody body = null)
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
                KeminiExperimentContract keminiCon = con as KeminiExperimentContract;
                if (keminiCon != null && (keminiCon.ContractState == Contract.State.Active ||
                    keminiCon.ContractState == Contract.State.Offered ||
                    keminiCon.ContractState == Contract.State.Completed) &&
                  (expData == null || keminiCon.experiment != null) &&
                  (body == null || keminiCon.targetBody != null) &&
                  ((expData == null || expData.getId() == keminiCon.experiment.getId()) &&
                   (body == null || body.theName == keminiCon.targetBody.theName)))
                    ret += 1;
            }
            return ret;
        }

        private ExperimentData getTargetExperiment()
        {
            List<ExperimentData> unlockedExperiments = ExperimentFactory.getAvailableExperiments(ExperimentFactory.KEMINI_EXPERIMENTS);
            List<ExperimentData> unlookedNoContract = new List<ExperimentData>();
            foreach (ExperimentData exp in unlockedExperiments)
            {
                if (activeAndDoneContracts(exp, targetBody) == 0)
                {
                    unlookedNoContract.Add(exp);
                }
            }
            if (unlookedNoContract.Count == 0)
            {
                return null;
            }
            else
            {
                return unlookedNoContract[UnityEngine.Random.Range(0, unlookedNoContract.Count)];
            }
        }

        private bool setTargetExperiment(ExperimentData exp)
        {
            if (exp == null) {
                NE_Helper.log("Generate Contract: Experiment null");
                return false; }
            NE_Helper.log("Generate Contract: Experiment: " + exp.getAbbreviation());
            this.experiment = exp;
            return true;
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
            return (targetBody.bodyName + experiment.getId());
        }
        protected override string GetTitle()
        {
            return "Run experiment " + experiment.getAbbreviation() + " in orbit around " + targetBody.theName + " and return it to Kerbin";
        }
        protected override string GetDescription()
        {
            //those 3 strings appear to do nothing
            return TextGen.GenerateBackStories(Agent.Name, Agent.GetMindsetString(), "science", "station", "expand knowledge", new System.Random().Next());
        }
        protected override string GetSynopsys()
        {
            return "Run experiment " + experiment.getName() + " in orbit around " + targetBody.theName;
        }
        protected override string MessageCompleted()
        {
            return "You have succesfully run the experiment " + experiment.getAbbreviation() + " in orbit around " + targetBody.theName;
        }

        protected override void OnLoad(ConfigNode node)
        {
            int bodyID = int.Parse(node.GetValue(TARGET_BODY));
            foreach (var body in FlightGlobals.Bodies)
            {
                if (body.flightGlobalsIndex == bodyID)
                    targetBody = body;
            }
            setTargetExperiment((KeminiExperimentData)KeminiExperimentData.getExperimentDataFromNode(node.GetNode(ExperimentData.CONFIG_NODE_NAME)));
        }
        protected override void OnSave(ConfigNode node)
        {
            int bodyID = targetBody.flightGlobalsIndex;
            node.AddValue(TARGET_BODY, bodyID);

            node.AddNode(experiment.getNode());
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

            return ExperimentFactory.getAvailableExperiments(ExperimentFactory.KEMINI_EXPERIMENTS).Count > 0;
        }
    }
}