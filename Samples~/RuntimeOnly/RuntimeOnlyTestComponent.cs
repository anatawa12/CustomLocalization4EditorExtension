using System;
using CustomLocalization4EditorExtension;
using UnityEngine;

namespace CustomLocalization4EditorExtension.Sample.EditorOnly
{
    public class RuntimeOnlyTestComponent : MonoBehaviour
    {
#if UNITY_EDITOR
        [AssemblyCL4EELocalization]
        private static Localization Localization { get; } = new Localization("17afd332a3d34cb29749601c70977f1e", "en");
#endif

        [CL4EELocalePicker(typeof(RuntimeOnlyTestComponent))]
        [CL4EELocalized("prop:testField")]
        public string testField;

        [CL4EELocalized("prop:testRangedField")]
        [Range(0, 100)]
        public float testRangedField;

        [CL4EELocalized("prop:testComplexField")]
        public SomeStruct testComplexField;

        public SomeStruct nonLocalizedStruct;

        [Serializable]
        public class SomeStruct
        {
            [CL4EELocalized("prop:inStruct")]
            public string inStruct;

            public int nonLocalized;
        }
    }
}
