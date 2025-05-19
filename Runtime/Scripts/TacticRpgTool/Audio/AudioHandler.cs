using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Audio
{
    [System.Serializable]
    public struct AudioPlayData
    {
        public AudioClip Clip;
        public bool bLoop;
        public bool bUseSpatialBlend;
        public bool bDestroyWhenDone;

        public AudioPlayData(AudioClip InClip)
        {
            Clip = InClip;
            bLoop = false;
            bUseSpatialBlend = false;
            bDestroyWhenDone = true;
        }
    }

    public class AudioHandler : MonoBehaviour
    {
        static bool m_bEnabled = true;

        public static void SetEnabledState(bool InbEnabled)
        {
            m_bEnabled = InbEnabled;
        }

        public static AudioPlayer PlayAudio(ProjectCI.CoreSystem.Runtime.TacticRpgTool.Audio.AudioPlayData InAudioData, Vector3 InLocation = default(Vector3))
        {
            if(m_bEnabled)
            {
                AudioPlayer audioPlayer = CreateAudioPlayer(InLocation);
                audioPlayer.Setup(InAudioData);
                audioPlayer.Play();

                return audioPlayer;
            }

            return null;
        }

        static AudioPlayer CreateAudioPlayer(Vector3 InLocation = default(Vector3))
        {
            if (m_bEnabled)
            {
                GameObject NewObj = new GameObject("Audio_" + Random.Range(0, 10000).ToString());
                NewObj.transform.position = InLocation;

                return NewObj.AddComponent<AudioPlayer>();
            }

            return null;
        }
    }
}
