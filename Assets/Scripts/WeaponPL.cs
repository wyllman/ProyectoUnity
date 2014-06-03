using UnityEngine;
using System.Collections;

public class WeaponPL : MonoBehaviour {
	public float launchForce = 200;
	public GameObject ammoPrefab;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}


	
	public void Attack() {
		GameObject go = GameObject.Instantiate (
			ammoPrefab,
			transform.position,
			Quaternion.identity) as GameObject;
		go.rigidbody.AddForce (transform.forward * launchForce);
	}
}
