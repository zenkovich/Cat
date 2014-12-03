using UnityEngine;
using System.Linq;
using System.Collections;

public class RopeRenderer : MonoBehaviour {

	public float width = 0.2f;
	public float nodesMultiplier = 4f;
	public float bezierCoef = 1f;

	private MeshFilter meshFilter;
	private Rope rope;
	private Vector3[] meshVericiesPos;
	private Vector2[] meshVericiesUv;
	private int[] meshPolygons;
	private Color[] meshColors;
	private Vector2[] pivotPoints;
	private int pivotsCount;

	void Awake() {
		meshFilter = GetComponent<MeshFilter>();
		rope = GetComponent<Rope>();
		rope.onNodesChanged = OnRopeNodesChanged;
	}
	
	Vector2 InterpolateBezier(Vector2 a, Vector2 b, Vector2 c, Vector2 d, float coef)
	{
		float m = 1 - coef;
		float n = m*m;
		float o = n*m;
		float p = coef*coef;
		float r = p*coef;

		return a*o + b*3.0f*coef*n + c*3.0f*p*m + d*r;
	}

	void FixedUpdate() {

		Vector2 transformPos = rope.nodes[rope.nodes.Count/2].position;
		transform.localPosition = transformPos;

		float fullRopeLength = 0;
		for (int i = 0; i < rope.nodes.Count - 1; i++) 
			fullRopeLength += (rope.nodes[i].position - rope.nodes[i + 1].position).magnitude;

		//Debug.Log("FULL LENGTH:" + fullRopeLength);
		
		int bezierNodeStartIdx = -1;
		int bezierNodeEndIdx = 0;
		Vector2 bezierNodeStartPos = Vector2.zero;
		Vector2 bezierNodeEndPos = Vector2.zero;
		Vector2 bezierStartSuppPos = Vector2.zero;
		Vector2 bezierEndSuppPos = Vector2.zero;
		float bezierLengthBegin = 0;
		float bezierLengthEnd = 0;

		bool first = true;
		for (int i = 0; i < pivotsCount; i++) {

			float coef = (float)i/(float)(pivotsCount - 1);
			float currentLength = coef*fullRopeLength;

			while (currentLength > bezierLengthEnd || first) {

				first = false;

				bezierNodeStartIdx++;
				bezierNodeEndIdx++;
				
				//Debug.Log("PP " + currentLength + " > " + bezierLengthEnd + " idx:" + bezierNodeStartIdx + "-" + bezierNodeEndIdx + " < " + rope.nodes.Count);

				bezierNodeStartPos = rope.nodes[bezierNodeStartIdx].position;
				bezierNodeEndPos = rope.nodes[bezierNodeEndIdx].position;
				bezierLengthBegin = bezierLengthEnd;
				bezierLengthEnd += (bezierNodeEndPos - bezierNodeStartPos).magnitude;

				//Debug.DrawLine(bezierNodeStartPos, bezierNodeEndPos);
				
				Vector2 dirStart = bezierNodeStartPos - rope.nodes[ Mathf.Clamp(bezierNodeStartIdx - 1, 0, rope.nodes.Count - 1) ].position;
				Vector2 dirEnd = rope.nodes[ Mathf.Clamp(bezierNodeEndIdx + 1, 0, rope.nodes.Count - 1) ].position - bezierNodeEndPos;

				float basicBezierCoef = 0.3f;
				bezierStartSuppPos = bezierNodeStartPos + dirStart*basicBezierCoef*bezierCoef;
				bezierEndSuppPos = bezierNodeEndPos - dirEnd*basicBezierCoef*bezierCoef;
			}

			float segCoef = (currentLength - bezierLengthBegin)/(bezierLengthEnd - bezierLengthBegin);

			pivotPoints[i] = InterpolateBezier(bezierNodeStartPos, bezierStartSuppPos, bezierEndSuppPos, bezierNodeEndPos, segCoef) - transformPos;
		}

		int polyIdx = 0;
		for (int i = 0; i < pivotsCount; i++) {
			
			int ii = i*2;

			//update position
			Vector2 norm = Vector2.up;
			Vector2 curNodePosition = pivotPoints[i];
			if (i == 0) {
				Vector2 dir = (pivotPoints[1] - curNodePosition).normalized;
				norm.x = -dir.y; norm.y = dir.x;
			}
			else if (i == pivotsCount - 1) {
				Vector2 dir = (curNodePosition - pivotPoints[pivotsCount - 2]).normalized;
				norm.x = -dir.y; norm.y = dir.x;
			}
			else {
				Vector2 dir = ((curNodePosition - pivotPoints[i - 1]) + (pivotPoints[i + 1] - curNodePosition)).normalized;
				norm.x = -dir.y; norm.y = dir.x;
			}

			meshVericiesPos[ii] = curNodePosition + norm*width;
			meshVericiesPos[ii + 1] = curNodePosition - norm*width;

			//Debug.DrawLine(meshVericiesPos[ii], meshVericiesPos[ii + 1]);

			//update uv
			meshVericiesUv[ii].x = 0;
			meshVericiesUv[ii].y = 0;
			
			meshVericiesUv[ii + 1].x = 0;
			meshVericiesUv[ii + 1].y = 0;

			//polygons
			if (i > 0) {
				meshPolygons[polyIdx*3]     = ii;
				meshPolygons[polyIdx*3 + 1] = ii + 1;
				meshPolygons[polyIdx*3 + 2] = ii - 2;
				polyIdx++;
				
				meshPolygons[polyIdx*3]     = ii + 1;
				meshPolygons[polyIdx*3 + 1] = ii - 1;
				meshPolygons[polyIdx*3 + 2] = ii - 2;
				polyIdx++;
			}
		}
		
		meshFilter.mesh.Clear();
		meshFilter.mesh.vertices = meshVericiesPos;
		meshFilter.mesh.triangles = meshPolygons;
		meshFilter.mesh.uv = meshVericiesUv;
		meshFilter.mesh.colors = meshColors;
	}

	void OnRopeNodesChanged() {
		pivotsCount = Mathf.CeilToInt(rope.nodes.Count*nodesMultiplier);
		int vertexCount = pivotsCount*2;
		pivotPoints = new Vector2[pivotsCount];
		meshVericiesPos = new Vector3[vertexCount];
		meshVericiesUv = new Vector2[vertexCount];
		meshPolygons = new int[(pivotsCount - 1)*2*3];

		meshColors = new Color[vertexCount];
		for (int i = 0; i < vertexCount; i++)
			meshColors[i] = Color.white;
		
		meshFilter.mesh.MarkDynamic();

		FixedUpdate();
	}
}
