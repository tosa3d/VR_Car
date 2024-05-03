using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Rendering;

public class ProCarPaintShader : ShaderGUI {

    MaterialProperty _Color;
    MaterialProperty _SecondaryPaint;
    MaterialProperty _PaintFresnelScale;
    MaterialProperty _PaintFresnelPaintExponent;
    MaterialProperty _Metallic;
    MaterialProperty _Glossiness;
    MaterialProperty _Translucent;
    MaterialProperty _EmissionColor;
    MaterialProperty _MainTex;
    MaterialProperty _BumpScale;
    MaterialProperty _BumpMap;
    MaterialProperty _Cutoff;
    MaterialProperty _CutoffMap;
    MaterialProperty _MetallicGlossMap;
    MaterialProperty _MatcapTexture;
    MaterialProperty _MatcapIntensity;
    MaterialProperty _FlakeNormal;
    MaterialProperty _FlakeMask;
    MaterialProperty _FlakeMetallic;
    MaterialProperty _FlakeSmoothness;
    MaterialProperty _FlakeSize;
    MaterialProperty _FlakeDistance;
    MaterialProperty _FlakeNormalScale;
    MaterialProperty _FlakeColorOverride;
    MaterialProperty _ReflectionCubeMap;
    MaterialProperty _ReflectionColorOverride;
    MaterialProperty _ReflectionColor;
    MaterialProperty _ReflectionIntensity;
    MaterialProperty _ReflectionStrength;
    MaterialProperty _ReflectionExponent;
    MaterialProperty _ReflectionBlur;
    MaterialProperty _ClearCoatNormal;
    MaterialProperty _ClearCoatNormalScale;
    MaterialProperty _ClearCoatSmoothness;
    MaterialProperty _1Decal;
    MaterialProperty _2Decal;
    MaterialProperty _3Decal;
    MaterialProperty _4Decal;
    MaterialProperty _5Decal;
    MaterialProperty _1DecalColor;
    MaterialProperty _2DecalColor;
    MaterialProperty _3DecalColor;
    MaterialProperty _4DecalColor;
    MaterialProperty _5DecalColor;

    public enum ShaderType { Opaque, OpaqueLite, Transparent, TransparentLite, MatcapLite }
    ShaderType shader, lastshader;
    public enum QuickSetup { Select, Metallic, Gloss, Matte, Plastic, Pearlescent }
    QuickSetup setup = QuickSetup.Select;
    public enum Layer { Base, Flake, Decal, ClearCoat, }

    Texture logo;
    Rect iconrect;
    MaterialEditor editor;
    Material material;
    Color originalguicolor, clr = new Color (.6f, .6f, .6f);

    bool firsttime = true;
    Layer view = (Layer) (-1);
    bool albedouvfold, albedonormaluvfold, extrasmapfold, cutoffmapfold, clearcoatnormaluvfold, decal1uvfold, decal2uvfold, decal3uvfold, decal4uvfold, decal5uvfold;
    bool flakekeyword, decalkeyword, clearcoatkeyword, decalunderkeyword, clearcoatunderkeyword, clearcoatfirstkeyword;
    bool last_flakekeyword, last_decalkeyword, last_clearcoatkeyword, last_decalunderkeyword, last_clearcoatunderkeyword, last_clearcoatfirstkeyword;
    public List<Layer> layers = new List<Layer> { Layer.Base };
    private ReorderableList layerslist;

    void AddLayer (params Layer[] Layers) {
        foreach (Layer L in Layers) {
            if (layers.Contains (L)) Debug.LogWarning ("Already Contains This Layer : " + L);
            else if ((shader == ShaderType.Transparent || shader == ShaderType.TransparentLite || shader == ShaderType.MatcapLite) && layers.Count >= 3) Debug.LogError ("Already Contains 4 layers, Cannot add " + L);
            else if (layers.Count >= 4) Debug.LogError ("Already Contains 4 layers, Cannot add " + L);
            else layers.Add (L);
        }
    }

    void RemoveLayer (Layer layer) {
        if ((shader == ShaderType.Transparent || shader == ShaderType.TransparentLite || shader == ShaderType.MatcapLite) && layer == Layer.Flake && !(layers.Contains (Layer.Flake))) return;
        else if (!(layers.Contains (layer))) Debug.LogError ("Does not Contains This Layer : " + layer);
        else layers.Remove (layer);
    }

