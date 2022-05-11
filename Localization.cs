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
        [NotNull] private readonly string _inputAssetPath;
        [NotNull] private string _currentLocale;
        [NotNull] private readonly string _defaultLocale;
        [CanBeNull] private Dictionary<string, LocalizationAsset> _locales;
        [CanBeNull] private string[] _localeList;
        [CanBeNull] private string[] _localeNameList;
        [CanBeNull] private LocalizationAsset _currentLocaleAsset;
        [CanBeNull] private LocalizationAsset _defaultLocaleAsset;
        private int _localeIndex;
        private bool _initialized;

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
            [NotNull] string defaultLocale) {
            _inputAssetPath = localeAssetPath;
            _defaultLocale = defaultLocale;
            _currentLocale = _defaultLocale;
        }

        /// <summary>
        /// Initializes the localization.
        /// This function will access assets so you should not call this in your constructor.
        /// Calling this function is not required.　If you don't call this manually,
        /// The first call of <c cref="Tr">Tr</c> will call this function.
        /// </summary>
        public void Setup()
        {
            _initialized = true;
            // first, list-up locales
            _locales = null;
            _localeList = null;
            void LoadDirectory(string pathOfDirectory)
            {
                try
                {
                    _locales = Directory.GetFiles($"{pathOfDirectory}")
                        .Select(AssetDatabase.LoadAssetAtPath<LocalizationAsset>)
                        .Where(asset => asset != null)
                        .ToDictionary(asset => asset.localeIsoCode, asset => asset);
                    if (_locales.Count == 0)
                    {
                        Debug.LogError($"no locales found at {pathOfDirectory}");
                        return;
                    }

                    _localeList = _locales.Keys.ToArray();
                    Array.Sort(_localeList);
                    _localeNameList = _localeList
                        .Select(id => TryGetLocalizedString(_locales[id], $"locale:{id}") ?? DefaultLocaleName(id))
                        .ToArray();
                    SetLocale(_currentLocale);
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

            if (_inputAssetPath.EndsWith("/"))
            {
                // it's asset path to folder
                // folder: UnityEditor.DefaultAsset
                if (Directory.Exists(_inputAssetPath))
                    LoadDirectory(_inputAssetPath);
                else
                    Debug.LogError($"inputAssetPath not found: {_inputAssetPath}");
            }
            else if (IsGuid(_inputAssetPath))
            {
                // it's GUID to folder or LocalizationAsset
                string path = AssetDatabase.GUIDToAssetPath(_inputAssetPath);
                if (Directory.Exists(path))
                    LoadDirectory(path);
                else if (File.Exists(path))
                    LoadDirectory(Path.GetDirectoryName(path));
                else
                    Debug.LogError($"inputAssetPath not found: {_inputAssetPath}");
            }
            else
            {
                // it's asset path to LocalizationAsset
                if (File.Exists(_inputAssetPath))
                    LoadDirectory(Path.GetDirectoryName(_inputAssetPath));
                else
                    Debug.LogError($"inputAssetPath not found: {_inputAssetPath}");
            }

            // then, find current locale
            if (_locales != null)
            {
                _locales.TryGetValue(_currentLocale, out _currentLocaleAsset); 
                _locales.TryGetValue(_defaultLocale, out _defaultLocaleAsset);
                if (_defaultLocaleAsset == null)
                    Debug.LogError($"locale asset for default locale({_defaultLocale}) not found.");
            }
        }

        private void SetLocale(string locale)
        {
            if (_localeList == null)
                throw new InvalidOperationException("_localeList is null but SetLocale was called");
            DoSetLocale(locale);
            System.Diagnostics.Debug.Assert(_localeList != null, nameof(_localeList) + " != null");
            System.Diagnostics.Debug.Assert(_locales != null, nameof(_locales) + " != null");
            _currentLocaleAsset = _locales[_currentLocale];
        }

        private void DoSetLocale(string locale)
        {
            System.Diagnostics.Debug.Assert(_localeList != null, nameof(_localeList) + " != null");
            _localeIndex = Array.IndexOf(_localeList, locale);
            if (_localeIndex == -1)
            {
                Debug.LogWarning($"locale not found: {locale}");
                DoSetLocale(locale == _defaultLocale ? _localeList[0] : _defaultLocale);
                return;
            }
            _currentLocale = locale;
        }

        private static Dictionary<string, string> FallbackLocaleName = new Dictionary<string, string>();
        private static string DefaultLocaleName(string code)
        {
            if (!FallbackLocaleName.TryGetValue(code, out var name))
            {
                name = new CultureInfo(code).EnglishName;
                FallbackLocaleName[code] = name;
            }

            return name;
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

            return (_currentLocaleAsset == null ? null : TryGetLocalizedString(_currentLocaleAsset, key))
                   ?? (_defaultLocaleAsset == null ? null : TryGetLocalizedString(_defaultLocaleAsset, key));
        }

        [CanBeNull]
        private static string TryGetLocalizedString(LocalizationAsset asset, string key)
        {
            string localized;
            return (localized = asset.GetLocalizedString(key)) != key ? localized : null;
        }

        public void DrawLanguagePicker()
        {
            if (_localeNameList == null)
            {
                EditorGUILayout.Popup(0, new []{"No Locale Available"});
                return;
            }

            int newIndex = EditorGUILayout.Popup(_localeIndex, _localeNameList);
            if (newIndex == _localeIndex) return;

            System.Diagnostics.Debug.Assert(_localeList != null, nameof(_localeList) + " != null");
            _localeIndex = newIndex;
            SetLocale(_localeList[newIndex]);
        }

        private static bool IsGuid([NotNull] string mayGuid)
        {
            int hexCnt = 0;
            foreach (var c in mayGuid)
            {
                if ('0' <= c && c <= '9') hexCnt++;
                else if ('a' <= c && c <= 'f') hexCnt++;
                else if ('A' <= c && c <= 'F') hexCnt++;
                else if (c == '-') {}
                else return false;
                if (hexCnt > 32) return false;
            }

            return hexCnt == 32;
        }
    }
}
