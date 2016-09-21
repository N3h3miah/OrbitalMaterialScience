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
/* MKW - copied from KAS/DependancyChecker.cs */


using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace NE_Science
{



[KSPAddon(KSPAddon.Startup.MainMenu, true)]
internal class DependancyChecker : MonoBehaviour
{
    string currentModName = "KEES";

   class AssemblyInfo {
        public AssemblyInfo(int pMinMajor, int pMinMinor, int pMinBuild, bool pIsRequired = false) {
            minimalVersionMajor = pMinMajor;
            minimalVersionMinor = pMinMinor;
            minimalVersionBuild = pMinBuild;
            isRequired = pIsRequired;
            isPresent = false;
            assembly = null;
        }

        public int minimalVersionMajor;
        public int minimalVersionMinor;
        public int minimalVersionBuild;
        public bool isRequired;
        public bool isPresent;
        public Assembly assembly;
    };

    static Dictionary<string, AssemblyInfo> assemblies = new Dictionary<string, AssemblyInfo> 
    {
        { "ModuleManager", new AssemblyInfo(2, 6, 25, false) },
        { "KIS", new AssemblyInfo(1, 2, 7) },
    };

    static public bool HasKIS { get { return assemblies["KIS"].isPresent; } }

    static public bool HasModuleManager { get { return assemblies["ModuleManager"].isPresent; } }

    public void Start()
    {        
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if ( assemblies.ContainsKey( assembly.GetName().Name ) )
            {
                assemblies[assembly.GetName().Name].assembly = assembly;
            }
        }

        foreach (KeyValuePair<string, AssemblyInfo> entry in assemblies )
        {
            string assemblyName = entry.Key;
            AssemblyInfo ai = entry.Value;
            string minimalVersion = ai.minimalVersionMajor + "." + ai.minimalVersionMinor + "." + ai.minimalVersionBuild;

            if (entry.Value.assembly != null)
            {
                Debug.Log("Assembly : " + ai.assembly.GetName().Name + " | Version : " + ai.assembly.GetName().Version + " found !");
                Debug.Log("Minimal version needed is : " + minimalVersion);
                int dependancyAssemblyVersion = (ai.assembly.GetName().Version.Major * 100) + (ai.assembly.GetName().Version.Minor * 10) + (ai.assembly.GetName().Version.Build);
                int minimalAssemblyVersion = (ai.minimalVersionMajor * 100) + (ai.minimalVersionMinor * 10) + (ai.minimalVersionBuild);
                Debug.Log("INT : " + dependancyAssemblyVersion + "/" + minimalAssemblyVersion);
                if (dependancyAssemblyVersion >= minimalAssemblyVersion)
                {
                    ai.isPresent = true;
                }
                else
                {
                    Debug.LogError (assemblyName + " version " + ai.assembly.GetName ().Version + "is not compatible with " + currentModName + "!");
                    /* TODO: If the assembly is required, pop up a dialog */
                    /*
                        var sb = new StringBuilder ();
                        sb.AppendFormat (assemblyName + " version must be " + minimalVersion + " or greater for this version of " + currentModName + ".");
                        sb.AppendLine ();
                        sb.AppendFormat ("Please update " + assemblyName + " to the latest version.");
                        sb.AppendLine ();
                        PopupDialog.SpawnPopupDialog (currentModName + "/" + assemblyName + " Version mismatch", sb.ToString (), "OK", false, HighLogic.Skin);
                        */
                }
            }
            else if(entry.Value.isRequired)
            {
                Debug.LogError(assemblyName + " reuired but not found!");
                /* TODO: Pop up dialog warning user
                var sb = new StringBuilder();
                sb.AppendFormat(assemblyName + " is required for " + currentModName + "."); sb.AppendLine();
                sb.AppendFormat("Please install " + assemblyName + " before using " + currentModName + "."); sb.AppendLine();
                PopupDialog.SpawnPopupDialog(assemblyName + " not found !", sb.ToString(), "OK", false, HighLogic.Skin);
                */
            }
            else 
            {
                Debug.LogError(assemblyName + " not found but optional!");
            }
        }
    }
} // Class DependancyChecker

} // Namespace NE_Science
