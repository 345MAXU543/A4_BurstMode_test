using A4_BurstMode_test.A4_MB_SDK;
using FTD2XX_NET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Remoting.Channels;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static FTD2XX_NET.FTDI;

namespace A4_BurstMode_test
{
    public partial class MainWindow : Window
    {
        //初始化四個FTDI物件
        static FTDI Ftdi_USB_A = new FTDI();
        static FTDI Ftdi_USB_B = new FTDI();
        static FTDI Ftdi_USB_C = new FTDI();
        static FTDI Ftdi_USB_D = new FTDI();

        // 初始化 A4_MotherBoard並傳入四個 FTDI 物件
        static A4MB A4Motherboard = new A4MB(Ftdi_USB_A, Ftdi_USB_B, Ftdi_USB_C, Ftdi_USB_D);
        public MainWindow()
        {
            InitializeComponent();

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // FTDI 預處理(連線)，傳入對應的序號(SerialNumber)
            // motherboard.Ftdi_Ctrl_USB_A.FTDI_Preprocessing("20241018A");
            //motherboard.Ftdi_Ctrl_USB_B.FTDI_Preprocessing("20241018B");
            A4Motherboard.Ftdi_Ctrl_USB_C.FTDI_Preprocessing("20241018C");
            // motherboard.Ftdi_Ctrl_USB_D.FTDI_Preprocessing("20241018D");

          //  MakeA4HardwareBeepBeepSound(A4Motherboard);
        }

        private void MakeA4HardwareBeepBeepSound(A4MB motherboard)
        {
            motherboard.Ftdi_Ctrl_USB_C.Write(0x01, 0x20000);
            //motherboard.Ctrl.Write(0x01, 0x20000);
            Thread.Sleep(100);

            motherboard.Ftdi_Ctrl_USB_C.Write(0x01, 0x00000);
            //motherboard.Ctrl.Write(0x01, 0x00000);
            Thread.Sleep(50);

            motherboard.Ftdi_Ctrl_USB_C.Write(0x01, 0x20000);
            //motherboard.Ctrl.Write(0x01, 0x20000);
            Thread.Sleep(50);

            motherboard.Ftdi_Ctrl_USB_C.Write(0x01, 0x00000);
            //motherboard.Ctrl.Write(0x01, 0x00000);
            Thread.Sleep(50);

            motherboard.Ftdi_Ctrl_USB_C.Write(0x01, 0x20000);
            //motherboard.Ctrl.Write(0x01, 0x20000);
            Thread.Sleep(50);

            motherboard.Ftdi_Ctrl_USB_C.Write(0x01, 0x00000);
            //motherboard.Ctrl.Write(0x01, 0x00000);
            Thread.Sleep(50);
        }

      

