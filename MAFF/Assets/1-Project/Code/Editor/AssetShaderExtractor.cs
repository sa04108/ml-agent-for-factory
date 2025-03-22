using Codice.Client.BaseCommands;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Merlin
{
    public class AssetShaderExtractor : EditorWindow
    {
        private Vector2 leftScrollPos;
        private Vector2 rightScrollPos;

        // 스플릿 바 관련
        private float splitterPosition = 500f;  // 초기 왼쪽 박스 너비
        private bool isDragging = false;
        private Rect splitterRect;

        private List<Shader> shaders = new();
        private int selectedShaderIndex = -1;
        private string outputPath = $"{Application.dataPath}/shader_properties.json";

        [MenuItem("Tools/Shader Property Extractor")]
        public static void ShowWindow()
        {
            GetWindow<AssetShaderExtractor>("Shader Property Extractor");
        }

        private void OnGUI()
        {
            if (GUILayout.Button("Load Shaders"))
            {
                LoadShaders();
            }

            outputPath = EditorGUILayout.TextField("Output Path", outputPath);
            if (GUILayout.Button("Generate Shader Property JSON"))
            {
                GenerateShaderPropertyJSON();
            }

            GUILayout.BeginHorizontal();

            // 왼쪽 패널: Shader 목록
            GUILayout.BeginVertical("box", GUILayout.Width(splitterPosition));
            leftScrollPos = EditorGUILayout.BeginScrollView(leftScrollPos);

            GUIStyle leftAlignedButton = new GUIStyle(GUI.skin.button);
            leftAlignedButton.alignment = TextAnchor.MiddleLeft;

            for (int i = 0; i < shaders.Count; i++)
            {
                if (GUILayout.Button(shaders[i].name, leftAlignedButton))
                {
                    selectedShaderIndex = i;
                }
            }
            EditorGUILayout.EndScrollView();
            GUILayout.EndVertical();

            // 스플릿바 그리기
            splitterRect = new Rect(splitterPosition + 2, 62, 4, position.height);
            EditorGUI.DrawRect(splitterRect, Color.gray);
            EditorGUIUtility.AddCursorRect(splitterRect, MouseCursor.ResizeHorizontal);
            if (Event.current.type == EventType.MouseDown && splitterRect.Contains(Event.current.mousePosition))
            {
                isDragging = true;
            }
            if (Event.current.type == EventType.MouseUp)
            {
                isDragging = false;
            }
            if (isDragging)
            {
                splitterPosition = Event.current.mousePosition.x - 4;
                // 최소 및 최대 너비 제한 (예: 최소 100, 최대 전체 너비 - 100)
                splitterPosition = Mathf.Clamp(splitterPosition, 100, position.width - 100);
                Repaint();
            }

            // 오른쪽 패널: 선택된 Shader의 Property 목록
            GUILayout.BeginVertical("box");
            rightScrollPos = EditorGUILayout.BeginScrollView(rightScrollPos);
            if (selectedShaderIndex >= 0 && selectedShaderIndex < shaders.Count)
            {
                Shader selectedShader = shaders[selectedShaderIndex];
                EditorGUILayout.LabelField("Shader:", selectedShader.name);
                int propertyCount = ShaderUtil.GetPropertyCount(selectedShader);
                for (int i = 0; i < propertyCount; i++)
                {
                    string propName = ShaderUtil.GetPropertyName(selectedShader, i);
                    ShaderUtil.ShaderPropertyType propType = ShaderUtil.GetPropertyType(selectedShader, i);
                    if (propType == ShaderUtil.ShaderPropertyType.Range)
                    {
                        float min = ShaderUtil.GetRangeLimits(selectedShader, i, 1);
                        float max = ShaderUtil.GetRangeLimits(selectedShader, i, 2);

                        EditorGUILayout.LabelField($"Property {i}:", $"{propName}, [{min} ~ {max}]");
                    }
                    else
                    {
                        EditorGUILayout.LabelField($"Property {i}:", $"{propName}, [{propType}]");
                    }
                }
            }
            else
            {
                EditorGUILayout.LabelField("No Shader Selected");
            }
            EditorGUILayout.EndScrollView();
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();
        }

        private void LoadShaders()
        {
            shaders.Clear();
            var allPaths = AssetDatabase.GetAllAssetPaths();
            foreach (var path in allPaths)
            {
                Shader shader = (Shader)AssetDatabase.LoadAssetAtPath(path, typeof(Shader));
                if (shader != null)
                {
                    shaders.Add(shader);
                }
            }
            shaders.Sort((a, b) => a.name.CompareTo(b.name));
            Repaint();
        }

        private void GenerateShaderPropertyJSON()
        {
            // 최종 JSON 구조:
            // {
            //   "셰이더 이름": {
            //       "프로퍼티 이름": {
            //           "프로퍼티 타입(Color, Range..)": {
            //               "값1": (float),
            //               "값2": (int)...
            //           }
            //       },
            //       ...
            //   },
            //   ...
            // }
            var shaderData = new Dictionary<string, Dictionary<string, Dictionary<string, object>>>();

            foreach (Shader shader in shaders)
            {
                if (shader == null)
                    continue;

                var propData = new Dictionary<string, Dictionary<string, object>>();
                int propertyCount = ShaderUtil.GetPropertyCount(shader);
                for (int i = 0; i < propertyCount; i++)
                {
                    if (ShaderUtil.GetPropertyType(shader, i) == ShaderUtil.ShaderPropertyType.Range)
                    {
                        float min, max;
                        min = ShaderUtil.GetRangeLimits(shader, i, 1);
                        max = ShaderUtil.GetRangeLimits(shader, i, 2);

                        var rangeDict = new Dictionary<string, float>
                        {
                            { "Min", min },
                            { "Max", max }
                        };
                        var wrapper = new Dictionary<string, object>
                        {
                            { ShaderUtil.ShaderPropertyType.Range.ToString(), rangeDict }
                        };
                        propData.Add(ShaderUtil.GetPropertyName(shader, i), wrapper);
                    }
                    else if (ShaderUtil.GetPropertyType(shader, i) == ShaderUtil.ShaderPropertyType.Color)
                    {
                        var colorDict = new Dictionary<string, Vector4>();
                        var wrapper = new Dictionary<string, object>
                        {
                            { ShaderUtil.ShaderPropertyType.Color.ToString(), colorDict }
                        };
                        propData.Add(ShaderUtil.GetPropertyName(shader, i), wrapper);
                    }
                }
                if (propData.Count > 0)
                {
                    if (!shaderData.ContainsKey(shader.name))
                        shaderData.Add(shader.name, propData);
                }
            }

            // Newtonsoft.Json 을 사용하여 JSON으로 직렬화합니다.
            string json = JsonConvert.SerializeObject(shaderData, Formatting.Indented);
            File.WriteAllText(outputPath, json);
            Debug.Log("Shader property JSON file created at: " + outputPath);
        }
    }

}