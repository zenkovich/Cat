using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class BezierPath : MonoBehaviour {

	[Serializable]
	public class PathPoint {
		public Vector2 position;
		public float rotation;
		public float bezierCoef;
	}

	[Serializable]
	public class PivotPoint {
		public Vector2 position;
		public Vector2 direction;
		public float length;
		public float distanceFromOrigin;
	}

	public List<PathPoint> pathPoints = new List<PathPoint>();
	public List<PivotPoint> pivots = new List<PivotPoint>();
	public float length;
	public int lastEdited = -1;

	void Update() {
		if (!Application.isPlaying)
			UpdatePivots();
	}

	public Vector2 GetPointOnPath(float length) {

		length = Mathf.Clamp(length, 0, this.length);

		for (int i = 0; i < pivots.Count - 1; i++) {
			if (pivots[i + 1].distanceFromOrigin < length)
				continue;

			return pivots[i].position + pivots[i].direction*pivots[i].length*(length - pivots[i].distanceFromOrigin)/pivots[i].length;
		}

		return pivots.Last().position;
	}

	public void UpdatePivots() {
		pivots.Clear();
		float pathSegPivots = 20;
		Transform transf = transform;

		for (int i = 0; i < pathPoints.Count - 1; i++) {
			PathPoint currPathPoint = pathPoints[i];
			PathPoint nextPathPoint = pathPoints[i + 1];
			
			Vector2 bezp1 = currPathPoint.position;
			Vector2 bezp2 = currPathPoint.position + Utils.RotatedVector2(currPathPoint.rotation, currPathPoint.bezierCoef);
			Vector2 bezp3 = nextPathPoint.position + Utils.RotatedVector2(nextPathPoint.rotation, -nextPathPoint.bezierCoef);
			Vector2 bezp4 = nextPathPoint.position;

			for (int j = 0; j < pathSegPivots; j++) {
				float cf = (float)j/(float)pathSegPivots;

				PivotPoint pivPoint = new PivotPoint();
				pivPoint.position = transf.TransformPoint(Utils.InterpolateBezier(bezp1, bezp2, bezp3, bezp4, cf));
				pivots.Add(pivPoint);
			}
		}

		pivots.Add(new PivotPoint(){ position = transf.TransformPoint(pathPoints.Last().position) });

		length = 0;
		for (int i = 0; i < pivots.Count - 1; i++) {
			Vector2 diff = pivots[i + 1].position - pivots[i].position;
			float len = diff.magnitude;
			pivots[i].direction = diff/len;
			pivots[i].length = len;
			pivots[i].distanceFromOrigin = length;

			length += len;
		}

		pivots.Last().distanceFromOrigin = length;
	}
}
