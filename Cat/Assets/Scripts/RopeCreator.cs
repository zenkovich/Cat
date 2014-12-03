using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class RopeCreator : MonoBehaviour {

	public Rope ropePrefab;

	[Range(0.5f, 3f)]
	public float lengthCoef = 1f;
	
	[Range(0.0f, 5f)]
	public float impulseCoef1 = 4f;
	[Range(0f, 1f)]
	public float posCoef1 = 0.2f;
	
	[Range(0.0f, 5f)]
	public float impulseCoef2 = 4f;
	[Range(0f, 1f)]
	public float posCoef2 = 0.2f;

	void Start() {

		float initLength = GetComponent<SpriteRenderer>().sprite.bounds.size.y;

		Vector2 worldAnchor1 = transform.TransformPoint(Vector2.zero);
		Vector2 worldAnchor2 = transform.TransformPoint(Vector2.up*-initLength);

		RaycastHit2D hit1 = Physics2D.RaycastAll(worldAnchor1, Vector2.zero).FirstOrDefault(x => x.collider.gameObject.rigidbody2D != null);
		RaycastHit2D hit2 = Physics2D.RaycastAll(worldAnchor2, Vector2.zero).FirstOrDefault(x => x.collider.gameObject.rigidbody2D != null);

		if ((hit1.collider != null && hit1.collider.gameObject.rigidbody2D == null) ||
			(hit2.collider != null && hit2.collider.gameObject.rigidbody2D == null)) {

			Debug.LogWarning("Rope " + gameObject + " doesn't connected! Hit1:" + hit1.collider + ", hit2:" + hit2.collider, gameObject);
			return;
		}
		
		Rigidbody2D rb1 = hit1.collider == null ? null:hit1.collider.gameObject.rigidbody2D;
		Rigidbody2D rb2 = hit2.collider == null ? null:hit2.collider.gameObject.rigidbody2D;
		
		Vector2 locAnchor1 = rb1 == null ? worldAnchor1:(Vector2)rb1.transform.InverseTransformPoint(worldAnchor1);
		Vector2 locAnchor2 = rb2 == null ? worldAnchor2:(Vector2)rb2.transform.InverseTransformPoint(worldAnchor2);

		Rope rope = (Instantiate(ropePrefab.gameObject) as GameObject).GetComponent<Rope>();
		rope.Length = initLength*lengthCoef;
		rope.AttachNode(0, locAnchor1, rb1, impulseCoef1, posCoef1);
		rope.AttachNode(-1, locAnchor2, rb2, impulseCoef2, posCoef2);

		for (int i = 0; i < rope.nodes.Count; i++) {
			float cf = (float)i/(float)(rope.nodes.Count - 1);
			rope.nodes[i].position = Vector2.Lerp(worldAnchor1, worldAnchor2, cf);
			rope.nodes[i].StopForcible();
		}

		Destroy(gameObject);
	}
}
