using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerAffectorBar : MonoBehaviour {

	public Slider bar;
	public SpriteRenderer icon;
	private float initialTime;

	public void Init(Sprite sprite, float time) {
		icon.sprite = sprite;
		initialTime = time;
	}

	public void UpdateTime(float time) {
		bar.value = 1f - time/initialTime;
	}
}
