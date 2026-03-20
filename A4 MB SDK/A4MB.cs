using DynamicData;
using FTD2XX_NET;
using Microsoft.SqlServer.Server;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using static A4_BurstMode_test.A4_MB_SDK.A4MB.MAX11270_AD_Converter;

namespace A4_BurstMode_test.A4_MB_SDK
{
    /// <summary>
    /// A4MB 主板 SDK，負責與 FTDI 裝置通訊與 AD 轉換器控制。
    /// </summary>
    public class A4MB
    {
        #region Read Command
        // 讀取命令常數定義
        public const byte
        Rcmd_CHIP_READ = 0x00,         // 讀取晶片
        Rcmd_PERIPH_READ = 0x01,       // 讀取周邊
        Rcmd_SIN12_READ = 0x02,        // 讀取 SIN12
        Rcmd_SIN34_READ = 0x03,        // 讀取 SIN34
        Rcmd_SIN56_READ = 0x04,        // 讀取 SIN56
        Rcmd_INT_FLAG = 0x05,          // 中斷旗標
        Rcmd_INT12_FLAG = 0x06,        // INT12 旗標
        Rcmd_INT34_FLAG = 0x07,        // INT34 旗標
        Rcmd_INT56_FLAG = 0x08,        // INT56 旗標
        Rcmd_IO_PROM_FLAG = 0x09,      // IO PROM 旗標
        Rcmd_PROM_READ1 = 0x0A,        // PROM 讀取1
        Rcmd_PROM_READ2 = 0x0B,        // PROM 讀取2
        Rcmd_PROM_READ3 = 0x0C,        // PROM 讀取3
        Rcmd_PROM_READ4 = 0x0D,        // PROM 讀取4
        Rcmd_Reserve_0x0E = 0x0E,      // 保留
        Rcmd_PMD_MOT_FLAG = 0x0F,      // PMD 馬達旗標
        Rcmd_BURST_FLAG = 0x10,        // Burst 旗標
        Rcmd_ENC_FLAG = 0x11,          // 編碼器旗標
        Rcmd_ENC1_READ = 0x12,         // 讀取編碼器1
        Rcmd_ENC2_READ = 0x13,         // 讀取編碼器2
        Rcmd_ENC3_READ = 0x14,         // 讀取編碼器3
        Rcmd_ENC4_READ = 0x15,         // 讀取編碼器4
        Rcmd_ADC_FLAG = 0x16,          // ADC 旗標
        Rcmd_ADC1_READ32 = 0x17,       // 讀取 ADC1 32bit
        Rcmd_ADC2_READ32 = 0x18,       // 讀取 ADC2 32bit
        Rcmd_ADC3_READ32 = 0x19,       // 讀取 ADC3 32bit
        Rcmd_ADC4_READ32 = 0x1A,       // 讀取 ADC4 32bit
        Rcmd_Reserve_0x1B = 0x1B,      // 保留
        Rcmd_DSP1_READ32 = 0x1C,       // 讀取 DSP1 32bit
        Rcmd_DSP2_READ32 = 0x1D,       // 讀取 DSP2 32bit
        Rcmd_DSP3_READ32 = 0x1E,       // 讀取 DSP3 32bit
        Rcmd_DSP4_READ32 = 0x1F,       // 讀取 DSP4 32bit
        Rcmd_ADC1_READ24 = 0x20,       // 讀取 ADC1 24bit
        Rcmd_ADC2_READ24 = 0x21,       // 讀取 ADC2 24bit
        Rcmd_ADC3_READ24 = 0x22,       // 讀取 ADC3 24bit
        Rcmd_ADC4_READ24 = 0x23,       // 讀取 ADC4 24bit

        Rcmd_ADC1_READ24_burstMode = 0x60,       // 讀取 ADC1 24bit
        Rcmd_ADC2_READ24_burstMode = 0x61,       // 讀取 ADC2 24bit
        Rcmd_ADC3_READ24_burstMode = 0x62,       // 讀取 ADC3 24bit
        Rcmd_ADC4_READ24_burstMode = 0x63;       // 讀取 ADC4 24bit
        #endregion

        #region Write Command
        // 寫入命令常數定義
        public const byte
           Wcmd_SINGLE_READ = 0x00,    // 單一讀取
           Wcmd_PERIPH_CTRL = 0x01,    // 周邊控制
           Wcmd_SOUT12_CTRL = 0x02,    // SOUT12 控制
           Wcmd_SOUT34_CTRL = 0x03,    // SOUT34 控制
           Wcmd_SOUT56_CTRL = 0x04,    // SOUT56 控制
           Wcmd_INT_CONFIG = 0x05,     // 中斷設定
           Wcmd_INT12_CTRL = 0x06,     // INT12 控制
           Wcmd_INT34_CTRL = 0x07,     // INT34 控制
           Wcmd_INT56_CTRL = 0x08,     // INT56 控制
           Wcmd_PROM_CTRL = 0x09,      // PROM 控制
           Wcmd_PROM_WRITE1 = 0x0A,    // PROM 寫入1
           Wcmd_PROM_WRITE2 = 0x0B,    // PROM 寫入2
           Wcmd_PROM_WRITE3 = 0x0C,    // PROM 寫入3
           Wcmd_PROM_WRITE4 = 0x0D,    // PROM 寫入4
           Wcmd_UART_CTRL = 0x0E,      // UART 控制
           Wcmd_PMD_MOT_CTRL = 0x0F,   // PMD 馬達控制
           Wcmd_BURST_CTRL = 0x10,     // Burst 控制
           Wcmd_ENC_CTRL = 0x11,       // 編碼器控制
           Wcmd_ENC1_WRITE = 0x12,     // 編碼器1 寫入
           Wcmd_ENC2_WRITE = 0x13,     // 編碼器2 寫入
           Wcmd_ENC3_WRITE = 0x14,     // 編碼器3 寫入
           Wcmd_ENC4_WRITE = 0x15,     // 編碼器4 寫入
           Wcmd_ADC12_CTRL = 0x16,     // ADC12 控制
           Wcmd_ADC34_CTRL = 0x17,     // ADC34 控制
           Wcmd_ADC1_WRITE = 0x18,     // ADC1 寫入
           Wcmd_ADC2_WRITE = 0x19,     // ADC2 寫入
           Wcmd_ADC3_WRITE = 0x1A,     // ADC3 寫入
           Wcmd_ADC4_WRITE = 0x1B,     // ADC4 寫入
           Wcmd_DSP_CTRL = 0x1C;       // DSP 控制
        #endregion

        // 每個主板有自己的 FTDI_Ctrl 實例
        public FTDI_Ctrl Ftdi_Ctrl_USB_A { get; set; } // FrameWare 寫入埠
        public FTDI_Ctrl Ftdi_Ctrl_USB_B { get; set; } // Burst mode 埠 (僅讀取)
        public FTDI_Ctrl Ftdi_Ctrl_USB_C { get; set; } // 一般命令埠
        public FTDI_Ctrl Ftdi_Ctrl_USB_D { get; set; }
        public MAX11270_AD_Converter ADConverter { get; private set; }
        public PROM Prom { get; private set; }

        public Encoder encoder { get; private set; }
        // public MemoryChip Memory { get; private set; }
        // public MotionChip Motion { get; private set; }

        /// <summary>
        /// 建構子，傳入四個 FTDI 裝置，初始化各控制器與 AD 轉換器。
        /// </summary>
        public A4MB(FTDI Ftdi_USB_A, FTDI Ftdi_USB_B, FTDI Ftdi_USB_C, FTDI Ftdi_USB_D)
        {
            Ftdi_Ctrl_USB_A = new FTDI_Ctrl(Ftdi_USB_A);
            Ftdi_Ctrl_USB_B = new FTDI_Ctrl(Ftdi_USB_B);
            Ftdi_Ctrl_USB_C = new FTDI_Ctrl(Ftdi_USB_C);
            Ftdi_Ctrl_USB_D = new FTDI_Ctrl(Ftdi_USB_D);
            // 初始化 AD 轉換器
            ADConverter = new MAX11270_AD_Converter(Ftdi_Ctrl_USB_C);
            Prom = new PROM(Ftdi_Ctrl_USB_C);
            //  Memory = new MemoryChip(Ctrl);
            //  Motion = new MotionChip(Ctrl);
        }

        /// <summary>
        /// FTDI 控制器，負責與單一 FTDI 裝置溝通。
        /// </summary>
        public class FTDI_Ctrl
        {
            private FTDI fTDI;
            int FtdiCount = 0;

            /// <summary>
            /// 建構子，傳入 FTDI 實例。
            /// </summary>
            public FTDI_Ctrl(FTDI ftdi)
            {
                fTDI = ftdi ?? throw new ArgumentNullException(nameof(ftdi));
            }

            //ok
            public void FTDI_Preprocessing(String SerialNumber)//FTDI預設連線流程
            {
                Init();
                Connect(SerialNumber);
                ConnectSetting(0, 115200, 8, 0, 2);
            }

            //ok
            /// <summary>
            /// 初始化，取得 FTDI 裝置數量。
            /// </summary>
            public string Init()
            {
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


            /// <summary>
            /// 依序號連接 FTDI 裝置。//ok
            /// </summary>
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
                //fTDI.GetSerialNumber(out String ASDSA);
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

            /// <summary>
            /// 設定 FTDI 連線參數。//ok
            /// </summary>
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

            public void purge()
            {
                fTDI.Purge(FTDI.FT_PURGE.FT_PURGE_RX | FTDI.FT_PURGE.FT_PURGE_TX);
            }


            /// <summary>
            /// 寫入命令與資料到 FTDI 裝置。//Not verified yet
            /// </summary>
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
                    fTDI.Purge(FTDI.FT_PURGE.FT_PURGE_RX | FTDI.FT_PURGE.FT_PURGE_TX);
                    if (fTDI.Write(Send, Send.Length, ref bytesWritten) != FTDI.FT_STATUS.FT_OK)
                    {
                        throw new InvalidOperationException("寫入失敗");
                    }
                }
            }

