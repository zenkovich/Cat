using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class BendBlock : MonoBehaviour {

	public List<Vector2> pivots = new List<Vector2>();

	[Range(1f, 6f)]
	public float meshPivotsCoef = 4f;
	public float width = 1f;
	public float colliderWidth = 1f;
	public float colliderOffs;
	
	[Range(0.5f, 2f)]
	public float bezierCoef = 1f;

	private float basicBezierCoef = 0.3f;


	void OnValidate() {
		UpdateMesh();
	}

	[ContextMenu("UpdateMesh")]
	public void UpdateMesh() {

		GetComponent<MeshFilter>().sharedMesh = new Mesh();
		Mesh mesh = GetComponent<MeshFilter>().sharedMesh;
		PolygonCollider2D collider = GetComponent<PolygonCollider2D>();

		//calculating mesh pivots
		int meshPivotsCount = Mathf.CeilToInt(pivots.Count*meshPivotsCoef);
		Vector2[] meshPivots = new Vector2[meshPivotsCount];
		Vector3[] verticies = new Vector3[meshPivotsCount*2];
		Vector2[] uvCoords = new Vector2[meshPivotsCount*2];
		Vector2[] collPoints = new Vector2[meshPivotsCount*2];
		int[] polyIndexes = new int[(meshPivotsCount - 1)*2*3];

		int currentPivot = -1;
		Vector2 curPivot = Vector2.zero;
		Vector2 nextPivot = Vector2.zero;
		Vector2 supPoint1 = Vector2.zero;
		Vector2 supPoint2 = Vector2.zero;
		float pivotCfStart = 0;
		float pivotCfEnd = 0;

		for (int i = 0; i < meshPivotsCount; i++) {

			float cf = (float)i/(float)(meshPivotsCount - 1);
			int cfPivot = Mathf.Min( Mathf.FloorToInt(cf*(float)(pivots.Count - 1)), pivots.Count - 2);

			if (cfPivot != currentPivot) {
				currentPivot = cfPivot;
				
				pivotCfStart = (float)cfPivot/(float)(pivots.Count - 1);
				pivotCfEnd = (float)(cfPivot + 1f)/(float)(pivots.Count - 1);
				curPivot = pivots[cfPivot];
				nextPivot = pivots[cfPivot + 1];

				if (cfPivot > 0)
					supPoint1 = curPivot + (curPivot - pivots[cfPivot - 1] + nextPivot - curPivot)*0.5f*basicBezierCoef*bezierCoef;
				else
					supPoint1 = curPivot + (nextPivot - curPivot)*basicBezierCoef*bezierCoef;

				if (cfPivot < pivots.Count - 2)
					supPoint2 = nextPivot - (nextPivot - pivots[cfPivot + 2] + curPivot - nextPivot)*-0.5f*basicBezierCoef*bezierCoef;
				else
					supPoint2 = nextPivot - (curPivot - nextPivot)*-basicBezierCoef*bezierCoef;

				//Debug.DrawLine(curPivot, nextPivot);
// 				Debug.DrawLine(curPivot, supPoint1, Color.red);
// 				Debug.DrawLine(supPoint2, nextPivot, Color.green);
			}

			meshPivots[i] = Utils.InterpolateBezier(curPivot, supPoint1, supPoint2, nextPivot, (cf - pivotCfStart)/(pivotCfEnd - pivotCfStart));
		}

		//calculate mesh verticies by pivots
		for (int i = 0; i < meshPivotsCount; i++) {

			//vertex pos
			float cf = (float)i/(float)(meshPivotsCount - 1);

			Vector2 segDir = Vector2.zero;
			if (i == 0)
				segDir = meshPivots[i + 1] - meshPivots[i];
			else if (i == meshPivotsCount - 1)
				segDir = meshPivots[i] - meshPivots[i - 1];
			else
				segDir = ((meshPivots[i + 1] - meshPivots[i]) + (meshPivots[i] - meshPivots[i - 1]))*0.5f;

			Vector2 segDirNorm = segDir.cross(1f).normalized*0.5f;
			
			int vid = i*2;
			verticies[vid]     = meshPivots[i] + segDirNorm*width;
			verticies[vid + 1] = meshPivots[i] - segDirNorm*width;

			//Debug.DrawLine(verticies[vid], verticies[vid + 1], Color.red, 2f);
			
			//uv
			uvCoords[vid]     = new Vector2(cf, 0);
			uvCoords[vid + 1] = new Vector2(cf, 1);

			//polygons
			if (i > 0) {
				polyIndexes[(i - 1)*3*2 + 0] = vid;
				polyIndexes[(i - 1)*3*2 + 1] = vid - 1;
				polyIndexes[(i - 1)*3*2 + 2] = vid - 2;
						
				polyIndexes[(i - 1)*3*2 + 3] = vid;
				polyIndexes[(i - 1)*3*2 + 4] = vid + 1;
				polyIndexes[(i - 1)*3*2 + 5] = vid - 1;
			}

			//collider
			Vector2 colUp = meshPivots[i] + segDirNorm*(colliderWidth + colliderOffs);
			Vector2 colDown = meshPivots[i] + segDirNorm*(-colliderWidth + colliderOffs);

			collPoints[i] = colUp;
			collPoints[meshPivotsCount*2 - i - 1] = colDown;
		}

		collider.points = collPoints;

// 		for (int i = 0; i < collPoints.Count() - 1; i++)
// 			Debug.DrawLine(collPoints[i], collPoints[i + 1]);

		mesh.Clear();
		mesh.vertices = verticies;
		mesh.uv = uvCoords;
		mesh.triangles = polyIndexes;

		//Debug.Log("mesh updated on " + gameObject, gameObject);
	}
}
