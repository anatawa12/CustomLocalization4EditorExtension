using System;
using CustomLocalization4EditorExtension;
using UnityEngine;

namespace CustomLocalization4EditorExtension.Sample.EditorOnly
{
    [AddComponentMenu("CL4EE Samples/Runtime Only First Sample Component")]
    public class CL4EERuntimeOnlyFirstSampleComponent : MonoBehaviour
    {
#if UNITY_EDITOR
        // Define the localization instance for this assembly
        // Since most API of CL4EE are available only in Editor, you have to wrap with #if UNITY_EDITOR
        [AssemblyCL4EELocalization]
        private static Localization Localization { get; } = new Localization("17afd332a3d34cb29749601c70977f1e", "en");
#endif

        // To draw language picker in the inspector, you can use CL4EELocalePicker attribute.
        // The argument of this attribute is the type of the component.
        [CL4EELocalePicker(typeof(CL4EERuntimeOnlyFirstSampleComponent))]
        // To change the label of the field, you can use CL4EELocalized attribute.
        [CL4EELocalized("prop:testField")]
        public string testField;

        // You can combine CL4EELocalized attribute with other property drawer attributes
        // like RangeAttribute by putting CL4EELocalized before the other attribute.
        [CL4EELocalized("prop:testRangedField")]
        [Range(0, 100)]
        public float testRangedField;

        // Of couse, you can change the label of struct field.
        [CL4EELocalized("prop:testComplexField")]
        public SomeStruct testComplexField;

        public SomeStruct nonLocalizedStruct;

        [Serializable]
        public class SomeStruct
        {
            // Also, you can use CL4EELocalized attribute for fields in a struct.
            [CL4EELocalized("prop:inStruct")]
            public string inStruct;

            public int nonLocalized;
        }
    }
}
