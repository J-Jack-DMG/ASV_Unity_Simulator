using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

//Creates an endless water system with squares
public class WaterPlaneGenerator : MonoBehaviour 
{
    [Header("The object to use to create the water")]
    public GameObject waterSqrObj;

    [Header("Lenght of the Water square and resolution")]
    public float squareWidth = 30f;
    public float innerSquareResolution = 0.25f;
    
    // For posible extantion of the environment
    List<WaterSquare> waterSquares = new List<WaterSquare>();

    //The position of the boat
    Vector3 boatPos;
    //The position of the water
    Vector3 waterPos;

    void Start() 
	{
        //create the water plane
        GenerateWaterPlane();
    }

    void FixedUpdate()
    {
        UpdateWater();
    }

    void UpdateWater()
    {
        // update postion to move the water
        transform.localPosition = waterPos;

        // update the vertices height with the wave function.
        for (int i = 0; i < waterSquares.Count; i++)
        {
            waterSquares[i].MoveWater(waterPos);
        }
    }
	
    // Crete a water plane in (0,0,0) position
    void GenerateWaterPlane()
    {
        //The sinle plane
        AddWaterPlane(0f, 0f, 0f, squareWidth, innerSquareResolution);
    }


    // add one plane
    void AddWaterPlane(float x, float z, float y, float squareWidth, float spacing)
    {
        //Create a new object for the water plane
        GameObject waterPlane = Instantiate(waterSqrObj, transform.position, transform.rotation) as GameObject;
        waterPlane.SetActive(true);

        // Set position
        Vector3 centerPos = transform.position;
        centerPos.x += x;   // shift on the x-axis
        centerPos.y = y;    // set height
        centerPos.z += z;   // shift on the z-axis
        waterPlane.transform.position = centerPos;

        //Parent it
        waterPlane.transform.parent = transform;

        // Add the functionality to move the water
        WaterSquare newWaterSquare = new WaterSquare(waterPlane, squareWidth, spacing);
        waterSquares.Add(newWaterSquare);
    }
}