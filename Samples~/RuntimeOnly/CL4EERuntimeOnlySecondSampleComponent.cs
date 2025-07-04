using System;
using CustomLocalization4EditorExtension;
using UnityEngine;

namespace CustomLocalization4EditorExtension.Sample.EditorOnly
{
    [AddComponentMenu("CL4EE Samples/Runtime Only Second Sample Component")]
    public class CL4EERuntimeOnlSecondSampleComponent : MonoBehaviour
    {
        // Since AssemblyCL4EELocalization is defined in the first component,
        // you should not define it again in this class.
        
        // You still can use CL4EELocalePicker and CL4EELocalized attributes in this class normaly.

        [CL4EELocalePicker(typeof(CL4EERuntimeOnlyFirstSampleComponent))]
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
            // Also, you can use CL4EELocalized attribute for fields in a struct.
            [CL4EELocalized("prop:inStruct")]
            public string inStruct;

            public int nonLocalized;
        }
    }
}