    public override void OnGUI (MaterialEditor editor, MaterialProperty[] properties) {
        EditorGUI.BeginChangeCheck ();

        if (this.firsttime) {
            layerslist = new ReorderableList (layers, typeof (Layer), true, true, true, true);
            layerslist.drawHeaderCallback = (Rect rect) => { EditorGUI.LabelField (rect, "Layers"); };
            layerslist.onAddCallback = (list) => {
                Layer temp = (shader != ShaderType.Transparent && shader != ShaderType.TransparentLite && shader != ShaderType.MatcapLite) ? Layer.Flake : Layer.Decal;
                while (layers.Contains ((Layer) temp)) temp++;
                layers.Insert ((shader != ShaderType.Transparent && shader != ShaderType.TransparentLite && shader != ShaderType.MatcapLite) ? (int) temp : (int) temp - 1, temp);
            };
            layerslist.onCanAddCallback = (list) => {
                if ((shader == ShaderType.Transparent || shader == ShaderType.TransparentLite || shader == ShaderType.MatcapLite) && layers.Count >= 3) return false;
                else if (layers.Count >= 4) return false;
                else return true;
            };
            layerslist.onCanRemoveCallback = (list) => { return (list.index != 0) ? true : false; };
            layerslist.onReorderCallbackWithDetails = (list, o, n) => {
                if ((Layer) layers[0] != Layer.Base) {
                    Layer temp = (Layer) list.list[n];
                    layers.RemoveAt (n);
                    layers.Insert (o, temp);
                }
            };
            layerslist.onChangedCallback = (list) => {

                bool temp_flakekeyword = layers.Contains (Layer.Flake);
                bool temp_decalkeyword = layers.Contains (Layer.Decal);
                bool temp_clearcoatkeyword = layers.Contains (Layer.ClearCoat);
                bool temp_decalunderkeyword = layers.Contains (Layer.Decal) && layers.Contains (Layer.Flake) && layers.IndexOf (Layer.Decal) < layers.IndexOf (Layer.Flake);
                bool temp_clearcoatfirstkeyword = layers.Contains (Layer.Flake) && layers.Contains (Layer.Decal) && layers.Contains (Layer.ClearCoat) && layers.IndexOf (Layer.ClearCoat) == 2;
                bool temp_clearcoatunderkeyword = layers.Contains (Layer.ClearCoat) &&
                    (layers.Contains (Layer.Decal) || layers.Contains (Layer.Flake)) &&
                    layers.IndexOf (Layer.ClearCoat) == 1 ||
                    (decalunderkeyword && clearcoatfirstkeyword);

                SetKeyword ("FLAKE", temp_flakekeyword);
                SetKeyword ("DECAL", temp_decalkeyword);
                SetKeyword ("CLEARCOAT", temp_clearcoatkeyword);
                SetKeyword ("DECAL_UNDER", temp_decalunderkeyword);
                SetKeyword ("CLEARCOAT_FIRST", temp_clearcoatfirstkeyword);
                SetKeyword ("CLEARCOAT_UNDER", temp_clearcoatunderkeyword);
            };
            this.editor = editor;
            this.material = editor.target as Material;
            originalguicolor = GUI.backgroundColor;
            logo = Resources.Load ("CarPaintShader", typeof (Texture2D)) as Texture2D;
            iconrect.height = logo.height;
            iconrect.width = logo.width;
            firsttime = false;
        }
        if (!editor.isVisible) return;
        GetData (this.material, properties);

        GUILayout.BeginHorizontal ();
        GUILayout.Space ((EditorGUIUtility.currentViewWidth - logo.width - 10f) / 2f);
        GUILayout.Label (logo, GUILayout.Width (iconrect.width - 25f), GUILayout.Height (iconrect.height));
        GUILayout.EndHorizontal ();

        GUILayout.BeginHorizontal ();
        editor.DefaultPreviewSettingsGUI ();
        GUILayout.EndHorizontal ();
        editor.OnInteractivePreviewGUI (GUILayoutUtility.GetRect (200, 200), EditorGUIUtility.isProSkin ? EditorStyles.helpBox : EditorStyles.foldout);
        _ ();

        layerslist.DoLayoutList ();

        _ ();
        setup = (QuickSetup) EditorGUILayout.EnumPopup ("Quick Setup", setup);
        if (setup != QuickSetup.Select) setup = Quicksetup (setup);

#if UNITY_2019
        if (RenderPipelineManager.currentPipeline == null)
            shader = (ShaderType) EditorGUILayout.EnumPopup ("Shader Type", shader);
#elif UNITY_2018
        shader = (ShaderType) EditorGUILayout.EnumPopup ("Shader Type", shader);
#endif
        if (shader != lastshader) {
            switch (shader) {
                case ShaderType.Opaque:
                    material.shader = UnityEngine.Shader.Find ("Pro Car Paint");
                    break;
                case ShaderType.OpaqueLite:
                    material.shader = UnityEngine.Shader.Find ("Hidden / Pro Car Paint - Lite");
                    break;
                case ShaderType.Transparent:
                    material.shader = UnityEngine.Shader.Find ("Hidden / Pro Car Paint Transparent");
                    RemoveLayer (Layer.Flake);
                    break;
                case ShaderType.TransparentLite:
                    material.shader = UnityEngine.Shader.Find ("Hidden / Pro Car Paint Transparent - Lite");
                    RemoveLayer (Layer.Flake);
                    break;
                case ShaderType.MatcapLite:
                    material.shader = UnityEngine.Shader.Find ("Hidden / Pro Car Paint Matcap - Lite");
                    RemoveLayer (Layer.Flake);
                    break;
            }
            lastshader = shader;
            editor.Repaint ();
        }
        _ ();

        GUI.backgroundColor = EditorGUIUtility.isProSkin ? new Color (.65f, .65f, .65f) : new Color (.7f, .7f, .7f);
        EditorGUILayout.BeginVertical (GUI.skin.button);

        RenderView ();
        EditorGUILayout.EndVertical ();

    }
    void Layers (Material material) {

        flakekeyword = material.IsKeywordEnabled ("FLAKE") && (shader == ShaderType.Opaque || shader == ShaderType.OpaqueLite);
        decalkeyword = material.IsKeywordEnabled ("DECAL");
        clearcoatkeyword = material.IsKeywordEnabled ("CLEARCOAT");
        decalunderkeyword = material.IsKeywordEnabled ("DECAL_UNDER");
        clearcoatunderkeyword = material.IsKeywordEnabled ("CLEARCOAT_UNDER");
        clearcoatfirstkeyword = material.IsKeywordEnabled ("CLEARCOAT_FIRST");

        if (flakekeyword == last_flakekeyword && decalkeyword == last_decalkeyword && clearcoatkeyword == last_clearcoatkeyword && decalunderkeyword == last_decalunderkeyword && clearcoatunderkeyword == last_clearcoatunderkeyword && clearcoatfirstkeyword == last_clearcoatfirstkeyword)
            return;

        last_flakekeyword = flakekeyword;
        last_decalkeyword = decalkeyword;
        last_clearcoatkeyword = clearcoatkeyword;
        last_decalunderkeyword = decalunderkeyword;
        last_clearcoatunderkeyword = clearcoatunderkeyword;
        last_clearcoatfirstkeyword = clearcoatfirstkeyword;

        if (flakekeyword) {
            if (decalkeyword) {
                if (decalunderkeyword) {
                    if (clearcoatkeyword) {
                        if (clearcoatunderkeyword) {
                            if (clearcoatfirstkeyword) AddLayer (Layer.Decal, Layer.ClearCoat, Layer.Flake);
                            else AddLayer (Layer.ClearCoat, Layer.Decal, Layer.Flake);
                        } else AddLayer (Layer.Decal, Layer.Flake, Layer.ClearCoat);
                    } else AddLayer (Layer.Decal, Layer.Flake);
                } else {
                    if (clearcoatkeyword) {
                        if (clearcoatunderkeyword) {
                            AddLayer (Layer.ClearCoat, Layer.Flake, Layer.Decal);
                        } else {
                            if (clearcoatfirstkeyword) AddLayer (Layer.Flake, Layer.ClearCoat, Layer.Decal);
                            else AddLayer (Layer.Flake, Layer.Decal, Layer.ClearCoat);
                        }
                    } else AddLayer (Layer.Flake, Layer.Decal);
                }
            } else {
                if (clearcoatkeyword) {
                    if (clearcoatunderkeyword) AddLayer (Layer.ClearCoat, Layer.Flake);
                    else AddLayer (Layer.Flake, Layer.ClearCoat);
                } else AddLayer (Layer.Flake);
            }
        } else {
            if (decalkeyword) {
                if (clearcoatkeyword) {
                    if (clearcoatunderkeyword) AddLayer (Layer.ClearCoat, Layer.Decal);
                    else AddLayer (Layer.Decal, Layer.ClearCoat);
                } else AddLayer (Layer.Decal);
            } else {
                if (clearcoatkeyword) AddLayer (Layer.ClearCoat);
                else { /* dont add any layer */ }
            }
        }

    }

