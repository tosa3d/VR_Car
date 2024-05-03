using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;
using System;

namespace ProCarPaint
{
	public class TextureEditor : EditorWindow
	{
		public Texture2D texture;
		public Texture2D ProTexture;
		public Color color = Color.red;
		public ProCarPaintShaderUtilities.ProcessTextureMethod Method;
		ColorPickerHDRConfig a;
		bool valid;

		[MenuItem("Tools/Pro Car Paint/Texture Editor")]
		static void Window()
		{
			TextureEditor window = (TextureEditor)EditorWindow.GetWindow(typeof(TextureEditor));
			window.maxSize = new Vector2(400, 317);
			window.minSize = new Vector2(400, 317);
			window.autoRepaintOnSceneChange = true;
			window.titleContent.text = "Texture Editor";
		}

		void OnGUI()
		{
			GUILayout.BeginVertical(GUI.skin.box);
			GUILayout.Label("Texture to Process ");
			texture = (Texture2D)EditorGUILayout.ObjectField(texture, typeof(Texture2D), true);
			color = EditorGUILayout.ColorField(new GUIContent(""), color, true, true, true, a);
			Method = (ProCarPaintShaderUtilities.ProcessTextureMethod)EditorGUILayout.EnumPopup("Method", Method);
			GUILayout.BeginHorizontal();
			if (texture) {
				try {
					texture.GetPixel(0, 0);
					valid = true;
				} catch (UnityException e) {
					valid = false;
				}
				if (valid) {
					if (GUILayout.Button("Process"))
						ProTexture = ProCarPaintShaderUtilities.ProcessTexture(texture, color, Method);
				} else {
					GUILayout.Label(new GUIContent("               Please enable Read/Write in texture's setting"));
				}
			}
			GUILayout.EndHorizontal();
			GUILayout.EndVertical();
			if (texture) {

				EditorGUILayout.BeginHorizontal();
				GUILayout.Label(new GUIContent("Preview:"));
				GUILayout.Label(new GUIContent("Processed:"));
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.BeginHorizontal();
				GUILayout.Space(30);
				GUILayout.Label(texture, GUILayout.Width(150), GUILayout.Height(150));
				GUILayout.Space(40);
				if (ProTexture)
					GUILayout.Label(ProTexture, GUILayout.Width(150), GUILayout.Height(150));

				EditorGUILayout.EndHorizontal();
				if (GUILayout.Button("Clear texture"))
					ProTexture = null;

				if (ProTexture && GUILayout.Button("Save texture")) {
					string path = EditorUtility.SaveFilePanel("Save texture as PNG", "", texture.name + ".png", "png");
					if (path.Length != 0)
						File.WriteAllBytes(path, ProTexture.EncodeToPNG());
					AssetDatabase.Refresh();
				}
			}
		}

		void OnInspectorUpdate()
		{
			Repaint();
		}
	}

	public class NormalMapMaskGenerator : EditorWindow
	{
		public Texture2D texture;
		public Texture2D ProTexture;
		public float Sharpness = 5;
		RenderTexture rentex;

		[MenuItem("Tools/Pro Car Paint/Normal Map Mask Generator")]
		static void Window()
		{
			NormalMapMaskGenerator window = (NormalMapMaskGenerator)EditorWindow.GetWindow(typeof(NormalMapMaskGenerator));
			window.maxSize = new Vector2(400, 317);
			window.minSize = new Vector2(400, 317);
			window.autoRepaintOnSceneChange = true;
			window.titleContent.text = "Normal Map Mask Generator";
		}

		void OnGUI()
		{
			GUILayout.BeginVertical(GUI.skin.box);
			GUILayout.Label("Normal Map ");
			texture = (Texture2D)EditorGUILayout.ObjectField(texture, typeof(Texture2D), true);
			Sharpness = EditorGUILayout.FloatField("Sharpness ", Sharpness);
			Sharpness = Mathf.Clamp(Sharpness, 1, 20);

			GUILayout.BeginHorizontal();
			if (texture) {
				if (GUILayout.Button("Process")) {
					ProTexture = ProCarPaintShaderUtilities.NormalMapMaskExtract(texture, Sharpness);
				} 
			} else {
				GUILayout.Label(new GUIContent("               Please Select Normat Map To Create Mask Of"));
			}
			GUILayout.EndHorizontal();
			GUILayout.EndVertical();
			if (texture) {

				EditorGUILayout.BeginHorizontal();
				GUILayout.Label(new GUIContent("Preview:"));
				GUILayout.Label(new GUIContent("Processed:"));
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.BeginHorizontal();
				GUILayout.Space(30);
				GUILayout.Label(texture, GUILayout.Width(150), GUILayout.Height(150));
				GUILayout.Space(40);
				if (ProTexture)
					GUILayout.Label(ProTexture, GUILayout.Width(150), GUILayout.Height(150));

				EditorGUILayout.EndHorizontal();
				if (GUILayout.Button("Clear texture"))
					ProTexture = null;

				if (ProTexture && GUILayout.Button("Save texture")) {
					string path = EditorUtility.SaveFilePanel("Save texture as PNG", "", texture.name + ".png", "png");
					if (path.Length != 0)
						File.WriteAllBytes(path, ProTexture.EncodeToPNG());
					AssetDatabase.Refresh();
				}
			}
		}

		void OnInspectorUpdate()
		{
			Repaint();
		}
	}

	public static class ProCarPaintShaderUtilities
	{
		public enum ProcessTextureMethod
		{
			Multiplier,
			Overwrite
		}

		public static Texture2D ProcessTexture(Texture2D tex, Color color, ProcessTextureMethod mode)
		{
			Color Pixel;
			Texture2D ProTexture = new Texture2D(tex.width, tex.height, TextureFormat.ARGB32, false);
			if (mode == ProcessTextureMethod.Multiplier) {
				for (int i = 0; i <= tex.width; i++) {
					for (int j = 0; j <= tex.width; j++) {
						Pixel = tex.GetPixel(i, j);
						ProTexture.SetPixel(i, j, (Pixel * color));
					}
				}
			} else {
				for (int i = 0; i <= tex.width; i++) {
					for (int j = 0; j <= tex.width; j++) {
						Pixel = tex.GetPixel(i, j);
						ProTexture.SetPixel(i, j, new Color(color.r, color.g, color.b, Pixel.a));
					}
				}
			}
			ProTexture.Apply();
			return ProTexture;
		}

		public static Texture2D NormalMapMaskExtract(Texture2D tex, float sharpness)
		{
			RenderTexture rentex = new  RenderTexture(tex.width, tex.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
			Material mat = new Material(Shader.Find("Hidden / Pro Car Paint Utility shader"));
			Debug.Log(mat.shader.name.ToString());
			mat.SetTexture("_NormalMap", tex);
			mat.SetFloat("_Sharp", sharpness);
			Texture2D ProTexture = new Texture2D(tex.width, tex.height, TextureFormat.ARGB32, false);
			Graphics.Blit(ProTexture, rentex, mat);
			RenderTexture.active = rentex;
			ProTexture.ReadPixels(new Rect(0, 0, rentex.width, rentex.height), 0, 0);
			ProTexture.Apply();
			RenderTexture.active = null;
			return ProTexture;
		}
	}
}