using UnityEngine;
using System.Collections;

/*
 * Niko Bazos (ndb9897@rit.edu)
 * This class manages the information for an obstacle
 */

public class Obstacle : MonoBehaviour 
{
	// Attributes
	public Vector3 pos;
	public float radius;

	// Use this for initialization
	void Start () 
	{
		pos = gameObject.transform.position;
		radius = gameObject.GetComponent<CharacterController>().radius;
	
	}
	
	// Update is called once per frame
	void Update () 
	{
		pos = gameObject.transform.position;
	}

}
