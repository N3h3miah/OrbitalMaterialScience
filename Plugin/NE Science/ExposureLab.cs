/*
 *   This file is part of Orbital Material Science.
 *   
 *   Part of the code may originate from Station Science ba ether net http://forum.kerbalspaceprogram.com/threads/54774-0-23-5-Station-Science-(fourth-alpha-low-tech-docking-port-experiment-pod-models)
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
    class ExposureLab:Lab
    {

        [KSPField(isPersistant = true)]
        public bool running = false;

        [KSPField(isPersistant = true)]
        public int expID = -1;

        [KSPField(isPersistant = true, guiActive = true, guiName = "Experiment Name")]
        public string experimentName = "No Experiment";

        private string animationName = "StartExperiment";

        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            if (state == StartState.Editor)
            {
                return;
            }

            if (running)
            {
                NE_Helper.log("Running Lab onStart. Set Animationstate");
                Animation anim = part.FindModelAnimators(animationName).FirstOrDefault();
                if (anim != null)
                {
                    anim[animationName].speed = 1f;
                    anim[animationName].normalizedTime = 1f;
                    anim.Play();
                }
            }
            else
            {
                Animation anim = part.FindModelAnimators(animationName).FirstOrDefault();
                if (anim != null)
                {
                    anim[animationName].speed = -1f;
                    anim[animationName].normalizedTime = 0f;
                    anim.Play();
                }
            }

        }

        protected override bool isActive()
        {
            return doResearch && running && part.protoModuleCrew.Count >= minimumCrew && !ExperimentCore.checkBoring(vessel, false);
        }

        public override void checkStatusLabRunning()
        {
            if (running)
            {
                List<ExposureExperiment> allExpExps = new List<ExposureExperiment>(GameObject.FindObjectsOfType(typeof(ExposureExperiment)) as ExposureExperiment[]);
                bool expFound = false;
                foreach (ExposureExperiment exp in allExpExps)
                {
                    if (exp.expID == expID)
                    {
                        expFound = true;
                        break;
                    }
                }
                if (!expFound)
                {
                    NE_Helper.log("Running experiment lost.");
                    stopExperiment();
                }
            }
        }

        public void startExperiment(string name, int expIDp)
        {
            NE_Helper.log("Starting Experiment: " + name);
            running = true;
            experimentName = name;
            expID = expIDp;
            Animation anim = part.FindModelAnimators(animationName).FirstOrDefault();
            if (anim != null)
            {
                anim[animationName].speed = 1f;
                anim[animationName].normalizedTime = 0f;
                anim.Play();
            }
            else
            {
                NE_Helper.log("no Animation; Name: " + animationName);
            }
        }

        public void stopExperiment()
        {
            running = false;
            experimentName = "No Experiment";
            expID = -1;
            Animation anim = part.FindModelAnimators(animationName).FirstOrDefault();
            if (anim != null)
            {
                anim[animationName].speed = -1f;
                anim[animationName].normalizedTime = 1f;
                anim.Play();
            }
            else
            {
                NE_Helper.log("no Animation; Name: " + animationName);
            }
        }

    }
}
