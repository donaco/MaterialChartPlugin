using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls.DataVisualization.Charting;
using System.Windows.Controls.DataVisualization.Charting.Primitives;
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
        private TextBlock _toolTipTitleBlock;
        private TextBlock _toolTipDateBlock;
        private TextBlock _toolTipValueBlock;

        public ToolView()
        {
            InitializeComponent();

            _toolTipTitleBlock = new TextBlock
            {
                Foreground = Brushes.White,
                FontWeight = FontWeights.Bold,
                FontSize = 13,
                Margin = new Thickness(8, 6, 8, 2)
            };

            _toolTipDateBlock = new TextBlock
            {
                Foreground = Brushes.LightGray,
                FontSize = 12,
                Margin = new Thickness(8, 2, 8, 2)
            };

            _toolTipValueBlock = new TextBlock
            {
                Foreground = Brushes.White,
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(8, 2, 8, 6)
            };

            var panel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Background = new SolidColorBrush(Color.FromArgb(240, 50, 50, 50)),
                Margin = new Thickness(0)
            };

            panel.Children.Add(_toolTipTitleBlock);
            panel.Children.Add(_toolTipDateBlock);
            panel.Children.Add(_toolTipValueBlock);

            _chartToolTip = new ToolTip
            {
                Content = panel,
                Placement = System.Windows.Controls.Primitives.PlacementMode.Mouse,
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(1),
                Padding = new Thickness(0),
                Background = Brushes.Transparent,
                IsOpen = false
            };
        }

        /// <summary>
        /// Chart テンプレート内のプロットエリア Grid を取得する
        /// </summary>
        private Grid GetPlotArea(Chart chart)
        {
            if (chart == null) return null;

            chart.ApplyTemplate();

            var chartArea = chart.Template?.FindName("ChartArea", chart) as EdgePanel;
            if (chartArea == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(chartArea); i++)
            {
                var child = VisualTreeHelper.GetChild(chartArea, i) as Grid;
                if (child == null) continue;

                // ToolView.xaml のテンプレートでは、
                // プロットエリアの Grid に Panel.ZIndex = -1 が設定されている
                if (Panel.GetZIndex(child) == -1)
                {
                    return child;
                }
            }

            return null;
        }

        private void Chart_MouseMove(object sender, MouseEventArgs e)
        {
            var chart = sender as Chart;
            if (chart == null)
            {
                HideToolTip();
                return;
            }

            try
            {
                var plotArea = GetPlotArea(chart);
                if (plotArea == null)
                {
                    HideToolTip();
                    return;
                }

                var mousePos = e.GetPosition(plotArea);

                var xAxis = chart.Axes.OfType<DateTimeAxis>().FirstOrDefault(a => a.Orientation == AxisOrientation.X);
                var yAxis = chart.Axes.OfType<LinearAxis>().FirstOrDefault(a => a.Orientation == AxisOrientation.Y);

                if (xAxis == null || yAxis == null)
                {
                    HideToolTip();
                    return;
                }

                var hitResult = FindNearestPoint(chart, xAxis, yAxis, plotArea, mousePos);

                if (hitResult != null)
                {
                    ShowToolTip(hitResult);
                }
                else
                {
                    HideToolTip();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MaterialChartPlugin] Chart_MouseMove failed: {ex}");
                HideToolTip();
            }
        }

        /// <summary>
        /// マウス位置に最も近いデータポイントを探す
        /// </summary>
        private HitTestResult FindNearestPoint(Chart chart,
            DateTimeAxis xAxis,
            LinearAxis yAxis,
            Grid plotArea,
            Point mousePos)
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
            HideToolTip();
        }

        /// <summary>
        /// ツールチップを表示
        /// </summary>
        private void ShowToolTip(HitTestResult hitResult)
        {
            if (hitResult == null || _chartToolTip == null) return;

            _toolTipTitleBlock.Text = hitResult.SeriesTitle;
            _toolTipDateBlock.Text = hitResult.DateTime.ToString("yyyy/MM/dd HH:mm");
            _toolTipValueBlock.Text = ((int)hitResult.Value).ToString("N0");

            // ツールチップに設定
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

        private void HideToolTip()
        {
            if (_chartToolTip != null)
            {
                _chartToolTip.IsOpen = false;
            }
        }
    }
}
