using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Globalization;
using MapControl;
using LiveCharts;
using Xceed.Wpf.Toolkit.Primitives;

namespace LionRiver
{
    [ValueConversion(typeof(double), typeof(String))]
    public class PositionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var v = value as Location;
                double deg = Math.Truncate(v.Latitude);
                double min = Math.Abs((double)v.Latitude - deg) * 60;
                string s1 = "N";
                if (deg < 0)
                    s1 = "S";
                s1 += Math.Abs(deg).ToString("00") + " " + min.ToString("00.00");

                deg = Math.Truncate(v.Longitude);
                min = Math.Abs((double)v.Longitude - deg) * 60;
                string s2 = "E";
                if (deg < 0)
                    s2 = "W";
                s2 += Math.Abs(deg).ToString("00") + " " + min.ToString("00.00");

                return s1 + "   " + s2;
            } catch
            { return ""; }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string s = (string)value;
            List<string> dm = new List<string>(s.Split(' '));
            dm.RemoveAll(r => string.IsNullOrEmpty(r));

            Location loc = new Location();

            if (dm.Count() == 4)
            {
                double deg, min;

                if (dm[0][0] == 'N')
                {
                    dm[0].TrimStart('N');
                    double.TryParse(dm[0].Substring(1), out deg);
                    double.TryParse(dm[1], out min);
                    loc.Latitude = deg + min / 60;
                }

                if (dm[0][0] == 'S')
                {
                    double.TryParse(dm[0].Substring(1), out deg);
                    double.TryParse(dm[1], out min);
                    loc.Latitude = -deg - min / 60;
                }

                if (dm[2][0] == 'E')
                {
                    dm[0].TrimStart('E');
                    double.TryParse(dm[2].Substring(1), out deg);
                    double.TryParse(dm[3], out min);
                    loc.Longitude = deg + min / 60;
                }

                if (dm[2][0] == 'W')
                {
                    dm[0].TrimStart('W');
                    double.TryParse(dm[2].Substring(1), out deg);
                    double.TryParse(dm[3], out min);
                    loc.Longitude = -deg - min / 60;
                }

                return loc;
            }
            else
                return null;
        }
    }

    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class CheckToVisConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value == true)
                return Visibility.Visible;
            else
                return Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((Visibility)value == Visibility.Visible)
                return true;
            else
                return false;
        }
    }

    public class SpeedToVisConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((double)value != 0)
                return Visibility.Visible;
            else
                return Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }

    public class PerfToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Color c = MainWindow.GetColor((double)value, MainWindow.perfGradientBrush);
            SolidColorBrush br = new SolidColorBrush(c);

            return br;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }


    public class LabelConverterTop : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double top = (double)parameter * -Math.Cos((double)value * Math.PI / 180);

            return top - 10;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }

    public class LabelConverterLeft : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double left = (double)parameter * Math.Sin((double)value * Math.PI / 180);

            return left - 8;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }

    public class TopTextboxConverter : IMultiValueConverter
    {
        object IMultiValueConverter.Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            MainWindow mWndw = values[0] as MainWindow;
            double? maxV = values[1] as double?;


            return new Thickness(0, 0, 0, 0);
        }

        object[] IMultiValueConverter.ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class CenterTextboxConverter : IMultiValueConverter
    {
        object IMultiValueConverter.Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double? width = values[0] as double?;
            double? height = values[1] as double?;

            return new Thickness(-(width ?? 0) / 2, -(height ?? 0) / 2, 0, 0);
        }

        object[] IMultiValueConverter.ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class XValuetoDateConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double? ticks = value as double?;

            if (ticks != null)
                return new DateTime((long)ticks).ToString("d-MMM-yy H:mm");
            else
                return "";
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    public class CustomSettingConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
                return true;
            else
                return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context,
           CultureInfo culture, object value)
        {
            if (value is string)
            {
                string[] v = ((string)value).Split(new char[] { ',' });
                if (v.Length == 6)
                    return new NMEASentence() { Name = v[0], InPort = int.Parse(v[1]), OutPort1 = bool.Parse(v[2]), OutPort2 = bool.Parse(v[3]), OutPort3 = bool.Parse(v[4]), OutPort4 = bool.Parse(v[5]) };
                else
                    return new NMEASentence();
            }
            return base.ConvertFrom(context, culture, value);
        }
        // Overrides the ConvertTo method of TypeConverter.
        public override object ConvertTo(ITypeDescriptorContext context,
           CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                NMEASentence ns = value as NMEASentence;
                return ns.Name + "," + ns.InPort.ToString() + "," + ns.OutPort1.ToString() + "," + ns.OutPort2.ToString() + "," + ns.OutPort3.ToString() + "," + ns.OutPort4.ToString();
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    public class BooleanToVisibilityMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility vis = (values[0] != null && values[0] != DependencyProperty.UnsetValue) ? (Visibility)(values[0]) : Visibility.Hidden;
            bool available = (values[1] != null && values[1] != DependencyProperty.UnsetValue) ? (bool)(values[1]) : false;

            if (available == true)
                return vis;
            else
                return Visibility.Hidden;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(PlotSelector), typeof(Visibility))]
    public class GroupAToVisConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var ps = (value as PlotSelector);

            if (ps != null)
            {
                if ((value as PlotSelector).Group == "A")
                    return Visibility.Visible;
                else
                    return Visibility.Hidden;
            }
            else
                return Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class GroupBToVisConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool selected = (values[0] != null && values[0] != DependencyProperty.UnsetValue) ? (bool)(values[0]) : false;
            string group = (values[1] != null && values[1] != DependencyProperty.UnsetValue) ? (values[1] as PlotSelector).Group : "";

            if (group == "B")
            {
                if (selected == true)
                    return Visibility.Visible;
                else
                    return
                        Visibility.Hidden;
            }
            else
                return Visibility.Hidden;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class DoubleToVisConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            
            if (value != null)
            {
                double val = (double)value;

                if (double.IsNaN(val))
                    return Visibility.Hidden;
                else
                    return Visibility.Visible;
            }
            else
                return Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }



}
