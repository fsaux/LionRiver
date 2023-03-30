using System;
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
using System.Media;
using System.IO;
using MapControl;

namespace LionRiver
{
    public partial class MainWindow : Window
    {
        public static double ConvertToDeg(string pos)
        {
            double deg = 0, min = 0;
            int i = pos.IndexOf('.');
            if (i != -1)
            {
                deg = double.Parse(pos.Substring(0, i - 2));
                min = double.Parse(pos.Substring(i - 2));
            }
            else
            {
                deg = double.Parse(pos.Substring(0, pos.Length - 2));
            }

            return deg + min / 60;
        }

        public static void CalcPosition(double lat, double lon, double dist, double bearing, ref double nlat, ref double nlon)
        {
            bearing = (bearing + 360) % 360;

            if (dist != 0)
            {
                double q;
                lat = lat * Math.PI / 180;
                lon = lon * Math.PI / 180;

                nlat = lat + dist / 6371000 * Math.Cos(bearing * Math.PI / 180);
                double dphi = Math.Log(Math.Tan(nlat / 2 + Math.PI / 4) / Math.Tan(lat / 2 + Math.PI / 4));
                if (bearing == 90 || bearing == 270)
                    q = Math.Cos(lat);
                else
                    q = (nlat - lat) / dphi;
                double dlon = dist / 6371000 * Math.Sin(bearing * Math.PI / 180) / q;
                nlon = (lon + dlon + Math.PI) % (2 * Math.PI) - Math.PI;

                nlat = nlat * 180 / Math.PI;
                nlon = nlon * 180 / Math.PI;
            }
            else
            {
                nlat = lat;
                nlon = lon;
            }

        }

        public static double CalcBearing(double lat1, double lon1, double lat2, double lon2)
        {
            double brg;

            lat1 = lat1 * Math.PI / 180;
            lon1 = lon1 * Math.PI / 180;
            lat2 = lat2 * Math.PI / 180;
            lon2 = lon2 * Math.PI / 180;

            double dphi = Math.Log(Math.Tan(lat2 / 2 + Math.PI / 4) / Math.Tan(lat1 / 2 + Math.PI / 4));
            brg = Math.Atan2(lon2 - lon1, dphi) * 180 / Math.PI;

            return brg;
        }

        public static double CalcDistance(double lat1, double lon1, double lat2, double lon2)
        {

            double dst, q, tc;

            lat1 = lat1 * Math.PI / 180;
            lon1 = lon1 * Math.PI / 180;
            lat2 = lat2 * Math.PI / 180;
            lon2 = lon2 * Math.PI / 180;

            double dlon_W = (lon2 - lon1) % (2 * Math.PI);
            double dlon_E = (lon1 - lon2) % (2 * Math.PI);
            double dphi = Math.Log(Math.Tan(lat2 / 2 + Math.PI / 4) / Math.Tan(lat1 / 2 + Math.PI / 4));
            if (Math.Abs(lat2 - lat1) < 1e-15)
                q = Math.Cos(lat1);
            else
                q = (lat2 - lat1) / dphi;
            if (dlon_W < dlon_E)
            {
                tc = Math.Atan2(-dlon_W, dphi) % (2 * Math.PI);
                dst = Math.Sqrt(Math.Pow(q * dlon_W, 2) + Math.Pow(lat2 - lat1, 2));
            }
            else
            {
                tc = Math.Atan2(dlon_E, dphi) % (2 * Math.PI);
                dst = Math.Sqrt(Math.Pow(q * dlon_E, 2) + Math.Pow(lat2 - lat1, 2));
            }

            return dst * 6371000;
        }

        public static double ConvertTo180(double ang)
        {
            if (ang > 180) return 360 - ang;
            else
                return ang;
        }

