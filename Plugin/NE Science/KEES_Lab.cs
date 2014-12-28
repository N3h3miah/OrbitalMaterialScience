using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NE_Science
{
    class KEES_Lab : Lab
    {

        [KSPField(isPersistant = false)]
        public float ExposureTimePerHour = 1;

        private AttachNode node;

        public override void OnStart(PartModule.StartState state)
        {
            base.OnStart(state);
            if (state == StartState.Editor)
            {
                return;
            }

            Events["stopResearch"].active = false;
            Events["startResearch"].active = false;
            Fields["labStatus"].guiActive = false;


            Generator exposureGenerator = createGenerator(Resources.EXPOSURE_TIME, ExposureTimePerHour, Resources.ELECTRIC_CHARGE, 0);
            generators.Add(exposureGenerator);
        }

        private Generator createGenerator(string resToCreate, float creationRate, string useRes, float usePerUnit)
        {
            Generator gen = new Generator(this.part);
            gen.addRate(resToCreate, -creationRate);
            if (usePerUnit > 0)
                gen.addRate(useRes, usePerUnit);
            return gen;
        }



    }
}
