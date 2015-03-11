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
    /*
  * Experiments for the Kemini Research Program
  */
    public class KeminiExperimentData : StepExperimentData
    {

        private Guid cachedVesselID;
        private int partCount;
        private List<Kemini_Module> KeminiLabCache = null;
        
        protected KeminiExperimentData(string id, string type, string name, string abb, float mass)
            : base(id, type, name, abb, EquipmentRacks.KEMINI, mass)
        {
            storageType = ExperimentFactory.KEMINI_EXPERIMENTS;
        }

        public override List<Lab> getFreeLabsWithEquipment(Vessel vessel)
        {
            List<Lab> ret = new List<Lab>();
            List<Kemini_Module> allKeminiLabs;
            if (cachedVesselID == vessel.id && partCount == vessel.parts.Count && KeminiLabCache != null)
            {
                allKeminiLabs = KeminiLabCache;
            }
            else
            {
                allKeminiLabs = new List<Kemini_Module>(UnityFindObjectsOfType(typeof(Kemini_Module)) as Kemini_Module[]);
                KeminiLabCache = allKeminiLabs;
                cachedVesselID = vessel.id;
                partCount = vessel.parts.Count;
                NE_Helper.log("Lab Cache refresh");
            }
            foreach (Kemini_Module lab in allKeminiLabs)
            {
                if (lab.vessel == vessel && lab.hasEquipmentInstalled(neededEquipment) && lab.hasEquipmentFreeExperimentSlot(neededEquipment))
                {
                    ret.Add(lab);
                }
            }
            return ret;
        }

        public override bool canInstall(Vessel vessel)
        {
            List<Lab> labs = getFreeLabsWithEquipment(vessel);
            return labs.Count > 0 && state == ExperimentState.STORED;
        }

        public override bool canMove(Vessel vessel)
        {
            return state == ExperimentState.INSTALLED;
        }

        public override void runLabAction()
        {
            base.runLabAction();
            if (state == ExperimentState.FINISHED)
            {
                ExperimentStorage[] storages = store.getPartGo().GetComponents<ExperimentStorage>();
                foreach (ExperimentStorage es in storages)
                {
                    if (es.isEmpty())
                    {
                        moveTo(es);
                    }
                }
            }
        }
    }

    public class KeminiD5_ExperimentData : KeminiExperimentData
    {
        public KeminiD5_ExperimentData(float mass)
            : base("NE_Kemini_D5", "KeminiD5", "Kemini D5: Star Occultation Navigation", "D5", mass)
        {
            step = new ResourceExperimentStep(this, Resources.LAB_TIME, 0.1f, "", 0);
        }
    }

    public class KeminiD8_ExperimentData : KeminiExperimentData
    {
        public KeminiD8_ExperimentData(float mass)
            : base("NE_Kemini_D8", "KeminiD8", "Kemini D8: Spacecraft Radiation Level", "D8", mass)
        {
            step = new ResourceExperimentStep(this, Resources.LAB_TIME, 0.15f, "", 0);
        }
    }

    public class KeminiMSC3_ExperimentData : KeminiExperimentData
    {
        public KeminiMSC3_ExperimentData(float mass)
            : base("NE_Kemini_MSC3", "KeminiMSC3", "Kemini MSC3: Tri-Axis Magnetometer", "MSC3", mass)
        {
            step = new ResourceExperimentStep(this, Resources.LAB_TIME, 0.13f, "", 0);
        }
    }

    public class KeminiD7_ExperimentData : KeminiExperimentData
    {
        public KeminiD7_ExperimentData(float mass)
            : base("NE_Kemini_D7", "KeminiD7", "Kemini D7: Space Object Radiometry", "D7", mass)
        {
            step = new ResourceExperimentStep(this, Resources.LAB_TIME, 0.23f, "", 0);
        }
    }
    
    public class KeminiD10_ExperimentData : KeminiExperimentData
    {
        public KeminiD10_ExperimentData(float mass)
            : base("NE_Kemini_D10", "KeminiD10", "Kemini D10: Ion-sensing Attitude Control", "D10", mass)
        {
            step = new ResourceExperimentStep(this, Resources.LAB_TIME, 0.21f, "", 0);
        }
    }

}
