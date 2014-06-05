using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour {
	/*
	 * Estados de actuación de cada enemigo.
	 * IDLE: El enemigo permanece en estado pasivo a la espera de 
	 *       que el jugador se encuentre dentro de un radio de 
	 *       escucha.
	 * 
	 * LOOKING_TARGET: El enemigo se encuentra en un estado de 
	 *       búsqueda de visión (RayCast) del jugador.
	 * 
	 * PURSUIT_TARGET: El enemigo se encuentra en un estado de 
	 *       persecución del jugador hasta la distancia de 
	 *       disparo.
	 * 
	 * SHOOT_TARGET: El enemigo se encuentra en un estado de
	 *       disparo al jugador.
	 *
	 */
	enum EnemyStates { IDLE, LOOKING_TARGET, PURSUIT_TRAGET, SHOOT_TARGET };
	private EnemyStates actualState = EnemyStates.IDLE;


	/*
	 * Constantes de configuración del enemigo.
	 */
	private const float SOUND_RANGE = 15.0f; // Distancia de escucha.
	private const float VISION_RANGE = 10.0f; // Distancia de visión.
	private const float VISION_ANGLE = 40.0f; // Ángulo de visión.
	private const float ATTACK_RANGE = 4.0f; // Distancia de ataque.
	
	private const float LINEAR_FORCE = 100.0f; // Fuerza de movimiento lineal.
	private const float ANGULAR_FORCE = 50.0f; // Fuerza de movimiento angular.

	private const float MAX_LIN_VEL = 5.0f; // Velocidad lineal máxima.
	private const float MAX_ANG_VEL = 5.0f; // Velocidad angular máxima.

	private const float IDLE_TIME = 3.0f; // Tiempo de inactividad para la acción 
	                                      // del estado IDLE.

	private const float MAX_LIVE = 100.0f; // Vida máxima que puede tener. 

	/*
	 * Atributos internos.
	 */
	private Transform weapon01 = null; // Acceso al arma.
	private Transform target = null; // Acceso al jugador.
	private float timeForIdle = IDLE_TIME; // Contador de tiempo para pasar al estado IDLE.
	private float timeTargetLost = IDLE_TIME; // Contador de tiempo en el que no se ha visto al jugador.
	private float actualLive = MAX_LIVE; // Contador de vida del enemigo.

	private LineRenderer lineRenderer; // = gameObject.AddComponent<LineRenderer>();
	private LineRenderer lineRenderer2;

	public LayerMask rayCastMask;
	public bool showSensors = true;
	/*
	 * Funciones de Unity:
	 * 
	 */
	void Start () {
	   // Buscando acceso al objeto jugador.
	   GameObject playerObject = GameObject.Find ("Player");
	   target = playerObject.transform;
	   
	   // Buscando acceso al objeto arma del enemigo.
	   weapon01 = transform.FindChild ("Weapon01");

		lineRenderer = gameObject.AddComponent<LineRenderer>();

		GameObject newLine = new GameObject("Line");
	    lineRenderer2 = newLine.AddComponent<LineRenderer>();
		newLine.transform.parent = transform;

	}
	void Update () {
	   // Vector desde el enemigo al jugador
	   Vector3 towardsTarget = target.position - transform.position;

	   if (showSensors) {
	      drawCircle (SOUND_RANGE);
	      drawConeVis (VISION_RANGE, VISION_ANGLE);
	   } else {
		  lineRenderer.SetVertexCount(0);
		  lineRenderer2.SetVertexCount(0);
	   }

	   if (actualLive <= 0) {
	      autoDestruct();
	   }

	   if (timeTargetLost >= 0) {
	      timeTargetLost -= Time.deltaTime;
	   }
	   manageState (towardsTarget);
	   nextState (towardsTarget);
	}

	/*
	 * Funciones de acciones de los estados.
	 */
	void idleAction () {
       if (timeForIdle <= 0) {
	      // Frenar al enemigo lineal y angularmente.
	      if (rigidbody.velocity.magnitude > 0 || rigidbody.angularVelocity.magnitude > 0) {
	         rigidbody.velocity = Vector3.ClampMagnitude (rigidbody.velocity, 0);
	         rigidbody.angularVelocity = Vector3.ClampMagnitude (rigidbody.angularVelocity, 0);
	      }
	   } else {
	      timeForIdle -= Time.deltaTime;
	   }

	}
	void lookingAction (Vector3 towardsTarget) {
		Vector3 vectorTmp;
		float torque = ANGULAR_FORCE;
		float accelerate = LINEAR_FORCE * 0.4f;


		if (!isPointTarget ()) {
	       // Si no se ha visto al enemigo girar en la dirección en la que se le 
	       // escucha.
			if (isTargetInVisionField (towardsTarget)) {
				rigidbody.AddForce (towardsTarget.normalized * LINEAR_FORCE * Time.deltaTime);
				actualState = EnemyStates.PURSUIT_TRAGET;
				//torque *= 0.7f;
			} else {
				linearDecelerate ();
			}
		   vectorTmp = Vector3.Cross (transform.forward, towardsTarget.normalized);
	       rigidbody.AddTorque (transform.up * vectorTmp.y * torque * Time.deltaTime);
		   rigidbody.angularVelocity = Vector3.ClampMagnitude (rigidbody.angularVelocity, MAX_ANG_VEL);
		} else {
	       // Dispara y pasar al estado persecución.
		   angDecelerate ();
		   shootWeapon ();
		   actualState = EnemyStates.PURSUIT_TRAGET;
		}
	}
	void pursuitAction (Vector3 towardsTarget) {
	   // Perseguir al jugador y disparar si está a tiro.
	   Vector3 vectorTmp = Vector3.Cross (transform.forward, towardsTarget.normalized);
	   float torque = ANGULAR_FORCE * 0.4f;

	   rigidbody.AddForce (towardsTarget.normalized * LINEAR_FORCE * Time.deltaTime);
	   rigidbody.velocity = Vector3.ClampMagnitude (rigidbody.velocity, MAX_LIN_VEL);
	   if (isPointTarget ()) {
	      angDecelerate ();
	      shootWeapon ();
	   } else {
	      rigidbody.AddTorque (transform.up * vectorTmp.y * torque * Time.deltaTime);
	   }
	}
	void shootAction (Vector3 towardsTarget) {
	   // Si el jugador está a tiro disparar y apuntar al centro del mismo.
	   //  - Si el enemigo y el jugador están centrados, frenar la velocidad angular.
	   // En caso contrario, girar mas rápidamente hacia el jugador.
	   Vector3 vectorTmp = Vector3.Cross (transform.forward, towardsTarget.normalized);
	   float angleTmp = Vector3.Angle (transform.forward, towardsTarget.normalized);
	  

	   linearDecelerate ();

	   if (isPointTarget ()) {
	      shootWeapon ();
		  if (angleTmp < 0.5f) {
	         angDecelerate ();
	      } else {
	         rigidbody.AddTorque (transform.up * vectorTmp.y * Time.deltaTime);
	      }
	   } else {
	      rigidbody.AddTorque (transform.up * vectorTmp.y * ANGULAR_FORCE / 3 * Time.deltaTime);
	   }
	   rigidbody.angularVelocity = Vector3.ClampMagnitude (rigidbody.angularVelocity, MAX_ANG_VEL);
	}

	/*
	 * Funciones de manejo de estados.
	 */
	void manageState (Vector3 towardsTarget) {
	   // Enlazado de estados y funciones a ejecutar.
	   switch (actualState) {
	      case EnemyStates.IDLE:
	         idleAction ();
	         break;
	      case EnemyStates.LOOKING_TARGET:
	         lookingAction (towardsTarget);
	         break;
	      case EnemyStates.PURSUIT_TRAGET:
	         pursuitAction (towardsTarget);
	         break;
	      case EnemyStates.SHOOT_TARGET:
	         shootAction (towardsTarget);
	         break;
	   }
	}

	void nextState (Vector3 towardsTarget) {
	   const float RET_ERR = 1.0f; // Rango de error para la vuelta a otro estado.
	   // Control principal de las transiciones entre estados.
	   float angleTmp = Vector3.Angle (transform.forward, towardsTarget.normalized);

	   switch (actualState) {
	      case EnemyStates.IDLE:
		     if (towardsTarget.magnitude < SOUND_RANGE) {
				// Si escuchamos al jugador pasar al estado de busqueda.
	            actualState = EnemyStates.LOOKING_TARGET;
	         }
	         break;
	      case EnemyStates.LOOKING_TARGET:
	         if (towardsTarget.magnitude > SOUND_RANGE + RET_ERR) {
				// Si el jugador se sale del rango de escucha, pasar al estado
				// inactivo
	            actualState = EnemyStates.IDLE;
				timeForIdle = IDLE_TIME;
	         } else if ((towardsTarget.magnitude < ATTACK_RANGE)
			           && (angleTmp < VISION_ANGLE)) {
				// Si el jugador está a distancia de tiro y dentro del rango de 
				// vision, pasar al estado disparo.
				actualState = EnemyStates.SHOOT_TARGET;
	         }
	         break;
	      case EnemyStates.PURSUIT_TRAGET:
	         if ((timeTargetLost <= 0)
			    || (angleTmp > VISION_ANGLE + RET_ERR)) {
				// Si se ha perdido de vista al jugador un cierto tiempo y éste
				// se encuentra fuera del rango de visión, pasar al estado de
				// búsqueda.
	            actualState = EnemyStates.LOOKING_TARGET;
	         } else if (towardsTarget.magnitude < ATTACK_RANGE) {
				// Si el jugador se encuentra dentro del rango de ataque, pasar
				// al estado de disparo.
				actualState = EnemyStates.SHOOT_TARGET;
	         }
	         break;
	      case EnemyStates.SHOOT_TARGET:
	         if ((timeTargetLost <= 0)
			    || (angleTmp > VISION_ANGLE + RET_ERR)) {
	            actualState = EnemyStates.LOOKING_TARGET;
			 } else if (towardsTarget.magnitude > ATTACK_RANGE + RET_ERR) {
				// Si el jugador se sale del rango de ataque pasar al estado
				// persecución.
	            actualState = EnemyStates.PURSUIT_TRAGET;
	         }
	         break;
		}
	}

	/*
	 * Funciones internas
	 */
	void drawCircle (float radio) {
		float theta_scale = 0.1f;             //Set lower to add more points
		int size = (int)((2.0f * Mathf.PI) / theta_scale); //Total number of points in circle.

		lineRenderer.material = new Material(Shader.Find("Particles/Additive"));
		lineRenderer.SetColors(Color.red, Color.red);
		lineRenderer.SetWidth(0.1f, 0.1f);
		lineRenderer.SetVertexCount(size + 1);
		
		int i = 0;
		float x, z;
		for(float theta = 0; theta < (2.0f * Mathf.PI); theta += theta_scale) {
			x = radio * Mathf.Cos(theta);
			z = radio * Mathf.Sin(theta);
			
			Vector3 pos = new Vector3(x, 0.5f, z);
			pos += transform.position;
			lineRenderer.SetPosition(i, pos);
			i+=1;
		}
	}
	void drawConeVis (float radio, float angle) {
		float enemyFrontAngle = Vector3.Angle (transform.forward, new Vector3 (1, 0, 0));
		Vector3 vectTmp = Vector3.Cross (transform.forward, new Vector3 (1, 0, 0));
		enemyFrontAngle *= Mathf.Deg2Rad;
		//Debug.Log (enemyFrontAngle);
		if (vectTmp.y < 0) {
			enemyFrontAngle = (360.0f * Mathf.Deg2Rad) - enemyFrontAngle;
		}
		angle *= Mathf.Deg2Rad;

		float theta_scale = 0.1f;             //Set lower to add more points
		int size = (int)((2.0f * angle) / theta_scale); //Total number of points in circle.

		lineRenderer2.material = new Material(Shader.Find("Particles/Additive"));
		lineRenderer2.SetColors(Color.blue, Color.blue);
		lineRenderer2.SetWidth(0.1F, 0.1F);
		lineRenderer2.SetVertexCount(size + 3);
		
		int i = 0;
		float x, z;
		Vector3 pos = new Vector3(transform.position.x, transform.position.y, transform.position.z);
		lineRenderer2.SetPosition(i, pos);
		i+=1;
		
		for(float theta = enemyFrontAngle - angle; theta < (enemyFrontAngle + angle); theta += theta_scale) {
			x = radio * Mathf.Cos(theta);
			z = radio * Mathf.Sin(theta);
			
			pos = new Vector3(x, 0.5f, z);
			pos += transform.position;
			lineRenderer2.SetPosition(i, pos);
			i+=1;
		}
		pos = new Vector3(transform.position.x, transform.position.y, transform.position.z);
		lineRenderer2.SetPosition(i, pos);
		i+=1;

	}
	void linearDecelerate () {
		float vectorMag = rigidbody.velocity.magnitude;
		if (vectorMag < 1.0f) {
			vectorMag = 0;
		} else {
			vectorMag *= 0.5f;
		}
		rigidbody.velocity = Vector3.ClampMagnitude(rigidbody.velocity, vectorMag);
	}
	void angDecelerate () {
		float vectorMag = rigidbody.angularVelocity.magnitude;

		if (vectorMag < 1.0f) {
			vectorMag = 0;
		} else {
			vectorMag *= 0.5f;
		}
		rigidbody.angularVelocity = Vector3.ClampMagnitude(rigidbody.angularVelocity
		                                                   , vectorMag); 
	}
	void shootWeapon () {
	   weapon01.SendMessage ("Attack");
	}
	bool isPointTarget () {
	   bool result = false;
	   Ray thePlayerRay = new Ray (this.transform.position, this.transform.forward);
	   RaycastHit thePlayerHit;
	   
	   if (Physics.Raycast(thePlayerRay, out thePlayerHit, VISION_RANGE)) {
		  if (thePlayerHit.transform.name == "Player") {
		     timeTargetLost = IDLE_TIME;
	         result = true;
	      }
	   }
	   return result;
	}
	bool isTargetInVisionField (Vector3 towardsTarget) {
		bool result = false;
		Ray thePlayerRay = new Ray (this.transform.position, towardsTarget.normalized);
		RaycastHit thePlayerHit;

		if (Physics.Raycast(thePlayerRay, out thePlayerHit, VISION_RANGE)) {
			if (thePlayerHit.transform.name == "Player") {
				timeTargetLost = IDLE_TIME;
				result = true;
			}
		}
		return result;
	}
	void hitEnemy () {
	   actualLive -= 10;
	   Debug.Log ("Enemy live: " + actualLive);
	}
	void autoDestruct() {	
		Destroy (gameObject);
	}
}
