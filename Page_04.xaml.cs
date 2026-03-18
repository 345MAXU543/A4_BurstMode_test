using A4_BurstMode_test.A4_MB_SDK;
using DynamicData;
using System;
using System.Collections.Generic;
using System.Data;
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
    public partial class Page_04 : Page
    {
        A4MB.PROM pROM;
        public Page_04()
        {
            InitializeComponent();
            pROM = MainWindow.A4Motherboard.Prom;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void btn_PageWrite_Click(object sender, RoutedEventArgs e)
        {
            string allData = txt_WriteAllData_HEX.Text.Trim();
            string[] dataArray = allData.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            uint[] Data = new uint[16];
            for (int i = 0; i < 16; i++)
            {
                    Data[i] = Convert.ToUInt32(dataArray[i], 16);
            }
            pROM.PageWrite(A4MB.PROM.Hardware.Motherboard, 0x00, A4MB.PROM.ByteSize.byte64, Data);
        }

        private void btn_PageRead_Click(object sender, RoutedEventArgs e)
        {
            uint[]Data = new uint[16];
            Data = pROM.PageRead(A4MB.PROM.Hardware.Motherboard, 0x00, A4MB.PROM.ByteSize.byte64);
            txt_ReadAllData_HEX.Clear();
            string[] str_Data = new string[16];
            for (int i = 0; i < 16; i++)
            {
                str_Data[i] = Data[i].ToString("X8");
            }

            for (int i = 0; i < 16; i++)
            {
                txt_ReadAllData_HEX.AppendText(str_Data[i] + " ");
            }

            putDataToTextBox_ReadGroup();
        }

        private void btn_RandomData_Click(object sender, RoutedEventArgs e)
        {
            txt_WriteAllData_HEX.Clear();

            Random rnd = new Random();

            for (int i = 0; i < 16; i++)
            {
                uint randomData = (uint)rnd.Next(0, int.MaxValue);
                uint hexData = 0xFFFFFFFF & randomData;
                txt_WriteAllData_HEX.AppendText(hexData.ToString("X8") + " ");
            }
            putDataToTextBox_WriteGroup();
        }

        //把txt_WriteAllData_HEX中的資料每八個字元切割並存入陣列，然後放到各個textBox
        private void putDataToTextBox_WriteGroup()
        {
            string allData = txt_WriteAllData_HEX.Text.Trim();
            string[] dataArray = allData.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (dataArray.Length >= 16)
            {
                txt_10.Text = dataArray[0];
                txt_20.Text = dataArray[1];
                txt_30.Text = dataArray[2];
                txt_40.Text = dataArray[3];

                txt_11.Text = dataArray[4];
                txt_21.Text = dataArray[5];
                txt_31.Text = dataArray[6];
                txt_41.Text = dataArray[7];

                txt_12.Text = dataArray[8];
                txt_22.Text = dataArray[9];
                txt_32.Text = dataArray[10];
                txt_42.Text = dataArray[11];

                txt_13.Text = dataArray[12];
                txt_23.Text = dataArray[13];
                txt_33.Text = dataArray[14];
                txt_43.Text = dataArray[15];
            }
            else
            {
                MessageBox.Show("請輸入至少16組八位元十六進位資料，以空格分隔。", "資料不足", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void putDataToTextBox_ReadGroup()
        {
            string allData = txt_ReadAllData_HEX.Text.Trim();
            string[] dataArray = allData.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (dataArray.Length >= 16)
            {
                txt_10_Read.Text = dataArray[0];
                txt_20_Read.Text = dataArray[1];
                txt_30_Read.Text = dataArray[2];
                txt_40_Read.Text = dataArray[3];

                txt_11_Read.Text = dataArray[4];
                txt_21_Read.Text = dataArray[5];
                txt_31_Read.Text = dataArray[6];
                txt_41_Read.Text = dataArray[7];

                txt_12_Read.Text = dataArray[8];
                txt_22_Read.Text = dataArray[9];
                txt_32_Read.Text = dataArray[10];
                txt_42_Read.Text = dataArray[11];

                txt_13_Read.Text = dataArray[12];
                txt_23_Read.Text = dataArray[13];
                txt_33_Read.Text = dataArray[14];
                txt_43_Read.Text = dataArray[15];
            }
            else
            {
                MessageBox.Show("請輸入至少16組八位元十六進位資料，以空格分隔。", "資料不足", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void btn_SendCtrlPar_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
