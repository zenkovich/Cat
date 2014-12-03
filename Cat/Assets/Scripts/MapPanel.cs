using UnityEngine;
using System.Collections;

public class MapPanel : MonoBehaviour {

	public bool Visible {
		set { GetComponent<Animator>().SetBool("visible", value); }
		get { return GetComponent<Animator>().GetBool("visible"); }
	}

	public void ShowHide() {
		Visible = !Visible;
	}
}
