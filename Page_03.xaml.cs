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

namespace A4_BurstMode_test
{
    /// <summary>
    /// Page_01.xaml 的互動邏輯
    /// </summary>
    public partial class Page_03 : Page
    {
        private List<Control> TempCBoxAndTextBox = new List<Control>();

        public Page_03()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            TempCBoxAndTextBox = new List<Control>    {
                  cb3130_0x16, cb29_0x16, cb27_0x16,
                   cb2624_0x16, txt2316_0x16, cb1514_0x16,
                    cb13_0x16, cb12_0x16, cb11_0x16,
                 cb1008_0x16, txt0700_0x16    };
            foreach (Control cb in TempCBoxAndTextBox)
            {
                if (cb is ComboBox)
                {
                    (cb as ComboBox).SelectionChanged += Cb_SelectionChanged;
                }
                else if (cb is TextBox)
                {
                    (cb as TextBox).TextChanged += Txt_TextChanged;
                }
            }
        }

        private void Txt_TextChanged(object sender, TextChangedEventArgs e)
        {
            dataGrid_ADC12_CTRL_0x16_SetData();
        }

        private void Cb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            dataGrid_ADC12_CTRL_0x16_SetData();
        }

        private void dataGrid_ADC12_CTRL_0x16_SetData()
        {
            var results = new List<string>();

            for (int i = 0; i < TempCBoxAndTextBox.Count; i++)
            {
                if (TempCBoxAndTextBox[i] is TextBox tb)
                {
                    if (int.TryParse(tb.Text, out int value))
                        results.Add(Convert.ToString(value, 2).PadLeft(8, '0')); // TextBox → 二進位字串
                    else
                        results.Add(string.Empty);
                }
                else if (TempCBoxAndTextBox[i] is ComboBox cb)
                {
                    results.Add(cb.Text);
                }
            }

            // DataGrid 顯示
            dataGrid_ADC12_CTRL_0x16.ItemsSource = results.Select(s => new { Value = s });
        }
    }
}
