using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class Spring : MonoBehaviour {

	public float frequency = 1f;
	public float damping = 0f;
	public bool isSlider;
	public float sliderLower;
	public float sliderUpper;

	private SpringJoint2D joint = null;
	private float initLength;
	private bool connected;

	void Start() {
		initLength = GetComponent<SpriteRenderer>().sprite.bounds.size.y;

		Vector2 worldAnchor1 = transform.TransformPoint(Vector2.zero);
		Vector2 worldAnchor2 = transform.TransformPoint(Vector2.up*-initLength);

		RaycastHit2D hit1 = Physics2D.RaycastAll(worldAnchor1, Vector2.zero).FirstOrDefault(x => x.collider.gameObject.rigidbody2D != null);
		RaycastHit2D hit2 = Physics2D.RaycastAll(worldAnchor2, Vector2.zero).FirstOrDefault(x => x.collider.gameObject.rigidbody2D != null);

		if (hit1.collider == null || hit1.collider.gameObject.rigidbody2D == null ||
			hit2.collider == null || hit2.collider.gameObject.rigidbody2D == null) {

			Debug.LogWarning("Spring " + gameObject + " doesn't connected! Hit1:" + hit1.collider + ", hit2:" + hit2.collider, gameObject);
			return;
		}
		
		Rigidbody2D rb1 = hit1.collider.gameObject.rigidbody2D;
		Rigidbody2D rb2 = hit2.collider.gameObject.rigidbody2D;

		joint = rb1.gameObject.AddComponent<SpringJoint2D>();
		joint.connectedBody = rb2;
		joint.anchor = rb1.transform.InverseTransformPoint(worldAnchor1);
		joint.connectedAnchor = rb2.transform.InverseTransformPoint(worldAnchor2);
		joint.frequency = frequency;
		joint.dampingRatio = damping;
		joint.distance = (worldAnchor2 - worldAnchor1).magnitude;

		if (isSlider) {
			SliderJoint2D slJoint = rb1.gameObject.AddComponent<SliderJoint2D>();
			slJoint.connectedBody = rb2;
			slJoint.anchor = rb1.transform.InverseTransformPoint(worldAnchor1);
			slJoint.connectedAnchor = rb2.transform.InverseTransformPoint(worldAnchor2);
			slJoint.useLimits = true;
			slJoint.limits = new JointTranslationLimits2D(){min = joint.distance - sliderLower, max = joint.distance + sliderUpper};
			slJoint.angle = Utils.AngleSigned(worldAnchor2 - worldAnchor1, Vector2.right)*Mathf.Rad2Deg - rb1.rotation;
		}

		connected = true;
	}

	void Update() {
		if (!connected)
			return;

		Vector2 worldAnchor1 = joint.rigidbody2D.transform.TransformPoint(joint.anchor);
		Vector2 worldAnchor2 = joint.connectedBody.transform.TransformPoint(joint.connectedAnchor);

		Utils.SetTransformToLine(transform, initLength, worldAnchor1, worldAnchor2);
	}
}
