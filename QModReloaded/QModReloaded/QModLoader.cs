using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Newtonsoft.Json;

namespace QModReloaded;

public class QModLoader
{
    private static readonly string QModBaseDir = Environment.CurrentDirectory + "\\QMods";

    public static void Patch()
    {
        Logger.WriteLog("Assembly-CSharp.dll has been patched, (otherwise you wouldn't see this message.");
        Logger.WriteLog("Patch method called. Attempting to load mods.");

        var dllFiles = Directory.GetFiles(QModBaseDir, "*.dll", SearchOption.AllDirectories);
        var mods = new List<QMod>();
        foreach (var dllFile in dllFiles)
        {
            var directoryName = new FileInfo(dllFile).DirectoryName;
            var jsonPath = Path.Combine(directoryName!, "mod.json");
            if (!new FileInfo(jsonPath).Exists)
            {
                var created = CreateJson(dllFile);
                if (created)
                {
                    Logger.WriteLog(
                        $"No mod.json found for {dllFile}. Have created one, attempting to load. Note if I couldn't automatically determine the entry method, a best guess is used.");
                    jsonPath = Path.Combine(directoryName, "mod.json");
                }
                else
                {
                    Logger.WriteLog(
                        $"No mod.json found for {dllFile}. Failed to create one automatically. Usually indicates an issue with the DLL.");
                    continue;
                }
            }

            var modToAdd = QMod.FromJsonFile(jsonPath);
            modToAdd.LoadedAssembly = Assembly.LoadFrom(dllFile);
            modToAdd.ModAssemblyPath = dllFile;
            mods.Add(modToAdd);
        }

        mods.Sort((m1, m2) => m1.LoadOrder.CompareTo(m2.LoadOrder));

        foreach (var mod in mods)
        {
            Console.WriteLine($"Load Order: {mod.LoadOrder}, Mod name: {mod.DisplayName}");
            if (mod.Enable)
                LoadMod(mod);
            else
                Logger.WriteLog($"{mod.DisplayName} has been disabled in config. Skipping.");
        }
    }

    private static (string namesp, string type, string method, bool found) GetModEntryPoint(string mod)
    {
        try
        {
            var modAssembly = AssemblyDefinition.ReadAssembly(mod);

            var toInspect = modAssembly.MainModule
                .GetTypes()
                .SelectMany(t => t.Methods
                    .Where(m => m.HasBody)
                    .Select(m => new {t, m}));

            toInspect = toInspect.Where(x => x.m.Name == "Patch");

            foreach (var method in toInspect)
                if (method.m.Body.Instructions.Where(instruction => instruction.Operand != null)
                    .Any(instruction => instruction.Operand.ToString().Contains("PatchAll")))
                    return (method.t.Namespace, method.t.Name, method.m.Name, true);
        }
        catch (Exception ex)
        {
            Logger.WriteLog($"GetModEntryPoint(): Error, {ex.Message}");
        }

        return (null, null, null, false);
    }

    private static bool CreateJson(string file)
    {
        var sFile = new FileInfo(file);
        var path = new FileInfo(file).DirectoryName;
        var fileNameWithoutExt = sFile.Name.Substring(0, sFile.Name.Length - 4);
        var fileNameWithExt = sFile.Name;
        var (namesp, type, method, found) = GetModEntryPoint(file);
        var newMod = new QMod
        {
            DisplayName = fileNameWithoutExt,
            Enable = true,
            ModAssemblyPath = path,
            AssemblyName = fileNameWithExt,
            Author = "?",
            Id = fileNameWithoutExt,
            EntryMethod = found ? $"{namesp}.{type}.{method}" : $"{fileNameWithoutExt}.MainPatcher.Patch",
            Config = new Dictionary<string, object>(),
            Priority = "Last or First",
            Requires = Array.Empty<string>(),
            Version = "?"
        };
        var newJson = JsonConvert.SerializeObject(newMod, Formatting.Indented);
        if (path == null) return false;
        File.WriteAllText(Path.Combine(path, "mod.json"), newJson);
        var files = new FileInfo(Path.Combine(path, "mod.json"));
        return files.Exists;
    }

    private static void LoadMod(QMod mod)
    {
        try
        {
            MethodInfo methodToLoad;
            var jsonEntrySplit = mod.EntryMethod.Split('.');
            var m = GetModEntryPoint(mod.ModAssemblyPath);

            var jsonEntry = $"{jsonEntrySplit[0]}.{jsonEntrySplit[1]}.{jsonEntrySplit[2]}";
            var foundEntry = $"{m.namesp}.{m.type}.{m.method}";

            if (!jsonEntry.Equals(foundEntry, StringComparison.Ordinal))
            {
                Logger.WriteLog(
                    $"Found entry point in {mod.AssemblyName} does not match what's in the JSON. Ignoring JSON and loading found entry method.");
                methodToLoad = mod.LoadedAssembly.GetType(m.namesp + "." + m.type).GetMethod(m.method);
            }
            else
            {
                methodToLoad = mod.LoadedAssembly.GetType($"{jsonEntrySplit[0]}.{jsonEntrySplit[1]}")
                    .GetMethod(jsonEntrySplit[2]);
            }

            methodToLoad?.Invoke(m, Array.Empty<object>());
            Logger.WriteLog($"Load order: {mod.LoadOrder}, successfully invoked {mod.DisplayName} entry method.");
        }
        catch (TargetInvocationException)
        {
            Logger.WriteLog($"Invoking the specified EntryMethod {mod.EntryMethod} failed for {mod.Id}");
        }
        catch (Exception finalEx)
        {
            Logger.WriteLog(finalEx.Message);
        }
    }
}