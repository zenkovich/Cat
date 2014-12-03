using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class GridBlock : GridSceneObject {

	public bool isStatic;
	public List<Vector2> anchorPoints = new List<Vector2>();
	
	void Awake() {
		if (Application.isPlaying) {
			if (!isStatic) {
				if (gameObject.GetComponent<Rigidbody2D>() == null)
					gameObject.AddComponent(typeof(Rigidbody2D));
			}
		}
	}

	void Start() {
		if (Application.isPlaying) {
			if (!isStatic) {
				anchorPoints.ForEach(x => SearchConnection(x));
			}
		}
	}

	void SearchConnection(Vector2 point) {
		Vector2 worldPoint = transform.TransformPoint(point);

		HingeJoint2D joint = gameObject.AddComponent<HingeJoint2D>();
		joint.anchor = point;
		joint.useLimits = true;
		joint.limits = new JointAngleLimits2D(){ min = 0, max = 0 };

		RaycastHit2D[] hits = Physics2D.RaycastAll(worldPoint, Vector2.zero);

		foreach (var hit in hits) {
			if (hit.collider.gameObject.rigidbody2D != null && hit.collider.gameObject != gameObject) {				
				joint.connectedBody = hit.collider.gameObject.rigidbody2D;
				joint.connectedAnchor = hit.transform.InverseTransformPoint(worldPoint);
				return;
			}
		}

		joint.connectedAnchor = worldPoint;
		joint.collideConnected = false;
	}

	void OnDrawGizmos() {
		if (isStatic)
			return;

		Vector3 h = Vector3.right*0.1f;
		Vector3 v = Vector3.up*0.1f;

		foreach (var ap in anchorPoints) {
			Vector3 screenPoint = transform.TransformPoint(ap);
			Gizmos.DrawLine(screenPoint - h, screenPoint + h);
			Gizmos.DrawLine(screenPoint - v, screenPoint + v);
		}
	}
}
