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
            L10N.DrawLanguagePicker();
            _input = EditorGUILayout.TextField(L10N.Tr("prop:key"), _input);
            if (_input != null)
            {
                EditorGUILayout.LabelField(L10N.Tr(_input));
            }
        }
    }
}
