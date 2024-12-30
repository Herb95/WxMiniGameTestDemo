using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectableManager : MonoBehaviour {

	public GameObject prefab;
	public int collectableCount;

	// Use this for initialization
	void Start () {
		for (int k = 0; k < collectableCount; k++) {
			Instantiate(prefab, GetRandomPosition(-10f, 10f), Quaternion.identity);
		}
	}
	
	// Update is called once per frame
	void Update () {
	}

	/**
	 * Assign a random position to a game object within a given range [min, max].
	 */ 
	void SetRandomPosition (GameObject other, float min, float max) {
		Vector3 randomPosition = new Vector3 (Random.Range (min, max), 0.5f, Random.Range (min, max)); 
		other.transform.position = randomPosition;
	}

	Vector3 GetRandomPosition (float min, float max) {
		Vector3 randomPosition = new Vector3 (Random.Range (min, max), 0.5f, Random.Range (min, max)); 
		return randomPosition;
	}
}
