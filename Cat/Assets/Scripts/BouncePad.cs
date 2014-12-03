using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class BouncePad : MonoBehaviour {

	public float impulse = 100f;

	void OnCollisionEnter2D(Collision2D coll) {
		Vector2 midNormal = Vector2.zero;
		coll.contacts.ToList().ForEach(x => midNormal += x.normal);
		midNormal /= (float)coll.contacts.Count();
		coll.rigidbody.AddForce(midNormal*-impulse);
	}
}
