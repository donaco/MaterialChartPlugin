using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using MaterialChartPlugin.Models;
using MaterialChartPlugin.Models.Settings;
using MaterialChartPlugin.Models.Utilities;
using System.Reactive.Linq;
using System.Windows;
using Microsoft.Win32;

namespace MaterialChartPlugin.ViewModels
{
    /// <summary>
    /// Livet ViewModelCommand の代替
    /// </summary>
    public class ViewModelCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public ViewModelCommand(Action execute) : this(execute, null) { }

        public ViewModelCommand(Action execute, Func<bool> canExecute)
        {
            this._execute = execute ?? throw new ArgumentNullException(nameof(execute));
            this._canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => this._canExecute?.Invoke() ?? true;

        public void Execute(object parameter) => this._execute();
    }

    public class ViewModelCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool> _canExecute;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public ViewModelCommand(Action<T> execute) : this(execute, null) { }

        public ViewModelCommand(Action<T> execute, Func<T, bool> canExecute)
        {
            this._execute = execute ?? throw new ArgumentNullException(nameof(execute));
            this._canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            if (parameter is T t) return this._canExecute?.Invoke(t) ?? true;
            if (parameter == null && !typeof(T).IsValueType) return this._canExecute?.Invoke(default) ?? true;
            return false;
        }

