using UnityEngine;

namespace ProCarPaint
{

	[AddComponentMenu ("Pro Car Paint Shader/Camera/Free Rom Camera")]
	[RequireComponent (typeof(Camera))]
	public class FreeRoamCamera : MonoBehaviour
	{
		public bool CursorVisible = false;
		public bool Enable = true;
		[Space]
		public bool allowMovement = true;
		public bool allowRotation = true;
		public bool allowZoom = true;
		[Space]
		[Space]
		public float initialSpeed = 10f;
		public float increaseSpeed = 0.05f;
		[Space]
		public float RotationSensitivity = 0.01f;
		[Space]
		public int zoomSteps = 5;
		public float zoomDampening = 0.3f;
		[Space]
		[Space]
		public KeyCode upButton = KeyCode.E;
		public KeyCode downButton = KeyCode.Q;
		public KeyCode forwardButton = KeyCode.W;
		public KeyCode backwardButton = KeyCode.S;
		public KeyCode rightButton = KeyCode.D;
		public KeyCode leftButton = KeyCode.A;
		public KeyCode ToggleButton = KeyCode.Escape;


		private float fov;
		private float desiredDistance;
		private float currentDistance;
		private float currentSpeed = 0f;
		private bool moving = false;
		private float temp;

		void Awake ()
		{
			Cursor.visible = CursorVisible;
			fov = GetComponent <Camera> ().fieldOfView;
			desiredDistance = fov;
		}

		void Update ()
		{
			if (!Enable)
				return;
		
			if (Input.GetKey (ToggleButton))
				ToggleEnable ();
		
			if (allowZoom) {
				if (Input.GetAxis ("Mouse ScrollWheel") != 0) {
					temp = Input.GetAxis ("Mouse ScrollWheel") * (200f / zoomSteps);
					desiredDistance = GetComponent <Camera> ().fieldOfView - temp;
				} else if (Input.GetMouseButtonDown (2))
					desiredDistance = fov;
				currentDistance = Mathf.Lerp (GetComponent <Camera> ().fieldOfView, desiredDistance, zoomDampening);
				GetComponent <Camera> ().fieldOfView = currentDistance;
				GetComponent <Camera> ().fieldOfView =	Mathf.Clamp (GetComponent <Camera> ().fieldOfView, 10, 30);
			}
		
			if (allowMovement) {
				bool lastMoving = moving;
				Vector3 deltaPosition = Vector3.zero;

				if (moving)
					currentSpeed += increaseSpeed * Time.deltaTime;

				moving = false;

				CheckMove (upButton, ref deltaPosition, transform.up);
				CheckMove (downButton, ref deltaPosition, -transform.up);
				CheckMove (forwardButton, ref deltaPosition, transform.forward);
				CheckMove (backwardButton, ref deltaPosition, -transform.forward);
				CheckMove (rightButton, ref deltaPosition, transform.right);
				CheckMove (leftButton, ref deltaPosition, -transform.right);

				if (moving) {
					if (moving != lastMoving)
						currentSpeed = initialSpeed;
					transform.position += deltaPosition * currentSpeed * Time.deltaTime;
				} else
					currentSpeed = 0f;            
			}

			if (allowRotation) {
				Vector3 eulerAngles = transform.eulerAngles;
				eulerAngles.x += -Input.GetAxis ("Mouse Y") * 359f * RotationSensitivity;
				eulerAngles.y += Input.GetAxis ("Mouse X") * 359f * RotationSensitivity;
				transform.eulerAngles = eulerAngles;
			}
		}

		private void CheckMove (KeyCode keyCode, ref Vector3 deltaPosition, Vector3 directionVector)
		{
			if (Input.GetKey (keyCode)) {
				moving = true;
				deltaPosition += directionVector;
			}
		}

		public void ToggleEnable ()
		{
			Enable = !Enable;
		}
	}
}