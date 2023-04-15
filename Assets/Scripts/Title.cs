using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Title : MonoBehaviour
{
    [SerializeField] private AudioSource source;
    [SerializeField] private AudioClip click;
    public void OnGame()
    {
        source.PlayOneShot(click);
        SceneManager.LoadScene(1);
    }
}