            /// <summary>
            /// 從 FTDI 裝置讀取 6 bytes，並解析為 32bit 資料。//Not verified yet
            /// </summary>
            public uint Read(out byte address)
            {
                byte[] recv = new byte[6];

                uint bytesRead = 0;
                fTDI.SetTimeouts(100, 100);
                FTDI.FT_STATUS status = fTDI.Read(recv, 6, ref bytesRead);

                if (status != FTD2XX_NET.FTDI.FT_STATUS.FT_OK)
                    //throw new InvalidOperationException("讀取失敗");

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

            /// <summary>
            /// 從 FTDI 裝置讀取 5 bytes，並解析為 24bit 資料。// 新增版本 for MAX11270 24-bit
            /// </summary>
            public uint Read24(out byte address)
            {
                // MAX11270 在 24bit 模式下回傳 4 Bytes 封包:
                // [0] = address / 命令
                // [1] = High Byte (bit23-16)
                // [2] = Mid Byte  (bit15-8)
                // [3] = Low Byte  (bit7-0)
                // 為了保持一致，這裡依舊讀 5~6 Bytes，依實際回應而定
                byte[] recv = new byte[5];
                uint bytesRead = 0;

                fTDI.SetTimeouts(100, 100);
                FTDI.FT_STATUS status = fTDI.Read(recv, (uint)recv.Length, ref bytesRead);
                if (status != FTDI.FT_STATUS.FT_OK)
                    throw new InvalidOperationException("讀取失敗 (Read24)");

                if (bytesRead < 4)
                    throw new InvalidOperationException("資料不足，讀取的 byte 數量太少 (預期 ≥4)");

                address = recv[0];
                uint data = ((uint)(recv[1] & 0x7F) << 16) |
                            ((uint)(recv[2] & 0x7F) << 8) |
                            ((uint)(recv[3] & 0x7F));

                return data;
            }

            public byte[] ReadAvailableBytes()
            {
                uint rxBytes = 0;
                FTDI.FT_STATUS status = fTDI.GetRxBytesAvailable(ref rxBytes);

                if (status != FTDI.FT_STATUS.FT_OK)
                    throw new InvalidOperationException("GetRxBytesAvailable 失敗");

                if (rxBytes == 0)
                    return Array.Empty<byte>();

                byte[] buffer = new byte[rxBytes];
                uint bytesRead = 0;

                status = fTDI.Read(buffer, rxBytes, ref bytesRead);
                if (status != FTDI.FT_STATUS.FT_OK)
                    throw new InvalidOperationException("PortB 讀取失敗");

                if (bytesRead < rxBytes)
                    Array.Resize(ref buffer, (int)bytesRead);

                return buffer;
            }

            public byte[] ForceReadSomeBytes(int count = 64)
            {
                byte[] buffer = new byte[count];
                uint bytesRead = 0;

                fTDI.SetTimeouts(200, 200);
                var status = fTDI.Read(buffer, (uint)count, ref bytesRead);

                if (status != FTDI.FT_STATUS.FT_OK)
                    throw new InvalidOperationException("Read failed: " + status);

                if (bytesRead == 0)
                    return Array.Empty<byte>();

                Array.Resize(ref buffer, (int)bytesRead);
                return buffer;
            }

            public class BurstStreamParser
            {
                private readonly List<byte> _buffer = new List<byte>();

                public void Append(byte[] data)
                {
                    if (data != null && data.Length > 0)
                        _buffer.AddRange(data);
                }

                public List<BurstFrame> ParseFrames()
                {
                    var frames = new List<BurstFrame>();

                    while (_buffer.Count >= 6)
                    {
                        // 找 frame 起點：第一個 byte bit7=0
                        if ((_buffer[0] & 0x80) != 0)
                        {
                            _buffer.RemoveAt(0);
                            continue;
                        }

                        // 後面 5 bytes 必須 bit7=1
                        bool valid = true;
                        for (int i = 1; i < 6; i++)
                        {
                            if ((_buffer[i] & 0x80) == 0)
                            {
                                valid = false;
                                break;
                            }
                        }

                        if (!valid)
                        {
                            _buffer.RemoveAt(0);
                            continue;
                        }

                        byte addr = _buffer[0];

                        uint part1 = (uint)(_buffer[1] & 0x0F);
                        uint part2 = (uint)(_buffer[2] & 0x7F);
                        uint part3 = (uint)(_buffer[3] & 0x7F);
                        uint part4 = (uint)(_buffer[4] & 0x7F);
                        uint part5 = (uint)(_buffer[5] & 0x7F);

                        uint data = (part1 << 28) |
                                    (part2 << 21) |
                                    (part3 << 14) |
                                    (part4 << 7) |
                                    part5;

                        frames.Add(new BurstFrame
                        {
                            Address = addr,
                            Data = data
                        });

                        _buffer.RemoveRange(0, 6);
                    }

                    return frames;
                }
            }

            public class BurstFrame
            {
                public byte Address { get; set; }
                public uint Data { get; set; }

                public override string ToString()
                    => $"Addr=0x{Address:X2}, Data=0x{Data:X8}";
            }

            public void BuzzerOn()
            {
                Write(0x01, 0x20000);
            }
            public void BuzzerOff()
            {
                Write(0x01, 0x00000);
            }

            /// <summary>
            /// 檢查 PROM 是否 busy 或 error。  //Not verified yet
            /// </summary>
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
            /// 讀取指定 command 的 24bit 資料（預設 page 0）。 // 新增版本 for MAX11270 24-bit
            /// </summary>
            public uint? Single_Read24(uint Command)
            {
                byte[] DataArray = new byte[4];
                DataArray[0] = 0;
                DataArray[1] = 0;
                DataArray[2] = (byte)(0 | 0x80);
                DataArray[3] = (byte)(Command & 0xFF);

                ulong packet = ((ulong)DataArray[0] << 24) |
                               ((ulong)DataArray[1] << 16) |
                               ((ulong)DataArray[2] << 8) |
                               ((ulong)DataArray[3]);

                Write(0x00, packet);

                // 讀取三個 bytes 組成 24-bit
                byte[] recv = new byte[4];
                uint bytesRead = 0;
                fTDI.Read(recv, 4, ref bytesRead);
                if (bytesRead < 4) return null;

                uint data = ((uint)(recv[1] & 0x7F) << 16) |
                            ((uint)(recv[2] & 0x7F) << 8) |
                            ((uint)(recv[3] & 0x7F));
                return data;
            }

            /// <summary>
            ///Read the value of the command position. //Not verified yet
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

            /// <summary>
            /// 讀取指定 command 的值（預設 page 0）。 //Not verified yet
            /// </summary>
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
                uint ReadRes = Read(out byte Addr);
                return ReadRes;
            }

            /// <summary>
            /// 僅發送命令，不做讀取，故不回傳任何值（指定 page）。 //Not verified yet
            /// </summary>
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
            /// 僅發送命令，不做讀取，故不回傳任何值（預設 page 0）。 //Not verified yet
            /// </summary>
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

            public uint GetRxBytesAvailable()
            {
                uint rxBytes = 0;
                FTDI.FT_STATUS status = fTDI.GetRxBytesAvailable(ref rxBytes);
                if (status != FTDI.FT_STATUS.FT_OK)
                    throw new InvalidOperationException("GetRxBytesAvailable failed");

                return rxBytes;
            }

            public byte[] ReadRaw(uint bytesToRead)
            {
                if (bytesToRead == 0)
                    return Array.Empty<byte>();

                byte[] buffer = new byte[bytesToRead];
                uint bytesRead = 0;

                FTDI.FT_STATUS status = fTDI.Read(buffer, bytesToRead, ref bytesRead);
                if (status != FTDI.FT_STATUS.FT_OK)
                    throw new InvalidOperationException("Raw read failed");

                if (bytesRead < bytesToRead)
                {
                    Array.Resize(ref buffer, (int)bytesRead);
                }

                return buffer;
            }
        }

        /// <summary>
        /// MAX11270 AD 轉換器控制類別。 //Not verified yet
        /// </summary>
        public class MAX11270_AD_Converter
        {
            private FTDI_Ctrl ctrl;
            private FTDI FtdiBurstRead;
            private FTDI FtdiNormalCtrl;
            static private double Zero_Value = 0;
            //public MAX11270_AD_Converter(FTDI_Ctrl FtdiBurstRead, FTDI_Ctrl FtdiNormalCtrl);
            public MAX11270_AD_Converter(FTDI_Ctrl ctrl)
            {
                this.ctrl = ctrl;

                // 初始化四個通道，index 對應 result[0]~result[3]
                AD1 = new ADChannel(this, 3);
                AD2 = new ADChannel(this, 2);
                AD3 = new ADChannel(this, 1);
                AD4 = new ADChannel(this, 0);
            }

            //OK
            #region AD Status
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

            /// <summary>
            /// AD 通道類別，提供 IsReady/IsBusy 屬性。
            /// </summary>
            public class ADChannel
            {
                private readonly MAX11270_AD_Converter parent;
                private readonly int index;

                internal ADChannel(MAX11270_AD_Converter parent, int index)
                {
                    this.parent = parent;
                    this.index = index;
                }

                /// <summary>
                /// 是否已準備好（最低位元）。
                /// </summary>
                public bool IsReady
                {
                    get
                    {
                        byte[] res = parent.ReadStatus();
                        return (res[index] & 0b0000_0001) != 0;  // 最低位元
                    }
                }

                /// <summary>
                /// 是否忙碌中（倒數第二位元）。
                /// </summary>
                public bool IsBusy
                {
                    get
                    {
                        byte[] res = parent.ReadStatus();
                        return (res[index] & 0b0000_0010) != 0;  // 倒數第二位元
                    }
                }
            }
            #endregion

            public const byte
                REG_ADC12_CTRL = 0x16,
                REG_ADC34_CTRL = 0x17,
                REG_ADC1_WRITE = 0x18,
                REG_ADC2_WRITE = 0x19,
                REG_ADC3_WRITE = 0x1A,
                REG_ADC4_WRITE = 0x1B;
            //Not verified yet
            #region  ADC12_CTRL_0x16
            public class ADC12_CTRL_0x16_Parameter
            {
                /// <summary>
                /// Reserved bits, always "00"
                /// </summary>
                public uint Bit31_30 = 0;
                /// <summary>
                /// ADC2_EN: ADC2 Enable control
                /// 0 = ADC2 disabled
                /// 1 = ADC2 enabled
                /// </summary>
                public uint Bit29 { get; set; }

                /// <summary>
                /// ADC2_AUTORD = Automatic Read control with command 0xCD Read
                /// 0 = Disable automatic read of ADC2 data
                /// 1 = Enable automatic read of ADC2 data
                /// </summary>
                public uint Bit28 { get; set; }

                /// <summary>
                /// ADC2_RW = ADC2 Read/Write control
                /// 0 = Write to ADC2
                /// 1= Read from ADC2
                /// </summary>
                public uint Bit27 { get; set; }

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
                public uint Bit26_24 { get; set; }

                /// <summary>
                /// ADX2_CMD = ADC2 Command
                /// </summary>
                public uint Bit23_16 { get; set; }

                /// <summary>
                /// Reserved bits, always "00"
                /// </summary>
                public uint Bit15_14 = 0;

                /// <summary>
                /// ACD1_EN = ADC1 Enable control
                /// 0 = ADC1 disabled
                /// 1 = ADC1 enabled
                /// </summary>
                public uint Bit13 { get; set; }

                /// <summary>
                /// ADC1_AUTORD = ADC1 Automatic Read control with command 0xCD Read
                /// 0 = Disable automatic read of ADC1 data
                /// 1 = Enable automatic read of ADC1 data
                /// </summary>
                public uint Bit12 { get; set; }

                /// <summary>
                /// ADC1_RW = ADC1 Read/Write control
                /// 0 = Write to ADC1
                /// 1= Read from ADC1
                /// </summary>
                public uint Bit11 { get; set; }

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
                public uint Bit10_8 { get; set; }

                /// <summary>
                /// ADC1_CMD = ADC1 Command
                /// </summary>
                public uint Bit7_0 { get; set; }

            }

            //Not verified yet
            internal uint ADC12_CTRL_0x16_BuildPacket(ADC12_CTRL_0x16_Parameter p)
            {
                //uint value = 0;
                //value |= (uint)(p.Bit31_30 & 0b11) << 30;
                //value |= (uint)(p.Bit29 & 0b1) << 29;
                //value |= (uint)(p.Bit28 & 0b1) << 28;
                //value |= (uint)(p.Bit27 & 0b1) << 27;
                //value |= (uint)(p.Bit26_24 & 0b111) << 24;
                //value |= (uint)(p.Bit23_16 & 0xFF) << 16;
                //value |= (uint)(p.Bit15_14 & 0b11) << 14;
                //value |= (uint)(p.Bit13 & 0b1) << 13;
                //value |= (uint)(p.Bit12 & 0b1) << 12;
                //value |= (uint)(p.Bit11 & 0b1) << 11;
                //value |= (uint)(p.Bit10_8 & 0b111) << 8;
                //value |= (uint)(p.Bit7_0 & 0xFF);
                //return value;
                uint value = 0;

                // -------- ADC2 (高 16bit) --------
                value |= (p.Bit31_30 & 0b11u) << 30;    // Reserved
                value |= (p.Bit29 & 0b1u) << 29;        // ADC2 Enable
                value |= (p.Bit28 & 0b1u) << 28;        // ADC2 AutoRead
                value |= (p.Bit27 & 0b1u) << 27;        // ADC2 R/W
                value |= (p.Bit26_24 & 0b111u) << 24;   // ADC2 Command Length
                value |= (p.Bit23_16 & 0xFFu) << 16;    // ADC2 Command (8bit)

                // -------- ADC1 (低 16bit) --------
                value |= (p.Bit15_14 & 0b11u) << 14;    // Reserved
                value |= (p.Bit13 & 0b1u) << 13;        // ADC1 Enable
                value |= (p.Bit12 & 0b1u) << 12;        // ADC1 AutoRead
                value |= (p.Bit11 & 0b1u) << 11;        // ADC1 R/W
                value |= (p.Bit10_8 & 0b111u) << 8;     // ADC1 Command Length
                value |= (p.Bit7_0 & 0xFFu);            // ADC1 Command (8bit)

                return value;
            }

            /// <summary>
            /// 設定 ADC12 控制暫存器。//Not verified yet
            /// </summary>
            public void ADC12_CTRL_0x16(ADC12_CTRL_0x16_Parameter pack)
            {
                uint packet = ADC12_CTRL_0x16_BuildPacket(pack);
                ctrl.Write(Wcmd_ADC12_CTRL, packet);
            }

            /// <summary>
            /// 以 uint 陣列設定 ADC12 控制暫存器。//Not verified yet
            /// </summary>
            public void ADC12_CTRL_0x16(IEnumerable<uint> bits)
            {
                if (bits == null)
                    throw new ArgumentNullException(nameof(bits));

                var arr = bits.ToArray();
                if (arr.Length < 12)
                    throw new ArgumentException("bits 集合必須至少有 12 個元素。");

                var p = new ADC12_CTRL_0x16_Parameter
                {
                    Bit31_30 = arr[0],
                    Bit29 = arr[1],
                    Bit28 = arr[2],
                    Bit27 = arr[3],
                    Bit26_24 = arr[4],
                    Bit23_16 = arr[5],
                    Bit15_14 = arr[6],
                    Bit13 = arr[7],
                    Bit12 = arr[8],
                    Bit11 = arr[9],
                    Bit10_8 = arr[10],
                    Bit7_0 = arr[11]
                };

                uint packet = ADC12_CTRL_0x16_BuildPacket(p);
                ctrl.Write(Wcmd_ADC12_CTRL, packet);
            }

