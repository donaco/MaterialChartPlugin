using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Grabacr07.KanColleWrapper;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.ComponentModel;

namespace MaterialChartPlugin.Models
{
    public class MaterialManager : ObservableObject, IDisposable
    {
        private MaterialChartPlugin plugin;

        public int Fuel => KanColleClient.Current.Homeport.Materials.Fuel;

        public int Ammunition => KanColleClient.Current.Homeport.Materials.Ammunition;

        public int Steel => KanColleClient.Current.Homeport.Materials.Steel;

        public int Bauxite => KanColleClient.Current.Homeport.Materials.Bauxite;

        public int RepairTool => KanColleClient.Current.Homeport.Materials.InstantRepairMaterials;

        public int InstantBuildTool => KanColleClient.Current.Homeport.Materials.InstantBuildMaterials;

        /// <summary>
        /// 備蓄可能な資材量の上限を表します。
        /// </summary>
        public int StorableMaterialLimit => KanColleClient.Current.Homeport.Admiral.Level * 250 + 750;

        public MaterialLog Log { get; private set; }

        #region IsAvailable変更通知プロパティ
        private bool _IsAvailable = false;

        public bool IsAvailable
        {
            get
            { return _IsAvailable; }
            set
            { 
                if (_IsAvailable == value)
                    return;
                _IsAvailable = value;
                this.OnPropertyChanged();
            }
        }
        #endregion

        private IDisposable _isStartedSubscription;
        private IDisposable _materialsSubscription;
        private IDisposable _loggingSubscription;

        public MaterialManager(MaterialChartPlugin plugin)
        {
            this.plugin = plugin;

            this.Log = new MaterialLog(plugin);

            // KanColleClientのIsStartedがtrueに変更されたら資材データの購読を開始
            _isStartedSubscription = Observable.FromEvent<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                h => (s, e) => h(e),
                h => KanColleClient.Current.PropertyChanged += h,
                h => KanColleClient.Current.PropertyChanged -= h)
                .Where(e => e.PropertyName == nameof(KanColleClient.IsStarted))
                .Subscribe(e =>
                {
                    if (!KanColleClient.Current.IsStarted) return;

                    var materials = KanColleClient.Current.Homeport.Materials;
                    var admiral = KanColleClient.Current.Homeport.Admiral;

                    _materialsSubscription?.Dispose();
                    _materialsSubscription = Observable.FromEvent<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                        h => (s, ea) => h(ea),
                        h => materials.PropertyChanged += h,
                        h => materials.PropertyChanged -= h)
                        .Subscribe(ea =>
                        {
                            if (ea.PropertyName == nameof(materials.Fuel)) this.OnPropertyChanged(nameof(Fuel));
                            else if (ea.PropertyName == nameof(materials.Ammunition)) this.OnPropertyChanged(nameof(Ammunition));
                            else if (ea.PropertyName == nameof(materials.Steel)) this.OnPropertyChanged(nameof(Steel));
                            else if (ea.PropertyName == nameof(materials.Bauxite)) this.OnPropertyChanged(nameof(Bauxite));
                            else if (ea.PropertyName == nameof(materials.InstantRepairMaterials)) this.OnPropertyChanged(nameof(RepairTool));
                            else if (ea.PropertyName == nameof(materials.InstantBuildMaterials)) this.OnPropertyChanged(nameof(InstantBuildTool));
                        });

                    // 資材のロギング
                    _loggingSubscription?.Dispose();
                    _loggingSubscription = Observable.FromEvent<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                        h => (s, ea) => h(ea),
                        h => materials.PropertyChanged += h,
                        h => materials.PropertyChanged -= h)
                    // プロパティ名が一致しているか調べて
                    .Where(ea => IsObservedPropertyName(ea.PropertyName))
                    // まとめて通知が来るので10ms待機して
                    .Throttle(TimeSpan.FromMilliseconds(10))
                    // 処理
                    .Subscribe((PropertyChangedEventArgs ea) =>
                    {
                        if (Log.HasLoaded)
                        {
                            Log.History.Add(new TimeMaterialsPair(DateTime.Now, Fuel, Ammunition, Steel, Bauxite, RepairTool,
                                materials.DevelopmentMaterials, materials.InstantBuildMaterials, materials.ImprovementMaterials));
                            Log.SaveAsync().ConfigureAwait(false);
                        }
                    });

                this.IsAvailable = true;
            });
        }

        /// <summary>
        /// 監視対象のプロパティ名と一致しているかを調べます。
        /// </summary>
        /// <param name="propertyName">変更が通知されたプロパティ名</param>
        /// <returns></returns>
        private bool IsObservedPropertyName(string propertyName)
        {
            var materials = KanColleClient.Current.Homeport.Materials;
            return propertyName == nameof(materials.Fuel) || propertyName == nameof(materials.Ammunition)
                || propertyName == nameof(materials.Steel) || propertyName == nameof(materials.Bauxite)
                || propertyName == nameof(materials.InstantRepairMaterials);
        }

        public async Task Initialize()
        {
            await Log.LoadAsync();
        }

        public void Dispose()
        {
            // 先にLogをDisposeしてHasLoaded=falseにすることで、
            // Throttle残留中のロギングコールバックからのデータ書き込み・保存を防ぐ
            Log?.Dispose();
            _isStartedSubscription?.Dispose();
            _materialsSubscription?.Dispose();
            _loggingSubscription?.Dispose();
        }
    }
}
