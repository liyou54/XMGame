#nullable enable
using System;
using System.Collections.Generic;
using System.Reflection;
using XM.Contracts;
using XM.Contracts.Config;

public class SortedModConfig
{
    public ModConfig ModConfig { get; }

    public bool IsEnabled { get; }

    public  SortedModConfig(ModConfig modConfig, bool isEnabled)
    {
        ModConfig = modConfig;
        IsEnabled = isEnabled;
    }

}

public class ModConfig
{
    public string ModName { get; set; }
    public string DllPath { get; set; }
    public string Author { get; set; }
    public string Description { get; set; }
    public string Version { get; set; }
    public string IconPath { get; set; }
    public string HomePageLink { get; set; }
    public string ImagePath { get; set; }
    public string PackageName { get; set; }
    
    public string AssetName { get; set; }

    public List<string> ConfigFiles { get; set; }

    public ModConfig(
        string modName,
        string version,
        string author,
        string description,
        string dllPath,
        string packageName,
        List<string> configFiles, 
        string assetName,
        string iconPath = "",
        string homePageLink = "",
        string imagePath = "")
    {
        ModName = modName ?? "";
        Version = version ?? "1.0.0";
        Author = author ?? "";
        Description = description ?? "";
        DllPath = dllPath ?? "";
        PackageName = packageName ?? modName ?? "";
        ConfigFiles = configFiles ?? new List<string>();
        AssetName = assetName ?? "";
        IconPath = iconPath ?? "";
        HomePageLink = homePageLink ?? "";
        ImagePath = imagePath ?? "";
    }
}

public class ModRuntime
{
    public ModS ModS { get; set; }
    public ModConfig Config { get; set; }

    public ModBase? ModEntry { get; set; }
    public Assembly? Assembly { get; set; }

    public List<Type> ConfigDefineTypes { get; set; }  = new List<Type>();
    
    public  ModRuntime(ModS modKey,  ModConfig config,Assembly? assembly, ModBase? modEntry)
    {
        ModS = modKey;
        Assembly = assembly;
        Config = config;
        ModEntry = modEntry;
    }

    public void AddConfigDefine(IEnumerable<Type> types)
    {
        ConfigDefineTypes.AddRange(types);
    }
}