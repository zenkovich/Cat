using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class TestEntry : MonoBehaviour {

	public TestScript tt;

	[ContextMenu("test")]
	void Test() {
		Debug.Log(tt.test);
		tt.test = "xx";
	}
}
