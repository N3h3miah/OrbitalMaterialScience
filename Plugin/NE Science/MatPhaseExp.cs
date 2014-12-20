using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NE_Science
{
    public class MatPhaseExp : ExperimentPhaseCore
    {

        [KSPField(isPersistant = false)]
        public int testPointsRequired;

        protected override void setPhases()
        {
            phase = new MaterialExpPhase(this, testPointsRequired);
        }
    }
}
