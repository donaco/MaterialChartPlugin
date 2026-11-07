using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xaml;

namespace MaterialChartPlugin.Models.Settings
{
    public class SettingsProviders
    {
        public static string DataDirectoryPath { get; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "grabacr.net", "KanColleViewer", "Plugins", "MaterialChartPlugin");

        public static string LocalFilePath { get; } = Path.Combine(DataDirectoryPath, "Settings.xaml");

        public static ISerializationProvider Local { get; } = new FileSettingsProvider(LocalFilePath);

        public static ISerializationProvider Roaming => Local;

        internal static void MigrateLegacyData()
        {
            MigrateLegacyData(
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "terry_u16", "MaterialChartPlugin"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "terry_u16", "MaterialChartPlugin"),
                DataDirectoryPath);
        }

        private static void MigrateLegacyData(string legacyLocalDirectory, string legacyRoamingDirectory, string destinationDirectory)
        {
            var settingsPath = Path.Combine(destinationDirectory, "Settings.xaml");
            if (!File.Exists(settingsPath))
            {
                var settings = new SortedDictionary<string, object>();
                var hasSettings = false;
                foreach (var directory in new[] { legacyRoamingDirectory, legacyLocalDirectory })
                {
                    var path = Path.Combine(directory, "Settings.xaml");
                    if (!File.Exists(path)) continue;
                    using (var stream = File.OpenRead(path))
                    {
                        var source = XamlServices.Load(stream) as IDictionary<string, object>
                            ?? throw new InvalidDataException($"設定ファイルの形式が不正です: {path}");
                        foreach (var entry in source)
                            settings[entry.Key] = entry.Value;
                    }
                    hasSettings = true;
                }

                if (hasSettings)
                    WriteMigratedFile(settingsPath, stream => XamlServices.Save(stream, settings));
            }

            var historyPath = Path.Combine(destinationDirectory, "materiallog.dat");
            var legacyHistoryPath = Path.Combine(legacyLocalDirectory, "materiallog.dat");
            if (!File.Exists(historyPath) && File.Exists(legacyHistoryPath))
            {
                WriteMigratedFile(historyPath, stream =>
                {
                    using (var source = File.OpenRead(legacyHistoryPath))
                        source.CopyTo(stream);
                });
            }
        }

        private static void WriteMigratedFile(string path, Action<Stream> write)
        {
            var directory = Path.GetDirectoryName(path) ?? throw new DirectoryNotFoundException();
            Directory.CreateDirectory(directory);
            var temporaryPath = Path.Combine(directory, Path.GetRandomFileName());
            try
            {
                using (var stream = new FileStream(temporaryPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    write(stream);
                    stream.Flush(true);
                }
                File.Move(temporaryPath, path);
            }
            finally
            {
                if (File.Exists(temporaryPath))
                    File.Delete(temporaryPath);
            }
        }
    }
}
