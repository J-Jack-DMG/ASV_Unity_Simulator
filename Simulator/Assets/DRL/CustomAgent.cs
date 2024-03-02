using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
// using UnityEditor.UI;

// Declaration of the main class for the agent, that inherited from the Agent class of Unity-ML Agents
public class CustomAgent : Agent {


	// Variables for the initial position of the target
	// private bool randomizeAgentRotation = false;
    private Rigidbody AgentRB;
	public bool randomizeTargetPosition = true;
	public bool randomizeAgentPosition = false;
	public float randomizeArea = 10f;
	public float distanceNormFact = 15f;

	// The object that represent the target
	private Transform target;

	// Basic starting position/rotation of the agent for the reset
	// after every episode
	private Vector3 startingPos;
	private Quaternion startingRot;

	// Private variables for internal computations
	private int stepCounter = 0;

	float distance_now_A;
	float distance_prev_A;
	float distance_norm_A;


	float distance_now_O;
	float distance_prev_O;
	float distance_norm_O;

	float angle_A;
	float angle_A_norm;
	float angle_O;
	float angle_O_norm;


	// [Header("For Debug: rotations")]
	// public rotX;
	// public rotY;
	// public rotZ;
	private Vector3 lastVelocity = new Vector3(0f, 0f, 0f);

	// Called at the creation of the enviornment (before the first episode)
	// and only once
	public override void Initialize() {
		// Fill the game object target searching for the tag (setted in the editor)
		target = GameObject.FindGameObjectWithTag("Target").transform;
        AgentRB = GetComponent<Rigidbody>();
		// Setting the basic rotation and position
		// startingPos = transform.position;
		// startingRot = transform.rotation;
	}


	// Called at the every new episode of the training loop,
	// after each reset (both from target, crash or timeout)
	// Tutto quello che deve essere resettato va qua:
	/// Onde, barca, forze, target.
	// Reset parametri onda tra ragne 0-1 [discreti] 0 0.5 1 (usa interi)
	// funzione reset per onde qui dentro
	public override void OnEpisodeBegin() {	

        Debug.Log("Episode Begin");

        GetComponent<BoatController>().resetBoat(randomizeAgentPosition, stepCounter);
        
		WaterManagerObject.current.Reset_Water();
		
		if ( randomizeTargetPosition )
			target.position = new Vector3(Random.Range(-randomizeArea, randomizeArea), 0.0f, Random.Range(-randomizeArea, randomizeArea));
			// target.position = new Vector3(Random.Range(-7f, 7f), 0.0f, Random.Range(4f, 5f));
		
		stepCounter = 0;

		distance_now_A = Vector3.Distance( target.position, AgentRB.position );
		distance_now_O = distance_now_A;
		distance_prev_A = distance_now_A;
		distance_prev_O = distance_now_A;
		
	}


