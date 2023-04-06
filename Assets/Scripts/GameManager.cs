using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    

    [SerializeField] private Transform goblinPrefab;
    [SerializeField] private int level = 1;

    [SerializeField] private Transform pentagram;
    [SerializeField] private List<Transform> portals;
    [SerializeField] private Transform lightBlue;
    [SerializeField] private Transform lightGreen;
    [SerializeField] private Transform lightWhite;
    [SerializeField] private Transform lightRed;
    [SerializeField] private Transform lightYellow;

    public enum GameState
    {
        AWAKE,
        AWAKE_LIGHTS,
        AWAKE_MAGE_1,
        AWAKE_MAGE_2,
        AWAKE_MAGE_3,
        AWAKE_MAGE_4,
        AWAKE_MAGE_5,
        AWAKE_PORTAL,
        PLAY
    };

    public GameState state { private set; get; }

    private float stateTime = 0;

    [SerializeField] private float minRadius = 5f;
    [SerializeField] private float maxRadius = 11f;

    private void Awake()
    {
        Goblin.OnGoblinEscaped += OnGoblinEscaped;
        foreach (Transform portal in portals)
        {
            portal.gameObject.SetActive(false);
        }
    }

    private void OnGoblinEscaped(object sender, EventArgs e)
    {
        //FIXME: make me better
        spawned--;
    }

    private void Start()
    {
        lightBlue.gameObject.SetActive(false);
        lightRed.gameObject.SetActive(false);
        lightYellow.gameObject.SetActive(false);
        lightWhite.gameObject.SetActive(false);
        lightGreen.gameObject.SetActive(false);

        state = GameState.AWAKE;
        stateTime = 1f;
    }

    private bool done = false;
    private int spawned = 0;

    private void Update()
    {
        if (state < GameState.PLAY)
        {
            stateTime -= Time.deltaTime;
            if (stateTime < 0)
            {
                state++;
                stateTime = 1f;
                done = false;
            }
        }

        switch (state)
        {
            case GameState.AWAKE:
                if (!done)
                {
                    Instantiate(pentagram, new Vector3(0f,-0.8f,0f), Quaternion.Euler(0,0,-90));
                    done = true;
                }
                break;

            case GameState.AWAKE_LIGHTS:
                if (!done)
                {
                    lightBlue.gameObject.SetActive(true);
                    lightRed.gameObject.SetActive(true);
                    lightYellow.gameObject.SetActive(true);
                    lightWhite.gameObject.SetActive(true);
                    lightGreen.gameObject.SetActive(true);
                    done = true;
                }
                break;

            case GameState.AWAKE_PORTAL:
                if (!done)
                {
                    foreach (Transform portal in portals)
                    {
                        portal.gameObject.SetActive(true);
                        
                    }
                    done = true;
                }
                break;

            case GameState.PLAY:
                //FIXME: make me better
                if (UnityEngine.Random.Range(0, 100) < 2 && spawned < level * 7)
                {
                    float grad = UnityEngine.Random.Range(0, 360f);
                    float rad = grad * Mathf.Deg2Rad;
                    float radius = UnityEngine.Random.Range(minRadius, maxRadius);
                    float x = radius * Mathf.Cos(rad);
                    float y = radius * Mathf.Sin(rad);

                    Vector3 pos = new Vector3(x, 4f, y);
                    Quaternion rot = Quaternion.Euler(0, UnityEngine.Random.Range(0, 360f), 0);

                    Transform g = Instantiate(goblinPrefab);
                    g.transform.position = pos;
                    g.transform.rotation = rot;

                    Goblin goblin = g.GetComponentInChildren<Goblin>();
                    goblin.level = level;
                    goblin.magic = (Mathf.FloorToInt(grad / 72f) + 3) % 5;

                    spawned++;
                }
                break;
        }
    }
}
