using System.Reflection;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.General
{
    public class GameUtils : Object
    {
        public static System.Type FindType(string qualifiedTypeName)
        {
            if(qualifiedTypeName != "" && qualifiedTypeName != null)
            {
                System.Type type = System.Type.GetType(qualifiedTypeName);

                if (type == null)
                {
                    foreach (Assembly asm in System.AppDomain.CurrentDomain.GetAssemblies())
                    {
                        if(asm != null)
                        {
                            type = asm.GetType(qualifiedTypeName);
                        }
                    }
                }

                return type;
            }

            return null;
        }
    }
}
