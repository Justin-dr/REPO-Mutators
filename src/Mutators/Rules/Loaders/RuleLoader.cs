using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mutators.Rules.Loaders.Json;
using Mutators.Rules.Loaders.Strategies;
using Newtonsoft.Json;

namespace Mutators.Rules.Loaders
{
    /// <summary>
    /// Base implementation for loading JSON rule definitions as executable rules.
    /// </summary>
    /// <typeparam name="T">The executable rule type produced by this loader.</typeparam>
    public abstract class RuleLoader<T> : IRuleLoader<T>
    {
        /// <summary>
        /// Default JSON rules keyed by rule key.
        /// </summary>
        protected readonly IDictionary<string, JsonMutatorRule> defaultRules = new Dictionary<string, JsonMutatorRule>();

        /// <summary>
        /// Rule loading strategies keyed by JSON rule type.
        /// </summary>
        protected readonly IDictionary<string, IRuleLoadingStrategy<T>> strategies = new Dictionary<string, IRuleLoadingStrategy<T>>();

        /// <summary>
        /// Loads valid, enabled JSON rules from disk and maps them to executable rules.
        /// <para>
        /// After loading, the default rules are written back to disk.
        /// </para>
        /// </summary>
        /// <returns>The loaded executable rules keyed by rule key.</returns>
        public IDictionary<string, T> Load()
        {
            IDictionary<string, T> executableRules = new Dictionary<string, T>();
            CreateDefaultFilesAndThen(RepoMutators.ConfigPath, () =>
            {
                JsonMutatorRule[] rules = ReadRules(GetRulesPath());
                foreach (JsonMutatorRule rule in rules)
                {
                    defaultRules[rule.Key] = rule;
                    if (!strategies.TryGetValue(rule.Type, out IRuleLoadingStrategy<T> strategy))
                    {
                        RepoMutators.Logger.LogError($"Skipping invalid rule {rule.Key}: unknown rule type {rule.Type}");
                        continue;
                    }

                    if (!rule.Enabled)
                    {
                        RepoMutators.Logger.LogDebug($"Skipping disabled rule {rule.Key}");
                        continue;
                    }
                    if (!strategy.Validate(rule))
                    {
                        RepoMutators.Logger.LogWarning($"Rule {rule.Key} has an invalid configuration, skipping...");
                        continue;
                    }
                    
                    executableRules.Add(rule.Key, strategy.Load(rule.Mutators, rule.Arguments));
                    RepoMutators.Logger.LogDebug($"Successfully loaded rule {rule.Key} with strategy {strategy.Key}!");
                }
            });
            
            WriteRules(GetRulesPath(), GetSortedDefaultRules());
            
            return executableRules;
        }
        
        /// <summary>
        /// Creates the default files if they don't exist, then calls the then action.'
        /// </summary>
        /// <param name="configPath">The path where the default files should be created.</param>
        /// <param name="then">The action to be executed after the default files have been created.</param>
        protected void CreateDefaultFilesAndThen(string configPath, Action then)
        {
            if (!Directory.Exists(configPath))
            {
                Directory.CreateDirectory(configPath);
            }

            if (!File.Exists(GetRulesPath()))
            {
                WriteRules(GetRulesPath(), GetSortedDefaultRules());
            }

            then();
        }
        
        private JsonMutatorRule[] GetSortedDefaultRules() =>  defaultRules.Select(kv => kv.Value)
            .OrderBy(rule => rule.Enabled)
            .ThenBy(rule => rule.Key)
            .ToArray();

        /// <summary>
        /// <inheritdoc cref="IRuleLoader{T}.AddDefaultRule"/>
        /// </summary>
        /// <param name="jsonRule"><inheritdoc cref="IRuleLoader{T}.AddDefaultRule"/></param>
        public void AddDefaultRule(JsonMutatorRule jsonRule)
        {
            defaultRules.Add(jsonRule.Key, jsonRule);
        }

        /// <summary>
        /// <inheritdoc cref="IRuleLoader{T}.AddDefaultRules"/>
        /// </summary>
        /// <param name="jsonRules"><inheritdoc cref="IRuleLoader{T}.AddDefaultRules"/></param>
        public void AddDefaultRules(params JsonMutatorRule[] jsonRules)
        {
            foreach (JsonMutatorRule jsonRule in jsonRules)
            {
                AddDefaultRule(jsonRule);
            }
        }

        /// <summary>
        /// <inheritdoc cref="IRuleLoader{T}.AddRuleStrategy"/>
        /// </summary>
        /// <param name="strategy"><inheritdoc cref="IRuleLoader{T}.AddRuleStrategy"/></param>
        public void AddRuleStrategy(IRuleLoadingStrategy<T> strategy)
        {
            strategies.Add(strategy.Key, strategy);
        }
        
        /// <summary>
        /// Reads the rules from the specified path.
        /// </summary>
        /// <param name="path">The path from which to read the rules.</param>
        /// <returns>An array of the read rules.</returns>
        protected JsonMutatorRule[] ReadRules(string path)
        {
            string json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<JsonMutatorRule[]>(json) ?? CreateDefaultRules();
        }
        
        /// <summary>
        /// Writes the rules to the specified path.
        /// </summary>
        /// <param name="path">The path to which the rules will be written.</param>
        /// <param name="rules">The rules to write.</param>
        protected void WriteRules(string path, JsonMutatorRule[] rules)
        {
            RepoMutators.Logger.LogDebug($"Writing rules to {path}");
            File.WriteAllText(path, JsonConvert.SerializeObject(rules, Formatting.Indented));
        }
        
        /// <summary>
        /// Creates the default rules.
        /// </summary>
        /// <returns>The created default rules.</returns>
        protected abstract JsonMutatorRule[] CreateDefaultRules();
        
        /// <summary>
        /// Gets the path to the JSON rules file.
        /// </summary>
        /// <returns>The path to the rules file.</returns>
        protected abstract string GetRulesPath();
    }
}
