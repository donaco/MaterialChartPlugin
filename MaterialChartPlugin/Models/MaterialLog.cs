using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using Livet;
using Grabacr07.KanColleWrapper;

namespace MaterialChartPlugin.Models
{
    public class MaterialLog : NotificationObject
    {
        static readonly string localDirectoryPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "terry_u16", "MaterialChartPlugin");

        public static readonly string ExportDirectoryPath = "MaterialChartPlugin";

        static readonly string saveFileName = "materiallog.dat";

        static string SaveFilePath => Path.Combine(localDirectoryPath, saveFileName);

        private MaterialChartPlugin plugin;

        private readonly object _saveLock = new object();

        private static readonly DataContractSerializer serializer =
            new DataContractSerializer(typeof(List<TimeMaterialsPair>));

        #region HasLoaded変更通知プロパティ
        private bool _HasLoaded = false;

        public bool HasLoaded
        {
            get
            { return _HasLoaded; }
            set
            { 
                if (_HasLoaded == value)
                    return;
                _HasLoaded = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        public ObservableCollection<TimeMaterialsPair> History { get; private set; }

        public MaterialLog(MaterialChartPlugin plugin)
        {
            this.plugin = plugin;
        }

        public async Task LoadAsync()
        {
            await LoadAsync(SaveFilePath, null);
        }

        private async Task LoadAsync(string filePath, Action onSuccess)
        {
            this.HasLoaded = false;

            if (File.Exists(filePath))
            {
                try
                {
                    var list = await Task.Run(() =>
                    {
                        using (var stream = File.OpenRead(filePath))
                        using (var reader = XmlDictionaryReader.CreateBinaryReader(stream, XmlDictionaryReaderQuotas.Max))
                        {
                            return (List<TimeMaterialsPair>)serializer.ReadObject(reader);
                        }
                    });

                    this.History = new ObservableCollection<TimeMaterialsPair>(list);
                    onSuccess?.Invoke();
                }
                catch (SerializationException ex)
                {
                    // 旧形式(protobuf-net)のデータファイルは読めないためリネームして退避
                    System.Diagnostics.Debug.WriteLine($"MaterialLog: Old format detected, renaming - {ex.Message}");
                    try
                    {
                        var backupPath = filePath + ".old";
                        if (File.Exists(backupPath))
                            File.Delete(backupPath);
                        File.Move(filePath, backupPath);
                    }
                    catch (IOException ioEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"MaterialLog: Failed to rename old file - {ioEx.Message}");
                    }

                    this.History = new ObservableCollection<TimeMaterialsPair>();
                }
                catch (Exception ex)
                {
                    plugin.InvokeNotifyRequested(new Grabacr07.KanColleViewer.Composition.NotifyEventArgs(
                        "MaterialChartPlugin.LoadFailed", "読み込み失敗",
                        "資材データの読み込みに失敗しました。データが破損しているか、形式が古い可能性があります。"));
                    System.Diagnostics.Debug.WriteLine($"MaterialLog: Load exception - {ex}");
                    if (this.History == null)
                        this.History = new ObservableCollection<TimeMaterialsPair>();
                }
            }
            else
            {
                if (this.History == null)
                    this.History = new ObservableCollection<TimeMaterialsPair>();
            }

            this.HasLoaded = true;
        }

        public async Task SaveAsync()
        {
            try
            {
                await SaveAsync(localDirectoryPath, SaveFilePath, null);
            }
            catch (Exception ex)
            {
                plugin.InvokeNotifyRequested(new Grabacr07.KanColleViewer.Composition.NotifyEventArgs(
                    "MaterialChartPlugin.SaveFailed", "保存失敗",
                    $"資材データの保存に失敗しました: {ex.GetType().Name}: {ex.Message}"));
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }

        private async Task SaveAsync(string directoryPath, string filePath, Action onSuccess)
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            List<TimeMaterialsPair> snapshot;
            lock (_saveLock)
            {
                snapshot = History.ToList();
            }

            await Task.Run(() =>
            {
                using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                using (var writer = XmlDictionaryWriter.CreateBinaryWriter(stream))
                {
                    serializer.WriteObject(writer, snapshot);
                }
            });

            onSuccess?.Invoke();
        }

        public async Task ExportAsCsvAsync()
        {
            var csvFileName = CreateCsvFileName(DateTime.Now);
            var csvFilePath = Path.Combine(ExportDirectoryPath, csvFileName);

            try
            {
                if (!Directory.Exists(ExportDirectoryPath))
                {
                    Directory.CreateDirectory(ExportDirectoryPath);
                }

                using (var writer = new StreamWriter(csvFilePath, false, Encoding.UTF8))
                {
                    await writer.WriteLineAsync("時刻,燃料,弾薬,鋼材,ボーキサイト,高速修復材,開発資材,高速建造材,改修資材");

                    foreach (var pair in History)
                    {
                        await writer.WriteLineAsync($"{pair.DateTime},{pair.Fuel},{pair.Ammunition},{pair.Steel},{pair.Bauxite},{pair.RepairTool},{pair.DevelopmentTool},{pair.InstantBuildTool},{pair.ImprovementTool}");
                    }
                }

                plugin.InvokeNotifyRequested(new Grabacr07.KanColleViewer.Composition.NotifyEventArgs(
                    "MaterialChartPlugin.CsvExportCompleted", "エクスポート完了",
                    $"資材データがエクスポートされました: {csvFilePath}")
                {
                    Activated = () =>
                    {
                        System.Diagnostics.Process.Start("EXPLORER.EXE", $"/select,\"\"{csvFilePath}\"\"");
                    }
                });
            }
            catch (IOException ex)
            {
                plugin.InvokeNotifyRequested(new Grabacr07.KanColleViewer.Composition.NotifyEventArgs(
                    "MaterialChartPlugin.ExportFailed", "エクスポート失敗",
                    "資材データのエクスポートに失敗しました。必要なアクセス権限がない可能性があります。"));
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }

        public async Task ImportAsync(string filePath)
        {
            await LoadAsync(filePath, async () =>
                {
                    plugin.InvokeNotifyRequested(new Grabacr07.KanColleViewer.Composition.NotifyEventArgs(
                        "MaterialChartPlugin.ImportComplete", "インポート完了",
                        "資材データのインポートに成功しました。"));

                    var materials = KanColleClient.Current.Homeport.Materials;
                    History.Add(new TimeMaterialsPair(DateTime.Now, materials.Fuel, materials.Ammunition, materials.Steel,
                        materials.Bauxite, materials.InstantRepairMaterials, materials.DevelopmentMaterials,
                        materials.InstantBuildMaterials, materials.ImprovementMaterials));

                    await SaveAsync();
                }
            );
        }

        public async Task ExportAsync()
        {
            var fileName = CreateExportedFileName(DateTime.Now);
            var filePath = Path.Combine(ExportDirectoryPath, fileName);
            await SaveAsync(ExportDirectoryPath, filePath, () =>
                plugin.InvokeNotifyRequested(new Grabacr07.KanColleViewer.Composition.NotifyEventArgs(
                    "MaterialChartPlugin.ExportComplete", "エクスポート完了",
                    $"資材データがエクスポートされました: {filePath}")
                {
                    Activated = () =>
                    {
                        System.Diagnostics.Process.Start("EXPLORER.EXE", $"/select,\"\"{filePath}\"\"");
                    }
                })
            );
        }

        private string CreateCsvFileName(DateTime dateTime)
        {
            return $"MaterialChartPlugin-{dateTime.ToString("yyMMdd-HHmmssff")}.csv";
        }

        private string CreateExportedFileName(DateTime dateTime)
        {
            return $"MaterialChartPlugin-BackUp-{dateTime.ToString("yyMMdd-HHmmssff")}.dat";
        }
    }
}
