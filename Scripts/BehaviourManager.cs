using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
 * Niko Bazos (ndb9897@rit.edu)
 * This class manages the behaviours of in-game objects and their spawning
 */

public class BehaviourManager : MonoBehaviour
{
	// Attributes
	public int seekerCount;
	public List<GameObject> seekerList; //vehicle to control
	public int targetCount;
	public List<GameObject> targetList; //target to seek/flee
	public int obstCount;
	public List<GameObject> obstList; // obstacle to avoid

	public GameObject vehiclePrototype; //vehicle to instantiate
	public GameObject targetPrototype; //target to instantiate
	public GameObject obstPrototype; // obstacke to instantiate

	public Terrain terrain; //terrain we are walking in
	private TerrainGenerator terrainGenerator; //terrain information
	public Vector3 worldSize; //world size

	private BoundingSphere vehicleBS; //bounding sphere of the vehicle
	private BoundingSphere targetBS; //bounding sphere of the target

	public bool debug = false; // Draw debug lines or not

	//Materials
	public Material matRed;
	public Material matGreen;
	public Material matBlue;
	public Material matWhite;
	public Material matYellow;
	public Material matPink;
	public Material matBlack;

	// Use this for initialization
	void Start ()
	{
		//is vehicle prototype assigned in editor?
		if(null == vehiclePrototype)
		{
			Debug.Log("Error in " + gameObject.name + 
			          ": VehiclePrototype is not assigned.");
			Debug.Break();
		}
		//is target prototype assigned in editor?
		if(null == targetPrototype)
		{
			Debug.Log("Error in " + gameObject.name + 
			          ": VehiclePrototype is not assigned.");
			Debug.Break();
		}
		//is terrain assigned in editor?
		if(null == terrain)
		{
			Debug.Log("Error in " + gameObject.name + 
			          ": Terrain is not assigned.");
			Debug.Break();
		}
		//is the terrain assigned a terraingenerator component?
		terrainGenerator = terrain.GetComponent<TerrainGenerator>();
		if(null == terrainGenerator)
		{
			Debug.Log("Error in " + gameObject.name + 
			          ": Terrain is required to have a TerrainGenerator script");
			Debug.Break();
		}

		//initialize this world size with the terrain generator world size
		worldSize = terrainGenerator.worldSize;

		//instantiate humans
		for (int i = 0; i < targetCount; i++) 
		{
			// Add humans to the list
			targetList.Add (Instantiate (targetPrototype));

			//initialize target bounding sphere with target component
			targetBS = targetList[i].GetComponent<BoundingSphere>();

			//check if assigned
			if(null == targetBS)
			{
				Debug.Log("Error in " + gameObject.name + 
					": TargetPrototype is required to have a Bounding Sphere script");
				Debug.Break();
			}

			//initialize movementForces with vehicle's component
			MovementForces targetMF = targetList[i].GetComponent<MovementForces>();
			if(null == targetMF)
			{
				Debug.Log("Error in " + gameObject.name + 
					": TargetPrototype is required to have a MovementForces script");
				Debug.Break();
			}

			// Randomize human's inital position
			RandomizePosition(targetList[i]);
		}
			
		//instantiate zombies
		for(int i = 0; i < seekerCount; i++)
		{
			// Add zombies to the list
			seekerList.Add(Instantiate(vehiclePrototype));

			//initialize vehicle bounding sphere with vehicle component
			vehicleBS = seekerList[i].GetComponent<BoundingSphere>();
			//check is assigned
			if(null == vehicleBS)
			{
				Debug.Log("Error in " + gameObject.name + 
				          ": VehiclePrototype is required to have a Bounding Sphere script");
				Debug.Break();
			}
				

			//initialize movementForces with vehicle's component
			MovementForces vehicleMF = seekerList[i].GetComponent<MovementForces>();
			if(null == vehicleMF)
			{
				Debug.Log("Error in " + gameObject.name + 
				          ": VehiclePrototype is required to have a MovementForces script");
				Debug.Break();
			}

			// Randomize zombie's intial position
			RandomizePosition(seekerList[i]);
		}

		//instantiate obstacles
		for(int i = 0; i < obstCount; i++)
		{
			obstList.Add (Instantiate (obstPrototype));

			RandomizePosition (obstList [i]);
		}
	}


	//calculates the position of the object at random
	void RandomizePosition(GameObject theObject)
	{
		//Set position of target based on the size of the world
		Vector3 position = new Vector3 (Random.Range(0.0f,worldSize.x - 5.0f), 0.0f, Random.Range(0.0f,worldSize.z - 5.0f));
		//set the height of the object based on the position of the terrain
		position.y = terrainGenerator.GetHeight(position) + 1.0f;
		//set the position of target back
		theObject.transform.position = position;
	}

	//destroys human and adds a zombie
	void AddZombie(int humanIndex)
	{
		// stores the position where the human died
		Vector3 spawnPos = targetList[humanIndex].transform.position;

		// removes human
		Destroy (targetList [humanIndex]);
		targetList.RemoveAt (humanIndex);

		// adds zombie
		seekerList.Add ((GameObject)Instantiate (vehiclePrototype,spawnPos,Quaternion.identity));

	}
	
	// Update is called once per frame
	void Update ()
	{
		// draw debug lines
		if (Input.GetKeyDown (KeyCode.D) && !debug) 
		{
			debug = true;
		} 
		else if (Input.GetKeyDown (KeyCode.D) && debug) 
		{
			debug = false;
		}

		// get a random position in the world
		Vector3 randomPos = new Vector3 (Random.Range (5.0f, worldSize.x- 5.0f), 7.0f, Random.Range (5.0f, worldSize.z-5.0f));

		// spawn zombie or human
		if (Input.GetKeyDown (KeyCode.H)) 
		{
				targetList.Add((GameObject)Instantiate (targetPrototype,randomPos,Quaternion.identity));
		} 

		if (Input.GetKeyDown (KeyCode.Z)) 
		{
			seekerList.Add((GameObject)Instantiate (vehiclePrototype,randomPos,Quaternion.identity));
		} 

		// nested for loop that sets targets to pursue and evade for both zombies and humans
		// as well as obstacles to avoid 
		for (int i = 0; i < seekerList.Count; i++)
		{
			//initialize vehicle bounding sphere with vehicle component
			vehicleBS = seekerList[i].GetComponent<BoundingSphere>();
			ZombieMovementForces zombieMF = seekerList[i].GetComponent<ZombieMovementForces>();
			zombieMF.SetDebug (debug);
			zombieMF.SetObstList (obstList);

			for(int j = 0; j < targetList.Count; j++)
			{
				targetBS = targetList[j].GetComponent<BoundingSphere> ();
				HumanMovementForces targetMF = targetList [j].GetComponent<HumanMovementForces> ();
				targetMF.SetDebug (debug);
				targetMF.SetObstList (obstList);

				targetMF.SetTargetList(seekerList);
				zombieMF.SetTargetList(targetList);
				zombieMF.SetTarget ();
				targetMF.SetTarget ();

					

				//Check for collision
				if (vehicleBS.IsColliding (targetBS)) 
				{
					// To teleport the target after being caught
					//RandomizePosition (t);

					// Convert human to zombue
					AddZombie(j);
				}
			}
		}
	}
}
