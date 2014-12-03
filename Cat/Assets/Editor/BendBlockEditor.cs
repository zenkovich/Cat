using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(BendBlock))]
public class BendBlockEditor : Editor {

	private BendBlock bendBlock;
	private GUIStyle mBlueDot = null;

	void OnEnable() {
		bendBlock = (BendBlock)target;
	}

	new void OnInspectorGUI() {
		base.OnInspectorGUI();
	}

	void OnSceneGUI() {		
		Handles.BeginGUI();

		bool moved = false;
		for (int i = 0; i < bendBlock.pivots.Count; i++) {
			Vector2 pos = bendBlock.pivots[i];
			
			bendBlock.pivots[i] = bendBlock.transform.InverseTransformPoint(
				Handles.FreeMoveHandle(bendBlock.transform.TransformPoint(pos), 
	        				           Quaternion.identity,
	        				           0.02f,
	        				           Vector3.zero, 
	        				           DrawCustomCap));


			if ((pos - bendBlock.pivots[i]).magnitude > 0.001f) 
				moved = true;
		}

		Handles.EndGUI();
			
		if (moved) {
			EditorUtility.SetDirty(bendBlock);
			bendBlock.UpdateMesh();
		}
	}

	void DrawCustomCap(int controlID, Vector3 position, Quaternion rotation, float size) {
		if (mBlueDot == null) mBlueDot = "sv_label_1";

		Vector2 screenPoint = HandleUtility.WorldToGUIPoint(position);
		Rect rect = new Rect(screenPoint.x - 7f, screenPoint.y - 7f, 14f, 14f);
		mBlueDot.Draw(rect, GUIContent.none, controlID);
	}
}
