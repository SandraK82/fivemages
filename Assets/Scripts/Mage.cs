using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mage : MonoBehaviour
{
    [SerializeField] private Material dissolver;
    [SerializeField] private Transform mage;

    public int magic = 0;

    [SerializeField] private Material redHat;
    [SerializeField] private Material blueHat;
    [SerializeField] private Material whiteHat;
    [SerializeField] private Material yellowHat;
    [SerializeField] private Material greenHat;

    [SerializeField] private MeshRenderer hat;

    private Dictionary<MeshRenderer, Material> originalMaterials;

    public enum MageState
    {
        AWAKE,
        IDLE,
        MOVE
    }

    private MageState state;

    private Quaternion hatRotation;
    private Quaternion hatRotationSelected;

    private float targetAngle;

    private void Start()
    {
        hatRotation = hat.gameObject.transform.localRotation;
        hatRotationSelected = Quaternion.Euler(hatRotation.eulerAngles.x - 90f, hatRotation.eulerAngles.y, hatRotation.eulerAngles.z);
        targetRotation = hatRotation;

        SetHatColor();
        state = MageState.AWAKE;
        originalMaterials = new Dictionary<MeshRenderer, Material>();
        PrepareBeam(this.transform);
    }

    private void PrepareBeam(Transform t)
    {
        if (t.TryGetComponent<MeshRenderer>(out MeshRenderer mr))
        {
            originalMaterials.Add(mr, mr.material);
            mr.material = dissolver;
            mr.material.SetFloat("_dissolved", 1.0f);
        }
        for (int i = 0; i < t.childCount; i++)
        {
            PrepareBeam(t.GetChild(i));
        }
    }

    private void SetHatColor()
    {
        switch (magic)
        {
            case 0:
                hat.material = redHat;
                break;
            case 1:
                hat.material = yellowHat;
                break;
            case 2:
                hat.material = blueHat;
                break;
            case 3:
                hat.material = whiteHat;
                break;
            case 4:
                hat.material = greenHat;
                break;
        }
    }

    private float scaleTarget = 1f;
    private float slerp = 0;

    private void Update()
    {
        switch (state)
        {
            case MageState.AWAKE:
                foreach (MeshRenderer mr in originalMaterials.Keys)
                {
                    float newValue = Mathf.Max(mr.material.GetFloat("_dissolved") - (Time.deltaTime / 1f), 0.0f);
                    mr.material.SetFloat("_dissolved", newValue);
                    if (newValue <= 0f)
                    {
                        state = MageState.IDLE;
                    }
                }
                if (state == MageState.IDLE)
                {
                    foreach (MeshRenderer mr in originalMaterials.Keys)
                    {
                        mr.material = originalMaterials[mr];
                    }
                }
                break;
            case MageState.IDLE:
                if (Mathf.Abs(scaleTarget - 1f) < 0.01f && Mathf.Abs(this.transform.localScale.y - 1.0f) < 0.01f)
                {
                    scaleTarget = Random.Range(1.5f, 2.25f);
                }
                else if (Mathf.Abs(this.transform.localScale.y - scaleTarget) < 0.01f)
                {
                    scaleTarget = 1.0f;
                }

                this.transform.localScale = new Vector3(1f, this.transform.localScale.y + ((scaleTarget - this.transform.localScale.y) * Time.deltaTime * 1.5f), 1.0f);
                break;
            case MageState.MOVE:
                if (scaleTarget != 1f)
                {
                    scaleTarget = 1.0f;
                }
                if (Mathf.Abs(this.transform.localScale.y - 1.0f) > 0.01f)
                {
                    Debug.Log("prep to move");
                    this.transform.localScale = new Vector3(1f, this.transform.localScale.y + ((scaleTarget - this.transform.localScale.y) * Time.deltaTime * 10f), 1.0f);
                }
                else
                {
                    float myAngle = Vector3.SignedAngle(Vector3.right, transform.position, Vector3.up);
                    myAngle = (myAngle + 360f) % 360f;

                    float need = targetAngle - myAngle;
                    if (need < 0)
                    {
                        need += 360f;
                    }

                    if(need > 180)
                    {
                        need -= 360f;
                    }

                    Vector3 npos = Vector3.zero;
                    if (need < -0.1)
                    {
                        npos = (transform.right * -1);
                    }
                    else if(need > 0.1)
                    {
                        npos = transform.right;
                    }
                    else
                    {
                        state = MageState.IDLE;
                    }
                    npos = transform.position + (npos * Time.deltaTime * 8f);
                    transform.position = npos.normalized * transform.position.magnitude;
                    transform.forward = (transform.position).normalized;

                }
                break;
        }

        if (hat.gameObject.transform.rotation != targetRotation)
        {
            slerp += Time.deltaTime * 2f;
            hat.gameObject.transform.localRotation = Quaternion.Slerp(hat.gameObject.transform.localRotation, targetRotation, slerp);
        }
    }

    public MageState GetState() => state;

    private Quaternion targetRotation;

    public void Deselect()
    {
        targetRotation = hatRotation;
        slerp = 0;
    }

    public void Select()
    {
        targetRotation = hatRotationSelected;
        slerp = 0;
    }

    public void Move(float angle)
    {
        Debug.Log("Mage moves to: " + angle);
        targetAngle = angle;
        state = MageState.MOVE;
    }
}
