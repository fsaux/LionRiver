using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using MapControl;
using CircularBuffer;

namespace LionRiver
{
    public class IData<T>
    {
        public DateTime Time { get; set; }
        public T Val { get; set; }
    }

    public class Inst
    {
        #region Constants
        public const int MaxBuffers = 6;
        public const int BufSize = 360;
        public const int ZoomFactor = 5; // Each buffer will average over ZoomFactor ticks on the previous one 
        public const int BufOneSec = 0;
        public const int BufFiveSec = 1;
        public const int BufHalfMin = 2;
        public const int BufTwoMin = 3;
        public const int BufTenMin = 4;
        public const int BufOneHr = 5;
        #endregion
    }

    public abstract class Instrument
    {
        public string DisplayName { get; set; }

    }

    public abstract class Instrument<T> : Instrument, INotifyPropertyChanged where T : new()
    {

        public event PropertyChangedEventHandler PropertyChanged;

        protected CircularBuffer<IData<T>>[] buffer = new CircularBuffer<IData<T>>[Inst.MaxBuffers];


        protected IData<T>[] LastVal = new IData<T>[Inst.MaxBuffers];    
        protected bool[] dataAvailable = new bool[Inst.MaxBuffers];

        protected List<IData<T>>[] avgBuffer = new List<IData<T>>[Inst.MaxBuffers];
        protected CircularBuffer<T> dampBuffer;
        protected bool _valid;
 
        public T Val { get; set; }
        public string Units { get; set; }
        public string FormatString { get; set; }

        #region Constructors
        public Instrument(string dn = "dummy", string un = "dummy", string formatString = "#.#" ,int DampingWindow = 1,bool logged=true)
        {
            this.Val = new T();
            this.DisplayName = dn;
            this.FormatString = formatString;
            this.Units = un;
            this.dampBuffer = new CircularBuffer<T>(DampingWindow, true);
            for (int i = 0; i < Inst.MaxBuffers; i++)
            {
                buffer[i] = new CircularBuffer<IData<T>>(Inst.BufSize, true);
                avgBuffer[i] = new List<IData<T>>();
                dataAvailable[i] = false;
            }
            if (logged)
                MainWindow.LoggedInstrumentList.Add(this);
        }

        #endregion

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        public virtual void SetValid(DateTime dt)
        {
            dampBuffer.Put(Val);
            Val = CalculateAverage(dampBuffer);
            //PushToBuffer(Val, dt, 0);
            _valid = true;
            OnPropertyChanged("FormattedValue");
        }

        public virtual void SetValid()
        {
            SetValid(System.DateTime.Now);
        }

        public void Invalidate()
        {
            _valid = false;
            OnPropertyChanged("FormattedValue");
        }

        public bool IsValid()
        {
            return _valid;
        }

        public CircularBuffer<IData<T>> GetBuffer(int level)
        {
            return buffer[level];
        }

        public CircularBuffer<IData<T>>[] GetBuffer()
        {
            return buffer;
        }

        public IData<T> GetLastVal(int level)
        {
            return this.LastVal[level];
        }

        public List<IData<T>> GetAvgBuffer(int level)
        {
            return avgBuffer[level];
        }

        public List<IData<T>>[] GetAvgBuffer()
        {
            return avgBuffer;
        }

        public bool AvgBufferDataAvailable(int bufnr)
        {
            return dataAvailable[bufnr];
        }

        public void SetAvgBufferDataAvailable(int bufnr)
        {
            dataAvailable[bufnr] = true;
        }

        public void ClearAvgBufferDataAvailable(int bufnr)
        {
            dataAvailable[bufnr] = false;
        }

        public abstract string FormattedValue { get; }

        protected abstract T CalculateAverage(ICollection<T> items);

        protected DateTime CalculateTimeAverage(ICollection<DateTime> items)
        {
            var averageTicks = (long)items.Select(d => d.Ticks).Average();
            return new DateTime(averageTicks);
        }

