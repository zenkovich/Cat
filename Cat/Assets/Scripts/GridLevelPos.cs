using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class GridLevelPos : MonoBehaviour {
	
	void Update () {
		Vector2 cellSize = LevelGrid.Instance.cellSize;

		Vector2 pos = transform.position;
		pos.x = Mathf.Round(pos.x/cellSize.x)*cellSize.x;
		pos.y = Mathf.Round(pos.y/cellSize.y)*cellSize.y;
		transform.position = pos;
	}
}