    void Shader (Material material) {
        string name = material.shader.name.ToLower ();
        bool lite = false, transparent = false, matcap = false;
        if (name.Contains ("lite")) lite = true;
        if (name.Contains ("transparent")) transparent = true;
        if (name.Contains ("matcap")) matcap = true;

        if (matcap) shader = ShaderType.MatcapLite;
        else if (transparent) shader = (lite) ? ShaderType.TransparentLite : ShaderType.Transparent;
        else shader = (lite) ? ShaderType.OpaqueLite : ShaderType.Opaque;
    }

    void GetData (Material material, MaterialProperty[] properties) {

        Shader (material);
        Layers (material);

        _Color = FindProperty ("_Color", properties);
        _MainTex = FindProperty ("_MainTex", properties);
        if (shader != ShaderType.MatcapLite) _Metallic = FindProperty ("_Metallic", properties);
        if (shader != ShaderType.MatcapLite) _Glossiness = FindProperty ("_Glossiness", properties);
        if (shader != ShaderType.MatcapLite) _EmissionColor = FindProperty ("_EmissionColor", properties);
        if (shader != ShaderType.Transparent && shader != ShaderType.TransparentLite) _SecondaryPaint = FindProperty ("_SecondaryPaint", properties);
        if (shader != ShaderType.Transparent && shader != ShaderType.TransparentLite) _PaintFresnelScale = FindProperty ("_PaintFresnelScale", properties);
        if (shader != ShaderType.Transparent && shader != ShaderType.TransparentLite) _PaintFresnelPaintExponent = FindProperty ("_PaintFresnelPaintExponent", properties);
        if (shader == ShaderType.Transparent || shader == ShaderType.TransparentLite) _Cutoff = FindProperty ("_Cutoff", properties);
        if (shader == ShaderType.Transparent || shader == ShaderType.TransparentLite) _CutoffMap = FindProperty ("_CutoffMap", properties);

        if (shader == ShaderType.Transparent || shader == ShaderType.TransparentLite) _Translucent = FindProperty ("_Translucent", properties);

        if (shader != ShaderType.MatcapLite) _BumpMap = FindProperty ("_BumpMap", properties);
        if (shader != ShaderType.MatcapLite) _BumpScale = FindProperty ("_BumpScale", properties);
        if (shader != ShaderType.OpaqueLite && shader != ShaderType.TransparentLite && shader != ShaderType.MatcapLite) _MetallicGlossMap = FindProperty ("_MetallicGlossMap", properties);
        if (shader == ShaderType.MatcapLite) _MatcapTexture = FindProperty ("_MatcapTexture", properties);
        if (shader == ShaderType.MatcapLite) _MatcapIntensity = FindProperty ("_MatcapIntensity", properties);

        if (shader != ShaderType.Transparent && shader != ShaderType.TransparentLite && shader != ShaderType.MatcapLite) _FlakeNormal = FindProperty ("_FlakeNormal", properties);
        if (shader != ShaderType.Transparent && shader != ShaderType.TransparentLite && shader != ShaderType.MatcapLite) _FlakeMask = FindProperty ("_FlakeMask", properties);
        if (shader != ShaderType.Transparent && shader != ShaderType.TransparentLite && shader != ShaderType.MatcapLite) _FlakeMetallic = FindProperty ("_FlakeMetallic", properties);
        if (shader != ShaderType.Transparent && shader != ShaderType.TransparentLite && shader != ShaderType.MatcapLite) _FlakeSmoothness = FindProperty ("_FlakeSmoothness", properties);
        if (shader != ShaderType.Transparent && shader != ShaderType.TransparentLite && shader != ShaderType.MatcapLite) _FlakeSize = FindProperty ("_FlakeSize", properties);
        if (shader != ShaderType.Transparent && shader != ShaderType.TransparentLite && shader != ShaderType.MatcapLite) _FlakeDistance = FindProperty ("_FlakeDistance", properties);
        if (shader != ShaderType.Transparent && shader != ShaderType.TransparentLite && shader != ShaderType.MatcapLite) _FlakeNormalScale = FindProperty ("_FlakeNormalScale", properties);
        if (shader != ShaderType.Transparent && shader != ShaderType.TransparentLite && shader != ShaderType.MatcapLite) _FlakeColorOverride = FindProperty ("_FlakeColorOverride", properties);

        _1Decal = FindProperty ("_1Decal", properties);
        _1DecalColor = FindProperty ("_1DecalColor", properties);
        _2Decal = FindProperty ("_2Decal", properties);
        _2DecalColor = FindProperty ("_2DecalColor", properties);
        _3Decal = FindProperty ("_3Decal", properties);
        _3DecalColor = FindProperty ("_3DecalColor", properties);

        if (shader != ShaderType.OpaqueLite && shader != ShaderType.TransparentLite) {
            _4Decal = FindProperty ("_4Decal", properties);
            _4DecalColor = FindProperty ("_4DecalColor", properties);
            _5Decal = FindProperty ("_5Decal", properties);
            _5DecalColor = FindProperty ("_5DecalColor", properties);
        }

        _ReflectionCubeMap = FindProperty ("_ReflectionCubeMap", properties);
        _ReflectionIntensity = FindProperty ("_ReflectionIntensity", properties);
        _ReflectionStrength = FindProperty ("_ReflectionStrength", properties);
        _ReflectionBlur = FindProperty ("_ReflectionBlur", properties);
        _ReflectionExponent = FindProperty ("_ReflectionExponent", properties);
        _ReflectionColor = FindProperty ("_ReflectionColor", properties);
        _ReflectionColorOverride = FindProperty ("_ReflectionColorOverride", properties);
        if (shader != ShaderType.Transparent && shader != ShaderType.TransparentLite && shader != ShaderType.MatcapLite) _ClearCoatNormal = FindProperty ("_ClearCoatNormal", properties);
        if (shader != ShaderType.Transparent && shader != ShaderType.TransparentLite && shader != ShaderType.MatcapLite) _ClearCoatNormalScale = FindProperty ("_ClearCoatNormalScale", properties);
        if (shader != ShaderType.Transparent && shader != ShaderType.TransparentLite && shader != ShaderType.MatcapLite) _ClearCoatSmoothness = FindProperty ("_ClearCoatSmoothness", properties);
    }

