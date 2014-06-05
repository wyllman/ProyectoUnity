using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyCreator : MonoBehaviour {
	// Prefab del objeto enemigo
	public GameObject enemyPrefab;

	// Definir las posiciones de generación para
	// los enemigos.
	private Vector3 guardPosB01 = new Vector3 (5, 0, 5);
	private Vector3 guardPosB02 = new Vector3 (-5, 0, 5);

	private Vector3 guardPosF01 = new Vector3 (10, 0, 10);
	private Vector3 guardPosF02 = new Vector3 (0, 0, 10);
	private Vector3 guardPosF03 = new Vector3 (-10, 0, 10);

	// Referencias a los enemigos generados actualmente.
	private const int NUM_ENEMY = 1;
	private List<GameObject> guardEnemyB01 = new List<GameObject> ();
	private List<GameObject> guardEnemyB02 = new List<GameObject> ();

	private List<GameObject> guardEnemyF01 = new List<GameObject> ();
	private List<GameObject> guardEnemyF02 = new List<GameObject> ();
	private List<GameObject> guardEnemyF03 = new List<GameObject> ();

	// Contadores de tiempo de espera para una nueva
	// generación de enemigos.
	private const float IDLE_TIME = 3.0f;
	private float timeIdleB01 = IDLE_TIME;
	private float timeIdleB02 = IDLE_TIME;

	private float timeIdleF01 = IDLE_TIME;
	private float timeIdleF02 = IDLE_TIME;
	private float timeIdleF03 = IDLE_TIME;
	// Use this for initialization
	void Start () {
	
	}

	// Update is called once per frame
	void Update () {
		checkEnemiesPos (ref guardEnemyB01, guardPosB01, ref timeIdleB01);
		checkEnemiesPos (ref guardEnemyB02, guardPosB02, ref timeIdleB02);

		checkEnemiesPos (ref guardEnemyF01, guardPosF01, ref timeIdleF01);
		checkEnemiesPos (ref guardEnemyF02, guardPosF02, ref timeIdleF02);
		checkEnemiesPos (ref guardEnemyF03, guardPosF03, ref timeIdleF03);
	}

	void checkEnemiesPos (ref List<GameObject> enemyObj, Vector3 generatePos, ref float timeIdle) {
		if (enemyObj.Count < NUM_ENEMY) {
		  if (timeIdle > 0) {
			timeIdle -= Time.deltaTime;
		  } else {
			timeIdle = IDLE_TIME;
			enemyObj.Add( GameObject.Instantiate (
				enemyPrefab,
				generatePos,
				Quaternion.identity) as GameObject);
			
		  }
		}
		
	}
}
