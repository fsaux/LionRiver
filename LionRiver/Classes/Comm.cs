﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO.Ports;
using System.Threading;

namespace LionRiver
{

    public partial class MainWindow : Window
    {

        static SerialPort SerialPort1, SerialPort2, SerialPort3, SerialPort4;
        static bool terminateThread1, terminateThread2, terminateThread3, terminateThread4;
        Thread readThread1, readThread2, readThread3, readThread4;

        static bool rmc_sentence_available = false;
        static bool rmb_sentence_available = false;
        static bool mwv_sentence_available = false;
        static bool vhw_sentence_available = false;
        static bool dpt_sentence_available = false;
        static bool hdg_sentence_available = false;
        static bool mtw_sentence_available = false;
        static bool ptak_sentence_available = false;
        static bool phdr_sentence_available = false;


        static string rmc_sentence, rmb_sentence, mwv_sentence, vhw_sentence, dpt_sentence, hdg_sentence, mtw_sentence;
        static string ptak_sentence;
        static int ptak_cntr = 0; //cycle through PTAK sentence, one per second.
        static string phdr1_sentence, phdr2_sentence, phdr3_sentence, phdr4_sentence, phdr5_sentence, phdr6_sentence;

        public void InitializeSerialPort1()
        {
            try
            {
                SerialPort1.PortName = Properties.Settings.Default.Port1;
            }
            catch (ArgumentException) { }
            SerialPort1.BaudRate = 4800;
            SerialPort1.Parity = Parity.None;
            SerialPort1.DataBits = 8;
            SerialPort1.StopBits = StopBits.One;
            SerialPort1.Handshake = Handshake.None;
            SerialPort1.RtsEnable = true;
            SerialPort1.ReadTimeout = 2000;
            SerialPort1.WriteTimeout = 2000;

            try
            {
                SerialPort1.Open();
            }
            catch (Exception) { }
        }

        public void InitializeSerialPort2()
        {
            try
            {
                SerialPort2.PortName = Properties.Settings.Default.Port2;
            }
            catch (ArgumentException) { }
            SerialPort2.BaudRate = 4800;
            SerialPort2.Parity = Parity.None;
            SerialPort2.DataBits = 8;
            SerialPort2.StopBits = StopBits.One;
            SerialPort2.Handshake = Handshake.None;
            SerialPort2.RtsEnable = true;
            SerialPort2.ReadTimeout = 2000;
            SerialPort2.WriteTimeout = 1000;

            try
            {
                SerialPort2.Open();
            }
            catch (Exception) { }
        }

        public void InitializeSerialPort3()
        {
            try
            {
                SerialPort3.PortName = Properties.Settings.Default.Port3;
            }
            catch (ArgumentException) { }
            SerialPort3.BaudRate = 4800;
            SerialPort3.Parity = Parity.None;
            SerialPort3.DataBits = 8;
            SerialPort3.StopBits = StopBits.One;
            SerialPort3.Handshake = Handshake.None;
            SerialPort3.RtsEnable = true;
            SerialPort3.ReadTimeout = 2000;
            SerialPort3.WriteTimeout = 2000;

            try
            {
                SerialPort3.Open();
            }
            catch (Exception) { }
        }

        public void InitializeSerialPort4()
        {
            try
            {
                SerialPort4.PortName = Properties.Settings.Default.Port4;
            }
            catch (ArgumentException) { }
            SerialPort4.BaudRate = 4800;
            SerialPort4.Parity = Parity.None;
            SerialPort4.DataBits = 8;
            SerialPort4.StopBits = StopBits.One;
            SerialPort4.Handshake = Handshake.None;
            SerialPort4.RtsEnable = true;
            SerialPort4.ReadTimeout = 2000;
            SerialPort4.WriteTimeout = 2000;

            try
            {
                SerialPort4.Open();
            }
            catch (Exception) { }
        }

        public void CloseSerialPort1()
        {
            terminateThread1 = true;
            readThread1.Join();
            SerialPort1.Close();
        }

        public void CloseSerialPort2()
        {
            terminateThread2 = true;
            readThread2.Join();
            SerialPort2.Close();
        }

