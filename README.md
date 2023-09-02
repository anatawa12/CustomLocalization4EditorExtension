Custom Localization for Editor Extension
===

A experimental library for Unity extensions which want to have custom locale for translation instead of unity's locale. 

## What's this?

In Japan, it also be general to use English for unity's locale because many useful information are on foreign countries and they uses English.
But for tools by Japanese, We may use Japanese because documentation may use Japanese.
So, I make a localization library to choose locale for each editor extension.

## How to install

### As a OpenUPM Package

This package is published on [openupm]. Please refer [project page on openupm][openupm-pkg] for installation step.

### As a VPM Package

This package is also published as [vpm package] on anatawa12's Repository at [`https://vpm.anatawa12.com/vpm.json`][vpm-repo].

By adding the Repository to your VPM configuration, you can use this library.

```sh
# add my vpm repository
vpm add repo https://vpm.anatawa12.com/vpm.json
# add package to your project
cd /path/to/your-unity-project
vpm add package com.anatawa12.custom-localization-for-editor-extension
```

### As a git UPM Package

You can add `https://github.com/anatawa12/CustomLocalization4EditorExtension.git#<version>` as git-based UPM dependencies.

### As a embedded library

Since v0.2.2, you can embed this library as a part of your tool. Please clone this project as submodule to non-Unity-tracked folder (e.g. suffixed by `~`) and symlink
`Runtime/CustomLocalization4EditorExtension.Runtime.cs` and `Editor/CustomLocalization4EditorExtension.Editor.cs` into your project.

If your tool is made only with either Runtime or Editor module, please symlink both into your your module.

If your tool have both Runtime and Editor module, please symlink `Runtime/CustomLocalization4EditorExtension.Runtime.cs` into your Runtime module and `Editor/CustomLocalization4EditorExtension.Editor.cs` to Editor one. 
And then please make internals members of Runtime module visible to Editor module using [`InternalsVisibleTo`] attribute.

Please **DO NOT** define `COM_ANATAWA12_CUSTOM_LOCALIZATION_FOR_EDITOR_EXTENSION_AS_PACKAGE` in your project defines.
When you define that, CL4EE will be compiled as public library mode, which is bad as a embedded library.
Without that define, CL4EE will be compiled as embedded library mode, which every public API of CL4EE will marked as `internal` 
so any external users cannot use CL4EE as a part of your library/tool.

## How to use

1. Install this tool (see [How to install](#how-to-install))
2. If your installation is not as a embedded library, add `com.anatawa12.custom-localization-for-editor-extension` (editor, main module) and `com.anatawa12.custom-localization-for-editor-extension.runtime` (atrributes to be used in MonoBehaviours)
to your assembly definition. It's very highly recommend you to use AssemblyDefinition but if you have some special circumstances, you can use [Assembly Definition Reference][asmref] to add to `Assembly-CSharp` and/or `Assembly-CSharp-Editor`.
3. Create one folder for localization files.
4. If your code consists of Editor and Runtime, Please add `[assembly:RedirectCL4EEInstance("Editor assembly name")]` attribute to your Runtime assembly and please do step 5 & 6 in Editor assembly.
5. Create static property of `Localization` in your tool and add `AssemblyCL4EELocalization` attribute.
6. Initialize that property with GUID or Path with '/' suffix to the folder for localization files as the first parameter and ISO code of default locale as the second parameter. 
I recommend you to use GUID because it doesn't change if tool users changed location of that tool.
7. Your code should be like below in step 5. & 6.
    ```c#
    using CustomLocalization4EditorExtension;

    [AssemblyCL4EELocalization]
    private static Localization Localization { get; } = new Localization("53b154bd853b4ecc893fc10c47694084", "ja");
    // or
    private static Localization Localization { get; } = new Localization("Assets/YourTool/Localization/", "ja");
    ```
8. Create [LocalizationAsset]s in the folder. The format of localization asset is [po format in gettext][po-gettext].

    Notice: Due to Unity API's limit & internal implementation of CL4EE, you should not make un-localized string same as localized string.
    It means localized string is not defined in CL4EE.

    Notice: Due to [Bug in Unity Editor 2022.3 or later][unity-bug], you should place assembly definition file in folder for LocalizationAssets.
10. Preparation is done! You can use `CL4EE.Tr("localization key")` to get localized text, add `[CL4EELocalized("localization key")]` attribute to localize property name in inspector, 
`CL4EE.DrawLanguagePicker()` or `[CL4EELocalePickerAttribute(typeof(your class))]` to draw Language Picker on inspector.

[openupm]: https://openupm.com/
[openupm-pkg]: https://openupm.com/packages/com.anatawa12.custom-localization-for-editor-extension/
[vpm package]: https://vcc.docs.vrchat.com/vpm/packages
[vpm-repo]: https://vpm.anatawa12.com/vpm.json
[`InternalsVisibleTo`]: https://learn.microsoft.com/dotnet/api/system.runtime.compilerservices.internalsvisibletoattribute
[asmref]: https://docs.unity3d.com/Manual/ScriptCompilationAssemblyDefinitionFiles.html#create-asmref
[LocalizationAsset]: https://docs.unity3d.com/ScriptReference/LocalizationAsset.html
[po-gettext]: https://www.gnu.org/software/gettext/manual/html_node/PO-Files.html
[unity-bug] :https://issuetracker.unity3d.com/issues/crash-on-gettargetassemblybyscriptpath-when-a-po-file-in-the-packages-directory-is-not-under-an-assembly-definition
