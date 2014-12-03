using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class PointJoint : MonoBehaviour {
	
	[Range(-360f, 360f)]
	public float minAngle = 0f;
	[Range(-360f, 360f)]
	public float maxAngle = 0f;
	public bool visible = true;

	private HingeJoint2D joint;
	
	void Awake() {
		RaycastHit2D hit1 = Physics2D.RaycastAll(transform.position, Vector2.zero).FirstOrDefault(x => x.collider.gameObject.rigidbody2D != null);
		RaycastHit2D hit2 = Physics2D.RaycastAll(transform.position, Vector2.zero).FirstOrDefault(x => x.collider.gameObject.rigidbody2D != null && x.collider != hit1.collider);

		if ((hit1.collider != null && hit1.collider.gameObject.rigidbody2D == null) ||
			(hit2.collider != null && hit2.collider.gameObject.rigidbody2D == null)) {

			Debug.LogWarning("Point joint " + gameObject + " doesn't connected! Hit1:" + hit1.collider + ", hit2:" + hit2.collider, gameObject);
			return;
		}		
		
		Rigidbody2D rb1 = hit1.collider.gameObject.rigidbody2D;
		Rigidbody2D rb2 = hit2.collider.gameObject.rigidbody2D;
		
		float realMinAngle = Mathf.Min(minAngle, maxAngle);
		float realMaxAngle = Mathf.Max(minAngle, maxAngle);

		joint = rb1.gameObject.AddComponent<HingeJoint2D>();
		joint.connectedBody = rb2;
		joint.anchor = rb1.transform.InverseTransformPoint(transform.position);
		joint.connectedAnchor = rb2.transform.InverseTransformPoint(transform.position);
		joint.useLimits = true;
		joint.limits = new JointAngleLimits2D(){min = -realMaxAngle + rb1.rotation, max = -realMinAngle + rb1.rotation};

		if (!visible)
			Destroy(gameObject);
	}

	void Update() {
		transform.position = joint.transform.TransformPoint(joint.anchor);
	}

	void OnDrawGizmos() {
		int segs = 30;
		float radius = 0.5f;
		
		float correctedMinAngle = minAngle - 180f;
		float correctedMaxAngle = maxAngle - 180f;

		Vector2 lastPoint = Vector2.zero;
		for (int i = 0; i < segs; i++) {
			float cf = (float)i/(float)(segs - 1);

			float an = (correctedMinAngle + (correctedMaxAngle - correctedMinAngle)*cf)*Mathf.Deg2Rad;
			Vector2 point = new Vector2(Mathf.Cos(an), Mathf.Sin(an))*radius + (Vector2)transform.position;

			if (i == 0 || i ==  segs - 1)
				Gizmos.DrawLine(point, transform.position);

			if (i > 0)
				Gizmos.DrawLine(lastPoint, point);

			lastPoint = point;
		}
	}
}