        public void CloseSerialPort3()
        {
            terminateThread3 = true;
            readThread3.Join();
            SerialPort3.Close();
        }

        public void CloseSerialPort4()
        {
            terminateThread4 = true;
            readThread4.Join();
            SerialPort4.Close();
        }

        public static void ReadSerial1()
        {
            string message = "";
            while (!terminateThread1)
            {
                if (SerialPort1.IsOpen)
                {
                    try
                    {
                        message = SerialPort1.ReadLine();
                    }
                    catch (Exception)
                    {
                        message = "";
                    }
                    ProcessNMEA(message, 0);
                }
                else
                    Thread.Sleep(1000);
            }
        }

        public static void ReadSerial2()
        {
            string message = "";
            while (!terminateThread2)
            {
                if (SerialPort2.IsOpen)
                {
                    try
                    {
                        message = SerialPort2.ReadLine();
                    }
                    catch (Exception)
                    {
                        message = "";
                    }
                    ProcessNMEA(message, 1);
                }
                else
                    Thread.Sleep(1000);
            }
        }

        public static void ReadSerial3()
        {
            string message = "";
            while (!terminateThread3)
            {
                if (SerialPort3.IsOpen)
                {
                    try
                    {
                        message = SerialPort3.ReadLine();
                    }
                    catch (Exception)
                    {
                        message = "";
                    }
                    ProcessNMEA(message, 2);
                }
                else
                    Thread.Sleep(1000);
            }
        }

        public static void ReadSerial4()
        {
            string message = "";
            while (!terminateThread4)
            {
                if (SerialPort4.IsOpen)
                {
                    try
                    {
                        message = SerialPort4.ReadLine();
                    }
                    catch (Exception)
                    {
                        message = "";
                    }
                    ProcessNMEA(message, 3);
                }
                else
                    Thread.Sleep(1000);
            }
        }

