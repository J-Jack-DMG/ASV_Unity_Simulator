using UnityEngine;
using System.Collections;

//To be able to change the different physics parameters real time
public class Parameters : MonoBehaviour
{
    public static Parameters current;

    [Header("Left Engine Parameters")]
    public float left_maxPower = 1000;
    public float left_incrementPowerFactor = 10;
    public float left_reductionPowerFactor = 100;
    public float left_currentPower = 0;

    [Header("Right Engine Parameters")]
    public float right_maxPower = 1000;
    public float right_incrementPowerFactor = 10;
    public float right_reductionPowerFactor = 100;
    public float right_currentPower = 0;

    [Header("Max Speed - m/s")]
    public float maxSpeed = 75;

    [Header("Agent Spawn Position")]
    public Vector3 spawnPosition = new Vector3(0f, 0f, 0f);

    // void Start()
    // {
    //     Debug.Log("Parameters instance create");
    //     current = this;
    // }
    void Awake()
    {
        current = this;
    }
}