using UnityEngine;
using System.Collections;

public class BoatController : MonoBehaviour
{
    //Speed calculations
    private float currentSpeed;
    private Vector3 lastPosition;
	
    //------------------components from engines-------------------//
    // LEFT Componenets
    public Transform waterJetTransform_L;
    private float maxPower_L;
    private Vector3 errVel_L;
    private float agent_action_L = 0f;

    // RIGHT Components
    public Transform waterJetTransform_R;
    private float maxPower_R;  
    private Vector3 errVel_R;
    private float agent_action_R = 0f;

    // Max speed that the boat can reach.
    private float maxSpeed;

    private Rigidbody boatRB;
    private bool resetFlag = false;
    private bool randomFlag = false;
    private int stepCounter;
    
    /// <summary>
    /// The following line must be group togheter with the other debug functionalities.
    /// </summary>
    // Debug Code 
    // [Header("Action Debug - Set value for both engines [0,1]")]
    // public float leftEngineAction;
    // public float rightEngineAction;
    [Header("Speed Debug - Current Boat speed")]
    // public float SpeedModule;
    // public Vector3 SpeedDirection;
    public float VelX;
    public float VelZ;
    public float VelY;
    [Header("Angular Velocity")]
    public float angVelY;
    public float angVelX;
    public float angVelZ;    

    // [Header("Linear Acceleration")]
    // private Vector3 lastVelocity = new Vector3(0f, 0f, 0f);
    // public Vector3 acceleration;


    void Start() 
	{
        boatRB = GetComponent<Rigidbody>();
        maxPower_L = Parameters.current.left_maxPower;
        maxPower_R = Parameters.current.right_maxPower;
        maxSpeed = Parameters.current.maxSpeed;
      
    }

    void FixedUpdate()
    {
        
        if (resetFlag){
            StartCoroutine(SafeReset(randomFlag, stepCounter));
            
        }
        else{
            UpdatePropulsionForces();
        }
        
	}

    /// <summary>
    /// This function determin the force to apply at boat through the two Transformer.
    /// 
    /// The specifics of the agents, require that the boat velocity is controlled with a value [0;1] 
    /// passed to each engine.
    /// 
    /// If both engines receive the value 1 then the boat is push at max (100%) speed. 
    /// 0 stops the engine.
    /// 
    /// </summary>
    void UpdatePropulsionForces()
    {

        // The max speed is the sum of both engines pushing the boat at the maximum power
        Vector3 velocity_L = waterJetTransform_L.forward * (maxSpeed / 2) * agent_action_L;
        Vector3 velocity_R = waterJetTransform_R.forward * (maxSpeed / 2) * agent_action_R;


        // Velocity error
        // Used to determin if the boat is reaching the maximum allowable speed 
        if (agent_action_L == 0f){
            errVel_L = new Vector3(0f, 0f, 0f);
        }
        else{
            errVel_L = waterJetTransform_L.forward * (Vector3.Dot(velocity_L, boatRB.velocity / 2) / velocity_L.magnitude);
        }
        if (agent_action_R == 0f){
            errVel_R = new Vector3(0f, 0f, 0f);
        }
        else{
            errVel_R = waterJetTransform_R.forward * (Vector3.Dot(velocity_R, boatRB.velocity / 2) / velocity_R.magnitude);
        }

        // Each engine can work if only if they are under the water level
        float waveYPos_L = WaterManagerObject.current.GetWaveYPos(waterJetTransform_L.position);
        float waveYPos_R = WaterManagerObject.current.GetWaveYPos(waterJetTransform_R.position);

        // Left Engine
        if (waterJetTransform_L.position.y < waveYPos_L)
        {
            // If it starts from a standstill
            if (boatRB.velocity.magnitude == 0) // the boat is not moving
            {
                boatRB.AddForceAtPosition(velocity_L * maxPower_L, waterJetTransform_L.position);
            }
            // If it was already moving
            else
            {
                boatRB.AddForceAtPosition((velocity_L - errVel_L) * maxPower_L, waterJetTransform_L.position);
            }
        }
        // If the engine is above the water level
        else
        {
            boatRB.AddForceAtPosition(Vector3.zero, waterJetTransform_L.position);
        }
        // Left Engine
        if (waterJetTransform_R.position.y < waveYPos_R)
        {
            // If it starts from a standstill
            if (boatRB.velocity.magnitude == 0) // the boat is not moving
            {
                boatRB.AddForceAtPosition(velocity_R * maxPower_R, waterJetTransform_R.position);
            }
            // If it was already moving
            else
            {
                boatRB.AddForceAtPosition((velocity_R - errVel_R) * maxPower_R, waterJetTransform_R.position);
            }
        }
        // If the engine is above the water level
        else
        {
            boatRB.AddForceAtPosition(Vector3.zero, waterJetTransform_R.position);
        }


        // Debug Values - Printed on the Unity Inspector
        // SpeedModule = boatRB.velocity.magnitude;
        // SpeedDirection = boatRB.velocity.normalized;
        VelX = boatRB.velocity.x;
        VelY = boatRB.velocity.y;
        VelZ = boatRB.velocity.z;
        angVelX = boatRB.angularVelocity.x;
        angVelY = boatRB.angularVelocity.y;
        angVelZ = boatRB.angularVelocity.z;
        // Vector3 now_vel = boatRB.velocity;
		// acceleration = (now_vel - lastVelocity) / Time.fixedDeltaTime;
        // lastVelocity = boatRB.velocity;
        
    }


    IEnumerator SafeReset(bool randomFlag, int stepCounter){

        // GetComponent<BoatPhysics>().enabled = false;
        // GetComponent<MeshCollider>().enabled = false;
        GetComponent<Rigidbody>().velocity = new Vector3(0f,0f,0.000001f); 
        GetComponent<Rigidbody>().angularVelocity = new Vector3(0f,0f,0.000001f);
        // GetComponent<BoatController>().setMotorPower( 0f, 0f );

		if (randomFlag){
            GetComponent<Rigidbody>().isKinematic = true;
            transform.localPosition = new Vector3(Random.Range(-15f, 15f), 0.5f, Random.Range(-15f, 15f));
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            GetComponent<Rigidbody>().isKinematic = false;
        }
        else{
            GetComponent<Rigidbody>().isKinematic = true;
            // transform.localPosition = new Vector3(0f, 0f, 0f);
            transform.localPosition = Parameters.current.spawnPosition;
			transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            GetComponent<Rigidbody>().isKinematic = false;
        }
        
        transform.rotation = Quaternion.Euler(new Vector3(0f,0f,0f));

        // GetComponent<MeshCollider>().enabled = true;
        // GetComponent<BoatPhysics>().enabled = true;
        Debug.Log("Reset Coroutine");
        resetFlag = false;
        yield return new WaitForFixedUpdate();
        // yield return new WaitForEndOfFrame();
    }

    public void resetBoat(bool randomFlag, int stepCounter)
    {

        this.resetFlag = true;      
        this.randomFlag = randomFlag;
        this.stepCounter = stepCounter;

    }
    public void setMotorPower( float left, float right ) {
        agent_action_L = left;
        agent_action_R = right;
        
    }
}