    void RenderView () {
        GUI.backgroundColor = originalguicolor;
        EditorGUILayout.BeginHorizontal ();
        GUI.backgroundColor = view == Layer.Base ? !EditorGUIUtility.isProSkin ? Color.white : Color.gray : EditorGUIUtility.isProSkin ? Color.white : Color.gray;
        if (GUILayout.Button (new GUIContent ("Base\nLayer"))) view = Layer.Base;
        GUI.backgroundColor = view == Layer.Flake ? !EditorGUIUtility.isProSkin ? Color.white : Color.gray : EditorGUIUtility.isProSkin ? Color.white : Color.gray;
        if (flakekeyword && GUILayout.Button (new GUIContent ("Flake\nLayer"))) view = Layer.Flake;
        GUI.backgroundColor = view == Layer.Decal ? !EditorGUIUtility.isProSkin ? Color.white : Color.gray : EditorGUIUtility.isProSkin ? Color.white : Color.gray;
        if (decalkeyword && GUILayout.Button (new GUIContent ("Decals\nLayer"))) view = Layer.Decal;
        GUI.backgroundColor = view == Layer.ClearCoat ? !EditorGUIUtility.isProSkin ? Color.white : Color.gray : EditorGUIUtility.isProSkin ? Color.white : Color.gray;
        if (clearcoatkeyword && GUILayout.Button (new GUIContent ("Clear Coat\nLayer"))) view = Layer.ClearCoat;
        EditorGUILayout.EndHorizontal ();
        GUI.backgroundColor = originalguicolor;

        switch (view) {
            case Layer.Base:
                BaseView ();
                break;
            case Layer.Flake:
                FlakeView ();
                break;
            case Layer.Decal:
                decalsView ();
                break;
            case Layer.ClearCoat:
                clearcloatView ();
                break;
        }
    }

