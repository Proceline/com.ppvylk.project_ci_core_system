using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.General
{
    public class SpinObj : MonoBehaviour
    {
        public Vector3 RotateDir;
        
        void Update()
        {
            transform.Rotate(RotateDir);
        }
    }
}
