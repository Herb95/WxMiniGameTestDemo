using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
	public GameObject player;
	private Vector3 viewingDirection;

	// Use this for initialization
	void Start () {
		viewingDirection = transform.position - player.transform.position;
	}
	
	// Update is called once per frame after all transformation calculations
	void LateUpdate () {
		transform.position = player.transform.position + viewingDirection;
	}
}