        static public void ProcessNMEA(string message, int port)
        {
            if (message != "")
            {
                message += "\n";

                string[] msg = null;
                string NMEASentence;

                try
                {
                    msg = message.Split(',');
                    NMEASentence = msg[0].Substring(3, 3);
                }
                catch (Exception)
                {
                    NMEASentence = "";
                    MarkErrorOnNMEA(port, "Bad NMEA sentence");
                }

                switch (NMEASentence)
                {
                    case "RMC":
                        if (port == Properties.Settings.Default.NavSentence.InPort)
                        {
                            try
                            {
                                lat = ConvertToDeg(msg[3]);
                                if (msg[4] == "S")
                                    lat = -lat;
                                lon = ConvertToDeg(msg[5]);
                                if (msg[6] == "W")
                                    lon = -lon;
                                sog = double.Parse(msg[7]);
                                cog = double.Parse(msg[8]);
                                if (msg[10] != "")
                                {
                                    mvar1 = double.Parse(msg[10]);
                                    if (msg[11][0] == 'W')
                                        mvar1 = -mvar1;
                                }

                                rmc_received = true;
                                MarkDataReceivedOnNMEA(port);
                                rmc_sentence = message;
                                rmc_sentence_available = true;


                            }
                            catch (Exception)
                            {
                                MarkErrorOnNMEA(port, "Bad RMC sentence");
                            }

                        }
                        break;

                    //case "RMB":
                    //if (port==Properties.Settings.Default.RouteSentence.InPort)
                    //    {
                    //        try
                    //        {
                    //            XTE.Val = double.Parse(msg[2]);
                    //            WPT.Val.str = msg[5];

                    //            WLAT.Val = ConvertToDeg(msg[6]);
                    //            if (msg[7] == "S")
                    //                WLAT.Val = -WLAT.Val;
                    //            WLON.Val = ConvertToDeg(msg[8]);
                    //            if (msg[9] == "W")
                    //                WLON.Val = -WLON.Val;

                    //            DST.Val = double.Parse(msg[10]);
                    //            BRG.Val = double.Parse(msg[11]);

                    //            XTE.SetValid();
                    //            WPT.SetValid();
                    //            WLAT.SetValid();
                    //            WLON.SetValid();
                    //            DST.SetValid();
                    //            BRG.SetValid();

                    //        }
                    //        catch (Exception) { }

                    //        if (port == 1) DataReceivedOnNMEA1 = true;
                    //        if (port == 2) DataReceivedOnNMEA2 = true;
                    //        if (port == 3) DataReceivedOnNMEA3 = true;
                    //        if (port == 4) DataReceivedOnNMEA4 = true;
                    //    }
                    //    break;

                    case "VHW":
                        if (port == Properties.Settings.Default.HullSpeedSentence.InPort)
                        {
                            try
                            {
                                spd = double.Parse(msg[5]);

                                vhw_received = true;
                                MarkDataReceivedOnNMEA(port);
                                vhw_sentence = message;
                                vhw_sentence_available = true;

                            }
                            catch (Exception)
                            {
                                MarkErrorOnNMEA(port, "Bad VHW sentence");
                            }
                        }
                        break;

                    case "DBT":
                        if (port == Properties.Settings.Default.DepthSentence.InPort)
                        {
                            try
                            {
                                dpt = double.Parse(msg[3]);

                                dpt_received = true;
                                MarkDataReceivedOnNMEA(port);
                                dpt_sentence = message;
                                dpt_sentence_available = true;

                            }
                            catch (Exception)
                            {
                                MarkErrorOnNMEA(port, "Bad DBT sentence");
                            }
                        }
                        break;

                    case "DPT":
                        if (port == Properties.Settings.Default.DepthSentence.InPort)
                        {
                            try
                            {
                                dpt = double.Parse(msg[1]);

                                dpt_received = true;
                                MarkDataReceivedOnNMEA(port);
                                dpt_sentence = message;
                                dpt_sentence_available = true;

                            }
                            catch (Exception)
                            {
                                MarkErrorOnNMEA(port, "Bad DPT sentence");
                            }
                        }
                        break;

                    case "MWV":
                        if (port == Properties.Settings.Default.AppWindSentence.InPort)
                        {
                            try
                            {
                                if (msg[2] == "R")
                                {
                                    awa = double.Parse(msg[1]);
                                    aws = double.Parse(msg[3]);
                                    switch (msg[4])
                                    {
                                        case "N":
                                            break;
                                        case "K":
                                            aws = aws / 1.852;
                                            break;
                                    }

                                    mwv_received = true;
                                    MarkDataReceivedOnNMEA(port);
                                    mwv_sentence = message;
                                    mwv_sentence_available = true;

                                }
                            }
                            catch (Exception)
                            {
                                MarkErrorOnNMEA(port, "Bad MWV sentence");
                            }

                        }
                        break;

                    case "HDG":
                        if (port == Properties.Settings.Default.HeadingSentence.InPort)
                        {
                            try
                            {
                                double mv = 0;
                                hdg = double.Parse(msg[1]);

                                if (msg[4] != "")
                                {
                                    mv = double.Parse(msg[4]);
                                    if (msg[5][0] == 'W')
                                        mv = -mv;
                                    mvar2 = mv;
                                }
                                else
                                {
                                    mvar2 = 0;
                                }

                                hdg_received = true;
                                MarkDataReceivedOnNMEA(port);
                                hdg_sentence = message;
                                hdg_sentence_available = true;
                            }
                            catch (Exception)
                            {
                                MarkErrorOnNMEA(port, "Bad HDG sentence");
                            }
                        }
                        break;

                    case "MTW":
                        if (port == Properties.Settings.Default.WaterTempSentence.InPort)
                        {
                            try
                            {
                                temp = double.Parse(msg[1]);

                                mtw_received = true;
                                MarkDataReceivedOnNMEA(port);
                                mtw_sentence = message;
                                mtw_sentence_available = true;
                            }
                            catch (Exception)
                            {
                                MarkErrorOnNMEA(port, "Bad MTW sentence");
                            }
                        }
                        break;
                }

            }
        }

        private static void MarkDataReceivedOnNMEA(int port)
        {
            if (port == 0) DataReceiverStatus1.Result = RxTxResult.Ok;
            if (port == 1) DataReceiverStatus2.Result = RxTxResult.Ok;
            if (port == 2) DataReceiverStatus3.Result = RxTxResult.Ok;
            if (port == 3) DataReceiverStatus4.Result = RxTxResult.Ok;
        }

