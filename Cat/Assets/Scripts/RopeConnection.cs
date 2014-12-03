using UnityEngine;
using System.Collections;
using UnityEngine.UI;
//using UnityEditor;

public class RopeConnection : MonoBehaviour {

	public Rope ropePrefab;
	public float lowerLengthSpeed = 1f;
	public bool attached;
	public float connectTimer = 2f;
	public float stressForceThreshold = 0.2f;
	public float straightTimer = 1f;
	public GameObject lightObj;
	
	private Rope attachedRope;
	private SliderJoint2D joint;
	private PlayerRopesControl batiscaff;
	
	private Rope.RigidBodyAttachement connectionAttachement;
	private Rope.RigidBodyAttachement batiscaffAttachement;

	private float curStraightTimer = 1f;
	private Button button;


	public Rope AttachedRope { get { return attachedRope; } }


	public bool Lightning { set { lightObj.SetActive(value); } }

	void Awake() {
		joint = GetComponent<SliderJoint2D>();
		joint.enabled = false;
	}

	public void Throw(PlayerRopesControl batiscaff, Vector2 direction, float velocity, Button btn) {
		button = btn;

		this.batiscaff = batiscaff;

		direction = direction.normalized;
		Vector2 batiscaffAttachingPointLocal = Quaternion.Inverse( batiscaff.transform.rotation ) *direction*0.3f;
		Vector2 batiscaffAttachingPointWorld = batiscaff.transform.position + batiscaff.transform.rotation*(Vector3)batiscaffAttachingPointLocal;

		rigidbody2D.velocity = direction*velocity;
		rigidbody2D.rotation = Vector2.Angle(direction, Vector2.up);

		attachedRope = (Instantiate(ropePrefab.gameObject, batiscaffAttachingPointWorld, Quaternion.identity) as GameObject).GetComponent<Rope>();
		attachedRope.Length = 2f;
		
		batiscaffAttachement = attachedRope.AttachNode(0, batiscaffAttachingPointLocal, batiscaff.rigidbody2D, 
			                                           batiscaff.ropesSets.connectionImpCoef1, batiscaff.ropesSets.connectionPosCoef1);

		connectionAttachement = attachedRope.AttachNode(-1, Vector2.up*(-0.2f), rigidbody2D, 
			                                            batiscaff.ropesSets.thisImpCoef1, batiscaff.ropesSets.thisPosCoef1);

		curStraightTimer = straightTimer;

		GetComponent<SpriteRenderer>().sprite = btn.GetComponent<Image>().sprite;
		btn.gameObject.SetActive(true);
		btn.onClick.RemoveAllListeners();
		btn.onClick.AddListener(Destroy);
	}

	public void Destroy() {
		button.gameObject.SetActive(false);
		batiscaff.ropes.Remove(this);
		Destroy(gameObject);
		Destroy(attachedRope.gameObject);
		batiscaff.RopeCutted();
	}

	void Update() {

		if (!attached) {
			float lineLength = (attachedRope.StartPoint - attachedRope.EndPoint).magnitude;
			attachedRope.Length = Mathf.Max( Mathf.Min( lineLength*2.5f, batiscaff.maxRopeLength), attachedRope.Length);

			if (lineLength > batiscaff.ropesSets.ropeFixinglength) {
				connectionAttachement.impulseCoef = batiscaff.ropesSets.connectionImpCoef2;
				connectionAttachement.positionCoef = batiscaff.ropesSets.connectionPosCoef2;
			}

			if (attachedRope.StressForce > stressForceThreshold)
				rigidbody2D.velocity *= 0.6f;

			connectTimer -= Time.deltaTime;

			if (connectTimer < 0) 
				Destroy();
		}
		else {
			if (curStraightTimer > 0f) {
				curStraightTimer -= Time.deltaTime;
				float cf = 1f - Mathf.Clamp01(curStraightTimer/straightTimer);				
			
				batiscaffAttachement.impulseCoef = Mathf.Lerp(batiscaff.ropesSets.thisImpCoef1, batiscaff.ropesSets.thisImpCoef2, cf);
				batiscaffAttachement.positionCoef = Mathf.Lerp(batiscaff.ropesSets.thisPosCoef1, batiscaff.ropesSets.thisPosCoef2, cf);
			}
		}
	}

	void OnTriggerEnter2D(Collider2D coll) {

		if (attached)
			return;

		if (coll.gameObject.layer == LayerMask.NameToLayer("LevelGeometry")) {
			attached = true;
						
			connectionAttachement.impulseCoef = batiscaff.ropesSets.connectionImpCoef3;
			connectionAttachement.positionCoef = batiscaff.ropesSets.connectionPosCoef3;

			joint.enabled = true;
			if (coll.rigidbody2D != null) {
				joint.connectedBody = coll.rigidbody2D;
				joint.connectedAnchor = joint.connectedBody.transform.InverseTransformPoint(transform.position);
				coll.rigidbody2D.gameObject.SendMessage("OnRopeConnected", this, SendMessageOptions.DontRequireReceiver);
			}
			else {
				joint.connectedAnchor = transform.position;
			}

			/*if (batiscaff.pauseWhenConnect)
				EditorApplication.isPaused = true;*/
		}
	}
}
