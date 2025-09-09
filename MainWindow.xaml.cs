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
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            TestUnit.Test();
        }

    }

    class A4_MotherBoard
    {
        // 每個主板有自己的 FTDI_Ctrl
        public FTDI_Ctrl Ctrl { get; private set; }

        // 建構子傳入 FTDI 裝置
        public A4_MotherBoard(FTDI ftdi)
        {
            Ctrl = new FTDI_Ctrl(ftdi);

            // 初始化子元件
            ADConverter = new MAX11270_AD_Converter(Ctrl);
            //  Memory = new MemoryChip(Ctrl);
            //  Motion = new MotionChip(Ctrl);
        }

        public MAX11270_AD_Converter ADConverter { get; private set; }
        // public MemoryChip Memory { get; private set; }
        // public MotionChip Motion { get; private set; }

        public class FTDI_Ctrl
        {
            private FTDI fTDI;
            public FTDI_Ctrl(FTDI ftdi)
            {
                fTDI = ftdi ?? throw new ArgumentNullException(nameof(ftdi));
            }
            public void Write(byte command, ulong DATA)
            {
                byte[] Send = new byte[6];
                // 命令
                Send[0] = (byte)(command & 0xFF);
                // data 拆成 5 個 7-bit，加上最高位 1
                Send[1] = (byte)(((DATA >> 28) & 0x0F) | 0x80); // bit 31~28 (4bit)
                Send[2] = (byte)(((DATA >> 21) & 0x7F) | 0x80); // bit 27~21 (7bit)
                Send[3] = (byte)(((DATA >> 14) & 0x7F) | 0x80); // bit 20~14 (7bit)
                Send[4] = (byte)(((DATA >> 7) & 0x7F) | 0x80);  // bit 13~7 (7bit)
                Send[5] = (byte)((DATA & 0x7F) | 0x80);         // bit 6~0  (7bit)
                string allbyte =
                    Convert.ToString(Send[0], 2) + "," + Convert.ToString(Send[1], 2) + ","
                    + Convert.ToString(Send[2], 2) + "," + Convert.ToString(Send[3], 2) + ","
                    + Convert.ToString(Send[4], 2) + "," + Convert.ToString(Send[5], 2);
                if (Send != null && Send.Length >= 6)
                {
                    uint bytesWritten = 0;
                    if (fTDI.Write(Send, Send.Length, ref bytesWritten) != FTDI.FT_STATUS.FT_OK)
                    {
                        MessageBox.Show("寫入失敗");
                        return;
                    }
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="fTDI"></param>
            /// <param name="ReadByteArrayLength"> Expected array length</param>
            /// <returns></returns>
            public uint Read(out byte address)
            {
                byte[] recv = new byte[6];

                uint bytesRead = 0;
                fTDI.SetTimeouts(100, 100);
                FTDI.FT_STATUS status = fTDI.Read(recv, 6, ref bytesRead);

                if (status != FTD2XX_NET.FTDI.FT_STATUS.FT_OK)
                    throw new InvalidOperationException("讀取失敗");

                if (bytesRead < 6)
                    throw new InvalidOperationException("資料不足，讀取到的 byte 數量太少");

                address = recv[0];

                // 把標誌位 (bit7) 清掉，只保留低 7 bit
                uint part1 = (uint)(recv[1] & 0x0F); // 4bit
                uint part2 = (uint)(recv[2] & 0x7F); // 7bit
                uint part3 = (uint)(recv[3] & 0x7F); // 7bit
                uint part4 = (uint)(recv[4] & 0x7F); // 7bit
                uint part5 = (uint)(recv[5] & 0x7F); // 7bit

                // 組回原本的 32bit DATA
                uint data = (part1 << 28) |
                            (part2 << 21) |
                            (part3 << 14) |
                            (part4 << 7) |
                            (part5);

                return data;
            }
            public void Single_Read(uint CMD_Page, uint Read_addr)
            {
                byte[] DataArray = new byte[4];
                DataArray[0] = 0;
                DataArray[1] = 0;
                DataArray[2] = (byte)(CMD_Page | 0x80); // Set
                DataArray[3] = (byte)(Read_addr & 0xFF);

                ulong packet = 0;
                packet |= ((ulong)DataArray[0]) << 24;
                packet |= ((ulong)DataArray[1]) << 16;
                packet |= ((ulong)DataArray[2]) << 8;
                packet |= ((ulong)DataArray[3]);

                Write(0x00, packet);
            }
        }


        public class MAX11270_AD_Converter
        {
            private FTDI_Ctrl ctrl;
            public MAX11270_AD_Converter(FTDI_Ctrl ctrl)
            {
                this.ctrl = ctrl;

                // 初始化四個通道，index 對應 resuld[0]~resuld[3]
                AD1 = new ADChannel(this, 3);
                AD2 = new ADChannel(this, 2);
                AD3 = new ADChannel(this, 1);
                AD4 = new ADChannel(this, 0);
            }

            public ADChannel AD1 { get; }
            public ADChannel AD2 { get; }
            public ADChannel AD3 { get; }
            public ADChannel AD4 { get; }


            internal byte[] ReadStatus()
            {
                ctrl.Single_Read(0x00, 0x00000016);
                uint readData = ctrl.Read(out byte address);
                byte[] result = new byte[4];
                result[0] = (byte)((readData >> 24) & 0xFF);
                result[1] = (byte)((readData >> 16) & 0xFF);
                result[2] = (byte)((readData >> 8) & 0xFF);
                result[3] = (byte)(readData & 0xFF);
                return result;
            }
            public class ADChannel
            {
                private readonly MAX11270_AD_Converter parent;
                private readonly int index;

                internal ADChannel(MAX11270_AD_Converter parent, int index)
                {
                    this.parent = parent;
                    this.index = index;
                }

                public bool IsReady
                {
                    get
                    {
                        byte[] res = parent.ReadStatus();
                        return (res[index] & 0b0000_0001) != 0;  // 最低位元
                    }
                }

                public bool IsBusy
                {
                    get
                    {
                        byte[] res = parent.ReadStatus();
                        return (res[index] & 0b0000_0010) != 0;  // 倒數第二位元
                    }
                }
            }


            #region ADC_Control_0x16
            /// <summary>
            /// Reserved bits, always "00"
            /// </summary>
            private const int ADC_Control_0x16_Pack_Bit31_30 = 0;

            /// <summary>
            /// Reserved bits, always "00"
            /// </summary>
            private const int ADC_Control_0x16_Pack_Bit15_14 = 0;


            public class ADC_Control_0x16_Pack
            {
                /// <summary>
                /// ADC2_EN: ADC2 Enable control
                /// 0 = ADC2 disabled
                /// 1 = ADC2 enabled
                /// </summary>
                public int Bit29 { get; set; }

                /// <summary>
                /// ADC2_AUTORD = Automatic Read control with command 0xCD Read
                /// 0 = Disable automatic read of ADC2 data
                /// 1 = Enable automatic read of ADC2 data
                /// </summary>
                public int Bit28 { get; set; }

                /// <summary>
                /// ADC2_RW = ADC2 Read/Write control
                /// 0 = Write to ADC2
                /// 1= Read from ADC2
                /// </summary>
                public int Bit27 { get; set; }

                /// <summary>
                /// ADC2_BYTEx = ADC2 Byte set
                /// 000 = 0 bytes
                /// 001 = 1 byte
                /// 010 = 2 bytes
                /// 011 = 3 bytes
                /// 100 = 4 bytes
                /// 101 = 5 bytes
                /// 110 = 6 bytes
                /// 111 = 7 bytes
                /// </summary>
                public int Bit26_24 { get; set; }

                /// <summary>
                /// ADX2_CMD = ADC2 Command
                /// </summary>
                public int Bit23_16 { get; set; }


                /// <summary>
                /// ACD1_EN = ADC1 Enable control
                /// 0 = ADC1 disabled
                /// 1 = ADC1 enabled
                /// </summary>
                public int Bit13 { get; set; }

                /// <summary>
                /// ADC1_AUTORD = ADC1 Automatic Read control with command 0xCD Read
                /// 0 = Disable automatic read of ADC1 data
                /// 1 = Enable automatic read of ADC1 data
                /// </summary>
                public int Bit12 { get; set; }

                /// <summary>
                /// ADC1_RW = ADC1 Read/Write control
                /// 0 = Write to ADC1
                /// 1= Read from ADC1
                /// </summary>
                public int Bit11 { get; set; }

                /// <summary>
                /// ADC1_BYTEx = ADC1 Byte set
                /// 000 = 0 bytes
                /// 001 = 1 byte
                /// 010 = 2 bytes
                /// 011 = 3 bytes
                /// 100 = 4 bytes
                /// 101 = 5 bytes
                /// 110 = 6 bytes
                /// 111 = 7 bytes
                /// </summary>
                public int Bit10_8 { get; set; }

                /// <summary>
                /// ADC1_CMD = ADC1 Command
                /// </summary>
                public int Bit7_0 { get; set; }

            }

            public uint BuildPacket(ADC_Control_0x16_Pack p)
            {
                uint value = 0;
                value |= (uint)(ADC_Control_0x16_Pack_Bit31_30 & 0b11) << 30;
                value |= (uint)(p.Bit29 & 0b1) << 29;
                value |= (uint)(p.Bit28 & 0b1) << 28;
                value |= (uint)(p.Bit27 & 0b1) << 27;
                value |= (uint)(p.Bit26_24 & 0b111) << 24;
                value |= (uint)(p.Bit23_16 & 0xFF) << 16;
                value |= (uint)(ADC_Control_0x16_Pack_Bit15_14 & 0b11) << 14;
                value |= (uint)(p.Bit13 & 0b1) << 13;
                value |= (uint)(p.Bit12 & 0b1) << 12;
                value |= (uint)(p.Bit11 & 0b1) << 11;
                value |= (uint)(p.Bit10_8 & 0b111) << 8;
                value |= (uint)(p.Bit7_0 & 0xFF);
                return value;
            }

            public void ADC_Control_0x16(ADC_Control_0x16_Pack pack)
            {
                uint packet = BuildPacket(pack);
                ctrl.Write(0x16, packet);
            }
            #endregion




        }




    }

    class TestUnit
    {
        static FTDI ftdi = new FTDI();
        public static void Test()
        {
            // 初始化 FTDI 裝置
            fn_Ftdi_Init(out int FtdiCount);
            fn_Ftdi_Connect(FtdiCount, 0);
            fn_Ftdi_Setting(FtdiCount);

            // 初始化 A4_MotherBoard
            A4_MotherBoard motherboard = new A4_MotherBoard(ftdi);
            fn_IfConnectedWillMakeSound(motherboard);//目前完成到這裡
            motherboard.Ctrl.Write(0x00, 0x09);
            uint data = motherboard.Ctrl.Read(out byte Addr);
            bool a = motherboard.ADConverter.AD1.IsReady;
            bool b = motherboard.ADConverter.AD2.IsReady;
            bool c = motherboard.ADConverter.AD3.IsReady;
            bool d = motherboard.ADConverter.AD4.IsReady;

            bool a1 = motherboard.ADConverter.AD1.IsBusy;
            bool b1 = motherboard.ADConverter.AD2.IsBusy;
            bool c1 = motherboard.ADConverter.AD3.IsBusy;
            bool d1 = motherboard.ADConverter.AD4.IsBusy;

            // 配置 ADC 控制寄存器
            var adcControlPack = new A4_MotherBoard.MAX11270_AD_Converter.ADC_Control_0x16_Pack
            {
                Bit29 = 1, // Enable ADC2
                Bit28 = 0xCD, // Enable automatic read for ADC2
                Bit27 = 1, // Read from ADC2
                Bit26_24 = 0b100, // 4 bytes for ADC2
                Bit23_16 = 0x01, // Example command for ADC2
                Bit13 = 1, // Enable ADC1
                Bit12 = 0xCD, // Enable automatic read for ADC1
                Bit11 = 0, // Read from ADC1
                Bit10_8 = 0b100, // 4 bytes for ADC1
                Bit7_0 = 0x01 // Example command for ADC1
            };
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
        static public string fn_Ftdi_Init(out int FindFtdiCount)
        {
            // 取得裝置數量
            uint ftdiDeviceCount = 0;
            FindFtdiCount = (int)ftdiDeviceCount;
            FTDI.FT_STATUS status = ftdi.GetNumberOfDevices(ref ftdiDeviceCount);

            if (status != FTDI.FT_STATUS.FT_OK || ftdiDeviceCount == 0)
            {
                ftdi.GetDescription(out string ftdiDescription);
                MessageBox.Show("無法取得 FTDI 裝置數量" + ftdiDescription);
                return "找不到 FTDI 裝置";
            }

            FindFtdiCount = (int)ftdiDeviceCount;
            Console.WriteLine("找到 " + ftdiDeviceCount + " 個 FTDI 裝置");
            return $"找到 {ftdiDeviceCount} 個裝置";
        }

        static public string fn_Ftdi_Connect(int count, int Index)
        {
            FTDI.FT_DEVICE_INFO_NODE[] deviceList = new FTDI.FT_DEVICE_INFO_NODE[count];
            FTDI.FT_STATUS status = ftdi.GetDeviceList(deviceList);
            if (status != FTDI.FT_STATUS.FT_OK)
            {
                ftdi.GetDescription(out string ftdiDescription);
                MessageBox.Show("無法取得裝置清單，請檢查連接或驅動程式" + ftdiDescription);
                return ("無法取得裝置清單");
            }

            if (ftdi.IsOpen != true)
            {
                if (ftdi.OpenByIndex((uint)Index) != FTDI.FT_STATUS.FT_OK)
                {
                    ftdi.GetDescription(out string ftdiDescription);
                    MessageBox.Show("無法開啟 FTDI 裝置，請檢查連接或驅動程式。" + ftdiDescription);
                }
            }
            status = ftdi.GetDeviceList(deviceList);


            if (status != FTDI.FT_STATUS.FT_OK || !ftdi.IsOpen)
            {

                ftdi.GetDescription(out string ftdiDescription);
                MessageBox.Show("無法開啟裝置: " + ftdiDescription);
                return ("無法開啟裝置" + ftdiDescription);
            }
            else if (status == FTDI.FT_STATUS.FT_OK && ftdi.IsOpen)
            {
                ftdi.GetDescription(out string ftdiDescription);
                //fn_FeedBackMessage("裝置已開啟: " + ftdiDescription);
                Console.WriteLine("裝置已開啟: " + ftdiDescription);
                return ("裝置已開啟" + ftdiDescription);
            }
            else
            {
                return (status.ToString());
            }
        }

        static public string fn_Ftdi_Setting(int count)
        {
            ftdi.SetLatency(0);
            ftdi.SetBaudRate(115200);
            ftdi.SetDataCharacteristics(8, 1, 2); // 8-1-Even // 0-4=None,Odd,Even,Mark,Space
            ftdi.SetFlowControl(FTDI.FT_FLOW_CONTROL.FT_FLOW_NONE, 0x11, 0x13); // 無 flow control

            FTDI.FT_DEVICE_INFO_NODE[] deviceList = new FTDI.FT_DEVICE_INFO_NODE[count];
            FTDI.FT_STATUS status = ftdi.GetDeviceList(deviceList);
            if (status != FTDI.FT_STATUS.FT_OK)
            {
                return ("無法取得裝置清單");
            }
            else
            {
                return ("裝置設定成功 "
                    + Environment.NewLine + "     SetLatency = 0"
                    + Environment.NewLine + "     SetBaudRate = 115200"
                    + Environment.NewLine + "     SetDataCharacteristics =  DataBits = 8 , StopBits = 1 ,Parity = 2"
                    + Environment.NewLine + "     SetFlowControl = No flow control"
                    );
            }
        }

        static public void fn_IfConnectedWillMakeSound(A4_MotherBoard motherboard)
        {
            motherboard.Ctrl.Write(0x01, 0x20000);
            //motherboard.Ctrl.Write(0x01, 0x20000);
            Thread.Sleep(100);

            motherboard.Ctrl.Write(0x01, 0x00000);
            //motherboard.Ctrl.Write(0x01, 0x00000);
            Thread.Sleep(50);

            motherboard.Ctrl.Write(0x01, 0x20000);
            //motherboard.Ctrl.Write(0x01, 0x20000);
            Thread.Sleep(50);

            motherboard.Ctrl.Write(0x01, 0x00000);
            //motherboard.Ctrl.Write(0x01, 0x00000);
            Thread.Sleep(50);

            motherboard.Ctrl.Write(0x01, 0x20000);
            //motherboard.Ctrl.Write(0x01, 0x20000);
            Thread.Sleep(50);

            motherboard.Ctrl.Write(0x01, 0x00000);
            //motherboard.Ctrl.Write(0x01, 0x00000);
            Thread.Sleep(50);
        }
        #endregion
    }


}

