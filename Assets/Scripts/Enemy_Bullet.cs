using UnityEngine;
using System.Collections;

public class Enemy_Bullet : MonoBehaviour {

	float lifeTime = 5.0f;
	// Use this for initialization
	void Start () {
		
	}
	
	void OnTriggerEnter(Collider other) {
		if (other.CompareTag("SolidObj")) {
			AutoDestruct();
		}
		
		if (other.CompareTag("Player")) {
			//Debug.Log("Player herido!");
			other.SendMessage("hitPlayer", SendMessageOptions.DontRequireReceiver);
			AutoDestruct();
		}
		
	}
	
	void Update() {
		lifeTime -= Time.deltaTime;
		if (lifeTime <= 0)
			AutoDestruct ();
	}
	
	void AutoDestruct() {	
		Destroy (gameObject);
	}
}
