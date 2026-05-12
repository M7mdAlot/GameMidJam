using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioMixerScript : MonoBehaviour
{
    public AudioMixer MyMixer;
    public Slider MusicSlider;
    public Slider MasterSlider;
    public Slider SFXSlider;


    public void SetMusicVolume()
    {
        float volume = MusicSlider.value;
        MyMixer.SetFloat("music", Mathf.Log(volume) * 20);
    }
    public void SetSFXVolume()
    {
        float volume = SFXSlider.value;
        MyMixer.SetFloat("soundeffect", Mathf.Log(volume)*20);
    }
    public void SetMasterVolume()
    {
        float volume = MasterSlider.value;
        MyMixer.SetFloat("master", Mathf.Log(volume) * 20);
    }
}