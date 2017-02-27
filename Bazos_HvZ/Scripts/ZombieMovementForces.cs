using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
 * Niko Bazos (ndb9897@rit.edu)
 * This class is an implementation of MovementForces specifc for humans
 */

public class ZombieMovementForces : MovementForces 
{
	// Attributes
	public float seekWeight;
	public float maxForce;
	public float detectDistance;

	// Use this for initialization
	public override void Start () 
	{
		base.Start ();
		maxForce = 100.0f;
		detectDistance = 20.0f;
		seekWeight = 20.0f;

	}

	// sets the closest human target
	override public void SetTarget()
	{
		float distance = Vector3.Distance (gameObject.transform.position, targetList [0].transform.position);
		int targetIndex = 0;

		for (int i = 0; i < targetList.Count; i++) 
		{
			if (Vector3.Distance (gameObject.transform.position, targetList [i].transform.position) < distance) 
			{
				distance = Vector3.Distance (gameObject.transform.position, targetList [i].transform.position);
				targetIndex = i;
			}
		}

		target = targetList [targetIndex];
	}

	// sets the list of human targets
	override public void SetTargetList(List<GameObject> targets)
	{
		targetList = targets;
	}

	// sets the bool for the debug lines to be on or off
	override public void SetDebug(bool d)
	{
		debug = d;
	}

	// sets the list of obstacles
	override public void SetObstList(List<GameObject> obstacles)
	{
		obstList = obstacles;
	}

	// Update the position based on the velocity and acceleration
	override public void UpdatePosition()
	{
		// update position to current tranform
		position = transform.position;

		// avoids the obstacles
		AvoidObstacle ();

		// seek the target
		if (target != null) 
		{
				Vector3 seekingForce = Pursuit (target.transform.position);
				ApplyForce (seekingForce);
		} 
		// otherwise wander
		else 
		{
			Vector3 wanderForce = Wander ();
			ApplyForce (wanderForce);

		}

		//Step 1: Add Acceleration to Velocity * Time
		velocity += acceleration * Time.deltaTime;
		//Step 2: Add vel to position * Time
		position += velocity * Time.deltaTime;
		//Step 3: Reset Acceleration vector
		acceleration = Vector3.zero;
		//Step 4: Calculate direction (to know where we are facing)
		direction = velocity.normalized;
	}

	// loops through each obstacle and checks the GameObject the script is attached to and checks to avoid
	override public void AvoidObstacle()
	{
		for (int i = 0; i < obstList.Count; i++) 
		{
			// forces to store
			Vector3 steer;
			Vector3 desiredVelocity = Vector3.zero;
			// gets the radii
			float obstRadius = obstList[i].GetComponent<CharacterController> ().radius;
			float agentRadius = gameObject.GetComponentInParent<CharacterController> ().radius;

			// Get vector from agent to an obstacle center - vecToCenter
			Vector3 vecToCenter = obstList[i].transform.position - gameObject.transform.position;

			// Remove obstacles too far away from the agent
			if ((vecToCenter.magnitude > detectDistance)) 
			{
				steer = Vector3.zero;
			}
			// Remove obstacles behind the agent
			else if (Vector3.Dot (vecToCenter, gameObject.transform.forward) < 0) 
			{
				steer = Vector3.zero;
			}
			else if ((agentRadius + obstRadius) < Vector3.Dot (vecToCenter, gameObject.transform.right)) 
			{
				steer = Vector3.zero;
			}

			// If obstacles is in the line of sight
			else 
			{
				if (Vector3.Dot (vecToCenter, gameObject.transform.right) > 0) 
				{
					desiredVelocity = -gameObject.transform.right * maxSpeed;
				} 
				else 
				{
					desiredVelocity = gameObject.transform.right * maxSpeed;
				}
			}
			// Determine the steering force
			steer = desiredVelocity - velocity;

			// Increases weight of force
			steer *= detectDistance / vecToCenter.magnitude;

			// Apply the force
			ApplyForce(steer);
		}

	}

	override public void OnRenderObject()
	{
		if (debug) 
		{
			//velocity
			GL.PushMatrix ();
			behaviourMngr.matGreen.SetPass (0);
			GL.Begin (GL.LINES);
			GL.Vertex (position);
			GL.Vertex (position + direction * 2.0f);			
			GL.End ();

			//To target
			if (null != targetList) 
			{
				foreach (GameObject v in targetList) 
				{
					behaviourMngr.matBlack.SetPass (0);
					GL.Begin (GL.LINES);
					GL.Vertex (position);
					GL.Vertex (v.transform.position);
					GL.End ();
				}
			}


			// display right vector
			behaviourMngr.matBlue.SetPass (0);
			GL.Begin (GL.LINES);
			GL.Vertex (position);
			GL.Vertex (position + transform.forward * 2.0f);
			GL.End ();

			GL.PopMatrix ();
		}

		
	}

