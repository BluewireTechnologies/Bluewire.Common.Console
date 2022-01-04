using System;
using System.IO;

namespace Bluewire.Common.Console.Util
{
    class ConfigurationReader
    {
        public static ConfigurationReader Default => new ConfigurationReader(AppDomain.CurrentDomain.BaseDirectory);

        internal readonly string DefaultBasePath;

        public ConfigurationReader(string defaultBasePath)
        {
            if (!PathValidator.IsValidPath(defaultBasePath)) throw new ArgumentException($"Invalid characters in path: {defaultBasePath}", nameof(defaultBasePath));
            if (!Path.IsPathRooted(defaultBasePath)) throw new ArgumentException("Base path must be absolute.", nameof(defaultBasePath));
            this.DefaultBasePath = defaultBasePath;
        }

        private string GetBasePath(string specifiedBasePath)
        {
            if (String.IsNullOrWhiteSpace(specifiedBasePath)) return DefaultBasePath;
            return specifiedBasePath;
        }

        public string GetAbsolutePath(string configuredValue, string defaultValue) => GetAbsolutePath(null, configuredValue, defaultValue);

        public string GetAbsolutePath(string basePath, string configuredValue, string defaultValue)
        {
            if (defaultValue == null) throw new ArgumentNullException(nameof(defaultValue));
            var configuredPathOrDefault = String.IsNullOrWhiteSpace(configuredValue) ? defaultValue : configuredValue.Trim();
            // Resolve relative paths from the basePath, and normalise separators.
            return Path.GetFullPath(Path.Combine(GetBasePath(basePath), configuredPathOrDefault));
        }
    }
}