            /// <summary>
            /// 以 hex 字串陣列設定 ADC12 控制暫存器。//Not verified yet
            /// </summary>
            public void ADC12_CTRL_0x16(string[] hexBits)
            {
                if (hexBits == null)
                    throw new ArgumentNullException(nameof(hexBits));
                if (hexBits.Length < 12)
                    throw new ArgumentException("hexBits 陣列必須至少有 12 個元素。");

                var arr = new uint[12];
                for (int i = 0; i < 12; i++)
                {
                    if (!uint.TryParse(hexBits[i], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out arr[i]))
                        throw new FormatException($"hexBits[{i}] = \"{hexBits[i]}\" 不是有效的十六進位數字。");
                }

                ADC12_CTRL_0x16(arr);
            }

            #region Enums for ADC12_CTRL_0x16
            public enum ADC_Enable : uint
            {
                Disable = 0,
                Enable = 1
            }

            public enum ReadMode : uint
            {
                Manual = 0,   // 手動讀取模式
                Auto = 1      // 自動讀取 (AutoRead)
            }

            public enum RW_mode : uint
            {
                Write = 0,    // 寫入模式
                Read = 1      // 讀取模式
            }

            public enum CmdLength : uint
            {
                Length_0byte = 0b000,
                Length_1byte = 0b001,
                Length_2byte = 0b010,
                Length_3byte = 0b011,
                Length_4byte = 0b100
            }

            public enum AdcCmd : byte
            {//本區後面如果沒寫註解代表這指令不用帶資料
                //
                //那為何說明書上寫CTRL1 = 0x1 但這在這裡的CTRL1_WIRTE卻寫0xC2?
                //因為說明書第30頁有提到在Register Access Mode(MODE = 1)下的命令
                //那B7==1與B6==1 並且B0=0是寫入 , 然後在B1~B5放CTRL1的地址0x1
                //所以就變成0xC2了(1100 0010)

                CONVERSION_NO_CAL_RATE0 = 0x80,
                CONVERSION_NO_CAL_RATE1 = 0x81,
                CONVERSION_NO_CAL_RATE2 = 0x82,
                CONVERSION_NO_CAL_RATE3 = 0x83,
                CONVERSION_NO_CAL_RATE4 = 0x84,
                CONVERSION_NO_CAL_RATE5 = 0x85,
                CONVERSION_NO_CAL_RATE6 = 0x86,
                CONVERSION_NO_CAL_RATE7 = 0x87,
                CONVERSION_NO_CAL_RATE8 = 0x88,
                CONVERSION_NO_CAL_RATE9 = 0x89,
                CONVERSION_NO_CAL_RATE10 = 0x8A,
                CONVERSION_NO_CAL_RATE11 = 0x8B,
                CONVERSION_NO_CAL_RATE12 = 0x8C,
                CONVERSION_NO_CAL_RATE13 = 0x8D,
                CONVERSION_NO_CAL_RATE14 = 0x8E,
                CONVERSION_NO_CAL_RATE15 = 0x8F,


                CONVERSION_CAL_RATE0 = 0xA0,
                CONVERSION_CAL_RATE1 = 0xA1,
                CONVERSION_CAL_RATE2 = 0xA2,
                CONVERSION_CAL_RATE3 = 0xA3,
                CONVERSION_CAL_RATE4 = 0xA4,
                CONVERSION_CAL_RATE5 = 0xA5,
                CONVERSION_CAL_RATE6 = 0xA6,
                CONVERSION_CAL_RATE7 = 0xA7,
                CONVERSION_CAL_RATE8 = 0xA8,
                CONVERSION_CAL_RATE9 = 0xA9,
                CONVERSION_CAL_RATE10 = 0xAA,
                CONVERSION_CAL_RATE11 = 0xAB,
                CONVERSION_CAL_RATE12 = 0xAC,
                CONVERSION_CAL_RATE13 = 0xAD,
                CONVERSION_CAL_RATE14 = 0xAE,
                CONVERSION_CAL_RATE15 = 0xAF,

                // Register Access Mode(MODE = 1)
                CTRL1_WRITE = 0xC2, //1 byte command
                CTRL2_WRITE = 0xC4, //1 byte command
                CTRL3_WRITE = 0xC6, //1 byte command
                CTRL4_WRITE = 0xC8, //1 byte command
                CTRL5_WRITE = 0xCA, //1 byte command
                SOC_SPI_WRITE = 0xCE,   //3 byte command
                SGC_SPI_WRITE = 0xD0,   //3 byte command
                SCOC_SPI_WRITE = 0xD2,      //3 byte command
                SCGC_SPI_WRITE = 0xD4,  //3 byte command
                RAM_WRITE = 0xD8,
                SYNC_SPI_WRITE = 0xDA,

                // A/D Converter Register Table (read)
                // Register Access Mode(MODE = 1)
                STAT_READ = 0xC1,   //2 byte command

                CTRL1_READ = 0xC3,  //1 byte command	
                CTRL2_READ = 0xC5,//1 byte command
                CTRL3_READ = 0xC7,//1 byte command
                CTRL4_READ = 0xC9,//1 byte command
                CTRL5_READ = 0xCB,//1 byte command

                DATA_READ_continuous = 0xCD,//3 byte command
                DATA_READ_Once = 0,//3 byte command
                SOC_SPI_READ = 0xCF,//3 byte command
                SGC_SPI_READ = 0xD1,//3 byte command
                SCOC_SPI_READ = 0xD3,   //3 byte command
                SCGC_SPI_READ = 0xD5,   //3 byte command
                RAM_READ = 0xD9,            //3 byte command
                SOC_ADC_READ = 0xEB,     //3 byte command
                SGC_ADC_READ = 0xED,    //3 byte command
                SCOC_ADC_READ = 0xEF,   //3 byte command
                SCGC_ADC_READ = 0xF1,		//3 byte command
            }

            public enum StatusBit
            {
                RDY = 0,
                MSTAT = 1,
                DOR = 2,
                SYSGOR = 3,
                RATE0 = 4,
                RATE1 = 5,
                RATE2 = 6,
                RATE3 = 7,
                AOR = 8,
                RDERR = 9,
                PDSTAT0 = 10,
                PDSTAT1 = 11,
                ERROR = 14,
                INRESET = 15
            }

            public enum Ctrl1Bit
            {
                CONTSC = 0,
                SCYCLE = 1,
                FORMAT = 2,
                UB = 3,
                PD0 = 4,
                PD1 = 5,
                SYNC = 6,
                EXTCK = 7
            }

            public enum Ctrl2Bit
            {
                PGA0 = 0,
                PGA1 = 1,
                PGA2 = 2,
                PGAEN = 3,
                LPMODE = 4,
                BUFEN = 5,
                DGAIN0 = 6,
                DGAIN1 = 7
            }

            public enum Ctrl3Bit
            {
                DATA32 = 3,
                MODBITS = 4,
                ENMSYNC = 5
            }

            public enum Ctrl4Bit
            {
                DIO1 = 0,
                DIO2 = 1,
                DIO3 = 2,
                DIR1 = 4,
                DIR2 = 5,
                DIR3 = 6
            }

            public enum Ctrl5Bit
            {
                NOSCO = 0,
                NOSCG = 1,
                NOSYSO = 2,
                NOSYSG = 3,
                CAL0 = 6,
                CAL1 = 7
            }


            private static CmdLength GetCmdLength(AdcCmd cmd)
            {
                switch (cmd)
                {
                    // 0-byte
                    case AdcCmd.CONVERSION_NO_CAL_RATE0:
                    case AdcCmd.CONVERSION_NO_CAL_RATE1:
                    case AdcCmd.CONVERSION_NO_CAL_RATE2:
                    case AdcCmd.CONVERSION_NO_CAL_RATE3:
                    case AdcCmd.CONVERSION_NO_CAL_RATE4:
                    case AdcCmd.CONVERSION_NO_CAL_RATE5:
                    case AdcCmd.CONVERSION_NO_CAL_RATE6:
                    case AdcCmd.CONVERSION_NO_CAL_RATE7:
                    case AdcCmd.CONVERSION_NO_CAL_RATE8:
                    case AdcCmd.CONVERSION_NO_CAL_RATE9:
                    case AdcCmd.CONVERSION_NO_CAL_RATE10:
                    case AdcCmd.CONVERSION_NO_CAL_RATE11:
                    case AdcCmd.CONVERSION_NO_CAL_RATE12:
                    case AdcCmd.CONVERSION_NO_CAL_RATE13:
                    case AdcCmd.CONVERSION_NO_CAL_RATE14:
                    case AdcCmd.CONVERSION_NO_CAL_RATE15:
                    case AdcCmd.CONVERSION_CAL_RATE0:
                    case AdcCmd.CONVERSION_CAL_RATE1:
                    case AdcCmd.CONVERSION_CAL_RATE2:
                    case AdcCmd.CONVERSION_CAL_RATE3:
                    case AdcCmd.CONVERSION_CAL_RATE4:
                    case AdcCmd.CONVERSION_CAL_RATE5:
                    case AdcCmd.CONVERSION_CAL_RATE6:
                    case AdcCmd.CONVERSION_CAL_RATE7:
                    case AdcCmd.CONVERSION_CAL_RATE8:
                    case AdcCmd.CONVERSION_CAL_RATE9:
                    case AdcCmd.CONVERSION_CAL_RATE10:
                    case AdcCmd.CONVERSION_CAL_RATE11:
                    case AdcCmd.CONVERSION_CAL_RATE12:
                    case AdcCmd.CONVERSION_CAL_RATE13:
                    case AdcCmd.CONVERSION_CAL_RATE14:
                    case AdcCmd.CONVERSION_CAL_RATE15:
                        return CmdLength.Length_0byte;

                    // 1-byte
                    case AdcCmd.CTRL1_READ:
                    case AdcCmd.CTRL2_READ:
                    case AdcCmd.CTRL3_READ:
                    case AdcCmd.CTRL4_READ:
                    case AdcCmd.CTRL5_READ:
                    case AdcCmd.CTRL1_WRITE:
                    case AdcCmd.CTRL2_WRITE:
                    case AdcCmd.CTRL3_WRITE:
                    case AdcCmd.CTRL4_WRITE:
                    case AdcCmd.CTRL5_WRITE:
                        return CmdLength.Length_1byte;

                    // 2-byte
                    case AdcCmd.STAT_READ:
                        return CmdLength.Length_2byte;

                    // 3-byte
                    case AdcCmd.DATA_READ_continuous:
                    case AdcCmd.SOC_SPI_READ:
                    case AdcCmd.SGC_SPI_READ:
                    case AdcCmd.SCOC_SPI_READ:
                    case AdcCmd.SCGC_SPI_READ:
                    case AdcCmd.SOC_SPI_WRITE:
                    case AdcCmd.SGC_SPI_WRITE:
                    case AdcCmd.SCOC_SPI_WRITE:
                    case AdcCmd.SCGC_SPI_WRITE:
                    case AdcCmd.SOC_ADC_READ:
                    case AdcCmd.SGC_ADC_READ:
                    case AdcCmd.SCOC_ADC_READ:
                    case AdcCmd.SCGC_ADC_READ:
                        return CmdLength.Length_3byte;

                    // 其餘（RAM/SYNC），請依你手邊說明書確認長度
                    case AdcCmd.RAM_READ:
                    case AdcCmd.RAM_WRITE:
                    case AdcCmd.SYNC_SPI_WRITE:
                        return CmdLength.Length_3byte;

                    default:
                        // 預設保守
                        return CmdLength.Length_0byte;


                }
            }
            #endregion

