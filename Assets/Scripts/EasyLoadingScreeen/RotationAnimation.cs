using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationAnimation : MonoBehaviour {

	private Vector3 initialOrientation;
	private float rotationalAcceleration;

	void Start () {
		float totalDegrees = 360f;
		initialOrientation = new Vector3 (
			Random.value * totalDegrees,
			Random.value * totalDegrees,
			Random.value * totalDegrees
		);
		initialOrientation.Normalize ();
		rotationalAcceleration = 100;
		SetRandomPosition (-10f, 10f);
		gameObject.GetComponent<Renderer> ().material.color = new Color (Random.value, Random.value, Random.value);
	}

	// Update is called once per frame
	void Update () {
		// Rotate the transform
		float speed = Time.deltaTime * rotationalAcceleration;
		transform.Rotate (initialOrientation * speed);
	}

	/**
	 * Assign a random position to a game object within a given range [min, max].
	 */
	void SetRandomPosition (float min, float max) {
		Vector3 randomPosition = new Vector3 (Random.Range (min, max), 0.5f, Random.Range (min, max));
		transform.position = randomPosition;
	}
}