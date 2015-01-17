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
    class MSL_FFR_Animation : InternalModule
    {

        private const float PUMP1_SPEED = 10;
        private const float PUMP2_SPEED = 15;

        private Transform pump1;
        private Transform pump2;

        private int count = 0;

        public override void OnFixedUpdate()
        {
            base.OnUpdate();
            if (count == 0)
            {
                if (pump1 == null || pump2 == null)
                {
                    initPartObjects();
                }
                PhysicsMaterialsLab lab = part.GetComponent<PhysicsMaterialsLab>();
                if (lab.ffrRunning)
                {
                    if (pump1 != null)
                    {
                        pump1.Rotate(PUMP1_SPEED, 0, 0);
                    }
                    if (pump2 != null)
                    {
                        pump2.Rotate(PUMP2_SPEED, 0, 0);
                    }
                }
            }
            count = (count + 1) % 2;
        }

        private void initPartObjects()
        {
            if (part.internalModel != null)
            {
                GameObject labIVA = part.internalModel.gameObject.transform.GetChild(0).GetChild(0).gameObject;
                if (labIVA.GetComponent<MeshFilter>().name == "Lab1IVA")
                {
                    NE_Helper.log("set pump transforms");
                    //printer = labIVA.transform.GetChild(0).gameObject;
                    //GameObject cir = labIVA.transform.GetChild(1).gameObject;
                    GameObject ffr = labIVA.transform.GetChild(2).gameObject;
                    pump1 = ffr.transform.GetChild(1);
                    pump2 = ffr.transform.GetChild(2);
                }
            }
        }
    }
}
