using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NE_Science
{
    public class MatPhaseExp : ExperimentCore
    {

        protected override void setPhases()
        {
            phase = new MaterialExpPhase(this, 15);
        }
    }
}
