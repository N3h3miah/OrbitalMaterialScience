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
using System.Text;
using UnityEngine;

namespace NE_Science
{
    class KEES_PEC : PartModule
    {

        [KSPField(isPersistant = false)]
        public string nodeName = "top";

        [KSPField(isPersistant = false)]
        public double maxGforce = 2.5;

        private AttachNode node = null;
        private IKEESExperiment exp = null;

        private int counter = 0;

        [KSPField(isPersistant = true)]
        public bool decoupled = false;

        public override void OnStart(PartModule.StartState state)
        {
            base.OnStart(state);

            node = part.FindAttachNode(nodeName);
            if (node == null)
            {
                NE_Helper.logError("KEES PEC: AttachNode not found");
                node = part.attachNodes[0];
            }

            Events["Decouple"].active = NE_Helper.debugging() && !decoupled;
        }

        /// <summary>
        /// Checks whether a KEESExperiement has just been mounted or unmounted.
        /// </summary>
        private void checkForExp()
        {
            // The following line is needed because Unity does some funky
            // stuff overloading the '==' operator and creating fake 'null'
            // objects in certain situations, such as when they become
            // inactive or destroyed.
            //
            // exp == null           // returns true if exp is null or "fake null"
            // (object)exp == null   // uses the C# '==' operator, so returns true only if exp is really null
            // exp is null           // also avoids the Unity overloaded '==', so also returns true only if exp is really null
            // 
            IKEESExperiment newExp = node?.attachedPart?.GetComponent<IKEESExperiment>();
            if (newExp != null)
            {
                if (exp is null)
                {
                    exp = newExp;
                    exp.OnExperimentMounted();
                    NE_Helper.log("New KEES Experiment installed");
                }
                else if (exp != newExp)
                {
                    exp.OnExperimentUnmounted();
                    exp = newExp;
                    exp.OnExperimentMounted();
                    NE_Helper.log("KEES Experiment switched");
                }
            }
            else if (!(exp is null))
            {
                exp.OnExperimentUnmounted();
                NE_Helper.log("KEES Experiment undocked");
                exp = null;
            }
        }

        /// <summary>
        /// Check whether we have been coupled or decoupled from a Vessel
        /// </summary>
        /// Decoupling can occur on purpose (eg, Player removes the part from the ship using KIS),
        /// or accidentally if the ship undergoes a high-G maneuvre.
        /// TODO: Figure out whether we can hook into some game or KIS events instead as
        ///       running these checks every frame is a bit expensive.
        public override void OnUpdate()
        {
            base.OnUpdate();
            /* Only perform the max-G check if we are attached to a vessel.
             * During KAS grab, vessel can be itself or a Kerbal, and we may
             * get spurious high G's. */
            bool isVesselShip = part.parent != null && vessel != null && !vessel.isEVA;
            if (decoupled && vessel.vesselType != VesselType.Debris)
            {
                NE_Helper.log("Decoupled PEC recovered");
                decoupled = false;
                Events["Decouple"].active = NE_Helper.debugging();
            }
            if (!decoupled && isVesselShip && vessel.geeForce > maxGforce)
            {
                NE_Helper.log ("KEES PEC over max G, decouple\n" + this.ToString ());
                Decouple();
            }
            if (!decoupled && counter == 0)//don't run this every frame
            {
                checkForExp();
            }
            counter = (++counter) % 6;
        }

        /// <summary>
        /// Decouples the PEC from its parent vessel.
        /// </summary>
        /// This can occur due to a high-G maneuvre, or if the Player removes the part
        /// from the ship, or due to a GUI click.
        [KSPEvent(guiActive = true, guiActiveUnfocused = true, unfocusedRange = 5, guiName = "Decouple", active = true)]
        public void Decouple()
        {
            Events["Decouple"].active = false;
            decoupled = true;
            part.decouple();
            if (!(exp is null))
            {
                exp.OnPecDecoupled();
            }
            exp = null;
        }

        #if (DEBUG)
        [KSPEvent(guiActive = true, guiName = "Debug Dump", active = true)]
        public void DebugDump()
        {
            /* Printed out in Player.log */
            NE_Helper.log(this.ToString ());
        }
        #endif

        /** Converts the object to a human-readble string suitable for printing.
        /** Converts the object to a human-readable string suitable for printing.
         * Overloads base-class implementation.
         */
        new public String ToString()
        {
            String ret = base.ToString () + "\n";
            ret += "\tnode:               " + node + "\n";
            ret += "\texp:                " + exp + "\n";
            ret += "\tdecoupled:          " + decoupled + "\n";
            ret += "\tpart:               " + part + "\n";
            ret += "\tpart.parent:        " + part.parent + "\n";
            ret += "\tvessel:             " + vessel + "\n";
            ret += "\tvessel.isEva:       " + vessel.isEVA + "\n";
            ret += "\tvessel.geeForce:    " + vessel.geeForce + "\n";
            ret += "\tmaxGforce:          " + maxGforce + "\n";
            return ret;
        }
    }
}
