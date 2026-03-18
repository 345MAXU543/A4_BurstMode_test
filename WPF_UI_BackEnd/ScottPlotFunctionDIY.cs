using ScottPlot;
using ScottPlot.Plottables;
using ScottPlot.WPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Markup;

namespace A4_BurstMode_test.WPF_UI_BackEnd
{
    internal class ScottPlotFunctionDIY
    {
        private readonly WpfPlot plot;
        private Scatter scatter;

        private bool isInspecting = false;  // Ctrl 吸點模式
        private bool isSelecting = false;   // Shift 框選放大模式
        private bool hasSelectionRect = false; // ✅ 用這個取代 Rectangle? 判斷
        private Coordinates selectStart;    // 框選起點
        private Rectangle selectionRect;    // 框選矩形
        private Crosshair crosshair;        // 十字線
        private ScottPlot.Color nowColor = new ScottPlot.Color();
        private List<dataPoint> lst_dataPoints = new List<dataPoint>();
        private List<dataPoint> lst_All_dataPoints = new List<dataPoint>();
        public class dataPoint
        {
            public ulong Index { get; set; }
            public double X1 { get; set; }
            public double Y1 { get; set; }
        }

        public ScottPlotFunctionDIY(WpfPlot plotControl, Scatter targetScatter)
        {
            plot = plotControl;
            scatter = targetScatter;
            nowColor = ScottPlot.Colors.DodgerBlue;

            // 註冊事件（Preview* 會搶先於內建互動）
            plot.PreviewMouseDown += OnPreviewMouseDown;
            plot.PreviewMouseMove += OnPreviewMouseMove;
            plot.PreviewMouseUp += OnPreviewMouseUp;
            plot.KeyDown += OnKeyDown;
            plot.MouseWheel += OnMouseWheel;
            plot.Focusable = true; // 確保可以接收鍵盤事件
        }

        private void OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            // ===== Ctrl 吸點 MODE =====
            if (Keyboard.IsKeyDown(Key.LeftCtrl) && e.LeftButton == MouseButtonState.Pressed)
            {
                isInspecting = true;
                e.Handled = true;
                UpdateCallout(e);
            }

            // ===== Shift 框選放大 =====
            else if (Keyboard.IsKeyDown(Key.LeftShift) && e.LeftButton == MouseButtonState.Pressed)
            {
                isSelecting = true;
                e.Handled = true;

                var pos = e.GetPosition(plot);
                var px = new Pixel(pos.X * plot.DisplayScale, pos.Y * plot.DisplayScale);
                selectStart = plot.Plot.GetCoordinates(px);

                // 🔹 新版 ScottPlot 建立矩形語法
                selectionRect = plot.Plot.Add.Rectangle(new CoordinateRect(selectStart, selectStart));
                selectionRect.LineColor = Colors.Red;
                selectionRect.LineWidth = 1;
                selectionRect.FillColor = Colors.Transparent;

                hasSelectionRect = true;
            }
        }

        private void OnPreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (isInspecting)
                isInspecting = false;

