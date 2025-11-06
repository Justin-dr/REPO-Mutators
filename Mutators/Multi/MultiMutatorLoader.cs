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
                    RepoMutators.Logger.LogError($"Aborting creation of MultiMuator with name {jsonMultiMutator.Name}!");
                    return null;
                }
            }

            return new MultiMutator(
                new GenericMutatorSettings(jsonMultiMutator.Name, jsonMultiMutator.Description, RepoMutators.Instance.Config),
                mutators
            );
            
        } 
    }
}
