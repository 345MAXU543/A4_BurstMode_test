using A4_BurstMode_test.A4_MB_SDK;
using A4_BurstMode_test.WPF_UI_BackEnd;
using DynamicData;
using ScottPlot;
using ScottPlot.Colormaps;
using ScottPlot.DataSources;
using ScottPlot.Plottables;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;
using static A4_BurstMode_test.A4_MB_SDK.A4MB.MAX11270_AD_Converter;
using Color = System.Windows.Media.Color;

namespace A4_BurstMode_test
{
    /// <summary>
    /// Page_01.xaml 的互動邏輯
    /// </summary>
    public partial class Page_03 : Page
    {
        private List<Control> TempCBoxAndTextBox = new List<Control>();
        private List<string> ADC12_CTRL_0x16_Packet;
        private Scatter Plot_scatter;
        private ScottPlotFunctionDIY plotHelper_1;
        private ScottPlotFunctionDIY plotHelper_2;
        List<ScottPlotFunctionDIY.dataPoint> ad1_dataPoints = new List<ScottPlotFunctionDIY.dataPoint>();
        List<ScottPlotFunctionDIY.dataPoint> ad2_dataPoints = new List<ScottPlotFunctionDIY.dataPoint>();
        List<ScottPlotFunctionDIY.dataPoint> ad1_dataPoints_ALL= new List<ScottPlotFunctionDIY.dataPoint>();
        List<ScottPlotFunctionDIY.dataPoint> ad2_dataPoints_ALL = new List<ScottPlotFunctionDIY.dataPoint>();

        double global_ADC1_Load_32bit = 0;
        double global_ADC2_Load_32bit = 0;

        double global_ADC1_Load_24bit = 0;
        double global_ADC2_Load_24bit = 0;
        private CancellationTokenSource _cts;

       
        A4MB.MAX11270_AD_Converter ADC;
        public Page_03()
        {
            InitializeComponent();
        }


        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ADC = MainWindow.A4Motherboard.ADConverter;
            TempCBoxAndTextBox = new List<Control>    {
                  cb3130_0x16, cb29_0x16,cb28_0x16, cb27_0x16,
                   cb2624_0x16, txt2316_0x16, cb1514_0x16,
                    cb13_0x16, cb12_0x16, cb11_0x16,
                 cb1008_0x16, txt0700_0x16    };
            foreach (Control cb in TempCBoxAndTextBox)
            {
                if (cb is ComboBox)
                {
                    (cb as ComboBox).DropDownClosed += Cb_SelectionChanged;
                }
                else if (cb is TextBox)
                {
                    (cb as TextBox).TextChanged += Txt_TextChanged;
                }
            }


            Plot_scatter = adc1_plot.Plot.Add.Scatter(xs: new List<double>(), ys: new List<double>(), color: ScottPlot.Colors.DodgerBlue);
            plotHelper_1 = new ScottPlotFunctionDIY(adc1_plot, Plot_scatter);
            adc1_plot.Refresh();

            Plot_scatter = adc2_plot.Plot.Add.Scatter(xs: new List<double>(), ys: new List<double>(), color: ScottPlot.Colors.DodgerBlue);
            plotHelper_2 = new ScottPlotFunctionDIY(adc2_plot, Plot_scatter);
            adc2_plot.Refresh();

        }

        #region 上半部
        private void Txt_TextChanged(object sender, TextChangedEventArgs e)
        {
            dataGrid_ADC12_CTRL_0x16_SetData();
        }

        private void Cb_SelectionChanged(object sender, EventArgs e)
        {
            dataGrid_ADC12_CTRL_0x16_SetData();
        }

