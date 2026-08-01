using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace MaterialChartPlugin.Models.Settings
{
    /// <summary>
    /// グラフに関する設定を表す静的プロパティを公開します。
    /// </summary>
    public static class ChartSettings
    {
        /// <summary>
        /// グラフの表示するデータの期間を示す設定値を取得します。
        /// </summary>
        public static SerializableProperty<DisplayedPeriod> DisplayedPeriod { get; }
            = new SerializableProperty<DisplayedPeriod>(GetKey(), SettingsProviders.Roaming, Models.DisplayedPeriod.OneDay) { AutoSave = true };

        /// <summary>
        /// 燃料グラフの表示状態を取得します。
        /// </summary>
        public static SerializableProperty<bool> ShowFuel { get; }
            = new SerializableProperty<bool>(GetKey(), SettingsProviders.Roaming, true) { AutoSave = true };

        /// <summary>
        /// 弾薬グラフの表示状態を取得します。
        /// </summary>
        public static SerializableProperty<bool> ShowAmmunition { get; }
            = new SerializableProperty<bool>(GetKey(), SettingsProviders.Roaming, true) { AutoSave = true };

        /// <summary>
        /// 鋼材グラフの表示状態を取得します。
        /// </summary>
        public static SerializableProperty<bool> ShowSteel { get; }
            = new SerializableProperty<bool>(GetKey(), SettingsProviders.Roaming, true) { AutoSave = true };

        /// <summary>
        /// ボーキサイトグラフの表示状態を取得します。
        /// </summary>
        public static SerializableProperty<bool> ShowBauxite { get; }
            = new SerializableProperty<bool>(GetKey(), SettingsProviders.Roaming, true) { AutoSave = true };

        /// <summary>
        /// 高速修復材グラフの表示状態を取得します。
        /// </summary>
        public static SerializableProperty<bool> ShowRepairTool { get; }
            = new SerializableProperty<bool>(GetKey(), SettingsProviders.Roaming, true) { AutoSave = true };

        /// <summary>
        /// 高速建造材グラフの表示状態を取得します。
        /// </summary>
        public static SerializableProperty<bool> ShowInstantBuildTool { get; }
            = new SerializableProperty<bool>(GetKey(), SettingsProviders.Roaming, true) { AutoSave = true };

        /// <summary>
        /// 回復上限ラインの表示状態を取得します。
        /// </summary>
        public static SerializableProperty<bool> ShowStorableLimit { get; }
            = new SerializableProperty<bool>(GetKey(), SettingsProviders.Roaming, true) { AutoSave = true };

        private static string GetKey([CallerMemberName] string propertyName = "")
        {
            return $"{nameof(ChartSettings)}.{propertyName}";
        }
    }
}
