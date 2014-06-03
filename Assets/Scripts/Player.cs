using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

	public float jumpForce = 100.0f;
	public float forwardForce = 50.0f;
	public float torque = 25.0f;

	private bool goBack = false;

	//ParticleSystem blood;
	Transform weapon01;
	Transform weapon02;
	Transform weapon03;
	Transform weapon04;

	public Transform playerGoal;

	// Use this for initialization
	void Start () {
		//Transform t = transform.FindChild ("Blood");
		//blood = t.particleSystem;
		
		weapon01 = transform.FindChild ("Weapon01");
		weapon02 = transform.FindChild ("Weapon02");
		weapon03 = transform.FindChild ("Weapon03");
		weapon04 = transform.FindChild ("Weapon04");
	}
	
	// Update is called once per frame
	void Update () {
		Vector3 velocityTemp = rigidbody.velocity;
		Vector3 angVelocityTemp = rigidbody.angularVelocity;
		Vector3 oppositeForce;
		Vector3 oppositeAngForce;
		bool decelerate;
		//bool goBack = false;

		if (Input.GetKeyDown (KeyCode.Space)) {
			Attack();
		}

		if (Input.GetKey (KeyCode.S)) {
			rigidbody.velocity = Vector3.ClampMagnitude (rigidbody.velocity, 0);
		}

		if (Input.GetKey (KeyCode.DownArrow)) {
			if (rigidbody.velocity.sqrMagnitude >= -0.1f && rigidbody.velocity.sqrMagnitude <= 0.1f) {
				goBack = true;
			}

			if (!goBack) {
			   oppositeForce = - velocityTemp * 1.5f;
			   oppositeAngForce = - angVelocityTemp * 1.5f;
			   rigidbody.AddForce(oppositeForce.x,oppositeForce.y,oppositeForce.z);
			   rigidbody.AddTorque(oppositeAngForce.x, oppositeAngForce.y, oppositeAngForce.z);
			} else {
				if (rigidbody.velocity.sqrMagnitude < 25) {
					rigidbody.AddForce (-transform.forward * forwardForce * Time.deltaTime);
				}
			}
		}

		if (Input.GetKey (KeyCode.UpArrow)) {
			if (angVelocityTemp.x != 0 
			    || angVelocityTemp.y != 0 
			    || angVelocityTemp.z != 0) {

				oppositeAngForce = - (angVelocityTemp) * 0.1f;
				rigidbody.AddRelativeTorque(oppositeAngForce.x, oppositeAngForce.y, oppositeAngForce.z);
			}

			if (rigidbody.velocity.sqrMagnitude < 50 && rigidbody.velocity.magnitude >= 0) {
			   goBack = false;
			   rigidbody.AddForce (transform.forward * forwardForce * Time.deltaTime);
			} else {
			   oppositeForce = -velocityTemp;
			   rigidbody.AddForce(oppositeForce.x,oppositeForce.y,oppositeForce.z);
			}

		}
		if (Input.GetKey (KeyCode.LeftArrow)) {
			decelerate = false;

			if (rigidbody.angularVelocity.y > 0.25f) {
				decelerate = true;
		    }

			if (!decelerate) {
				if (rigidbody.angularVelocity.sqrMagnitude < 10) {
			       rigidbody.AddTorque (-transform.up * torque * Time.deltaTime);
				}
			} else {
			   oppositeAngForce = - (angVelocityTemp);
			   rigidbody.AddTorque(oppositeAngForce.x, oppositeAngForce.y, oppositeAngForce.z);
			}
		}
		if (Input.GetKey (KeyCode.RightArrow)) {
			decelerate = false;
			
			if (rigidbody.angularVelocity.y < -0.25f) {
				decelerate = true;
			}
			if (!decelerate) {
				if (rigidbody.angularVelocity.sqrMagnitude < 10) {
				   rigidbody.AddTorque (transform.up * torque * Time.deltaTime);
				}
			} else {
				oppositeAngForce = - (angVelocityTemp);
				rigidbody.AddTorque(oppositeAngForce.x, oppositeAngForce.y, oppositeAngForce.z);
			}
		}
	}
	
	public void hitPlayer () {   //(float receivedDamage) {
		Debug.Log ("OUCH!");
		//blood.Play ();
		Vector3 goalDir = playerGoal.position - transform.position;

		rigidbody.AddForce (goalDir.normalized * 10000.0f * Time.deltaTime);

	}
	
	void Attack() {
		weapon01.SendMessage ("Attack");
		weapon02.SendMessage ("Attack");
		weapon03.SendMessage ("Attack");
		weapon04.SendMessage ("Attack");
	}

}
