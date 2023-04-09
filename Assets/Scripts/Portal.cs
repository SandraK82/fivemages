using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    private float originalRot = 0;
    private float timer = 3f;
    private float div = 3f;
    private float target = 0;

    private void Start()
    {
        div = timer = Random.Range(0.3f, 1.0f);
        originalRot = transform.rotation.eulerAngles.y;
        target = Random.Range(originalRot - 45, originalRot+45f);
        if (target < 0) target += 360;
        if (target >= 180) target -= 360;
        
    }

    private void Update()
    {
        float grad = target - transform.rotation.eulerAngles.y;
        if (grad < -180f) grad += 360f;
        if (grad > 180f) grad -= 360f;
        float turn = Time.deltaTime * (grad) / div;
        //Debug.Log("turn: " + turn + " target: " + target+" div:"+div+" delta:"+ Time.deltaTime);
        transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y + turn, 0);
        timer -= Time.deltaTime;

        if(timer <0f)
        {
            div = timer = Random.Range(0.3f, 1.0f);
            target = Random.Range(originalRot - 45, originalRot + 45f);
            if (target < 0) target += 360;
            if (target >= 360) target -= 360;
        }
    }
}