        private void btn_exit_Click(object sender, MouseButtonEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void btn_windowMin_MouseUp(object sender, MouseButtonEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void ToggleNav(object sender, RoutedEventArgs e)
        {
            if (grd_SideManu.Margin.Right< 1512)
            {
                // 收合
                grd_SideManu.Margin = new Thickness(0,30,1552,0);
            }
            else
            {
                // 展開
                grd_SideManu.Margin = new Thickness(0, 30, 1400, 0);
            }
        }
    }

    class TestUnit//同時是使用範例
    {
        //初始化四個FTDI物件
        static FTDI Ftdi_USB_A = new FTDI();
        static FTDI Ftdi_USB_B = new FTDI();
        static FTDI Ftdi_USB_C = new FTDI();
        static FTDI Ftdi_USB_D = new FTDI();

        // 初始化 A4_MotherBoard並傳入四個 FTDI 物件
        static A4MB A4Motherboard = new A4MB(Ftdi_USB_A, Ftdi_USB_B, Ftdi_USB_C, Ftdi_USB_D);
        public static void Test()
        {
            // FTDI 預處理(連線)，傳入對應的序號(SerialNumber)
            // motherboard.Ftdi_Ctrl_USB_A.FTDI_Preprocessing("20241018A");
            //motherboard.Ftdi_Ctrl_USB_B.FTDI_Preprocessing("20241018B");
            A4Motherboard.Ftdi_Ctrl_USB_C.FTDI_Preprocessing("20241018C");
            // motherboard.Ftdi_Ctrl_USB_D.FTDI_Preprocessing("20241018D");

            // MakeA4HardwareBeepBeepSound(A4Motherboard);

            //  ADC_Test();
        }

        public static void ADC_Test()
        {
            A4MB.MAX11270_AD_Converter ADC = A4Motherboard.ADConverter;
            bool a = ADC.AD1.IsReady;
            bool b = ADC.AD2.IsReady;
            bool c = ADC.AD3.IsReady;
            bool d = ADC.AD4.IsReady;

            bool a1 = ADC.AD1.IsBusy;
            bool b1 = ADC.AD2.IsBusy;
            bool c1 = ADC.AD3.IsBusy;
            bool d1 = ADC.AD4.IsBusy;

            // 0xCD連續讀取
            var adcControlPack = new A4MB.MAX11270_AD_Converter.ADC12_CTRL_0x16_Parameter
            {
                Bit29 = 1, // Enable ADC2
                Bit28 = 1, // Enable automatic read for ADC2
                Bit27 = 1, // Read from ADC2
                Bit26_24 = 0b100, // 4 bytes for ADC2
                Bit23_16 = 0xCD, // Example command for ADC2

                Bit13 = 1, // Enable ADC1
                Bit12 = 1, // Enable automatic read for ADC1
                Bit11 = 1, // Read from ADC1
                Bit10_8 = 0b100, // 4 bytes for ADC1
                Bit7_0 = 0xCD // Example command for ADC1
            };
            uint pack = ADC.ADC12_CTRL_0x16_BuildPacket(adcControlPack);

            //motherboard.Ftdi_Ctrl_USB_C.O_Write(pack);
            A4Motherboard.ADConverter.ADC12_CTRL_0x16(adcControlPack);
            //uint? aa = motherboard.ADConverter.ADC1_READ32_0x17();
            //while (true)
            //{
            //    A4Motherboard.Ftdi_Ctrl_USB_C.O_Write(pack);
            //    A4Motherboard.Ftdi_Ctrl_USB_C.Write(0x16, pack);
            //    uint? LC1 = A4Motherboard.Ftdi_Ctrl_USB_C.Single_Read(A4MB.Rcmd_ADC1_READ32);
            //    uint? LC2 = A4Motherboard.Ftdi_Ctrl_USB_C.Single_Read(A4MB.Rcmd_ADC2_READ32);
            //}

            //motherboard.ADConverter.ADC_Control_0x16(adcControlPack);
            //// 等待 ADC 準備好數據
            //while (!motherboard.ADConverter.AD1.IsReady || !motherboard.ADConverter.AD2.IsReady)
            //{
            //    Thread.Sleep(10); // 避免忙等
            //}
            //// 讀取數據（假設每個通道返回4字節數據）
            //byte[] ad1Data = motherboard.ADConverter.ctrl.Read(4);
            //byte[] ad2Data = motherboard.ADConverter.ctrl.Read(4);
            //Console.WriteLine("ADC1 Data: " + BitConverter.ToString(ad1Data));
            //Console.WriteLine("ADC2 Data: " + BitConverter.ToString(ad2Data));
        }
        #region ftdi funtion
        static private void MakeA4HardwareBeepBeepSound(A4MB motherboard)
        {
            motherboard.Ftdi_Ctrl_USB_C.Write(0x01, 0x20000);
            //motherboard.Ctrl.Write(0x01, 0x20000);
            Thread.Sleep(100);

            motherboard.Ftdi_Ctrl_USB_C.Write(0x01, 0x00000);
            //motherboard.Ctrl.Write(0x01, 0x00000);
            Thread.Sleep(50);

            motherboard.Ftdi_Ctrl_USB_C.Write(0x01, 0x20000);
            //motherboard.Ctrl.Write(0x01, 0x20000);
            Thread.Sleep(50);

            motherboard.Ftdi_Ctrl_USB_C.Write(0x01, 0x00000);
            //motherboard.Ctrl.Write(0x01, 0x00000);
            Thread.Sleep(50);

            motherboard.Ftdi_Ctrl_USB_C.Write(0x01, 0x20000);
            //motherboard.Ctrl.Write(0x01, 0x20000);
            Thread.Sleep(50);

            motherboard.Ftdi_Ctrl_USB_C.Write(0x01, 0x00000);
            //motherboard.Ctrl.Write(0x01, 0x00000);
            Thread.Sleep(50);
        }
        #endregion
    }







}