            if (isSelecting && e.LeftButton == MouseButtonState.Released)
            {
                isSelecting = false;
                e.Handled = true;

                if (hasSelectionRect)
                {
                    var pos = e.GetPosition(plot);
                    var px = new Pixel(pos.X * plot.DisplayScale, pos.Y * plot.DisplayScale);
                    Coordinates end = plot.Plot.GetCoordinates(px);
                    double xMin = System.Math.Min(selectStart.X, end.X);
                    double xMax = System.Math.Max(selectStart.X, end.X);
                    double yMin = System.Math.Min(selectStart.Y, end.Y);
                    double yMax = System.Math.Max(selectStart.Y, end.Y);

                    plot.Plot.Axes.SetLimits(xMin, xMax, yMin, yMax);

                    plot.Plot.Remove(selectionRect);
                    hasSelectionRect = false;
                    plot.Refresh();
                }
            }
        }

        private void OnPreviewMouseMove(object sender, MouseEventArgs e)
        {
            // Ctrl 吸點模式
            if (isInspecting && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                e.Handled = true;
                UpdateCallout(e);
            }

            // Shift 框選放大
            if (isSelecting && Keyboard.IsKeyDown(Key.LeftShift) && hasSelectionRect)
            {
                e.Handled = true;

                var pos = e.GetPosition(plot);
                var px = new Pixel(pos.X * plot.DisplayScale, pos.Y * plot.DisplayScale);
                Coordinates current = plot.Plot.GetCoordinates(px);

                selectionRect.CoordinateRect = new CoordinateRect(selectStart, current);
                plot.Refresh();
            }

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var limits = plot.Plot.Axes.GetLimits();
                var data = ViewportDownSample(lst_All_dataPoints, limits, 2000);
                UpdateScatter(data, nowColor);
            }

        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var limits = plot.Plot.Axes.GetLimits();
            var data = ViewportDownSample(lst_All_dataPoints, limits, 2000);
            UpdateScatter(data, nowColor);
        }
        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                UpdateScatter(lst_All_dataPoints, nowColor);
                plot.Plot.Axes.AutoScale();
                plot.Plot.Clear<Callout>();

                if (crosshair != null)
                    crosshair.IsVisible = false;

                plot.Refresh();
            }
            var limits = plot.Plot.Axes.GetLimits();
            var data = ViewportDownSample(lst_All_dataPoints, limits, 2000);
            UpdateScatter(data, nowColor);
        }
        private void EnsureCrosshair()
        {
            if (crosshair == null)
            {
                crosshair = plot.Plot.Add.Crosshair(0, 0);
                crosshair.IsVisible = false;

                // 你可以在這裡設定外觀
                crosshair.HorizontalLine.Color = Colors.OrangeRed;
                crosshair.VerticalLine.Color = Colors.OrangeRed;
                crosshair.HorizontalLine.LineWidth = 1;
                crosshair.VerticalLine.LineWidth = 1;
            }
        }
        private void UpdateCallout(MouseEventArgs e)
        {
            EnsureCrosshair(); // 保證 crosshair 一定存在
            var pos = e.GetPosition(plot);
            var px = new Pixel(pos.X * plot.DisplayScale, pos.Y * plot.DisplayScale);
            Coordinates mouse = plot.Plot.GetCoordinates(px);

            // 取得最近資料點
            DataPoint nearest = scatter.Data.GetNearest(mouse, plot.Plot.LastRender);
            int idx = nearest.Index;
            if (idx < 0 || idx >= lst_All_dataPoints.Count)
                return;
            Coordinates pt = nearest.Coordinates;
            ulong index = lst_All_dataPoints.Find(x => x.X1 == pt.X && x.Y1 == pt.Y)?.Index ?? 0;
            // 清除舊的 Callout
            plot.Plot.Clear<Callout>();


            // 🔹 建立新的 Callout
            plot.Plot.Add.Callout(
     $"Index = {index}\nX = {pt.X:F3}\nY = {pt.Y:F3}",
     textLocation: new Coordinates(pt.X, pt.Y),
     tipLocation: new Coordinates(pt.X, pt.Y)
             );

            // 🔹 更新十字線位置
            crosshair.X = pt.X;
            crosshair.Y = pt.Y;
            crosshair.IsVisible = true;

            plot.Refresh();
        }

        public void UpdateScatter(List<dataPoint> datas, ScottPlot.Color color)
        {
            List<double> dataX = datas.Select(dp => dp.X1).ToList();
            List<double> dataY = datas.Select(dp => dp.Y1).ToList();
            try
            {
                nowColor = color;

                if (plot == null) return;
                if (dataX == null || dataY == null || dataX.Count == 0 || dataY.Count == 0) return;

                double[] xs, ys;
                try
                {
                    xs = dataX.ToArray();  // 建立快照
                    ys = dataY.ToArray();
                }
                catch
                {
                    // 若複製失敗，略過本幀
                    return;
                }

                if (plot.Dispatcher.CheckAccess())
                {
                    if (scatter != null)
                        plot.Plot.Remove(scatter);

                    scatter = plot.Plot.Add.Scatter(xs, ys, color);

                    double xMin = xs.First();
                    double xMax = xs.Last();
                    plot.Plot.Axes.SetLimitsX(xMin, xMax);
                    plot.Plot.Axes.AutoScaleY();

                    try
                    {
                        // 立即重繪
                        plot.Refresh();
                    }
                    catch
                    {
                        // 忽略重繪失敗
                    }
                }
                else
                {
                    // 如果目前不在 UI 執行緒，回到主執行緒
                    plot.Dispatcher.Invoke(() => UpdateScatter(datas, color));
                }
            }
            catch
            {
                // 忽略更新失敗
            }
        }

        public void LastTimeUpdateScatter(List<dataPoint> data, ScottPlot.Color color)
        {
            lst_All_dataPoints = data;

            nowColor = color;
            UpdateScatter(data, color);
        }
        /// <summary>
        /// 清除所有資料
        /// </summary>
        public void Clear()
        {
            double[] xs, ys;
            plot.Plot.Clear();
            scatter = plot.Plot.Add.Scatter(Array.Empty<double>(), Array.Empty<double>(), ScottPlot.Colors.DodgerBlue);
            plot.Refresh();
        }

        public List<dataPoint> ViewportDownSample(List<dataPoint> data, AxisLimits limits, int targetVisiblePoints = 2000)
        {
            var down = new List<dataPoint>();

            if (data == null || data.Count == 0)
                return down;

            // 1️⃣ 找出目前畫面上可見範圍的索引
            int startIndex = data.FindIndex(p => p.X1 >= limits.XRange.Min);
            int endIndex = data.FindLastIndex(p => p.X1 <= limits.XRange.Max);
            if (startIndex < 0 || endIndex <= startIndex)
                return down;

            int visibleCount = endIndex - startIndex + 1;

            // 2️⃣ 若目前可見點數比目標少 → 不壓縮
            if (visibleCount <= targetVisiblePoints)
            {
                down.AddRange(data.Skip(startIndex).Take(visibleCount));
                return down;
            }

            // 3️⃣ 動態決定 step：確保輸出點數 ≈ targetVisiblePoints
            int step = Math.Max(1, visibleCount / targetVisiblePoints);

            // 4️⃣ Min-Max 取樣，保留原始索引
            for (int i = startIndex; i <= endIndex; i += step)
            {
                int chunkEnd = Math.Min(i + step, endIndex);
                double minY = double.MaxValue;
                double maxY = double.MinValue;
                int minIndex = i;
                int maxIndex = i;

                for (int j = i; j < chunkEnd; j++)
                {
                    double y = data[j].Y1;
                    if (y < minY) { minY = y; minIndex = j; }
                    if (y > maxY) { maxY = y; maxIndex = j; }
                }

                // 保留 min / max 的原始 Index
                down.Add(new dataPoint
                {
                    Index = data[minIndex].Index, // ✅ 原始索引保留
                    X1 = data[minIndex].X1,
                    Y1 = data[minIndex].Y1
                });

                if (maxIndex != minIndex)
                {
                    down.Add(new dataPoint
                    {
                        Index = data[maxIndex].Index, // ✅ 原始索引保留
                        X1 = data[maxIndex].X1,
                        Y1 = data[maxIndex].Y1
                    });
                }
            }

            return down;
        }


    }
}
