using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ProCarPaint
{

	[AddComponentMenu ("Pro Car Paint Shader/Misc/Runtime Edit")]
	public class RuntimeEdit : MonoBehaviour
	{
		public GameObject Mesh;
		public Material Mat;
		[Space]

		public Slider _Metallic;
		public Slider _Glossiness;
		public Slider _FresnelScale;
		public Slider _FresnelPaintExponent;
		public Slider _FlakesQuantity;
		public Slider _FlakesMetallic;
		public Slider _FlakesSmoothness;
		public Slider _FlakesSize;
		public Slider _FlakesDistance;
		public Slider _ReflectionIntensity;
		public Slider _ReflectionStrength;
		public Slider _ReflectionPower;
		public Slider _ReflectionBlur;
		public Slider _ClearCoatSmoothness;
		public Slider _ClearCoatNormalScale;
		public Toggle _FlakesEnabled;
		public Toggle _DecalsEnabled;
		public Toggle _ClearCoatEnabled;
		public Toggle _FlakesColorOverride;
		public Toggle _ReflectionColorOverride;
		public GameObject _ReflectionColor;
		public Dropdown _DecalsColorOver;
		public Dropdown _ReflectionColorOver;

		public GameObject _Base;
		public GameObject _Flakes;
		public GameObject _Decals;
		public GameObject _Reflection;
		public GameObject Canvas;
		public GameObject Setting;

		void Start ()
		{
			InvokeRepeating ("upd", 0, 1);
			GameObject.FindGameObjectWithTag ("MainCamera").GetComponent<MyCamera> ().Rotation.GetInput = !Canvas.activeInHierarchy;
		}

		void upd ()
		{
			Mat = Mesh.GetComponent<MeshRenderer> ().material;
			_Metallic.value = Mat.GetFloat ("_Metallic");
			_Glossiness.value = Mat.GetFloat ("_Glossiness");
			_FresnelScale.value = Mat.GetFloat ("_FresnelScale");
			_FresnelPaintExponent.value = Mat.GetFloat ("_FresnelPaintExponent");
			_FlakesMetallic.value = Mat.GetFloat ("_FlakesMetallic");
			_FlakesSmoothness.value = Mat.GetFloat ("_FlakesSmoothness");
			_FlakesSize.value = Mat.GetFloat ("_FlakesSize");
			_FlakesQuantity.value = Mat.GetFloat ("_FlakesQuantity");
			_FlakesDistance.value = Mat.GetFloat ("_FlakesDistance");
			_FlakesColorOverride.isOn = (Mat.GetFloat ("_FlakesColorOverride") == 1) ? true : false;
			_ReflectionIntensity.value = Mat.GetFloat ("_ReflectionIntensity");
			_ReflectionStrength.value = Mat.GetFloat ("_ReflectionStrength");
			_ReflectionPower.value = Mat.GetFloat ("_ReflectionExponent");
			_ReflectionBlur.value = Mat.GetFloat ("_ReflectionBlur");
			_ClearCoatSmoothness.value = Mat.GetFloat ("_ClearCoatSmoothness");
			_ClearCoatNormalScale.value = Mat.GetFloat ("_ClearCoatNormalScale");
			_ReflectionColorOverride.isOn = (Mat.GetFloat ("_ReflectionColorOverride") == 1) ? true : false;

			_FlakesEnabled.isOn = Mat.IsKeywordEnabled ("FLAKES");
			_DecalsEnabled.isOn = Mat.IsKeywordEnabled ("DECALS");
			_ClearCoatEnabled.isOn = Mat.IsKeywordEnabled ("REF");

			_DecalsColorOver.gameObject.SetActive (_FlakesEnabled.isOn);

			_DecalsColorOver.value = Mat.IsKeywordEnabled ("DECALS_Under") ? 1 : 0;
			_ReflectionColorOver.value = Mat.IsKeywordEnabled ("REF_Under") ? 1 : 0;

			if (_DecalsEnabled.isOn || _FlakesEnabled.isOn) {
				_ReflectionColorOver.gameObject.SetActive (true);
				if (_DecalsEnabled.isOn) {
					_ReflectionColorOver.ClearOptions ();
					List<string> options = new List<string> ();
					options.Add ("Over Decals");
					options.Add ("Under Decals");
					_ReflectionColorOver.AddOptions (options);
				} else {
					_ReflectionColorOver.ClearOptions ();
					List<string> options = new List<string> ();
					options.Add ("Over Flakes");
					options.Add ("Under Flakes");
					_ReflectionColorOver.AddOptions (options);
				}
			} else {
				_ReflectionColorOver.gameObject.SetActive (false);
			}
			_ReflectionColor.SetActive (_ReflectionColorOverride.isOn);
		}

		public void set_Metallic ()
		{
			Mat.SetFloat ("_Metallic", _Metallic.value);
		}

		public void set_Settings ()
		{
			Canvas.SetActive (!Canvas.activeInHierarchy);
			GameObject.FindGameObjectWithTag ("MainCamera").GetComponent<MyCamera> ().Rotation.GetInput = !Canvas.activeInHierarchy;
		}

		public void set_Glossiness ()
		{
			Mat.SetFloat ("_Glossiness", _Glossiness.value);
		}

		public void set_EmissionColor (GameObject g)
		{
			Mat.SetColor ("_EmissionColor", g.GetComponent<Image> ().color);
		}

		public void set_Color (GameObject g)
		{
			Mat.SetColor ("_Color", g.GetComponent<Image> ().color);
		}

		public void set_SecondaryPaint (GameObject g)
		{
			Mat.SetColor ("_SecondaryPaint", g.GetComponent<Image> ().color);
		}

		public void set_FresnelScale ()
		{
			Mat.SetFloat ("_FresnelScale", _FresnelScale.value);
		}

		public void set_FresnelPaintExponent ()
		{
			Mat.SetFloat ("_FresnelPaintExponent", _FresnelPaintExponent.value);
		}

		public void set_FlakesMetallic ()
		{
			Mat.SetFloat ("_FlakesMetallic", _FlakesMetallic.value);
		}

		public void set_FlakesSmoothness ()
		{
			Mat.SetFloat ("_FlakesSmoothness", _FlakesSmoothness.value);
		}

		public void set_FlakesSize ()
		{
			Mat.SetFloat ("_FlakesSize", _FlakesSize.value);
		}

		public void set_FlakesQuantity ()
		{
			Mat.SetFloat ("_FlakesQuantity", _FlakesQuantity.value);
		}

		public void set_FlakesDistance ()
		{
			Mat.SetFloat ("_FlakesDistance", _FlakesDistance.value);
		}

		public void set_ReflectionIntensity ()
		{
			Mat.SetFloat ("_ReflectionIntensity", _ReflectionIntensity.value);
		}

		public void set_ReflectionStrength ()
		{
			Mat.SetFloat ("_ReflectionStrength", _ReflectionStrength.value);
		}

		public void set_ReflectionPower ()
		{
			Mat.SetFloat ("_ReflectionExponent", _ReflectionPower.value);
		}

		public void set_ReflectionBlur ()
		{
			Mat.SetFloat ("_ReflectionBlur", _ReflectionBlur.value);
		}

		public void set_ReflectionColor (GameObject g)
		{
			Mat.SetColor ("_ReflectionColor", g.GetComponent<Image> ().color);
		}

		public void set_ClearCoatSmoothness ()
		{
			Mat.SetFloat ("_ClearCoatSmoothness", _ClearCoatSmoothness.value);
		}

		public void set_ClearCoatNormalScale ()
		{
			Mat.SetFloat ("_ClearCoatNormalScale", _ClearCoatNormalScale.value);
		}

		public void set_1DecalColor (GameObject g)
		{
			Mat.SetColor ("_1DecalColor", g.GetComponent<Image> ().color);
		}

		public void set_2DecalColor (GameObject g)
		{
			Mat.SetColor ("_2DecalColor", g.GetComponent<Image> ().color);
		}

		public void set_3DecalColor (GameObject g)
		{
			Mat.SetColor ("_3DecalColor", g.GetComponent<Image> ().color);
		}

		public void set_4DecalColor (GameObject g)
		{
			Mat.SetColor ("_4DecalColor", g.GetComponent<Image> ().color);
		}

		public void set_5DecalColor (GameObject g)
		{
			Mat.SetColor ("_5DecalColor", g.GetComponent<Image> ().color);
		}

		public void set_FlakesColorOverride (GameObject g)
		{
			Mat.SetFloat ("_FlakesColorOverride", (g.GetComponent<Toggle> ().isOn) ? 1 : 0);
		}

		public void set_ReflectionColorOverride (GameObject g)
		{
			Mat.SetFloat ("_ReflectionColorOverride", (g.GetComponent<Toggle> ().isOn) ? 1 : 0);
			_ReflectionColor.SetActive ((g.GetComponent<Toggle> ().isOn) ? true : false);

		}

		public void set_DecalsColorOver (GameObject g)
		{
			SetKeyword ("DECALS_Under", (g.GetComponent<Dropdown> ().value == 0) ? false : true);
		}

		public void set_ReflectionColorOver (GameObject g)
		{
			SetKeyword ("REF_Under", (g.GetComponent<Dropdown> ().value == 0) ? false : true);
		}

		public void set_FlakesEnabled (GameObject g)
		{
			SetKeyword ("FLAKES", g.GetComponent<Toggle> ().isOn);
		}

		public void set_DecalsEnabled (GameObject g)
		{
			SetKeyword ("DECALS", g.GetComponent<Toggle> ().isOn);
		}

		public void set_ClearCoatEnabled (GameObject g)
		{
			SetKeyword ("REF", g.GetComponent<Toggle> ().isOn);
		}

		public void BaseMenu ()
		{
			_Base.SetActive (On ());
		}

		public void FlakesMenu ()
		{
			_Flakes.SetActive (On ());
		}

		public void DecalsMenu ()
		{
			_Decals.SetActive (On ());
		}

		public void ReflectionMenu ()
		{
			_Reflection.SetActive (On ());
		}

		bool On ()
		{
			_Base.SetActive (false);
			_Flakes.SetActive (false);
			_Decals.SetActive (false);
			_Reflection.SetActive (false);
			return true;
		}

		void SetKeyword (string keyword, bool state)
		{
			if (state)
				Mat.EnableKeyword (keyword);
			else
				Mat.DisableKeyword (keyword);
		}
	}
}