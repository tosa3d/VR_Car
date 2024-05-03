using UnityEngine;
using UnityStandardAssets.ImageEffects;
using System;

namespace ProCarPaint
{
	[ExecuteInEditMode]
	[AddComponentMenu ("Pro Car Paint Shader/Effects/Auto Depth Of Field")]
	[RequireComponent (typeof(Camera))]
	[RequireComponent (typeof(DepthOfField))]
	public sealed class AutoDOF: MonoBehaviour
	{
		[Range (0.1f, 10)]
		public float delay = 1;
		Transform cameraTransform;
		DepthOfField dof;

		void Awake ()
		{
			dof = GetComponent <DepthOfField> ();
			cameraTransform = this.GetComponent <Transform> (); 
		}

		void Start ()
		{
			if (dof.enabled == false)
				this.enabled = false;
		}

		void OnEnable ()
		{
			dof.enabled = true;
		}

		void OnDisable ()
		{
			dof.enabled = false;
		}

		private void FixedUpdate ()
		{
			RaycastHit hitInfo;
			if (Physics.Raycast (cameraTransform.position, cameraTransform.forward, out hitInfo)) {
				dof.focalLength = Mathf.Lerp (dof.focalLength, hitInfo.distance, Time.deltaTime * delay);
			}
		}

	}
}