        public void PushToBuffer(T v, DateTime dt, int bufnr)
        {
            if (bufnr < Inst.MaxBuffers)
            {
                T x = v;

                LastVal[bufnr] = new IData<T> { Val = x, Time = dt };
                if (bufnr == 0)
                    this.SetAvgBufferDataAvailable(0); // Level 0 is allways new data

                avgBuffer[bufnr].Add(new IData<T> { Val = x, Time = dt });
                if (avgBuffer[bufnr].Count == Inst.ZoomFactor && bufnr < Inst.MaxBuffers - 1)
                {
                    var avgValList = (from insdat in avgBuffer[bufnr] select insdat.Val).ToList();
                    T avg = CalculateAverage(avgValList);

                    var avgTimeList = (from insdat in avgBuffer[bufnr] select insdat.Time).ToList();
                    DateTime avgdt = CalculateTimeAverage(avgTimeList);

                    PushToBuffer(avg, avgdt, bufnr + 1);
                    this.SetAvgBufferDataAvailable(bufnr + 1);
                    avgBuffer[bufnr].Clear();
                }
            }
        }
    }


    public class LinearInstrument : Instrument<double>
    {
        public LinearInstrument(string dn = "dummy", string un = "dummy", string formatString = "#.#" ,int DampingWindow = 1, bool logged = true)
            : base(dn, un, formatString, DampingWindow, logged)
        {
        }

        protected override double CalculateAverage(ICollection<double> items)
        {
            return items.Average();
        }

        public override string FormattedValue
        {
            get
            {
                if (_valid)
                    return Val.ToString(FormatString);
                else
                    return "";
            }
        }

    }

    public class LinearInstrumentShort : LinearInstrument
    {
        public LinearInstrumentShort(string dn = "dummy", string un = "dummy", string formatString = "#", int DampingWindow = 1, bool logged = true)
            : base(dn, un, formatString, DampingWindow, logged)
        {
        }

        public override string FormattedValue
        {
            get
            {
                if (_valid)
                    return Val.ToString(FormatString);
                else
                    return "";
            }
        }

    }

    class AngularInstrument : Instrument<double>
    {
        public AngularInstrument(string dn = "dummy", string un = "dummy", string formatString = "000", int DampingWindow = 1, bool logged = true)
            : base(dn, un, formatString, DampingWindow, logged)
        {
        }

        protected override double CalculateAverage(ICollection<double> items)
        {
            double sumcos = 0, sumsin = 0;
            foreach (double v in items)
            {
                sumcos += Math.Cos(v * Math.PI / 180);
                sumsin += Math.Sin(v * Math.PI / 180);
            }
            return Math.Atan2(sumsin, sumcos) * 180 / Math.PI;
        }

        public override string FormattedValue
        {
            get
            {
                if (_valid)
                {
                    double _val = (Val + 360) % 360;
                    return _val.ToString(FormatString);
                }
                else
                    return "";
            }
        }
    }

    class AngularInstrumentAbs : AngularInstrument
    {
        public AngularInstrumentAbs(string dn = "dummy", string un = "dummy", string formatString = "000", int DampingWindow = 1, bool logged = true)
            : base(dn, un, formatString, DampingWindow, logged)
        {
        }

        public override string FormattedValue
        {
            get
            {
                if (_valid)
                {
                    double _val = (Val + 360) % 360;
                    return _val.ToString(FormatString);
                }
                else
                    return "";
            }
        }
    }

    class AngularInstrumentRel : AngularInstrument
    {
        public AngularInstrumentRel(string dn = "dummy", string un = "dummy", string formatString = "#", int DampingWindow = 1, bool logged = true)
            : base(dn, un, formatString, DampingWindow, logged)
        {
        }

        public override string FormattedValue
        {
            get
            {
                if (_valid)
                {
                    double _val = (Val + 360) % 360;
                    if (_val > 180) _val = _val - 360;
                    return _val.ToString(FormatString);
                }
                else
                    return "";
            }
        }

    }

    class LatitudeInstrument : AngularInstrument
    {
        public LatitudeInstrument(string dn = "dummy", string un = "dummy", string formatString = "", int DampingWindow = 1, bool logged = true)
            : base(dn, un, formatString,DampingWindow, logged)
        {
        }

