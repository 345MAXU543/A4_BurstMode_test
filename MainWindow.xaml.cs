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
using System.Windows.Media.Animation;
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

        int NowPage = 1;
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

            Frame_mainFrame.Navigate(new Page_01());
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



        private void Label_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }


        private void grd_SideManu_MouseEnter_1(object sender, MouseEventArgs e)
        {
            var target = new Thickness(0, 30, 1300, 0);
            var anim = new ThicknessAnimation
            {
                To = target,
                Duration = TimeSpan.FromMilliseconds(150), // 動畫時間
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };

            // 執行動畫 (Margin 是依附屬性)
            grd_SideManu.BeginAnimation(FrameworkElement.MarginProperty, anim);

            var fadeIn = new DoubleAnimation
            {
                To = 0.5, // 半透明黑
                Duration = TimeSpan.FromMilliseconds(150)
            };
            rectOverlay.BeginAnimation(UIElement.OpacityProperty, fadeIn);
        }

        private void grd_SideManu_MouseLeave(object sender, MouseEventArgs e)
        {
            var target = new Thickness(0, 30, 1500, 0);
            var anim = new ThicknessAnimation
            {
                To = target,
                Duration = TimeSpan.FromMilliseconds(150), // 動畫時間
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };

            // 執行動畫 (Margin 是依附屬性)
            grd_SideManu.BeginAnimation(FrameworkElement.MarginProperty, anim);

            var fadeOut = new DoubleAnimation
            {
                To = 0, // 全透明
                Duration = TimeSpan.FromMilliseconds(150)
            };
            rectOverlay.BeginAnimation(UIElement.OpacityProperty, fadeOut);
        }


        private void fn_MouseEnterSideMenuItem(object sender, MouseEventArgs e)
        {
            Image ima = sender as Image;

        }

        private void img_SIdeManu_btn_Click(object sender, RoutedEventArgs e)
        {
            Button [] buttons = { img_SIdeManu_btn01, img_SIdeManu_btn02, img_SIdeManu_btn03, img_SIdeManu_btn04 };
            foreach (var btn in buttons)
            {
                btn.Background = (Brush)new BrushConverter().ConvertFromString("#FFDDDDDD");
            }


            Button button = sender as Button;
            button.Background = (Brush)new BrushConverter().ConvertFromString("#FFB4D8E4");

            if (NowPage.ToString() != button.Tag.ToString())
            {

                switch (button.Tag)
                {
                    case "1":
                        Frame_mainFrame.Navigate(new Page_01());
                        NowPage = 1;
                        break;

                    case "2":
                        Frame_mainFrame.Navigate(new Page_02());
                        NowPage = 2;
                        break;

                    case "3":
                        Frame_mainFrame.Navigate(new Page_03());
                        NowPage = 3;
                        break;

                    case "4":
                        Frame_mainFrame.Navigate(new Page_04());
                        NowPage = 4;
                        break;
                }


                //在這邊做畫面由下往上滑入
                var anim = new DoubleAnimation
                {
                    From = 1000, // 起始位置 (視窗底下，可以依實際高度調整)
                    To = 0,     // 回到正常位置
                    Duration = TimeSpan.FromMilliseconds(200),
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                };

                frameTransform.BeginAnimation(TranslateTransform.XProperty, anim);
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

