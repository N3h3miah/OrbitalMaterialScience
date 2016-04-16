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
    string assemblyName = "KIS";
    int minimalVersionMajor = 1;
    int minimalVersionMinor = 2;
    int minimalVersionBuild = 7;
    bool checkPresence = false;

    static private bool hasKIS = false;

    static public bool HasKIS { get { return hasKIS; } }

    public void Start()
    {
        string minimalVersion = minimalVersionMajor + "." + minimalVersionMinor + "." + minimalVersionBuild;
        Assembly dependancyAssembly = null;
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (assembly.GetName().Name == assemblyName)
            {
                dependancyAssembly = assembly;
                break;
            }
        }
        if (dependancyAssembly != null)
        {
            Debug.Log("Assembly : " + dependancyAssembly.GetName().Name + " | Version : " + dependancyAssembly.GetName().Version + " found !");
            Debug.Log("Minimal version needed is : " + minimalVersion);
            int dependancyAssemblyVersion = (dependancyAssembly.GetName().Version.Major * 100) + (dependancyAssembly.GetName().Version.Minor * 10) + (dependancyAssembly.GetName().Version.Build);
            int minimalAssemblyVersion = (minimalVersionMajor * 100) + (minimalVersionMinor * 10) + (minimalVersionBuild);
            Debug.Log("INT : " + dependancyAssemblyVersion + "/" + minimalAssemblyVersion);
            if (dependancyAssemblyVersion < minimalAssemblyVersion) {
                Debug.LogError (assemblyName + " version " + dependancyAssembly.GetName ().Version + "is not compatible with " + currentModName + "!");
                /*
                    var sb = new StringBuilder ();
                    sb.AppendFormat (assemblyName + " version must be " + minimalVersion + " or greater for this version of " + currentModName + ".");
                    sb.AppendLine ();
                    sb.AppendFormat ("Please update " + assemblyName + " to the latest version.");
                    sb.AppendLine ();
                    PopupDialog.SpawnPopupDialog (currentModName + "/" + assemblyName + " Version mismatch", sb.ToString (), "OK", false, HighLogic.Skin);
                    */
            } else {
                Debug.LogError(assemblyName + " found !");
                hasKIS = true;
            }
        }
        else if (checkPresence)
        {
            Debug.LogError(assemblyName + " not found !");
            /*
                // MKW - the mod is actually optional..
                var sb = new StringBuilder();
                sb.AppendFormat(assemblyName + " is required for " + currentModName + "."); sb.AppendLine();
                sb.AppendFormat("Please install " + assemblyName + " before using " + currentModName + "."); sb.AppendLine();
                PopupDialog.SpawnPopupDialog(assemblyName + " not found !", sb.ToString(), "OK", false, HighLogic.Skin);
*/
            }
        }
    }

} // Namespace NE_Science
