using FTD2XX_NET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace A4_BurstMode_test.A4_MB_SDK
{
    internal class A4MB
    {
        #region Read Command
        public const byte
        Rcmd_CHIP_READ = 0x00,
        Rcmd_PERIPH_READ = 0x01,
        Rcmd_SIN12_READ = 0x02,
        Rcmd_SIN34_READ = 0x03,
        Rcmd_SIN56_READ = 0x04,
        Rcmd_INT_FLAG = 0x05,
        Rcmd_INT12_FLAG = 0x06,
        Rcmd_INT34_FLAG = 0x07,
        Rcmd_INT56_FLAG = 0x08,
        Rcmd_IO_PROM_FLAG = 0x09,
        Rcmd_PROM_READ1 = 0x0A,
        Rcmd_PROM_READ2 = 0x0B,
        Rcmd_PROM_READ3 = 0x0C,
        Rcmd_PROM_READ4 = 0x0D,
        Rcmd_Reserve_0x0E = 0x0E,
        Rcmd_PMD_MOT_FLAG = 0x0F,
        Rcmd_BURST_FLAG = 0x10,
        Rcmd_ENC_FLAG = 0x11,
        Rcmd_ENC1_READ = 0x12,
        Rcmd_ENC2_READ = 0x13,
        Rcmd_ENC3_READ = 0x14,
        Rcmd_ENC4_READ = 0x15,
        Rcmd_ADC_FLAG = 0x16,
        Rcmd_ADC1_READ32 = 0x17,
        Rcmd_ADC2_READ32 = 0x18,
        Rcmd_ADC3_READ32 = 0x19,
        Rcmd_ADC4_READ32 = 0x1A,
        Rcmd_Reserve_0x1B = 0x1B,
        Rcmd_DSP1_READ32 = 0x1C,
        Rcmd_DSP2_READ32 = 0x1D,
        Rcmd_DSP3_READ32 = 0x1E,
        Rcmd_DSP4_READ32 = 0x1F,
        Rcmd_ADC1_READ24 = 0x20,
        Rcmd_ADC2_READ24 = 0x21,
        Rcmd_ADC3_READ24 = 0x22,
        Rcmd_ADC4_READ24 = 0x23;
        #endregion
        #region Write Command
        const byte 
            Wcmd_SINGLE_READ = 0x00,
            Wcmd_PERIPH_CTRL = 0x01,
            Wcmd_SOUT12_CTRL = 0x02,
            Wcmd_SOUT34_CTRL = 0x03,
            Wcmd_SOUT56_CTRL = 0x04,
            Wcmd_INT_CONFIG = 0x05,
            Wcmd_INT12_CTRL = 0x06,
            Wcmd_INT34_CTRL = 0x07,
            Wcmd_INT56_CTRL = 0x08,
            Wcmd_PROM_CTRL = 0x09,
            Wcmd_PROM_WRITE1 = 0x0A,
            Wcmd_PROM_WRITE2 = 0x0B,
            Wcmd_PROM_WRITE3 = 0x0C,
            Wcmd_PROM_WRITE4 = 0x0D,
            Wcmd_UART_CTRL = 0x0E,
            Wcmd_PMD_MOT_CTRL = 0x0F,
            Wcmd_BURST_CTRL = 0x10,
            Wcmd_ENC_CTRL = 0x11,
            Wcmd_ENC1_WRITE = 0x12,
            Wcmd_ENC2_WRITE = 0x13,
            Wcmd_ENC3_WRITE = 0x14,
            Wcmd_ENC4_WRITE = 0x15,
            Wcmd_ADC12_CTRL = 0x16,
            Wcmd_ADC34_CTRL = 0x17,
            Wcmd_ADC1_WRITE = 0x18,
            Wcmd_ADC2_WRITE = 0x19,
            Wcmd_ADC3_WRITE = 0x1A,
            Wcmd_ADC4_WRITE = 0x1B,
            Wcmd_DSP_CTRL = 0x1C;
        #endregion
        // 每個主板有自己的 FTDI_Ctrl
        public FTDI_Ctrl Ftdi_Ctrl_USB_A { get; set; }//FrameWare write port
        public FTDI_Ctrl Ftdi_Ctrl_USB_B { get; set; }//Burst mode port(Only Read)
        public FTDI_Ctrl Ftdi_Ctrl_USB_C { get; set; }//Nomral CMD port
        public FTDI_Ctrl Ftdi_Ctrl_USB_D { get; set; }
        public MAX11270_AD_Converter ADConverter { get; private set; }
        // public MemoryChip Memory { get; private set; }
        // public MotionChip Motion { get; private set; }


        // 建構子傳入 FTDI 裝置
        public A4MB(FTDI Ftdi_USB_A, FTDI Ftdi_USB_B, FTDI Ftdi_USB_C, FTDI Ftdi_USB_D)
        {
            Ftdi_Ctrl_USB_A = new FTDI_Ctrl(Ftdi_USB_A);
            Ftdi_Ctrl_USB_B = new FTDI_Ctrl(Ftdi_USB_B);
            Ftdi_Ctrl_USB_C = new FTDI_Ctrl(Ftdi_USB_C);
            Ftdi_Ctrl_USB_D = new FTDI_Ctrl(Ftdi_USB_D);
            // 初始化子元件
            ADConverter = new MAX11270_AD_Converter(Ftdi_Ctrl_USB_C);
            //  Memory = new MemoryChip(Ctrl);
            //  Motion = new MotionChip(Ctrl);
        }

        public class FTDI_Ctrl
        {
            private FTDI fTDI;
            int FtdiCount = 0;
            public FTDI_Ctrl(FTDI ftdi)
            {
                fTDI = ftdi ?? throw new ArgumentNullException(nameof(ftdi));
            }

            public void FTDI_Preprocessing(String SerialNumber)//FTDI預設連線流程
            {
                Init();
                Connect(SerialNumber);
                ConnectSetting(0, 115200, 8, 1, 2);
            }

            public string Init()
            {
                // 取得裝置數量
                uint ftdiDeviceCount = 0;
                FtdiCount = (int)ftdiDeviceCount;
                FTDI.FT_STATUS status = fTDI.GetNumberOfDevices(ref ftdiDeviceCount);

                if (status != FTDI.FT_STATUS.FT_OK || ftdiDeviceCount == 0)
                {
                    fTDI.GetDescription(out string ftdiDescription);
                    throw new InvalidOperationException("無法取得 FTDI 裝置數量" + ftdiDescription);
                }

                FtdiCount = (int)ftdiDeviceCount;
                Console.WriteLine("找到 " + ftdiDeviceCount + " 個 FTDI 裝置");
                return $"找到 {ftdiDeviceCount} 個裝置";

            }

            public void Connect(String SerialNumber)
            {
                FTDI.FT_DEVICE_INFO_NODE[] deviceList = new FTDI.FT_DEVICE_INFO_NODE[FtdiCount];
                FTDI.FT_STATUS status = fTDI.GetDeviceList(deviceList);
                if (status != FTDI.FT_STATUS.FT_OK)
                {
                    fTDI.GetDescription(out string ftdiDescription);
                    throw new InvalidOperationException("無法取得裝置清單，請檢查連接或驅動程式" + ftdiDescription);
                }

                //if (fTDI.IsOpen != true)
                //{
                //    if (fTDI.OpenByIndex(2) != FTDI.FT_STATUS.FT_OK)
                //    {
                //        fTDI.GetDescription(out string ftdiDescription);
                //        throw new InvalidOperationException("無法開啟 FTDI 裝置，請檢查連接或驅動程式。" + ftdiDescription);
                //    }
                //}

                if (fTDI.IsOpen != true)
                {
                    if (fTDI.OpenBySerialNumber(SerialNumber) != FTDI.FT_STATUS.FT_OK)
                    {
                        fTDI.GetDescription(out string ftdiDescription);
                        throw new InvalidOperationException("無法開啟 FTDI 裝置，請檢查連接或驅動程式。" + ftdiDescription);
                    }
                }
                status = fTDI.GetDeviceList(deviceList);


                if (status != FTDI.FT_STATUS.FT_OK || !fTDI.IsOpen)
                {

                    fTDI.GetDescription(out string ftdiDescription);
                    throw new InvalidOperationException("無法開啟裝置: " + ftdiDescription);
                }
                else if (status == FTDI.FT_STATUS.FT_OK && fTDI.IsOpen)
                {
                    fTDI.GetDescription(out string ftdiDescription);
                    Console.WriteLine("裝置已開啟: " + ftdiDescription);
                }
                else
                {
                    throw new InvalidOperationException(status.ToString());
                }
            }

            public void ConnectSetting(byte Latency, uint BaudRate, byte DataBits, byte StopBits, byte Parity)
            {
                fTDI.SetLatency(Latency);
                fTDI.SetBaudRate(BaudRate);
                fTDI.SetDataCharacteristics(DataBits, StopBits, Parity); // 8-1-Even // 0-4=None,Odd,Even,Mark,Space
                fTDI.SetFlowControl(FTDI.FT_FLOW_CONTROL.FT_FLOW_NONE, 0x11, 0x13); // 無 flow control

                FTDI.FT_DEVICE_INFO_NODE[] deviceList = new FTDI.FT_DEVICE_INFO_NODE[FtdiCount];
                FTDI.FT_STATUS status = fTDI.GetDeviceList(deviceList);
                if (status != FTDI.FT_STATUS.FT_OK)
                {
                    throw new InvalidOperationException("無法取得裝置清單");
                }
                else
                {
                    Console.WriteLine("裝置設定成功 "
                        + Environment.NewLine + "     SetLatency = " + Latency
                        + Environment.NewLine + "     SetBaudRate = " + BaudRate
                        + Environment.NewLine + "      DataBits =  " + DataBits + ", StopBits = " + StopBits + ", Parity = " + Parity
                        + Environment.NewLine + "     SetFlowControl = No flow control"
                        );
                }
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

                if (Send != null && Send.Length >= 6)
                {
                    uint bytesWritten = 0;
                    if (fTDI.Write(Send, Send.Length, ref bytesWritten) != FTDI.FT_STATUS.FT_OK)
                    {
                        throw new InvalidOperationException("寫入失敗");
                    }
                }
            }

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

            public bool PROM_is_Busy_or_Not(int CMDPage)//01=busy 10 = error
            {
                ulong data = 0x09;
                if (CMDPage == 0) data = 0x009;
                else if (CMDPage == 1) data = 0x109;
                else if (CMDPage == 2) data = 0x209;
                else if (CMDPage == 3) data = 0x309;
                Write(0x00, data);
                Thread.Sleep(10); // 等待讀取完成
                uint Data = Read(out byte add);
                uint head2Bits = Data & 0b11;
                if (head2Bits == 1)
                {
                    return true;//busy
                }
                else if (head2Bits == 2)
                {
                    throw new InvalidOperationException("PROM is error");
                }
                else
                {
                    return false;//not busy
                }
            }

            /// <summary>
            ///Read the value of the command position.
            /// <para>EX: uint? Val = Single_Read( 3 , 0x16 );  -> Trun to page 3 and Val will get retrun value from 0x16 command</para>
            /// </summary>
            public uint? Single_Read(uint WhichPageToTurnTo, uint Command)
            {
                byte[] DataArray = new byte[4];
                DataArray[0] = 0;
                DataArray[1] = 0;
                DataArray[2] = (byte)(WhichPageToTurnTo | 0x80); // Set
                DataArray[3] = (byte)(Command & 0xFF);

                ulong packet = 0;
                packet |= ((ulong)DataArray[0]) << 24;
                packet |= ((ulong)DataArray[1]) << 16;
                packet |= ((ulong)DataArray[2]) << 8;
                packet |= ((ulong)DataArray[3]);

                Write(0x00, packet);
                return Read(out byte Addr);
            }

            public uint? Single_Read(uint Command)
            {
                byte[] DataArray = new byte[4];
                DataArray[0] = 0;
                DataArray[1] = 0;
                DataArray[2] = (byte)(0 | 0x80); // Set
                DataArray[3] = (byte)(Command & 0xFF);

                ulong packet = 0;
                packet |= ((ulong)DataArray[0]) << 24;
                packet |= ((ulong)DataArray[1]) << 16;
                packet |= ((ulong)DataArray[2]) << 8;
                packet |= ((ulong)DataArray[3]);

                Write(0x00, packet);
                return Read(out byte Addr);
            }

            /// <summary>
            /// 僅發送命令，不做讀取，故不回傳任何值
            /// </summary>
            /// <param name="WhichPageToTurnTo"></param>
            /// <param name="Command"></param>
            public void Single_Read_OnlyWriteCommand(uint WhichPageToTurnTo, uint Command)
            {
                byte[] DataArray = new byte[4];
                DataArray[0] = 0;
                DataArray[1] = 0;
                DataArray[2] = (byte)(WhichPageToTurnTo | 0x80); // Set
                DataArray[3] = (byte)(Command & 0xFF);

                ulong packet = 0;
                packet |= ((ulong)DataArray[0]) << 24;
                packet |= ((ulong)DataArray[1]) << 16;
                packet |= ((ulong)DataArray[2]) << 8;
                packet |= ((ulong)DataArray[3]);

                Write(0x00, packet);
            }

            /// <summary>
            /// 僅發送命令，不做讀取，故不回傳任何值
            /// </summary>
            /// <param name="WhichPageToTurnTo"></param>
            /// <param name="Command"></param>
            public void Single_Read_OnlyWriteCommand(uint Command)
            {
                byte[] DataArray = new byte[4];
                DataArray[0] = 0;
                DataArray[1] = 0;
                DataArray[2] = (byte)(0 | 0x80); // Set
                DataArray[3] = (byte)(Command & 0xFF);

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
            private FTDI FtdiBurstRead;
            private FTDI FtdiNormalCtrl;
            //public MAX11270_AD_Converter(FTDI_Ctrl FtdiBurstRead, FTDI_Ctrl FtdiNormalCtrl);
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
                uint? readData = ctrl.Single_Read(Rcmd_ADC_FLAG);
                //uint readData =  ctrl.Read(out byte Addr);
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


            #region  ADC12_CTRL_0x16
            /// <summary>
            /// Reserved bits, always "00"
            /// </summary>
            private const int ADC12_CTRL_0x16_Parameter_Bit31_30 = 0;

            /// <summary>
            /// Reserved bits, always "00"
            /// </summary>
            private const int ADC12_CTRL_0x16_Parameter_Bit15_14 = 0;


            public class ADC12_CTRL_0x16_Parameter
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

            internal uint ADC12_CTRL_0x16_BuildPacket(ADC12_CTRL_0x16_Parameter p)
            {
                uint value = 0;
                value |= (uint)(ADC12_CTRL_0x16_Parameter_Bit31_30 & 0b11) << 30;
                value |= (uint)(p.Bit29 & 0b1) << 29;
                value |= (uint)(p.Bit28 & 0b1) << 28;
                value |= (uint)(p.Bit27 & 0b1) << 27;
                value |= (uint)(p.Bit26_24 & 0b111) << 24;
                value |= (uint)(p.Bit23_16 & 0xFF) << 16;
                value |= (uint)(ADC12_CTRL_0x16_Parameter_Bit15_14 & 0b11) << 14;
                value |= (uint)(p.Bit13 & 0b1) << 13;
                value |= (uint)(p.Bit12 & 0b1) << 12;
                value |= (uint)(p.Bit11 & 0b1) << 11;
                value |= (uint)(p.Bit10_8 & 0b111) << 8;
                value |= (uint)(p.Bit7_0 & 0xFF);
                return value;
            }

            public void ADC12_CTRL_0x16(ADC12_CTRL_0x16_Parameter pack)
            {
                uint packet = ADC12_CTRL_0x16_BuildPacket(pack);
                ctrl.Write(Wcmd_ADC12_CTRL, packet);
            }
            #endregion

            public bool ADC1_WRITE_0x18(uint data)
            {
                try
                {
                    ctrl.Write(Wcmd_ADC1_WRITE, data);
                    return true;
                }
                catch (Exception ex)
                {
                    throw new Exception("ADC1_WRITE_0x18 Error: " + ex.Message);
                }
            }

            public bool ADC2_WRITE_0x19(uint data)
            {
                try
                {
                    ctrl.Write(Wcmd_ADC2_WRITE, data);
                    return true;
                }
                catch (Exception ex)
                {
                    throw new Exception("ADC2_WRITE_0x19 Error: " + ex.Message);
                }
            }

            public uint? ADC1_READ32_0x17()
            {
                int iFlowStep = 0;

                // Step 1: Configure ADC1 to read data
                if (SpinWait.SpinUntil(() => AD1.IsReady, 3000) && iFlowStep == 0)// 等待 ADC 準備好數據// 最多等 3 秒//步驟0
                {
                    ADC12_CTRL_0x16(new ADC12_CTRL_0x16_Parameter
                    {
                        Bit29 = 1, // Enable ADC2
                        Bit28 = 0, // disable automatic read for ADC1
                        Bit27 = 1, // Read from ADC2
                        Bit26_24 = 0b011, // 4 bytes for ADC2//go look at datasheet
                        Bit23_16 = 0x20, // Example command for ADC2//go look at datasheet

                        Bit13 = 1, // Enable ADC1
                        Bit12 = 0, // disable automatic read for ADC1
                        Bit11 = 1, // Read from ADC1
                        Bit10_8 = 0b011, // 4 bytes for ADC1//go look at datasheet
                        Bit7_0 = 0xCD // Example command for ADC1//go look at MAX11270 datasheet
                    });

                    iFlowStep = 1;
                }
                else
                {
                    return null;
                    throw new TimeoutException("等待 ADC1 準備數據超時");
                }

                if (SpinWait.SpinUntil(() => AD1.IsReady, 3000) && iFlowStep == 1)// 等待 ADC 準備好數據// 最多等 3 秒//步驟1
                {
                    return ctrl.Single_Read(Rcmd_ADC1_READ32);
                }
                else
                {
                    throw new TimeoutException("等待 ADC1 準備數據超時");
                }
            }
        }
    }
}
