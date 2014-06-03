using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour {

	float lifeTime = 5.0f;
	// Use this for initialization
	void Start () {
	
	}
	
	void OnTriggerEnter(Collider other) {
		if (other.CompareTag("SolidObj")) {
			AutoDestruct();
		}

		if (other.CompareTag("Enemy")) {
			//Debug.Log("Enemigo herido!");
			other.SendMessage("hitEnemy", SendMessageOptions.DontRequireReceiver);
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
