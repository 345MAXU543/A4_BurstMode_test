using FTD2XX_NET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Remoting.Channels;
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
            public bool Write(ulong CMD, ulong data)
            {
                byte[] Data_buffer = BitConverter.GetBytes(data);
                byte[] CMD_buffer = BitConverter.GetBytes(CMD);
                byte[] buffer = new byte[Data_buffer.Length + CMD_buffer.Length];
                Array.Copy(CMD_buffer, 0, buffer, 0, CMD_buffer.Length);
                Array.Copy(Data_buffer, 0, buffer, CMD_buffer.Length, Data_buffer.Length);
                uint bytesWritten = 0;
                FT_STATUS result = fTDI.Write(buffer, (uint)buffer.Length, ref bytesWritten);
                if (result == FT_STATUS.FT_OK && buffer.Length == bytesWritten)
                {
                    return true;
                }
                else
                {
                    throw new InvalidOperationException("Write Error");
                }
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

            /// <summary>
            /// 
            /// </summary>
            /// <param name="fTDI"></param>
            /// <param name="ReadByteArrayLength"> Expected array length</param>
            /// <returns></returns>
            public byte[] Read(uint ReadByteArrayLength)
            {
                byte[] recv = new byte[ReadByteArrayLength];
                uint bytesRead = 0;
                fTDI.SetTimeouts(100, 100);
                FTDI.FT_STATUS status = fTDI.Read(recv, ReadByteArrayLength, ref bytesRead);

                if (status != FTD2XX_NET.FTDI.FT_STATUS.FT_OK)
                    throw new InvalidOperationException("讀取失敗");

                if (bytesRead < ReadByteArrayLength)
                    throw new InvalidOperationException("資料不足，讀取到的 byte 數量太少");

                return recv;
            }
        }


        public class MAX11270_AD_Converter
        {
            private FTDI_Ctrl ctrl;
            public MAX11270_AD_Converter(FTDI_Ctrl ctrl)
            {
                this.ctrl = ctrl;

                // 初始化四個通道，index 對應 resuld[0]~resuld[3]
                AD1 = new ADChannel(this, 0);
                AD2 = new ADChannel(this, 1);
                AD3 = new ADChannel(this, 2);
                AD4 = new ADChannel(this, 3);
            }

            public ADChannel AD1 { get; }
            public ADChannel AD2 { get; }
            public ADChannel AD3 { get; }
            public ADChannel AD4 { get; }


            internal byte[] ReadStatus()
            {
                ctrl.Single_Read(0x00, 0x00000016);
                return ctrl.Read(4);
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

            public bool ADC_Control_0x16(ADC_Control_0x16_Pack pack)
            {
                uint packet = BuildPacket(pack);
                return ctrl.Write(0x16, packet);
            }
            #endregion

           


        }




    }
}
