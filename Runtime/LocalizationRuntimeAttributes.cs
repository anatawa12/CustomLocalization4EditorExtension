using System;

namespace CustomLocalization4EditorExtension
{
    /// <summary>
    /// Use CL4EE Localization instance of other Assembly for this assembly.
    /// You can share CL4EE Localization instance between multiple assembly using this attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly)]
#if COM_ANATAWA12_CUSTOM_LOCALIZATION_FOR_EDITOR_EXTENSION_AS_PACKAGE
    public
#else
    internal
#endif
        // ReSharper disable once InconsistentNaming
        sealed class RedirectCL4EEInstanceAttribute : Attribute
    {
        public string RedirectToName { get; }

        public RedirectCL4EEInstanceAttribute(string redirectToName)
        {
            RedirectToName = redirectToName;
        }
    }
}