    void BaseView () {
        EditorGUILayout.BeginVertical (GUI.skin.box);
        GUI.backgroundColor = clr;
        if (shader != ShaderType.MatcapLite) {
            _ ();
            EditorGUILayout.HelpBox ("Properties", MessageType.None);
            _ ();
            editor.ShaderProperty (_Metallic, "Metallic");
            editor.ShaderProperty (_Glossiness, "Smoothness");
            if (shader == ShaderType.Transparent || shader == ShaderType.TransparentLite) editor.ShaderProperty (_Cutoff, "Transparency");
            if (shader == ShaderType.Transparent || shader == ShaderType.TransparentLite) {
                editor.ShaderProperty (_Translucent, "Translucent");
                SetKeyword ("BLUR", _Translucent.floatValue != 0);
            }

            _ ();
        }

        EditorGUILayout.HelpBox ("Paints", MessageType.None);
        _ ();
        editor.ShaderProperty (_Color, "Paint (RGB)");
        if (shader != ShaderType.Transparent && shader != ShaderType.TransparentLite) editor.ShaderProperty (_SecondaryPaint, "Secoondary Paint (RGB)");
        if (shader != ShaderType.MatcapLite) editor.ShaderProperty (_EmissionColor, "Emission (RGB)");
        if (shader != ShaderType.Transparent && shader != ShaderType.TransparentLite) editor.ShaderProperty (_PaintFresnelScale, "Fresnel Scale");
        if (shader != ShaderType.Transparent && shader != ShaderType.TransparentLite) editor.ShaderProperty (_PaintFresnelPaintExponent, "Fresnel Exponent");
        if (shader == ShaderType.MatcapLite) editor.ShaderProperty (_MatcapIntensity, "Matcap Intensity");
        _ ();

        EditorGUILayout.HelpBox ("Textures", MessageType.None);
        _ ();
        EditorGUILayout.BeginHorizontal ();
        editor.TexturePropertySingleLine (new GUIContent ("Albedo (RGB)  Paints  (A)"), _MainTex);
        albedouvfold = EditorGUILayout.Foldout (albedouvfold, "UV");
        EditorGUILayout.EndHorizontal ();
        if (albedouvfold) editor.TextureScaleOffsetProperty (_MainTex);

        if (shader != ShaderType.MatcapLite) {
            GUILayout.BeginHorizontal ();
            editor.TexturePropertySingleLine (new GUIContent ("Albedo Normal"), _BumpMap, _BumpScale);
            albedonormaluvfold = EditorGUILayout.Foldout (albedonormaluvfold, "UV");
            GUILayout.EndHorizontal ();
            if (albedonormaluvfold) editor.TextureScaleOffsetProperty (_BumpMap);
        }

        if (shader != ShaderType.OpaqueLite && shader != ShaderType.TransparentLite && shader != ShaderType.MatcapLite) {
            GUILayout.BeginHorizontal ();
            editor.TexturePropertySingleLine (new GUIContent ("Metallic (R) Smoothness (G) Emission (B)"), _MetallicGlossMap);
            extrasmapfold = EditorGUILayout.Foldout (extrasmapfold, "UV");
            GUILayout.EndHorizontal ();
            if (extrasmapfold) editor.TextureScaleOffsetProperty (_MetallicGlossMap);
        }

        if (shader == ShaderType.MatcapLite) {
            GUILayout.BeginHorizontal ();
            editor.TexturePropertySingleLine (new GUIContent ("Matcap Texture"), _MatcapTexture);
            extrasmapfold = EditorGUILayout.Foldout (extrasmapfold, "UV");
            GUILayout.EndHorizontal ();
            if (extrasmapfold) editor.TextureScaleOffsetProperty (_MatcapTexture);

        }

        if (shader == ShaderType.Transparent || shader == ShaderType.TransparentLite) {
            GUILayout.BeginHorizontal ();
            editor.TexturePropertySingleLine (new GUIContent ("Transparent (A)"), _CutoffMap);
            cutoffmapfold = EditorGUILayout.Foldout (cutoffmapfold, "UV");
            GUILayout.EndHorizontal ();
            if (cutoffmapfold) editor.TextureScaleOffsetProperty (_CutoffMap);
        }

        GUI.backgroundColor = originalguicolor;
        EditorGUILayout.EndVertical ();
    }