        public void CalcNav(DateTime now, bool bypassComm = false)
        {
            sailingMode = SailingMode.None;

            #region Primitives
            if (navSentence_received || bypassComm)
            {
                //LAT.Val = lat;
                //LON.Val = lon;
                //SOG.Val = sog;
                //COG.Val = cog;
                //LAT.SetValid();
                //LON.SetValid();
                //SOG.SetValid();
                //COG.SetValid();
                //RMC_received_Timer.Start();
            }

            if (hullSpeedSentence_received || bypassComm)
            {
                //SPD.Val = spd;
                //SPD.SetValid();
            }

            if (depthSentence_received || bypassComm)
            {
                //DPT.Val = dpt;
                //DPT.SetValid();
            }

            if (AppWindSentence_received || bypassComm)
            {
                //AWA.Val = awa;
                //AWS.Val = aws;
                //AWA.SetValid();
                //AWS.SetValid();
            }

            if (waterTempSentence_received || bypassComm)
            {
                //TEMP.Val = wtemp;
                //TEMP.SetValid();
            }

            if (headingSentence_received || bypassComm)
            {
                //double mv = Properties.Settings.Default.MagVar; //default
                //if (mvar3 != 0) mv = mvar3;                     //From SignalK
                //if (mvar2 != 0) mv = mvar2;                     //From HDG
                //if (mvar1 != 0) mv = mvar1;                     //From RMC

                //MVAR.Val = mv;
                //MVAR.SetValid();

                //if (bypassComm)
                //    mv = 0;         // heading from log file is "true heading" no need for correction

                //HDT.Val = hdg + mv;
                //HDT.SetValid();
            }

            #endregion

            #region Position, Leg bearing, distance, XTE and VMG

            if (LAT.IsValid() && LON.IsValid())
            {
                //POS.Val.Latitude = LAT.Val;
                //POS.Val.Longitude = LON.Val;
                //POS.SetValid();
            }
            else
            {
                POS.Invalidate();
            }


            if (ActiveLeg != null)
            {
                LWLAT.Val = ActiveLeg.FromLocation.Latitude;
                LWLAT.SetValid();
                LWLON.Val = ActiveLeg.FromLocation.Longitude;
                LWLON.SetValid();
                LWPT.Val.str = ActiveLeg.FromMark.Name;
                LWPT.SetValid();
            }
            else
            {
                LWLAT.Invalidate();
                LWLON.Invalidate();
                LWPT.Invalidate();
            }

            if (!bypassComm || replayLog)
            {
                if (ActiveMark != null && POS.IsValid())
                {
                    WLAT.Val = ActiveMark.Location.Latitude;
                    WLAT.SetValid();
                    WLON.Val = ActiveMark.Location.Longitude;
                    WLON.SetValid();
                    WPT.Val.str = ActiveMark.Name;
                    WPT.SetValid();
                    //BRG.Val = CalcBearing(LAT.Val, LON.Val, WLAT.Val, WLON.Val);
                    //BRG.SetValid();
                    //DST.Val = CalcDistance(LAT.Val, LON.Val, WLAT.Val, WLON.Val) / 1852;
                    //DST.SetValid();
                }
                else
                {
                    //WLAT.Invalidate();
                    //WLON.Invalidate();
                    //WPT.Invalidate();
                    //BRG.Invalidate();
                    //DST.Invalidate();
                }
            }

            if (WPT.IsValid() && LWPT.IsValid())
            {
                LEGBRG.Val = CalcBearing(LWLAT.Val, LWLON.Val, WLAT.Val, WLON.Val);
                LEGBRG.SetValid();
            }
            else
            {
                if (LEGBRG.IsValid())
                    LEGBRG.Invalidate();
            }

            if (LWPT.IsValid())
            {
                //XTE.Val = Math.Asin(Math.Sin(DST.Val * 1.852 / 6371) * Math.Sin((BRG.Val - LEGBRG.Val) * Math.PI / 180)) * 6371 / 1.852;
                //XTE.SetValid();
            }
            else
                //if (XTE.IsValid())
                //XTE.Invalidate();

            if (SOG.IsValid() && BRG.IsValid() && WPT.IsValid())
            {
                //VMGWPT.Val = SOG.Val * Math.Cos((COG.Val - BRG.Val) * Math.PI / 180);
                //VMGWPT.SetValid();
            }
            else
            {
                if (VMGWPT.IsValid())
                    VMGWPT.Invalidate();
            }
            #endregion

            #region True Wind
            if (AWA.IsValid() && SPD.IsValid())
            {
                //double Dx = AWS.Val * Math.Cos(AWA.Val * Math.PI / 180) - SPD.Val;
                //double Dy = AWS.Val * Math.Sin(AWA.Val * Math.PI / 180);
                //TWS.Val = Math.Sqrt(Dx * Dx + Dy * Dy);
                //TWS.SetValid();
                //TWA.Val = Math.Atan2(Dy, Dx) * 180 / Math.PI;
                //TWA.SetValid();
                //VMG.Val = SPD.Val * Math.Cos(TWA.Val * Math.PI / 180);
                //VMG.SetValid();

                //Set estimated saling mode in case route and/or performance data is not available
                if (Math.Abs(TWA.Val) < 55)
                    sailingMode = SailingMode.Beating;
                else
                if (Math.Abs(TWA.Val) > 130)
                    sailingMode = SailingMode.Running;
                else
                    sailingMode = SailingMode.Reaching;
            }
            else
            {
                if (TWS.IsValid())
                    TWS.Invalidate();
                if (TWA.IsValid())
                    TWA.Invalidate();
                if (VMG.IsValid())
                    VMG.Invalidate();
            }

            if (TWS.IsValid() && HDT.IsValid())
            {
                //TWD.Val = HDT.Val + TWA.Val;
                //TWD.SetValid();
            }
            else
            {
                if (TWD.IsValid())
                    TWD.Invalidate();
            }
            #endregion

            #region Leeway
            if(AWA.IsValid() && SPD.IsValid() && LWay.IsAvailable() && Properties.Settings.Default.EstimateLeeway)
            {
                //LWY.Val = LWay.Get(AWA.Val, AWS.Val, SPD.Val);
                //LWY.SetValid();
            }

            #endregion

            #region Heel
            //if (AWA.IsValid() && SPD.IsValid())
            //{
            //    double k = 7,
            //            a = 2,
            //            b = 200,
            //            c = 1.5;

            //    var awa = Math.Abs(AWA.Val);
            //    var aws = AWS.Val;

            //    HEEL.Val = k * awa * Math.Pow(aws, c) / (Math.Pow(awa, a) + b);
            //    if (HEEL.Val > 45) HEEL.Val = 45;
            //    HEEL.SetValid();
            //}
            //else
            //{
            //    if (HEEL.IsValid())
            //        HEEL.Invalidate();
            //}

            #endregion

            #region Drift
            if (SOG.IsValid() && COG.IsValid() && HDT.IsValid() && SPD.IsValid())
            {
                //double Dx = SOG.Val * Math.Cos(COG.Val * Math.PI / 180) - SPD.Val * Math.Cos(HDT.Val * Math.PI / 180);
                //double Dy = SOG.Val * Math.Sin(COG.Val * Math.PI / 180) - SPD.Val * Math.Sin(HDT.Val * Math.PI / 180);

                //if(LWY.IsValid())
                //{
                //    double lwy;
                //    if (AWA.Val < 0)
                //        lwy = -LWY.Val;
                //    else
                //        lwy = LWY.Val;

                //    double lm = SPD.Val * Math.Tan(lwy * Math.PI / 180);
                //    double la = HDT.Val - 90;
                //    double lx = lm * Math.Cos(la * Math.PI / 180);
                //    double ly = lm * Math.Sin(la * Math.PI / 180);

                //    double ang = Math.Atan2(ly, lx) * 180 / Math.PI;

                //    Dx -= lx;
                //    Dy -= ly;
                //}

                //DRIFT.Val = Math.Sqrt(Dx * Dx + Dy * Dy);
                //DRIFT.SetValid();
                //SET.Val = Math.Atan2(Dy, Dx) * 180 / Math.PI;
                //SET.SetValid();
            }
            else
            {
                if (DRIFT.IsValid())
                    DRIFT.Invalidate();
                if (SET.IsValid())
                    SET.Invalidate();
            }
            #endregion

            #region Performance
            if (TWA.IsValid() && SPD.IsValid() && NavPolar.IsLoaded && BRG.IsValid())
            {
                double Angle = Math.Abs(TWD.Val - BRG.Val + 360) % 360;
                if (Angle > 180) Angle = 360 - Angle;

                //PolarPoint pb = NavPolar.GetBeatTargeInterpolated(TWS.Val);
                //PolarPoint pr = NavPolar.GetRunTargetInterpolated(TWS.Val);

                if (Angle <= (65)) // Beating
                {
                    //    TGTSPD.Val = pb.SPD;
                    //    TGTSPD.SetValid();
                    //    TGTTWA.Val = pb.TWA;
                    //    TGTTWA.SetValid();
                    //    var z = (pb.SPD * Math.Cos(pb.TWA * Math.PI / 180));
                    //    if (z != 0)
                    //    {
                    //        PERF.Val = VMG.Val / z;
                    //        PERF.SetValid();
                    //    }
                    //    else
                    //        PERF.Invalidate();

                    sailingMode = SailingMode.Beating;
                }

                if (Angle < (130) && Angle > (65)) // Reaching
                    {
                        //    TGTSPD.Val = NavPolar.GetTargeInterpolated(Math.Abs(TWA.Val), TWS.Val);
                        //    TGTSPD.SetValid();
                        //    TGTTWA.Val = Math.Abs(TWA.Val);
                        //    TGTTWA.SetValid();
                        //    var z = TGTSPD.Val;
                        //    if (z != 0)
                        //    {
                        //        PERF.Val = Math.Abs(SPD.Val * Math.Cos((COG.Val - BRG.Val) * Math.PI / 180) / z);
                        //        PERF.SetValid();
                        //    }
                        //    else
                        //        PERF.Invalidate();

                        sailingMode = SailingMode.Reaching;
                    }

                if (Angle >= (130)) // Running
                {
                    //    TGTSPD.Val = pr.SPD;
                    //    TGTSPD.SetValid();
                    //    TGTTWA.Val = pr.TWA;
                    //    TGTTWA.SetValid();
                    //    var z = (pr.SPD * Math.Cos(pr.TWA * Math.PI / 180));
                    //    if (z != 0)
                    //    {
                    //        PERF.Val = VMG.Val / z;
                    //        PERF.SetValid();
                    //    }
                    //    else
                    //        PERF.Invalidate();

                    sailingMode = SailingMode.Running;
                }

            }
                else
            {
                if (TGTSPD.IsValid())
                    TGTSPD.Invalidate();
                if (TGTTWA.IsValid())
                    TGTTWA.Invalidate();
                if (PERF.IsValid())
                    PERF.Invalidate();
            }
            #endregion

            #region Line
            if (p1_set && p2_set && LAT.IsValid() && HDT.IsValid())
            {
                //double p3_lat = LAT.Val, p3_lon = LON.Val;

                //if (Properties.Settings.Default.GPSoffsetToBow != 0)
                //    CalcPosition(LAT.Val, LON.Val, Properties.Settings.Default.GPSoffsetToBow, HDT.Val, ref p3_lat, ref p3_lon);
                //double brg32 = CalcBearing(p3_lat, p3_lon, p2_lat, p2_lon);
                //double dst32 = CalcDistance(p3_lat, p3_lon, p2_lat, p2_lon);

                //LINEDST.Val = dst32 * Math.Sin((linebrg - brg32) * Math.PI / 180);
                //LINEDST.SetValid();
            }
            else
            {
                if (LINEDST.IsValid())
                    LINEDST.Invalidate();
            }
            #endregion

            #region Route nav

            if (ActiveMark != null && DST.IsValid() && !ManOverBoard)
            {
                if (DST.Val <= Properties.Settings.Default.WptProximity && ActiveMark != MOB)
                {
                    (new SoundPlayer(@".\Sounds\BELL7.WAV")).PlaySync();
                    if (ActiveLeg != null)
                    {
                        if (ActiveLeg.NextLeg != null)
                        {
                            ActiveLeg = ActiveLeg.NextLeg;
                            ActiveMark = ActiveLeg.ToMark;
                            DST.Invalidate();
                            WriteSKDeltaActiveWpt();
                        }
                        else
                        {
                            ActiveMark = null;
                            ActiveLeg = null;
                            ActiveRoute = null;
                            WLAT.Invalidate();
                            WLON.Invalidate();
                            LWLAT.Invalidate();
                            LWLON.Invalidate();
                            DST.Invalidate();
                            WriteSKDeltaActiveWpt();
                        }
                    }
                    else
                    {
                        ActiveMark = null;
                        WLAT.Invalidate();
                        WLON.Invalidate();
                        DST.Invalidate();
                        WriteSKDeltaActiveWpt();
                    }
                }
            }


            if (ActiveRoute != null)
            {
                if (ActiveLeg.NextLeg != null && TWD.IsValid())
                {
                    NTWA.Val = TWD.Val - ActiveLeg.NextLeg.Bearing;
                    NTWA.SetValid();
                }
                else
                {
                    NTWA.Invalidate();
                }
            }

            #endregion

            #region Laylines

            insideCourse = false; // Need to determine later

            if (DRIFT.IsValid() && PERF.IsValid() && TWD.IsValid())
            {
                //double ttwa = TGTTWA.Val;
                //double tgtlwy = 0;

                //if(LWY.IsValid())
                //{
                //    double awx = TWS.Val * Math.Cos(ttwa * Math.PI / 180) + TGTSPD.Val;
                //    double awy = TWS.Val * Math.Sin(ttwa * Math.PI / 180);
                //    double tgtawa = Math.Atan2(awy, awx) * 180 / Math.PI;
                //    double tgtaws = Math.Sqrt(awx * awx + awy * awy);
                //    tgtlwy = LWay.Get(tgtawa, tgtaws, TGTSPD.Val);
                //}

                //ttwa += tgtlwy;
                //if (ttwa > 180) ttwa = 180;

                //double relset = SET.Val - TWD.Val;
                //double dxs = TGTSPD.Val * Math.Cos(ttwa * Math.PI / 180) + DRIFT.Val * Math.Cos(relset * Math.PI / 180);
                //double dys = TGTSPD.Val * Math.Sin(ttwa * Math.PI / 180) + DRIFT.Val * Math.Sin(relset * Math.PI / 180);

                //TGTCOGp.Val = Math.Atan2(dys, dxs) * 180 / Math.PI + TWD.Val;
                //TGTCOGp.SetValid();
                //TGTSOGp.Val = Math.Sqrt(dxs * dxs + dys * dys);
                //TGTSOGp.SetValid();

                //double dxp = TGTSPD.Val * Math.Cos(-ttwa * Math.PI / 180) + DRIFT.Val * Math.Cos(relset * Math.PI / 180);
                //double dyp = TGTSPD.Val * Math.Sin(-ttwa * Math.PI / 180) + DRIFT.Val * Math.Sin(relset * Math.PI / 180);

                //TGTCOGs.Val = Math.Atan2(dyp, dxp) * 180 / Math.PI + TWD.Val;
                //TGTCOGs.SetValid();
                //TGTSOGs.Val = Math.Sqrt(dxp * dxp + dyp * dyp);
                //TGTSOGs.SetValid();

                // Determine if sailing inside course +/- 0 degrees
                
                double tgtcogs, tgtcogp;
                if (TWA.Val < 0)
                {
                    tgtcogs = TGTCOG.Val;
                    tgtcogp = TGTCOGo.Val;
                }
                else
                {
                    tgtcogp = TGTCOG.Val;
                    tgtcogs = TGTCOGo.Val;
                }



                double a1 = (BRG.Val - tgtcogp - 0 + 360) % 360;
                double a2 = (tgtcogs - 0 - tgtcogp + 360) % 360;

                switch (sailingMode)
                {
                    case SailingMode.Beating:

                        if (a1 < a2)
                            insideCourse = true;
                        break;

                    case SailingMode.Running:

                        if (a2 < a1)
                            insideCourse = true;
                        break;

                }

                // Calculate Layline hit points

                if (ActiveMark != null)
                {
                    //double alpha = (TGTCOGp.Val - BRG.Val + 360) % 360;
                    //double beta = (BRG.Val - TGTCOGs.Val + 360) % 360;

                    //double dist_s, dist_p;

                    //if (alpha == 0)
                    //{
                    //    dist_p = DST.Val;
                    //    dist_s = 0;
                    //}
                    //else
                    //{
                    //    if (beta == 0)
                    //    {
                    //        dist_s = DST.Val;
                    //        dist_p = 0;
                    //    }
                    //    else
                    //    {
                    //        dist_p = DST.Val * Math.Sin(beta * Math.PI / 180) /
                    //            (Math.Sin(alpha * Math.PI / 180) * Math.Cos(beta * Math.PI / 180) +
                    //            Math.Cos(alpha * Math.PI / 180) * Math.Sin(beta * Math.PI / 180));
                    //        dist_s = DST.Val * Math.Sin(alpha * Math.PI / 180) /
                    //            (Math.Sin(alpha * Math.PI / 180) * Math.Cos(beta * Math.PI / 180) +
                    //            Math.Cos(alpha * Math.PI / 180) * Math.Sin(beta * Math.PI / 180));
                    //    }
                    //}
                    //DSTLYLp.Val = dist_p * 1852;
                    //DSTLYLp.SetValid();
                    //DSTLYLs.Val = dist_s * 1852;
                    //DSTLYLs.SetValid();

                    //double xx = DSTLYLp.Val / TGTSOGp.Val * 3600 / 1852;
                    //if (xx > TimeSpan.MaxValue.TotalHours) xx = TimeSpan.MaxValue.TotalHours - 1;
                    //if (xx < TimeSpan.MinValue.TotalHours) xx = TimeSpan.MinValue.TotalHours + 1;
                    //TTGLYLp.Val = TimeSpan.FromSeconds(xx);
                    //TTGLYLp.SetValid();

                    //xx = DSTLYLs.Val / TGTSOGs.Val * 3600 / 1852;
                    //if (xx > TimeSpan.MaxValue.TotalHours) xx = TimeSpan.MaxValue.TotalHours - 1;
                    //if (xx < TimeSpan.MinValue.TotalHours) xx = TimeSpan.MinValue.TotalHours + 1;
                    //TTGLYLs.Val = TimeSpan.FromSeconds(xx);
                    //TTGLYLs.SetValid();
                }
            }
            else
            {
                if (TGTCOG.IsValid())
                    TGTCOG.Invalidate();
                if (TGTSOG.IsValid())
                    TGTSOG.Invalidate();
                if (TGTCOGo.IsValid())
                    TGTCOGo.Invalidate();
                if (TGTSOGo.IsValid())
                    TGTSOGo.Invalidate();
                if (DSTLYL.IsValid())
                    DSTLYL.Invalidate();
                if (DSTLYLo.IsValid())
                    DSTLYLo.Invalidate();
            }

            #endregion

            if (replayLog == true)
            {
                if (deltaLog == TimeSpan.Zero)
                    //deltaLog = now - new DateTime(2020, 03, 15);
                    deltaLog = now - DateTime.Now;


                now = now - deltaLog;

            }

            PushToLogDB(now);
        }

