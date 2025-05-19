using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.General
{
    [System.Serializable]
    public struct CameraInfo
    {
        public float m_MoveSpeed;
        public float m_RotationSpeed;

        public float m_MaxZoom;
        public float m_MinZoom;
        public float m_ZoomInterval;
        public float m_ZoomSpeed;

        public float m_TouchZoomSpeed;

        public float[] m_RotationAngles;
    }

    [RequireComponent(typeof(Rigidbody))]
    public class CameraController : MonoBehaviour
    {
        public Camera m_Camera;
        public CameraInfo m_CameraInfo;

        Rigidbody m_RigidBody;
        Vector2 m_PanDirection;
        float m_CurrHeight;

        int m_RotationIndex = 0;

        float m_PanMultiplier = 100.0f;

        float m_TouchPanMultiplier = 10.0f;

        Vector2 m_LastTouchOnePos = new Vector2();
        Vector2 m_LastTouchTwoPos = new Vector2();

        bool m_bStartedTouchInput = false;

        void SnapToZoom()
        {
            if (m_CurrHeight > m_CameraInfo.m_MaxZoom)
            {
                m_CurrHeight = m_CameraInfo.m_MaxZoom;
            }

            if (m_CurrHeight < m_CameraInfo.m_MinZoom)
            {
                m_CurrHeight = m_CameraInfo.m_MinZoom;
            }
        }

        void Start()
        {
            m_RigidBody = GetComponent<Rigidbody>();
            m_PanDirection = new Vector2(0.0f, 0.0f);
            m_CurrHeight = m_CameraInfo.m_MaxZoom;

            int NumAngles = m_CameraInfo.m_RotationAngles.Length;
            if (m_RotationIndex < NumAngles)
            {
                float rotationAngle = m_CameraInfo.m_RotationAngles[m_RotationIndex];
                transform.rotation = Quaternion.AngleAxis(rotationAngle, Vector3.up);
            }
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Plus) || Input.GetKeyDown(KeyCode.KeypadPlus))
            {
                ZoomIn();
            }

            if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus))
            {
                ZoomOut();
            }

            if (Input.GetKeyDown(KeyCode.Q))
            {
                RotateRight();
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                RotateLeft();
            }

            if (Input.GetKey(KeyCode.W))
            {
                m_PanDirection.x = 1.0f;
            }
            else if (Input.GetKey(KeyCode.S))
            {
                m_PanDirection.x = -1.0f;
            }

            if (Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.S))
            {
                m_PanDirection.x = 0.0f;
            }

            if (Input.GetKey(KeyCode.A))
            {
                m_PanDirection.y = -1.0f;

            }
            else if (Input.GetKey(KeyCode.D))
            {
                m_PanDirection.y = 1.0f;
            }

            if(Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.D))
            {
                m_PanDirection.y = 0.0f;
            }

            if(Input.touchCount == 2)
            {
                Touch touchOne = Input.touches[0];
                Touch touchTwo = Input.touches[1];

                if(m_bStartedTouchInput)
                {
                    //Update stuff
                    float oldDist = Vector2.Distance(m_LastTouchOnePos, m_LastTouchTwoPos);
                    float currDist = Vector2.Distance(touchOne.position, touchTwo.position);

                    float distDiff = currDist - oldDist;
                    if(distDiff < 0)
                    {
                        ZoomOut( Mathf.Abs( distDiff ) * m_CameraInfo.m_TouchZoomSpeed * Time.deltaTime );
                    }
                    else if(distDiff > 0)
                    {
                        ZoomIn( Mathf.Abs( distDiff ) * m_CameraInfo.m_TouchZoomSpeed * Time.deltaTime );
                    }

                    Vector2 oldMovePoint = Vector2.Lerp(m_LastTouchOnePos, m_LastTouchTwoPos, 0.5f);
                    Vector2 currMovePoint = Vector2.Lerp(touchOne.position, touchTwo.position, 0.5f);
                    Vector2 positionDiff = oldMovePoint - currMovePoint;

                    m_RigidBody.AddRelativeForce(new Vector3( positionDiff.x, 0, positionDiff.y ) * m_CameraInfo.m_MoveSpeed * m_TouchPanMultiplier * Time.deltaTime);
                }

                m_LastTouchOnePos = touchOne.position;
                m_LastTouchTwoPos = touchTwo.position;

                m_bStartedTouchInput = true;
            }
            else
            {
                m_bStartedTouchInput = false;
            }

            float mouseWheelAxisValue = Input.GetAxis("Mouse ScrollWheel");

            if (mouseWheelAxisValue > 0)
            {
                ZoomIn();
            }

            if (mouseWheelAxisValue < 0)
            {
                ZoomOut();
            }

            m_RigidBody.AddRelativeForce(new Vector3(m_PanDirection.y, 0, m_PanDirection.x) * m_CameraInfo.m_MoveSpeed * m_PanMultiplier * Time.deltaTime);

            if(m_Camera.orthographic)
            {   
                m_Camera.orthographicSize = Mathf.Lerp(m_Camera.orthographicSize, m_CurrHeight, Time.deltaTime * m_CameraInfo.m_ZoomSpeed);
            }
            else
            {
                m_Camera.fieldOfView = Mathf.Lerp(m_Camera.fieldOfView, m_CurrHeight, Time.deltaTime * m_CameraInfo.m_ZoomSpeed);
            }

            int NumAngles = m_CameraInfo.m_RotationAngles.Length;
            if(m_RotationIndex < NumAngles)
            {
                float rotationAngle = m_CameraInfo.m_RotationAngles[m_RotationIndex];
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.AngleAxis(rotationAngle, Vector3.up), m_CameraInfo.m_RotationSpeed * Time.deltaTime);
            }

            
        }

        public void SetCameraInfo(CameraInfo InInfo)
        {
            m_CameraInfo = InInfo;

            SnapToZoom();
        }

        public void ZoomIn(float amount)
        {
            m_CurrHeight -= amount;
            if (m_CurrHeight < m_CameraInfo.m_MinZoom)
            {
                m_CurrHeight = m_CameraInfo.m_MinZoom;
            }
        }

        public void ZoomOut(float amount)
        {
            m_CurrHeight += amount;
            if (m_CurrHeight > m_CameraInfo.m_MaxZoom)
            {
                m_CurrHeight = m_CameraInfo.m_MaxZoom;
            }
        }

        public void ZoomIn()
        {
            if (m_CurrHeight - m_CameraInfo.m_ZoomInterval > m_CameraInfo.m_MinZoom)
            {
                m_CurrHeight -= m_CameraInfo.m_ZoomInterval;
            }
        }

        public void ZoomOut()
        {
            if (m_CurrHeight + m_CameraInfo.m_ZoomInterval < m_CameraInfo.m_MaxZoom)
            {
                m_CurrHeight += m_CameraInfo.m_ZoomInterval;
            }
        }

        public void RotateLeft()
        {
            int NumAngles = m_CameraInfo.m_RotationAngles.Length - 1;

            if (m_RotationIndex > 0)
            {
                m_RotationIndex--;
            }
            else
            {
                m_RotationIndex = NumAngles;
            }
        }

        public void RotateRight()
        {
            int NumAngles = m_CameraInfo.m_RotationAngles.Length - 1;

            if (m_RotationIndex < NumAngles)
            {
                m_RotationIndex++;
            }
            else
            {
                m_RotationIndex = 0;
            }
        }

        public void SetPanDirection(Vector2 InDirection)
        {
            m_PanDirection = InDirection;
        }

        public void StopPanning()
        {
            m_PanDirection = new Vector2(0.0f, 0.0f);
        }
    }
}
