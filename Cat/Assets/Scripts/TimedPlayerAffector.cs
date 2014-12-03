using UnityEngine;
using System.Collections;

public class TimedPlayerAffector : MonoBehaviour {

	public Sprite barIconSprite;
	public PlayerAffectorBar barPrefab;
	public float time;

	private PlayerAffectorBar bar;
	private float timePast;

	void Awake() {
		bar = (Instantiate(barPrefab.gameObject) as GameObject).GetComponent<PlayerAffectorBar>();
		bar.transform.parent = GameObject.FindGameObjectWithTag("AffectorsBars").transform;
		bar.Init(barIconSprite, time);
	}

	void OnDestroy() {
		Destroy(bar.gameObject);
	}

	void Update() {
		time -= Time.deltaTime;
		bar.UpdateTime(time);

		if (time < 0) {
			GetComponent<PlayerUnit>().OnDead();
		}
	}
}
