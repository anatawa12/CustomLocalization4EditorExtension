using CustomLocalization4EditorExtension;
using UnityEditor;

namespace CustomLocalization4EditorExtension.Sample.EditorOnly
{
    public class LocalizationTest : EditorWindow
    {
        [MenuItem("CustomLocalization4EditorExtension/LocalizationTest EditorOnly")]
        public static void Open()
        {
            GetWindow<LocalizationTest>();
        }

        [AssemblyCL4EELocalization]
        private static Localization Localization { get; } = new Localization("9d07519d86254715b7e730d6731520b2", "en");
        private string _input;

        private void OnGUI()
        {
            CL4EE.DrawLanguagePicker();
            _input = EditorGUILayout.TextField(CL4EE.Tr("prop:key"), _input);
            if (_input != null)
            {
                EditorGUILayout.LabelField(CL4EE.Tr(_input));
            }
        }
    }
}
