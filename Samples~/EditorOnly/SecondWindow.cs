using CustomLocalization4EditorExtension;
using UnityEditor;

namespace CustomLocalization4EditorExtension.Sample.EditorOnly
{
    public class SecondWindow : EditorWindow
    {
        [MenuItem("CustomLocalization4EditorExtension/Samples/EditorOnly/Second Window")]
        public static void Open()
        {
            GetWindow<SecondWindow>();
        }

        private string _input;

        // since [AssemblyCL4EELocalization] is defined in the FirstWindow class,
        // you should not define it again in this class.

        private void OnGUI()
        {
            // With CL4EE class, you can access the [AssemblyCL4EELocalization] instance from this class.
            CL4EE.DrawLanguagePicker();
            _input = EditorGUILayout.TextField(CL4EE.Tr("prop:key"), _input);
            if (_input != null)
            {
                EditorGUILayout.LabelField(CL4EE.Tr(_input));
            }
        }
    }
}
