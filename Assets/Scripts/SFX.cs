using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class SFX : MonoBehaviour
{
    [SerializeField] private AudioMixer mixer;
    [SerializeField] private Toggle sfxToggle;

    private void Start()
    {
        if(PlayerPrefs.GetInt("sfx",1)==0)
        {
            mixer.SetFloat("SFX", -80f);
            if(sfxToggle!=null)
            {
                sfxToggle.SetIsOnWithoutNotify(false);
            }
        }

        
    }

    public void ToggleSFX()
    {
        if (PlayerPrefs.GetInt("sfx", 1) == 0)
        {
            PlayerPrefs.SetInt("sfx", 1);
            mixer.SetFloat("SFX", -19f);
        } else
        {
            PlayerPrefs.SetInt("sfx", 0);
            mixer.SetFloat("SFX", -80f);
        }
    }
}