    void FlakeView () {
        EditorGUILayout.BeginVertical (GUI.skin.box);
        GUI.backgroundColor = clr;
        _ ();
        EditorGUILayout.HelpBox ("Textures", MessageType.None);
        _ ();
        editor.TexturePropertySingleLine (new GUIContent ("Flakes Color (RGB) Mask (A)"), _FlakeMask);
        editor.TexturePropertySingleLine (new GUIContent ("Flakes Normal"), _FlakeNormal, _FlakeNormalScale);
        _ ();
        EditorGUILayout.HelpBox ("Properties", MessageType.None);
        _ ();
        editor.ShaderProperty (_FlakeMetallic, "Metallic");
        editor.ShaderProperty (_FlakeSmoothness, "Smoothness");
        EditorGUILayout.BeginHorizontal ();
        _FlakeSize.floatValue = EditorGUILayout.FloatField ("Size - Distance", _FlakeSize.floatValue);
        _FlakeDistance.floatValue = Mathf.Clamp (EditorGUILayout.FloatField (_FlakeDistance.floatValue), 0, 20);
        EditorGUILayout.EndHorizontal ();
        editor.ShaderProperty (_FlakeColorOverride, "Color Override");
        GUI.backgroundColor = originalguicolor;
        EditorGUILayout.EndVertical ();
    }

