using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace CustomLocalization4EditorExtension
{
    public class Localization
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
#pragma warning disable CS642
                if ('0' <= c && c <= '9') hexCnt++;
                else if ('a' <= c && c <= 'f') hexCnt++;
                else if ('A' <= c && c <= 'F') hexCnt++;
                else if (c == '-') ;
                else return false;
#pragma warning restore CS642

                if (hexCnt > 32) return false;
            }

            return hexCnt == 32;
        }

        #endregion
    }

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