	public void OnDrawGizmos()
	{
		if (debug) 
		{
			// Display the future position
			Gizmos.color = Color.red;
			Gizmos.DrawSphere (transform.position + velocity / 2.0f, 0.25f);
		}
	}

	//Apply a force to the vehicle
	override public void ApplyForce(Vector3 force)
	{
		//F = M * A
		//F / M = M * A / M
		//F / M = A * (M / M)
		//F / M = A * 1
		//A = F / M

		// clamps the force if too large
		if (force.magnitude > maxForce) 
		{
			force = Vector3.ClampMagnitude(force, maxForce);
		}

		acceleration += force / mass;
	}

	// Pursue
	override public Vector3 Pursuit(Vector3 targetPosition)
	{
		Vector3 distance = targetPosition - position;
		float update = distance.magnitude / maxForce;
		Vector3 futurePosition = targetPosition + target.GetComponent<MovementForces> ().velocity * update;
		return Seek (futurePosition);
	}

	override public Vector3 Seek(Vector3 targetPosition)
	{
		//Step 1: Calculate the desired unclamped velocity
		//which is from this vehicle to target's position
		Vector3 desiredVelocity = targetPosition - position;

		//Step 2: Calculate maximum speed
		//so the vehicle does not move faster than it should
		//desiredVelocity = Vector3.ClampMagnitude(desiredVelocity, maxSpeed);

		//Step 2 Alternative:
		desiredVelocity.Normalize ();
		desiredVelocity *= maxSpeed;

		//Step 3: Calculate steering force
		Vector3 steeringForce = desiredVelocity - velocity;

		steeringForce *= seekWeight;

		//Step 4: return the force so it can be applied to this vehicle
		return steeringForce;
	}

	// Evade
	override public Vector3 Evade(Vector3 targetPosition)
	{
		Vector3 distance = targetPosition - position;
		float update = distance.magnitude / maxForce;
		Vector3 futurePosition = targetPosition + target.GetComponent<MovementForces> ().velocity * update;
		return Flee (futurePosition);
	}

	override public Vector3 Flee(Vector3 targetPosition)
	{
		//Step 1: Calculate the desired unclamped velocity
		//which is from this vehicle to target's position
		Vector3 desiredVelocity = position - targetPosition;

		//Step 2: Calculate maximum speed
		//so the vehicle does not move faster than it should
		//desiredVelocity = Vector3.ClampMagnitude(desiredVelocity, maxSpeed);

		//Step 2 Alternative:
		desiredVelocity.Normalize ();
		desiredVelocity *= maxSpeed;

		//Step 3: Calculate steering force
		Vector3 steeringForce = desiredVelocity - velocity;

		//Step 4: return the force so it can be applied to this vehicle
		return steeringForce;
	}

	override public Vector3 Wander()
	{
		// get a random point within the unit sphere
		Vector3 wanderForce = (Random.insideUnitSphere * 50.0f);

		// kepe the y to 0
		wanderForce.y = 0.0f;

		// scale the vector
		wanderForce *= seekWeight;

		return wanderForce;

	}

	//Apply friction to the vehicle based on the coefficient
	override public void ApplyFriction(float coeff)
	{
		// Step 1: Oposite velocity
		Vector3 friction = velocity * -1.0f;
		// Step 2: Normalize so is independent of velocity
		friction.Normalize ();
		// Step 3: Multiply by coefficient
		friction = friction * coeff;
		// Step 4: Add friction to acceleration
		acceleration += friction;
	}

	//Apply the trasformation
	override public void SetTransform()
	{
		// keeps the y position constant
		position.y = 7.0f;
		transform.position = position;
		//orient the object
		transform.right = direction;
	}
		
	// Bounce the object towards the center
	override public void BounceTowardsCenter()
	{	
		// has a buffer zone 5.0f before seeks the center

		//Check within X
		if(position.x > worldSize.x - 5.0f)
		{
			Seek (new Vector3 (worldSize.x / 2.0f, 7.0f, worldSize.z / 2.0f));

			velocity.x *= -1.0f;
		}
		else if(position.x < 5.0f)
		{
			Seek (new Vector3 (worldSize.x / 2.0f, 7.0f, worldSize.z / 2.0f));

			velocity.x *= -1.0f;
		}

		//check within Z
		if(position.z > worldSize.z - 5.0f)
		{
			Seek (new Vector3 (worldSize.x / 2.0f, 7.0f, worldSize.z / 2.0f));

			velocity.z *= -1.0f;
		}
		else if(position.z < 5.0f)
		{
			Seek (new Vector3 (worldSize.x / 2.0f, 7.0f, worldSize.z / 2.0f));

			velocity.z *= -1.0f;
		}
	}

	void Update ()
	{
		UpdatePosition (); //Update the position based on forces
		BounceTowardsCenter(); // keeps forces within park
		SetTransform();//Set the transform before render
	}
}
