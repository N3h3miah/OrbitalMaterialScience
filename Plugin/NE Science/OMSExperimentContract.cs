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
    public class OMSExperimentContract : Contract
    {

        public const string TARGET_BODY = "targetBody";
        public const string EXPERIMENT_STRING = "experiment";

        CelestialBody targetBody = null;
        AvailablePart experiment = null;

        protected override bool Generate()
        {
            targetBody = getTargetBody();
            if (targetBody == null)
            {
                targetBody = Planetarium.fetch.Home;
            }

            if (!setTargetExperiment(getTargetExperimentString())) return false;

            //TODO AddParameter
            AddParameter(new Parameters.OMSDoExperimentParameter(targetBody, experiment));
            AddParameter(new Parameters.OMSReturnExperimentParameter(targetBody, experiment));

            base.SetExpiry();
            base.SetScience(2.25f, targetBody);
            base.SetDeadlineYears(1f, targetBody);
            base.SetReputation(150f, 60f, targetBody);
            base.SetFunds(15000f, 50000f, 35000f, targetBody);
            return true;
        }

        private string getTargetExperimentString()
        {
            return "NE.KEES.PPMD";
        }

        private bool setTargetExperiment(string exp)
        {
            experiment = PartLoader.getPartInfoByName(exp);
            if (experiment == null)
            {
                return false;
            }
            return true;
        }

        private CelestialBody getTargetBody()
        {
            //List<CelestialBody> bodies = Contract.GetBodies_Reached(true, false);
            //return bodies[UnityEngine.Random.Range(0, bodies.Count - 1)];

            return Planetarium.fetch.Home;

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
            return (targetBody.bodyName + experiment.name);
        }
        protected override string GetTitle()
        {
            return "Run " + experiment.title + " in orbit around " + targetBody.theName + " and return it to Kerbin";
        }
        protected override string GetDescription()
        {
            //those 3 strings appear to do nothing
            return TextGen.GenerateBackStories(Agent.Name, Agent.GetMindsetString(), "science", "station", "expand knowledge", new System.Random().Next());
        }
        protected override string GetSynopsys()
        {
            return "Run " + experiment.title + " in orbit around " + targetBody.theName;
        }
        protected override string MessageCompleted()
        {
            return "You have succesfully run " +experiment.title + " in orbit around " + targetBody.theName;
        }

        protected override void OnLoad(ConfigNode node)
        {
            int bodyID = int.Parse(node.GetValue(TARGET_BODY));
            foreach (var body in FlightGlobals.Bodies)
            {
                if (body.flightGlobalsIndex == bodyID)
                    targetBody = body;
            }
            setTargetExperiment(node.GetValue(EXPERIMENT_STRING));
        }
        protected override void OnSave(ConfigNode node)
        {
            int bodyID = targetBody.flightGlobalsIndex;
            node.AddValue(TARGET_BODY, bodyID);

            node.AddValue(EXPERIMENT_STRING, experiment.name);
        }

        //for testing purposes
        public override bool MeetRequirements()
        {
            return true;
        }

    }
}