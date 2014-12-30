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
using UnityEngine;

namespace NE_Science
{
    public class ExperimentPhase 
    {

        protected PhaseExperimentCore exp;

        private string name = "";

        public ExperimentPhase()
        {
        }

        public ExperimentPhase(PhaseExperimentCore experiment, string n)
        {
            exp = experiment;
            name = n;
        }

        //Create the resources needed to finish the Experiment. Subclasses should override this.
        public virtual void createResources()
        {

        }

        public virtual void stopResearch()
        {

        }

        public virtual void checkFinished()
        {

            if (isFinished())
            {
                exp.finished();
            }
        }

        public virtual bool isFinished()
        {

            return true;
        }

        public virtual void checkLabFixed()
        {

        }

        public virtual void checkUndocked()
        {

        }

        public virtual void checkBiomeChange()
        {
            double numTestPoints = exp.getResourceAmount("TestPoints");
            double numExposureTime = exp.getResourceAmount("ExposureTime");
            int sciCount = exp.GetScienceCount();


            var subject = ScienceHelper.getScienceSubject(exp.experimentID, exp.vessel);
            string subjectId = ((subject == null) ? "" : subject.id);
            if (subjectId != "" && exp.last_subjectId != "" && exp.last_subjectId != subjectId &&
                (numTestPoints > 0 || numExposureTime > 0))
            {
                exp.biomeChanged();
            }
            exp.last_subjectId = subjectId;
        }

        public virtual void checkForLabs(bool ready)
        {
            if (!ready)
            {
                exp.labFound();
            }
        }

        public virtual string getInfo()
        {
            return "";
        }


        public virtual bool startExperiment()
        {
            return true;
        }

        public virtual void biomeChanged()
        {
            
        }

        public virtual void undockedRunningExp()
        {
           
        }

        public virtual bool finished()
        {
            return true;
        }


        public virtual void load(ConfigNode node)
        {
        }

        public virtual void save(ConfigNode node)
        {
        }

        public virtual int getExperimentID()
        {
            return Int32.MinValue;
        }

        public virtual void done()
        {
            stopResearch();
        }

        public bool hasName()
        {
            return name.Length > 0;
        }

        public string getName()
        {
            return name;
        }
    }
}
