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

namespace NE_Science
{
    class KEES_PEC : PartModule
    {

        [KSPField(isPersistant = false)]
        public string nodeName;

        [KSPField(isPersistant = false)]
        public double maxGforce = 2.5;

        private AttachNode node = null;
        private KEESExperiment exp = null;

        public override void OnStart(PartModule.StartState state)
        {
            base.OnStart(state);

            node = part.findAttachNode(nodeName);
            if (node == null)
            {
                NE_Helper.logError("KEES PEC: AttachNode not found");
                node = part.attachNodes.First();
            }

            /* Run this as a coroutine so the experiment aborts if the
             * PEC decouples from the ship. */
            StartCoroutine(checkNode());
        }

        public System.Collections.IEnumerator checkNode()
        {
            while (true)
            {
                checkForExp();
                yield return new UnityEngine.WaitForSeconds(1f);
            }
        }

        private void checkForExp()
        {
            if (node != null && node.attachedPart != null)
            {
                KEESExperiment newExp = node.attachedPart.GetComponent<KEESExperiment>();
                if (newExp != null)
                {
                    if (exp == null)
                    {
                        exp = newExp;
                        exp.dockedToPEC(true);
                    } else if (exp != newExp) {
                        exp.dockedToPEC(false);
                        exp = newExp;
                        exp.dockedToPEC(true);
                    }
                }
            } else if (exp != null) {
                exp.dockedToPEC(false);
                exp = null;
            }
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            if( vessel != null && !vessel.isEVA && vessel.geeForce > maxGforce) {
                part.decouple ();
            }
        }

    }
}