    void decalsView () {
        EditorGUILayout.BeginVertical (GUI.skin.box);
        GUI.backgroundColor = clr;
        _ ();

        EditorGUILayout.BeginHorizontal ();
        editor.TexturePropertySingleLine (new GUIContent ("1 decal"), _1Decal);
        decal1uvfold = EditorGUILayout.Foldout (decal1uvfold, "UV");
        GUILayout.EndHorizontal ();
        if (decal1uvfold) editor.TextureScaleOffsetProperty (_1Decal);
        editor.ShaderProperty (_1DecalColor, "", 0);

        EditorGUILayout.BeginHorizontal ();
        editor.TexturePropertySingleLine (new GUIContent ("2 decal"), _2Decal);
        decal2uvfold = EditorGUILayout.Foldout (decal2uvfold, "UV");
        GUILayout.EndHorizontal ();
        if (decal2uvfold) editor.TextureScaleOffsetProperty (_2Decal);
        editor.ShaderProperty (_2DecalColor, "", 0);

        EditorGUILayout.BeginHorizontal ();
        editor.TexturePropertySingleLine (new GUIContent ("3 decal"), _3Decal);
        decal3uvfold = EditorGUILayout.Foldout (decal3uvfold, "UV");
        GUILayout.EndHorizontal ();
        if (decal3uvfold) editor.TextureScaleOffsetProperty (_3Decal);
        editor.ShaderProperty (_3DecalColor, "", 0);

        if (shader != ShaderType.OpaqueLite && shader != ShaderType.TransparentLite) {
            EditorGUILayout.BeginHorizontal ();
            editor.TexturePropertySingleLine (new GUIContent ("4 decal"), _4Decal);
            decal4uvfold = EditorGUILayout.Foldout (decal4uvfold, "UV");
            GUILayout.EndHorizontal ();
            if (decal4uvfold) editor.TextureScaleOffsetProperty (_4Decal);
            editor.ShaderProperty (_4DecalColor, "", 0);

            EditorGUILayout.BeginHorizontal ();
            editor.TexturePropertySingleLine (new GUIContent ("5 decal"), _5Decal);
            decal5uvfold = EditorGUILayout.Foldout (decal5uvfold, "UV");
            GUILayout.EndHorizontal ();
            if (decal5uvfold) editor.TextureScaleOffsetProperty (_5Decal);
            editor.ShaderProperty (_5DecalColor, "", 0);
        }

        GUI.backgroundColor = originalguicolor;
        EditorGUILayout.EndVertical ();
    }

