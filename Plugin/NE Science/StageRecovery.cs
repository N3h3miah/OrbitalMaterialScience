﻿//This StageRecoveryWrapper.cs file is provided as-is and is not to be modified other than to update
//the namespace. Should further modification be made, no support will be provided by the author,
//magico13.
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

//Change this to your mod's namespace!
namespace NE_Science
{
    /////////////////////////////////////
    // DO NOT EDIT BEYOND THIS POINT!  //
    /////////////////////////////////////
    public class StageRecoveryWrapper
    {
        private static bool? available = null;
        private static Type SRType = null;
        private static object instance_;


        /* Call this to see if the addon is available. If this returns false, no additional API calls should be made! */
        public static bool StageRecoveryAvailable
        {
            get
            {
                if (available == null)
                {
                    SRType = AssemblyLoader.loadedAssemblies
                        .Select(a => a.assembly.GetExportedTypes())
                        .SelectMany(t => t)
                        .FirstOrDefault(t => t.FullName == "StageRecovery.APIManager");
                    available = SRType != null;
                }
                return (bool)available;
            }
        }

        #region APIMethods
        /***************/
        /* API methods */
        /***************/

        /* Adds a listener to the Recovery Success Event. When a vessel is recovered by StageRecovery the method will 
         * be invoked with the Vessel; an array of floats representing the percent returned after damage, funds returned,
         * and science returned; and a string representing the reason for failure (SUCCESS, SPEED, or BURNUP)*/
        public static void AddRecoverySuccessEvent(Action<Vessel, float[], string> method)
        {
            object successList = GetMemberInfoValue(SRType.GetMember("RecoverySuccessEvent")[0], Instance);
            System.Reflection.MethodInfo addMethod = successList.GetType().GetMethod("Add");
            addMethod.Invoke(successList, new object[] { method });
        }

        /* Removes a listener from the Recovery Success Event */
        public static void RemoveRecoverySuccessEvent(Action<Vessel, float[], string> method)
        {
            object successList = GetMemberInfoValue(SRType.GetMember("RecoverySuccessEvent")[0], Instance);
            System.Reflection.MethodInfo removeMethod = successList.GetType().GetMethod("Remove");
            removeMethod.Invoke(successList, new object[] { method });
        }

        /* Adds a listener to the Recovery Failure Event. When a vessel fails to be recovered, the method will be invoked 
         * with the Vessel; an array of floats representing the percent returned after damage, funds returned,
         * and science returned; and a string representing the reason for failure (SUCCESS, SPEED, or BURNUP)*/
        public static void AddRecoveryFailureEvent(Action<Vessel, float[], string> method)
        {
            object failList = GetMemberInfoValue(SRType.GetMember("RecoveryFailureEvent")[0], Instance);
            System.Reflection.MethodInfo addMethod = failList.GetType().GetMethod("Add");
            addMethod.Invoke(failList, new object[] { method });
        }

        /* Removes a listener from the Recovery Failure Event */
        public static void RemoveRecoveryFailureEvent(Action<Vessel, float[], string> method)
        {
            object failList = GetMemberInfoValue(SRType.GetMember("RecoveryFailureEvent")[0], Instance);
            System.Reflection.MethodInfo removeMethod = failList.GetType().GetMethod("Remove");
            removeMethod.Invoke(failList, new object[] { method });
        }
        #endregion

        #region InternalFunctions
        /******************************************/
        /* Internal functions. Just ignore these. */
        /******************************************/

        /* The APIManager instance */
        private static object Instance
        {
            get
            {
                if (StageRecoveryAvailable && instance_ == null)
                {
                    instance_ = SRType.GetProperty("instance").GetValue(null, null);
                }

                return instance_;
            }
        }

        /* A helper function I use since I'm bad at reflection. It's for getting the value of a MemberInfo */
        private static object GetMemberInfoValue(System.Reflection.MemberInfo member, object sourceObject)
        {
            object newVal;
            if (member is System.Reflection.FieldInfo)
                newVal = ((System.Reflection.FieldInfo)member).GetValue(sourceObject);
            else
                newVal = ((System.Reflection.PropertyInfo)member).GetValue(sourceObject, null);
            return newVal;
        }

        #endregion
    }
}
