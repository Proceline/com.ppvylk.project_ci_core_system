using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Gameplay;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.General
{
    public class CameraPanArea : MonoBehaviour
    {
        public Vector2 PanDir;
        
        public void StartPanning()
        {
            CameraController cameraController = TacticBattleManager.GetCameraController();
            if(cameraController)
            {
                cameraController.SetPanDirection(PanDir);
            }
        }

        public void StopPanning()
        {
            CameraController cameraController = TacticBattleManager.GetCameraController();
            if (cameraController)
            {
                cameraController.StopPanning();
            }
        }
    }
}
