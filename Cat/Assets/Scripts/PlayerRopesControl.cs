using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
//using UnityEditor;

public class PlayerRopesControl : MonoBehaviour {
	
	[Serializable]
	public class RopesSettings {	
		public float connectionImpCoef1 = 0.001f;
		public float connectionPosCoef1 = 0.001f;

		public float connectionImpCoef2 = 4f;
		public float connectionPosCoef2 = 0.5f;

		public float connectionImpCoef3 = 4f;
		public float connectionPosCoef3 = 0.2f;

		public float thisImpCoef1 = 0.001f;
		public float thisPosCoef1 = 0.001f;

		public float thisImpCoef2 = 4f;
		public float thisPosCoef2 = 0.2f;

		public float ropeFixinglength = 5f;
	}

	[Serializable]
	public class ControlView {
		public Transform cutRopesCursor;
		public Button[] buttons;
		public Transform arrowCursor;
		public GameObject lightCircle;
		public float arrowCursorInitialLength;
	}

	public enum ControlState { Waiting, DragPlayer, CutRopes, DragRope } 

	public int maxRopes = 4;
	public float maxRopeLength = 10f;
	public float minRopeLength = 1f;
	public float throwRopeVelocity;
	public RopeConnection connectionPrefab;
	public RopesSettings ropesSets;
	public ControlView viewSets;
	public float dragPlayerRopesSense = 0.3f;
	public float cuttingRopeDistThreshold = 1f;
	public float checkRopeCutDelay = 0.1f;	
	public float gyroForce;
	public bool cutRopesOnlyByButtons;

	public bool pauseWhenShoot;
	public bool pauseWhenConnect;

//private
	public List<RopeConnection> ropes = new List<RopeConnection>();
	public ControlState controlState = ControlState.Waiting;
	public RopeConnection draggingRope;
	public Vector2 lastCursorPos;
	private Vector2 pressingCursorPos;
	private float lastCutRopeCheckTime;
	private float lastRopeButtonTime;

	void Awake() {
		foreach (var btn in viewSets.buttons) 
			btn.gameObject.SetActive(false);
		
		viewSets.cutRopesCursor.gameObject.SetActive(false);
		viewSets.arrowCursor.gameObject.SetActive(false);
		viewSets.lightCircle.SetActive(false);

		Input.gyro.enabled = true;
	}

	public void RopeCutted() {
		lastRopeButtonTime = Time.time;
	}

	public void ThrowRope(Vector2 direction) {
		RopeConnection newConnection = (Instantiate(connectionPrefab.gameObject, transform.position, Quaternion.identity) as GameObject).GetComponent<RopeConnection>();
		ropes.Add(newConnection);

		newConnection.Throw(this, direction, throwRopeVelocity, viewSets.buttons.First(x => !x.gameObject.activeSelf));

		/*if (pauseWhenShoot)
			EditorApplication.isPaused = true;*/
	}

	void OnGUI() {
		Input.gyro.enabled = true;
		Input.gyro.updateInterval = 0.01f;
		GUI.Label(new Rect(0, 0, 200, 200), "Inch: " + Screen.width/Screen.dpi);
	}