        private void dataGrid_ADC12_CTRL_0x16_SetData()
        {
            List<string> results = new List<string>();
            foreach (var ctrl in TempCBoxAndTextBox)
            {
                var tb = ctrl as TextBox;
                if (tb != null)
                {
                    try
                    {
                        tb.Text = tb.Text.Replace("0x", "");
                    }
                    catch (Exception ep) { }

                    if (uint.TryParse(tb.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint value))
                    {
                        string binaryString = Convert.ToString(value, 2);
                        if (binaryString.Length < 8)
                        {
                            int notEnough = 8 - binaryString.Length;
                            for (int i = 0; i < notEnough; i++)
                                binaryString = "0" + binaryString;
                        }
                        results.Add(binaryString);
                    }
                    else
                        results.Add("00000000");
                    continue;
                }

                var cb = ctrl as ComboBox;
                if (cb != null)
                    results.Add(cb.Text);
            }

            string merged = string.Join("", results);
            char[] chars = merged.ToCharArray();
            ADC12_CTRL_0x16_Packet = results;
            DataTable dt = new DataTable();

            // 建立 8 個欄位
            for (int j = 7; j >= 0; j--)
                dt.Columns.Add("Bit" + j, typeof(string));

            // 填入 4 列資料
            for (int i = 0; i < 4; i++)
            {
                DataRow row = dt.NewRow();
                for (int j = 0; j < 8; j++)
                {
                    row[j] = chars[i * 8 + j].ToString();
                }
                dt.Rows.Add(row);
            }
            dataGrid_ADC12_CTRL_0x16.ItemsSource = dt.DefaultView;
        }

        private void btn_AskBusy_Click(object sender, RoutedEventArgs e)
        {


            txt_ad1Busy.Background = (Brush)new BrushConverter().ConvertFromString("#FF3E2E2E");
            txt_ad1Busy.Foreground = (Brush)new BrushConverter().ConvertFromString("#FF3E2E2E");
            txt_ad1Rdy.Background = (Brush)new BrushConverter().ConvertFromString("#FF3E2E2E");
            txt_ad1Rdy.Foreground = (Brush)new BrushConverter().ConvertFromString("#FF3E2E2E");
            txt_ad2Busy.Background = (Brush)new BrushConverter().ConvertFromString("#FF3E2E2E");
            txt_ad2Busy.Foreground = (Brush)new BrushConverter().ConvertFromString("#FF3E2E2E");
            txt_ad2Rdy.Background = (Brush)new BrushConverter().ConvertFromString("#FF3E2E2E");
            txt_ad2Rdy.Foreground = (Brush)new BrushConverter().ConvertFromString("#FF3E2E2E");
            txt_ad3Busy.Background = (Brush)new BrushConverter().ConvertFromString("#FF3E2E2E");
            txt_ad3Busy.Foreground = (Brush)new BrushConverter().ConvertFromString("#FF3E2E2E");
            txt_ad3Rdy.Background = (Brush)new BrushConverter().ConvertFromString("#FF3E2E2E");
            txt_ad3Rdy.Foreground = (Brush)new BrushConverter().ConvertFromString("#FF3E2E2E");
            txt_ad4Busy.Background = (Brush)new BrushConverter().ConvertFromString("#FF3E2E2E");
            txt_ad4Busy.Foreground = (Brush)new BrushConverter().ConvertFromString("#FF3E2E2E");
            txt_ad4Rdy.Background = (Brush)new BrushConverter().ConvertFromString("#FF3E2E2E");
            txt_ad4Rdy.Foreground = (Brush)new BrushConverter().ConvertFromString("#FF3E2E2E");

            if (ADC.AD1.IsBusy)
                txt_ad1Busy.Background = Brushes.Red;
            if (ADC.AD2.IsBusy)
                txt_ad2Busy.Background = Brushes.Red;
            if (ADC.AD3.IsBusy)
                txt_ad3Busy.Background = Brushes.Red;
            if (ADC.AD4.IsBusy)
                txt_ad4Busy.Background = Brushes.Red;

            if (ADC.AD1.IsReady)
                txt_ad1Rdy.Background = Brushes.Lime;
            if (ADC.AD2.IsReady)
                txt_ad2Rdy.Background = Brushes.Lime;
            if (ADC.AD3.IsReady)
                txt_ad3Rdy.Background = Brushes.Lime;
            if (ADC.AD4.IsReady)
                txt_ad4Rdy.Background = Brushes.Lime;


        }

