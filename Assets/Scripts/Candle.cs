using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* based on https://gist.github.com/sinbad/4a9ded6b00cf6063c36a4837b15df969 */
public class Candle : MonoBehaviour
{
    private Light light;

    [SerializeField][Range(50f, 500f)] private float minIntensity = 50f;
    [SerializeField][Range(50f, 1000f)] private float maxIntensity = 1000f;

    private float nextIntensity;

    [SerializeField][Range(1f, 50f)] private int smoothing = 50;

    private Queue<float> smoothQueue;
    private float lastSum = 0;

    private void Awake()
    {
        light = GetComponent<Light>();
        smoothQueue = new Queue<float>(smoothing);
    }

    private void Start()
    {
        nextIntensity = Random.Range(minIntensity, maxIntensity);
    }
    private void Update()
    {
        while (smoothQueue.Count >= smoothing)
        {
            lastSum -= smoothQueue.Dequeue();
        }

        // Generate random new item, calculate new average
        float newVal = Random.Range(minIntensity, maxIntensity);
        smoothQueue.Enqueue(newVal);
        lastSum += newVal;

        // Calculate new smoothed average
        light.intensity = lastSum / (float)smoothQueue.Count;
    }
}