	// Listener for the observations collections.
	// The observations for the LiDAR sensor are inherited from the 
	// editor, in thi function we add the other observations (angle, distance)
	public override void CollectObservations(VectorSensor sensor) {	
		// Compute the distance between agent and target with the built in function,
		// based on the position of the two elements
		// Vector2 targetPos = new Vector2( target.position.x, target.position.z);
		// Vector2 agentPos = new Vector2( AgentRB.position.x, AgentRB.position.z );
		// Normalization of the distance on the size of the room in [0, 1]
		// Compute the angle using the built-in function, the function returns a value between -180 and +180
		// float angle = Vector3.SignedAngle(targetDir, transform.forward, transform.up);
		// Normalize between [0, 1] also the angle
		// distance_now = Vector2.Distance( targetPos, agentPos );
		// distance_now = distance_now / distanceNormFact;
		// sensor.AddObservation( distance_now );

		// Compute the distance between agent and target with the built in function,
		// based on the position of the two elements
		// Vector3 targetPos = target.position;
		// Vector3 agentPos = AgentRB.position;
		// distance_now = Vector3.Distance( targetPos, agentPos );
		// distance_now = distance_now / distanceNormFact;
		distance_now_O = Vector3.Distance( target.position, AgentRB.position );
		distance_norm_O = (distance_prev_O - distance_now_O) / (distance_prev_O + distance_now_O); 
		float distance_debug = Mathf.Clamp( distance_norm_O * 30,-1, 1);
		// Debug.Log("OBSERVATION-" + stepCounter + ": distance : " + distance_norm_O + " // distance_debug : " + distance_debug );
		sensor.AddObservation( distance_debug );
		distance_prev_O = distance_now_O;


		Vector3 tmp_norm_vel = AgentRB.velocity.normalized;
		Vector3 tmp_norm_dir = (target.position - AgentRB.position).normalized; 
		angle_O = Vector3.SignedAngle(tmp_norm_dir, tmp_norm_vel,  transform.up);
		angle_O_norm = angle_O / 180f;
		sensor.AddObservation( angle_O_norm );

		// Vector3 targetDir = target.position - AgentRB.position;
		// float angle = Vector3.SignedAngle(targetDir, transform.right, transform.up);
		// angle = 0.5f - (angle / 360f);
		// sensor.AddObservation( angle );


		// _________________Acceleration________________//
		// const float maxModVel = 5.6f;
		// const float maxModAcc = 6f;
		// Vector3 now_vel = GetComponent<Rigidbody>().velocity;
		// Vector3 acceleration = (now_vel - lastVelocity) / Time.fixedDeltaTime;
		// lastVelocity = now_vel;
		// float acc_x = ((Mathf.Clamp(acceleration.x, -maxModAcc, maxModAcc)) + maxModAcc) / (2 * maxModAcc);
		// float acc_y = ((Mathf.Clamp(acceleration.y, -maxModAcc, maxModAcc)) + maxModAcc) / (2 * maxModAcc);
		// float acc_z = ((Mathf.Clamp(acceleration.z, -maxModAcc, maxModAcc)) + maxModAcc) / (2 * maxModAcc);
		// sensor.AddObservation( acc_x );
		// sensor.AddObservation( acc_y );
		// sensor.AddObservation( acc_z );


		// The max velocity is explained inside a comment in issue #1 task #11
		// With the current setting The module of the maxspeed is 5.5 [m/s]
		// This means that the maximum speed that both the x-axis and z-axis can experience 
		// falls within the range -5.5 and 5.5
		const float maxModVel = 2f;
		float linVelX = Mathf.Clamp(GetComponent<Rigidbody>().velocity.x, -maxModVel, maxModVel);
		// float linVelY = Mathf.Clamp(GetComponent<Rigidbody>().velocity.y, -maxModVel, maxModVel);
		float linVelZ = Mathf.Clamp(GetComponent<Rigidbody>().velocity.z, -maxModVel, maxModVel);
		float linVelX_norm = (linVelX + maxModVel) / (2*maxModVel);
		// float linVelY_norm = (linVelY + maxModVel) / (2*maxModVel);
		float linVelZ_norm = (linVelZ + maxModVel) / (2*maxModVel);
		sensor.AddObservation( linVelX_norm );
		// sensor.AddObservation( linVelY_norm );
		sensor.AddObservation( linVelZ_norm );


		const float maxModAngVel = 1.3f;
		// float angVelX = Mathf.Clamp(GetComponent<Rigidbody>().angularVelocity.x, -maxModAngVel, maxModAngVel);
		float angVelY = Mathf.Clamp(GetComponent<Rigidbody>().angularVelocity.y, -maxModAngVel, maxModAngVel);
		// float angVelZ = Mathf.Clamp(GetComponent<Rigidbody>().angularVelocity.z, -maxModAngVel, maxModAngVel);
		// float angVelX_norm = (angVelX + maxModAngVel) / (2*maxModAngVel);
		float angVelY_norm = (angVelY + maxModAngVel) / (2*maxModAngVel);
		// float angVelZ_norm = (angVelZ + maxModAngVel) / (2*maxModAngVel);
		// sensor.AddObservation( angVelX );
		sensor.AddObservation( angVelY );
		// sensor.AddObservation( angVelZ );


		// Addding additional information for the boat:
		// const float maxRotation = 360f;
		// float tiltX = ((AgentRB.rotation.x + 360) % maxRotation) / maxRotation; // this handle the range [-180°, inf]
		// float tiltY = ((AgentRB.rotation.y + 360) % maxRotation) / maxRotation; // this handle the range [-180°, inf]
		// float tiltZ = ((AgentRB.rotation.z + 360) % maxRotation) / maxRotation; // this handle the range [-180°, inf]
		// sensor.AddObservation( tiltX );
		// sensor.AddObservation( tiltY );
		// sensor.AddObservation( tiltZ );



		// Debug.Log("tilt X: " + tiltX);
		// Debug.Log("tilt Y: " + tiltY);
		// Debug.Log("tilt Z: " + tiltZ);
		// Adding all the information the to observation spaces
		
		
	}

