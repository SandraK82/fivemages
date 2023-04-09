using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FPS : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI fpsCounter;

    private float deltaTimer;
    private float update;

    private void Update()
    {
        deltaTimer += (Time.deltaTime - deltaTimer) * 0.1f;
        update += Time.deltaTime;
        if (update >= 1.0f)
        {
            float fps = 1f / deltaTimer;
            fpsCounter.text = "FPS: " + (Mathf.FloorToInt(fps)).ToString();
            update = 0f;
        }
    }
}