	public void Update() {

		Vector2 cursorPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

		rigidbody2D.velocity += (Vector2)Input.gyro.gravity*gyroForce;

		//cheating
		if (Input.GetMouseButton(1)) {
			transform.position = cursorPos;
			rigidbody2D.velocity = Vector2.zero;
		}

		if (Input.GetMouseButtonDown(0)) {

			lastCursorPos = cursorPos;
			pressingCursorPos = cursorPos;

			if ((cursorPos - (Vector2)transform.position).magnitude < (collider2D as CircleCollider2D).radius*2f) {
				controlState = ControlState.DragPlayer;
				viewSets.arrowCursor.gameObject.SetActive(true);
				viewSets.lightCircle.SetActive(true);
			}
			else {
				RopeConnection connection = ropes.Find(x => ((Vector2)x.transform.position - cursorPos).magnitude < (x.collider2D as CircleCollider2D).radius*5.5f);

				if (connection != null) {
					draggingRope = connection;
					draggingRope.Lightning = true;
					controlState = ControlState.DragRope;
				}
			}

		}

		if (controlState == ControlState.Waiting && Input.GetMouseButton(0)) {
			if ((cursorPos - lastCursorPos).magnitude > cuttingRopeDistThreshold && !cutRopesOnlyByButtons) {
				controlState = ControlState.CutRopes;
				lastCursorPos = pressingCursorPos;
				viewSets.cutRopesCursor.gameObject.SetActive(true);
				lastCutRopeCheckTime = Time.time;
			}
		}

		if (controlState == ControlState.DragRope) {
			if (draggingRope.AttachedRope.Length > minRopeLength && draggingRope.AttachedRope.StressForce < draggingRope.stressForceThreshold)
				draggingRope.AttachedRope.Length -= Time.deltaTime*draggingRope.lowerLengthSpeed;
		}

		if (controlState == ControlState.CutRopes) {
			
			viewSets.cutRopesCursor.position = cursorPos;

			if (Time.time - lastCutRopeCheckTime > checkRopeCutDelay || Input.GetMouseButtonUp(0)) {

				lastCutRopeCheckTime = Time.time;
				List<RopeConnection> removingRopes = new List<RopeConnection>();

				Debug.DrawLine(cursorPos, lastCursorPos, Color.red, checkRopeCutDelay);

				foreach (var ropeConn in ropes) {
					if (ropeConn.AttachedRope.IsIntersects(lastCursorPos, cursorPos))
						removingRopes.Add(ropeConn);
				}

				removingRopes.ForEach(x => { ropes.Remove(x); x.Destroy(); }); 

				lastCursorPos = cursorPos;
			}
		}

		if (Input.GetMouseButtonUp(0)) {
			if (controlState == ControlState.Waiting && Time.time - lastRopeButtonTime > 0.1f) {
				int fid = -1;
				if (Input.touchCount > 0)
					fid = Input.touches[Input.touchCount - 1].fingerId;

				if (ropes.Count < maxRopes && !EventSystemManager.currentSystem.IsPointerOverEventSystemObject(fid)) {
					ThrowRope((cursorPos - (Vector2)transform.position).normalized);
				}
			}
			
			if (controlState == ControlState.DragRope)
				draggingRope.Lightning = false;

			controlState = ControlState.Waiting;
			viewSets.cutRopesCursor.gameObject.SetActive(false);
			viewSets.arrowCursor.gameObject.SetActive(false);
			viewSets.lightCircle.SetActive(false);
		}

		if (controlState == ControlState.DragPlayer) {
			Vector2 curDirNN = cursorPos - (Vector2)transform.position;
			Vector2 cursorDir = curDirNN.normalized;

			viewSets.arrowCursor.rotation = Quaternion.LookRotation(curDirNN, Vector3.forward)*Quaternion.Euler(90, 0, 90);
			viewSets.arrowCursor.localScale = Vector3.one*curDirNN.magnitude/viewSets.arrowCursorInitialLength;

			foreach (var conn in ropes) {
				Vector2 ropeDir = (conn.transform.position - transform.position).normalized;

				float d = conn.lowerLengthSpeed*Time.deltaTime*Vector2.Dot(cursorDir, ropeDir)*curDirNN.magnitude*dragPlayerRopesSense;
				if (d > 0) {
					if (conn.AttachedRope.Length > 0.8f && conn.AttachedRope.StressForce < conn.stressForceThreshold) 
						conn.AttachedRope.Length -= d;
				}
				else {
					if (conn.AttachedRope.Length < maxRopeLength)
						conn.AttachedRope.Length -= d;
				}
			}
		}
	}
}