        private static void MarkErrorOnNMEA(int port, string err)
        {
            if (port == 0) { DataReceiverStatus1.Result = RxTxResult.WrongRxData; DataReceiverStatus1.Error = err; }
            if (port == 1) { DataReceiverStatus2.Result = RxTxResult.WrongRxData; DataReceiverStatus2.Error = err; }
            if (port == 2) { DataReceiverStatus3.Result = RxTxResult.WrongRxData; DataReceiverStatus3.Error = err; }
            if (port == 3) { DataReceiverStatus4.Result = RxTxResult.WrongRxData; DataReceiverStatus4.Error = err; }
        }

        private static bool DataReceivedOnPort(int port)
        {
            return (port == 1 && Properties.Settings.Default.WaterTempSentence.InPort == 0 ||
                    port == 2 && Properties.Settings.Default.WaterTempSentence.InPort == 1 ||
                    port == 3 && Properties.Settings.Default.WaterTempSentence.InPort == 2 ||
                    port == 4 && Properties.Settings.Default.WaterTempSentence.InPort == 3);
        }

        private static void WriteSerial(int port, string msg)
        {
            try
            {
                if (port == 1) SerialPort1.WriteLine(msg);
                if (port == 2) SerialPort2.WriteLine(msg);
                if (port == 3) SerialPort3.WriteLine(msg);
                if (port == 4) SerialPort4.WriteLine(msg);
            }
            catch
            {
                MarkErrorOnNMEA(port, "Cannot TX sentence");
            }
        }

