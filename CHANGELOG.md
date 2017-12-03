Changelog
=========

Major features are **bolded**, contributors are *emphasized*.

0.7.1 (HOTFIX) - 2017.12.03
-----------------------------------------------------------

### KRP
* Replace missing texture making the Kemini experiments show up in their full glory again.

### KEES
* Fix contract experiment recovery again (wasn't working for user-created parts).


0.7 - 2017.12.02
-----------------------------------------------------------

### General
* Add README and LICENSE files to release packages
* Add MiniAVC and ModuleManager DLLs to release packages

### KRP
* Revert Mk1 Pod change; it now has room for 2 experiments again

### KEES
* New models and textures for the PC and PEC


0.7b3 (pre-release) - 2017.11.26
-----------------------------------------------------------

### General
* **Add KSPedia entries**
* Recompile for KSP 1.3.1
* Remove spaces from some part configurations
* Remove superfluous NE_XXX_LabEquipmentSlot node from OMS/KLS; savefiles should get updated automatically.
* Various minor tweaks and fixes
* Mod Release layout change - merged KLS/OMS into OSS and only build release packages for the 3 major sub-components as well as the "all-in-one".

### KRP
* **Can now create new Kemini experiments by editing configuration  files**

### KEES
* **Can now create new KEES experiments by editing configuration files**
* Prevent exceptions if KEES is not installed (affects partial-installs of NEOS)
* Nerf the Mk1 pod to only have a single Kemini experiment slot

### OSS
* Remove "ModuleCommand" from science labs


0.7b2 
-----------------------------------------------------------

### General
* Localisation - spanish added (thanks to *fitiales*)
* Various tweaks and fixes to the localisation

### Kemini
* Added support for the Brumby pod (thanks *Sudragon*)
* Added support for the Bluedog Gemini pod (thanks *CobaltWolf*)


0.7b1
-----------------------------------------------------------

**New official release**

Maintainer is now Micha until Nehemiah can pick this up again.

### KEES
* Localisation fixes

### OMS/KLS
* Tweak ESC3 - better flight characteristics and adjust bottom node so heat-shield doesn't clip through model.


0.7a0-mw9 (Unofficial release)
-----------------------------------------------------------

### General
* **Added localisation support**
  * For now only English and German languages
* More GUI rework

### KEES
* Finally fixed experiment recovery bug


0.7a0-mw8 (Unofficial release)
-----------------------------------------------------------

* Recompiled for KSP1.3.0.1804

### GENERAL
* Bump and sanitise version numbers (build number is now unified across all components)
* Fix version files to work with (mini)AVC
* Performance improvements
* Adjust some part categories
* Increase crash tolerance of containers slightly
* Made a start on localisation support

### KEES/KEMINI
* Fix contract generation
* Restrict contracts from spawning until a kerballed landing from orbit has been achieved
* Enable experimental parts for contracts
* Disable KEES debug menu in release builds


0.7a0-mw7 (Unofficial release)
-----------------------------------------------------------

### GENERAL:
* Recompile for KSP1.2 (build 1564)
* Fix for Extraplanetary Launchpads
* Various fixes and tweaks required for KSP1.2
* Fix to only allow purchased parts when part-purchasing is enabled
* Fix adjusting weight & cost of container parts when equipment and experiments are added to the containers
* Add some extra tags to parts (KLS, OMS, NEOS - use to filter parts in VAB)
* Make save-file-parsing more robust

### KEMINI:
* contracts can now mark required parts/experiments as "experimental"

### OMS:
* Add ModuleDataTransmitter to ESC3 automated pod


0.7a0-mw6 (Unofficial release)
-----------------------------------------------------------
* Merge in *Kerbas_ad_astra*'s fixes for empty experiment names
* Recompile for KSP 1.1.1


0.7a0-mw5 (Unofficial release)
-----------------------------------------------------------
* Recompile and fix up for KSP 1.1-pre


0.7a0-mw4 (Unofficial release)
-----------------------------------------------------------
* Recompile for KSP 1.04
* Minor tweak when recovering KEES experiments which may or may not be required to successfully trigger completion of KEES contracts.


0.7a0-mw3 (Unofficial release)
-----------------------------------------------------------
* Fix KEES contract completion when recovering experiments
* Remove KIS "carry" flag from KEES items; instead bump up their size so only 1 can be in a Kerbal inventory at a time, and only 2 can be stored in the KEES PEC.
* Fix UniversalStorage wedge to reference US's KIS wedge instead of the no-longer existing KAS wedge


0.7a0-mw2 (Unofficial release)
-----------------------------------------------------------
* Convert textures to DDS
* Move KEES Payload Carrier to "Basic Science" node


0.7a0-mw1 (Unofficial release)
-----------------------------------------------------------
* Recompiled using KSP v1.0.2 and KIS 1.1 (minor API fixes to replace deprecated functions)
* Converted KEES experiments from KAS to KIS
* Adjusted heat of parts to match stock values
* Fixed a couple of attachment nodes


