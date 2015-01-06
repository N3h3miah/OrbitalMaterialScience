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
    class KEES_PayloadCarrier : PartModule
    {

        private bool kasInstalled = false;


        public override void OnStart(PartModule.StartState state)
        {
            base.OnStart(state);

            KAS.KASModuleGrab kasGrab = part.GetComponent<KAS.KASModuleGrab>();
            if (kasGrab == null)
            {
                kasInstalled = false;
                NE_Helper.log("No KAS");
            }
            else
            {
                kasInstalled = false;
                NE_Helper.log("KAS Installed");
            }
        }
    }
}
