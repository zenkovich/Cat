using UnityEngine;
using System.Collections;

public class SmoothFollowCam : MonoBehaviour {

	public float smooth = 1f;
	public Transform target;

	void Update() {
		Vector3 pos = transform.position;
		pos = Vector3.Lerp(pos, target.position, Time.deltaTime*smooth);
		pos.z = transform.position.z;
		transform.position = pos;
	}
}