        public void BuildNMEASentences()
        {
            string message;

            // #CommRework - All primitive NMEA sentences are relayed inmeditely

            //// Build HDG Sentence ****************************************************************************************

            //if (HDT.IsValid())  // Implies MVAR is valid too
            //{

            //    string mv;

            //    if (MVAR.Val > 0)
            //        mv = "E";
            //    else
            //        mv = "W";

            //    double hdg = (HDT.Val - MVAR.Val + 360) % 360;

            //    message = "IIHDG," + hdg.ToString("0.#") + ",,," + Math.Abs(MVAR.Val).ToString("0.#") + "," + mv;

            //    int checksum = 0;

            //    foreach (char c in message)
            //        checksum ^= Convert.ToByte(c);

            //    message = "$" + message + "*" + checksum.ToString("X2") + "\r\n";

            //    if (Properties.Settings.Default.HeadingSentence.OutPort1)
            //        if (SerialPort1.IsOpen)
            //            WriteSerial(1,message);
            //    if (Properties.Settings.Default.HeadingSentence.OutPort2)
            //        if (SerialPort2.IsOpen)
            //            WriteSerial(2,message);
            //    if (Properties.Settings.Default.HeadingSentence.OutPort3)
            //        if (SerialPort3.IsOpen)
            //            WriteSerial(3,message);
            //    if (Properties.Settings.Default.HeadingSentence.OutPort4)
            //        if (SerialPort4.IsOpen)
            //            WriteSerial(4,message);
            //}

            //// Build MWV Sentence ****************************************************************************************

            //if (AWA.IsValid())
            //{

            //    message = "IIMWV," + ((AWA.Val+360)%360).ToString("0") + ",R," + AWS.Val.ToString("0.#") + ",N,A";

            //    int checksum = 0;

            //    foreach (char c in message)
            //        checksum ^= Convert.ToByte(c);

            //    message = "$" + message + "*" + checksum.ToString("X2") + "\r\n";

            //    if (Properties.Settings.Default.AppWindSentence.OutPort1)
            //        if(SerialPort1.IsOpen)
            //            WriteSerial(1,message);
            //    if (Properties.Settings.Default.AppWindSentence.OutPort2)
            //        if (SerialPort2.IsOpen)
            //            WriteSerial(2,message);
            //    if (Properties.Settings.Default.AppWindSentence.OutPort3)
            //        if (SerialPort3.IsOpen)
            //            WriteSerial(3,message);
            //    if (Properties.Settings.Default.AppWindSentence.OutPort4)
            //        if (SerialPort4.IsOpen)
            //            WriteSerial(4,message); 
            //}

            //// Build VHW Sentence ****************************************************************************************

            //if (SPD.IsValid())
            //{
            //    string hdg;
            //    if (HDT.IsValid())
            //        hdg = HDT.Val.ToString("0") + ",T,,M,";
            //    else
            //        hdg = ",T,,M,";

            //    message = "IIVHW," + hdg + SPD.Val.ToString("0.##") + ",N,,K";

            //    int checksum = 0;

            //    foreach (char c in message)
            //        checksum ^= Convert.ToByte(c);

            //    message = "$" + message + "*" + checksum.ToString("X2") + "\r\n";

            //    if (Properties.Settings.Default.HullSpeedSentence.OutPort1)
            //        if (SerialPort1.IsOpen)
            //            WriteSerial(1,message);
            //    if (Properties.Settings.Default.HullSpeedSentence.OutPort2)
            //        if (SerialPort2.IsOpen)
            //            WriteSerial(2,message);
            //    if (Properties.Settings.Default.HullSpeedSentence.OutPort3)
            //        if (SerialPort3.IsOpen)
            //            WriteSerial(3,message);
            //    if (Properties.Settings.Default.HullSpeedSentence.OutPort4)
            //        if (SerialPort4.IsOpen)
            //            WriteSerial(4,message);    
            //}

            //// Build DPT Sentence ****************************************************************************************

            //if (DPT.IsValid())
            //{

            //    message = "IIDPT,"+DPT.Val.ToString("0.#")+",0";
            //    //message = "IIDPT,25.4,0";

            //    int checksum = 0;

            //    foreach (char c in message)
            //        checksum ^= Convert.ToByte(c);

            //    message = "$" + message + "*" + checksum.ToString("X2") + "\r\n";

            //    if (Properties.Settings.Default.DepthSentence.OutPort1)
            //        if (SerialPort1.IsOpen)
            //            WriteSerial(1,message);
            //    if (Properties.Settings.Default.DepthSentence.OutPort2)
            //        if (SerialPort2.IsOpen)
            //            WriteSerial(2,message);
            //    if (Properties.Settings.Default.DepthSentence.OutPort3)
            //        if (SerialPort3.IsOpen)
            //            WriteSerial(3,message);
            //    if (Properties.Settings.Default.DepthSentence.OutPort4)
            //        if (SerialPort4.IsOpen)
            //            WriteSerial(4,message); 
            //}

            //// Build RMC Sentence ****************************************************************************************

            //if (COG.IsValid())   // Implies SOG, LAT and LON are also valid
            //{

            //    DateTime UTC = DateTime.UtcNow;

            //    string hms = UTC.Hour.ToString("00") + UTC.Minute.ToString("00") + UTC.Second.ToString("00");
            //    string date = UTC.Date.Day.ToString("00") + UTC.Date.Month.ToString("00") + UTC.Date.Year.ToString().Substring(2, 2);

            //    double deg, min;
            //    string cd;

            //    deg = Math.Abs(Math.Truncate(LAT.Val));
            //    min = (Math.Abs(LAT.Val) - deg) * 60;

            //    if (LAT.Val > 0)
            //        cd = "N";
            //    else
            //        cd = "S";

            //    string lat = deg.ToString("#")+min.ToString("00.####")+","+cd;

            //    deg = Math.Abs(Math.Truncate(LON.Val));
            //    min = (Math.Abs(LON.Val) - deg) * 60;

            //    if (LON.Val > 0)
            //        cd = "E";
            //    else
            //        cd = "W";

            //    string lon = deg.ToString("#")+min.ToString("00.####")+","+cd;

            //    if (MVAR.Val > 0)
            //        cd = "E";
            //    else
            //        cd = "W";

            //    double cog = (COG.Val + 360) % 360;

            //    message = "IIRMC," + hms + ",A," + lat + "," + lon + "," + SOG.Val.ToString("#.##") + "," + cog.ToString("0.#") + ","
            //        + date + "," + Math.Abs(MVAR.Val).ToString("0.#") + "," + cd + ",A";

            //    int checksum = 0;

            //    foreach (char c in message)
            //        checksum ^= Convert.ToByte(c);

            //    message = "$" + message + "*" + checksum.ToString("X2") + "\r\n";

            //    //message = "$GPRMC,173933,A,3430.6759,S,05828.3633,W,000.1,173.3,291220,008.1,W*68\r\n";

            //    if (Properties.Settings.Default.NavSentence.OutPort1)
            //        if (SerialPort1.IsOpen)
            //            WriteSerial(1,message);
            //    if (Properties.Settings.Default.NavSentence.OutPort2)
            //        if (SerialPort2.IsOpen)
            //            WriteSerial(2,message);
            //    if (Properties.Settings.Default.NavSentence.OutPort3)
            //        if (SerialPort3.IsOpen)
            //            WriteSerial(3,message);
            //    if (Properties.Settings.Default.NavSentence.OutPort4)
            //        if (SerialPort4.IsOpen)
            //            WriteSerial(4,message); 
            //}

            //// Build MTW Sentence ****************************************************************************************

            //if (TEMP.IsValid())
            //{

            //    message = "IIMTW," + TEMP.Val.ToString("0.#") + ",C";

            //    int checksum = 0;

            //    foreach (char c in message)
            //        checksum ^= Convert.ToByte(c);

            //    message = "$" + message + "*" + checksum.ToString("X2") + "\r\n";

            //    if (Properties.Settings.Default.WaterTempSentence.OutPort1)
            //        if (SerialPort1.IsOpen)
            //            WriteSerial(1,message);
            //    if (Properties.Settings.Default.WaterTempSentence.OutPort2)
            //        if (SerialPort2.IsOpen)
            //            WriteSerial(2,message);
            //    if (Properties.Settings.Default.WaterTempSentence.OutPort3)
            //        if (SerialPort3.IsOpen)
            //            WriteSerial(3,message);
            //    if (Properties.Settings.Default.WaterTempSentence.OutPort4)
            //        if (SerialPort4.IsOpen)
            //            WriteSerial(4,message); 
            //}

            // Build RMB Sentence ****************************************************************************************

            if (WPT.IsValid())   // Implies BRG and DST are also valid
            {
                string xte = ",,";
                string owpt = ",";

                if (XTE.IsValid())
                {
                    if (XTE.Val > 0)
                        xte = XTE.Val.ToString("0.##") + ",R,";
                    else
                        xte = Math.Abs(XTE.Val).ToString("0.##") + ",L,";
                    owpt = LWPT.FormattedValue + ",";
                }

                double brg = (BRG.Val + 360) % 360;

                message = "IIRMB,A," + xte + owpt + WPT.FormattedValue + ",,,,," + DST.Val.ToString("0.##") + "," + brg.ToString("0.#")
                    + "," + VMGWPT.Val.ToString("0.##") + ",,A";

                int checksum = 0;

                foreach (char c in message)
                    checksum ^= Convert.ToByte(c);

                message = "$" + message + "*" + checksum.ToString("X2") + "\r\n";

                rmb_sentence = message;
                rmb_sentence_available = true;
            }

            // Build PTAK4 Sentence ****************************************************************************************

            if (LINEDST.IsValid())
            {

                string message4;

                message4 = "PTAK,FFD4," + LINEDST.Val.ToString("0");

                int checksum = 0;

                foreach (char c in message4)
                    checksum ^= Convert.ToByte(c);
                message4 = "$" + message4 + "*" + checksum.ToString("X2") + "\r\n";

                if (Properties.Settings.Default.TacktickPerformanceSentence.OutPort1)
                    if (SerialPort1.IsOpen)
                        SerialPort1.WriteLine(message4);
                if (Properties.Settings.Default.TacktickPerformanceSentence.OutPort2)
                    if (SerialPort2.IsOpen)
                        SerialPort2.WriteLine(message4);
                if (Properties.Settings.Default.TacktickPerformanceSentence.OutPort3)
                    if (SerialPort3.IsOpen)
                        SerialPort3.WriteLine(message4);
                if (Properties.Settings.Default.TacktickPerformanceSentence.OutPort4)
                    if (SerialPort4.IsOpen)
                        SerialPort4.WriteLine(message4);
            }
        }

