using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Emby.AutoOrganize.Model;
using MediaBrowser.Common.Configuration;

namespace Emby.AutoOrganize.Core
{
    /// <summary>
    /// Static class containing extension methods helpful for working with configuration for this plugin.
    /// </summary>
    public static class ConfigurationExtension
    {
        /// <summary>
        /// The key to use with <see cref="IConfigurationManager"/> for storing the configuration of this plugin.
        /// </summary>
        public const string AutoOrganizeOptionsKey = "autoorganize";

        /// <summary>
        /// Perform a one-time migration of smart match info from the plugin configuration to the SQLite database.
        /// </summary>
        [SuppressMessage("Compiler", "CS0618:Type or member is obsolete", Justification = "This method is used to migrates configuration away from the obsolete property.")]
        public static void ConvertSmartMatchInfo(this IConfigurationManager manager, IFileOrganizationService service)
        {
            var options = manager.GetConfiguration<AutoOrganizeOptions>(AutoOrganizeOptionsKey);
            if (!options.Converted)
            {
                options.Converted = true;

                foreach (SmartMatchInfo optionsSmartMatchInfo in options.SmartMatchInfos)
                {
                    var result = new SmartMatchResult
                    {
                        DisplayName = optionsSmartMatchInfo.DisplayName,
                        ItemName = optionsSmartMatchInfo.ItemName,
                        OrganizerType = optionsSmartMatchInfo.OrganizerType,
                    };
                    result.MatchStrings.AddRange(optionsSmartMatchInfo.MatchStrings);
                    service.SaveResult(result, CancellationToken.None);
                }

                manager.SaveAutoOrganizeOptions(options);
            }
        }

        /// <summary>
        /// Get the <see cref="AutoOrganizeOptions"/> stored in the configuration manager using the
        /// <see cref="AutoOrganizeOptionsKey"/>.
        /// </summary>
        /// <param name="manager">The manager to retrieve the options from.</param>
        /// <returns>The retrieved options.</returns>
        public static AutoOrganizeOptions GetAutoOrganizeOptions(this IConfigurationManager manager)
        {
            return manager.GetConfiguration<AutoOrganizeOptions>(AutoOrganizeOptionsKey);
        }

        /// <summary>
        /// Save <see cref="AutoOrganizeOptions"/> into the configuration manager.
        /// </summary>
        /// <param name="manager">The configuration manager to store the options into.</param>
        /// <param name="options">The options to store.</param>
        public static void SaveAutoOrganizeOptions(this IConfigurationManager manager, AutoOrganizeOptions options)
        {
            manager.SaveConfiguration(AutoOrganizeOptionsKey, options);
        }
    }

    /// <summary>
    /// A configuration factory that registers the configuration entry required for the <see cref="AutoOrganizePlugin"/>.
    /// </summary>
    public class AutoOrganizeOptionsFactory : IConfigurationFactory
    {
        /// <inheritdoc/>
        public IEnumerable<ConfigurationStore> GetConfigurations()
        {
            return new List<ConfigurationStore>
            {
                new ConfigurationStore
                {
                    Key = ConfigurationExtension.AutoOrganizeOptionsKey,
                    ConfigurationType = typeof(AutoOrganizeOptions)
                }
            };
        }
    }
}
