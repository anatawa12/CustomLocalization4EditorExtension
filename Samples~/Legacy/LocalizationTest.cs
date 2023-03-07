using CustomLocalization4EditorExtension;
using UnityEditor;

namespace CustomLocalization4EditorExtension.Sample.Legacy
{
    public class LocalizationTest : EditorWindow
    {
        [MenuItem("CustomLocalization4EditorExtension/LocalizationTest Legacy")]
        public static void Open()
        {
            GetWindow<LocalizationTest>();
        }

        private Localization _local = new Localization("d5c0fdcf0b524fb6ba5332337e2eecfb", "en");
        private string _input;

        private void OnGUI()
        {
            _local.DrawLanguagePicker();
            _input = EditorGUILayout.TextField("key", _input);
            if (_input != null)
            {
                EditorGUILayout.LabelField(_local.Tr(_input));
            }
        }

        private void OnEnable()
        {
            _local.Setup();
        }
    }
}
