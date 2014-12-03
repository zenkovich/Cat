using UnityEngine;
using System.Collections;

public class BatiscaffViewController : MonoBehaviour {

	public Transform insideCircle;
	public float insideCircleMass;
	public float insideCircleDamping;

	private float insideCircleInertia;
	private float insideCircleAngVel;

	void Awake() {
		insideCircleInertia = insideCircleMass;
	}

	float vCross(Vector2 a, Vector2 b) {
		return a.x * b.y - a.y * b.x;;
	}

	void FixedUpdate () {
		Vector2 gravNorm = Vector2.up*(-1f);
		Vector2 downVector = insideCircle.TransformDirection(gravNorm);
		float impulse = insideCircleMass;
		insideCircleAngVel -= vCross(gravNorm, downVector)*impulse/insideCircleInertia;
		insideCircleAngVel *= 1f - insideCircleDamping;
		insideCircle.transform.eulerAngles += Vector3.forward*insideCircleAngVel;
	}
}
