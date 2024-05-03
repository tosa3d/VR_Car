using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProCarPaint
{
	[AddComponentMenu("Pro Car Paint Shader/Effects/Realtime Reflection")]
	public class RealtimeReflection : MonoBehaviour
	{
		public enum Reflection_Resolution
		{
			VeryLow16 = 16,
			Low32 = 32,
			Medium128 = 128,
			High512 = 512,
			VeryHigh1024 = 1024,
			Ultra2048 = 2048,
		}

		[Header("------------ Reflection Probe ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------")]
		public ReflectionProbe reflectionProbe;

		[Space]
		[Header("Settings will be used if no Reflection Probe is assigned*")]
		[Header("------------ Reflection Probe Setting ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------")]
		[Space]
		public FrameOptions updateMode = FrameOptions.EveryPhysicsFrame;
		public ReflectionProbeTimeSlicingMode TimeSlicingMode = ReflectionProbeTimeSlicingMode.IndividualFaces;
		[Range(0f, 10f)]
		public float intensity = 1f;
		public Vector3 localBoxOffset_ = new Vector3(0, 0, 0);
		public Reflection_Resolution resolution_ = Reflection_Resolution.Medium128;
		public bool HDR_ = true;
		public ReflectionProbeClearFlags clearFlag_ = ReflectionProbeClearFlags.Skybox;
		public Color backgroundColor_ = new Color(49f / 255f, 77f / 255f, 121f / 255f, 0);
		public LayerMask cullingMask_ = 55;
		public bool UseOcculitionCulling_ = true;
		[Range(0f, 1f)]
		public float clipingPlaneNear_ = 0.3f;
		[Range(1f, 100f)]
		public float clipingPlaneFar_ = 50f;

		private Material mat;
		int renderID;

		void setSettings()
		{
			reflectionProbe.transform.parent = this.gameObject.transform;
			reflectionProbe.transform.localPosition = localBoxOffset_;

			reflectionProbe.backgroundColor = backgroundColor_;
			reflectionProbe.clearFlags = clearFlag_;
			reflectionProbe.cullingMask = cullingMask_;
			reflectionProbe.farClipPlane = clipingPlaneFar_;
			reflectionProbe.nearClipPlane = clipingPlaneNear_;
			reflectionProbe.intensity = intensity;
			reflectionProbe.hdr = HDR_;
			reflectionProbe.timeSlicingMode = TimeSlicingMode;

			reflectionProbe.transform.localRotation = new Quaternion(0, 0, 0, 0);
			reflectionProbe.size = new Vector3(0, 0, 0);
			reflectionProbe.boxProjection = true;
			reflectionProbe.blendDistance = 0;
			reflectionProbe.shadowDistance = 0;
			reflectionProbe.importance = 0;

			switch (resolution_) {
				case Reflection_Resolution.VeryLow16:
					reflectionProbe.resolution = 16;
					break;
				case Reflection_Resolution.Low32:
					reflectionProbe.resolution = 32;
					break;
				case Reflection_Resolution.Medium128:
					reflectionProbe.resolution = 128;
					break;
				case Reflection_Resolution.High512:
					reflectionProbe.resolution = 512;
					break;
				case Reflection_Resolution.VeryHigh1024:
					reflectionProbe.resolution = 1024;
					break;
				case Reflection_Resolution.Ultra2048:
					reflectionProbe.resolution = 2048;
					break;
			}
		}

		void Awake()
		{
			mat = gameObject.GetComponent<MeshRenderer>().material;
			if (!mat.shader.name.Contains("Pro Car Paint")) {
				Destroy(GetComponent<RealtimeReflection>());
			}

			gameObject.layer = 30;

			if (reflectionProbe == null) {
				if (transform.Find("Pro Car Paint Reflection") && transform.Find("Pro Car Paint Reflection").GetComponent <ReflectionProbe>()) {
					reflectionProbe = transform.Find("Pro Car Paint Reflection").GetComponent <ReflectionProbe>();
				} else {
					reflectionProbe = new GameObject("Pro Car Paint Reflection").AddComponent <ReflectionProbe>();
					this.setSettings();
				}
			}
		}

		void Start()
		{
			reflectionProbe.mode = ReflectionProbeMode.Realtime;
			reflectionProbe.refreshMode = ReflectionProbeRefreshMode.ViaScripting;
			gameObject.GetComponent<MeshRenderer>().material.SetTexture("_ReflectionCubeMap", reflectionProbe.texture);
			renderID = reflectionProbe.RenderProbe();
		}

		void Update()
		{
			if (updateMode == FrameOptions.EveryFrame && reflectionProbe.IsFinishedRendering(renderID)) {
				gameObject.GetComponent<MeshRenderer>().material.SetTexture("_ReflectionCubeMap", reflectionProbe.texture);
				renderID = reflectionProbe.RenderProbe();
			}
		}

		void LateUpdate()
		{
			if (updateMode == FrameOptions.EndOfEveryFrame && reflectionProbe.IsFinishedRendering(renderID)) {
				gameObject.GetComponent<MeshRenderer>().material.SetTexture("_ReflectionCubeMap", reflectionProbe.texture);
				renderID = reflectionProbe.RenderProbe();
			}
		}

		void FixedUpdate()
		{
			if (updateMode == FrameOptions.EveryPhysicsFrame && reflectionProbe.IsFinishedRendering(renderID)) {
				gameObject.GetComponent<MeshRenderer>().material.SetTexture("_ReflectionCubeMap", reflectionProbe.texture);
				renderID = reflectionProbe.RenderProbe();
			}
		}
	}
}