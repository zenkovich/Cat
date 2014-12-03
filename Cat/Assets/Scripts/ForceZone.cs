using UnityEngine;
using System.Collections;

public class ForceZone : MonoBehaviour {

	public float force = 10f;

	void OnTriggerStay2D(Collider2D other) {
		if (other.rigidbody2D != null)
			other.rigidbody2D.AddForce(transform.right*force*Time.fixedDeltaTime);
	}
}