        private void CalcNavFromFile(StreamReader tempfile, DateTime starttime)
        {
            string rl = tempfile.ReadLine();
            string[] str = null;

            if (rl != null)
                str = rl.Split(',');

            DateTime dt;

            try
            {
                if (DateTime.TryParse(str[0], out dt))
                {
                    lat = double.Parse(str[1]);
                    lon = double.Parse(str[2]);
                    cog = double.Parse(str[3]);
                    hdg = double.Parse(str[4]);
                    sog = double.Parse(str[5]);
                    spd = double.Parse(str[6]);
                    awa = double.Parse(str[7]);
                    aws = double.Parse(str[8]);
                    dpt = double.Parse(str[9]);
                    wtemp = double.Parse(str[10]);

                    if (dt.ToLocalTime() > starttime)
                        CalcNav(dt.ToLocalTime(), true);
                }

            }
            catch
            { }
        }

        public void CalcMeasure()
        {
            if (measureRange.FromLocation != null && measureRange.ToLocation != null)
            {
                double lat1 = measureRange.FromLocation.Latitude;
                double lon1 = measureRange.FromLocation.Longitude;
                double lat2 = measureRange.ToLocation.Latitude;
                double lon2 = measureRange.ToLocation.Longitude;

                measureResult.DST = CalcDistance(lat1, lon1, lat2, lon2) / 1852;
                double brg = CalcBearing(lat1, lon1, lat2, lon2);
                measureResult.BRG = (brg + 360) % 360;

                if (TWS.IsValid())
                    measureResult.TWA = ConvertTo180((TWD.Val - brg + 360) % 360);

                double vmc = 0;
                PolarPoint p = new PolarPoint();

                if (SOG.IsValid())
                    vmc = SOG.Val;

                if (TWD.IsValid() && SPD.IsValid() && NavPolar.IsLoaded)
                {
                    double Angle = Math.Abs(measureResult.TWA % 360);
                    if (Angle > 180) Angle = 360 - Angle;
                    if (Angle < 50)
                    {
                        p = NavPolar.GetBeatTargeInterpolated(TWS.Val);
                        vmc = p.SPD * Math.Cos(p.TWA * Math.PI / 180);
                    }
                    else
                        if (Angle > 140)
                    {
                        p = NavPolar.GetRunTargetInterpolated(TWS.Val);
                        vmc = -p.SPD * Math.Cos(p.TWA * Math.PI / 180);
                    }
                    else
                        vmc = NavPolar.GetTargeInterpolated(Angle, TWS.Val);
                }

                if (vmc != 0)
                {
                    double xx = measureResult.DST / vmc;
                    if (xx > TimeSpan.MaxValue.TotalHours) xx = TimeSpan.MaxValue.TotalHours-1;
                    if (xx < TimeSpan.MinValue.TotalHours) xx = TimeSpan.MinValue.TotalHours+1;
                    measureResult.TTG = TimeSpan.FromHours(xx);
                }
                else
                    measureResult.TTG = TimeSpan.FromHours(0);


            }
        }

