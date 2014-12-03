using UnityEngine;
using System.Collections;

public class ParallaxMovement : MonoBehaviour {

	public Vector3 scale;
	public Transform origin;
	
	private Vector3 lastOrigPos;

	void Awake() {
		lastOrigPos = origin.position;
	}

	void Update () {
		Vector3 delta = origin.position - lastOrigPos;
		transform.position += Vector3.Scale( delta, scale );
		lastOrigPos = origin.position;
	}
}