        public void BuildPTAKSentences()
        {

            // Build PTAK Sentence ****************************************************************************************

            if (PERF.IsValid())
            {

                string message = "";

                switch (ptak_cntr)
                {
                    case 0:
                        message = "PTAK,FFD1," + TGTSPD.Val.ToString("0.0");
                        break;

                    case 1:
                        message = "PTAK,FFD2," + TGTTWA.Val.ToString("0") + "@";
                        break;

                    case 2:
                        double pf = PERF.Val * 100;
                        message = "PTAK,FFD3," + pf.ToString("0");
                        break;

                    case 3:
                        message = "PTAK,FFD4," + TGTVMC.Val.ToString("0.0");
                        break;

                    case 4:
                        message = "PTAK,FFD5," + NTWA.FormattedValue + "@";
                        break;

                    case 5:
                        message = "PTAK,FFD6," + TGTCTS.Val.ToString("0") + "@";
                        break;
                }

                ptak_cntr++;
                if (ptak_cntr > 5)
                    ptak_cntr = 0;

                int checksum = 0;

                foreach (char c in message)
                    checksum ^= Convert.ToByte(c);
                message = "$" + message + "*" + checksum.ToString("X2") + "\r\n";

                ptak_sentence = message;
                ptak_sentence_available = true;

            }
        }

