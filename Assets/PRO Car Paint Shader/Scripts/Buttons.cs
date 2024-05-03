using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace ProCarPaint
{

	[AddComponentMenu ("Pro Car Paint Shader/Misc/Buttons")]
	public class Buttons : MonoBehaviour
	{
		public Material Mat;
		public Transform[] PaintObject;

		void Start ()
		{
			GetComponent<Button> ().onClick.AddListener (Onclick);
		}

		void Onclick ()
		{
			for (int i = 0; i < PaintObject.Length; i++) {
				PaintObject [i].GetComponent<MeshRenderer> ().material = Mat;
			}
		}
	}
}