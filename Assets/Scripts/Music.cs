using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class Music : MonoBehaviour
{
    [SerializeField] private AudioSource music;
    [SerializeField] private Toggle musicToggle;


    private void Start()
    {
        if(PlayerPrefs.GetInt("music",1)==0)
        {
            music.Stop();

            if (musicToggle != null)
            {
                musicToggle.SetIsOnWithoutNotify(false);
            }
        }
    }


    public void ToggleMusic ()
    {
        if (PlayerPrefs.GetInt("music", 1) == 0)
        {
            PlayerPrefs.SetInt("music", 1);
            music.Play();
        }
        else
        {
            PlayerPrefs.SetInt("music", 0);
            music.Stop();
        }
    }
}
