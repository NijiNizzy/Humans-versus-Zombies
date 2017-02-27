using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
 * Niko Bazos (ndb9897@rit.edu)
 * This class is an abstract class that gives a basis for the forces that affects GameObjects
 */

abstract public class MovementForces : MonoBehaviour
{
	// Attributes
	public Vector3 position;//position of the object
	public Vector3 direction; //where we are facing
	public Vector3 velocity;//current velocity moving the object
	public Vector3 acceleration;//sum of forces acting on the object
	
	public float mass = 1.0f;// mass of the object
	public float maxSpeed = 75.0f; //maximum speed of vehicle

	public bool debug;

	protected BehaviourManager behaviourMngr; //behaviour manager to calculate forces
	protected Vector3 worldSize; //store the world size
	protected Vector3 targetPosition; //object to interact with
	protected GameObject target = null;
	protected List<GameObject> targetList = null; //object list to interact with
	protected List<GameObject> obstList = null;

	// Use this for initialization
	public virtual void Start ()
	{
		GameObject gameMngr = GameObject.Find("GameManager");
		if(null == gameMngr)
		{
			Debug.Log("Error in " + gameObject.name + 
			          ": Requires a GameManager object in the scene.");
			Debug.Break();
		}
		position = transform.position;
		behaviourMngr = gameMngr.GetComponent<BehaviourManager>();
		worldSize = behaviourMngr.worldSize;

		//Check that mas is initialized to something. Mass cannot be negative
		if (mass <= 0.0f)
		{
			mass = 0.01f;
		}
	}

	public abstract void SetTarget ();
		
	public abstract void SetTargetList (List<GameObject> targets);

	public abstract void SetDebug(bool d);

	public abstract void SetObstList (List<GameObject> obstacles);

	// Update the position based on the velocity and acceleration
	public abstract void UpdatePosition();

	public abstract void AvoidObstacle ();

	public abstract void OnRenderObject ();
	
	//Apply a force to the vehicle
	public abstract void ApplyForce(Vector3 force);

	// Pursue
	public abstract Vector3 Pursuit(Vector3 targetPosition);

	public abstract Vector3 Seek(Vector3 targetPosition);

	// Evade
	public abstract Vector3 Evade(Vector3 targetPosition);

	public abstract Vector3 Flee(Vector3 targetPosition);

	public abstract Vector3 Wander();
	
	//Apply friction to the vehicle based on the coefficient
	public abstract void ApplyFriction(float coeff);
	
	//Apply the trasformation
	public abstract void SetTransform();

	// Bounce the object towards the center
	public abstract void BounceTowardsCenter();
}
