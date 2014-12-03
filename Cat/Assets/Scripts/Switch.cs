using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class Switch : MonoBehaviour {

	[Serializable]
	public class HandlePos {
		public float position;
		public string message;
		public SpringJoint2D joint;
	}

	public GameObject targetMessageObject;
	public GameObject handleObj;
	public List<HandlePos> handlePositions = new List<HandlePos>();
	private int lastSpwitchPos = -1;

	void Awake() {
		Rigidbody2D rb = handleObj.AddComponent<Rigidbody2D>();

		HingeJoint2D pivotJoint = handleObj.AddComponent<HingeJoint2D>();
		pivotJoint.anchor = Vector2.zero;
		pivotJoint.connectedAnchor = rb.transform.position;
		pivotJoint.useLimits = false;
		pivotJoint.collideConnected = false;

		float joindDist = 1f;
		float springForce = 4f;
		float springDamp = 2f;
		foreach (var hp in handlePositions) {
			hp.joint = handleObj.AddComponent<SpringJoint2D>();
			Vector2 anchorWorld = (Vector2)rb.transform.position + ((Vector2)rb.transform.up*joindDist).RotateDeg(hp.position);
			hp.joint.anchor = Vector2.up/-rb.transform.localScale.y*joindDist;
			hp.joint.connectedAnchor = anchorWorld;
			hp.joint.frequency = springForce;
			hp.joint.dampingRatio = springDamp;
			hp.joint.distance = 0;
			hp.joint.enabled = false;
		}
	}

	void OnDrawGizmos() {
		float joindDist = 1f;
		foreach (var hp in handlePositions) {
			Vector2 anchorWorld = (Vector2)handleObj.transform.position + ((Vector2)handleObj.transform.up*joindDist).RotateDeg(hp.position);
			Gizmos.DrawLine(handleObj.transform.position, anchorWorld);
		}
	}

	void FixedUpdate() {
		int nearestPos = 0;
		float minDist = float.MaxValue;
		for (int i = 0; i < handlePositions.Count; i++) {
			float dst = ((Vector2)handleObj.transform.TransformPoint( handlePositions[i].joint.anchor ) - handlePositions[i].joint.connectedAnchor).magnitude;
			if (dst < minDist) {
				minDist = dst;
				nearestPos = i;
			}
		}

		for (int i = 0; i < handlePositions.Count; i++) {
			handlePositions[i].joint.enabled = i == nearestPos;
		}

		if (nearestPos != lastSpwitchPos) {
			lastSpwitchPos = nearestPos;
			targetMessageObject.SendMessage("OnSwitchMessage", handlePositions[nearestPos].message);
		}
	}
}
