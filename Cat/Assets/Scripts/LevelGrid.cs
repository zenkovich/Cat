using UnityEngine;
using System.Collections;

public class LevelGrid : MonoBehaviour {

	public Vector2 cellSize;
	private static LevelGrid instance;

	public static LevelGrid Instance {
		get {
			if (instance ==  null)
				instance = FindObjectOfType<LevelGrid>();

			return instance;
		}
	}

}
