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
using System.Linq;
using System.Text;
using UnityEngine;

namespace NE_Science
{
    class MEP_Animations : PartModule
    {

        private ExposureLab lab;

        private Light warnLight;
        private Light warnPointLight;

        private bool error = false;

        public override void OnStart(PartModule.StartState state)
        {
            base.OnStart(state);

            if (state == StartState.Editor)
            {
                return;
            }

            lab = gameObject.GetComponent<ExposureLab>();

            foreach (Light child in gameObject.GetComponentsInChildren(typeof(Light)))
            {
                if (child.name == "rotationLight")
                {
                    warnLight = child;
                }
                else if (child.name == "WarnPointLlight")
                {
                    warnPointLight = child;
                }

            }
        }

        public override void OnUpdate()
        {
            if (lab.MEPlabState == NE_Helper.MEP_ERROR_ON_START || lab.MEPlabState == NE_Helper.MEP_ERROR_ON_STOP)
            {
                if (!error)
                {
                    switchLightsOn();
                }
                warnLight.transform.Rotate(Time.deltaTime * 180, 0, 0);
            }
            else
            {
                if (error)
                {
                    switchLightsOff();
                }
            }


        }

        private void switchLightsOff()
        {
            if (warnLight != null)
            {
                warnLight.intensity = 0f;
            }
            else
            {
                NE_Helper.logError("WarnLight null");
            }
            if (warnPointLight != null)
            {
                warnPointLight.intensity = 0.0f;
            }
            else
            {
                NE_Helper.logError("WarnPointLight null");
            }
        }

        private void switchLightsOn()
        {
            if (warnLight != null)
            {
                warnLight.intensity = 6f;
            }
            else
            {
                NE_Helper.logError("WarnLight null");
            }
            if (warnPointLight != null)
            {
                warnPointLight.intensity = 0.5f;
            }
            else
            {
                NE_Helper.logError("WarnPointLight null");
            }
        }
    }
}
