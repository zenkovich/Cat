using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;


public class Rope : MonoBehaviour {
	
	[Serializable]
	public class RigidBodyAttachement {
		public Rigidbody2D rigidBody;
		public int nodeIdx;
		public Vector2 attachingAnchor;
		public float impulseCoef = 4f;
		public float positionCoef = 0.2f;
		public Action resolveFunc;

		public void Resolve(List<VerletNode> nodes) {
			int idx = nodeIdx < 0 ? nodes.Count - 1:nodeIdx;

			if (rigidBody == null)
				nodes[idx].position = attachingAnchor;
			else 
				nodes[idx].ResolveBodyAttach(rigidBody, attachingAnchor, impulseCoef, positionCoef);
		}
	}

	public List<VerletNode> nodes;
	public List<VerletNodeLink> links;
	
	public float initialLength;
	public float segmentLength;

	public List<RigidBodyAttachement> attachements;
	
	public Action onNodesChanged;

	public float stressForce;
	private float length;


	public float Length {
		set {
			length = Mathf.Max(value, segmentLength);

			int newLinksCount = Mathf.CeilToInt(length/segmentLength);

			int linksCount = links.Count;
			int linksDiff = linksCount - newLinksCount;
			if (linksDiff > 0) //remove links
			{ 
				for (int i = 0; i < linksDiff; i++) {
					int remidx = linksCount - 1 - i;
					VerletNodeLink remLink = links[remidx];
					nodes.Remove(remLink.node2);
					links.RemoveAt(remidx);

					VerletPhysics.RemoveNode(remLink.node2);
					VerletPhysics.RemoveLink(remLink, false);
				}
			}
			else //add links
			{ 
				for (int i = 0; i < -linksDiff; i++) {

					if (nodes.Count > 0) {
						VerletNode newNode = VerletPhysics.CreateNode(nodes.Last().position + Vector2.up*0.000001f);
						VerletNodeLink newLink = VerletPhysics.CreateLink(nodes.Last(), newNode, segmentLength);
						nodes.Add(newNode);
						links.Add(newLink);
					}
					else nodes.Add(VerletPhysics.CreateNode(transform.localPosition));
				}
			}

			links.ForEach(x => x.distance = segmentLength);

			//correct length
			links.Last().distance = length - segmentLength*(newLinksCount - 1);

			if (onNodesChanged != null)
				onNodesChanged();
		}
		get { return length; }
	}
	
	public Vector2 StartPoint { get { return nodes.First().position; } }
	public Vector2 EndPoint { get { return nodes.Last().position; } }
	public float StressForce {
		get {
			return links.Max(x => (x.node2.position - x.node1.position).magnitude - x.distance);
		}
	}

	public RigidBodyAttachement AttachNode(int nodeIdx, Vector2 anchor, Rigidbody2D body = null, float impCoef = 4f, float posCoef = 0.2f) {
		RigidBodyAttachement attachement = new RigidBodyAttachement(){ nodeIdx = nodeIdx, attachingAnchor = anchor, rigidBody = body,
			                                                           impulseCoef = impCoef, positionCoef = posCoef };
		attachements.Add(attachement);
		attachement.resolveFunc = () => { attachement.Resolve(nodes); };
		VerletPhysics.instance.addSolves.Add(attachement.resolveFunc);

		return attachement;
	}

	public void DeattachNode(int nodeIdx) {
		VerletPhysics.instance.addSolves.Remove(attachements.Find(x => x.nodeIdx == nodeIdx).resolveFunc);
		attachements.RemoveAll(x => x.nodeIdx == nodeIdx);
	}

	bool FasterLineSegmentIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4) {   
		Vector2 a = p2 - p1;
		Vector2 b = p3 - p4;
		Vector2 c = p1 - p3;
   
		float alphaNumerator = b.y*c.x - b.x*c.y;
		float alphaDenominator = a.y*b.x - a.x*b.y;
		float betaNumerator  = a.x*c.y - a.y*c.x;
		float betaDenominator  = a.y*b.x - a.x*b.y;
   
		bool doIntersect = true;
   
		if (alphaDenominator == 0 || betaDenominator == 0) {
			doIntersect = false;
		} else {
       
			if (alphaDenominator > 0) {
				if (alphaNumerator < 0 || alphaNumerator > alphaDenominator) {
					doIntersect = false;
               
				}
			} else if (alphaNumerator > 0 || alphaNumerator < alphaDenominator) {
				doIntersect = false;
			}
       
			if (betaDenominator > 0) {
				if (betaNumerator < 0 || betaNumerator > betaDenominator) {
					doIntersect = false;
				}
			} else if (betaNumerator > 0 || betaNumerator < betaDenominator) {
				doIntersect = false;
			}
		}
 
		return doIntersect;
	}

	public bool IsIntersects(Vector2 a, Vector2 b) {
		foreach(var lnk in links) {
			if (FasterLineSegmentIntersection(a, b, lnk.node1.position, lnk.node2.position))
				return true;
		}

		return false;
	}

	void Awake() {
		Length = initialLength;
	}

	void FixedUpdate() {
		//attachements.ForEach(x => x.Resolve(nodes));
		stressForce = StressForce;
	}

	void OnDestroy() {
		links.ForEach(x => VerletPhysics.RemoveLink(x, false));
		nodes.ForEach(x => VerletPhysics.RemoveNode(x));
		attachements.ForEach(x => VerletPhysics.instance.addSolves.Remove(x.resolveFunc));
	}

	void OnDrawGizmos() {
		Vector2 v = Vector2.up*0.05f;
		Vector2 h = Vector2.right*0.05f;

		links.ForEach(x => Debug.DrawLine(x.node1.position, x.node2.position, Color.green));
		nodes.ForEach(x => { Debug.DrawLine(x.position - v, x.position + v, Color.red); Debug.DrawLine(x.position - h, x.position + h, Color.red); });
	}
}
