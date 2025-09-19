using System.Collections;
using System.Reflection;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Library
{
    public static class GameUtils
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
        
         public static IEnumerator WaitSecondsNoAlloc(float seconds, bool unscaled = false)
        {
            var t = 0f;
            while (t < seconds)
            {
                t += unscaled ? Time.unscaledDeltaTime : Time.deltaTime;
                yield return null;
            }
        }
    }
}
