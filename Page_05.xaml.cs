using A4_BurstMode_test.A4_MB_SDK;
using System;
using System.Collections.Generic;
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
using static A4_BurstMode_test.A4_MB_SDK.A4MB;
using static A4_BurstMode_test.A4_MB_SDK.A4MB.BurstFrameDecoder;

namespace A4_BurstMode_test
{
    /// <summary>
    /// Page5.xaml 的互動邏輯
    /// </summary>
    public partial class Page_05 : Page
    {
        A4MB.BurstMode burstMode = new A4MB.BurstMode(MainWindow.A4Motherboard.Ftdi_Ctrl_USB_B, MainWindow.A4Motherboard.Ftdi_Ctrl_USB_C);

        public Page_05()
        {
            InitializeComponent();
        }


        private void btn_readPortB_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                while (true)
                {
                    try
                    {
                        byte[] raw = MainWindow.A4Motherboard.Ftdi_Ctrl_USB_B.ReadAvailableBytes();

                        if (raw.Length > 0)
                        {
                            string hex = BitConverter.ToString(raw).Replace("-", " ");

                            Dispatcher.Invoke(() =>
                            {
                                txt_read.Text = hex;
                            });
                        }

                        Thread.Sleep(10);
                    }
                    catch (Exception ex)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            txt_read.Text = "錯誤: " + ex.Message;
                        });

                        Thread.Sleep(100);
                    }
                }
            });
        }


        private void btn_config_Click(object sender, RoutedEventArgs e)
        {
            A4MB.BurstMode.BurstModeConfig config = new A4MB.BurstMode.BurstModeConfig();
            config.Source = 0x01;
            config.Peri = config.Sin12 = config.Sin34 = config.Sin56 = false;
            config.Adc1 = (bool)ckb_adc1.IsChecked;
            config.Adc2 = (bool)ckb_adc2.IsChecked;
            config.Adc3 = (bool)ckb_adc3.IsChecked;
            config.Adc4 = (bool)ckb_adc4.IsChecked;

            config.Dsp1 = (bool)ckb_dsp1.IsChecked;
            config.Dsp2 = (bool)ckb_dsp2.IsChecked;
            config.Dsp3 = (bool)ckb_dsp3.IsChecked;
            config.Dsp4 = (bool)ckb_dsp4.IsChecked;

            config.Enc1 = (bool)ckb_enc1.IsChecked;
            config.Enc2 = (bool)ckb_enc2.IsChecked;
            config.Enc3 = (bool)ckb_enc3.IsChecked;
            config.Enc4 = (bool)ckb_enc4.IsChecked;

            burstMode.StartBurst(config);

        }
        private BurstReader _burstReader;
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var defs = new List<BurstFieldDefinition>
    {
        new BurstFieldDefinition { Address = A4MB.Rcmd_ADC1_READ24_burstMode, Name = "ADC1_24", Type = BurstValueType.UInt24 },
        new BurstFieldDefinition { Address = A4MB.Rcmd_ADC2_READ24_burstMode, Name = "ADC2_24", Type = BurstValueType.UInt24 },
        new BurstFieldDefinition { Address = A4MB.Rcmd_ADC3_READ24_burstMode, Name = "ADC3_24", Type = BurstValueType.UInt24 },
        new BurstFieldDefinition { Address = A4MB.Rcmd_ADC4_READ24_burstMode, Name = "ADC4_24", Type = BurstValueType.UInt24 },

              new BurstFieldDefinition { Address = A4MB.Rcmd_DSP1_READ32, Name = "DSP1_32", Type = BurstValueType.UInt32 },
        new BurstFieldDefinition { Address = A4MB.Rcmd_DSP2_READ32, Name = "DSP2_32", Type = BurstValueType.UInt32 },
        new BurstFieldDefinition { Address = A4MB.Rcmd_DSP3_READ32, Name = "DSP3_32", Type = BurstValueType.UInt32 },
        new BurstFieldDefinition { Address = A4MB.Rcmd_DSP4_READ32, Name = "DSP4_32", Type = BurstValueType.UInt32 },

         new BurstFieldDefinition { Address = A4MB.Rcmd_ENC1_READ, Name = "ENC1_32", Type = BurstValueType.UInt32 },
        new BurstFieldDefinition { Address = A4MB.Rcmd_ENC2_READ, Name = "ENC2_32", Type = BurstValueType.UInt32 },
        new BurstFieldDefinition { Address = A4MB.Rcmd_ENC3_READ, Name = "ENC3_32", Type = BurstValueType.UInt32 },
        new BurstFieldDefinition { Address = A4MB.Rcmd_ENC4_READ, Name = "ENC4_32", Type = BurstValueType.UInt32 },


            };

            _burstReader = new BurstReader(MainWindow.A4Motherboard.Ftdi_Ctrl_USB_C, defs);
            _burstReader.FrameDecoded += OnFrameDecoded;
            _burstReader.Start();
        }

        private void OnFrameDecoded(BurstDecodedFrame frame)
        {
            Dispatcher.Invoke(() =>
            {
                switch (frame.Name)
                {
                    case "ADC1_24":
                        txt_adc1.Text = frame.RawValue.ToString();
                        break;
                    case "ADC2_24":
                        txt_adc2.Text = frame.RawValue.ToString();
                        break;
                    case "ADC3_24":
                        txt_adc3.Text = frame.RawValue.ToString();
                        break;
                    case "ADC4_24":
                        txt_adc4.Text = frame.RawValue.ToString();
                        break;

                    case "DSP1_32":
                        txt_dsp1.Text = frame.RawValue.ToString();
                        break;
                    case "DSP2_32":
                        txt_dsp2.Text = frame.RawValue.ToString();
                        break;
                    case "DSP3_32":
                        txt_dsp3.Text = frame.RawValue.ToString();
                        break;
                    case "DSP4_32":
                        txt_dsp4.Text = frame.RawValue.ToString();
                        break;

                    case "ENC1_32":
                        txt_enc1.Text = frame.RawValue.ToString();
                        break;
                    case "ENC2_32":
                        txt_enc2.Text = frame.RawValue.ToString();
                        break;
                    case "ENC3_32":
                        txt_enc3.Text = frame.RawValue.ToString();
                        break;
                    case "ENC4_32":
                        txt_enc4.Text = frame.RawValue.ToString();
                        break;



                }
            });
        }
    }
}
