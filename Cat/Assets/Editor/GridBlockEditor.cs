using UnityEngine;
using System.Collections;
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(GridBlock))]
public class GridBlockEditor : Editor {

	private GridBlock gridBlock;
	private GUIStyle mBlueDot = null;

	void OnEnable() {
		gridBlock = (GridBlock)target;
	}

	new void OnInspectorGUI() {
		base.OnInspectorGUI();
	}

	void OnSceneGUI() {		
		if (gridBlock.isStatic)
			return;

		Handles.BeginGUI();


		for (int i = 0; i < gridBlock.anchorPoints.Count; i++) {
			Vector2 pos = gridBlock.anchorPoints[i];
			
			gridBlock.anchorPoints[i] = gridBlock.transform.InverseTransformPoint(
				Handles.FreeMoveHandle(gridBlock.transform.TransformPoint(pos), 
	        				           Quaternion.identity,
	        				           0.02f,
	        				           Vector3.zero, 
	        				           DrawCustomCap));

			EditorUtility.SetDirty(gridBlock);
	         
		}

		Handles.EndGUI();
	}

	void DrawCustomCap(int controlID, Vector3 position, Quaternion rotation, float size) {
		if (mBlueDot == null) mBlueDot = "sv_label_3";

		Vector2 screenPoint = HandleUtility.WorldToGUIPoint(position);
		Rect rect = new Rect(screenPoint.x - 7f, screenPoint.y - 7f, 14f, 14f);
		mBlueDot.Draw(rect, GUIContent.none, controlID);
	}
}