	// Listener for the action received, both from the neural network and the keyboard
	// (if heuristic mode), inside the Python script, the action is passed with the step funciton
	// il ppo manda il segnale per l'agente. I valori sono tra 0 e 1 (fermo e massimo)
	public override void OnActionReceived(ActionBuffers actionBuffers)	{

		// Set Actions
		
		// Get the actions from the input
		float leftPower = actionBuffers.ContinuousActions[0];
		float rightPower = actionBuffers.ContinuousActions[1];

		// The negatives values are set to zero, and they are not rewarded later.
        leftPower = Mathf.Clamp(leftPower, 0f, 1f);
        rightPower = Mathf.Clamp(rightPower, 0f, 1f);
		// Call the boat controller to set the speed 
		GetComponent<BoatController>().setMotorPower( leftPower, rightPower );


		// Set Rewards

		// Vector3 targetPos = target.position;
		// Vector3 agentPos = AgentRB.position;
		

		// This is the distance normalized,
		// Its values are in this range: (-1,1)
		distance_now_A = Vector3.Distance( target.position, AgentRB.position );
		distance_norm_A = (distance_prev_A - distance_now_A) / (distance_prev_A + distance_now_A); 
		float distance_debug_A = Mathf.Clamp( distance_norm_A * 30,-1, 1);
		// Debug.Log("ACTION-" + stepCounter + ": distance : " + distance_norm_A + " // distance_debug : " + distance_debug_A );

		// Vector3 tmp_norm_vel = AgentRB.velocity.normalized;
		// Vector3 tmp_norm_dir = (target.position - AgentRB.position).normalized; 
		// angle_A = Vector3.Angle(tmp_norm_dir, tmp_norm_vel);
		// angle_A_norm = angle_A / 180f;

		// // Debug.Log("Angle : " + angle_A);
		// AddReward(distance_debug_A);
		// if (distance_debug_A > 0){
		// 	if (leftPower > 0 || rightPower > 0){
		// 		AddReward(0.005f);
		// 	}
		// }
		// else{
		// 	if ((leftPower > 0) ^ (rightPower > 0)){
		// 		AddReward(0.005f);
		// 	}
		// }
		// if (angle_A < 15f){
		// 	AddReward(0.1f);
		// 	Debug.Log("Angle : " + angle_A);
		// 	if (leftPower > 0 && rightPower > 0){
		// 		AddReward(0.005f);
		// 	}
		// }
		// AddReward(-0.001f);



		// AddReward(distance_debug_A);
		// // if (distance_norm_A > 0){
		// if (angle_A < 15f){
		// 	AddReward(0.1f);
		// 	Debug.Log("Angle : " + angle_A);
		// 	if (leftPower > 0 || rightPower > 0){
		// 		AddReward(0.05f);
		// 	}
		// }
		// // else{
		// // 	if ((leftPower > 0) ^ (rightPower > 0)){
		// // 		AddReward(0.005f);
		// // 	}
		// // }
        // // R- for any step the agent doesn't reach the goal
		// AddReward(-0.02f);

		// -----------------------------build_08-----------------------------

		// Reward based on the distance
		// Negative if it moves away
		// Positive if it gets closser
		// Greater if it moves faster
		// smaller if it moves slower
		// AddReward(distance_debug_A); 

		// Reward base on getting close to the target correctly
		// if It MOVES toward the target on the trajectory to reach the taget correctly
		// if:
		// 	- Stay on the same point
		//  - move in any other direction exept for the correct one
		// then reward negative but positive if it moves left or right
		// if (angle_A < 15f && distance_debug_A > 0){
		// 	AddReward(0.1f);
			// Debug.Log("Angle : " + angle_A);
			// if (leftPower > 0 && rightPower > 0){
			// 	AddReward(0.005f);
			// }
		// }
		// else{
		// 	AddReward(-0.0005f);
		// 	if(leftPower > 0 ^ rightPower > 0){
		// 		AddReward(0.0004f);
		// 	}
		// }
		// reward negative to force the agents to learn
		// AddReward(-0.001f);


		// ----------------------build_09_fix_01-----------------------------

		// AddReward(distance_debug_A); 

		// if (distance_debug_A > 0){
		// 	if (leftPower > 0 || rightPower > 0){
		// 		AddReward(0.002f); // 0.001 for build previous build_09_fix_01
		// 	}
		// 	if (angle_A < 15f){
		// 		AddReward(0.002f); // 0.001 for build previous build_09_fix_01
		// 	}
		// }
		// // reward negative to force the agents to learn
		// AddReward(-0.001f); // <---- -0.003 for build_09_fix  // -0.005f for build 09 // 



		// stepCounter += 1;
		// distance_prev_A = distance_now_A;
		
		// ----------------------build_10-----------------------------

		// Description ENV Reward:
		// When the agnet understands in which direction it should move, receives a reward that is based on how fast it move closer to the target
		// If it moves close to the target it 


		// AddReward(distance_debug_A); 

		// if (distance_debug_A > 0){
		// 	if (leftPower > 0 || rightPower > 0){
		// 		AddReward(0.005f);
		// 		if (angle_A < 15f){
		// 			AddReward(0.005f);
		// 		}
		// 	}
			
		// }
		// // reward negative to force the agents to learn
		// AddReward(-0.0001f); 

		// stepCounter += 1;
		// distance_prev_A = distance_now_A;

		// ------------------build_11 -------------------------------------------
		// AddReward(distance_debug_A); 

		// if (distance_debug_A > 0){
		// 	if (leftPower > 0 || rightPower > 0){
		// 		AddReward(0.005f);
		// 	}
			
		// }
		// // reward negative to force the agents to learn
		// AddReward(-0.001f); 

		// stepCounter += 1;
		// distance_prev_A = distance_now_A;

		// ------------------build_12 -------------------------------------------
		// if (distance_debug_A > 0){
		// 	float reward = distance_debug_A * (1f - angle_A_norm);
		// 	AddReward(reward);
		// 	// Debug.Log("Reward: " + reward);

		// }
		// else{
		// 	float reward = -0.001f * (angle_A_norm);
		// 	AddReward(reward);
		// 	// Debug.Log("Reward: " + reward);
		// }
		// AddReward(-0.0005f);
		// stepCounter += 1;
		// distance_prev_A = distance_now_A;

		// ------------------build_13 -------------------------------------------
		// if (distance_debug_A > 0 && angle_A > 20){
		// 	float reward = distance_debug_A * (1f - angle_A_norm);
		// 	AddReward(reward);
		// 	// Debug.Log("Reward: " + reward);

		// }
		// else{
		// 	float reward = -0.001f * (angle_A_norm);
		// 	AddReward(reward);
		// 	// Debug.Log("Reward: " + reward);
		// }
		// AddReward(-0.0005f);
		// stepCounter += 1;
		// distance_prev_A = distance_now_A;

		// ------------------Build_13_01-------------------------------------------
		// if (distance_debug_A > 0 && angle_A > 5){
		// 	float reward = distance_debug_A * (1f - angle_A_norm);
		// 	AddReward(reward);
		// 	// Debug.Log("Reward: " + reward);

		// }
		// else{
		// 	float reward = -0.001f * (angle_A_norm);
		// 	AddReward(reward);
		// 	// Debug.Log("Reward: " + reward);
		// }
		// AddReward(-0.000025f);
		// stepCounter += 1;
		// distance_prev_A = distance_now_A;
		
		// -----------------Build 14 ------------------------------------------
		// Vector3 tmp_norm = transform.forward;
		// Vector3 tmp_norm_dir = (target.position - AgentRB.position).normalized; 
		// angle_A = Vector3.Angle(tmp_norm_dir, tmp_norm);
		// angle_A_norm = angle_A / 180f;

		// Debug.Log("Angle: " + angle_A);

		// if (distance_debug_A > 0 && angle_A > 5){
		// 	float reward = distance_debug_A * (1f - angle_A_norm);
		// 	AddReward(reward);
		// 	// Debug.Log("Reward: " + reward);

		// }
		// else{
		// 	float reward = -0.001f * (angle_A_norm);
		// 	AddReward(reward);
		// 	// Debug.Log("Reward: " + reward);
		// }
		// AddReward(-0.000025f);
		// stepCounter += 1;
		// distance_prev_A = distance_now_A;

		// -----------------Build 15 ------------------------------------------
		// Vector3 tmp_norm = transform.forward;
		// Vector3 tmp_norm_dir = (target.position - AgentRB.position).normalized; 
		// angle_A = Vector3.Angle(tmp_norm_dir, tmp_norm);
		// angle_A_norm = angle_A / 180f;

		// Debug.Log("Angle: " + angle_A);

		// if (distance_debug_A > 0 && angle_A > 5){
		// 	float reward = distance_debug_A * (1f - angle_A_norm);
		// 	AddReward(reward);
		// 	// Debug.Log("Reward: " + reward);

		// }
		// // else{
		// // 	float reward = -0.001f * (angle_A_norm);
		// // 	AddReward(reward);
		// // 	// Debug.Log("Reward: " + reward);
		// // }
		// AddReward(-0.00005f);
		// stepCounter += 1;
		// distance_prev_A = distance_now_A;


		// -------------------Build_ 16 ----------------------------------------
		// Vector3 tmp_norm = transform.forward;
		// Vector3 tmp_norm_dir = (target.position - AgentRB.position).normalized; 
		// angle_A = Vector3.Angle(tmp_norm_dir, tmp_norm);
		// angle_A_norm = angle_A / 180f;

		// Debug.Log("Angle: " + angle_A);

		// if (distance_debug_A > 0 && angle_A > 5){
		// 	float reward = distance_debug_A * (1f - angle_A_norm);
		// 	AddReward(reward);
		// 	// Debug.Log("Reward: " + reward);

		// }
		// else{
		// 	float reward = -0.001f * (angle_A_norm);
		// 	AddReward(reward);
		// 	// Debug.Log("Reward: " + reward);
		// }
		AddReward(-0.005f);
		// stepCounter += 1;
		// distance_prev_A = distance_now_A;
	}

	