            public void ADC12_CTRL_0x16(
      ADC_Enable Adc2Enable, ReadMode Adc2ReadMode, RW_mode Adc2RW, AdcCmd Adc2Cmd,
      ADC_Enable Adc1Enable, ReadMode Adc1ReadMode, RW_mode Adc1RW, AdcCmd Adc1Cmd)
            {
                SpinWait.SpinUntil(() => !AD1.IsBusy && !AD2.IsBusy && AD1.IsReady && AD2.IsReady, 3000); // 等待 ADC 不忙碌，最多等 3 秒
                var pack = new ADC12_CTRL_0x16_Parameter
                {
                    Bit29 = (uint)Adc2Enable,
                    Bit28 = (uint)Adc2ReadMode,
                    Bit27 = (uint)Adc2RW,
                    Bit26_24 = (uint)GetCmdLength(Adc2Cmd),
                    Bit23_16 = (uint)Adc2Cmd,

                    Bit13 = (uint)Adc1Enable,
                    Bit12 = (uint)Adc1ReadMode,
                    Bit11 = (uint)Adc1RW,
                    Bit10_8 = (uint)GetCmdLength(Adc1Cmd),
                    Bit7_0 = (uint)Adc1Cmd
                };

                uint packet = ADC12_CTRL_0x16_BuildPacket(pack);
                ctrl.Write(Wcmd_ADC12_CTRL, packet);
            }

            public void ADC12_CTRL_0x16(ADC_Enable Adc_1and2_Enable, ReadMode Adc_1and2_ReadMode, RW_mode Adc_1and2_RW, AdcCmd Adc_1and2_Cmd)
            {
                SpinWait.SpinUntil(() => !AD1.IsBusy && !AD2.IsBusy && AD1.IsReady && AD2.IsReady, 100); // 等待 ADC 不忙碌，最多等 3 秒
                var pack = new ADC12_CTRL_0x16_Parameter
                {
                    Bit29 = (uint)Adc_1and2_Enable,
                    Bit28 = (uint)Adc_1and2_ReadMode,
                    Bit27 = (uint)Adc_1and2_RW,
                    Bit26_24 = (uint)GetCmdLength(Adc_1and2_Cmd),
                    Bit23_16 = (uint)Adc_1and2_Cmd,

                    Bit13 = (uint)Adc_1and2_Enable,
                    Bit12 = (uint)Adc_1and2_ReadMode,
                    Bit11 = (uint)Adc_1and2_RW,
                    Bit10_8 = (uint)GetCmdLength(Adc_1and2_Cmd),
                    Bit7_0 = (uint)Adc_1and2_Cmd
                };

                uint packet = ADC12_CTRL_0x16_BuildPacket(pack);
                ctrl.Write(Wcmd_ADC12_CTRL, packet);
            }

            public void ADC12_CTRL_0x16(
     ADC_Enable Adc2Enable, ReadMode Adc2ReadMode, RW_mode Adc2RW, CmdLength Adc2CmdLength, uint Adc2Cmd,
     ADC_Enable Adc1Enable, ReadMode Adc1ReadMode, RW_mode Adc1RW, CmdLength Adc1CmdLength, uint Adc1Cmd)
            {
                var pack = new ADC12_CTRL_0x16_Parameter
                {
                    Bit29 = (uint)Adc2Enable,
                    Bit28 = (uint)Adc2ReadMode,
                    Bit27 = (uint)Adc2RW,
                    Bit26_24 = (uint)Adc2CmdLength,
                    Bit23_16 = (uint)Adc2Cmd,

                    Bit13 = (uint)Adc1Enable,
                    Bit12 = (uint)Adc1ReadMode,
                    Bit11 = (uint)Adc1RW,
                    Bit10_8 = (uint)Adc1CmdLength,
                    Bit7_0 = (uint)Adc1Cmd
                };

                uint packet = ADC12_CTRL_0x16_BuildPacket(pack);
                ctrl.Write(Wcmd_ADC12_CTRL, packet);
            }

            public void ADC12_CTRL_0x16(ADC_Enable AdcEnable, ReadMode AdcReadMode, RW_mode AdcRW, CmdLength AdcCmdLength, uint AdcCmd)
            {
                var pack = new ADC12_CTRL_0x16_Parameter
                {
                    Bit29 = (uint)AdcEnable,
                    Bit28 = (uint)AdcReadMode,
                    Bit27 = (uint)AdcRW,
                    Bit26_24 = (uint)AdcCmdLength,
                    Bit23_16 = (uint)AdcCmd,

                    Bit13 = (uint)AdcEnable,
                    Bit12 = (uint)AdcReadMode,
                    Bit11 = (uint)AdcRW,
                    Bit10_8 = (uint)AdcCmdLength,
                    Bit7_0 = (uint)AdcCmd
                };

                uint packet = ADC12_CTRL_0x16_BuildPacket(pack);
                ctrl.Write(Wcmd_ADC12_CTRL, packet);
            }

            #endregion

            /// <summary>
            /// 寫入 ADC1 資料。//Not verified yet
            /// </summary>
            public bool ADC1_WRITE_0x18(uint data)
            {
                try
                {
                    ctrl.Write(Wcmd_ADC1_WRITE, data);
                    return true;
                }
                catch (Exception ex)
                {
                    throw new Exception("ADC2_WRITE_0x19 Error: " + ex.Message);
                }

            }

            /// <summary>
            /// 寫入 ADC2 資料。//Not verified yet
            /// </summary>
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

            //Not verified yet
            /// <summary>
            /// 讀取 ADC1 32 位元資料流程。
            /// 1. 等待 ADC1 準備好，然後設定 ADC12 控制暫存器，啟用 ADC1/ADC2 並設為讀取模式。
            /// 2. 再次等待 ADC1 準備好，然後讀取 ADC1 32 位元資料。
            /// 若任一步驟超時則丟出 TimeoutException。
            /// </summary>
            /// <returns>成功時回傳 ADC1 32 位元資料，失敗時回傳 null 或丟出例外。</returns>
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
                        Bit12 = 0, // disable automatic read of ADC1
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

            public void ADC_1and2_READ32(out uint? Adc1_val, out uint? Adc2_val)
            {
                int iFlowStep = 0;

                // Step 1: Configure ADC1 to read data
                if (SpinWait.SpinUntil(() => AD1.IsReady, 3000) && iFlowStep == 0)// 等待 ADC 準備好數據// 最多等 3 秒//步驟0
                {
                    ADC12_CTRL_0x16(new ADC12_CTRL_0x16_Parameter
                    {
                        Bit29 = 1, // Enable ADC2
                        Bit28 = 1,
                        Bit27 = 0, // Read from ADC2
                        Bit26_24 = 0b100, // 4 bytes for ADC2//go look at datasheet
                        Bit23_16 = 0x10, // Example command for ADC2//go look at datasheet

                        Bit13 = 1, // Enable ADC1
                        Bit12 = 1,
                        Bit11 = 0, // Read from ADC1
                        Bit10_8 = 0b100, // 4 bytes for ADC1//go look at datasheet
                        Bit7_0 = 0x10 // Example command for ADC1//go look at MAX11270 datasheet
                    });

                    ADC12_CTRL_0x16(new ADC12_CTRL_0x16_Parameter
                    {
                        Bit29 = 1, // Enable ADC2
                        Bit28 = 1,
                        Bit27 = 0, // Read from ADC2
                        Bit26_24 = 0b100, // 4 bytes for ADC2//go look at datasheet
                        Bit23_16 = 0x20, // Example command for ADC2//go look at datasheet

                        Bit13 = 1, // Enable ADC1
                        Bit12 = 1,
                        Bit11 = 0, // Read from ADC1
                        Bit10_8 = 0b100, // 4 bytes for ADC1//go look at datasheet
                        Bit7_0 = 0x20 // Example command for ADC1//go look at MAX11270 datasheet
                    });

                    iFlowStep = 1;
                }
                else
                {
                    throw new TimeoutException("等待 ADC1 準備數據超時");
                }

                if (SpinWait.SpinUntil(() => AD1.IsReady, 10000) && iFlowStep == 1)// 等待 ADC 準備好數據// 最多等 3 秒//步驟1
                {
                    Adc1_val = ctrl.Single_Read(Rcmd_ADC1_READ32);
                    Adc2_val = ctrl.Single_Read(Rcmd_ADC2_READ32);
                }
                else
                {
                    throw new TimeoutException("等待 ADC1 準備數據超時");
                }
            }

            public double ReadOnce_AD1_32_bit(double vRef, double mvPerV, double fullScaleKg)
            {
                ADC12_CTRL_0x16(ADC_Enable.Enable, ReadMode.Manual, RW_mode.Read, AdcCmd.DATA_READ_Once);//啟動連續讀
                Thread.Sleep(100); // 等待 ADC 進行轉換
                uint? raw = ctrl.Single_Read(Rcmd_ADC1_READ32);
                if (raw == null) return double.NaN;
                int val = (int)raw;
                if ((val & 0x800000) != 0) val |= unchecked((int)0xFF000000);
                double voltage = (val / (double)(1 << 23)) * vRef;
                double loadKg = voltage / (mvPerV * vRef) * fullScaleKg;
                return loadKg;
            }

            public double ReadContinuous_AD1_32bit()
            {
                uint? raw = ctrl.Single_Read(Rcmd_ADC1_READ32);
                if (raw == null) return double.NaN;

                int val = (int)raw;
                //if ((val & 0x800000) != 0) val |= unchecked((int)0xFF000000);

                return val;
            }

            public double ReadContinuous_AD2_32bit()
            {
                uint? raw = ctrl.Single_Read(Rcmd_ADC2_READ32);
                if (raw == null) return double.NaN;

                int val = (int)raw;
                //if ((val & 0x800000) != 0) val |= unchecked((int)0xFF000000);

                return val;
            }

            public double ReadContinuous_AD1_24bit()
            {
                uint? raw = ctrl.Single_Read24(Rcmd_ADC1_READ24);
                if (raw == null) return double.NaN;

                int val = (int)raw;
                //if ((val & 0x800000) != 0) val |= unchecked((int)0xFF000000);

                return val;
            }

            public double ReadContinuous_AD2_24bit()
            {
                uint? raw = ctrl.Single_Read24(Rcmd_ADC2_READ24);
                if (raw == null) return double.NaN;

                int val = (int)raw;
                //if ((val & 0x800000) != 0) val |= unchecked((int)0xFF000000);

                return val;
            }

            /// <summary>
            /// MAX11270 CTRL1 register structure
            /// Bits: [7:0] = EXTCK, SYNC, PD1, PD0, UB, FORMAT, SCYCLE, CONTSC
            /// </summary>
            public class CTRL1_Settings
            {
                /// <summary>Bit7: External clock select (0=Internal, 1=External)
                /// <para>EXTCK：外部時鐘選擇位。EXTCK = 1時，ADC使用外部時鐘信號作為轉換時鐘；EXTCK = 0時，ADC使用內部振盪器產生的時鐘信號。</para>
                /// </summary>
                public bool EXTCK { get; set; } = false;

                /// <summary>Bit6: Sync mode (0=Pulse, 1=Continuous)
                /// <para>SYNC：同步模式位。SYNC = 1時，ADC處於連續同步模式，ADC在每個轉換週期結束後自動重新同步；SYNC = 0時，ADC處於脈衝同步模式，ADC在接收到外部同步脈衝信號後才重新同步。</para>
                /// </summary>
                public bool SYNC { get; set; } = true;

                /// <summary>
                /// <para>PD =[PD1,PD0],電源管理位, 別忘記PD0也要設定才是完整的PD。</para>
                /// <para>PD = 00時，ADC處於正常工作模式；</para>
                /// <para>PD = 01時，休眠模式—关断子稳压器和整个数字电路。上电恢复时，PD[1:0]位复位为默认状态“00”</para>
                /// <para>PD = 10時，待機模式—关断模拟电路，子稳压器保持上电。</para>
                /// <para>PD = 11時，将所有寄存器恢复为POR状态，子稳压器保持上电。PD[1:0]位复位为“00”。该位的功能与RSTB引脚完全相同。</para>
                /// </summary>
                public bool PD1 { get; set; } = false;

                /// <summary>
                /// <para>bit4 PD =[PD1,PD0],電源管理位, 別忘記PD1也要設定才是完整的PD。</para>
                /// <para>PD = 00時，ADC處於正常工作模式；</para>
                /// <para>PD = 01時，休眠模式—关断子稳压器和整个数字电路。上电恢复时，PD[1:0]位复位为默认状态“00”</para>
                /// <para>PD = 10時，待機模式—关断模拟电路，子稳压器保持上电。</para>
                /// <para>PD = 11時，将所有寄存器恢复为POR状态，子稳压器保持上电。PD[1:0]位复位为“00”。该位的功能与RSTB引脚完全相同。</para>
                /// </summary>
                public bool PD0 { get; set; } = false;

                /// <summary>Bit3: Unipolar/Bipolar (False=Bipolar, true=Unipolar)
                ///  <para>U/B：单极性/双极性位。U/B = 1时选择单极性输入范围(0至VREF)，U/B = 0时选择双极性输入范 围(±VREF)。</para>
                /// </summary>
                public bool UB { get; set; } = false;

