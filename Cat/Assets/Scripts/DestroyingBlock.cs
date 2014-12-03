using UnityEngine;
using System.Collections;

public class DestroyingBlock : MonoBehaviour {

	public float damagePoints = 100f;
	public float connectionDamage = 60f;
	public Sprite[] crackSprites;

	private float damagePointsInitial;
	private RopeConnection ropeConnection;

	void Awake() {
		damagePointsInitial = damagePoints;
	}

	void OnRopeConnected(RopeConnection connection) {
		ropeConnection = connection;
		damagePoints -= connectionDamage;

		int spriteIdx = Mathf.FloorToInt((float)(crackSprites.Length + 2)*(damagePoints/damagePointsInitial)) - 1;
		GetComponent<SpriteRenderer>().sprite = crackSprites[ Mathf.Clamp(spriteIdx, 0, crackSprites.Length - 1) ];

		if (damagePoints < 0) {
			ropeConnection.Destroy();
			Destroy(gameObject);
		}
	}
}
