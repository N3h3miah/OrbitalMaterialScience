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
    class MSL_Printer_Animation : InternalModule
    {

        private const float BASE_SPEED = 0.01f;
        private const float HEAD_SPEED = 0.02f;

        private const float HEAD_MAX = 0.54f;
        private const float HEAD_MIN = -0.54f;

        private const float BASE_MAX = 0.35f;
        private const float BASE_MIN = -0.33f;

        private Transform headBase;
        private Transform head;

        private int count = 0;

        private int baseDirection = 1;
        private int headDirection = 1;

        public override void OnFixedUpdate()
        {
            base.OnUpdate();
            if (count == 0)
            {
                if (headBase == null || head == null)
                {
                    initPartObjects();
                }
                PhysicsMaterialsLab lab = part.GetComponent<PhysicsMaterialsLab>();
                if (lab.printerRunning)
                {
                    moveBase();
                    moveHead();
                }
            }
            count = (count + 1) % 2;
        }

        private void moveHead()
        {
            float pos = head.localPosition.y;
            pos += HEAD_SPEED * -headDirection; //I dont understand why it has to be -headDirection to work
            if (pos > HEAD_MAX || pos < HEAD_MIN)
            {
                headDirection = headDirection * -1;
            }
            else
            {
                float movment = HEAD_SPEED * headDirection;
                head.Translate(0, movment, 0, Space.Self);
            }
        }

        private void moveBase()
        {
            float pos = headBase.localPosition.x;
            pos += BASE_SPEED * baseDirection;
            if (pos > BASE_MAX || pos < BASE_MIN)
            {
                baseDirection = baseDirection * -1;
            }
            else
            {
                headBase.Translate(BASE_SPEED * baseDirection, 0, 0);
            }
        }

        private void initPartObjects()
        {
            if (part.internalModel != null)
            {
                GameObject labIVA = part.internalModel.gameObject.transform.GetChild(0).GetChild(0).gameObject;
                if (labIVA.GetComponent<MeshFilter>().name == "Lab1IVA")
                {
                    NE_Helper.log("set printer transforms");
                    GameObject printer = labIVA.transform.GetChild(0).gameObject;
                    //GameObject cir = labIVA.transform.GetChild(1).gameObject;
                    headBase = printer.transform.GetChild(1).GetChild(0);
                    head = headBase.GetChild(0);
                }
            }
        }
    }
}