                /// <summary>Bit2: Output data format (0=Two’s complement, 1=Offset binary) 
                /// <para>双极性范围格式位。读取双极性数据时，FORMAT = 0选择二进制补码，FORMAT = 1选择偏移二进制。单极性范围的数据始终采用偏移二进制格式</para>
                /// </summary>
                public bool FORMAT { get; set; } = true;

                /// <summary>Bit1: Conversion mode - SCYCLE (0=Continuous, 1=Single-cycle)
                /// <para>单周期控制位。SCYCLE = 1时选择单周期模式。MAX11270完成一次无延迟转换，然后关断进入仅漏泄状态。SCYCLE = 0时，选择连续转换模式。</para>
                /// </summary>
                public bool SCYCLE { get; set; } = false;

                /// <summary>Bit0: Continuous conversion start (0=Stop, 1=Start single conversion)
                /// <para>连续单周期位。CONTSC = 1时选择连续转换模式；CONTSC = 0时选择单次转换。</para>
                /// </summary>
                public bool CONTSC { get; set; } = false;

                /// <summary>
                /// Build the 8-bit register value for CTRL1.
                /// </summary>
                public byte ToByte()
                {
                    byte value = 0;
                    if (EXTCK) value |= 0x80;                // Bit7
                    if (SYNC) value |= 0x40;                 // Bit6
                    if (PD1) value |= 0x20;                  // Bit5
                    if (PD0) value |= 0x10;                  // Bit4
                    if (UB) value |= 0x08;                   // Bit3
                    if (FORMAT) value |= 0x04;               // Bit2
                    if (SCYCLE) value |= 0x02;               // Bit1
                    if (CONTSC) value |= 0x01;               // Bit0
                    return value;
                }

                /// <summary>
                /// Parse a byte back into individual bit settings (for readback verification).
                /// </summary>
                public static CTRL1_Settings FromByte(byte value)
                {
                    return new CTRL1_Settings
                    {
                        EXTCK = (value & 0x80) != 0,
                        SYNC = (value & 0x40) != 0,
                        PD1 = (value & 0x20) != 0,              // Bit5
                        PD0 = (value & 0x10) != 0,                  // Bit4
                        UB = (value & 0x08) != 0,
                        FORMAT = (value & 0x04) != 0,
                        SCYCLE = (value & 0x02) != 0,
                        CONTSC = (value & 0x01) != 0
                    };
                }

                public override string ToString()
                {
                    return $"CTRL1 = 0x{ToByte():X2}  (EXTCK={EXTCK}, SYNC={SYNC}, PD1={PD1},PD0 = {PD0}, UB={UB}, FORMAT={FORMAT}, SCYCLE={SCYCLE}, CONTSC={CONTSC})";
                }
            }
            public class CTRL2_Settings
            {
                /// <summary>Bit7: DGAIN1 (Digital gain control bit 1)
                /// <para>數位增益控制位。DGAIN[1:0]設定數位放大倍率：</para>
                /// <para>[DGAIN1 , DGAIN0]00 = x1, 01 = x2, 10 = x4, 11 = x8。</para>
                /// </summary>
                public bool DGAIN1 { get; set; } = false;

                /// <summary>Bit6: DGAIN0 (Digital gain control bit 0)
                /// <para>數位增益控制位。DGAIN[1:0]設定數位放大倍率：</para>
                /// <para>[DGAIN1 , DGAIN0]00 = x1, 01 = x2, 10 = x4, 11 = x8。</para>
                /// </summary>
                public bool DGAIN0 { get; set; } = false;

                /// <summary>Bit5: BUFEN (Input buffer enable)
                /// <para>輸入緩衝啟用位。BUFEN = 1 時啟用輸入緩衝器，輸入阻抗增高。</para>
                /// </summary>
                public bool BUFEN { get; set; } = false;

                /// <summary>Bit4: LPMODE (Low-power mode)
                /// <para>低功耗模式選擇位。LPMODE = 1 時進入低功耗模式。</para>
                /// </summary>
                public bool LPMODE { get; set; } = false;

                /// <summary>Bit3: PGAEN (Programmable Gain Amplifier enable)
                /// <para>PGA啟用位。PGAEN = 1 時啟用可程式增益放大器。</para>
                /// </summary>
                public bool PGAEN { get; set; } = false;

                /// <summary>Bit2: PGA2 (Gain select bit 2)
                /// <para>PGA增益設定。PGA[2:0]設定放大倍數：</para>
                /// <para>000=x1, 001=x2, 010=x4, 011=x8, 100=x16, 101=x32, 110=x64, 111=x128。</para>
                /// </summary>
                public bool PGA2 { get; set; } = true;

                /// <summary>Bit1: PGA1 (Gain select bit 1)
                /// <para>與PGA2、PGA0組合設定放大倍數。</para>
                /// </summary>
                public bool PGA1 { get; set; } = true;

                /// <summary>Bit0: PGA0 (Gain select bit 0)
                /// <para>與PGA2、PGA1組合設定放大倍數。</para>
                /// </summary>
                public bool PGA0 { get; set; } = true;

                public byte ToByte()
                {
                    byte value = 0;
                    if (DGAIN1) value |= 0x80;
                    if (DGAIN0) value |= 0x40;
                    if (BUFEN) value |= 0x20;
                    if (LPMODE) value |= 0x10;
                    if (PGAEN) value |= 0x08;
                    if (PGA2) value |= 0x04;
                    if (PGA1) value |= 0x02;
                    if (PGA0) value |= 0x01;
                    return value;
                }

                public static CTRL2_Settings FromByte(byte value)
                {
                    return new CTRL2_Settings
                    {
                        DGAIN1 = (value & 0x80) != 0,
                        DGAIN0 = (value & 0x40) != 0,
                        BUFEN = (value & 0x20) != 0,
                        LPMODE = (value & 0x10) != 0,
                        PGAEN = (value & 0x08) != 0,
                        PGA2 = (value & 0x04) != 0,
                        PGA1 = (value & 0x02) != 0,
                        PGA0 = (value & 0x01) != 0
                    };
                }

                public override string ToString()
                {
                    return $"CTRL2 = 0x{ToByte():X2}  (DGAIN1={DGAIN1}, DGAIN0={DGAIN0}, BUFEN={BUFEN}, LPMODE={LPMODE}, PGAEN={PGAEN}, PGA2={PGA2}, PGA1={PGA1}, PGA0={PGA0})";
                }
            }
            public class CTRL3_Settings
            {
                private const byte RESERVED_TOP = 0b01000000; // Bit7=0, Bit6=1
                private const byte RESERVED_BOTTOM = 0b00000001;// Bit2=0, Bit1=0, ,Bit0=1
                /// <summary>Bit5: ENMSYNC (Master/Slave Sync Enable)
                /// <para>主從同步啟用位。ENMSYNC = 1 時啟用多通道同步操作。</para>
                /// </summary>
                public bool ENMSYNC { get; set; } = false;

                /// <summary>Bit4: MODBITS (Modulator Test Bit), 調變器測試位，正常操作時應保持為 0
                /// <para> 调节器输出模式使能位。MODBITS = 1时使能调节器通过DOUT和GPIO1输出，MODBITS = 0时通过DOUT进行标准数据输出</para>
                /// </summary>
                public bool MODBITS { get; set; } = false;

                /// <summary>Bit3: DATA32 (Data output width)
                /// <para>資料輸出位寬設定。DATA32 = 1 時，ADC輸出32位資料格式；DATA32 = 0 時，輸出24位資料。</para>
                /// </summary>
                public bool DATA32 { get; set; } = false;

                public byte ToByte()
                {
                    byte value = 0;
                    value |= RESERVED_TOP; // Bit7–6 固定為 1
                    if (ENMSYNC) value |= 0x20;
                    if (MODBITS) value |= 0x10;
                    if (DATA32) value |= 0x08;
                    value |= RESERVED_BOTTOM; // Bit2–0 固定為 1
                    return value;
                }

                public static CTRL3_Settings FromByte(byte value)
                {
                    return new CTRL3_Settings
                    {
                        ENMSYNC = (value & 0x20) != 0,
                        MODBITS = (value & 0x10) != 0,
                        DATA32 = (value & 0x08) != 0
                    };
                }

                public override string ToString()
                {
                    return $"CTRL3 = 0x{ToByte():X2}  (ENMSYNC={ENMSYNC}, MODBITS={MODBITS}, DATA32={DATA32})";
                }
            }
            public class CTRL4_Settings
            {
                private const byte RESERVED_FIXED_BITS = 0b00001000; // Bit3 = 1, Bit0 = 0
                /// <summary>Bit6: DIR3 (DIO3 Direction)</summary>
                public bool DIR3 { get; set; } = false;

                /// <summary>Bit5: DIR2 (DIO2 Direction)</summary>
                public bool DIR2 { get; set; } = false;

                /// <summary>Bit4: DIR1 (DIO1 Direction)</summary>
                public bool DIR1 { get; set; } = false;

                /// <summary>Bit2: DIO3 (Digital I/O 3 Value)</summary>
                public bool DIO3 { get; set; } = false;

                /// <summary>Bit1: DIO2 (Digital I/O 2 Value)</summary>
                public bool DIO2 { get; set; } = false;

                /// <summary>Bit0: DIO1 (Digital I/O 1 Value)</summary>
                public bool DIO1 { get; set; } = false;

                public byte ToByte()
                {
                    byte value = RESERVED_FIXED_BITS;
                    if (DIR3) value |= 0x40;
                    if (DIR2) value |= 0x20;
                    if (DIR1) value |= 0x10;
                    if (DIO3) value |= 0x04;
                    if (DIO2) value |= 0x02;
                    if (DIO1) value |= 0x01;
                    return value;
                }

                public static CTRL4_Settings FromByte(byte value)
                {
                    return new CTRL4_Settings
                    {
                        DIR3 = (value & 0x40) != 0,
                        DIR2 = (value & 0x20) != 0,
                        DIR1 = (value & 0x10) != 0,
                        DIO3 = (value & 0x04) != 0,
                        DIO2 = (value & 0x02) != 0,
                        DIO1 = (value & 0x01) != 0
                    };
                }

                public override string ToString()
                {
                    return $"CTRL4 = 0x{ToByte():X2}  (DIR3={DIR3}, DIR2={DIR2}, DIR1={DIR1}, DIO3={DIO3}, DIO2={DIO2}, DIO1={DIO1})";
                }
            }
            public class CTRL5_Settings
            {
                // 固定保留位 (Bit5–4 = 0)
                private const byte RESERVED_FIXED_BITS = 0b00000000;

                /// <summary>Bit7: CAL1 – Calibration select high bit
                /// <para>與 CAL0 組合選擇校正模式：</para>
                /// <para>00 = Self Offset/Gain</para>
                /// <para>01 = System Offset</para>
                /// <para>10 = System Gain</para>
                /// <para>11 = Reserved</para>
                /// </summary>
                public bool CAL1 { get; set; } = false;

                /// <summary>Bit6: CAL0 – Calibration select low bit
                /// <para>與 CAL1 組合選擇校正模式（詳見上表）。</para>
                /// </summary>
                public bool CAL0 { get; set; } = false;

                /// <summary>Bit3: NOSYSG – System Gain Calibration Enable (active low)
                /// <para>0 = 啟用系統增益校正，1 = 停用</para>
                /// </summary>
                public bool NOSYSG { get; set; } = true;

                /// <summary>Bit2: NOSYSO – System Offset Calibration Enable (active low)
                /// <para>0 = 啟用系統偏移校正，1 = 停用</para>
                /// </summary>
                public bool NOSYSO { get; set; } = true;

                /// <summary>Bit1: NOSCG – Self-Cal Gain Calibration Enable (active low)
                /// <para>0 = 啟用自校正增益，1 = 停用</para>
                /// </summary>
                public bool NOSCG { get; set; } = false;

                /// <summary>Bit0: NOSCO – Self-Cal Offset Calibration Enable (active low)
                /// <para>0 = 啟用自校正偏移，1 = 停用</para>
                /// </summary>
                public bool NOSCO { get; set; } = false;

                // ────────────────────────────────────────────────
                /// <summary>組出完整 8-bit 控制暫存器數值</summary>
                public byte ToByte()
                {
                    byte value = RESERVED_FIXED_BITS;
                    if (CAL1) value |= 0x80; // Bit7
                    if (CAL0) value |= 0x40; // Bit6
                    if (NOSYSG) value |= 0x08; // Bit3
                    if (NOSYSO) value |= 0x04; // Bit2
                    if (NOSCG) value |= 0x02;  // Bit1
                    if (NOSCO) value |= 0x01;  // Bit0
                    return value;
                }

