using System;
using System.Collections.Generic;
using System.Reflection;


namespace NE_Science
{
    class NE_ExperimentPhaseParser
    {

        public static List<ExperimentPhase> getPhasesFromConfig(string config, PhaseExperimentCore exp)
        {
            config = config.Trim();
            string[] phaseConfigs = config.Split(';');
            List<ExperimentPhase> ret = new List<ExperimentPhase>(phaseConfigs.Length);
            foreach (string pc in phaseConfigs)
            {
                string phaseConfig = pc.Trim();
                string[] parts = phaseConfig.Split(':');

                string phaseName = "";
                ExperimentPhase phase = new ExperimentPhase(exp, phaseName);

                if (parts.Length == 2)
                {
                    string className = parts[0].Trim();

                    string[] nameParam = parts[1].Trim().Split(',');
                    string paramString = "";
                    if(nameParam.Length == 1){
                        paramString = nameParam[0].Trim();
                    }else{
                        phaseName = nameParam[0].Trim();
                        paramString = nameParam[1].Trim();
                    }

                    Parameter para = getParameter(paramString.Split(' '));
                    Type type = Type.GetType(className);

                    switch (para.parameterType)
                    {
                        case ParaType.int_T:
                            int i = Convert.ToInt32(para.valueString);
                            Type[] paramTypes = new Type[] { typeof(PhaseExperimentCore),typeof(string), typeof(int) };
                            Object[] ctorParams = new Object[] { exp, phaseName, i };
                            ConstructorInfo ctorInfo = type.GetConstructor(paramTypes);
                            phase = (ExperimentPhase)ctorInfo.Invoke(ctorParams);
                            break;

                        case ParaType.doulbe_T:
                            double d = Convert.ToDouble(para.valueString);
                            paramTypes = new Type[] { typeof(PhaseExperimentCore), typeof(string), typeof(double) };
                            ctorParams = new Object[] { exp, phaseName, d };
                            ctorInfo = type.GetConstructor(paramTypes);
                            phase = (ExperimentPhase)ctorInfo.Invoke(ctorParams);
                            break;

                        case ParaType.string_T:
                            paramTypes = new Type[] { typeof(PhaseExperimentCore), typeof(string), typeof(string) };
                            ctorParams = new Object[] { exp, phaseName, para.valueString };
                            ctorInfo = type.GetConstructor(paramTypes);
                            phase = (ExperimentPhase)ctorInfo.Invoke(ctorParams);
                            break;
                    }
                }
                else
                {
                    NE_Helper.logError("Experimentphase config invalid");
                }
                ret.Add(phase);
            }

            return ret;
        }

        private static Parameter getParameter(string[] paramParts)
        {
            if (paramParts.Length == 2)
            {
                Parameter ret = new Parameter();
                ret.valueString = paramParts[1].Trim();
                try
                {
                    switch (paramParts[0].Trim())
                    {
                        case "int":
                            ret.parameterType = ParaType.int_T;
                            //test conversion
                            int i = Convert.ToInt32(ret.valueString);
                            break;
                        case "string":
                            ret.parameterType = ParaType.string_T;
                            //test conversion
                            double d= Convert.ToDouble(ret.valueString);
                            break;
                        case "double":
                            ret.parameterType = ParaType.doulbe_T;
                            break;

                        default:
                            throw new Exception("Experimentphase config invalid; Parameter type invalid");
                    }

                    
                }
                catch (FormatException e)
                {
                    NE_Helper.logError("Param string is not a sequence of digits. Set Value to: 10");
                    ret.valueString = "10";
                }
                catch (OverflowException e)
                {
                    NE_Helper.logError("The number cannot fit in an Int32. Set Value to: 10");
                    ret.valueString = "10";
                }

                return ret;
            }
            else
            {
                throw new Exception("Experimentphase config invalid; Parameter config invalid");
            }
        }


        class Parameter
        {
           
            public ParaType parameterType;
            public string valueString;
        }

        enum ParaType
        {
            int_T, string_T, doulbe_T
        }
    }
}