	// Debug function, useful to control the agent with the keyboard in heurisitc mode
	// (must be setted in the editor)
	public override void Heuristic(in ActionBuffers actionsOut) { 
		// Set the basic action and wait or a keyboard key
		float leftAction = 0f; 
		float rightAction = 0f;
		if (Input.GetKey(KeyCode.D)) rightAction = 1f;
		if (Input.GetKey(KeyCode.A)) leftAction = 1f;
		// Add the action to the actionsOut object
		var actions = actionsOut.ContinuousActions;
		// Insert the selected action in the buffer to be executed
		actions[0] = leftAction; actions[1] = rightAction;
	}


	// Listener for the collison with a solid object
	private void OnCollisionEnter(Collision collision) { 
	// private void OnCollisionStay(Collision collision) { 
	// 	Check if the collision is within an obstacle (avoid activation with the floor)
	// 	or with a wall, the end of the episode is now menaged by the wrapper.
	// 	Set the reward base value for a crash

		

		if (collision.collider.CompareTag("Obstacle")){ 
			SetReward(-1f);	
			EndEpisode();
		}


		if (collision.collider.CompareTag("Wall")){
			SetReward(-1f);
			EndEpisode();
		}


		if (collision.collider.CompareTag("Target")){
			SetReward(1f);
			EndEpisode();
		}
		
		

	}

	// void OnTriggerEnter(Collider collision)
    // {
    //     // Debug.Log("detect collision");
    //     Debug.Log(collision.gameObject);
    //     Debug.Log(collision.gameObject.tag);

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