                /// <summary>解析 byte → CTRL5 結構</summary>
                public static CTRL5_Settings FromByte(byte value)
                {
                    return new CTRL5_Settings
                    {
                        CAL1 = (value & 0x80) != 0,
                        CAL0 = (value & 0x40) != 0,
                        NOSYSG = (value & 0x08) != 0,
                        NOSYSO = (value & 0x04) != 0,
                        NOSCG = (value & 0x02) != 0,
                        NOSCO = (value & 0x01) != 0
                    };
                }

                public override string ToString()
                {
                    return $"CTRL5 = 0x{ToByte():X2}  (CAL1={CAL1}, CAL0={CAL0}, NOSYSG={NOSYSG}, NOSYSO={NOSYSO}, NOSCG={NOSCG}, NOSCO={NOSCO})";
                }
            }


            public string Config_Default_MAX11270()
            {
                const int MAX_RETRY = 5;

                // 建立所有控制暫存器的設定物件
                var ctrl1 = new CTRL1_Settings
                {
                    EXTCK = false,
                    SYNC = true,
                    PD1 = true,
                    PD0 = true,
                    UB = true,
                    FORMAT = true,
                    SCYCLE = true,
                    CONTSC = false
                };

                var ctrl2 = new CTRL2_Settings
                {
                    DGAIN1 = true,
                    DGAIN0 = true,
                    BUFEN = true,
                    LPMODE = false,
                    PGAEN = true,
                    PGA2 = true,
                    PGA1 = true,
                    PGA0 = true
                };

                var ctrl3 = new CTRL3_Settings
                {
                    ENMSYNC = true,
                    MODBITS = false,
                    DATA32 = false
                };

                var ctrl4 = new CTRL4_Settings
                {
                    DIR3 = false,
                    DIR2 = false,
                    DIR1 = false,
                    DIO3 = true,
                    DIO2 = true,
                    DIO1 = true
                };

                var ctrl5 = new CTRL5_Settings
                {
                    CAL1 = false,
                    CAL0 = false,
                    NOSYSG = false,
                    NOSYSO = false,
                    NOSCG = true,
                    NOSCO = true
                };

                //        // 建立註冊表
                //        var ctrlSettings = new (byte value, AdcCmd cmd, string name)[]
                // {
                // ((byte)ctrl1.ToByte(), AdcCmd.CTRL1_WRITE, "CTRL1"),
                //((byte)ctrl2.ToByte(), AdcCmd.CTRL2_WRITE, "CTRL2"),
                //((byte)ctrl3.ToByte(), AdcCmd.CTRL3_WRITE, "CTRL3"),
                //((byte)ctrl4.ToByte(), AdcCmd.CTRL4_WRITE, "CTRL4"),
                //((byte)ctrl5.ToByte(), AdcCmd.CTRL5_WRITE, "CTRL5"),
                //        };

                // 初始化等待
                //  WaitForReady("ADC 初始化", MAX_RETRY, 3000);

                WriteToBothAdcs(ctrl1.ToByte());
                ADC12_CTRL_0x16(ADC_Enable.Enable, ReadMode.Manual, RW_mode.Write, AdcCmd.CTRL1_WRITE);
                WaitForReady("Ctrl 1 write", MAX_RETRY, 3000);

                WriteToBothAdcs(ctrl2.ToByte());
                ADC12_CTRL_0x16(ADC_Enable.Enable, ReadMode.Manual, RW_mode.Write, AdcCmd.CTRL2_WRITE);
                WaitForReady("Ctrl 2 write", MAX_RETRY, 3000);

                WriteToBothAdcs(ctrl3.ToByte());
                ADC12_CTRL_0x16(ADC_Enable.Enable, ReadMode.Manual, RW_mode.Write, AdcCmd.CTRL3_WRITE);
                WaitForReady("Ctrl 3 write", MAX_RETRY, 3000);

                WriteToBothAdcs(ctrl4.ToByte());
                ADC12_CTRL_0x16(ADC_Enable.Enable, ReadMode.Manual, RW_mode.Write, AdcCmd.CTRL4_WRITE);
                WaitForReady("Ctrl 4 write", MAX_RETRY, 3000);

                WriteToBothAdcs(ctrl5.ToByte());
                ADC12_CTRL_0x16(ADC_Enable.Enable, ReadMode.Manual, RW_mode.Write, AdcCmd.CTRL5_WRITE);
                WaitForReady("Ctrl 5 write", MAX_RETRY, 3000);


                // Calibration
                ADC12_CTRL_0x16(ADC_Enable.Enable, ReadMode.Manual, RW_mode.Write, AdcCmd.CONVERSION_CAL_RATE15);
                Thread.Sleep(100);
                WaitForReady("校正", MAX_RETRY, 3000);

                //Conversion rate
                //這時候已經設定連續讀取的轉換速度，而且似乎不能再去問狀態了，否則會卡在等待 ADC ready 的迴圈裡，因為連續讀取模式下 ADC 是不會有 ready 狀態的

                ADC12_CTRL_0x16(ADC_Enable.Enable, ReadMode.Manual, RW_mode.Write, AdcCmd.CONVERSION_NO_CAL_RATE15);
                //WaitForReady("設定 Conversion rate", MAX_RETRY, 3000);

                return "Config done";
            }

            public string StartContinuous()
            {
                ADC12_CTRL_0x16(ADC_Enable.Enable, ReadMode.Auto, RW_mode.Read, AdcCmd.DATA_READ_continuous);

                // 啟動連續讀取後 ADC 會持續進行轉換並更新數據，但不會再有 Ready 狀態，因此無法再用 WaitForReady 等待 ADC 就緒。
                //const int MAX_RETRY = 5;
                //WaitForReady("啟動連續讀取", MAX_RETRY);

                return "Start Continuous done";
            }
            public void ForceResetADC()
            {
                Console.WriteLine("⚠️ ADC 無回應，執行強制軟重置...");

                // 建立 Reset 設定 (PD[1:0]=11)
                var ctrl1_reset = new CTRL1_Settings
                {
                    PD1 = true,
                    PD0 = true,
                    SYNC = true,
                    FORMAT = true,
                    EXTCK = false
                };

                // 嘗試送出 Reset 命令多次（因為第一次可能沒反應）
                for (int i = 0; i < 10; i++)
                {
                    ctrl.Write(Wcmd_ADC1_WRITE, ctrl1_reset.ToByte());
                    ADC12_CTRL_0x16(
                        ADC_Enable.Enable,
                        ReadMode.Manual,
                        RW_mode.Write,
                        AdcCmd.CTRL1_WRITE);

                    Thread.Sleep(10);
                    ctrl.Write(Wcmd_ADC12_CTRL, 0xFFFFFFFF); // Dummy clock
                    Thread.Sleep(5);
                }

                Thread.Sleep(20);
            }

            public void RecoverFromReset()
            {
                var ctrl1_normal = new CTRL1_Settings
                {
                    PD1 = false,
                    PD0 = false,
                    SYNC = true,
                    FORMAT = true
                };

                ctrl.Write(Wcmd_ADC1_WRITE, ctrl1_normal.ToByte());
                ADC12_CTRL_0x16(
                    ADC_Enable.Enable,
                    ReadMode.Manual,
                    RW_mode.Write,
                    AdcCmd.CTRL1_WRITE);

                Thread.Sleep(30);
            }

            public void StopContinuousRead()
            {
                // 結束連續模式
                ADC12_CTRL_0x16(
                    ADC_Enable.Enable,
                    ReadMode.Manual,
                    RW_mode.Read,
                    AdcCmd.STAT_READ);
            }

            public double AdcCodeToLoad(double originalValue = 0,
            double Gain = 128.0,        // PGA 增益
            double LoadCap = 2000.0,     // Load Cell 滿量程 (單位: N 或 kN 視設定)
            double Sensitivity = 0.0019995  // Load Cell 靈敏度 (2mV/V)
                )
            {
                // === 計算 Load Cell 滿量程差分電壓 (V) ===
                //double VfsCell = Sensitivity * Excitation;  
                double VfsCell = Sensitivity * 3;  // 例如 0.002 * 3 = 0.006 V

                // === 將 32-bit ADC 輸出碼轉換為實際輸入電壓 ===
                //  double VoltageValue = (3 / Math.Pow(2, 31)) * originalValue;
                double VoltageValue = (originalValue / Math.Pow(2, 31));

                // === 將電壓轉換為荷重 ===
                double LoadValue = ((LoadCap / (3 * Sensitivity * Gain / 1000.0)) * (VoltageValue - 1.50)) - Zero_Value;
                return LoadValue;
            }




            public void Set_ReturnToZero(double value)
            {
                Zero_Value = value;
            }

            public double Get_ReturnToZero()
            {
                return Zero_Value;
            }

            public void WriteToBothAdcs(uint data)
            {
                ctrl.Write(Wcmd_ADC1_WRITE, data);
                ctrl.Write(Wcmd_ADC2_WRITE, data);
            }

            private void WaitForReady(string step, int maxRetry, int timeoutMs = 1000)
            {
                int retry = 0;
                while (!SpinWait.SpinUntil(() =>
                {
                    Thread.Sleep(2);
                    return AD1.IsReady && AD2.IsReady && !AD1.IsBusy && !AD2.IsBusy;
                }, timeoutMs))
                {
                    if (++retry > maxRetry)
                    {
                        byte[] status = ReadStatus();
                        Console.WriteLine($"ADC_FLAG = {BitConverter.ToString(status)}");
                        throw new TimeoutException($"{step} 超時");
                    }
                }
            }

            /// <summary>
            /// 對 MAX11270 進行軟體 Reset，相當於拉低 RSTB 腳再釋放。
            /// 執行後 ADC 所有暫存器會恢復預設值，需重新 Config。
            /// </summary>
            public void ChipReset()
            {
                // STEP 1️⃣：送出 PD[1:0] = 11（Reset 模式）
                var ctrl1_reset = new CTRL1_Settings
                {
                    EXTCK = false,
                    SYNC = true,
                    PD1 = true,   // PD[1]
                    PD0 = true,   // PD[0] → ⚠️ 必須都是 1 才會真正 Reset
                    UB = false,
                    FORMAT = true,
                    SCYCLE = false,
                    CONTSC = false
                };

                // 將設定寫入 ADC1 / ADC2 暫存器
                ctrl.Write(Wcmd_ADC1_WRITE, ctrl1_reset.ToByte());
                ctrl.Write(Wcmd_ADC2_WRITE, ctrl1_reset.ToByte());

                // 寫入 CTRL1_REGISTER 觸發 Reset
                ADC12_CTRL_0x16(
                    ADC_Enable.Enable,
                    ReadMode.Manual,
                    RW_mode.Write,
                    AdcCmd.CTRL1_WRITE);

                Thread.Sleep(15); // 保持 Reset 狀態至少 10ms

                // STEP 2️⃣：寫回 PD[1:0] = 00（恢復正常模式）
                var ctrl1_normal = new CTRL1_Settings
                {
                    EXTCK = false,
                    SYNC = true,
                    PD1 = false,
                    PD0 = false,
                    UB = false,
                    FORMAT = true,
                    SCYCLE = false,
                    CONTSC = false
                };

                ctrl.Write(Wcmd_ADC1_WRITE, ctrl1_normal.ToByte());
                ctrl.Write(Wcmd_ADC2_WRITE, ctrl1_normal.ToByte());

                ADC12_CTRL_0x16(
                    ADC_Enable.Enable,
                    ReadMode.Manual,
                    RW_mode.Write,
                    AdcCmd.CTRL1_WRITE);

                Thread.Sleep(20); // 給晶片穩壓器時間恢復

                // STEP 3️⃣：驗證狀態
                ADC12_CTRL_0x16(
                    ADC_Enable.Enable,
                    ReadMode.Manual,
                    RW_mode.Read,
                    AdcCmd.STAT_READ);

                Console.WriteLine("✅ MAX11270 Chip Reset Completed.");
            }

        }

        public class PROM
        {
            private FTDI_Ctrl ctrl;
            private FTDI FtdiBurstRead;
            private FTDI FtdiNormalCtrl;
            public PROM(FTDI_Ctrl ctrl)
            {
                this.ctrl = ctrl;
            }

            /// <summary>
            /// 這Function如果PROM ERROR會丟出例外
            /// <para>回傳true代表PROM ready</para>
            /// </summary>
            /// <param name="CMDPage"></param>
            /// <returns></returns>
            /// <exception cref="InvalidOperationException"></exception>
            public bool IsReady
            {
                get
                {
                    bool res = this.ReadStatus();
                    return res;  // 倒數第二位元
                }
            }

