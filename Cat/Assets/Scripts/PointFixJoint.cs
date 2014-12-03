using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class PointFixJoint : MonoBehaviour {
		
	public bool visible = true;
	public bool fixAngle = true;

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
		
		joint = rb1.gameObject.AddComponent<HingeJoint2D>();
		joint.connectedBody = rb2;
		joint.anchor = rb1.transform.InverseTransformPoint(transform.position);
		joint.connectedAnchor = rb2.transform.InverseTransformPoint(transform.position);
		joint.useLimits = fixAngle;
		if (fixAngle)
			joint.limits = new JointAngleLimits2D(){min = 0, max = 0};

		if (!visible)
			Destroy(gameObject);
	}

	void Update() {
		transform.position = joint.transform.TransformPoint(joint.anchor);
	}
}
