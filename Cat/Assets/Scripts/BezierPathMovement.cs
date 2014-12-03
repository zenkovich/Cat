using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BezierPath))]
public class BezierPathMovement : MonoBehaviour {

	public enum RepeatType { Once, PingPong };

	public float speed = 1f;
	public RepeatType repeatType = RepeatType.PingPong;
	public float currentPos;
	public AnimationCurve curve;
	public bool moving = true;
	private BezierPath bezierPath;

	void Awake() {
		bezierPath = GetComponent<BezierPath>();
		rigidbody2D.fixedAngle = true;
	}

	void FixedUpdate() {

		if (moving) {
			if (speed > 0) {
				currentPos += speed*Time.deltaTime;
				if (repeatType == RepeatType.PingPong && currentPos > bezierPath.length)
					speed = -speed;
			}
			else {
				currentPos += speed*Time.deltaTime;
				if (repeatType == RepeatType.PingPong && currentPos < 0)
					speed = -speed;
			}
		}

		float correctedPos = curve.Evaluate(currentPos/bezierPath.length)*bezierPath.length;
		rigidbody2D.velocity = (bezierPath.GetPointOnPath( correctedPos ) - rigidbody2D.position)/Time.fixedDeltaTime;
	}

	void OnSwitchMessage(string message) {
		if (message == "start") {
			moving = true;
			enabled = true;
		}
		else if (message == "stop") {
			moving = false;
			enabled = true;
		}
		else if (message == "off") {
			enabled = false;
		}
	}
}