        private void btn_Send_0x16_Click(object sender, RoutedEventArgs e)
        {


            List<uint> bitList = new List<uint>();
            foreach (string aa in ADC12_CTRL_0x16_Packet)
            {
                uint.TryParse(aa, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint value);
                bitList.Add(value);
            }

            A4MB.MAX11270_AD_Converter.ADC12_CTRL_0x16_Parameter aDC12_CTRL_0X16_Parameter = new A4MB.MAX11270_AD_Converter.ADC12_CTRL_0x16_Parameter
            {
                Bit31_30 = bitList[0],
                Bit29 = bitList[1],
                Bit28 = bitList[2],
                Bit27 = bitList[3],
                Bit26_24 = bitList[4],
                Bit23_16 = bitList[5],
                Bit15_14 = bitList[6],
                Bit13 = bitList[7],
                Bit12 = bitList[8],
                Bit11 = bitList[9],
                Bit10_8 = bitList[10],
                Bit7_0 = bitList[11]
            };
            ADC.ADC12_CTRL_0x16(aDC12_CTRL_0X16_Parameter);



            ////1. List<uint>
            //MainWindow.A4Motherboard.ADConverter.ADC12_CTRL_0x16(bitList);

            ////2.uint[]
            //uint[] bits_uint = bitList.ToArray();
            //MainWindow.A4Motherboard.ADConverter.ADC12_CTRL_0x16(bits_uint);

            ////string[]
            //string[] bits_string = ADC12_CTRL_0x16_Packet.ToArray();
            //MainWindow.A4Motherboard.ADConverter.ADC12_CTRL_0x16(bits_string);
        }
        #endregion
        private bool _isReading = false;


      

        private void btn_AD12_StarContinuous_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string status = ADC.StartContinuous();
                txt_adc1_logger.AppendText(status + Environment.NewLine);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void btn_AD12_InitConfig_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string status = ADC.Config_Default_MAX11270();
                txt_adc1_logger.AppendText(status + Environment.NewLine);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private async void btn_AD12_ReadContinuous_Click(object sender, RoutedEventArgs e)
        {
            _isReading = !_isReading; // 按一次啟動，再按一次停止

            if (_isReading)// 開始讀取
            {
                ad1_dataPoints.Clear();
                ad2_dataPoints.Clear();
                ad1_dataPoints_ALL.Clear();
                ad2_dataPoints_ALL.Clear();
                plotHelper_1.Clear();
                plotHelper_2.Clear();
                btn_AD12_ReadContinuous.Content = "停止讀取";
                btn_AskBusy.IsEnabled = false;
                btn_AD12_InitConfig.IsEnabled = false;
                btn_AD12_OnceRead.IsEnabled = false;
                btn_AD12_StarContinuous.IsEnabled = false;

                _cts = new CancellationTokenSource();

                // 啟動高頻 ADC 背景執行緒
                _ = Task.Run(() => HighSpeedAdcLoop(_cts.Token));//前面的 底線 _ 其實是一個 「捨棄變數（discard）」，意思是：「我知道這個表達式會回傳一個值（Task），但我不打算使用它，所以我就明確地丟掉它。」

                _ = Task.Run(async () =>
                {
                    while (_isReading)
                    {
                        txt_adc1_logger.Dispatcher.Invoke(() =>
                        {
                            //textBox
                            if (txt_adc1_logger.LineCount > 10000)
                                txt_adc1_logger.Clear();
                            txt_adc1_logger.AppendText($"<24bit = {global_ADC1_Load_24bit}> <32bit = {global_ADC1_Load_32bit}>" + Environment.NewLine);
                            txt_adc1_logger.ScrollToEnd();

                            // 加鎖保護 dataX, dataY 操作
                            lock (dataLock)
                            {
                                plotHelper_1.UpdateScatter(ad1_dataPoints, ScottPlot.Colors.DodgerBlue);
                                if (ad1_dataPoints.Count > 10000)
                                {
                                    int removeCount = ad1_dataPoints.Count - 10000;
                                    ad1_dataPoints_ALL.AddRange(ad1_dataPoints.Take(removeCount));
                                    ad1_dataPoints.RemoveRange(0, removeCount);
                                }
                            }
                        });

                        txt_adc2_logger.Dispatcher.Invoke(() =>
                        {
                            //textBox
                            if (txt_adc2_logger.LineCount > 10000)
                                txt_adc2_logger.Clear();
                            txt_adc2_logger.AppendText($"<24bit = {global_ADC2_Load_24bit}> <32bit = {global_ADC2_Load_32bit}>" + Environment.NewLine);
                            txt_adc2_logger.ScrollToEnd();

                            // 加鎖保護 dataX, dataY 操作
                            lock (dataLock)
                            {
                                plotHelper_2.UpdateScatter(ad2_dataPoints, ScottPlot.Colors.DodgerBlue);
                                if (ad2_dataPoints.Count > 10000)
                                {
                                    int removeCount = ad2_dataPoints.Count - 10000;
                                    ad2_dataPoints_ALL.AddRange(ad2_dataPoints.Take(removeCount));
                                    ad2_dataPoints.RemoveRange(0, removeCount);
                                }
                            }
                        });
                        await Task.Delay(50);
                    }
                });
            }
            else// 停止讀取
            {
                btn_AskBusy.IsEnabled = true;
                btn_AD12_InitConfig.IsEnabled = true;
                btn_AD12_OnceRead.IsEnabled = true;
                btn_AD12_StarContinuous.IsEnabled = true;

                _cts?.Cancel();
                if (ad1_dataPoints.Count == 0) return;

                btn_AD12_ReadContinuous.Content = "開始讀取";

                lock (dataLock)
                {
                    ad1_dataPoints_ALL.AddRange(ad1_dataPoints);
                    ad2_dataPoints_ALL.AddRange(ad2_dataPoints);
                }
                plotHelper_1.LastTimeUpdateScatter(ad1_dataPoints_ALL, ScottPlot.Colors.DodgerBlue);
                plotHelper_2.LastTimeUpdateScatter(ad2_dataPoints_ALL, ScottPlot.Colors.DodgerBlue);
            }

        }