            #region Enums
            public enum I2C_clockFrequency
            {
                Anti_Noise = 0b00,
                Normal = 0b01,
                Standard = 0b10,
                Fast = 0b11,
            }

            public enum Type
            {
                bits_32k_to_512k = 1,
                bits_2K = 0,
            }

            public enum Hardware
            {
                Motherboard = 0b010,
                Encoder1 = 0b100,
                Encoder2 = 0b101,
                LoadCell1 = 0b110,
                LoadCell2 = 0b111,
            }

            public enum RW_mode
            {
                Read = 1,
                Write = 0,
            }

            public enum ByteSize
            {
                byte8 = 0b00,
                byte16 = 0b01,
                byte32 = 0b10,
                byte64 = 0b11,
            }

            public enum WriteBuffer : uint
            {
                Buffer1 = 0x0A,
                Buffer2 = 0x0B,
                Buffer3 = 0x0C,
                Buffer4 = 0x0D,
            }
            #endregion
            public void WaitReady(int timeoutMs = 100)
            {
                var sw = Stopwatch.StartNew();

                while (!ReadStatus())
                {
                    if (sw.ElapsedMilliseconds > timeoutMs)
                        throw new TimeoutException("PROM Ready Timeout!");

                    Thread.Sleep(1);
                }
            }

            private bool WaitReady(int retryTimes = 5, int delayMs = 10)
            {
                for (int i = 0; i < retryTimes; i++)
                {
                    try
                    {

                        if (ReadStatus())
                            return true; // ✅ Ready
                    }
                    catch
                    {
                        // ❌ PROM Status = Error → 不要再繼續試
                        throw;
                    }

                    Thread.Sleep(delayMs);
                }

                return false; // ❌ Busy Timeout
            }


            public bool ReadStatus()//01=busy 10 = error
            {
                ctrl.Write(0x00, 0x09);
                Thread.Sleep(10); // 等待讀取完成
                uint Data = ctrl.Read(out byte add);
                uint head2Bits = Data & 0b11;
                if (head2Bits == 1)
                {
                    return false;//busy
                }
                else if (head2Bits == 2)
                {
                    throw new InvalidOperationException("PROM is error");
                }
                else
                {
                    return true;//not busy
                }
            }

            public void TurnPage(int CMDPage, WriteBuffer buffer)//01=busy 10 = error
            {
                ulong data = 0;
                data = (ulong)buffer + (ulong)(CMDPage * 0x100);

                ctrl.Write(0x00, data);
            }

            public void TurnPage(int CMDPage)//01=busy 10 = error
            {
                ulong data = 0;
                data = (ulong)(CMDPage * 0x100) + 0x09;

                ctrl.Write(0x00, data);
            }

            public void Ctrl(I2C_clockFrequency clk, Type type, Hardware hw, RW_mode rw, ByteSize size, uint promAddressHex)
            {
                uint value = 0;
                value |= (uint)clk << 24;
                value |= (uint)type << 22;
                value |= (uint)hw << 19;
                value |= (uint)rw << 17;
                value |= (uint)size << 15;
                value |= (promAddressHex & 0xFFFF);

                ctrl.Write(0x09, value);
            }

            public void PROM_Write(int Page, WriteBuffer buffer, uint DATA)
            {
                // 防呆
                if (Page < 0 || Page > 3)
                    throw new ArgumentOutOfRangeException(nameof(Page), "Page must be 0-3.");

                // 防呆
                if (DATA > 0xFFFFFFFF)
                    throw new ArgumentOutOfRangeException(nameof(DATA), "Must be 32-bit value (0~0xFFFFFFFF).");

                TurnPage(Page);
                Thread.Sleep(10); // 等待切換page完成
                ctrl.Write((byte)buffer, DATA);
                Thread.Sleep(10); // 等待write完成
            }

            private uint? PROM_Read(int Page, WriteBuffer buffer)
            {
                if (Page < 0 || Page > 3)
                    throw new ArgumentOutOfRangeException(nameof(Page), "Page must be 0-3.");

                TurnPage(Page, buffer);
                Thread.Sleep(10); // 等待讀取完成
                return ctrl.Read(out byte add);
            }

            WriteBuffer[] buffers = new[] { WriteBuffer.Buffer1, WriteBuffer.Buffer2, WriteBuffer.Buffer3, WriteBuffer.Buffer4 };
            public void PageWrite(Hardware hardwareID, uint Address, ByteSize byteSize, uint[] DATA)
            {
                if (DATA == null || DATA.Length != 16)
                    throw new ArgumentException("DATA must contain 16 x 32-bit values (64 bytes total).");

                const int totalPages = 4;
                const int BufferPerPage = 4;
                int dataIndex = 0;

                for (int page = 0; page < totalPages; page++)
                {
                    for (int row = 0; row < BufferPerPage; row++)
                    {
                        WaitReady(5, 5);
                        PROM_Write(page, buffers[row], DATA[dataIndex++]);
                    }
                }

                Ctrl(I2C_clockFrequency.Anti_Noise, Type.bits_32k_to_512k, hardwareID, RW_mode.Write, byteSize, Address);
                WaitReady(5, 5);
            }

            public uint[] PageRead(Hardware hardwareID, uint Address, ByteSize byteSize)
            {
                uint[] result = new uint[16];
                int index = 0;

                // 1. PROM 啟動 Read 動作 → 新資料進暫存器
                Ctrl(I2C_clockFrequency.Anti_Noise, Type.bits_32k_to_512k, hardwareID, RW_mode.Read, byteSize, Address);

                // 2.  等 IDLE
                if (!WaitReady(5, 10))
                    throw new InvalidOperationException("PROM Read timeout");

                // 3.  Read 16 Rows = 64 bytes
                for (int page = 0; page < 4; page++)
                {
                    for (int row = 0; row < 4; row++)
                    {
                        if (!WaitReady(5, 5))
                            throw new InvalidOperationException("Buffer not ready");

                        result[index++] = PROM_Read(page, buffers[row])
                                          ?? throw new Exception("Null read error");
                    }
                }
                return result;
            }
        }

        public class Encoder
        {
            //example
            #region example
            //            Encoder encoder = new Encoder(ctrl);

            //            var enc1 = new Encoder.ChannelConfig
            //            {
            //                Filter = Encoder.Filter._921KHz,
            //                WritePara = Encoder.WritePara.InitPara,
            //                Enable = true,
            //                ResetToInit = false,
            //                ABPhaseExchange = false
            //            };

            //            var enc2 = new Encoder.ChannelConfig
            //            {
            //                Filter = Encoder.Filter._1_23MHz,
            //                WritePara = Encoder.WritePara.SoftwareUpLimit,
            //                Enable = true,
            //                ResetToInit = false,
            //                ABPhaseExchange = true
            //            };

            //            var enc3 = new Encoder.ChannelConfig
            //            {
            //                Filter = Encoder.Filter._1_84MHz,
            //                WritePara = Encoder.WritePara.SoftwareDownLimit,
            //                Enable = false,
            //                ResetToInit = false,
            //                ABPhaseExchange = false
            //            };

            //            var enc4 = new Encoder.ChannelConfig
            //            {
            //                Filter = Encoder.Filter._3_68MHz,
            //                WritePara = Encoder.WritePara.SoftwareDensity,
            //                Enable = true,
            //                ResetToInit = true,
            //                ABPhaseExchange = true
            //            };

            //            uint packet = encoder.BuildPacket(enc1, enc2, enc3, enc4);

            //            // 寫入
            //            encoder.Ctrl(packet);

            //// 或直接
            //encoder.Ctrl(enc1, enc2, enc3, enc4);
            #endregion


            private readonly FTDI_Ctrl ctrl;
            private FTDI FtdiBurstRead;
            private FTDI FtdiNormalCtrl;

            public Encoder(FTDI_Ctrl ctrl)
            {
                this.ctrl = ctrl ?? throw new ArgumentNullException(nameof(ctrl));
            }

            /// <summary>
            /// 3 bits
            /// </summary>
            public enum Filter : byte
            {
                _737KHz = 0b000,
                _921KHz = 0b001,
                _1_05MHz = 0b010,
                _1_23MHz = 0b011,
                _1_47MHz = 0b100,
                _1_84MHz = 0b101,
                _2_45MHz = 0b110,
                _3_68MHz = 0b111
            }

            /// <summary>
            /// 2 bits
            /// </summary>
            public enum WritePara : byte
            {
                InitPara = 0b00,
                SoftwareUpLimit = 0b01,
                SoftwareDownLimit = 0b10,
                SoftwareDensity = 0b11
            }

            /// <summary>
            /// 單一 Encoder 通道設定
            /// bit[7:5] = Filter
            /// bit[4:3] = WritePara
            /// bit[2]   = Enable
            /// bit[1]   = ResetToInit
            /// bit[0]   = ABPhaseExchange
            /// </summary>
            public class ChannelConfig
            {
                public Filter Filter { get; set; } = Filter._737KHz;
                public WritePara WritePara { get; set; } = WritePara.InitPara;

                /// <summary>
                /// 0 = Disable
                ///1 = Enable
                /// </summary>
                public bool Enable { get; set; } = true;

                /// <summary>
                ///0 = Normal
                ///1 = InitPara
                /// </summary>
                public bool ResetToInit { get; set; } = false;

                /// <summary>
                /// 0 = Normal
                ///1 = Exchange
                /// </summary>
                public bool ABPhaseExchange { get; set; } = false;

                public ChannelConfig()
                {
                }

                public ChannelConfig(
                    Filter filter,
                    WritePara writePara,
                    bool enable = false,
                    bool resetToInit = false,
                    bool abPhaseExchange = false)
                {
                    Filter = filter;
                    WritePara = writePara;
                    Enable = enable;
                    ResetToInit = resetToInit;
                    ABPhaseExchange = abPhaseExchange;
                }

                public override string ToString()
                {
                    return $"Filter={Filter}, WritePara={WritePara}, Enable={Enable}, ResetToInit={ResetToInit}, ABPhaseExchange={ABPhaseExchange}";
                }
            }

            /// <summary>
            /// 建立單一 channel 的 8-bit 設定值
            /// </summary>
            public byte BuildChannelByte(ChannelConfig config)
            {
                if (config == null)
                    throw new ArgumentNullException(nameof(config));

                byte value = 0;

                value |= (byte)(((byte)config.Filter & 0b111) << 5);
                value |= (byte)(((byte)config.WritePara & 0b11) << 3);

                if (config.Enable)
                    value |= 0b00000100;

                if (config.ResetToInit)
                    value |= 0b00000010;

                if (config.ABPhaseExchange)
                    value |= 0b00000001;

                return value;
            }

            /// <summary>
            /// 將 4 個 channel 設定組成 32-bit packet
            /// 排列方式：
            /// [31:24] ENC4
            /// [23:16] ENC3
            /// [15:8]  ENC2
            /// [7:0]   ENC1
            /// </summary>
            public uint BuildPacket(
                ChannelConfig enc1,
                ChannelConfig enc2,
                ChannelConfig enc3,
                ChannelConfig enc4)
            {
                byte b1 = BuildChannelByte(enc1);
                byte b2 = BuildChannelByte(enc2);
                byte b3 = BuildChannelByte(enc3);
                byte b4 = BuildChannelByte(enc4);

                uint packet =
                    ((uint)b4 << 24) |
                    ((uint)b3 << 16) |
                    ((uint)b2 << 8) |
                    b1;

                return packet;
            }

            /// <summary>
            /// 陣列版本，長度必須為 4
            /// channels[0]=ENC1, channels[1]=ENC2, channels[2]=ENC3, channels[3]=ENC4
            /// </summary>
            public uint BuildPacket(ChannelConfig[] channels)
            {
                if (channels == null)
                    throw new ArgumentNullException(nameof(channels));

                if (channels.Length != 4)
                    throw new ArgumentException("Encoder channel count must be exactly 4.", nameof(channels));

                return BuildPacket(channels[0], channels[1], channels[2], channels[3]);
            }

            /// <summary>
            /// 直接寫入已組好的 packet
            /// </summary>
            public void Ctrl(uint packet)
            {
                ctrl.Write(Wcmd_ENC_CTRL, packet);
            }

            /// <summary>
            /// 直接用 4 個 channel 設定寫入
            /// </summary>
            public void Ctrl(
                ChannelConfig enc1,
                ChannelConfig enc2,
                ChannelConfig enc3,
                ChannelConfig enc4)
            {
                uint packet = BuildPacket(enc1, enc2, enc3, enc4);
                Ctrl(packet);
            }

