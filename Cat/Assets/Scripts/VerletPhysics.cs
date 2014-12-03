using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class VerletNode {
	public Vector2 position;
	public Vector2 prevPosition;

	public VerletNode(Vector2 position) {
		this.position = position;
		prevPosition = position;
	}

	public void Update(Vector2 gravity, float damping) {
		Vector2 delta = position - prevPosition;
		prevPosition = position;
		position += delta*(1f - Time.fixedDeltaTime*damping) + gravity*Time.fixedDeltaTime*Time.fixedDeltaTime;
	}

	public void StopForcible() {
		prevPosition = position;
	}

	public void ResolveBodyAttach(Rigidbody2D body, Vector2 relPoint, float impCoef, float posCoef) {
		Vector2 worldAttachPoint = body.position + (Vector2)(body.transform.localRotation*(Vector3)relPoint);
		Vector2 diff = worldAttachPoint - position;
		body.AddForceAtPosition(-diff*body.mass*impCoef, worldAttachPoint, ForceMode2D.Impulse);
		body.position -= diff*posCoef;
		worldAttachPoint = body.position + (Vector2)(body.transform.localRotation*(Vector3)relPoint);
		position = worldAttachPoint;
	}
}

[Serializable]
public class VerletNodeLink {
	public VerletNode node1;
	public VerletNode node2;
	public float distance;

	public void Resolve() {
		Vector2 delta = node2.position - node1.position;
		float deltalength = delta.magnitude;
		if (Mathf.Approximately(deltalength, 0f))
			deltalength = 0.001f;
		float diff = (deltalength - distance)/deltalength;
		node1.position += delta*0.5f*diff;
		node2.position -= delta*0.5f*diff;
	}
}

public class VerletPhysics : MonoBehaviour {
	
	public List<VerletNode> nodes;
	public List<VerletNodeLink> links;
	public List<Action> addSolves = new List<Action>();
	public int iterations = 8;
	public Vector2 gravity = new Vector2(0, -9.8f);
	public float globalDamping = 0.5f;
	
	private List<VerletNode> nodesPool = new List<VerletNode>();
	private List<VerletNodeLink> linksPool = new List<VerletNodeLink>();
	private int nodesPoolChunkSize = 50;

	static public VerletPhysics instance;


	void Awake() {
		instance = this;
		CreatePoolNodes(nodesPoolChunkSize);
		CreatePoolLinks(nodesPoolChunkSize);
	}

	void FixedUpdate() {
		nodes.ForEach(x => x.Update(gravity, globalDamping));

		for (int i = 0; i < iterations; i++) {
			links.ForEach(x => x.Resolve());
			addSolves.ForEach(x => x());
		}
	}

	void CreatePoolNodes(int count) {
		for (int i = 0; i < count ; i++) 
			nodesPool.Add(new VerletNode(Vector2.zero));
	}

	void CreatePoolLinks(int count) {
		for (int i = 0; i < count ; i++) 
			linksPool.Add(new VerletNodeLink());
	}


	static public VerletNode CreateNode(Vector2 position) {
		if (instance.nodesPool.Count == 0)
			instance.CreatePoolNodes(instance.nodesPoolChunkSize);

		VerletNode node = instance.nodesPool[instance.nodesPool.Count - 1];
		instance.nodesPool.RemoveAt(instance.nodesPool.Count - 1);
		node.position = position;
		node.StopForcible();

		instance.nodes.Add(node);

		return node;
	}

	static public VerletNodeLink CreateLink(VerletNode a, VerletNode b, float distance) {
		if (instance.linksPool.Count == 0)
			instance.CreatePoolLinks(instance.nodesPoolChunkSize);

		VerletNodeLink link = instance.linksPool[instance.linksPool.Count - 1];
		instance.linksPool.RemoveAt(instance.linksPool.Count - 1);
		link.node1 = a;
		link.node2 = b;
		link.distance = distance;

		instance.links.Add(link);

		return link;
	}

	static public void RemoveNode(VerletNode node) {
		instance.nodes.Remove(node);
		instance.nodesPool.Add(node);
	}

	static public void RemoveLink(VerletNodeLink link, bool withNodes = true) {
		instance.links.Remove(link);
		instance.linksPool.Add(link);

		if (withNodes) {
			RemoveNode(link.node1);
			RemoveNode(link.node2);
		}
	}
}