        private object dataLock = new object();
        // 🧠 高速取樣任務
        private void HighSpeedAdcLoop(CancellationToken token)
        {
            double TargetHz = 1000;
            Stopwatch sw = Stopwatch.StartNew();
            double period = 1000.0 / TargetHz; // 毫秒
            ulong index = 0;
            double adc1_raw32 = 0;
            double adc2_raw32 = 0;
            double adc1_raw24 = 0;
            double adc2_raw24 = 0;
            while (!token.IsCancellationRequested)
            {
                //adc1_raw32 = ADC.ReadContinuous_AD1_32bit();
                //adc2_raw32 = ADC.ReadContinuous_AD2_32bit();
                adc1_raw24 = ADC.ReadContinuous_AD1_24bit();
                adc2_raw24 = ADC.ReadContinuous_AD2_24bit();

                double t_sec = Math.Round(sw.Elapsed.TotalMilliseconds / 1000.0, 6);

                // 用 List<double> 取代 Append
                lock (dataLock)
                {
                    ad1_dataPoints.Add(new ScottPlotFunctionDIY.dataPoint
                    {
                        Index = index++,
                        X1 = t_sec,
                        Y1 = adc1_raw24,
                    });

                    ad2_dataPoints.Add(new ScottPlotFunctionDIY.dataPoint
                    {
                        Index = index++,
                        X1 = t_sec,
                        Y1 = adc2_raw24,
                    });
                }

                // raw24_2 = raw24;
                global_ADC1_Load_32bit = adc1_raw32;
                global_ADC2_Load_32bit = adc2_raw32;
                global_ADC1_Load_24bit = adc1_raw24;
                global_ADC2_Load_24bit = adc2_raw24;

                // 控制頻率：自算延遲時間
                double elapsed = sw.Elapsed.TotalMilliseconds % period;
                double sleepTime = period - elapsed;
                if (sleepTime > 0)
                {
                    // 使用 Thread.SpinWait 取代 Task.Delay，以免精度損失
                    var waitUntil = sw.Elapsed.TotalMilliseconds + sleepTime;
                    while (sw.Elapsed.TotalMilliseconds < waitUntil)
                    {
                        Thread.SpinWait(0); // 忙等，避免切換 context
                    }
                }

            }
        }

        private void btn_AD12_OnceRead_Click(object sender, RoutedEventArgs e)
        {
            double raw32 = ADC.ReadContinuous_AD1_32bit();
            txt_adc1_logger.AppendText($"一次讀取 32bit 數值: {raw32}" + Environment.NewLine);
        }

        private void btn_ClearText_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button.Name == "btn_adc1ClearText")
            {
                txt_adc1_logger.Clear();
            }
            else
            {
                txt_adc2_logger.Clear();
            }

        }


    }
}
