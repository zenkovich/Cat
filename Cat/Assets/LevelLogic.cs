using UnityEngine;
using System.Collections;

public class LevelLogic : MonoBehaviour {

	public static LevelLogic instance;

	void Awake() {
		instance = this;
	}

	void Start() {
		OnLevelStart();
	}

	public void OnLevelStart() {
	}

	public void OnLevelWin() {
	}

	public void OnLevelLost() {
	}
}
