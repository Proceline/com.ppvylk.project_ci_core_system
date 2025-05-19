using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.TacticRpgTool.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioPlayer : MonoBehaviour
    {
        AudioPlayData AudioData;

        public void Setup(AudioPlayData InPlayerData)
        {
            AudioData = InPlayerData;

            AudioSource audioSource = GetComponent<AudioSource>();
            if (audioSource)
            {
                audioSource.playOnAwake = false;
                audioSource.clip = AudioData.Clip;
                audioSource.loop = AudioData.bLoop;
                audioSource.spatialBlend = AudioData.bUseSpatialBlend ? 1.0f : 0.0f;
            }
        }

        public void Play()
        {
            AudioSource audioSource = GetComponent<AudioSource>();
            if(audioSource)
            {
                audioSource.Play();

                if(AudioData.bDestroyWhenDone)
                {
                    if(AudioData.Clip)
                    {
                        float clipLength = AudioData.Clip.length;
                        Destroy(gameObject, clipLength);
                    }
                }
            }
        }
    }
}
