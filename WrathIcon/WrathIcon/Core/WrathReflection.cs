using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using WrathIcon.Utilities;

namespace WrathIcon.Core
{
    public static class WrathReflection
    {
        private static Dictionary<uint, string[]>? burstPresetsByJobId;
        private static bool initialized;

        public static IReadOnlyDictionary<uint, string[]>? BurstPresetsByJobId
        {
            get
            {
                if (!initialized)
                    Initialize();
                return burstPresetsByJobId;
            }
        }

        private static void Initialize()
        {
            initialized = true;

            try
            {
                var asm = AssemblyLoadContext.All
                    .SelectMany(ctx => ctx.Assemblies)
                    .FirstOrDefault(a => a.GetName().Name == "WrathCombo");

                if (asm == null)
                {
                    Logger.Warning("WrathCombo assembly not found - burst state readback unavailable");
                    return;
                }

                var type = asm.GetType("WrathCombo.WrathCombo");
                if (type == null)
                {
                    Logger.Warning("WrathCombo.WrathCombo type not found - burst state readback unavailable");
                    return;
                }

                var field = type.GetField("BurstPresetMap", BindingFlags.NonPublic | BindingFlags.Static);
                if (field == null)
                {
                    Logger.Warning("BurstPresetMap field not found - burst state readback unavailable");
                    return;
                }

                if (field.GetValue(null) is not IDictionary raw)
                {
                    Logger.Warning("BurstPresetMap value not a dictionary");
                    return;
                }

                var result = new Dictionary<uint, string[]>();
                foreach (DictionaryEntry entry in raw)
                {
                    if (entry.Value is not Array presets)
                        continue;

                    var jobId = (uint)Convert.ToInt32(entry.Key);
                    var names = new string[presets.Length];
                    for (int i = 0; i < presets.Length; i++)
                        names[i] = presets.GetValue(i)?.ToString() ?? string.Empty;
                    result[jobId] = names;
                }

                burstPresetsByJobId = result;
                Logger.Info($"Reflected WrathCombo BurstPresetMap: {result.Count} jobs");
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to reflect WrathCombo BurstPresetMap", ex);
            }
        }
    }
}
