using System;
using System.Collections.Generic;
using UnityEngine;

namespace NE_Science
{
    public class MatPhaseExp : PhaseExperimentCore
    {

        [KSPField(isPersistant = false)]
        public int testPointsRequired;

        protected override void setPhases()
        {
            NE_Helper.log("set Phase");
            phase = new MaterialExpPhase(this, testPointsRequired);
        }
    }
}
