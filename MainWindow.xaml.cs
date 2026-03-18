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

        Page_01 page_01;
        Page_02 page_02;
        Page_03 page_03;
        Page_04 page_04;
        Page_05 page_05;

        // 初始化 A4_MotherBoard並傳入四個 FTDI 物件
        static public A4MB A4Motherboard = new A4MB(Ftdi_USB_A, Ftdi_USB_B, Ftdi_USB_C, Ftdi_USB_D);
        int NowPage = 1;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // FTDI 預處理(連線)，傳入對應的序號(SerialNumber)
                // motherboard.Ftdi_Ctrl_USB_A.FTDI_Preprocessing("20241018A");
                A4Motherboard.Ftdi_Ctrl_USB_B.FTDI_Preprocessing("20241018B");
                A4Motherboard.Ftdi_Ctrl_USB_C.FTDI_Preprocessing("20241018C");
                // motherboard.Ftdi_Ctrl_USB_D.FTDI_Preprocessing("20241018D");

                  MakeA4HardwareBeepBeepSound(A4Motherboard);
                page_01 = new Page_01();
                page_02 = new Page_02();
                page_03 = new Page_03();
                page_04 = new Page_04();
                page_05 = new Page_05();
                Frame_mainFrame.Navigate(page_01);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
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


        private void img_SIdeManu_btn_Click(object sender, RoutedEventArgs e)
        {
            Button [] buttons = { img_SIdeManu_btn01, img_SIdeManu_btn02, img_SIdeManu_btn03, img_SIdeManu_btn04, img_SIdeManu_btn05 };
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
                        Frame_mainFrame.Navigate(page_01);
                        NowPage = 1;
                        break;

                    case "2":
                        Frame_mainFrame.Navigate(page_02);
                        NowPage = 2;
                        break;

                    case "3":
                        Frame_mainFrame.Navigate(page_03);
                        NowPage = 3;
                        break;

                    case "4":
                        Frame_mainFrame.Navigate(page_04);
                        NowPage = 4;
                        break;

                       case "5":
                        Frame_mainFrame.Navigate(page_05);
                        NowPage = 5;
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

}