        public static void CalcLongNav(DateTime now)
        {

            if (TWA.IsValid() && BRG.IsValid() && DRIFT.IsValid() && NavPolar.IsLoaded)
            {
                PolarPoint p = NavPolar.GetTargetVMC(TWS.Val, TWD.Val, BRG.Val, DRIFT.Val, SET.Val);
                TGTVMC.Val = p.SPD;
                TGTVMC.SetValid();
                TGTCTS.Val = TWD.Val + p.TWA;
                TGTCTS.SetValid();
            }
        }

        public static void CalcRouteData()
        {
            //if (SelectedRoute != null)
            //{
            //    if (SelectedRoute.Legs.Count > 0)
            //    {
            //        Waypoint w0 = SelectedRoute.Legs[0].ToWpt;
            //        SelectedRoute.Legs[0].FromWpt = null;
            //        SelectedRoute.Legs[0].AccDist = "";
            //        SelectedRoute.Legs[0].Distance = "";
            //        SelectedRoute.Legs[0].Bearing = "";
            //        SelectedRoute.Legs[0].TWA = "";
            //        SelectedRoute.Legs[0].ETA = "";

            //        double accu = 0;

            //        for (int i = 1; i < SelectedRoute.Legs.Count; i++)
            //        {
            //            Waypoint w1 = SelectedRoute.Legs[i].ToWpt;
            //            SelectedRoute.Legs[i].FromWpt = w0;
            //            double distance = CalcDistance(w0.Lat, w0.Lon, w1.Lat, w1.Lon) / 1852;
            //            accu += distance;
            //            SelectedRoute.Legs[i].AccDist = accu.ToString("#.##");
            //            SelectedRoute.Legs[i].Distance = distance.ToString("#.##");
            //            double bearing = (CalcBearing(w0.Lat, w0.Lon, w1.Lat, w1.Lon) + 360) % 360;
            //            SelectedRoute.Legs[i].Bearing = bearing.ToString("#");
            //            if (TWD.IsValid())
            //            {
            //                double twangle = ((TWD.LongAverage - bearing) + 360) % 360;
            //                if (twangle > 180)
            //                    twangle = twangle - 360;
            //                SelectedRoute.Legs[i].TWA = twangle.ToString("#");
            //            }
            //            w0 = w1;
            //        }
            //    }
            //}
        }