    void clearcloatView () {
        EditorGUILayout.BeginVertical (GUI.skin.box);
        GUI.backgroundColor = clr;
        _ ();
        EditorGUILayout.HelpBox ("Textures", MessageType.None);
        _ ();
        editor.TexturePropertySingleLine (new GUIContent ("Reflection Cubemap"), _ReflectionCubeMap);

        if (shader != ShaderType.Transparent && shader != ShaderType.TransparentLite && shader != ShaderType.MatcapLite) {
            GUILayout.BeginHorizontal ();
            editor.TexturePropertySingleLine (new GUIContent ("Clear Coat Normal"), _ClearCoatNormal, _ClearCoatNormalScale);
            clearcoatnormaluvfold = EditorGUILayout.Foldout (clearcoatnormaluvfold, "UV");
            GUILayout.EndHorizontal ();
            if (clearcoatnormaluvfold) editor.TextureScaleOffsetProperty (_ClearCoatNormal);
        }

        EditorGUILayout.HelpBox ("Properties", MessageType.None);
        _ ();
        if (shader != ShaderType.Transparent && shader != ShaderType.TransparentLite && shader != ShaderType.MatcapLite) editor.ShaderProperty (_ClearCoatSmoothness, "Clear Coat Smoothness");
        EditorGUILayout.BeginHorizontal ();
        _ReflectionIntensity.floatValue = Mathf.Clamp (EditorGUILayout.FloatField ("Intensity - Strength", _ReflectionIntensity.floatValue), 0, 10);
        _ReflectionStrength.floatValue = Mathf.Clamp (EditorGUILayout.FloatField (_ReflectionStrength.floatValue), 0, 1);
        EditorGUILayout.EndHorizontal ();
        EditorGUILayout.BeginHorizontal ();
        _ReflectionExponent.floatValue = Mathf.Clamp (EditorGUILayout.FloatField ("Exponent - Blur", _ReflectionExponent.floatValue), 0, 10);
        _ReflectionBlur.floatValue = Mathf.Clamp (EditorGUILayout.FloatField (_ReflectionBlur.floatValue), 0, 10);
        EditorGUILayout.EndHorizontal ();
        editor.ShaderProperty (_ReflectionColorOverride, "Reflection Color Override");
        if (_ReflectionColorOverride.floatValue == 1) editor.ShaderProperty (_ReflectionColor, "Reflection Color (RGB)");
        GUI.backgroundColor = originalguicolor;
        EditorGUILayout.EndVertical ();
    }

    void _ () {
        EditorGUILayout.Space ();
    }

    void SetKeyword (string keyword, bool state) {
        if (state) material.EnableKeyword (keyword);
        else material.DisableKeyword (keyword);
    }

    QuickSetup Quicksetup (QuickSetup setup) {
        switch (setup) {
            case QuickSetup.Metallic:
                _ReflectionIntensity.floatValue = 1f;
                _Metallic.floatValue = 0.75f;
                _Glossiness.floatValue = 0.5f;
                _PaintFresnelScale.floatValue = 0f;
                _ReflectionIntensity.floatValue = 1f;
                _ReflectionBlur.floatValue = 0.5f;
                break;
            case QuickSetup.Gloss:
                _Metallic.floatValue = 0.5f;
                _Glossiness.floatValue = 0.5f;
                _PaintFresnelScale.floatValue = 0f;
                _ReflectionIntensity.floatValue = 0.5f;
                _ReflectionBlur.floatValue = 1f;
                break;
            case QuickSetup.Matte:
                _Metallic.floatValue = 0.5f;
                _Glossiness.floatValue = 0.25f;
                _PaintFresnelScale.floatValue = 0f;
                _ReflectionIntensity.floatValue = 0.5f;
                _ReflectionBlur.floatValue = 2f;
                break;
            case QuickSetup.Plastic:
                _Metallic.floatValue = 0.0f;
                _Glossiness.floatValue = 0.25f;
                _PaintFresnelScale.floatValue = 0f;
                _ReflectionIntensity.floatValue = 0.1f;
                _ReflectionBlur.floatValue = 5f;
                break;
            case QuickSetup.Pearlescent:
                _Metallic.floatValue = 0.5f;
                _Glossiness.floatValue = 0.5f;
                _PaintFresnelScale.floatValue = 1f;
                _ReflectionIntensity.floatValue = 0.2f;
                _ReflectionBlur.floatValue = 1f;
                break;
        }

        return QuickSetup.Select;
    }
}