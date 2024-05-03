using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

namespace ProCarPaint
{

	[AddComponentMenu ("Pro Car Paint Shader/Effects/Lerp Depth Of Field")]
	[RequireComponent (typeof(DepthOfField))]
	public class lerpDOF : MonoBehaviour
	{
		public float startFL = 0f;
		public float lerpTo = 8f;
		public float damping = 0.25f;
		public float Optimizer = .25f;
		public bool PingPong = true;

		void Awake ()
		{
			if (startFL == 0)
				startFL = GetComponent <DepthOfField> ().focalLength;
		}

		void Update ()
		{
			lerp ();

			if (GetComponent <DepthOfField> ().focalLength > lerpTo - Optimizer && GetComponent <DepthOfField> ().focalLength < lerpTo + Optimizer) {
				if (PingPong) {
					float temp;
					temp = startFL;
					startFL = lerpTo;
					lerpTo = temp;
				} else {
					this.enabled = false;
				}
			}
		}


		void lerp ()
		{
			GetComponent <DepthOfField> ().focalLength = Mathf.Lerp (
				GetComponent <DepthOfField> ().focalLength, 
				lerpTo,
				Time.smoothDeltaTime * damping);
		}

	}
}