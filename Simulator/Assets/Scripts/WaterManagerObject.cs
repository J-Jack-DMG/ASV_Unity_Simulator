using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


//Controlls the water
public class WaterManagerObject : MonoBehaviour 
{
    // public static WaterManagerObject current;
    public static WaterManagerObject current = null;
    

    [Header("Wave Configuration :\n\t0 - flat water\n\t1 - Calm\n\t2 - slightly turbulent water\n\t3 - turbulent water")]
    public int configuration = 0;
    // private const int minConfig = 0;
    // private const int maxConfig = 2;
    public bool stopWater = false;

    [Header("Scale of the sinusoidal wave component\n[Do not modify]\n\t[0.1, 0.9]")]
    public float scale = 0.1f;

    [Header("Speed of the sinusoidal wave component\n[Do not modify]\n\t[0.1, 0.9]")]
    public float speed = 0.1f;

    [Header("Scale of the PerlinNoise component\n[Do not modify]\n\t[0.1, 0.9]")]
    public float scaleNoise = 0.1f;

    [Header("Wave Direction\n[Do not modify]\n\t[0.0, 1.0]")]
    public float directionAngle = 0.125f;

    [Header("Distance between waves\n[Do not modify]")]
    public float waveDistance = 1f;
    [Header("Additional values to the Perlin Noise inputs")]
    public float noiseWalk = 1f;

    [Header("Reset water parameter[Do not modify]")]
    public bool resetWater = false;

    private const int minConfig = 0;
    private const int maxConfig = 3;
    private int _configuration = 0;

    private static System.Random rnd = new System.Random();
    private List<int> list_angles = new List<int>(){45, 90, 135, 180, 225, 270, 315, 360};


    private WaveTypes currentWave = new WaveTypes();

    void Awake()
    {
        current = this;
    
        if( current == null ) {
            current = this;
        } 
    }

    void Start()
    {
        Reset_Water(configuration);
    }

    //Get the y coordinate from whatever wavetype we are using
    public float GetWaveYPos(Vector3 position)
    {
        if (stopWater)
        {
            // this.stopWater = false;
            return 0f;
        }
    
        float waveHight = currentWave.SinXWave(position, this.speed, this.scale, this.waveDistance, this.directionAngle, this.scaleNoise, this.noiseWalk);
        return waveHight;
       
    }

    public float DistanceToWater(Vector3 position)
    {
        float waterHeight = GetWaveYPos(position);

        float distanceToWater = position.y - waterHeight;

        return distanceToWater;
    }

    public void Reset_Water(int config)
    {
        config = Mathf.Clamp(config, minConfig, maxConfig);
        this._configuration = config;
        this.configuration = config;

        // this.stopWater = true;
        if (config == 0)
        {
            this.scale = 0f;
            this.speed = 0f;
            this.scaleNoise = 0f;
        }
        else if (config == 1)
        {
            this.scale = UnityEngine.Random.Range(0.1f, 0.3f);
            this.speed = UnityEngine.Random.Range(0.1f, 0.3f);
            this.scaleNoise = UnityEngine.Random.Range(0.1f, 0.3f);
        }
        else if(config == 2)
        {
            this.scale = UnityEngine.Random.Range(0.4f, 0.6f);
            this.speed = UnityEngine.Random.Range(0.4f, 0.6f);
            this.scaleNoise = UnityEngine.Random.Range(0.4f, 0.6f);
        }
        else if(config == 3)
        {
            this.scale = UnityEngine.Random.Range(0.7f, 0.9f);
            this.speed = UnityEngine.Random.Range(0.7f, 0.9f);
            this.scaleNoise = UnityEngine.Random.Range(0.7f, 0.9f);
        }
        else
        {
            this.scale = 0f;
            this.speed = 0f;
            this.scaleNoise = 0f;
        }


      
        
        int r = rnd.Next(list_angles.Count);
        this.directionAngle = list_angles[r];
        
    }


    public void Reset_Water()
    {
        Reset_Water(this.configuration);
    }
 
    public void Stop_Water()
    {
        this.stopWater = true;
    }
    public void Start_Water()
    {
        this.stopWater = false;
    }

    void FixedUpdate(){

        if (this._configuration != this.configuration){
            Reset_Water(this.configuration);
        }

    }

    public class WaveTypes
    {
        // public static float SinXWave(
        public float SinXWave(
            Vector3 position,
            float speed, 
            float scale,
            float waveDistance,
            float waveAngle,
            float scaleNoise,
            float noiseWalk)
        {

            float x = position.x;
            float y = position.y;
            float z = position.z;

            // Wave Direction
            // Make sure that the input value is in the range [0,1]
            waveAngle = Mathf.Clamp(waveAngle, 0f, 1f);
            // Conversion to Degree angle [0,1] -> [0°, 360°] -> [rad]
            waveAngle = waveAngle * Mathf.PI / 180;
            float waveType = Mathf.Sin(waveAngle) * z + Mathf.Cos(waveAngle) * x + 0f * y; 
            // Make sure the speed value is in between 0 and 1
            speed = Mathf.Clamp(speed, 0f, 1f);
            scale = Mathf.Clamp(scale, 0f, 1f);
            scaleNoise = Mathf.Clamp(scaleNoise, 0f, 1f);


            // The function Time.time can causes problem whne the code is accelerated
            // Sinusoidal component of the waves
            float waveBase = Mathf.Sin((Time.time * speed + waveType) / waveDistance) * scale;
            // Final wave form results
            float waveHight = Mathf.PerlinNoise(x * waveBase + Time.time * speed + noiseWalk, z * waveBase + Time.time * speed + noiseWalk) * scaleNoise;
            
            return waveHight;
        }
    }


}