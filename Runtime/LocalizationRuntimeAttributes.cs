using System;
using JetBrains.Annotations;
using UnityEngine;

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

    /// <summary>
    /// Use localized name for the property name in the Inspector.
    /// If you want to combine with other PropertyDrawer (e.g. <see cref="RangeAttribute"/>),
    /// Please put this attribute at the top of PropertyDrawer.
    /// </summary>
#if COM_ANATAWA12_CUSTOM_LOCALIZATION_FOR_EDITOR_EXTENSION_AS_PACKAGE
    public
#else
    internal
#endif
        // ReSharper disable once InconsistentNaming
        sealed class CL4EELocalizedAttribute : PropertyAttribute
    {
        [NotNull] public string LocalizationKey { get; }
        [CanBeNull] public string TooltipKey { get; }

        public CL4EELocalizedAttribute([NotNull] string localizationKey) : this(localizationKey, null) {}

        public CL4EELocalizedAttribute([NotNull] string localizationKey, [CanBeNull] string tooltipKey)
        {
            LocalizationKey = localizationKey ?? throw new ArgumentNullException(nameof(localizationKey));
            TooltipKey = tooltipKey;
        }
    } 
}
