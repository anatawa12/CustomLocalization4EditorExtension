using System;
using CustomLocalization4EditorExtension;
using UnityEditor;
using UnityEngine.UIElements;

namespace CustomLocalization4EditorExtension.Sample.EditorOnly
{
    public class UIElementsWindow : EditorWindow
    {
        [MenuItem("CustomLocalization4EditorExtension/Samples/UI Elements")]
        public static void Open()
        {
            GetWindow<UIElementsWindow>();
        }

        // Define the main localization instance for this assembly.
        [AssemblyCL4EELocalization]
        private static Localization Localization { get; } = new Localization("1ba8d62d53d5485d929cc3e1133187bb", "en");

        private void CreateGUI()
        {
            rootVisualElement.Clear();
            // UI Elements support in CL4EE is relatively small.

            // First, you can create DropdownField for language selection.
            var dropdown = new DropdownField();
            CL4EE.MountLanguagePicker(dropdown);
            //Localization.MountLanguagePicker(dropdown);
            rootVisualElement.Add(dropdown);

            // You can localize labels and other UI elements using CL4EE.Tr method.
            var label = new Label(CL4EE.Tr("some label"));
            // Or you can use Localization instance directly.
            // var label = new Label(Localization.Tr("prop:key"));
            rootVisualElement.Add(label);

            // You have to update the label text manually when selected language changes.
            dropdown.RegisterValueChangedCallback(_ =>
            {
                label.text = CL4EE.Tr("some label");
            });
        }

        // For UI Elements created as VisualTreeAsset or UXML, you can clone tree and localize binded elements.
        // Since how localization key should be defined in UXML is very dependent on the project,
        // this method is not provided as a part of CL4EE API. you can copy this method and customize it for your project.
        //
        // For example, this code and this method does not support localizing text labels.
        //
        // Original License:
        // Copyright (c) 2025 Ram.Type-0
        // MIT License
        // https://github.com/RamType0/Meshia.MeshSimplification/blob/91b9cce30255235be6b7c76e505d4c2475007d00/Editor/Localization/LocalizationProvider.cs#L16-L48
        
        private void CreateGUIFromAsset()
        {
            var visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(AssetDatabase.GUIDToAssetPath("Your asst GUID here"));

            var root = visualTreeAsset.CloneTree();

            var languagePicker = root.Q<DropdownField>("LanguagePicker");

            LocalizeBindedElements<UIElementsWindow>(root);
            CL4EE.MountLanguagePicker(languagePicker);

            languagePicker.RegisterValueChangedCallback(evt => LocalizeBindedElements<UIElementsWindow>(root));
        }

        public static void LocalizeBindedElements<T>(VisualElement root)
        {
            var typeName = typeof(T).FullName;
            root.Query().OfType<BindableElement>().Where(bindableElement => !string.IsNullOrEmpty(bindableElement.bindingPath))
                .ForEach(bindableElement =>
                {
                    if (Localization.TryTr($"{typeName}.{bindableElement.bindingPath}.label") is { } translatedLabel)
                    {
                        switch (bindableElement)
                        {
                            case Toggle toggle:
                            {
                                toggle.label = translatedLabel;
                            }
                                break;
                            case FloatField floatField:
                            {
                                floatField.label = translatedLabel;
                            }
                                break;
                            case Slider slider:
                            {
                                slider.label = translatedLabel;
                            }
                                break;
                        }
                    }
                    if (Localization.TryTr($"{typeName}.{bindableElement.bindingPath}.tooltip") is { } translatedTooltip)
                    {
                        bindableElement.tooltip = translatedTooltip;
                    }
                });
        }
    }
}
