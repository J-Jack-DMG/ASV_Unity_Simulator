using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//Equations that calculates boat physics forces
public static class PhysicsManagerUtils
{
    // public const float RHO_WATER = 1000f;
    public const float RHO_WATER = 1027f; // ocean water


    public static Vector3 DirectBuoyancyForce(float rho, float underWaterVolume){
        //Buoyancy is a hydrostatic force - it's there even if the water isn't flowing or if the boat stays still

        // F_buoyancy = rho * g * V
        // rho - density of the mediaum you are in
        // g - gravity
        // V - volume of fluid directly above the curved surface 

        Vector3 buoyancyForce = rho * Physics.gravity * -underWaterVolume;

        //The vertical component of the hydrostatic forces don't cancel out but the horizontal do
        buoyancyForce.x = 0f;
        buoyancyForce.z = 0f;

        //Check that the force is valid, such as not NaN to not break the physics model
        buoyancyForce = CheckForceIsValid(buoyancyForce, "Buoyancy");

        return buoyancyForce;
    }
    //
    // Resistance forces from http://www.gamasutra.com/view/news/263237/Water_interaction_model_for_boats_in_video_games_Part_2.php
    //
    private static Vector3 CheckForceIsValid(Vector3 force, string forceName)
    {
        if (!float.IsNaN(force.x + force.y + force.z))
        {
            return force;
        }
        else
        {
            Debug.Log(forceName += " force is NaN");

            return Vector3.zero;
        }
    }

    
}