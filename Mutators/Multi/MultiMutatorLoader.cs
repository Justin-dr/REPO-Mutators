using BepInEx;
using Mutators.Managers;
using Mutators.Settings;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Mutators.Mutators.Multi
{
    internal static class MultiMutatorLoader
    {
        public static IList<IMultiMutator> LoadAll()
        {
            string multiMutatorPath = Path.Combine(Paths.PluginPath, $"Xepos-{MyPluginInfo.NAME}", "MultiMutators");

            if (!File.Exists(multiMutatorPath))
            {
                Directory.CreateDirectory(multiMutatorPath);
            }

            string[] files = Directory.GetFiles(multiMutatorPath);

            return files.Where(file => Path.GetExtension(file).Equals(".json", StringComparison.OrdinalIgnoreCase))
                .Select(File.ReadAllText)
                .Select(JsonConvert.DeserializeObject<JsonMultiMutator>)
                .Select(JsonToMultiMutator)
                .Where(multiMutator => multiMutator is not null)
                .ToList()!;
        }

        private static IMultiMutator? JsonToMultiMutator(JsonMultiMutator? jsonMultiMutator)
        {
            if (jsonMultiMutator == null) return null;

            IDictionary<IMutator, IDictionary<string, object>> mutators = new Dictionary<IMutator, IDictionary<string, object>>();

            foreach (KeyValuePair<string, IDictionary<string, object>> item in jsonMultiMutator.Mutators)
            {
                if (MutatorManager.Instance.RegisteredMutators.TryGetValue(item.Key, out IMutator mutator))
                {
                    mutators.Add(mutator, item.Value);
                }
                else
                {
                    RepoMutators.Logger.LogError($"Unable to find Mutator with name {item.Key}!");
                    LogAbortCreation(jsonMultiMutator);
                    return null;
                }
            }
            
            if (!ValidateAndIsValid(jsonMultiMutator))
            {
                LogAbortCreation(jsonMultiMutator);
                return null;
            }

            return new MultiMutator(
                new MultiMutatorSettings(
                    jsonMultiMutator.Name,
                    jsonMultiMutator.Description,
                    jsonMultiMutator.Weight,
                    jsonMultiMutator.MinimumLevel,
                    jsonMultiMutator.MaximumLevel
                ),
                mutators
            );
            
        }

        private static bool ValidateAndIsValid(JsonMultiMutator jsonMultiMutator)
        {
            List<string> invalidMutatorProperties = [];
            
            if (string.IsNullOrWhiteSpace(jsonMultiMutator.Name))
            {
                invalidMutatorProperties.Add("Name");
            }
            
            if (string.IsNullOrWhiteSpace(jsonMultiMutator.Description))
            {
                invalidMutatorProperties.Add("Description");
            }

            if (jsonMultiMutator.Weight == 0)
            {
                RepoMutators.Logger.LogInfo($"MultiMutator {jsonMultiMutator.Name} configured with weight: 0");
            }

            if (invalidMutatorProperties.Count <= 0) return true;
            
            RepoMutators.Logger.LogError($"Invalid MultiMutator properties: {string.Join(", ", invalidMutatorProperties)}");
            return false;

        }

        private static void LogAbortCreation(JsonMultiMutator jsonMultiMutator)
        {
            RepoMutators.Logger.LogError($"Aborting creation of MultiMutator with name {jsonMultiMutator.Name ?? "<Missing Name>"}!");
        }
    }
}