            /// <summary>
            /// 陣列版本直接寫入
            /// </summary>
            public void Ctrl(ChannelConfig[] channels)
            {
                uint packet = BuildPacket(channels);
                Ctrl(packet);
            }

            /// <summary>
            /// 建立一個預設通道設定
            /// </summary>
            public static ChannelConfig CreateDefaultChannel()
            {
                return new ChannelConfig
                {
                    Filter = Filter._1_47MHz,
                    WritePara = WritePara.InitPara,
                    Enable = true,
                    ResetToInit = false,
                    ABPhaseExchange = false
                };
            }

            /// <summary>
            /// 建立 4 通道預設設定
            /// </summary>
            public static ChannelConfig[] CreateDefaultChannels()
            {
                return new[]
                {
                CreateDefaultChannel(), // ENC1
                CreateDefaultChannel(), // ENC2
                CreateDefaultChannel(), // ENC3
                CreateDefaultChannel()  // ENC4
            };
            }

            /// <summary>
            /// 將 packet 解析回 4 個 channel，方便除錯
            /// </summary>
            public ChannelConfig[] ParsePacket(uint packet)
            {
                return new[]
                {
                ParseChannel((byte)(packet & 0xFF)),           // ENC1
                ParseChannel((byte)((packet >> 8) & 0xFF)),    // ENC2
                ParseChannel((byte)((packet >> 16) & 0xFF)),   // ENC3
                ParseChannel((byte)((packet >> 24) & 0xFF))    // ENC4
            };
            }

            /// <summary>
            /// 將單一 byte 解析回 ChannelConfig，方便除錯
            /// </summary>
            public ChannelConfig ParseChannel(byte value)
            {
                return new ChannelConfig
                {
                    Filter = (Filter)((value >> 5) & 0b111),
                    WritePara = (WritePara)((value >> 3) & 0b11),
                    Enable = (value & 0b00000100) != 0,
                    ResetToInit = (value & 0b00000010) != 0,
                    ABPhaseExchange = (value & 0b00000001) != 0
                };
            }
            public enum Channel
            {
                ENC1 = 1,
                ENC2 = 2,
                ENC3 = 3,
                ENC4 = 4
            }

            private readonly byte[] readCmdTable =
{
    0,              // index 0 不用
    Rcmd_ENC1_READ,
    Rcmd_ENC2_READ,
    Rcmd_ENC3_READ,
    Rcmd_ENC4_READ
};

            public uint ReadValue(int channel)
            {
                if (channel < 1 || channel > 4)
                    throw new ArgumentOutOfRangeException(nameof(channel));

                return ReadValue((Channel)channel);
            }


            public uint ReadValue(Channel ch)
            {
                int index = (int)ch;

                if (index < 1 || index >= readCmdTable.Length)
                    throw new ArgumentOutOfRangeException(nameof(ch));

                return (uint)ctrl.Single_Read(readCmdTable[index]);
            }

            public uint ReadENC1() => ReadValue(Channel.ENC1);
            public uint ReadENC2() => ReadValue(Channel.ENC2);
            public uint ReadENC3() => ReadValue(Channel.ENC3);
            public uint ReadENC4() => ReadValue(Channel.ENC4);
        }

        public class BurstMode
        {
            private readonly FTDI_Ctrl ctrlPortB;
            private readonly FTDI_Ctrl ctrlPortC;
            private FTDI FtdiBurstRead;
            private FTDI FtdiNormalCtrl;

            public BurstMode(FTDI_Ctrl portB, FTDI_Ctrl portC)
            {
                ctrlPortB = portB;
                ctrlPortC = portC;
            }

            public class BurstModeConfig
            {
                public byte Source { get; set; }       // 0~7
                public ushort BrtTime { get; set; }    // 0~4095

                public bool Sin12 { get; set; }
                public bool Sin34 { get; set; }
                public bool Sin56 { get; set; }
                public bool Peri { get; set; }

                public bool Enc1 { get; set; }
                public bool Enc2 { get; set; }
                public bool Enc3 { get; set; }
                public bool Enc4 { get; set; }

                public bool Dsp1 { get; set; }
                public bool Dsp2 { get; set; }
                public bool Dsp3 { get; set; }
                public bool Dsp4 { get; set; }

                public bool Adc1 { get; set; }
                public bool Adc2 { get; set; }
                public bool Adc3 { get; set; }
                public bool Adc4 { get; set; }
            }

            private static BurstModeConfig lastBurstModeConfig;

            public static byte[] bulidPack(BurstModeConfig cfg)
            {
                if (cfg == null)
                    throw new ArgumentNullException(nameof(cfg));

                if (cfg.Source > 7)
                    throw new ArgumentOutOfRangeException(nameof(cfg.Source), "Source must be 0~7.");

                if (cfg.BrtTime > 0x0FFF)
                    throw new ArgumentOutOfRangeException(nameof(cfg.BrtTime), "BrtTime must be 0~4095.");

                byte byte0 = 0;
                byte byte1 = 0;
                byte byte2 = 0;
                byte byte3 = 0;

                // Byte0
                // bit6~4 = BRT_SOURCE2:0
                byte0 |= (byte)((cfg.Source & 0x07) << 4);

                // bit3~0 = BRT_TIME11:8
                byte0 |= (byte)((cfg.BrtTime >> 8) & 0x0F);

                // Byte1
                // bit7~0 = BRT_TIME7:0
                byte1 = (byte)(cfg.BrtTime & 0xFF);

                // Byte2
                if (cfg.Sin56) byte2 |= (1 << 7);
                if (cfg.Sin34) byte2 |= (1 << 6);
                if (cfg.Sin12) byte2 |= (1 << 5);
                if (cfg.Peri) byte2 |= (1 << 4);

                if (cfg.Enc4) byte2 |= (1 << 3);
                if (cfg.Enc3) byte2 |= (1 << 2);
                if (cfg.Enc2) byte2 |= (1 << 1);
                if (cfg.Enc1) byte2 |= (1 << 0);

                // Byte3
                if (cfg.Dsp4) byte3 |= (1 << 7);
                if (cfg.Dsp3) byte3 |= (1 << 6);
                if (cfg.Dsp2) byte3 |= (1 << 5);
                if (cfg.Dsp1) byte3 |= (1 << 4);

                if (cfg.Adc4) byte3 |= (1 << 3);
                if (cfg.Adc3) byte3 |= (1 << 2);
                if (cfg.Adc2) byte3 |= (1 << 1);
                if (cfg.Adc1) byte3 |= (1 << 0);

                return new byte[] { byte0, byte1, byte2, byte3 };
            }

            public void StartBurst(BurstModeConfig cfg)
            {
                lastBurstModeConfig = cfg;
                byte[] packet = bulidPack(cfg);

                if (packet == null || packet.Length != 4)
                    throw new InvalidOperationException("Burst packet must be exactly 4 bytes.");

                ulong packetValue =
                    ((ulong)packet[0] << 24) |
                    ((ulong)packet[1] << 16) |
                    ((ulong)packet[2] << 8) |
                    ((ulong)packet[3]);

                ctrlPortC.Write(Wcmd_BURST_CTRL, packetValue);

            }

            public uint BustReadRawData()
            {

                return ctrlPortB.Read(out byte add);
            }

        }

        public enum BurstValueType
        {
            UInt24,
            UInt32
        }

        public class BurstFieldDefinition
        {
            public byte Address { get; set; }
            public string Name { get; set; }
            public BurstValueType Type { get; set; }
        }

        public class BurstDecodedFrame
        {
            public byte Address { get; set; }
            public string Name { get; set; }
            public BurstValueType Type { get; set; }
            public uint RawValue { get; set; }
            public DateTime Timestamp { get; set; } = DateTime.Now;

            public int SignedValue
            {
                get
                {
                    if (Type == BurstValueType.UInt24)
                    {
                        int v = (int)(RawValue & 0xFFFFFF);
                        if ((v & 0x800000) != 0)
                            v |= unchecked((int)0xFF000000);
                        return v;
                    }
                    else
                    {
                        return unchecked((int)RawValue);
                    }
                }
            }
        }

        public class BurstFrameDecoder
        {
            private readonly Dictionary<byte, BurstFieldDefinition> _definitions;
            private readonly List<byte> _buffer = new List<byte>();

            public BurstFrameDecoder(IEnumerable<BurstFieldDefinition> definitions)
            {
                _definitions = new Dictionary<byte, BurstFieldDefinition>();
                foreach (var def in definitions)
                {
                    _definitions[def.Address] = def;
                }
            }

            public void Append(byte[] data, int length)
            {
                for (int i = 0; i < length; i++)
                    _buffer.Add(data[i]);
            }

            public List<BurstDecodedFrame> DecodeAvailable()
            {
                var result = new List<BurstDecodedFrame>();

                while (true)
                {
                    if (_buffer.Count < 1)
                        break;

                    byte address = _buffer[0];

                    if (!_definitions.TryGetValue(address, out var def))
                    {
                        // 找不到這個 address，表示可能已經不同步，丟掉 1 byte 重找
                        _buffer.RemoveAt(0);
                        continue;
                    }

                    int payloadBytes = def.Type == BurstValueType.UInt24 ? 3 : 5;
                    int frameLength = 1 + payloadBytes;

                    if (_buffer.Count < frameLength)
                        break; // 還不夠一包，等下次資料進來

                    uint rawValue = 0;

                    if (def.Type == BurstValueType.UInt24)
                    {
                        uint b1 = (uint)(_buffer[1] & 0x7F);
                        uint b2 = (uint)(_buffer[2] & 0x7F);
                        uint b3 = (uint)(_buffer[3] & 0x7F);

                        rawValue = (b1 << 16) |
                                   (b2 << 8) |
                                   (b3);
                    }
                    else
                    {
                        // 32 bit = 1個4bit + 4個7bit = 5 bytes
                        uint p1 = (uint)(_buffer[1] & 0x0F);
                        uint p2 = (uint)(_buffer[2] & 0x7F);
                        uint p3 = (uint)(_buffer[3] & 0x7F);
                        uint p4 = (uint)(_buffer[4] & 0x7F);
                        uint p5 = (uint)(_buffer[5] & 0x7F);

                        rawValue = (p1 << 28) |
                                   (p2 << 21) |
                                   (p3 << 14) |
                                   (p4 << 7) |
                                   (p5);
                    }

                    result.Add(new BurstDecodedFrame
                    {
                        Address = address,
                        Name = def.Name,
                        Type = def.Type,
                        RawValue = rawValue
                    });

                    _buffer.RemoveRange(0, frameLength);
                }

                return result;
            }

            public class BurstValueStore
            {
                private readonly Dictionary<string, BurstDecodedFrame> _latest = new Dictionary<string, BurstDecodedFrame>();

                public void Update(BurstDecodedFrame frame)
                {
                    _latest[frame.Name] = frame;
                }

                public BurstDecodedFrame Get(string name)
                {
                    _latest.TryGetValue(name, out var value);
                    return value;
                }

                public IReadOnlyDictionary<string, BurstDecodedFrame> All => _latest;
            }

            public class BurstReader
            {
                private readonly A4MB.FTDI_Ctrl _burstPort;
                private readonly BurstFrameDecoder _decoder;
                private readonly BurstValueStore _store;

                private CancellationTokenSource _cts;

                public event Action<BurstDecodedFrame> FrameDecoded;

                public BurstReader(A4MB.FTDI_Ctrl burstPort, IEnumerable<BurstFieldDefinition> definitions)
                {
                    _burstPort = burstPort;
                    _decoder = new BurstFrameDecoder(definitions);
                    _store = new BurstValueStore();
                }

                public BurstValueStore Store => _store;

                public void Start()
                {
                    if (_cts != null) return;

                    _cts = new CancellationTokenSource();
                    Task.Run(() => ReadLoop(_cts.Token));
                }

                public void Stop()
                {
                    _cts?.Cancel();
                    _cts = null;
                }

                private async Task ReadLoop(CancellationToken token)
                {
                    while (!token.IsCancellationRequested)
                    {
                        try
                        {
                            uint rx = _burstPort.GetRxBytesAvailable();
                            if (rx > 0)
                            {
                                var raw = _burstPort.ReadRaw(rx);
                                _decoder.Append(raw, raw.Length);

                                var frames = _decoder.DecodeAvailable();
                                foreach (var frame in frames)
                                {
                                    _store.Update(frame);
                                    FrameDecoded?.Invoke(frame);
                                }
                            }
                            else
                            {
                                await Task.Delay(1, token);
                            }
                        }
                        catch
                        {
                            await Task.Delay(10, token);
                        }
                    }
                }
            }
        }
    }
}