        public override string FormattedValue
        {
            get
            {
                if (_valid)
                {
                    double deg, min;
                    string d, m, c;

                    deg = Math.Abs(Math.Truncate(Val));
                    min = (Math.Abs(Val) - deg) * 60;

                    d = deg.ToString();
                    m = min.ToString("0.00");
                    if (Val > 0)
                        c = "N";
                    else
                        c = "S";

                    return d + "° " + m + "' " + c;
                }
                else
                    return "";
            }
        }
    }

    class LongitudeInstrument : AngularInstrument
    {
        public LongitudeInstrument(string dn = "dummy", string un = "dummy", string formatString = "", int DampingWindow = 1, bool logged = true)
            : base(dn, un, formatString, DampingWindow, logged)
        {
        }

        public override string FormattedValue
        {
            get
            {
                if (_valid)
                {
                    double deg, min;
                    string d, m, c;

                    deg = Math.Abs(Math.Truncate(Val));
                    min = (Math.Abs(Val) - deg) * 60;

                    d = deg.ToString();
                    m = min.ToString("0.00");
                    if (Val > 0)
                        c = "E";
                    else
                        c = "W";

                    return d + "° " + m + "' " + c;
                }
                else
                    return "";
            }
        }
    }

    class PercentInstrument : LinearInstrument
    {
        public PercentInstrument(string dn = "dummy", string un = "dummy", string formatString = "#%", int DampingWindow = 1, bool logged = true)
            : base(dn, un, formatString, DampingWindow, logged)
        {
        }

        public override string FormattedValue
        {
            get
            {
                if (_valid)
                    return (Val * 100).ToString(FormatString);
                else
                    return "";
            }
        }
    }

    class TimeSpanInstrument : Instrument<TimeSpan>
    {
        public TimeSpanInstrument(string dn = "dummy", string un = "dummy", string formatString = "", int DampingWindow = 1, bool logged = true)
            : base(dn, un, formatString, DampingWindow, logged)
        {
        }

        protected override TimeSpan CalculateAverage(ICollection<TimeSpan> items)
        {
            IEnumerable<double> x =
                from i in items
                select i.TotalSeconds;

            return TimeSpan.FromSeconds(x.Average());
        }
        public override string FormattedValue
        {
            get
            {
                if (_valid)
                    return Val.ToString(@"hh\:mm\:ss");
                else
                    return "";
            }
        }
    }


    class String2
    {
        public String2(string s)
        {
            str = s;
        }

        public String2() : this(String.Empty) { }

        public string str { get; set; }

    }

    class WaypointInstrument : Instrument<String2>
    {
        public WaypointInstrument(string dn = "dummy", string un = "dummy", string formatString = "", int DampingWindow = 1, bool logged = true)
            : base(dn, un, formatString, DampingWindow, logged)
        {
        }

        protected override String2 CalculateAverage(ICollection<String2> items)
        {
            return Val;
        }

        public override string FormattedValue
        {
            get
            {
                if (_valid)
                    return Val.str;
                else
                    return "";
            }

        }

    }

    class PositionInstrument : Instrument<Location>
    {
        public PositionInstrument(string dn = "dummy", string un = "dummy", string formatString = "", int DampingWindow = 1, bool logged = true)
            : base(dn, un, formatString, DampingWindow, logged)
        {
        }

        protected override Location CalculateAverage(ICollection<Location> items)
        {
            double latsumcos = 0, latsumsin = 0;
            double lonsumcos = 0, lonsumsin = 0;

            foreach (Location v in items)
            {
                latsumcos += Math.Cos(v.Latitude * Math.PI / 180);
                latsumsin += Math.Sin(v.Latitude * Math.PI / 180);
                lonsumcos += Math.Cos(v.Longitude * Math.PI / 180);
                lonsumsin += Math.Sin(v.Longitude * Math.PI / 180);
            }

            double lat = Math.Atan2(latsumsin, latsumcos) * 180 / Math.PI;
            double lon = Math.Atan2(lonsumsin, lonsumcos) * 180 / Math.PI;

            return new Location { Latitude = lat, Longitude = lon };

        }

        public override string FormattedValue
        {
            get
            {
                PositionConverter pc = new PositionConverter();
                return (string)pc.Convert(Val, typeof(string), null, null);
            }
        }

    }


}
