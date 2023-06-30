using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.IO;
using System;

namespace CityBuilderCore.Editor
{
    public class SetupWindow : EditorWindow
    {
        [MenuItem("Window/CityBuilder/Setup")]
        public static void ShowWindow()
        {
            GetWindow<SetupWindow>().Show();
        }

        private SetupModel _model = new SetupModel();

        private void OnEnable()
        {
            titleContent = new GUIContent("CCBK Setup");

            var scriptPath = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this));

            var style = AssetDatabase.LoadAssetAtPath<StyleSheet>(Path.ChangeExtension(scriptPath, "uss"));
            var layout = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Path.ChangeExtension(scriptPath, "uxml"));

            var root = rootVisualElement;

            root.styleSheets.Add(style);

            layout.CloneTree(root);

            initializeElements(root);
        }

        private void initializeElements(VisualElement root)
        {
            initializeField("DirectoryField", _model.Directory, v => _model.Directory = v);

            initializeEnum("CityDisplayField", _model.CityDisplay, v => _model.CityDisplay = v);
            initializeEnum("MapDisplayField", _model.MapDisplay, setMapDisplay);
            initializeEnum("MapLayoutField", _model.MapLayout, v => _model.MapLayout = v);
            initializeEnum("MapAxisField", _model.MapAxis, v => _model.MapAxis = v);

            initializeField("MapSizeField", _model.MapSize, v => _model.MapSize = v);
            initializeField("ScaleField", _model.Scale, v => _model.Scale = v);

            root.Q<Button>("GenerateButton").clicked += generate;
        }

        private void setMapDisplay(SetupModel.MapDisplayMode mode)
        {
            _model.MapDisplay = mode;

            var mapAxis = rootVisualElement.Q<EnumField>("MapAxisField");

            if (_model.MapDisplay == SetupModel.MapDisplayMode.Terrain)
            {
                mapAxis.value = SetupModel.MapAxisMode.XZ;
                mapAxis.SetEnabled(false);
            }
            else
            {
                mapAxis.SetEnabled(true);
            }
        }

        private BaseField<T> initializeField<T>(string name, T value, Action<T> callback)
        {
            var field = rootVisualElement.Q<BaseField<T>>(name);
            field.value = value;
            field.RegisterValueChangedCallback(evt => callback(evt.newValue));
            return field;
        }

        private EnumField initializeEnum<T>(string name, T value, Action<T> callback) where T : Enum
        {
            var enumField = rootVisualElement.Q<EnumField>(name);
            enumField.Init(value);
            initializeField(name, (Enum)value, v => callback((T)v));
            return enumField;
        }

        private void generate()
        {
            SetupGenerator.Instance.Execute(_model);
        }
    }
}