        public void BuildPTAKheaders()
        {
            string message1, message2, message3, message4, message5, message6;

            message1 = "PTAK,FFP1,TgtSPD, KNOTS";
            message2 = "PTAK,FFP2,TgtTWA, TRUE";
            message3 = "PTAK,FFP3,Perf, %";
            //message4 = "PTAK,FFP4,Toline,METRES";
            message4 = "PTAK,FFP4,TgtVMC, KNOTS";
            message5 = "PTAK,FFP5,NxtTWA, ";
            message6 = "PTAK,FFP6,TgtCTS, TRUE";

            int checksum = 0;

            foreach (char c in message1)
                checksum ^= Convert.ToByte(c);
            message1 = "$" + message1 + "*" + checksum.ToString("X2") + "\r\n";

            checksum = 0;
            foreach (char c in message2)
                checksum ^= Convert.ToByte(c);
            message2 = "$" + message2 + "*" + checksum.ToString("X2") + "\r\n";

            checksum = 0; 
            foreach (char c in message3)
                checksum ^= Convert.ToByte(c);
            message3 = "$" + message3 + "*" + checksum.ToString("X2") + "\r\n";

            checksum = 0; 
            foreach (char c in message4)
                checksum ^= Convert.ToByte(c);
            message4 = "$" + message4 + "*" + checksum.ToString("X2") + "\r\n";

            checksum = 0;
            foreach (char c in message5)
                checksum ^= Convert.ToByte(c);
            message5 = "$" + message5 + "*" + checksum.ToString("X2") + "\r\n";

            checksum = 0;
            foreach (char c in message6)
                checksum ^= Convert.ToByte(c);
            message6 = "$" + message6 + "*" + checksum.ToString("X2") + "\r\n";

            phdr1_sentence = message1;
            phdr2_sentence = message2;
            phdr3_sentence = message3;
            phdr4_sentence = message4;
            phdr5_sentence = message5;
            phdr6_sentence = message6;

            phdr_sentence_available = true;

        }

        public void CheckTXoverrun()
        {
            if(SerialPort1.IsOpen)
            {
                if(SerialPort1.BytesToWrite!=0)
                    borderPort1.Background = Brushes.Red;
            }

            if (SerialPort2.IsOpen)
            {
                if (SerialPort2.BytesToWrite != 0)
                    borderPort2.Background = Brushes.Red;
            }

            if (SerialPort3.IsOpen)
            {
                if (SerialPort3.BytesToWrite != 0)
                    borderPort3.Background = Brushes.Red;
            }

            if (SerialPort4.IsOpen)
            {
                if (SerialPort4.BytesToWrite != 0)
                    borderPort4.Background = Brushes.Red;
            }

        }
    }

}
