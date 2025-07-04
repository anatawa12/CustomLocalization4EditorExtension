using CustomLocalization4EditorExtension;
using UnityEditor;

namespace CustomLocalization4EditorExtension.Sample.EditorOnly
{
    public class FirstWindow : EditorWindow
    {
        [MenuItem("CustomLocalization4EditorExtension/Samples/EditorOnly/First Window")]
        public static void Open()
        {
            GetWindow<FirstWindow>();
        }

        // Define the main localization instance for this assembly.
        [AssemblyCL4EELocalization]
        private static Localization Localization { get; } = new Localization("9d07519d86254715b7e730d6731520b2", "en");
        private string _input;

        private void OnGUI()
        {
            // You can access Localization instance directly
            Localization.DrawLanguagePicker();
            //CL4EE.DrawLanguagePicker();
            // Or use CL4EE class to access the main localization instance
            _input = EditorGUILayout.TextField(Localization.Tr("prop:key"), _input);
            if (_input != null)
            {
                EditorGUILayout.LabelField(CL4EE.Tr(_input));
            }
        }
    }
}
