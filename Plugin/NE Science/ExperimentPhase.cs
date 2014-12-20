using System;
using System.Collections.Generic;
using UnityEngine;

namespace NE_Science
{
    public class ExperimentPhase 
    {

        protected PhaseExperimentCore exp;

        public ExperimentPhase()
        {
        }

        public ExperimentPhase(PhaseExperimentCore experiment)
        {
            exp = experiment;
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

    }
}
