using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Controls.DataVisualization.Charting;
using MaterialChartPlugin.Models;
using MaterialChartPlugin.ViewModels;

namespace MaterialChartPlugin.Views
{
    /// <summary>
    /// UserControl1.xaml の相互作用ロジック
    /// </summary>
    public partial class ToolView : UserControl
    {
        private ToolTip _chartToolTip;

        public ToolView()
        {
            InitializeComponent();

            _chartToolTip = new ToolTip
            {
                Placement = System.Windows.Controls.Primitives.PlacementMode.Mouse,
                IsOpen = false
            };
        }

        /// <summary>
        /// ビジュアルツリーから指定した型の子要素を探す
        /// </summary>
        private T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T result)
                    return result;

                var childOfChild = FindVisualChild<T>(child);
                if (childOfChild != null)
                    return childOfChild;
            }
            return null;
        }

        private void Chart_MouseMove(object sender, MouseEventArgs e)
        {
            var chart = sender as Chart;
            if (chart == null || this.DataContext == null) return;

            var viewModel = this.DataContext as ToolViewModel;
            if (viewModel == null) return;

            try
            {
                // プロットエリアの座標を取得
                var plotArea = FindVisualChild<Grid>(chart);
                if (plotArea == null) return;

                var mousePos = e.GetPosition(plotArea);

                // 軸を取得
                var xAxis = chart.Axes.OfType<DateTimeAxis>().FirstOrDefault(a => a.Orientation == AxisOrientation.X);
                var yAxis = chart.Axes.OfType<LinearAxis>().FirstOrDefault(a => a.Orientation == AxisOrientation.Y);

                if (xAxis == null || yAxis == null) return;

                // すべての系列からヒットテスト
                var hitResult = FindNearestPoint(chart, viewModel, xAxis, yAxis, plotArea, mousePos);

                if (hitResult != null)
                {
                    // ツールチップを表示
                    ShowToolTip(hitResult);
                }
                else
                {
                    _chartToolTip.IsOpen = false;
                }
            }
            catch
            {
                _chartToolTip.IsOpen = false;
            }
        }

        /// <summary>
        /// マウス位置に最も近いデータポイントを探す
        /// </summary>
        private HitTestResult FindNearestPoint(Chart chart, ToolViewModel viewModel, 
            DateTimeAxis xAxis, LinearAxis yAxis, Grid plotArea, Point mousePos)
        {
            const double maxDistanceX = 20.0; // X方向の許容距離（ピクセル）

            HitTestResult nearest = null;
            double minDistance = double.MaxValue;

            // すべての系列をチェック
            var allSeries = chart.Series.OfType<DataPointSeries>().ToList();

            foreach (var series in allSeries)
            {
                // 非表示の系列はスキップ
                if (series.Visibility != Visibility.Visible) continue;

                var itemsSource = series.ItemsSource as System.Collections.IEnumerable;
                if (itemsSource == null) continue;

                foreach (var item in itemsSource)
                {
                    var chartPoint = item as ChartPoint;
                    if (chartPoint == null) continue;

                    // データポイントを画面座標に変換
                    var xCoord = xAxis.GetPlotAreaCoordinate(chartPoint.X);
                    var yCoord = yAxis.GetPlotAreaCoordinate(chartPoint.Y);

                    double screenX = xCoord.Value;
                    double screenY = plotArea.ActualHeight - yCoord.Value;

                    // NaNチェック
                    if (double.IsNaN(screenX) || double.IsNaN(screenY)) continue;

                    // マウス位置との距離を計算（X方向を優先）
                    double dx = Math.Abs(screenX - mousePos.X);
                    double dy = Math.Abs(screenY - mousePos.Y);

                    // X方向の許容距離外ならスキップ
                    if (dx > maxDistanceX) continue;

                    // 距離は X を主、Y を副として評価
                    double distance = dx * dx + dy * dy * 0.01;

                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        nearest = new HitTestResult
                        {
                            SeriesTitle = series.Title?.ToString() ?? "不明",
                            DateTime = chartPoint.X,
                            Value = chartPoint.Y
                        };
                    }
                }
            }

            return nearest;
        }

        /// <summary>
        /// ヒットテスト結果
        /// </summary>
        private class HitTestResult
        {
            public string SeriesTitle { get; set; }
            public DateTime DateTime { get; set; }
            public double Value { get; set; }
        }

        private void Chart_MouseLeave(object sender, MouseEventArgs e)
        {
            if (_chartToolTip != null)
            {
                _chartToolTip.IsOpen = false;
            }
        }

        /// <summary>
        /// ツールチップを表示
        /// </summary>
        private void ShowToolTip(HitTestResult hitResult)
        {
            if (hitResult == null || _chartToolTip == null) return;

            var panel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Background = new SolidColorBrush(Color.FromArgb(240, 50, 50, 50)),
                Margin = new Thickness(0)
            };

            // 系列名
            var titleBlock = new TextBlock
            {
                Text = hitResult.SeriesTitle,
                Foreground = Brushes.White,
                FontWeight = FontWeights.Bold,
                FontSize = 13,
                Margin = new Thickness(8, 6, 8, 2)
            };
            panel.Children.Add(titleBlock);

            // 日時
            var dateBlock = new TextBlock
            {
                Text = hitResult.DateTime.ToString("yyyy/MM/dd HH:mm"),
                Foreground = Brushes.LightGray,
                FontSize = 12,
                Margin = new Thickness(8, 2, 8, 2)
            };
            panel.Children.Add(dateBlock);

            // 値
            var valueBlock = new TextBlock
            {
                Text = ((int)hitResult.Value).ToString("N0"),
                Foreground = Brushes.White,
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(8, 2, 8, 6)
            };
            panel.Children.Add(valueBlock);

            // ツールチップに設定
            _chartToolTip.Content = panel;
            _chartToolTip.BorderBrush = Brushes.Gray;
            _chartToolTip.BorderThickness = new Thickness(1);
            _chartToolTip.Padding = new Thickness(0);
            _chartToolTip.Background = Brushes.Transparent;
			_chartToolTip.PlacementTarget = this;

			if (!_chartToolTip.IsOpen)
			{
				_chartToolTip.IsOpen = true;
			}
			else
			{
				// IsOpen のままだと Popup が再配置されないため、
				// オフセットを微小に変化させてマウスに追従させる
				_chartToolTip.HorizontalOffset = _chartToolTip.HorizontalOffset == 0 ? 0.01 : 0;
			}
		}
    }
}