        public void Execute(object parameter)
        {
            if (parameter is T t) this._execute(t);
            else if (parameter == null && !typeof(T).IsValueType) this._execute(default);
        }
    }

    public class ToolViewModel : ObservableObject, IDisposable
    {
        private MaterialChartPlugin plugin;

        public MaterialManager materialManager { get; }

        public int Fuel => materialManager.Fuel;

        public int Ammunition => materialManager.Ammunition;

        public int Steel => materialManager.Steel;

        public int Bauxite => materialManager.Bauxite;

        public int RepairTool => materialManager.RepairTool;

        public int InstantBuildTool => materialManager.InstantBuildTool;

        #region IsPopupMode変更通知プロパティ
        private bool _IsPopupMode;

        public bool IsPopupMode
        {
            get
            { return _IsPopupMode; }
            set
            {
                if (_IsPopupMode == value)
                    return;
                _IsPopupMode = value;
                this.OnPropertyChanged();
            }
        }
        #endregion

        #region IsTopMost変更通知プロパティ
        private bool _IsTopMost;

        public bool IsTopMost
        {
            get
            { return _IsTopMost; }
            set
            {
                if (_IsTopMost == value)
                    return;
                _IsTopMost = value;
                this.OnPropertyChanged();
            }
        }
        #endregion

        #region FuelSeries変更通知プロパティ
        private ObservableCollection<ChartPoint> _FuelSeries = new ObservableCollection<ChartPoint>();

        public ObservableCollection<ChartPoint> FuelSeries
        {
            get
            { return _FuelSeries; }
            set
            {
                if (_FuelSeries == value)
                    return;
                _FuelSeries = value;
                this.OnPropertyChanged();
            }
        }
        #endregion

        #region AmmunitionSeries変更通知プロパティ
        private ObservableCollection<ChartPoint> _AmmunitionSeries = new ObservableCollection<ChartPoint>();

        public ObservableCollection<ChartPoint> AmmunitionSeries
        {
            get
            { return _AmmunitionSeries; }
            set
            {
                if (_AmmunitionSeries == value)
                    return;
                _AmmunitionSeries = value;
                this.OnPropertyChanged();
            }
        }
        #endregion

        #region SteelSeries変更通知プロパティ
        private ObservableCollection<ChartPoint> _SteelSeries = new ObservableCollection<ChartPoint>();

        public ObservableCollection<ChartPoint> SteelSeries
        {
            get
            { return _SteelSeries; }
            set
            {
                if (_SteelSeries == value)
                    return;
                _SteelSeries = value;
                this.OnPropertyChanged();
            }
        }
        #endregion

        #region BauxiteSeries変更通知プロパティ
        private ObservableCollection<ChartPoint> _BauxiteSeries = new ObservableCollection<ChartPoint>();

        public ObservableCollection<ChartPoint> BauxiteSeries
        {
            get
            { return _BauxiteSeries; }
            set
            {
                if (_BauxiteSeries == value)
                    return;
                _BauxiteSeries = value;
                this.OnPropertyChanged();
            }
        }
        #endregion


        #region RepairToolSeries変更通知プロパティ
        private ObservableCollection<ChartPoint> _RepairToolSeries = new ObservableCollection<ChartPoint>();

        public ObservableCollection<ChartPoint> RepairToolSeries
        {
            get
            { return _RepairToolSeries; }
            set
            {
                if (_RepairToolSeries == value)
                    return;
                _RepairToolSeries = value;
                this.OnPropertyChanged();
            }
        }
        #endregion

        #region InstantBuildToolSeries変更通知プロパティ
        private ObservableCollection<ChartPoint> _InstantBuildToolSeries = new ObservableCollection<ChartPoint>();

        public ObservableCollection<ChartPoint> InstantBuildToolSeries
        {
            get
            { return _InstantBuildToolSeries; }
            set
            {
                if (_InstantBuildToolSeries == value)
                    return;
                _InstantBuildToolSeries = value;
                this.OnPropertyChanged();
            }
        }
        #endregion

        #region StorableLimitSeries変更通知プロパティ
        private ObservableCollection<ChartPoint> _StorableLimitSeries;

        public ObservableCollection<ChartPoint> StorableLimitSeries
        {
            get
            { return _StorableLimitSeries; }
            set
            {
                if (_StorableLimitSeries == value)
                    return;
                _StorableLimitSeries = value;
                this.OnPropertyChanged();
            }
        }
        #endregion

        #region XMin変更通知プロパティ
        private DateTime _XMin = DateTime.Now - TimeSpan.FromDays(1);

        public DateTime XMin
        {
            get
            { return _XMin; }
            set
            {
                if (_XMin == value)
                    return;
                _XMin = value;
                this.OnPropertyChanged();
            }
        }
        #endregion

        #region XMax変更通知プロパティ
        private DateTime _XMax = DateTime.Now;

        public DateTime XMax
        {
            get
            { return _XMax; }
            set
            {
                if (_XMax == value)
                    return;
                _XMax = value;
                this.OnPropertyChanged();
            }
        }
        #endregion

        #region YMax1変更通知プロパティ
        private double _YMax1 = 1000;

        public double YMax1
        {
            get
            { return _YMax1; }
            set
            {
                if (_YMax1 == value)
                    return;
                _YMax1 = value;
                this.OnPropertyChanged();
            }
        }
        #endregion

        #region YMax2変更通知プロパティ
        private double _YMax2 = 100;

        public double YMax2
        {
            get
            { return _YMax2; }
            set
            {
                if (_YMax2 == value)
                    return;
                _YMax2 = value;
                this.OnPropertyChanged();
            }
        }
        #endregion

        public DisplayedPeriod DisplayedPeriod => ChartSettings.DisplayedPeriod.Value;

        public IReadOnlyCollection<DisplayViewModel<DisplayedPeriod>> DisplayedPeriods { get; }

        int mostMaterial = 0;

        int mostRepairTool = 0;

        private readonly CompositeDisposable disposables = new CompositeDisposable();

        private IDisposable _displayedPeriodSubscription;

        private IDisposable _logChangedSubscription;

        private IDisposable _managerChangedSubscription;

        public ICommand OpenPopupWindowCommand { get; private set; }
        public ICommand ImportMaterialDataCommand { get; private set; }
        public ICommand ExportMaterialDataCommand { get; private set; }
        public ICommand ExportAsCsvCommand { get; private set; }

        public ToolViewModel(MaterialChartPlugin plugin)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("ToolViewModel: Constructor started");
                
                this.plugin = plugin;

                this.materialManager = new MaterialManager(plugin);

                this.DisplayedPeriods = new List<DisplayViewModel<DisplayedPeriod>>()
                {
                    DisplayViewModel.Create(DisplayedPeriod.OneDay, "1日"),
                    DisplayViewModel.Create(DisplayedPeriod.OneWeek, "1週間"),
                    DisplayViewModel.Create(DisplayedPeriod.OneMonth, "1ヶ月"),
                    DisplayViewModel.Create(DisplayedPeriod.ThreeMonths, "3ヶ月"),
                    DisplayViewModel.Create(DisplayedPeriod.OneYear, "1年"),
                    DisplayViewModel.Create(DisplayedPeriod.ThreeYears, "3年")
                };

                OpenPopupWindowCommand = new ViewModelCommand(OpenPopupWindow);
                ImportMaterialDataCommand = new ViewModelCommand(async () => await ImportMaterialData());
                ExportMaterialDataCommand = new ViewModelCommand(async () => await ExportMaterialData());
                ExportAsCsvCommand = new ViewModelCommand(async () => await ExportAsCsv());
                
                System.Diagnostics.Debug.WriteLine("ToolViewModel: Constructor completed");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ToolViewModel constructor failed: {ex}");
                throw;
            }
        }

        public async Task InitializeAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("ToolViewModel: Initialize started");

                await materialManager.Initialize();

                var history = materialManager.Log.History;

                // データ初期読み込み
                _logChangedSubscription = Observable.FromEvent<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                    h => (s, e) => h(e),
                    h => materialManager.Log.PropertyChanged += h,
                    h => materialManager.Log.PropertyChanged -= h)
                    .Where(e => e.PropertyName == nameof(materialManager.Log.HasLoaded))
                    .Subscribe(_ =>
                    {
                        if (materialManager.Log.HasLoaded)
                            RefleshData();
                    });
                disposables.Add(_logChangedSubscription);

                // 資材データの通知設定
                _managerChangedSubscription = Observable.FromEvent<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                    h => (s, e) => h(e),
                    h => materialManager.PropertyChanged += h,
                    h => materialManager.PropertyChanged -= h)
                    .Subscribe(e =>
                    {
                        if (e.PropertyName == nameof(materialManager.Fuel)) this.OnPropertyChanged(nameof(Fuel));
                        else if (e.PropertyName == nameof(materialManager.Ammunition)) this.OnPropertyChanged(nameof(Ammunition));
                        else if (e.PropertyName == nameof(materialManager.Steel)) this.OnPropertyChanged(nameof(Steel));
                        else if (e.PropertyName == nameof(materialManager.Bauxite)) this.OnPropertyChanged(nameof(Bauxite));
                        else if (e.PropertyName == nameof(materialManager.RepairTool)) this.OnPropertyChanged(nameof(RepairTool));
                        else if (e.PropertyName == nameof(materialManager.InstantBuildTool)) this.OnPropertyChanged(nameof(InstantBuildTool));
                        else if (e.PropertyName == nameof(materialManager.IsAvailable))
                        {
                            _displayedPeriodSubscription?.Dispose();
                            _displayedPeriodSubscription = ChartSettings.DisplayedPeriod.Subscribe(___ =>
                            {
                                RefleshData();
                                this.OnPropertyChanged(nameof(DisplayedPeriod));
                            });
                        }
                    });
                disposables.Add(_managerChangedSubscription);
                disposables.Add(materialManager);

                // データ更新設定
                disposables.Add(Observable.FromEvent<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>
                    (h => (sender, e) => h(e), h => history.CollectionChanged += h, h => history.CollectionChanged -= h)
                    .Where(_ => materialManager.Log.HasLoaded)
                    .Throttle(TimeSpan.FromMilliseconds(10))
                    .Subscribe(_ => UpdateData(history.Last())));
                    
                System.Diagnostics.Debug.WriteLine("ToolViewModel: Initialize completed");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ToolViewModel Initialize failed: {ex}");
                throw; // Taskに例外を乗せて呼び元に伝播する
            }
        }

        /// <summary>
        /// ポップアップウィンドウを開きます。
        /// </summary>
        public void OpenPopupWindow()
        {
            var window = new Views.MaterialWindow
            {
                DataContext = this,
            };
            IsPopupMode = true;
            EventHandler onClosed = null;
            onClosed = (s, e) =>
            {
                IsPopupMode = false;
                window.Closed -= onClosed;
            };
            window.Closed += onClosed;
            window.Show();
        }

        /// <summary>
        /// グラフに新しいデータを追加します。
        /// </summary>
        /// <param name="newData"></param>
        public void UpdateData(TimeMaterialsPair newData)
        {
            SetXAxis(newData);
            SetMaterialYAxis(Math.Max(this.mostMaterial, newData.MostMaterial));
            SetRepairToolYAxis(Math.Max(this.mostRepairTool, Math.Max(newData.RepairTool, newData.InstantBuildTool)));
            AddChartData(newData);
        }

        /// <summary>
        /// グラフのデータをリフレッシュします。
        /// </summary>
        public void RefleshData()
        {
            // 描画すべきデータがなかったら何もしない
            if (materialManager.Log.History
                .Within(ChartSettings.DisplayedPeriod)
                .ThinOut(ChartSettings.DisplayedPeriod).Count() == 0)
                return;

            var neededData = materialManager.Log.History
                .Within(ChartSettings.DisplayedPeriod)
                .ThinOut(ChartSettings.DisplayedPeriod)
                .ToArray();

            SetXAxis(neededData[neededData.Length - 1]);
            SetMaterialYAxis(neededData.Max(p => p.MostMaterial));
            SetRepairToolYAxis(neededData.Max(p => Math.Max(p.RepairTool, p.InstantBuildTool)));
            RefleshChartData(neededData);
        }

        private void AddChartData(TimeMaterialsPair data)
        {
            Application.Current.Dispatcher.Invoke(
                () =>
                {
                    FuelSeries.Add(new ChartPoint(data.DateTime, data.Fuel));
                    AmmunitionSeries.Add(new ChartPoint(data.DateTime, data.Ammunition));
                    SteelSeries.Add(new ChartPoint(data.DateTime, data.Steel));
                    BauxiteSeries.Add(new ChartPoint(data.DateTime, data.Bauxite));
                    RepairToolSeries.Add(new ChartPoint(data.DateTime, data.RepairTool));
                    InstantBuildToolSeries.Add(new ChartPoint(data.DateTime, data.InstantBuildTool));
                });

            var currentDateTime = data.DateTime;

            var storableLimit = new ObservableCollection<ChartPoint>();
            storableLimit.Add(new ChartPoint(currentDateTime - ChartSettings.DisplayedPeriod.Value.ToTimeSpan(),
                materialManager.StorableMaterialLimit));
            storableLimit.Add(new ChartPoint(currentDateTime, materialManager.StorableMaterialLimit));
            this.StorableLimitSeries = storableLimit;
        }

        /// <summary>
        /// チャートにバインディングされたObservableCollectionのデータをリフレッシュします。
        /// </summary>
        /// <param name="neededData"></param>
        private void RefleshChartData(TimeMaterialsPair[] neededData)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                FuelSeries.Clear();
                AmmunitionSeries.Clear();
                SteelSeries.Clear();
                BauxiteSeries.Clear();
                RepairToolSeries.Clear();
                InstantBuildToolSeries.Clear();

                foreach (var data in neededData)
                {
                    FuelSeries.Add(new ChartPoint(data.DateTime, data.Fuel));
                    AmmunitionSeries.Add(new ChartPoint(data.DateTime, data.Ammunition));
                    SteelSeries.Add(new ChartPoint(data.DateTime, data.Steel));
                    BauxiteSeries.Add(new ChartPoint(data.DateTime, data.Bauxite));
                    RepairToolSeries.Add(new ChartPoint(data.DateTime, data.RepairTool));
                    InstantBuildToolSeries.Add(new ChartPoint(data.DateTime, data.InstantBuildTool));
                }

                var currentDateTime = neededData[neededData.Length - 1].DateTime;

                var storableLimit = new ObservableCollection<ChartPoint>();
                storableLimit.Add(new ChartPoint(currentDateTime - ChartSettings.DisplayedPeriod.Value.ToTimeSpan(),
                    materialManager.StorableMaterialLimit));
                storableLimit.Add(new ChartPoint(currentDateTime, materialManager.StorableMaterialLimit));
                this.StorableLimitSeries = storableLimit;
            });
        }

        /// <summary>
        /// X軸の設定を行います。
        /// </summary>
        private void SetXAxis(TimeMaterialsPair newData)
        {
            // X軸
            XMin = newData.DateTime - ChartSettings.DisplayedPeriod.Value.ToTimeSpan();
            XMax = newData.DateTime;
        }

        /// <summary>
        /// 資材グラフのY軸の設定を行います。
        /// </summary>
        /// <param name="mostMaterial">最も多い資材の量</param>
        private void SetMaterialYAxis(int mostMaterial)
        {
            this.mostMaterial = Math.Max(mostMaterial, 100);
            var interval = ChartUtilities.GetInterval(0, this.mostMaterial);
            YMax1 = ChartUtilities.GetYAxisMax(this.mostMaterial, interval);
        }

        /// <summary>
        /// 高速修復材グラフのY軸の設定を行います。
        /// </summary>
        /// <param name="mostRepairTool">最も多い高速修復材の量</param>
        private void SetRepairToolYAxis(int mostRepairTool)
        {
            this.mostRepairTool = Math.Max(mostRepairTool, 10);
            var interval = ChartUtilities.GetInterval(0, this.mostRepairTool);
            YMax2 = ChartUtilities.GetYAxisMax(this.mostRepairTool, interval);
        }

        public async Task ExportAsCsv()
        {
            var fileDialog = new SaveFileDialog()
            {
                Filter = "CSVファイル(*.csv)|*.csv|すべてのファイル(*.*)|*.*",
                FilterIndex = 1,
                Title = "エクスポート先の選択",
                FileName = $"MaterialChartPlugin-{DateTime.Now:yyMMdd-HHmmssff}.csv",
            };

            if (fileDialog.ShowDialog() == true)
            {
                await materialManager.Log.ExportAsCsvAsync(fileDialog.FileName);
            }
        }

        public async Task ImportMaterialData()
        {
            var fileDialog = new OpenFileDialog()
            {
                Filter = "データファイル(*.dat)|*dat|すべてのファイル(*.*)|*.*",
                FilterIndex = 1,
                Title = "インポートデータの選択",
            };

            if (fileDialog.ShowDialog() == true)
            {
                var messageBoxResult = MessageBox.Show("インポートすると、現在のデータは上書きされます。\nよろしいですか？", "上書き確認", MessageBoxButton.OKCancel);
                if (messageBoxResult == MessageBoxResult.OK)
                {
                    await materialManager.Log.ImportAsync(fileDialog.FileName);
                }
            }
        }

        public async Task ExportMaterialData()
        {
            var fileDialog = new SaveFileDialog()
            {
                Filter = "データファイル(*.dat)|*.dat|すべてのファイル(*.*)|*.*",
                FilterIndex = 1,
                Title = "エクスポート先の選択",
                FileName = $"MaterialChartPlugin-BackUp-{DateTime.Now:yyMMdd-HHmmssff}.dat",
            };

            if (fileDialog.ShowDialog() == true)
            {
                await materialManager.Log.ExportAsync(fileDialog.FileName);
            }
        }

        public void Dispose()
        {
            _displayedPeriodSubscription?.Dispose();
            _logChangedSubscription?.Dispose();
            _managerChangedSubscription?.Dispose();
            disposables.Dispose();
        }
    }
}
