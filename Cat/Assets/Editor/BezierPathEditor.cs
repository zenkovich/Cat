using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

[CustomEditor(typeof(BezierPath))]
public class BezierPathEditor : Editor {

	private BezierPath bezierPath;
	private bool openedPivots = true;

	void OnEnable() {
		bezierPath = (BezierPath)target;
	}

	public override void OnInspectorGUI() {
		//base.OnInspectorGUI();

		openedPivots = EditorsUtilities.DrawHeader("Pivots", openedPivots);

		if (openedPivots) {
				
			EditorsUtilities.BeginContents();
			Color[] secColors = new Color[]{ new Color(1f, 1f, 1f), new Color(0.8f, 1.0f, 0.8f) };
			Color lastEditedColor = Color.yellow;
			int removingIdx = -1;

			for (int i = 0; i < bezierPath.pathPoints.Count; i++) {
				EditorsUtilities.BeginContentsColored( bezierPath.lastEdited == i ? lastEditedColor:secColors[i%secColors.Length], 1);
				
				GUILayout.BeginHorizontal();
				bezierPath.pathPoints[i].position = EditorGUILayout.Vector2Field("", bezierPath.pathPoints[i].position);
				bezierPath.pathPoints[i].rotation = EditorGUILayout.FloatField(bezierPath.pathPoints[i].rotation);
				bezierPath.pathPoints[i].bezierCoef = EditorGUILayout.FloatField(bezierPath.pathPoints[i].bezierCoef);	

				if (GUILayout.Button("\u25AC", "ButtonRight", GUILayout.Width(16f), GUILayout.Height(16f)))
					removingIdx = i;

				GUILayout.EndHorizontal();

				EditorsUtilities.EndContents();
			}

			if (removingIdx >= 0)
				bezierPath.pathPoints.RemoveAt(removingIdx);
			
			EditorsUtilities.EndContents();
		}

		if (GUILayout.Button("Add")) {
			bezierPath.pathPoints.Add(new BezierPath.PathPoint(){ position = Vector2.right*0.5f, bezierCoef = 1f });
		}

		if (GUI.changed) {
			EditorUtility.SetDirty(bezierPath);
			bezierPath.UpdatePivots();
		}
	}

	void OnSceneGUI() {				

		GUI.Label(new Rect(10,10,10,10), "This is a label");
		for (int i = 0; i < bezierPath.pivots.Count - 1; i++) {
			Handles.DrawLine(bezierPath.pivots[i].position, bezierPath.pivots[i + 1].position);
		}

		if (Application.isPlaying)
			return;
		
		for (int i = 0; i < bezierPath.pathPoints.Count; i++) {
			Vector2 pathPointPosLocal = bezierPath.pathPoints[i].position;
			Vector2 pathPointPosWorld = bezierPath.transform.TransformPoint(pathPointPosLocal);

			Vector2 bezSupPointLeftLocal = Utils.RotatedVector2(bezierPath.pathPoints[i].rotation, bezierPath.pathPoints[i].bezierCoef) + pathPointPosLocal;
			Vector2 bezSupPointLeftWorld = bezierPath.transform.TransformPoint(bezSupPointLeftLocal);
			
			Vector2 bezSupPointRightLocal = Utils.RotatedVector2(bezierPath.pathPoints[i].rotation, -bezierPath.pathPoints[i].bezierCoef) + pathPointPosLocal;
			Vector2 bezSupPointRightWorld = bezierPath.transform.TransformPoint(bezSupPointRightLocal);
			
			Handles.color = new Color(0.2f, 1.0f, 0.2f, 0.5f);
			Handles.DrawDottedLine(pathPointPosWorld, bezSupPointLeftWorld, 10f);

			Vector2 bezSupPointLeftWorldNew = Handles.FreeMoveHandle(bezSupPointLeftWorld, Quaternion.identity, HandleUtility.GetHandleSize(Vector2.zero)*0.1f, 
				                              Vector3.zero, Handles.SphereCap);
			
			Handles.color = new Color(1.0f, 0.2f, 0.2f, 0.5f);
			Handles.DrawDottedLine(pathPointPosWorld, bezSupPointRightWorld, 10f);
			Vector2 bezSupPointRightWorldNew = Handles.FreeMoveHandle(bezSupPointRightWorld, Quaternion.identity, HandleUtility.GetHandleSize(Vector2.zero)*0.1f, 
				                              Vector3.zero, Handles.SphereCap);
			
			Vector2 bezSupPointLeftLocalNew = (Vector2)bezierPath.transform.InverseTransformPoint(bezSupPointLeftWorldNew) - pathPointPosLocal;			
			Vector2 bezSupPointRightLocalNew = (Vector2)bezierPath.transform.InverseTransformPoint(bezSupPointRightWorldNew) - pathPointPosLocal;
			
			if ((bezSupPointLeftLocalNew - bezSupPointLeftLocal + pathPointPosLocal).magnitude > 0.001f) {
				bezierPath.pathPoints[i].rotation = Utils.AngleSigned(bezSupPointLeftLocalNew);
 				bezierPath.pathPoints[i].bezierCoef = bezSupPointLeftLocalNew.magnitude;
				bezierPath.lastEdited = i;
			}
			
			if ((bezSupPointRightLocalNew - bezSupPointRightLocal + pathPointPosLocal).magnitude > 0.001f) {
				bezierPath.pathPoints[i].rotation = Utils.AngleSigned(bezSupPointRightLocalNew*-1f);
 				bezierPath.pathPoints[i].bezierCoef = bezSupPointRightLocalNew.magnitude;
				bezierPath.lastEdited = i;
			}
		}
		
		Handles.color = new Color(0.2f, 0.2f, 1f, 0.5f);
		for (int i = 0; i < bezierPath.pathPoints.Count; i++) {
			Vector2 pathPointPosLocal = bezierPath.pathPoints[i].position;
			Vector2 pathPointPosWorld = bezierPath.transform.TransformPoint(pathPointPosLocal);
			
			Vector2 pathPointPosWorldNew = Handles.FreeMoveHandle(pathPointPosWorld, Quaternion.identity, HandleUtility.GetHandleSize(Vector2.zero)*0.15f, 
				                                                  Vector3.zero, Handles.SphereCap);

			if ((pathPointPosWorld - pathPointPosWorldNew).magnitude > 0.001f) {
				bezierPath.pathPoints[i].position = bezierPath.transform.InverseTransformPoint(pathPointPosWorldNew);
				bezierPath.lastEdited = i;
			}
		}
			
		if (GUI.changed) {
			EditorUtility.SetDirty(bezierPath);
			bezierPath.UpdatePivots();
		}
	}
}
