using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using MeshManager;
//Main controller for all boat physics
public class PhysicsManagerObject : MonoBehaviour
{
    //Drags
    public GameObject boatMeshObj;
    public GameObject underWaterObj;
    //Change the center of mass
    private Vector3 centerOfMass;

    public MeshManager.MeshManagerUtils meshManagerUtils;

    //Meshes for debugging
    private Mesh underWaterMesh;

    //The boats rigidbody
    private Rigidbody boatRB;

    //The density of the water the boat is traveling in
    private float rhoWater = PhysicsManagerUtils.RHO_WATER;
    public Vector3 CoB;
    private BuoyancyData b_struct;
    private Vector3 auxUnderWaterCenter;

    void Awake()
    {
        CoB = new Vector3(0f, 0f, 0f);
        boatRB = this.GetComponent<Rigidbody>();
        meshManagerUtils= new MeshManager.MeshManagerUtils(boatMeshObj, underWaterObj, boatRB);
        boatRB.centerOfMass = meshManagerUtils.MeshCenterOfGravity(boatMeshObj.GetComponent<MeshFilter>().mesh, new Vector3(0f, 0.28f, 0f)); //0.1f
        Debug.Log("DEBUG CoM : " + boatRB.centerOfMass);

    }

	void Start()
	{

        // FOR DEBUG
        centerOfMass = boatRB.centerOfMass;

        //Init the script that will modify the boat mesh
        

        //Meshes that are below and above the water
        underWaterMesh = underWaterObj.GetComponent<MeshFilter>().mesh;

        auxUnderWaterCenter = Vector3.zero;

    }

    // void Update()
    // {
    //     //Generate the under water and above water meshes

    //     meshManagerUtils.GenerateUnderwaterMesh();

    //     // Display the under water mesh - is always needed to get the underwater length for forces calculations
    //     meshManagerUtils.DisplayMesh(underWaterMesh, "UnderWater Mesh", meshManagerUtils.underWaterTriangleData);

    // }

    void FixedUpdate()
	{
        // test from update to fixedupdate
        meshManagerUtils.GenerateUnderwaterMesh();
        meshManagerUtils.DisplayMesh(underWaterMesh, "UnderWater Mesh", meshManagerUtils.underWaterTriangleData);

        //Add forces to the part of the boat that's below the water

        if (meshManagerUtils.underWaterTriangleData.Count > 0)
        {
            AddUnderWaterForces();
        }

    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.TransformPoint(new Vector3(-1.948444e-05f, -0.3083269f, 0.01228585f)), 0.05f); // -1.948444e-05, -0.3083269, 0.01228585
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(CoB, 0.05f); // -1.948444e-05, -0.3083269, 0.01228585

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.TransformPoint(new Vector3(-1.948444e-05f, -0.3083269f, 0.01228585f)), transform.TransformPoint(new Vector3(-1.948444e-05f, -0.3083269f, 0.01228585f))+new Vector3(0f ,-1f, 0f));

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(CoB, CoB+new Vector3(0f ,1f, 0f));


        Gizmos.color = Color.black;
        Gizmos.DrawLine(transform.TransformPoint(new Vector3(0f, 0f, 0f)), transform.TransformPoint(new Vector3(0f, 0f, 0f))+boatRB.velocity.normalized*2);

        Gizmos.color = Color.white;
        Gizmos.DrawSphere(auxUnderWaterCenter, 0.05f);

        // Draw a yellow sphere at the transform's position
        // Gizmos.color = Color.yellow;
        // Gizmos.DrawSphere(new Vector3(0f, -0.31f, 0.1f), 1f);
    }

    //Add all forces that act on the squares below the water
    void AddUnderWaterForces()
    {

        //Get all triangles
        List<TriangleData> underWaterTriangleData = meshManagerUtils.underWaterTriangleData;

        //Calculate the forces
        Vector3 forceToAdd = Vector3.zero;
        auxUnderWaterCenter = Vector3.zero;
        // BuoyancyData b_struct;

        auxUnderWaterCenter = meshManagerUtils.CreateAuxUnderWaterCenter();
        b_struct = meshManagerUtils.retrieveBuoyancyDate(auxUnderWaterCenter, boatRB.centerOfMass);

        CoB = b_struct.b_center;

        //Force 1 - The hydrostatic force (buoyancy)
        forceToAdd = PhysicsManagerUtils.DirectBuoyancyForce(rhoWater, b_struct.v_submerged);

      
        //Add the forces to the boat
        boatRB.AddForceAtPosition(forceToAdd, b_struct.b_center);

        //Normal
        // Debug.DrawRay(triangleData.center, triangleData.normal * 3f, Color.white);


    }
    //
    // Section for under water objstacals 
    //
    // void OnTriggerEnter(Collider collision)
    // {
    //     // Debug.Log("detect collision");
    //     // Debug.Log("Obstacle detected" + collision.gameObject + " at " + collision.ClosestPoint() + " [m] ");
    //     Debug.Log("Obstacle detected" + collision.gameObject.tag);

    //     foreach (ContactPoint contact in collision.contacts)
    //     {
    //         print(contact.thisCollider.name + " hit " + contact.otherCollider.name);
    //         // Visualize the contact point
    //         Debug.DrawRay(contact.point, contact.normal, Color.white);
    //     }
    // }
    // void OnCollisionEnter(Collision collision)
    // {

    //     // foreach (ContactPoint contact in collision.contacts)
    //     // {
    //     //     print(contact.thisCollider.name + " hit " + contact.otherCollider.name);
    //     //     // Visualize the contact point
    //     //     Debug.DrawRay(contact.point, contact.normal, Color.white);
    //     // }

    //     if(collision.gameObject.tag == "Lidar_C"){
	// 		Debug.Log(" [C] Lider detects obstacle");
	// 	}

	// 	if(collision.gameObject.tag == "Lidar_L"){
	// 		Debug.Log(" [L] Lider detects obstacle");
	// 	}

	// 	if(collision.gameObject.tag == "Lidar_R"){
	// 		Debug.Log(" [R] Lider detects obstacle");
	// 	}
    // }
}


