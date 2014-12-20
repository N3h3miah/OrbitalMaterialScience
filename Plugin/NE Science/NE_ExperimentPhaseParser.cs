using System;
using System.Collections.Generic;
using System.Reflection;


namespace NE_Science
{
    class NE_ExperimentPhaseParser
    {

        public static ExperimentPhase getPhasesFromConfig(string config, PhaseExperimentCore exp)
        {
            config = config.Trim();
            string[] parts = config.Split(':');
            ExperimentPhase ret = new ExperimentPhase(exp);

            if (parts.Length == 2)
            {
                string name = parts[0].Trim();
                string paramString = parts[1].Trim();
                Parameter para = getParameter(paramString.Split(' '));
                Type type = Type.GetType(name);

                switch (para.parameterType)
                {
                    case ParaType.int_T:
                        int i = Convert.ToInt32(para.valueString);
                        Type[] paramTypes = new Type[] { typeof(PhaseExperimentCore), typeof(int) };
                        Object[] ctorParams = new Object[] { exp, i};
                        ConstructorInfo ctorInfo = type.GetConstructor(paramTypes);
                        ret = (ExperimentPhase)ctorInfo.Invoke(ctorParams);
                        break;

                    case ParaType.doulbe_T:
                        double d = Convert.ToDouble(para.valueString);
                        paramTypes = new Type[] { typeof(PhaseExperimentCore), typeof(double) };
                        ctorParams = new Object[] { exp, d};
                        ctorInfo = type.GetConstructor(paramTypes);
                        ret = (ExperimentPhase)ctorInfo.Invoke(ctorParams);
                        break;

                    case ParaType.string_T:
                        paramTypes = new Type[] { typeof(PhaseExperimentCore), typeof(string) };
                        ctorParams = new Object[] { exp, para.valueString};
                        ctorInfo = type.GetConstructor(paramTypes);
                        ret = (ExperimentPhase)ctorInfo.Invoke(ctorParams);
                        break;
                }
            }
            else
            {
                NE_Helper.logError("Experimentphase config invalid");
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
                            NE_Helper.log("Parametertype: int");
                            ret.parameterType = ParaType.int_T;
                            //test conversion
                            int i = Convert.ToInt32(ret.valueString);
                            break;
                        case "string":
                            NE_Helper.log("Parametertype: string");
                            ret.parameterType = ParaType.string_T;
                            //test conversion
                            double d= Convert.ToDouble(ret.valueString);
                            break;
                        case "double":
                            NE_Helper.log("Parametertype: double");
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
