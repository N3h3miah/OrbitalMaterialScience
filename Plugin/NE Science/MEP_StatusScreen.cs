using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NE_Science
{
    class MEP_StatusScreen : InternalModule
    {
        private double lastUpdate = 0;

        public override void OnUpdate()
        {
            base.OnUpdate();
            if (lastUpdate + 2 < Time.time)
            {
                lastUpdate = Time.time;
                ExposureLab lab = part.GetComponent<ExposureLab>();
                NE_Helper.log("MEP Lab Status: " + lab.labStatus);

            }
        }
    }
}
