using UnityEngine;
using System.Collections;

public class Enemy_Weapon : MonoBehaviour {

	public float launchForce = 400;
	public GameObject ammoPrefab;

	private float attackTime = 0.75f;
	
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (attackTime > 0) {
		   attackTime -= Time.deltaTime;
		}
	}
	
	
	
	public void Attack() {
		if (attackTime < 0) {
		   GameObject go = GameObject.Instantiate (
			   ammoPrefab,
			   transform.position,
		 	   Quaternion.identity) as GameObject;
		   go.rigidbody.AddForce (transform.up * launchForce);
			attackTime = 0.75f;
		}
	}
}
