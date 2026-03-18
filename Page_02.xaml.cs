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

namespace A4_BurstMode_test
{
    /// <summary>
    /// Page_01.xaml 的互動邏輯
    /// </summary>
    public partial class Page_02 : Page
    {
        A4MB.Encoder encoder;

        public Page_02()
        {
            InitializeComponent();
            encoder = new A4MB.Encoder(MainWindow.A4Motherboard.Ftdi_Ctrl_USB_C);
        }

        private void btn_config_Click(object sender, RoutedEventArgs e)
        {
            encoder.Ctrl(A4MB.Encoder.CreateDefaultChannels());
        }

        private void btn_autoRead_Click(object sender, RoutedEventArgs e)
        {
            Task task = Task.Run(() => encReadLoop());
        }

        private void encReadLoop()
        {
            while (true)
            {
                uint RawData_E1 = encoder.ReadENC1();
                uint RawData_E2 = encoder.ReadENC2();
                uint RawData_E3 = encoder.ReadENC3();
                uint RawData_E4 = encoder.ReadENC4();



                Dispatcher.Invoke(() =>
                {
                    txt_enc1.Text = RawData_E1.ToString();
                    txt_enc2.Text = RawData_E2.ToString();
                    txt_enc3.Text = RawData_E3.ToString();
                    txt_enc4.Text = RawData_E4.ToString();
                });
                Thread.Sleep(100);
            }
        }
    }
}