        public void PushToLogDB(DateTime dt)
        {
            if (POS.IsValid())
            {
                POS.PushToBuffer(POS.Val, dt, 0);
                COG.PushToBuffer(COG.Val, dt, 0);
                SOG.PushToBuffer(SOG.Val, dt, 0);
                HDT.PushToBuffer(HDT.Val, dt, 0);
                TWD.PushToBuffer(TWD.Val, dt, 0);
                PERF.PushToBuffer(PERF.Val, dt, 0);
                DPT.PushToBuffer(DPT.Val, dt, 0);
                TWS.PushToBuffer(TWS.Val, dt, 0);
                DRIFT.PushToBuffer(DRIFT.Val, dt, 0);
                SET.PushToBuffer(SET.Val, dt, 0);
                SPD.PushToBuffer(SPD.Val, dt, 0);


                using (var context = new LionRiverDBContext())
                {

                    for (int i = 0; i < Inst.MaxBuffers ; i++)
                    {
                        if (POS.AvgBufferDataAvailable(i))
                        {
                            if (POS.GetLastVal(i) != null)
                            {
                                var log = new Log()
                                {
                                    timestamp = POS.GetLastVal(i).Time,
                                    level = i,
                                    LAT = POS.GetLastVal(i).Val.Latitude,
                                    LON = POS.GetLastVal(i).Val.Longitude,
                                    COG = COG.GetLastVal(i).Val,
                                    SOG = SOG.GetLastVal(i).Val,
                                    HDT = HDT.GetLastVal(i).Val,
                                    TWD = TWD.GetLastVal(i).Val,
                                    PERF = PERF.GetLastVal(i).Val,
                                    DPT = DPT.GetLastVal(i).Val,
                                    TWS = TWS.GetLastVal(i).Val,
                                    DRIFT = DRIFT.GetLastVal(i).Val,
                                    SET = SET.GetLastVal(i).Val,
                                    SPD = SPD.GetLastVal(i).Val
                                };
                                context.Logs.Add(log);

                                if (i == NavPlotModel.Resolution)
                                    newTrackPositionAvailable = true;

                                POS.ClearAvgBufferDataAvailable(i);
                                COG.ClearAvgBufferDataAvailable(i);
                                SOG.ClearAvgBufferDataAvailable(i);
                                HDT.ClearAvgBufferDataAvailable(i);
                                TWD.ClearAvgBufferDataAvailable(i);
                                PERF.ClearAvgBufferDataAvailable(i);
                                DPT.ClearAvgBufferDataAvailable(i);
                                TWS.ClearAvgBufferDataAvailable(i);
                                DRIFT.ClearAvgBufferDataAvailable(i);
                                SET.ClearAvgBufferDataAvailable(i);
                                SPD.ClearAvgBufferDataAvailable(i);
                            }
                        }
                    }
                    context.SaveChanges();
                }
            }
        }
    }
}




