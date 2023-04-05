using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    private float timer = 3f;
    private float div = 3f;
    private float target = 0;

    private void Start()
    {
        div = timer = Random.Range(0.1f, 1.5f);
        target = Random.Range(-180,180f);
    }

    private void Update()
    {
        float turn = Time.deltaTime * target / div;
        //Debug.Log("turn: " + turn + " target: " + target+" div:"+div+" delta:"+ Time.deltaTime);
        transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y + turn, 0);
        timer -= Time.deltaTime;

        if(timer <0f)
        {
            div = timer = Random.Range(0.1f, 1.5f);
            target = Random.Range(-180, 180f);
        }
    }
}
