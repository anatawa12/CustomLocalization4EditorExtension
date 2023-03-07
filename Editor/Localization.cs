#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace CustomLocalization4EditorExtension
{
    // as a package, this is public, as a embed library, public to embedder library so mark as internal
#if COM_ANATAWA12_CUSTOM_LOCALIZATION_FOR_EDITOR_EXTENSION_AS_PACKAGE
    public
#else
    internal
#endif
        sealed class Localization
    {
        // constructor configurations
        [NotNull] private readonly string _inputAssetPath;
        [NotNull] private readonly string _defaultLocaleName;

        // the if this is not null, we found at lease one localization asset(s).
        [CanBeNull] private LocaleAssetConfig _config;
        private bool _initialized;

        public string CurrentLocaleCode
        {
            [CanBeNull] get => _config?.CurrentLocaleCode;
            [NotNull]
            set
            {
                if (_config?.TrySetLocale(value) == false)
                {
                    throw new ArgumentException($"locale {value} not found", nameof(value));
                }
            }
        }

        /// <summary>
        /// Instantiate Localization
        /// </summary>
        /// <param name="localeAssetPath">
        /// The asset path or GUID of <code cref="LocalizationAsset">LocalizationAsset</code> or
        /// the asset path to directory contains <code cref="LocalizationAsset">LocalizationAsset</code>s.
        /// </param>
        /// <param name="defaultLocale">The fallback locale of localization.</param>
        /// <exception cref="ArgumentException">Both keyLocale and defaultLocale are null</exception>
        public Localization(
            [NotNull] string localeAssetPath,
            [NotNull] string defaultLocale)
        {
            _inputAssetPath = localeAssetPath;
            _defaultLocaleName = defaultLocale;
        }

        /// <summary>
        /// Initializes the localization.
        /// This function will access assets so you should not call this in your constructor.
        /// Calling this function is not required.　If you don't call this manually,
        /// The first call of <c cref="Tr">Tr</c> will call this function.
        /// </summary>
        public void Setup()
        {
            var currentLocaleCode = _config?.CurrentLocaleCode;
            _initialized = true;

            // reset
            _config = null;

            // find directories
            string directoryPath = null;

            if (_inputAssetPath.EndsWith("/"))
            {
                // it's asset path to folder
                if (Directory.Exists(_inputAssetPath))
                    directoryPath = _inputAssetPath;
            }
            else if (IsGuid(_inputAssetPath))
            {
                // it's GUID to folder or LocalizationAsset
                var path = AssetDatabase.GUIDToAssetPath(_inputAssetPath);
                if (Directory.Exists(path))
                    directoryPath = path;
                else if (File.Exists(path))
                    directoryPath = Path.GetDirectoryName(path);
            }
            else
            {
                // it's asset path to LocalizationAsset
                if (File.Exists(_inputAssetPath))
                    directoryPath = Path.GetDirectoryName(_inputAssetPath);
            }

            if (directoryPath == null)
            {
                Debug.LogError($"inputAssetPath not found: {_inputAssetPath}");
                return;
            }

            try
            {
                var locales = Directory.GetFiles($"{directoryPath}")
                    .Select(AssetDatabase.LoadAssetAtPath<LocalizationAsset>)
                    .Where(asset => asset != null)
                    .OrderBy(asset => asset.localeIsoCode)
                    .Select((asset, i) => new LocaleInfo(asset, i))
                    .ToArray();

                if (locales.Length == 0)
                {
                    Debug.LogError($"no locales found at {directoryPath}");
                    return;
                }

                _config = new LocaleAssetConfig(locales, _defaultLocaleName, currentLocaleCode);
            }
            catch (IOException e)
            {
                Debug.LogError(e);
            }
            catch (AggregateException e)
            {
                Debug.LogError($"locale duplicated: {e}");
            }
        }

        /// <summary>
        /// Translate the specified text via this Localization setting
        /// </summary>
        /// <param name="key">
        /// The untranslated text. This should be english but can be changed via keyLocale of constructor.
        /// </param>
        /// <returns>The translated text or translation key</returns>
        [NotNull]
        public string Tr([NotNull] string key) => TryTr(key) ?? key;

        /// <summary>
        /// Translate the specified text via this Localization setting
        /// </summary>
        /// <param name="key">
        /// The untranslated text. This should be english but can be changed via keyLocale of constructor.
        /// </param>
        /// <returns>The translated text or null</returns>
        [CanBeNull]
        public string TryTr([NotNull] string key)
        {
            if (!_initialized)
                Setup();

            return _config?.TryGetLocalizedString(key);
        }

        public void DrawLanguagePicker()
        {
            if (_config == null)
            {
                EditorGUILayout.Popup(0, new[] { "No Locale Available" });
                return;
            }

            _config.DrawLanguagePicker();
        }

        #region utilities

        private static bool IsGuid([NotNull] string mayGuid)
        {
            int hexCnt = 0;
            foreach (var c in mayGuid)
            {
#pragma warning disable CS0642
                if ('0' <= c && c <= '9') hexCnt++;
                else if ('a' <= c && c <= 'f') hexCnt++;
                else if ('A' <= c && c <= 'F') hexCnt++;
                else if (c == '-') ;
                else return false;
#pragma warning restore CS0642

                if (hexCnt > 32) return false;
            }

            return hexCnt == 32;
        }

        #endregion

        class LocaleAssetConfig
        {
            [NotNull] private readonly LocaleInfo[] _locales;
            [NotNull] private readonly string[] _localeNameList;
            [CanBeNull] private readonly LocaleInfo _defaultLocale;
            [NotNull] private LocaleInfo _currentLocale;

            public LocaleAssetConfig(
                [NotNull] LocaleInfo[] locales,
                [NotNull] string defaultLocaleName,
                [CanBeNull] string currentLocaleCode)
            {
                if (locales.Length == 0)
                    throw new ArgumentException("empty", nameof(locales));
                _locales = locales;
                _localeNameList = _locales.Select(id => id.Name).ToArray();
                _defaultLocale = _locales.FirstOrDefault(loc => loc.Asset.localeIsoCode == defaultLocaleName);
                if (_defaultLocale == null)
                    Debug.LogError($"locale asset for default locale({defaultLocaleName}) not found.");

                if (currentLocaleCode == null)
                {
                    // previous locale is not exists. use default
                    _currentLocale = _defaultLocale ?? _locales[0];
                }
                else if (!TrySetLocale(currentLocaleCode))
                {
                    Debug.LogWarning($"locale not found: {currentLocaleCode}");
                    _currentLocale = _defaultLocale ?? _locales[0];
                }
            }

            public string CurrentLocaleCode => _currentLocale.Asset != null ? _currentLocale.Asset.localeIsoCode : null;

            public bool TrySetLocale(string localeCode)
            {
                var locale = _locales.FirstOrDefault(loc => loc.Asset.localeIsoCode == localeCode);
                if (locale == null) return false;

                _currentLocale = locale;

                return true;
            }

            public void DrawLanguagePicker()
            {
                int newIndex = EditorGUILayout.Popup(_currentLocale.Index, _localeNameList);
                if (newIndex == _currentLocale.Index) return;

                _currentLocale = _locales[newIndex];
            }

            public string TryGetLocalizedString(string key)
            {
                return _currentLocale.TryGetLocalizedString(key) ?? _defaultLocale?.TryGetLocalizedString(key);
            }
        }

        class LocaleInfo
        {
            public readonly LocalizationAsset Asset;
            public readonly int Index;
            public readonly string Name;

            public LocaleInfo(LocalizationAsset asset, int index)
            {
                Asset = asset;
                Index = index;
                Name = TryGetLocalizedString($"locale:{asset.localeIsoCode}") ?? DefaultLocaleName(asset.localeIsoCode);
            }

            [CanBeNull]
            public string TryGetLocalizedString(string key)
            {
                string localized;
                return Asset != null && (localized = Asset.GetLocalizedString(key)) != key ? localized : null;
            }

            private static readonly Dictionary<string, string> FallbackLocaleNames = new Dictionary<string, string>();

            private static string DefaultLocaleName(string code)
            {
                if (FallbackLocaleNames.TryGetValue(code, out var name)) return name;

                name = new CultureInfo(code).EnglishName;
                FallbackLocaleNames[code] = name;

                return name;
            }
        }
    }

#if COM_ANATAWA12_CUSTOM_LOCALIZATION_FOR_EDITOR_EXTENSION_AS_PACKAGE
    public
#else
    internal
#endif
        static class L10N
    {
        private static readonly Dictionary<Assembly, Func<Localization>> LocalizationGetter =
            new Dictionary<Assembly, Func<Localization>>();

        /// <returns>Localization instance for caller assembly</returns>
        [CanBeNull]
        public static Localization GetLocalization() => GetLocalization(Assembly.GetCallingAssembly());

        [CanBeNull]
        public static Localization GetLocalization([NotNull] Assembly assembly) => GetLocalizationGetter(assembly)?.Invoke();

        [NotNull]
        public static string Tr([NotNull] string localizationKey) =>
            GetLocalization(Assembly.GetCallingAssembly())?.Tr(localizationKey) ?? localizationKey;

        [CanBeNull]
        private static Func<Localization> GetLocalizationGetter([NotNull] Assembly assembly)
        {
            if (LocalizationGetter.TryGetValue(assembly, out var getter))
                return getter;

            if (IsDisallowedAssemblyName(assembly.GetName().Name))
            {
                Debug.LogError("Getting Assembly for unity default assemblies are not allowed.");
                return null;
            }

            if (assembly.GetCustomAttribute<RedirectCL4EEInstanceAttribute>() is RedirectCL4EEInstanceAttribute attr)
            {
                Assembly redirectTo = null;
                try
                {
                    redirectTo = Assembly.Load(attr.RedirectToName);
                }
                catch (Exception e)
                {
                    Debug.LogError(
                        $"Unable to load redirected assembly for {assembly.GetName().Name} ({attr.RedirectToName}).");
                    Debug.LogException(e);
                }

                if (redirectTo != null)
                    return GetLocalizationGetter(redirectTo);
            }

            const BindingFlags bindingFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

            var types = assembly.GetTypes();

            var properties = types.SelectMany(x => x.GetProperties(bindingFlags))
                .Where(x => x.PropertyType == typeof(Localization))
                .Where(x => x.GetMethod != null)
                .Where(x => x.GetCustomAttribute<AssemblyCL4EELocalizationAttribute>() != null)
                .ToArray();

            if (properties.Length == 0)
            {
                Debug.LogError($"Static property with AssemblyLocalizationInstanceAttribute not found for {assembly}");
                getter = null;
            }
            else
            {
                if (properties.Length != 1)
                {
                    Debug.LogError(
                        $"Multiple static property with AssemblyLocalizationInstanceAttribute not found for {assembly}! First one will be used");
                }

                var propertyInfo = properties[0];
                var getterMethod = propertyInfo.GetMethod;

                getter = (Func<Localization>)Delegate.CreateDelegate(typeof(Func<Localization>), getterMethod);
            }

            LocalizationGetter[assembly] = getter;

            return getter;
        }

        // default assembly names are not allowed.
        // https://docs.unity3d.com/ja/2019.4/Manual/ScriptCompileOrderFolders.html
        private static bool IsDisallowedAssemblyName(string name)
        {
            switch (name)
            {
                case "Assembly-CSharp-firstpass":
                case "Assembly-CSharp-Editor-firstpass":
                case "Assembly-CSharp":
                case "Assembly-CSharp-Editor":
                    return true;
                default:
                    return false;
            }
        }
    }

    /// <summary>
    /// Specifies the Localization instance for that assembly.
    /// You can use specified instance using <see cref="L10N"/> class or Cl4EeLocalizedAttribute (to be implemented)
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
#if COM_ANATAWA12_CUSTOM_LOCALIZATION_FOR_EDITOR_EXTENSION_AS_PACKAGE
    public
#else
    internal
#endif
        // ReSharper disable once InconsistentNaming
        sealed class AssemblyCL4EELocalizationAttribute : Attribute
    {
        
    }
}
#endif 
