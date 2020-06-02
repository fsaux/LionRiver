using LionRiver.UserControls;
using LiveCharts;
using LiveCharts.Configurations;
using LiveCharts.Wpf;
using MapControl;
using Newtonsoft.Json;
using OSGeo.GDAL;
using OSGeo.OGR;
using OSGeo.OSR;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace LionRiver
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        #region MainWindow Properties (Globals)

        TimeSpan deltaLog;

        #region Comm

        static DataReceiverStatus DataReceiverStatus1 = new DataReceiverStatus();
        static DataReceiverStatus DataReceiverStatus2 = new DataReceiverStatus();
        static DataReceiverStatus DataReceiverStatus3 = new DataReceiverStatus();
        static DataReceiverStatus DataReceiverStatus4 = new DataReceiverStatus();

        //Primitives
        static double lat, lon, sog, cog, mvar1, mvar2, spd, dpt, awa, aws, hdg, temp;
        static bool rmc_received = false;
        static bool mwv_received = false;
        static bool vhw_received = false;
        static bool dpt_received = false;
        static bool hdg_received = false;
        static bool mtw_received = false;

        #endregion

        #region Instruments

        //Dictionary<string, object> InstrumentList = new Dictionary<string, object>
        Dictionary<string, UserControl> InstrumentDisplays = new Dictionary<string, UserControl>
        {
            { "COG", new InstrumentDisplay1() },
            { "SOG", new InstrumentDisplay1() },
            { "HDT", new InstrumentDisplay1() },
            { "SPD", new InstrumentDisplay1() },
            { "TEMP", new InstrumentDisplay1() },
            { "DPT", new InstrumentDisplay1() },
            { "AWA", new InstrumentDisplay1() },
            { "TWA", new InstrumentDisplay1() },
            { "TWS", new InstrumentDisplay1() },
            { "TWD", new InstrumentDisplay1() },
            { "BRG", new InstrumentDisplay1() },
            { "WPT", new InstrumentDisplay2() },
            { "DST", new InstrumentDisplay1() },
            { "XTE", new InstrumentDisplay1() },
            { "VMG", new InstrumentDisplay1() },
            { "VMGWPT", new InstrumentDisplay1() },
            { "NTWA", new InstrumentDisplay1() },
            { "DRIFT", new InstrumentDisplay1() },
            { "SET", new InstrumentDisplay1() },
            { "TGTSPD", new InstrumentDisplay1() },
            { "TGTTWA", new InstrumentDisplay1() },
            { "PERF", new InstrumentDisplay1() },
            { "TGTVMC", new InstrumentDisplay1() },
            { "TGTCTS", new InstrumentDisplay1() }
        };

        public FrameworkElement MovingInstrument;
        public FrameworkElement hitTestUI;


        public static List<Instrument> LoggedInstrumentList = new List<Instrument>();

        static AngularInstrumentAbs COG = new AngularInstrumentAbs("COG", "°T", "000", 10);
        static LinearInstrument SOG = new LinearInstrument("SOG", "Kn", "0.0");
        static LatitudeInstrument LAT = new LatitudeInstrument("Lat", "");
        static LongitudeInstrument LON = new LongitudeInstrument("Lon", "");
        static PositionInstrument POS = new PositionInstrument("Position", "");
        static AngularInstrumentRel MVAR = new AngularInstrumentRel("MVar", "°", "#", 1, false);
        static LinearInstrument SPD = new LinearInstrument("SPD", "Kn", "0.0");
        static LinearInstrument TEMP = new LinearInstrument("Temp", "°C", "#.#");
        static LinearInstrument DPT = new LinearInstrument("Depth", "m", "0.0");
        static LinearInstrument AWS = new LinearInstrument("AWS", "Kn");
        static AngularInstrumentRel AWA = new AngularInstrumentRel("AWA", "°");
        static LinearInstrument TWS = new LinearInstrument("TWS", "Kn", "#.#", 2);
        static AngularInstrumentRel TWA = new AngularInstrumentRel("TWA", "°", "#", 2);
        static AngularInstrumentAbs TWD = new AngularInstrumentAbs("TWD", "°", "000", 15);
        static AngularInstrumentAbs HDT = new AngularInstrumentAbs("HDG", "°T");
        static AngularInstrumentAbs BRG = new AngularInstrumentAbs("BRG", "°T", "000", 1, false);
        static LinearInstrument DST = new LinearInstrument("DST", "Nm", "#.#", 1, false);
        static LinearInstrument XTE = new LinearInstrument("XTE", "Nm", "#.##", 1, false);
        static LinearInstrument VMG = new LinearInstrument("WMG", "Kn", "0.0", 1, false);
        static AngularInstrumentAbs HEEL = new AngularInstrumentAbs("HEEL", "°", "#", 15, false);

        // Destination Waypoint
        static WaypointInstrument WPT = new WaypointInstrument("To:", "", "", 1, false);
        static LatitudeInstrument WLAT = new LatitudeInstrument("Lat", "", "", 1, false);
        static LongitudeInstrument WLON = new LongitudeInstrument("Lon", "", "", 1, false);

        // Last Waypoint
        static WaypointInstrument LWPT = new WaypointInstrument("", "", "", 1, false);
        static LatitudeInstrument LWLAT = new LatitudeInstrument("Lat", "", "", 1, false);
        static LongitudeInstrument LWLON = new LongitudeInstrument("Lon", "", "", 1, false);
        static AngularInstrumentAbs LEGBRG = new AngularInstrumentAbs("BRG", "", "000", 1, false);
        static LinearInstrument VMGWPT = new LinearInstrument("WMGwpt", "Kn", "0.0", 1, false);

        // Next Leg
        static AngularInstrumentRel NTWA = new AngularInstrumentRel("Next TWA", "°", "#", 1, false);

        // Drift
        static AngularInstrumentAbs SET = new AngularInstrumentAbs("Set", "°T", "000", 30);
        static LinearInstrument DRIFT = new LinearInstrument("Drift", "Kn", "0.0", 30);

        // Performance
        static LinearInstrument TGTSPD = new LinearInstrument("Tgt SPD", "Kn", "0.0", 4, false);
        static AngularInstrumentRel TGTTWA = new AngularInstrumentRel("Tgt TWA", "°T", "#", 4, false);
        static PercentInstrument PERF = new PercentInstrument("Perf", "%", "#", 15);
        static LinearInstrument TGTVMC = new LinearInstrument("Tgt VMC", "Kn", "#.##", 4, false);
        static AngularInstrumentAbs TGTCTS = new AngularInstrumentAbs("Tgt CTS", "°T", "000", 4, false);

        // Starting Line
        static LinearInstrument LINEDST = new LinearInstrument("Dst to line", "m", "#", 1, false);

        // Laylines
        static AngularInstrumentAbs TGTCOGp = new AngularInstrumentAbs("Tgt COG Stbd", "°T", "", 1, false);
        static AngularInstrumentAbs TGTCOGs = new AngularInstrumentAbs("Tgt COG Port", "°T", "", 1, false);
        static LinearInstrument TGTSOGp = new LinearInstrument("Tgt SOG Stbd", "Kn", "", 1, false);
        static LinearInstrument TGTSOGs = new LinearInstrument("Tgt SOG Port", "Kn", "", 1, false);
        static LinearInstrument DSTLYLp = new LinearInstrumentShort("Dst Lay Port", "m", "", 1, false);
        static LinearInstrument DSTLYLs = new LinearInstrumentShort("Dst Lay Stbd", "m", "", 1, false);

        static TimeSpanInstrument TTGLYLp = new TimeSpanInstrument("TTG Lay Port", "", "", 1, false);
        static TimeSpanInstrument TTGLYLs = new TimeSpanInstrument("TTG Lay Stbd", "", "", 1, false);

        #endregion

        #region Timers
        DispatcherTimer NMEATimer = new DispatcherTimer();
        DispatcherTimer RMC_received_Timer = new DispatcherTimer();
        DispatcherTimer ShortNavTimer = new DispatcherTimer();
        DispatcherTimer MediumNavTimer = new DispatcherTimer();
        DispatcherTimer LongNavTimer = new DispatcherTimer();
        DispatcherTimer XLNavTimer = new DispatcherTimer();
        DispatcherTimer FleetUpdateTimer = new DispatcherTimer();
        DispatcherTimer ReplayTimer = new DispatcherTimer();
        DispatcherTimer PlotPanTimer = new DispatcherTimer();
        DispatcherTimer UpdatePlotResolutionTimer = new DispatcherTimer();

        #endregion

        #region Database & LogFile 
        StreamWriter LogFile;
        StreamReader ReplayFile;
        bool logging = false;
        bool replayLog = false;
        StartUpWindow StartupWdw;
        #endregion

        #region Polar
        static Polar NavPolar;
        #endregion

        #region Start Line
        // Starting Line position
        static double p1_lat, p1_lon, p2_lat, p2_lon;
        static double linebrg;
        static bool p1_set = false;
        static bool p2_set = false;
        static Mark lineBoatMark = new Mark(),
                    linePinMark = new Mark();
        MapSegment staringLineMapSegment = new MapSegment();

        WLCourseSetupWindow wlCourseSetupWindow = new WLCourseSetupWindow();
        #endregion

        #region Map
        // Map pan & zoom info

        public enum MouseHandlingMode
        {
            None,
            Panning,
            CreatingRoute,
            MovingMark,
            SettingMeasureStart,
            SelectingPlotRange,
            SettingPlotCurrentValue,
            PlotPanning
        }

        public enum MapOrientationMode
        {
            NorthUp,
            CourseUp
        }

        public enum MapCenterMode
        {
            Centered,
            NotCentered
        }

        private MouseHandlingMode mouseHandlingMode = MouseHandlingMode.None;
        private MapOrientationMode mapOrientationMode = MapOrientationMode.NorthUp;
        private MapCenterMode mapCenterMode = MapCenterMode.NotCentered;

        public TileLayer MapLayer1 = new TileLayer();
        public TileLayer MapLayer2 = new TileLayer();

        LayerControlWindow layerControl = new LayerControlWindow();

        public Point PanStartingPoint = new Point();
        public int ClickTime;

        #endregion

        #region Marks
        MarkControlWindow marksControl = new MarkControlWindow();
        int markNumber = 0;
        Mark MovingMark;
        Mark MOB;
        Mark HitPointStbd = new Mark(), HitPointPort = new Mark();

        bool ManOverBoard = false;

        private static readonly DependencyProperty ActiveMarkProperty = DependencyProperty.Register(
            "ActiveMark", typeof(Mark), typeof(Window), null);

        public Mark ActiveMark
        {
            get { return (Mark)GetValue(ActiveMarkProperty); }
            set { SetValue(ActiveMarkProperty, value); }
        }

        #endregion

        #region Routes
        public static RouteControlWindow routeControl = new RouteControlWindow();
        static Route ActiveRoute;
        static Leg ActiveLeg;

        // These are for creating a new route
        Route TempRoute;
        int routeNumber = 1;
        MapSegment TempSegment;
        Mark FirstMark;
        List<Mark> NewMarksOnRoute = new List<Mark>();

        ObservableCollection<Route> routeList = new ObservableCollection<Route>();
        #endregion

        #region MapItems

        ICollection<object> boatsItemCollection;
        ICollection<object> legsItemCollection;

        SampleItemCollection marksItemCollection;

        Boat boat = new Boat
        {
            Name = "AltoRiesgo",
            Location = new Location(-34.5, -58.5),
            BoatVisible = Visibility.Hidden,
            BoatColor = Brushes.Yellow.Color
        };

        Boat replayBoat = new Boat
        {
            Name = "Replay",
            Location = new Location(-34.5, -58.5),
            BoatVisible = Visibility.Hidden,
            BoatColor = Brushes.Yellow.Color
        };

        static public List<Boat> fleetBoats = new List<Boat>();

        static public LinearGradientBrush perfGradientBrush = (LinearGradientBrush)App.Current.FindResource("PerformanceMap");

        Track track;
        bool newTrackPositionAvailable = false;

        static public List<Track> fleetTracks = new List<Track>();

        MapMeasureRange measureRange = new MapMeasureRange();
        Location measureLocation;
        bool fixMeasure = false;
        bool measureCenteredOnBoat = false;
        MeasureResult measureResult = new MeasureResult();

        MapSegment StbLaylineTo = new MapSegment();
        MapSegment PrtLaylineTo = new MapSegment();
        MapSegment StbLaylineFrom = new MapSegment();
        MapSegment PrtLaylineFrom = new MapSegment();
        MapSegment StbBearingTarget = new MapSegment();
        MapSegment PrtBearingTarget = new MapSegment();

        bool laylinesVisible = false;
        bool bearingTargetsVisible = false;

        #endregion

        #region Grib / GDAL

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetEnvironmentVariable(string lpName, string lpValue);

        GribControl gribControl = new GribControl();
        windgrib wgrib;
        currentgrib cgrib;
        WindArrowGrid wagrid;
        CurrentArrowGrid cagrid;
        DateTime minGribTime = new DateTime();
        DateTime maxGribTime = new DateTime();
        #endregion

        #region Plot

        public NavPlotModel NavPlotModel = new NavPlotModel();
        Binding CursorXBinding = new Binding();
        Binding CursorYBinding = new Binding();
        Binding CurrentXBinding = new Binding();
        Binding CurrentYBinding = new Binding();

        ChartValues<DateModel> MainPlotValues = new ChartValues<DateModel>();
        ChartValues<DateModel> AuxPlotValues = new ChartValues<DateModel>();
        ChartValues<DateModel> FleetActivityValues = new ChartValues<DateModel>();

        ObservableCollection<PlotSelector> PlotSelectors = new ObservableCollection<PlotSelector>
        {
            new PlotSelector {Name="SOG",Description="Speed Over Ground", MinValue=0,MaxValue=double.NaN,Formatter=s=>s.ToString("0.0") },
            new PlotSelector {Name="SPD",Description="Speed through Water",MinValue=0,MaxValue=double.NaN,Formatter=s=>s.ToString("0.0") },
            new PlotSelector {Name="TWD",Description="True Wind Direction",MinValue=double.NaN,MaxValue=double.NaN,Formatter=s=>s.ToString("#") },
            new PlotSelector {Name="TWS",Description="True Wind Speed",MinValue=0,MaxValue=double.NaN,Formatter=s=>s.ToString("#") },
            new PlotSelector {Name="Drift",Description="Drift",MinValue=0,MaxValue=double.NaN,Formatter=s=>s.ToString("0.0") },
            new PlotSelector {Name="Perf",Description="Performance",MinValue=double.NaN,MaxValue=double.NaN,Formatter=s=>s.ToString("#") },
            new PlotSelector {Name="Depth",Description="Depth",MinValue=double.NaN,MaxValue=0,Formatter=s=>s.ToString("#.#") },
            new PlotSelector {Name="Fleet",Description="Fleet boats",MinValue=0,MaxValue=double.NaN,Formatter=s=>s.ToString("#") }
        };

        #endregion

        #region Routing

        public static RoutingCalculationControl routeCalculationControl = new RoutingCalculationControl();
        ObservableCollection<RoutingResult> routingResults = new ObservableCollection<RoutingResult>();
        private readonly BackgroundWorker CalcRouteWorker = new BackgroundWorker();
        int totalVertex, processedVertexCnt;
        double lastProcessedPercentage;
        DateTime startTime;
        double perfAdj;
        bool useCurrents;
        List<Location> sourceRouteLocations = new List<Location>();
        #endregion

        #region Sailing

        public enum SailingMode
        {
            None,
            Beating,
            Reaching,
            Running
        }

        SailingMode sailingMode = SailingMode.None;

        bool insideCourse;

        #endregion

        #region Regatta

        Regatta Regatta;

        #endregion

        #endregion

        #region MainWindow Constructor (Initializers)

        public MainWindow()
        {
            InitializeComponent();

            this.Title = "LionRiver " + GetRunningVersion().ToString();

            StartupWdw = new StartUpWindow();
            StartupWdw.Show();

            MainWindow_Initialize();

            StartupWdw.Close();
        }

        private void MainWindow_Initialize()
        {
            #region Polars
            NavPolar = new Polar();

            string pfilename = Properties.Settings.Default.PolarFile;

            if (pfilename != "")
            {
                try
                {
                    StreamReader sr = new StreamReader(pfilename);
                    NavPolar.Load(sr);
                    sr.Close();
                    NavPolar.IsLoaded = true;
                    SendPTAKheaders();
                }
                catch
                { NavPolar.IsLoaded = false; }
            }

            #endregion

            #region Mapping
            string GDAL_HOME = Directory.GetCurrentDirectory();
            GDAL_HOME += @"\GDAL";
            string path = Environment.GetEnvironmentVariable("PATH");
            path += ";" + GDAL_HOME;
            SetEnvironmentVariable("PATH", path);

            //GdalEnvironment.SetupEnvironment(GDAL_HOME);
            //Gdal.AllRegister();
            //Ogr.RegisterAll();

            GdalConfiguration.ConfigureGdal();
            GdalConfiguration.ConfigureOgr();

            MapLayer1.TileSource = new TileSource();
            MapLayer1.TileSource.UriFormat = "file:\\" + Properties.Settings.Default.Layer1Directory + "\\{z}\\{x}\\{v}.png";
            MapLayer1.Opacity = Properties.Settings.Default.Layer1Opacity;
            MapLayer1.MaxZoomLevel = 18;
            if (Properties.Settings.Default.Layer1Check) map.TileLayers.Add(MapLayer1);

            MapLayer2.TileSource = new TileSource();
            MapLayer2.TileSource.UriFormat = "file://" + Properties.Settings.Default.Layer2Directory + "/{z}/{x}/{v}.png";
            MapLayer2.Opacity = Properties.Settings.Default.Layer2Opacity;
            MapLayer2.MaxZoomLevel = 18;
            if (Properties.Settings.Default.Layer2Check) map.TileLayers.Add(MapLayer2);

            //TileLayer MapLayer3 = new TileLayer();
            //MapLayer3.TileSource = new TileSource("http://{c}.tile.openstreetmap.org/{z}/{x}/{y}.png");
            //MapLayer3.Opacity = 0.8;
            //map.TileLayers.Add(MapLayer3);

            map.Center = Properties.Settings.Default.MapCenter;
            map.ZoomLevel = Properties.Settings.Default.MapScale;

            MapBase.AnimationDuration = TimeSpan.FromSeconds(0);

            #endregion

            #region MapItems

            boatsItemCollection = (ICollection<object>)Resources["BoatsItemCollection"];
            legsItemCollection = (ICollection<object>)Resources["LegsItemCollection"];

            //boatsItemCollection.Add(boat);
            //boatsItemCollection.Add(replayBoat);     

            mainBoat.DataContext = boat;
            repBoat.DataContext = replayBoat;

            Panel.SetZIndex(repBoat, 45);
            Panel.SetZIndex(mainBoat, 100);


            marksItemCollection = new SampleItemCollection();
            markItemsControl.DataContext = marksItemCollection;
            marksControl.DataContext = marksItemCollection;

            measureRange.Visibility = Visibility.Hidden;
            map.Children.Add(measureRange);
            this.mapMeasureControl.DataContext = measureResult;


            #endregion

            #region Routes & Marks & Layers
            routeControl.RouteCtrlHd += new RouteCtrlEventHandler(RouteCtrlCommandReceived);
            marksControl.MarkCtrlHd += new MarkCtrlEventHandler(MarkCtrlCommandReceived);
            layerControl.LayerCtrlHd += new LayerControlWindow.LayerCtrlEventHandler(LayerCtrlCommandReceived);

            routeControl.LoadButton.Click += new RoutedEventHandler(GPXLoadButton_Click);
            routeControl.RouteListComboBox.DataContext = routeList;
            #endregion

            #region Grib
            gribControl.GribSlider.ValueChanged += new RoutedPropertyChangedEventHandler<double>(GribSlider_ValueChanged);
            gribControl.NowButton.Click += new RoutedEventHandler(GribNowButton_Click);
            gribControl.DisplayWind.Checked += new RoutedEventHandler(GribDisplay_Checked);
            gribControl.DisplayWind.Unchecked += new RoutedEventHandler(GribDisplay_Checked);
            gribControl.DisplayCurrent.Checked += new RoutedEventHandler(GribDisplay_Checked);
            gribControl.DisplayCurrent.Unchecked += new RoutedEventHandler(GribDisplay_Checked);
            #endregion

            #region Routing
            routeCalculationControl.RouteListCombo.DataContext = routeList;
            routeCalculationControl.ResultCombo.DataContext = routingResults;
            routeCalculationControl.CalculateRoute.Click += new RoutedEventHandler(RouteCalcButton_Click);
            routeCalculationControl.ClearResults.Click += new RoutedEventHandler(RouteClearResultsButton_Click);
            routeCalculationControl.ResultCombo.SelectionChanged += ResultCombo_SelectionChanged;
            routeCalculationControl.RouteReplaySlider.ValueChanged += RouteReplaySlider_ValueChanged;

            CalcRouteWorker.DoWork += CalcRouteWorker_DoWork;
            CalcRouteWorker.RunWorkerCompleted += CalcRouteWorker_RunWorkerCompleted;
            CalcRouteWorker.ProgressChanged += CalcRouteWorker_ProgressChanged;
            CalcRouteWorker.WorkerReportsProgress = true;
            #endregion

            #region Command Bindings

            CommandManager.RegisterClassCommandBinding(typeof(Window), new CommandBinding(CommandLibrary.AddMark, AddMarkCommand_Executed, AddMarkCommand_CanExecute));
            CommandManager.RegisterClassCommandBinding(typeof(Window), new CommandBinding(CommandLibrary.DeleteMark, DeleteMarkCommand_Executed, DeleteMarkCommand_CanExecute));
            CommandManager.RegisterClassCommandBinding(typeof(Window), new CommandBinding(CommandLibrary.NewRoute, NewRouteCommand_Executed, NewRouteCommand_CanExecute));
            CommandManager.RegisterClassCommandBinding(typeof(Window), new CommandBinding(CommandLibrary.NavigateTo, NavigateToCommand_Executed, NavigateToCommand_CanExecute));
            CommandManager.RegisterClassCommandBinding(typeof(Window), new CommandBinding(CommandLibrary.ActivateRoute, ActivateRouteCommand_Executed, ActivateRouteCommand_CanExecute));
            CommandManager.RegisterClassCommandBinding(typeof(Window), new CommandBinding(CommandLibrary.StopNav, StopNavCommand_Executed, StopNavCommand_CanExecute));
            CommandManager.RegisterClassCommandBinding(typeof(Window), new CommandBinding(CommandLibrary.FwdRoute, FwdRouteCommand_Executed, FwdRouteCommand_CanExecute));
            CommandManager.RegisterClassCommandBinding(typeof(Window), new CommandBinding(CommandLibrary.RwdRoute, RwdRouteCommand_Executed, RwdRouteCommand_CanExecute));
            CommandManager.RegisterClassCommandBinding(typeof(Window), new CommandBinding(CommandLibrary.ReverseRoute, ReverseRouteCommand_Executed, ReverseRouteCommand_CanExecute));
            CommandManager.RegisterClassCommandBinding(typeof(Window), new CommandBinding(CommandLibrary.DeleteRoute, DeleteRouteCommand_Executed, DeleteRouteCommand_CanExecute));
            CommandManager.RegisterClassCommandBinding(typeof(Window), new CommandBinding(CommandLibrary.SetLineBoat, SetLineBoatCommand_Executed));
            CommandManager.RegisterClassCommandBinding(typeof(Window), new CommandBinding(CommandLibrary.SetLinePin, SetLinePinCommand_Executed));
            CommandManager.RegisterClassCommandBinding(typeof(Window), new CommandBinding(CommandLibrary.RemoveInstrument, RemoveInstrumentCommand_Executed));
            CommandManager.RegisterClassCommandBinding(typeof(Window), new CommandBinding(CommandLibrary.SelectFleetBoat, SelectFleetBoatCommand_Executed, SelectFleetBoatCommand_CanExecute));
            CommandManager.RegisterClassCommandBinding(typeof(Window), new CommandBinding(CommandLibrary.UnselectFleetBoat, UnselectFleetBoatCommand_Executed, UnselectFleetBoatCommand_CanExecute));
            CommandManager.RegisterClassCommandBinding(typeof(Window), new CommandBinding(CommandLibrary.HideUnselectedFleetBoats, HideUnselectedFleetBoatsCommand_Executed));
            CommandManager.RegisterClassCommandBinding(typeof(Window), new CommandBinding(CommandLibrary.UnhideAllFleetBoats, UnhideAllFleetBoatsCommand_Executed));
            CommandManager.RegisterClassCommandBinding(typeof(Window), new CommandBinding(CommandLibrary.UnselectAllFleetBoats, UnselectAllFleetBoatsCommand_Executed));
            CommandManager.RegisterClassCommandBinding(typeof(Window), new CommandBinding(CommandLibrary.CalcRegatta, CalcRegattaCommand_Executed));

            #endregion

            #region Nav Plots

            NavPlotModel.CurrentValue = DateTime.Now.Ticks;

            DateTime minV = new DateTime((long)(NavPlotModel.CurrentValue - TimeSpan.FromHours(2).Ticks));
            DateTime maxV = new DateTime((long)(NavPlotModel.CurrentValue));

            NavPlotModel.MinXAxisValue = minV.Ticks;
            NavPlotModel.MaxXAxisValue = NavPlotModel.CurrentValue +
                                            (NavPlotModel.CurrentValue - NavPlotModel.MinXAxisValue) * 0.2;
            NavPlotModel.Resolution = 2;
            NavPlotModel.XFormatter = value => new System.DateTime((long)(value)).ToString("dd MMM");

            //NavPlotModel.XStep = (double)TimeSpan.FromDays(7).Ticks;

            var dayConfig = Mappers.Xy<DateModel>()
                .X(dayModel => (double)dayModel.DateTime.Ticks)
                .Y(dayModel => dayModel.Value ?? double.NaN);

            NavPlotModel.MinY1AxisValue = 0;
            NavPlotModel.MaxY1AxisValue = double.NaN;

            NavPlotModel.MinY2AxisValue = 0;
            NavPlotModel.MaxY2AxisValue = double.NaN;

            NavPlotModel.SeriesCollection = new SeriesCollection(dayConfig)
                {
                new LineSeries
                    {
                    Title = "MainPlot",
                    Values = MainPlotValues,
                    Fill=System.Windows.Media.Brushes.Transparent,
                    Stroke=System.Windows.Media.Brushes.DodgerBlue,
                    PointGeometry = null,
                    LineSmoothness=0,
                    StrokeThickness=1,
                    ScalesYAt=0
                    },

                new LineSeries
                    {
                    Title = "AuxPlot",
                    Values = AuxPlotValues,
                    Fill=System.Windows.Media.Brushes.Transparent,
                    Stroke=System.Windows.Media.Brushes.MediumVioletRed,
                    PointGeometry = null,
                    LineSmoothness=0,
                    StrokeThickness=1,
                    ScalesYAt=1
                    }
                };

            using (var context = new LionRiverDBContext())
            {
                // Insert Empty values on Timestamp = Last available data an Timestamp = Now. This avoids connecting plot points from previous Logs

                var logEntry = (from x in context.Logs
                                select x).Take(1).ToList();

                if (logEntry.Count != 0)
                {
                    var vLast = logEntry.Last();

                    for (int i = 0; i < Inst.MaxBuffers; i++)
                    {
                        var logLast = new Log()
                        {
                            timestamp = vLast.timestamp,
                            level = i,
                            LAT = vLast.LAT,
                            LON = vLast.LON,
                            COG = double.NaN,
                            SOG = double.NaN,
                            HDT = double.NaN,
                            TWD = double.NaN,
                            PERF = double.NaN,
                            DPT = double.NaN,
                            TWS = double.NaN,
                            DRIFT = double.NaN,
                            SET = double.NaN,
                            SPD = double.NaN
                        };

                        var logFirst = new Log()
                        {
                            timestamp = maxV,
                            level = i,
                            LAT = vLast.LAT,
                            LON = vLast.LON,
                            COG = double.NaN,
                            SOG = double.NaN,
                            HDT = double.NaN,
                            TWD = double.NaN,
                            PERF = double.NaN,
                            DPT = double.NaN,
                            TWS = double.NaN,
                            DRIFT = double.NaN,
                            SET = double.NaN,
                            SPD = double.NaN
                        };

                        context.Logs.Add(logLast);
                        context.Logs.Add(logFirst);

                    }
                    context.SaveChanges();
                }

                // Get Fleet Activity over all available data, grouped by hour

                var ftEntries = (from x in context.FleetTracks
                                 group x by new { year = x.timestamp.Year, month = x.timestamp.Month, day = x.timestamp.Day, hour = x.timestamp.Hour } into grp
                                 select new { k = grp.Key, cnt = grp.Count() }).ToList();

                var ftList = (from x in ftEntries
                              select new DateModel { DateTime = new DateTime(x.k.year, x.k.month, x.k.day, x.k.hour, 0, 0), Value = x.cnt }).ToList();

                FleetActivityValues.AddRange(ftList);

            }

            CursorXBinding.Source = NavPlotModel;
            CursorXBinding.Path = new PropertyPath("CursorValue");
            CursorXBinding.Mode = BindingMode.OneWay;
            CursorXBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            BindingOperations.SetBinding(MainNavPlot.Cursor, VisualElement.XProperty, CursorXBinding);

            CursorYBinding.Source = NavPlotModel;
            CursorYBinding.Path = new PropertyPath("MinY1AxisValue");
            CursorYBinding.Mode = BindingMode.OneWay;
            CursorYBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            BindingOperations.SetBinding(MainNavPlot.Cursor, VisualElement.YProperty, CursorYBinding);


            CurrentXBinding.Source = NavPlotModel;
            CurrentXBinding.Path = new PropertyPath("CurrentValue");
            CurrentXBinding.Mode = BindingMode.OneWay;
            CurrentXBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            BindingOperations.SetBinding(MainNavPlot.Current, VisualElement.XProperty, CurrentXBinding);

            CurrentYBinding.Source = NavPlotModel;
            CurrentYBinding.Path = new PropertyPath("MaxY1AxisValue");
            CurrentYBinding.Mode = BindingMode.OneWay;
            CurrentYBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            BindingOperations.SetBinding(MainNavPlot.Current, VisualElement.YProperty, CurrentYBinding);

            MainNavPlot.DataContext = NavPlotModel;

            MainPlotSelectionComboBox.DataContext = PlotSelectors;
            AuxPlotSelectionComboBox.DataContext = PlotSelectors;

            PlayButton.IsChecked = true;

            #endregion

            #region Track

            UpdateTracks(minV, maxV);

            #endregion

            #region Replay & Fleet

            using (var context = new LionRiverDBContext())
            {

                var boatList =
                    (from b in context.FleetTracks
                     select b.Name).Distinct();

                Random rnd = new Random();

                foreach (string name in boatList)
                {
                    Boat b = new Boat()
                    {
                        Name = name,
                        IsSelected = false
                    };

                    MapItem boatMapItem = new MapItem();
                    boatMapItem.Style = (Style)Application.Current.Resources["FleetBoatItemStyle"];

                    b.BoatColor = Color.FromRgb((byte)rnd.Next(256), (byte)rnd.Next(256), (byte)rnd.Next(256));

                    fleetBoats.Add(b);
                    boatMapItem.DataContext = b;
                    map.Children.Add(boatMapItem);
                }
            }

            #endregion

            #region Starting Line
            //map.Children.Add(lineBoatMark);
            //map.Children.Add(linePinMark);
            #endregion

            #region MapItems
            bearingTargetsVisible = Properties.Settings.Default.TargetBearingsCheck;
            laylinesVisible = Properties.Settings.Default.LaylinesCheck;

            map.Children.Add(StbLaylineTo);
            map.Children.Add(PrtLaylineTo);
            map.Children.Add(StbLaylineFrom);
            map.Children.Add(PrtLaylineFrom);
            map.Children.Add(StbBearingTarget);
            map.Children.Add(PrtBearingTarget);

            StbLaylineTo.Stroke = Brushes.LimeGreen;
            StbLaylineTo.StrokeThickness = 1;
            //StbLaylineTo.StrokeDashArray = new DoubleCollection() { 1, 6 };

            PrtLaylineTo.Stroke = Brushes.Red;
            PrtLaylineTo.StrokeThickness = 1;
            //PrtLaylineTo.StrokeDashArray = new DoubleCollection() { 1, 6 };

            StbLaylineFrom.Stroke = Brushes.LimeGreen;
            StbLaylineFrom.StrokeThickness = 1;
            StbLaylineFrom.StrokeDashArray = new DoubleCollection() { 1, 6 };

            PrtLaylineFrom.Stroke = Brushes.Red;
            PrtLaylineFrom.StrokeThickness = 1;
            PrtLaylineFrom.StrokeDashArray = new DoubleCollection() { 1, 6 };

            StbBearingTarget.Stroke = Brushes.LimeGreen;
            StbBearingTarget.StrokeThickness = 1;
            //StbBearingTarget.StrokeDashArray = new DoubleCollection() { 1, 6 };

            PrtBearingTarget.Stroke = Brushes.Red;
            PrtBearingTarget.StrokeThickness = 1;
            //PrtBearingTarget.StrokeDashArray = new DoubleCollection() { 1, 6 };

            HitP.DataContext = HitPointPort;
            HitS.DataContext = HitPointStbd;


            #endregion

            #region Timers
            NMEATimer.Tick += new EventHandler(NMEATimer_Tick);
            NMEATimer.Interval = new TimeSpan(0, 0, 5);

            RMC_received_Timer.Tick += new EventHandler(RMC_received_Timer_Tick);
            RMC_received_Timer.Interval = new TimeSpan(0, 0, 5);

            ShortNavTimer.Tick += new EventHandler(ShortNavTimer_Tick);
            ShortNavTimer.Interval = new TimeSpan(0, 0, 1);

            MediumNavTimer.Tick += new EventHandler(MediumNavTimer_Tick);
            MediumNavTimer.Interval = new TimeSpan(0, 0, 4);

            LongNavTimer.Tick += new EventHandler(LongNavTimer_Tick);
            LongNavTimer.Interval = new TimeSpan(0, 0, 15);

            XLNavTimer.Tick += new EventHandler(XLNavTimer_Tick);
            XLNavTimer.Interval = new TimeSpan(0, 15, 0);

            FleetUpdateTimer.Tick += new EventHandler(FleetUpdateTimer_Tick);
            FleetUpdateTimer.Interval = new TimeSpan(0, 5, 0);
            //FleetUpdateTimer.Interval = new TimeSpan(0, 0, 60);  // For testing

            ReplayTimer.Tick += new EventHandler(ReplayTimer_Tick);
            ReplayTimer.Interval = new TimeSpan(0, 0, 0, 0, 150);

            PlotPanTimer.Tick += new EventHandler(PlotPanTimer_Tick);
            PlotPanTimer.Interval = new TimeSpan(0, 0, 0, 0, 150);

            UpdatePlotResolutionTimer.Tick += new EventHandler(UpdatePlotResolution_Tick);
            UpdatePlotResolutionTimer.Interval = new TimeSpan(0, 0, 1);

            NMEATimer.Start();
            ShortNavTimer.Start();
            MediumNavTimer.Start();
            LongNavTimer.Start();
            XLNavTimer.Start();
            FleetUpdateTimer.Start();
            #endregion

            #region Ports and Threads

            SerialPort1 = new SerialPort();
            InitializeSerialPort1();

            SerialPort2 = new SerialPort();
            InitializeSerialPort2();

            SerialPort3 = new SerialPort();
            InitializeSerialPort3();

            SerialPort4 = new SerialPort();
            InitializeSerialPort4();

            readThread1 = new Thread(ReadSerial1);
            readThread1.CurrentCulture = new System.Globalization.CultureInfo("en-US", false);
            terminateThread1 = false;
            readThread1.Start();

            readThread2 = new Thread(ReadSerial2);
            readThread2.CurrentCulture = new System.Globalization.CultureInfo("en-US", false);
            terminateThread2 = false;
            readThread2.Start();

            readThread3 = new Thread(ReadSerial3);
            readThread3.CurrentCulture = new System.Globalization.CultureInfo("en-US", false);
            terminateThread3 = false;
            readThread3.Start();

            readThread4 = new Thread(ReadSerial4);
            readThread4.CurrentCulture = new System.Globalization.CultureInfo("en-US", false);
            terminateThread4 = false;
            readThread4.Start();


            if (Properties.Settings.Default.TacktickPerformanceSentence == null)
            {
                NMEASentence dummy = new NMEASentence();
                Properties.Settings.Default.TacktickPerformanceSentence = dummy;
                Properties.Settings.Default.Save();
            }

            SendPTAKheaders();

            #endregion

            #region Instrument display context binding


            InstrumentDisplays["COG"].DataContext = COG;
            InstrumentDisplays["SOG"].DataContext = SOG;
            InstrumentDisplays["HDT"].DataContext = HDT;
            InstrumentDisplays["SPD"].DataContext = SPD;
            InstrumentDisplays["DPT"].DataContext = DPT;
            InstrumentDisplays["TEMP"].DataContext = TEMP;
            InstrumentDisplays["AWA"].DataContext = AWA;
            InstrumentDisplays["TWA"].DataContext = TWA;
            InstrumentDisplays["TWS"].DataContext = TWS;
            InstrumentDisplays["TWD"].DataContext = TWD;
            InstrumentDisplays["BRG"].DataContext = BRG;
            InstrumentDisplays["DST"].DataContext = DST;
            InstrumentDisplays["WPT"].DataContext = WPT;
            InstrumentDisplays["XTE"].DataContext = XTE;
            InstrumentDisplays["VMG"].DataContext = VMG;
            InstrumentDisplays["VMGWPT"].DataContext = VMGWPT;
            InstrumentDisplays["NTWA"].DataContext = NTWA;
            InstrumentDisplays["DRIFT"].DataContext = DRIFT;
            InstrumentDisplays["SET"].DataContext = SET;
            InstrumentDisplays["TGTTWA"].DataContext = TGTTWA;
            InstrumentDisplays["TGTSPD"].DataContext = TGTSPD;
            InstrumentDisplays["PERF"].DataContext = PERF;
            InstrumentDisplays["TGTVMC"].DataContext = TGTVMC;
            InstrumentDisplays["TGTCTS"].DataContext = TGTCTS;

            InstrumentStackContextMenu.ItemsSource = InstrumentDisplays;

            this.LatTextBlock.DataContext = LAT;
            this.LonTextBlock.DataContext = LON;

            string[] ikeys = Properties.Settings.Default.InstrumentDisplayList.Split(',');

            foreach (string k in ikeys)
            {
                if (k != "")
                    InstrumentStackPanel.Children.Add(InstrumentDisplays[k]);
            }


            #endregion

        }

        #endregion

        #region Timer ticks

        private void NMEATimer_Tick(object sender, EventArgs e)
        {

            switch (DataReceiverStatus1.Result)
            {
                case RxTxResult.Ok:
                    DataReceiverStatus1.Result = RxTxResult.NoRxData; // Reset for next period of NMEATimer
                    borderPort1.Background = Brushes.Violet;
                    borderPort1.ToolTip = "";
                    break;

                case RxTxResult.NoRxData:
                    borderPort1.Background = Brushes.LightGray;
                    borderPort1.ToolTip = "";
                    break;

                case RxTxResult.WrongRxData:
                    borderPort1.Background = Brushes.Red;
                    borderPort1.ToolTip = DataReceiverStatus1.Error;
                    break;
            }

            switch (DataReceiverStatus2.Result)
            {
                case RxTxResult.Ok:
                    DataReceiverStatus2.Result = RxTxResult.NoRxData; // Reset for next period of NMEATimer
                    borderPort2.Background = Brushes.Violet;
                    borderPort2.ToolTip = "";
                    break;

                case RxTxResult.NoRxData:
                    borderPort2.Background = Brushes.LightGray;
                    borderPort2.ToolTip = "";
                    break;

                case RxTxResult.WrongRxData:
                    borderPort2.Background = Brushes.Red;
                    borderPort2.ToolTip = DataReceiverStatus2.Error;
                    break;
            }

            switch (DataReceiverStatus3.Result)
            {
                case RxTxResult.Ok:
                    DataReceiverStatus3.Result = RxTxResult.NoRxData; // Reset for next period of NMEATimer
                    borderPort3.Background = Brushes.Violet;
                    borderPort3.ToolTip = "";
                    break;

                case RxTxResult.NoRxData:
                    borderPort3.Background = Brushes.LightGray;
                    borderPort3.ToolTip = "";
                    break;

                case RxTxResult.WrongRxData:
                    borderPort3.Background = Brushes.Red;
                    borderPort3.ToolTip = DataReceiverStatus3.Error;
                    break;
            }

            switch (DataReceiverStatus4.Result)
            {
                case RxTxResult.Ok:
                    DataReceiverStatus4.Result = RxTxResult.NoRxData; // Reset for next period of NMEATimer
                    borderPort4.Background = Brushes.Violet;
                    borderPort4.ToolTip = "";
                    break;

                case RxTxResult.NoRxData:
                    borderPort4.Background = Brushes.LightGray;
                    borderPort4.ToolTip = "";
                    break;

                case RxTxResult.WrongRxData:
                    borderPort4.Background = Brushes.Red;
                    borderPort4.ToolTip = DataReceiverStatus4.Error;
                    break;
            }

        }

        private void RMC_received_Timer_Tick(object sender, EventArgs e)
        {
            rmc_received = false;
        }

        private void ShortNavTimer_Tick(object sender, EventArgs e)
        {
            if (replayLog)
            {
                CalcNavFromFile(ReplayFile, DateTime.MinValue);
            }
            else
            {
                CalcNav(DateTime.Now);
                SendNMEA();
            }

            UpdateNav();
        }

        private void MediumNavTimer_Tick(object sender, EventArgs e)
        {
            SendPerformanceNMEA();
        }

        private void LongNavTimer_Tick(object sender, EventArgs e)
        {
            CalcLongNav(DateTime.Now);
            CalcRouteData();
            routeControl.DataGrid1.Items.Refresh();
        }

        private void XLNavTimer_Tick(object sender, EventArgs e)
        {
            SetGribTimeNow();
            SendPTAKheaders();
        }

        static async Task<string> DownloadPage(string url)
        {
            using (var client = new HttpClient())
            {
                using (var r = await client.GetAsync(new Uri(url)))
                {
                    string result = await r.Content.ReadAsStringAsync();
                    return result;
                }
            }
        }

        static async Task<string> HttpPost(string url, string str)
        {
            try
            {
                HttpClient client = new HttpClient();

                Uri uri = new Uri(url);
                StringContent content = new StringContent(str, System.Text.Encoding.UTF8, "application/x-www-form-urlencoded");
                HttpResponseMessage response = await client.PostAsync(uri, content);

                if (!response.IsSuccessStatusCode)
                {
                    return string.Empty;
                }

                string jsonResponse = await response.Content.ReadAsStringAsync();
                return jsonResponse;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        private async void FleetUpdateTimer_Tick(object sender, EventArgs e)
        {
            string json;

            try
            {
                json = await DownloadPage("http://posicionadores.yca.org.ar/sistema/regatas/RegataYCA/feedposiciones.json?_=1487086731874");
            }
            catch
            {
                return;
            }

            RootObj BoatsLastPosition = new RootObj();
            BoatsLastPosition = JsonConvert.DeserializeObject<RootObj>(json);

            List<FleetTrack> ftList = new List<FleetTrack>();

            using (var context = new LionRiverDBContext())
            {
                double cnt = 0;
                FleetDownloadProgressGrid.Visibility = Visibility.Visible;
                FleetDownloadProgressBar.Maximum = BoatsLastPosition.posicion.Count();

                foreach (JSONBoatPosition bp in BoatsLastPosition.posicion)
                {
                    FleetDownloadProgressBar.Value = ++cnt;

                    DateTime newUpdateTime = DateTime.ParseExact(bp.fecha + " " + bp.hora, "dd/MM/yyyy HH:mm", System.Globalization.CultureInfo.InvariantCulture);

                    var fb =
                        (from x in context.FleetTracks
                         where x.Name == bp.embarcacion
                         orderby x.timestamp descending
                         select x);

                    var z = fb.FirstOrDefault(); // Get last entry if available

                    if (z == null) //new Boat available
                    {
                        Boat b = new Boat()
                        {
                            Name = bp.embarcacion,
                            Location = new Location()
                            {
                                Latitude = bp.latitud,
                                Longitude = bp.longitud
                            },
                            Time = newUpdateTime,
                            BoatVisible = Visibility.Visible,
                            Heading = Convert.ToDouble(bp.rumbo.TrimEnd('°')),
                            IsSelected = false,
                            IsAvailable = true
                        };

                        MapItem boatMapItem = new MapItem();
                        boatMapItem.Style = (Style)Application.Current.Resources["FleetBoatItemStyle"];

                        Random rnd = new Random();
                        b.BoatColor = Color.FromRgb((byte)rnd.Next(256), (byte)rnd.Next(256), (byte)rnd.Next(256));

                        fleetBoats.Add(b);
                        boatMapItem.DataContext = b;
                        map.Children.Add(boatMapItem);
                    }
                    else //Update Time for existing fleet boat
                    {
                        var bx = (from x in fleetBoats
                                  where x.Name == z.Name
                                  select x).FirstOrDefault();

                        if (bx != null)
                            bx.Time = newUpdateTime;
                    }

                    DateTime lastUpdateTime;

                    if (z == null)
                        lastUpdateTime = DateTime.MinValue;
                    else
                        lastUpdateTime = z.timestamp;

                    if (lastUpdateTime != newUpdateTime)  // New boat or data available, download complete track, save new entries only
                    {
                        RootObj bTrack = new RootObj();

                        try
                        {
                            json = await HttpPost("http://posicionadores.yca.org.ar/sistema/posicionador.php", "modo=trayectoria&barco=" + bp.embarcacion);
                        }
                        catch
                        {
                            FleetDownloadProgressGrid.Visibility = Visibility.Hidden;
                            return;
                        }

                        if (json != "")
                        {
                            try
                            {
                                bTrack = JsonConvert.DeserializeObject<RootObj>(json);
                            }
                            catch { bTrack = null; }
                        }

                        if (bTrack != null)
                        {
                            // If there is only one track point available, add and continue
                            if (bTrack.posicion.Count() == 1)
                            {
                                var x0 = bTrack.posicion[0];
                                DateTime t0 = DateTime.ParseExact(x0.fecha + " " + x0.hora, "dd/MM/yyyy HH:mm", System.Globalization.CultureInfo.InvariantCulture);

                                if (t0 > lastUpdateTime)
                                {
                                    ftList.Add(new FleetTrack()
                                    {
                                        Name = bp.embarcacion,
                                        timestamp = t0,
                                        Latitude = x0.latitud,
                                        Longitude = x0.longitud,
                                        COG = Convert.ToDouble(x0.rumbo.TrimEnd('°')),
                                        SOG = Convert.ToDouble(x0.velocidad.Split(' ')[0])
                                    });
                                }
                            }

                            // If there are more that one track points, interpolate between entries
                            for (int i = 1; i < bTrack.posicion.Count(); i++)
                            {
                                var x0 = bTrack.posicion[i - 1];
                                var x1 = bTrack.posicion[i];

                                DateTime t0 = DateTime.ParseExact(x0.fecha + " " + x0.hora, "dd/MM/yyyy HH:mm", System.Globalization.CultureInfo.InvariantCulture);
                                DateTime t1 = DateTime.ParseExact(x1.fecha + " " + x1.hora, "dd/MM/yyyy HH:mm", System.Globalization.CultureInfo.InvariantCulture);

                                double sog0 = Convert.ToDouble(x0.velocidad.Split(' ')[0]);
                                double cog = CalcBearing(x0.latitud, x0.longitud, x1.latitud, x1.longitud);
                                double dist = CalcDistance(x0.latitud, x0.longitud, x1.latitud, x1.longitud) / 1852; // nautical miles
                                int steps = 3;
                                double deltaT = (t1 - t0).TotalHours; // Hours

                                if (t1 > lastUpdateTime && deltaT != 0) // New data available
                                {
                                    double a = 2 * (dist / deltaT - sog0) / deltaT;

                                    // Add #steps interpolated equally spaced entries if time span <2 hr
                                    if ((t1 - t0) < TimeSpan.FromHours(1))
                                    {
                                        for (int j = 1; j < steps; j++)
                                        {
                                            double d = j * dist / steps;
                                            double t = 0;

                                            if (a == 0)
                                            {
                                                if (sog0 != 0)
                                                    t = d / sog0;
                                                else
                                                    t = 0;
                                            }
                                            else
                                            {
                                                double k = Math.Sqrt(sog0 * sog0 + 2 * a * d);
                                                double sol1 = (-sog0 + k) / a;
                                                double sol2 = (-sog0 - k) / a;
                                                if (sol1 > 0 && sol1 < deltaT)
                                                    t = sol1;
                                                else
                                                if (sol2 > 0 && sol2 < deltaT)
                                                    t = sol2;
                                            }

                                            double sog = sog0 + a * t;

                                            double lat = 0, lon = 0;
                                            CalcPosition(x0.latitud, x0.longitud, d * 1852, cog, ref lat, ref lon);

                                            ftList.Add(new FleetTrack()
                                            {
                                                Name = bp.embarcacion,
                                                timestamp = t0.AddHours(t),
                                                Latitude = lat,
                                                Longitude = lon,
                                                COG = cog,
                                                SOG = sog
                                            });
                                        }

                                        // Add last entry (x1)
                                        ftList.Add(new FleetTrack()
                                        {
                                            Name = bp.embarcacion,
                                            timestamp = t1,
                                            Latitude = x1.latitud,
                                            Longitude = x1.longitud,
                                            COG = Convert.ToDouble(x1.rumbo.TrimEnd('°')),
                                            SOG = Convert.ToDouble(x1.velocidad.Split(' ')[0])
                                        });
                                    }

                                    // Add new entry

                                }
                            }
                        }

                    }
                }

                var zz = ftList.GroupBy(g => new { g.Name, g.timestamp }).Select(h => h.First()).ToList(); // Eliminate duplicates

                context.FleetTracks.AddRange(zz);
                await context.SaveChangesAsync();

            }
            FleetDownloadProgressGrid.Visibility = Visibility.Hidden;

            UpdatePlotResolutionTimer.Start(); // Forces updating tracks on Map s well

            if (PlayButton.IsChecked == true)
                UpdateFleet(DateTime.Now);
        }

        private void ReplayTimer_Tick(object sender, EventArgs e)
        {
            double deltaT = NavPlotModel.MaxXAxisValue - NavPlotModel.MinXAxisValue;

            NavPlotModel.CurrentValue += deltaT * FwdBackSlider.Value / 1000;

            double lim = NavPlotModel.MinXAxisValue + 0.8 * deltaT;

            if (NavPlotModel.CurrentValue > lim)
            {
                NavPlotModel.MinXAxisValue = (NavPlotModel.CurrentValue - 0.8 * deltaT);
                NavPlotModel.MaxXAxisValue = (NavPlotModel.CurrentValue + 0.2 * deltaT);
            }

            if (PlotCenterButton.IsChecked == true)
            {
                NavPlotModel.MaxXAxisValue = NavPlotModel.CurrentValue + deltaT / 2;
                NavPlotModel.MinXAxisValue = NavPlotModel.CurrentValue - deltaT / 2;
            }

            DateTime dt = new DateTime((long)NavPlotModel.CurrentValue);
            UpdateFleet(dt);

            if (UpdatePlotResolutionTimer.IsEnabled == false)
                UpdatePlotResolutionTimer.Start();
        }

        private void PlotPanTimer_Tick(object sender, EventArgs e)
        {
            PlotPanTimer.Stop();

            DateTime dt = new DateTime((long)NavPlotModel.CurrentValue);
            UpdateFleet(dt);
        }

        private void UpdatePlotResolution_Tick(object sender, EventArgs e)
        {
            UpdatePlotResolutionTimer.Stop();

            const double minWSize = 1.1;
            const double tgtWSize = 1.3;
            const double maxWSize = 1.5;

            double PlotWStart = NavPlotModel.MinXAxisValue;
            double PlotWEnd = NavPlotModel.MaxXAxisValue;
            double PlotWindowSize = PlotWEnd - PlotWStart;

            double DataWStart, DataWEnd;

            if (MainPlotValues.Count() != 0)
            {
                DataWStart = MainPlotValues.Last().DateTime.Ticks;
                DataWEnd = MainPlotValues.First().DateTime.Ticks;
            }
            else
            {
                DataWStart = PlotWStart;
                DataWEnd = PlotWEnd;
            }

            double DataWindowSize = DataWEnd - DataWStart;

            if (DataWindowSize < minWSize * PlotWindowSize ||
                DataWindowSize > maxWSize * PlotWindowSize ||
                DataWStart > PlotWStart ||
                DataWEnd < PlotWEnd)
            {
                DateTime newDataWStart = new DateTime((long)(PlotWStart - PlotWindowSize * (tgtWSize - 1) / 2));
                DateTime newDataWEnd = new DateTime((long)(PlotWEnd + PlotWindowSize * (tgtWSize - 1) / 2));

                double k = TimeSpan.FromSeconds(1).Ticks;

                double deltaStep = tgtWSize * PlotWindowSize / NavPlotModel.MaxData / k;

                double n = Math.Round(Math.Log(deltaStep) / Math.Log(Inst.ZoomFactor));

                if (n > Inst.MaxBuffers - 1)
                    n = Inst.MaxBuffers - 1;

                if (n < 0)
                    n = 0;

                NavPlotModel.Resolution = (int)n;

                UpdatePlot(n, newDataWStart, newDataWEnd);

                if (PlayButton.IsChecked == true)
                    UpdateTracks(newDataWStart, new DateTime((long)NavPlotModel.MaxXAxisValue), Track.MaxLength);
            }

        }

        #endregion

        #region Map Mouse Manipulation

        private void MapMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ClickTime = e.Timestamp;
            if (e.ClickCount == 2)
            {
                map.ZoomMap(e.GetPosition(map), Math.Floor(map.ZoomLevel + 1.5));
                map.TargetCenter = map.ViewportPointToLocation(e.GetPosition(map));
            }
            e.Handled = true;
        }

        private void MapMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (mouseHandlingMode == MouseHandlingMode.CreatingRoute && (e.Timestamp - ClickTime) < 200)
            {
                RouteAddMarkAt(map.ViewportPointToLocation(e.GetPosition(map)));
            }

            if (mouseHandlingMode == MouseHandlingMode.SettingMeasureStart && (e.Timestamp - ClickTime) < 200)
            {
                if (!fixMeasure)
                    measureRange.FromLocation = map.ViewportPointToLocation(e.GetPosition(map));
                else
                    measureRange.FromLocation = measureLocation;

                measureRange.Visibility = Visibility.Visible;
                mapMeasureControl.Visibility = Visibility.Visible;
                mouseHandlingMode = MouseHandlingMode.None;
                this.Cursor = Cursors.Arrow;
            }
        }

        private void MapMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                map.ZoomMap(e.GetPosition(map), Math.Ceiling(map.ZoomLevel - 1.5));
            }
        }

        private void MapMouseMove(object sender, MouseEventArgs e)
        {
            Point p = e.GetPosition(map);
            var mouseLocation = map.ViewportPointToLocation(p);

            switch (mouseHandlingMode)
            {
                case MouseHandlingMode.MovingMark:
                    {
                        MovingMark.Location = mouseLocation;
                        measureLocation = mouseLocation;
                        break;
                    }
                case MouseHandlingMode.CreatingRoute:
                    {
                        if (TempSegment != null)
                            TempSegment.ToLocation = mouseLocation;
                        break;
                    }
            }

            if (!fixMeasure)
                measureLocation = mouseLocation;

            if ((bool)MeasureButton.IsChecked)
            {
                measureRange.ToLocation = measureLocation;
                mapMeasureControl.Margin = new Thickness(p.X, p.Y, 0, 0);
                CalcMeasure();
            }


            e.Handled = true;
        }

        private void MapManipulationInertiaStarting(object sender, ManipulationInertiaStartingEventArgs e)
        {
            e.TranslationBehavior.DesiredDeceleration = 0.001;
        }

        private void Mark_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ClickTime = e.Timestamp;
            switch (mouseHandlingMode)
            {
                case MouseHandlingMode.MovingMark:
                    {
                        PanStartingPoint = e.GetPosition(map);
                        break;
                    }

                case MouseHandlingMode.CreatingRoute:
                    {
                        PanStartingPoint = e.GetPosition(map);
                        break;
                    }
            }
        }

        private void Mark_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (mouseHandlingMode == MouseHandlingMode.MovingMark && (e.Timestamp - ClickTime) < 200)
            {
                mouseHandlingMode = MouseHandlingMode.None;
            }

            if (mouseHandlingMode == MouseHandlingMode.CreatingRoute && (e.Timestamp - ClickTime) < 200)
            {
                var lb = sender as MarkItemsControl;
                var mk = lb.SelectedItem as Mark;

                if (TempSegment == null)
                {
                    TempSegment = new MapSegment();
                    map.Children.Add(TempSegment);
                    Panel.SetZIndex(TempSegment, 15);
                    TempSegment.FromLocation = mk.Location;
                    FirstMark = mk;
                    e.Handled = true;
                }
                else
                {
                    Leg nleg = new Leg(FirstMark, mk);
                    legsItemCollection.Add(nleg);
                    TempRoute.Legs.Add(nleg);
                    TempSegment.FromLocation = mk.Location;
                    FirstMark = mk;
                    e.Handled = true;
                }
            }
        }

        private void Mark_PreviewMouseMove(object sender, MouseEventArgs e)
        {

            if (mouseHandlingMode == MouseHandlingMode.Panning || (mouseHandlingMode == MouseHandlingMode.MovingMark || mouseHandlingMode == MouseHandlingMode.CreatingRoute)
                && e.LeftButton == MouseButtonState.Pressed)
            {
                var position = e.GetPosition(map);
                map.TranslateMap(new Point(position.X - PanStartingPoint.X, position.Y - PanStartingPoint.Y));
                PanStartingPoint = position;
            }


        }

        private void MarkItem_OnMouseEnter(object sender, MouseEventArgs e)
        {
            var mki = sender as MarkItem;
            var mk = markItemsControl.ItemFromContainer(mki) as Mark;
            if (mk != null)
            {
                measureLocation = mk.Location;
                fixMeasure = true;
            }
        }

        private void MarkItem_OnMouseLeave(object sender, MouseEventArgs e)
        {
            fixMeasure = false;
        }

        private void BoatItem_OnMouseEnter(object sender, MouseEventArgs e)
        {
            var mi = sender as MapItem;
            var boat = mi.DataContext as Boat;
            if (boat != null)
            {
                measureLocation = boat.Location;
                fixMeasure = true;
                if (mouseHandlingMode == MouseHandlingMode.SettingMeasureStart)
                    measureCenteredOnBoat = true;
            }
        }

        private void BoatItem_OnMouseLeave(object sender, MouseEventArgs e)
        {
            fixMeasure = false;
            if (mouseHandlingMode == MouseHandlingMode.SettingMeasureStart)
                measureCenteredOnBoat = false;
        }

        private void Boat_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ClickTime = e.Timestamp;
            switch (mouseHandlingMode)
            {
                case MouseHandlingMode.CreatingRoute:
                    {
                        PanStartingPoint = e.GetPosition(map);
                        break;
                    }
            }
        }

        private void Boat_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (mouseHandlingMode == MouseHandlingMode.CreatingRoute && (e.Timestamp - ClickTime) < 200)
            {
                var lb = sender as MapItemsControl;
                var boat = lb.SelectedItem as Boat;

                RouteAddMarkAt(boat.Location);
                e.Handled = true;
            }
        }

        void RouteAddMarkAt(Location loc)
        {
            if (TempSegment == null)
            {
                TempSegment = new MapSegment();
                map.Children.Add(TempSegment);
                TempSegment.Stroke = Brushes.Blue;
                TempSegment.StrokeThickness = 3;
                Panel.SetZIndex(TempSegment, 15);
                TempSegment.FromLocation = loc;

                markNumber++;
                FirstMark = new Mark
                {
                    Location = loc,
                    Name = "mk" + markNumber.ToString()
                };

                marksItemCollection.Add(FirstMark);
                NewMarksOnRoute.Add(FirstMark);
            }
            else
            {
                markNumber++;
                Mark mk = new Mark
                {
                    Location = loc,
                    Name = "mk" + markNumber.ToString()
                };

                marksItemCollection.Add(mk);
                NewMarksOnRoute.Add(mk);

                Leg nleg = new Leg(FirstMark, mk);
                legsItemCollection.Add(nleg);
                TempRoute.Legs.Add(nleg);
                TempSegment.FromLocation = mk.Location;
                FirstMark = mk;
            }

            MeasureButton.IsChecked = true;
            measureRange.FromLocation = loc;
            measureCenteredOnBoat = false;
        }

        #endregion

        #region Grib handling

        private void GribSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            double ts = (maxGribTime - minGribTime).TotalSeconds * gribControl.GribSlider.Value / (gribControl.GribSlider.Maximum + 1);
            DateTime dt = minGribTime + TimeSpan.FromSeconds(ts);

            SetGribTime(dt);
        }

        private void GribNowButton_Click(object sender, RoutedEventArgs e)
        {
            SetGribTimeNow();
        }

        private void GribDisplay_Checked(object sender, RoutedEventArgs e)
        {
            if (wgrib != null)
                if ((bool)gribControl.DisplayWind.IsChecked)
                {
                    wagrid.Visibility = Visibility.Visible;
                }
                else
                {
                    wagrid.Visibility = Visibility.Hidden;
                }

            if (cgrib != null)
                if ((bool)gribControl.DisplayCurrent.IsChecked)
                {
                    cagrid.Visibility = Visibility.Visible;
                }
                else
                    cagrid.Visibility = Visibility.Hidden;

        }

        private void SetGribTimeNow()
        {
            DateTime dt = DateTime.Now.ToUniversalTime();

            if (dt >= minGribTime && dt <= maxGribTime)
            {
                gribControl.GribSlider.Value = (dt - minGribTime).TotalSeconds / (maxGribTime - minGribTime).TotalSeconds * (gribControl.GribSlider.Maximum + 1);
            }

            SetGribTime(dt);
        }

        private void SetGribTime(DateTime dt)
        {
            if (wgrib != null)
            {
                if (dt >= wgrib.band[0].datetime && dt <= wgrib.band[wgrib.band.Count - 1].datetime && (bool)gribControl.DisplayWind.IsChecked)
                {
                    wagrid.Update(wgrib, dt);
                    if ((bool)GribButton.IsChecked)
                        wagrid.Visibility = Visibility.Visible;
                }
                else
                    wagrid.Visibility = Visibility.Hidden;
            }

            if (cgrib != null)
            {
                if (dt >= cgrib.band[0].datetime && dt <= cgrib.band[cgrib.band.Count - 1].datetime && (bool)gribControl.DisplayCurrent.IsChecked)
                {
                    cagrid.Update(cgrib, dt);
                    if ((bool)GribButton.IsChecked)
                        cagrid.Visibility = Visibility.Visible;
                }
                else
                    cagrid.Visibility = Visibility.Hidden;
            }

            gribControl.textblock.Text = dt.ToLocalTime().ToString();

        }

        #endregion

        #region Menu

        private void ReplayLogButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.Title = "Replay Log";
            dlg.Filter = "Log files|*.log";
            if (Properties.Settings.Default.LogFile != "")
                dlg.InitialDirectory = System.IO.Path.GetDirectoryName(Properties.Settings.Default.LogFile);
            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                // Read Log
                string filename = dlg.FileName;
                if (logging)
                {
                    LogFile.Close();
                    logging = false;
                }

                replayLog = true;
                ReplayFile = new StreamReader(filename);

                ShortNavTimer.Interval = new TimeSpan(0, 0, 0, 0, 80);
            }

        }

        private void MenuItem_Setup_Click(object sender, RoutedEventArgs e)
        {

            SetupWindow SetupWindow1 = new SetupWindow();

            Nullable<bool> result = SetupWindow1.ShowDialog();

            if (result == true)
            {

                SerialPort1.Close();
                InitializeSerialPort1();
                DataReceiverStatus1.Result = RxTxResult.NoRxData;

                SerialPort2.Close();
                InitializeSerialPort2();
                DataReceiverStatus2.Result = RxTxResult.NoRxData;

                SerialPort3.Close();
                InitializeSerialPort3();
                DataReceiverStatus3.Result = RxTxResult.NoRxData;

                SerialPort4.Close();
                InitializeSerialPort4();
                DataReceiverStatus4.Result = RxTxResult.NoRxData;

                MapLayer1.TileSource = new TileSource();
                MapLayer1.TileSource.UriFormat = "file:\\" + Properties.Settings.Default.Layer1Directory + "\\{z}\\{x}\\{v}.png";
                MapLayer1.Opacity = Properties.Settings.Default.Layer1Opacity;
                MapLayer1.MaxZoomLevel = 18;

                MapLayer2.TileSource = new TileSource();
                MapLayer2.TileSource.UriFormat = "file://" + Properties.Settings.Default.Layer2Directory + "/{z}/{x}/{v}.png";
                MapLayer2.Opacity = Properties.Settings.Default.Layer2Opacity;
                MapLayer2.MaxZoomLevel = 18;

                if (LogFile != null)
                    LogFile.Close();

                try
                {
                    LogFile = new StreamWriter(Properties.Settings.Default.LogFile, true);   // Append to existing LogFile
                    LogFile.AutoFlush = true;
                    LogFile.WriteLine("");
                    logging = false;
                }
                catch { logging = false; }

                SendPTAKheaders();
            }
        }

        private void MenuItem_WLCourseSetup_Click(object sender, RoutedEventArgs e)
        {
            wlCourseSetupWindow.Show();
        }

        private void MenuItem_Polar_Click(object sender, RoutedEventArgs e)
        {

            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.Filter = "Polar files|*.pol";
            try
            {
                dlg.InitialDirectory = System.IO.Path.GetDirectoryName(Properties.Settings.Default.PolarFile);
            }
            catch { };

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                // Read Polar
                string filename = dlg.FileName;
                Properties.Settings.Default.PolarFile = filename;

                StreamReader sr = new StreamReader(filename);
                NavPolar.Load(sr);
                sr.Close();
                SendPTAKheaders();
            }

        }

        //private void MenuItem_InsertComment_Click(object sender, RoutedEventArgs e)
        //{
        //    LogCommentDlg dlg = new LogCommentDlg();

        //    Nullable<bool> result = dlg.ShowDialog();

        //    if (result == true)
        //    {
        //        CommentLogged = true;
        //        Comment = dlg.textBox1.Text;
        //    }
        //}

        private void GPXLoadButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.Filter = "gpx files|*.gpx";
            dlg.InitialDirectory = Properties.Settings.Default.WaypointDirectory;
            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                string filename = dlg.FileName;
                Properties.Settings.Default.WaypointDirectory = System.IO.Path.GetDirectoryName(filename);

                DataSource inputDS;
                Layer layer;
                Feature f;

                inputDS = Ogr.Open(filename, 0);

                marksItemCollection.Clear();

                layer = inputDS.GetLayerByName("waypoints");


                while ((f = layer.GetNextFeature()) != null)
                {
                    OSGeo.OGR.Geometry g = f.GetGeometryRef();
                    Mark mk = new Mark
                    {
                        Location = new Location(g.GetY(0), g.GetX(0)),
                        Name = f.GetFieldAsString("name")
                    };
                    marksItemCollection.Add(mk);
                }

                routeList.Clear();

                layer = inputDS.GetLayerByName("routes");
                while ((f = layer.GetNextFeature()) != null)
                {
                    string name = f.GetFieldAsString("name");
                    Route r = new Route();
                    r.Name = name;
                    routeList.Add(r);
                }

                Mark prevMark = new Mark();

                layer = inputDS.GetLayerByName("route_points");

                int last_fid = 0;
                while ((f = layer.GetNextFeature()) != null)
                {
                    int route_fid = f.GetFieldAsInteger("route_fid");
                    if (last_fid != route_fid)
                        prevMark = new Mark();

                    string name = f.GetFieldAsString("name");
                    Mark currMark = (Mark)(from mx in marksItemCollection where ((Mark)mx).Name == name select mx).First();
                    if (prevMark.Name != null)
                    {
                        Leg lg = new Leg(prevMark, currMark);
                        routeList[route_fid].Legs.Add(lg);
                    }
                    prevMark = currMark;
                    last_fid = route_fid;
                }

                if (routeList.Count() > 0)
                    routeControl.RouteListComboBox.SelectedIndex = 0;
            }
        }

        private void MenuItem_SaveWaypoint_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();

            dlg.Filter = "gpx files|*.gpx";
            dlg.InitialDirectory = Properties.Settings.Default.WaypointDirectory;
            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                string filename = dlg.FileName;
                Properties.Settings.Default.WaypointDirectory = System.IO.Path.GetDirectoryName(filename);


                if (File.Exists(filename))
                    File.Delete(filename);

                SpatialReference wgs84;
                string wkt;
                Osr.GetWellKnownGeogCSAsWKT("WGS84", out wkt);
                wgs84 = new SpatialReference(wkt);

                OSGeo.OGR.Driver drv = Ogr.GetDriverByName("GPX");
                DataSource ds = drv.CreateDataSource(filename, new string[] { });

                //Write Waypoints

                Layer layer1 = ds.CreateLayer("waypoints", wgs84, wkbGeometryType.wkbPoint, new string[] { });
                Feature f1 = new Feature(layer1.GetLayerDefn());
                OSGeo.OGR.Geometry g1 = new OSGeo.OGR.Geometry(wkbGeometryType.wkbPoint);

                foreach (Mark mk in marksItemCollection)
                {
                    f1.SetField("Name", mk.Name);
                    g1.AddPoint_2D(mk.Location.Longitude, mk.Location.Latitude);
                    f1.SetGeometry(g1);
                    layer1.CreateFeature(f1);
                }


                //Write Routes

                Layer layer3 = ds.CreateLayer("route_points", wgs84, wkbGeometryType.wkbPoint, new string[] { });
                Feature f3 = new Feature(layer3.GetLayerDefn());
                OSGeo.OGR.Geometry g3 = new OSGeo.OGR.Geometry(wkbGeometryType.wkbPoint);


                int i = 0;
                foreach (Route rte in routeList)
                {
                    int j = 0;
                    Leg tleg = rte.Legs[0];

                    Mark mk = tleg.FromMark;

                    g3.AddPoint_2D(mk.Location.Longitude, mk.Location.Latitude);
                    f3.SetField("route_fid", i);
                    f3.SetField("route_point_id", j);
                    f3.SetField("route_name", rte.Name);
                    f3.SetField("name", mk.Name);
                    f3.SetGeometry(g3);
                    layer3.CreateFeature(f3);

                    j++;

                    while (tleg != null)
                    {
                        mk = tleg.ToMark;
                        g3.AddPoint_2D(mk.Location.Longitude, mk.Location.Latitude);
                        f3.SetField("route_fid", i);
                        f3.SetField("route_point_id", j);
                        f3.SetField("route_name", rte.Name);
                        f3.SetField("name", mk.Name);
                        f3.SetGeometry(g3);
                        layer3.CreateFeature(f3);
                        j++;
                        tleg = tleg.NextLeg;
                    }
                    i++;
                }
            }
        }

        private void GribWindLoadButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.Filter = "grib files|*.grb";
            dlg.InitialDirectory = Properties.Settings.Default.GribDirectory;
            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {

                string filename = dlg.FileName;
                Properties.Settings.Default.GribDirectory = System.IO.Path.GetDirectoryName(filename);

                Dataset ds = Gdal.Open(filename, Access.GA_ReadOnly);

                wgrib = new windgrib(ref ds);

                Collection<uvpair> uvpairs = new Collection<uvpair>();  // Here uvpairs are used to correlate u&v bands

                for (int i = 1; i <= ds.RasterCount; i++)
                {
                    Band uband = ds.GetRasterBand(i);
                    string sss = uband.GetMetadataItem("GRIB_SHORT_NAME", "");
                    if (uband.GetMetadataItem("GRIB_SHORT_NAME", "") == "10-HTGL")
                    {
                        string hhh = uband.GetMetadataItem("GRIB_ELEMENT", "");
                        if (uband.GetMetadataItem("GRIB_ELEMENT", "") == "UGRD")
                        {
                            string fcst_time = uband.GetMetadataItem("GRIB_FORECAST_SECONDS", "");
                            for (int j = 1; j <= ds.RasterCount; j++)
                            {
                                Band vband = ds.GetRasterBand(j);
                                if (vband.GetMetadataItem("GRIB_SHORT_NAME", "") == "10-HTGL")
                                    if (vband.GetMetadataItem("GRIB_ELEMENT", "") == "VGRD")
                                        if (vband.GetMetadataItem("GRIB_FORECAST_SECONDS", "") == fcst_time)
                                            uvpairs.Add(new uvpair { u = i, v = j });
                            }
                        }
                    }
                }

                foreach (uvpair uv in uvpairs)
                {
                    Band uband = ds.GetRasterBand((int)uv.u);
                    Band vband = ds.GetRasterBand((int)uv.v);

                    gribband wb = new gribband(ds.RasterXSize, ds.RasterYSize);

                    string[] s = uband.GetMetadataItem("GRIB_REF_TIME", "").Trim().Split(' ');
                    long secs = Convert.ToInt64(s[0]);
                    s = uband.GetMetadataItem("GRIB_FORECAST_SECONDS", "").Trim().Split(' ');
                    long offs = Convert.ToInt64(s[0]);
                    DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    wb.datetime = UnixEpoch + TimeSpan.FromMilliseconds(1000 * (secs + offs));

                    for (int j = 0; j < ds.RasterYSize; j++)
                        for (int i = 0; i < ds.RasterXSize; i++)
                        {
                            double[] buffer = new double[1];
                            uvpair valuepair = new uvpair();
                            uband.ReadRaster(i, j, 1, 1, buffer, 1, 1, 1, 1);
                            valuepair.u = buffer[0];
                            vband.ReadRaster(i, j, 1, 1, buffer, 1, 1, 1, 1);
                            valuepair.v = buffer[0];
                            double lat, lon;
                            wgrib.ConvertToLL(i, j, out lon, out lat);
                            valuepair.Lat = lat > 180 ? lat - 360 : lat;
                            valuepair.Lon = lon > 180 ? lon - 360 : lon;
                            wb.data[i, j] = valuepair;
                        }
                    wgrib.band.Add(wb);
                }

                map.Children.Remove(wagrid);
                wagrid = new WindArrowGrid(wgrib, wgrib.DeltaLat * .1);
                map.Children.Add(wagrid);

                maxGribTime = wgrib.band[wgrib.band.Count - 1].datetime;
                minGribTime = wgrib.band[0].datetime;

                if (cgrib != null)
                {
                    if (cgrib.band[cgrib.band.Count - 1].datetime > maxGribTime)
                        maxGribTime = cgrib.band[cgrib.band.Count - 1].datetime;
                    if (cgrib.band[0].datetime < minGribTime)
                        minGribTime = cgrib.band[0].datetime;
                }

                gribControl.DisplayWind.IsChecked = true;

                if (GribButton.IsChecked == false)
                {
                    GribButton.IsChecked = true;
                }
                SetGribTimeNow();


            }
        }

        private void GribCurrentLoadButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.Filter = "grib files|*.grb";
            dlg.InitialDirectory = Properties.Settings.Default.GribDirectory;
            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {

                string filename = dlg.FileName;
                Properties.Settings.Default.GribDirectory = System.IO.Path.GetDirectoryName(filename);

                Dataset ds = Gdal.Open(filename, Access.GA_ReadOnly);

                cgrib = new currentgrib(ref ds);

                Collection<uvpair> uvpairs = new Collection<uvpair>();  // Here uvpairs are used to correlate u&v bands

                for (int i = 1; i <= ds.RasterCount; i++)
                {
                    Band uband = ds.GetRasterBand(i);
                    string sss = uband.GetMetadataItem("GRIB_SHORT_NAME", "");
                    if (uband.GetMetadataItem("GRIB_SHORT_NAME", "") == "0-SFC")
                    {
                        string hhh = uband.GetMetadataItem("GRIB_ELEMENT", "");
                        if (uband.GetMetadataItem("GRIB_ELEMENT", "") == "UOGRD")
                        {
                            string fcst_time = uband.GetMetadataItem("GRIB_FORECAST_SECONDS", "");
                            for (int j = 1; j <= ds.RasterCount; j++)
                            {
                                Band vband = ds.GetRasterBand(j);
                                if (vband.GetMetadataItem("GRIB_SHORT_NAME", "") == "0-SFC")
                                    if (vband.GetMetadataItem("GRIB_ELEMENT", "") == "VOGRD")
                                        if (vband.GetMetadataItem("GRIB_FORECAST_SECONDS", "") == fcst_time)
                                            uvpairs.Add(new uvpair { u = i, v = j });
                            }
                        }
                    }
                }


                foreach (uvpair uv in uvpairs)
                {
                    Band uband = ds.GetRasterBand((int)uv.u);
                    Band vband = ds.GetRasterBand((int)uv.v);

                    gribband cb = new gribband(ds.RasterXSize, ds.RasterYSize);

                    string[] s = uband.GetMetadataItem("GRIB_REF_TIME", "").Trim().Split(' ');
                    long secs = Convert.ToInt64(s[0]);
                    s = uband.GetMetadataItem("GRIB_FORECAST_SECONDS", "").Trim().Split(' ');
                    long offs = Convert.ToInt64(s[0]);
                    DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    cb.datetime = UnixEpoch + TimeSpan.FromMilliseconds(1000 * (secs + offs));

                    for (int j = 0; j < ds.RasterYSize; j++)
                        for (int i = 0; i < ds.RasterXSize; i++)
                        {
                            double[] buffer = new double[1];
                            uvpair valuepair = new uvpair();
                            uband.ReadRaster(i, j, 1, 1, buffer, 1, 1, 1, 1);
                            valuepair.u = buffer[0];
                            vband.ReadRaster(i, j, 1, 1, buffer, 1, 1, 1, 1);
                            valuepair.v = buffer[0];
                            double lat, lon;
                            cgrib.ConvertToLL(i, j, out lon, out lat);
                            valuepair.Lat = lat > 180 ? lat - 360 : lat;
                            valuepair.Lon = lon > 180 ? lon - 360 : lon;
                            if (valuepair.u != 9999 && valuepair.v != 9999)
                                cb.data[i, j] = valuepair;
                        }
                    cgrib.band.Add(cb);
                }

                map.Children.Remove(cagrid);
                cagrid = new CurrentArrowGrid(cgrib, cgrib.DeltaLat);
                map.Children.Add(cagrid);

                maxGribTime = cgrib.band[cgrib.band.Count - 1].datetime;
                minGribTime = cgrib.band[0].datetime;

                if (wgrib != null)
                {
                    if (wgrib.band[wgrib.band.Count - 1].datetime > maxGribTime)
                        maxGribTime = wgrib.band[wgrib.band.Count - 1].datetime;
                    if (wgrib.band[0].datetime < minGribTime)
                        minGribTime = wgrib.band[0].datetime;
                }

                gribControl.DisplayCurrent.IsChecked = true;

                if (GribButton.IsChecked == false)
                {
                    GribButton.IsChecked = true;
                }

                SetGribTimeNow();
            }

        }

        #endregion

        #region Commands

        private void MoveMark(object sender, RoutedEventArgs e)
        {
            MenuItem mi = sender as MenuItem;
            if (mi != null)
            {
                ContextMenu cm = mi.CommandParameter as ContextMenu;
                if (cm != null)
                {
                    var miCtrl = cm.PlacementTarget as MapItemsControl;

                    MovingMark = (Mark)miCtrl.SelectedItem;
                    mouseHandlingMode = MouseHandlingMode.MovingMark;

                }
            }

        }

        private void LegInsertMark(object sender, RoutedEventArgs e)
        {
            MenuItem mi = sender as MenuItem;

            if (mi != null)
            {
                ContextMenu cm = mi.CommandParameter as ContextMenu;
                if (cm != null)
                {
                    var miCtrl = cm.PlacementTarget as MapItemsControl;
                    Leg lg = (Leg)miCtrl.SelectedItem;

                    markNumber++;
                    Mark mk = new Mark
                    {
                        Location = new Location(0, 0),
                        Name = "mk" + markNumber.ToString()
                    };

                    Leg nleg = new Leg(mk, lg.ToMark);
                    lg.ToMark = mk;
                    nleg.NextLeg = lg.NextLeg;
                    nleg.PreviousLeg = lg;
                    lg.NextLeg = nleg;
                    if (nleg.NextLeg != null)
                        nleg.NextLeg.PreviousLeg = nleg;

                    foreach (Route rte in routeList)
                        for (int i = 0; i < rte.Legs.Count; i++)
                        {
                            if (rte.Legs[i] == lg)
                            {
                                rte.Legs.Insert(i + 1, nleg);
                                break;
                            }
                        }

                    if (lg == ActiveLeg)
                        ActiveLeg = nleg;

                    marksItemCollection.Add(mk);
                    legsItemCollection.Add(nleg);

                    MovingMark = mk;
                    mouseHandlingMode = MouseHandlingMode.MovingMark;
                }
            }
        }

        private void CreateRoute(object sender, RoutedEventArgs e)
        {
            mouseHandlingMode = MouseHandlingMode.None;
            markItemsControl.ContextMenu = (ContextMenu)Resources["MarkContextMenu_Standard"];
            map.ContextMenu = (ContextMenu)Resources["MapContextMenu_Standard"];

            this.Cursor = Cursors.Arrow;

            map.Children.Remove(TempSegment);

            MeasureButton.IsChecked = false;
            measureRange.Visibility = Visibility.Hidden;
        }

        private void CancelCreateRoute(object sender, RoutedEventArgs e)
        {
            mouseHandlingMode = MouseHandlingMode.None;
            markItemsControl.ContextMenu = (ContextMenu)Resources["MarkContextMenu_Standard"];
            map.ContextMenu = (ContextMenu)Resources["MapContextMenu_Standard"];

            this.Cursor = Cursors.Arrow;

            foreach (Leg lg in TempRoute.Legs)
            {
                lg.PreviousLeg = null;
                lg.NextLeg = null;
                legsItemCollection.Remove(lg);
            }

            foreach (Mark mk in NewMarksOnRoute)
                marksItemCollection.Remove(mk);

            map.Children.Remove(TempSegment);

            routeList.Remove(TempRoute);
            routeNumber--;

            if (ActiveRoute != null)
                routeControl.RouteListComboBox.SelectedItem = ActiveRoute;
            else
                routeControl.RouteListComboBox.SelectedIndex = 0;

            MeasureButton.IsChecked = false;

        }

        private void AddMarkCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            markNumber++;

            Mark mk = new Mark
            {
                Location = new Location(0, 0),
                Name = "mk" + markNumber.ToString()
            };

            marksItemCollection.Add(mk);

            MovingMark = mk;
            mouseHandlingMode = MouseHandlingMode.MovingMark;

        }

        private void AddMarkCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void DeleteMarkCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var mk = e.Parameter as Mark;
            List<Route> routesToDel = new List<Route>();

            foreach (Route rte in routeList)
            {
                if (rte.Legs.Count > 0)
                {
                    Leg lg = rte.Legs[0];

                    if (lg.FromMark == mk)
                    {
                        legsItemCollection.Remove(lg);
                        rte.Legs.Remove(lg);
                        if (lg.NextLeg != null)
                            lg.NextLeg.PreviousLeg = null;
                        else
                            routesToDel.Add(rte);
                    }

                    while (lg != null)
                    {
                        if (lg.ToMark == mk)
                        {
                            var nleg = lg.NextLeg;

                            if (nleg != null)
                            {
                                lg.ToMark = nleg.ToMark;
                                lg.NextLeg = nleg.NextLeg;
                                if (nleg.NextLeg != null)
                                    nleg.NextLeg.PreviousLeg = lg;
                                legsItemCollection.Remove(nleg);
                                rte.Legs.Remove(nleg);
                            }
                            else
                            {
                                var pleg = lg.PreviousLeg;
                                if (pleg != null)
                                {
                                    legsItemCollection.Remove(lg);
                                    rte.Legs.Remove(lg);
                                    pleg.NextLeg = null;
                                }
                                else
                                {
                                    legsItemCollection.Remove(lg);
                                    rte.Legs.Remove(lg);
                                    routesToDel.Add(rte);
                                }
                            }
                        }
                        lg = lg.NextLeg;
                    }
                }
            }

            marksItemCollection.Remove(mk);

            foreach (Route r in routesToDel)
                routeList.Remove(r);
        }

        private void DeleteMarkCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            var mk = e.Parameter as Mark;

            if (ActiveLeg != null)
            {
                if (mk != ActiveLeg.FromMark && mk != ActiveLeg.ToMark)
                    e.CanExecute = true;
                else
                    e.CanExecute = false;
            }
            else
                if (mk != ActiveMark)
                e.CanExecute = true;
            else
                e.CanExecute = false;

            e.Handled = true;
        }

        private void NavigateToCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var mk = e.Parameter as Mark;

            ActiveMark = mk;

            if (ActiveRoute != null)
            {
                ActiveLeg = ActiveRoute.Legs.FirstOrDefault(lg => lg.ToMark == mk);
                if (ActiveLeg == null)
                    ActiveRoute = null;
            }
        }

        private void NavigateToCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void ActivateRouteCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            var si = routeControl.RouteListComboBox.SelectedItem;
            e.CanExecute = (si != null) && (si != ActiveRoute);
            e.Handled = true;
        }

        private void ActivateRouteCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var cb = e.Parameter as ComboBox;
            ActiveRoute = cb.SelectedItem as Route;
            ActiveLeg = ActiveRoute.Legs[0];
            ActiveMark = (Mark)ActiveLeg.ToMark;
        }

        public void RouteCtrlCommandReceived(object sender, RouteCtrlEventArgs e)
        {
            switch (e.Command)
            {
                #region SelectionChanged
                case RouteCtrlCmd.SelectionChanged:
                    {
                        var newRoute = e.RouteTarget as Route;

                        if (newRoute != null)
                        {
                            routeControl.DataGrid1.DataContext = newRoute.Legs;

                            ICollection<object> legsItemCollection = (ICollection<object>)Resources["LegsItemCollection"];
                            legsItemCollection.Clear();

                            foreach (Leg lg in newRoute.Legs)
                            {
                                legsItemCollection.Add(lg);
                            }
                        }
                        else
                            routeControl.DataGrid1.DataContext = null;

                        break;
                    }
                #endregion

                #region Hiding
                case RouteCtrlCmd.Hiding:
                    {
                        RouteButton.IsChecked = false;
                        break;
                    }
                    #endregion
            }
        }

        public void MarkCtrlCommandReceived(object sender, MarkCtrlEventArgs e)
        {
            switch (e.Command)
            {
                #region Hiding
                case MarkCtrlCmd.Hiding:
                    {
                        MarkButton.IsChecked = false;
                        break;
                    }
                    #endregion
            }
        }

        public void LayerCtrlCommandReceived(object sender, LayerControlWindow.LayerCtrlEventArgs e)
        {
            switch (e.Command)
            {
                case LayerControlWindow.LayerCtrlCmd.Hiding:
                    {
                        LayersButton.IsChecked = false;
                        break;
                    }

                case LayerControlWindow.LayerCtrlCmd.LaylinesChanged:
                    {
                        if (e.Visible == Visibility.Hidden)
                        {
                            laylinesVisible = false;
                            break;
                        }
                        else
                        {
                            laylinesVisible = true;
                            break;
                        }
                    }

                case LayerControlWindow.LayerCtrlCmd.TargetBearingsChanged:
                    {
                        if (e.Visible == Visibility.Hidden)
                        {
                            bearingTargetsVisible = false;
                            break;
                        }
                        else
                        {
                            bearingTargetsVisible = true;
                            break;
                        }
                    }

                case LayerControlWindow.LayerCtrlCmd.Layer1Changed:
                    if (e.Visible == Visibility.Hidden)
                    {
                        map.TileLayers.Remove(MapLayer1);
                        break;
                    }
                    else
                    {
                        map.TileLayers.Insert(0, MapLayer1);
                        break;
                    }

                case LayerControlWindow.LayerCtrlCmd.Layer2Changed:
                    if (e.Visible == Visibility.Hidden)
                    {
                        map.TileLayers.Remove(MapLayer2);
                        break;
                    }
                    else
                    {
                        map.TileLayers.Add(MapLayer2);
                        break;
                    }

                case LayerControlWindow.LayerCtrlCmd.Layer1OpacityChanged:
                    {
                        MapLayer1.Opacity = e.Value;
                        break;
                    }

                case LayerControlWindow.LayerCtrlCmd.Layer2OpacityChanged:
                    {
                        MapLayer2.Opacity = e.Value;
                        break;
                    }

                case LayerControlWindow.LayerCtrlCmd.TrackResolutionChanged:
                    {
                        map.Children.Remove(track);

                        using (var context = new LionRiverDBContext())
                        {
                            var dt1 = DateTime.Now.AddYears(-5);
                            var res = Properties.Settings.Default.TrackResolution;

                            var lastLogEntries = (from x in context.Logs
                                                  where (x.level == res && (x.timestamp > dt1))
                                                  orderby x.timestamp descending
                                                  select x).Take(Track.MaxLength).ToList();


                            track = new Track(lastLogEntries, Properties.Settings.Default.TrackResolution, Properties.Settings.Default.SPDminVal,
                                                Properties.Settings.Default.SPDminIndex, Properties.Settings.Default.SPDmaxVal, Properties.Settings.Default.SPDmaxIndex);
                        }
                        map.Children.Add(track);
                        Panel.SetZIndex(track, 5);

                        break;
                    }
            }
        }

        private void StopNavCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ActiveRoute = null;
            ActiveLeg = null;
            ActiveMark = null;
        }

        private void StopNavCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (ActiveMark != null);
            e.Handled = true;
        }

        private void NewRouteCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            TempRoute = new Route();
            TempRoute.Name = "Route " + routeNumber.ToString();
            routeNumber++;

            routeList.Add(TempRoute);
            routeControl.RouteListComboBox.SelectedItem = TempRoute;

            TempSegment = null;
            NewMarksOnRoute.Clear();

            markItemsControl.ContextMenu = (ContextMenu)Resources["ContextMenu_CreatingRoute"];
            map.ContextMenu = (ContextMenu)Resources["ContextMenu_CreatingRoute"];

            this.Cursor = Cursors.Cross;

            mouseHandlingMode = MouseHandlingMode.CreatingRoute;

            Properties.Settings.Default.RouteVisible = true;

            e.Handled = true;
        }

        private void NewRouteCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (mouseHandlingMode != MouseHandlingMode.CreatingRoute);
            e.Handled = true;
        }

        private void FwdRouteCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (ActiveLeg != null)
            {
                if (ActiveLeg.NextLeg != null)
                {
                    ActiveLeg = ActiveLeg.NextLeg;
                    ActiveMark = ActiveLeg.ToMark;
                }
            }
            e.Handled = true;

        }

        private void FwdRouteCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (ActiveLeg != null)
                e.CanExecute = ActiveLeg.NextLeg != null;
            else
                e.CanExecute = false;
            e.Handled = true;
        }

        private void RwdRouteCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (ActiveLeg != null)
            {
                if (ActiveLeg.PreviousLeg != null)
                {
                    ActiveLeg = ActiveLeg.PreviousLeg;
                    ActiveMark = ActiveLeg.ToMark;
                }
            }
            e.Handled = true;
        }

        private void RwdRouteCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (ActiveLeg != null)
                e.CanExecute = ActiveLeg.PreviousLeg != null;
            else
                e.CanExecute = false;
            e.Handled = true;
        }

        private void ReverseRouteCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var SelectedRoute = routeControl.RouteListComboBox.SelectedItem as Route;

            Route InvertedRoute = new Route();

            var tleg = SelectedRoute.Legs[0]; // temporary leg
            while (tleg.NextLeg != null)
                tleg = tleg.NextLeg;

            Leg npleg = null;   // new previous Leg
            Leg nleg;           // new Leg

            while (tleg != null)
            {
                nleg = new Leg(tleg.ToMark, tleg.FromMark); // new leg
                nleg.PreviousLeg = npleg;    // Link to previous
                if (npleg != null)
                    npleg.NextLeg = nleg;    // Link previous to current as nextleg
                InvertedRoute.Legs.Add(nleg);
                legsItemCollection.Add(nleg);
                npleg = nleg;
                tleg = tleg.PreviousLeg;
            }

            InvertedRoute.Name = SelectedRoute.Name;

            routeList.Remove(SelectedRoute);
            routeList.Add(InvertedRoute);
            routeControl.RouteListComboBox.SelectedItem = InvertedRoute;

            foreach (Leg l in SelectedRoute.Legs)
            {
                l.PreviousLeg = null;
                l.NextLeg = null;
                legsItemCollection.Remove(l);
            }


            e.Handled = true;
        }

        private void ReverseRouteCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            var si = routeControl.RouteListComboBox.SelectedItem;
            e.CanExecute = (si != null) && (si != ActiveRoute);
            e.Handled = true;
        }

        private void DeleteRouteCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var SelectedRoute = routeControl.RouteListComboBox.SelectedItem as Route;

            foreach (Leg lg in SelectedRoute.Legs)
            {
                legsItemCollection.Remove(lg);
            }

            routeList.Remove(SelectedRoute);

            e.Handled = true;
        }

        private void DeleteRouteCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            var si = routeControl.RouteListComboBox.SelectedItem;
            e.CanExecute = (si != null) && (si != ActiveRoute);
            e.Handled = true;
        }

        private void SetLineBoatCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (LAT.IsValid() && HDT.IsValid())
            {
                p1_lat = LAT.Val;
                p1_lon = LON.Val;

                if (Properties.Settings.Default.GPSoffsetToBow != 0)
                    CalcPosition(LAT.Val, LON.Val, Properties.Settings.Default.GPSoffsetToBow, HDT.Val, ref p1_lat, ref p1_lon);

                p1_set = true;

                if (p2_set)
                    linebrg = CalcBearing(p1_lat, p1_lon, p2_lat, p2_lon);

                wlCourseSetupWindow.SetLinePinButton.Background = Brushes.Lime;
            }
            e.Handled = true;
        }

        private void SetLinePinCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (LAT.IsValid() && HDT.IsValid())
            {
                p2_lat = LAT.Val;
                p2_lon = LON.Val;

                if (Properties.Settings.Default.GPSoffsetToBow != 0)
                    CalcPosition(LAT.Val, LON.Val, Properties.Settings.Default.GPSoffsetToBow, HDT.Val, ref p2_lat, ref p2_lon);

                //lineBoatMark.Location.Latitude = p2_lat;
                //lineBoatMark.Location.Longitude = p2_lon;                

                p2_set = true;

                if (p1_set)
                    linebrg = CalcBearing(p1_lat, p1_lon, p2_lat, p2_lon);

                wlCourseSetupWindow.SetLineBoatButton.Background = Brushes.Lime;
            }
            e.Handled = true;
        }

        private void RemoveInstrumentCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var z = e.Source as UserControl;

            if (z != null)
                if (InstrumentStackPanel.Children.Contains(z))
                    InstrumentStackPanel.Children.Remove(z);

            Properties.Settings.Default.InstrumentDisplayList = GetCurrentInstrumentDisplayList();
            Properties.Settings.Default.Save();

            e.Handled = true;
        }

        private void SelectFleetBoatCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var fboat = (e.Source as MapItem).DataContext as Boat;

            fboat.IsSelected = true;

            if (PlayButton.IsChecked == false)
            {
                DateTime startTime = new DateTime((long)NavPlotModel.SelectionFromValue);
                DateTime endTime = new DateTime((long)NavPlotModel.SelectionToValue);

                UpdateTracks(startTime, endTime);
            }

            e.Handled = true;
        }

        private void SelectFleetBoatCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            var fboat = (e.Source as MapItem).DataContext as Boat;

            if (fboat.IsSelected)
                e.CanExecute = false;
            else
                e.CanExecute = true;

            e.Handled = true;

        }

        private void UnselectFleetBoatCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var fboat = (e.Source as MapItem).DataContext as Boat;

            fboat.IsSelected = false;

            if (PlayButton.IsChecked == false)
            {
                DateTime startTime = new DateTime((long)NavPlotModel.SelectionFromValue);
                DateTime endTime = new DateTime((long)NavPlotModel.SelectionToValue);

                UpdateTracks(startTime, endTime);
            }

            e.Handled = true;
        }

        private void UnselectFleetBoatCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            var fboat = (e.Source as MapItem).DataContext as Boat;

            if (fboat.IsSelected == false)
                e.CanExecute = false;
            else
                e.CanExecute = true;

            e.Handled = true;

        }

        private void HideUnselectedFleetBoatsCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            foreach (Boat b in fleetBoats)
            {
                if (b.IsSelected == false)
                    b.BoatVisible = Visibility.Hidden;
            }

            e.Handled = true;
        }

        private void UnhideAllFleetBoatsCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            foreach (Boat b in fleetBoats)
                b.BoatVisible = Visibility.Visible;

            e.Handled = true;
        }

        private void UnselectAllFleetBoatsCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            foreach (Boat b in fleetBoats)
                b.IsSelected = false;

            if (PlayButton.IsChecked == false)
            {
                DateTime startTime = new DateTime((long)NavPlotModel.SelectionFromValue);
                DateTime endTime = new DateTime((long)NavPlotModel.SelectionToValue);

                UpdateTracks(startTime, endTime);
            }


            e.Handled = true;
        }

        private void CalcRegattaCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (ActiveRoute != null)
            {

                Regatta = new Regatta()
                {
                    Name = "Regatta",
                    Start = new DateTime((long)NavPlotModel.SelectionFromValue),
                    End = new DateTime((long)NavPlotModel.SelectionToValue)
                };

                using (var context = new LionRiverDBContext())
                {

                    var boatList =
                        (from b in context.FleetTracks
                         where b.timestamp > Regatta.Start && b.timestamp < Regatta.End
                         select b.Name).Distinct();
                }
            
            
            }
            else
            {

                string messageBoxText = "No Active Route";
                string caption = "LionRiver";
                MessageBoxButton button = MessageBoxButton.OK;
                MessageBoxImage icon = MessageBoxImage.Warning;

                MessageBox.Show(messageBoxText, caption, button, icon);

            }


            e.Handled = true;
        }

        #endregion

        #region UI Events

        private void MarkButton_Click(object sender, RoutedEventArgs e)
        {
            marksControl.Show();
        }

        private void RouteButton_Checked(object sender, RoutedEventArgs e)
        {
            routeControl.Show();
        }

        private void RouteButton_Unchecked(object sender, RoutedEventArgs e)
        {
            routeControl.Hide();
        }

        private void LayersButton_Checked(object sender, RoutedEventArgs e)
        {
            layerControl.Show();
        }

        private void LayersButton_Unchecked(object sender, RoutedEventArgs e)
        {
            layerControl.Hide();
        }

        private void MarkButton_Checked(object sender, RoutedEventArgs e)
        {
            marksControl.Show();
        }

        private void MarkButton_Unchecked(object sender, RoutedEventArgs e)
        {
            marksControl.Hide();
        }

        private void OrientationButton_Checked(object sender, RoutedEventArgs e)
        {
            mapOrientationMode = MapOrientationMode.CourseUp;
            mapCenterMode = MapCenterMode.Centered;
            CenterButton.IsChecked = true;
            boat.Heading = 0;
        }

        private void OrientationButton_Unchecked(object sender, RoutedEventArgs e)
        {
            mapOrientationMode = MapOrientationMode.NorthUp;
            map.TargetHeading = 0;
        }

        private void CenterButton_Checked(object sender, RoutedEventArgs e)
        {
            mapCenterMode = MapCenterMode.Centered;

            if (PlayButton.IsChecked == false)
            {
                DateTime dt = new DateTime((long)NavPlotModel.CurrentValue);
                UpdateFleet(dt);
            }
        }

        private void CenterButton_Unchecked(object sender, RoutedEventArgs e)
        {
            mapCenterMode = MapCenterMode.NotCentered;
        }

        private void MeasureButton_Checked(object sender, RoutedEventArgs e)
        {
            if (mouseHandlingMode != MouseHandlingMode.CreatingRoute)
            {
                mouseHandlingMode = MouseHandlingMode.SettingMeasureStart;
                this.Cursor = Cursors.Cross;
            }
            else
            {
                measureRange.ToLocation = POS.Val;
                measureRange.Visibility = Visibility.Visible;
                mapMeasureControl.Visibility = Visibility.Visible;
            }
        }

        private void MeasureButton_Unchecked(object sender, RoutedEventArgs e)
        {
            measureRange.Visibility = Visibility.Hidden;
            mapMeasureControl.Visibility = Visibility.Hidden;
            if (mouseHandlingMode == MouseHandlingMode.SettingMeasureStart)
            {
                mouseHandlingMode = MouseHandlingMode.None;
                this.Cursor = Cursors.Arrow;
            }
            measureCenteredOnBoat = false;
        }

        private void GribButton_Checked(object sender, RoutedEventArgs e)
        {
            if (wgrib != null)
            {
                if ((bool)gribControl.DisplayWind.IsChecked)
                    wagrid.Visibility = Visibility.Visible;
            }

            if (cgrib != null)
            {
                if ((bool)gribControl.DisplayCurrent.IsChecked)
                    cagrid.Visibility = Visibility.Visible;
            }

            MapGrid.Children.Remove(routeCalculationControl);
            RouteCalcButton.IsChecked = false;
            MapGrid.Children.Add(gribControl);
        }

        private void GribButton_Unchecked(object sender, RoutedEventArgs e)
        {
            if (wgrib != null)
            {
                wagrid.Visibility = Visibility.Hidden;
            }

            if (cgrib != null)
            {
                cagrid.Visibility = Visibility.Hidden;
            }

            MapGrid.Children.Remove(gribControl);
        }

        private void RouteCalcButton_Checked(object sender, RoutedEventArgs e)
        {
            MapGrid.Children.Remove(gribControl);
            GribButton.IsChecked = false;
            MapGrid.Children.Add(routeCalculationControl);
        }

        private void RouteCalcButton_Unchecked(object sender, RoutedEventArgs e)
        {
            MapGrid.Children.Remove(routeCalculationControl);
        }

        private void PlayButton_Checked(object sender, RoutedEventArgs e)
        {
            DateTime dt;

            if (POS.IsValid())
                dt = POS.GetLastVal(0).Time;
            else
                dt = DateTime.Now;

            NavPlotModel.CurrentValue = dt.Ticks;

            double deltaT = NavPlotModel.MaxXAxisValue - NavPlotModel.MinXAxisValue;
            NavPlotModel.MinXAxisValue = (NavPlotModel.CurrentValue - 0.8 * deltaT);
            NavPlotModel.MaxXAxisValue = (NavPlotModel.CurrentValue + 0.2 * deltaT);

            UpdateTracks(new DateTime((long)NavPlotModel.MinXAxisValue), dt, Track.MaxLength);

            UpdateFleet(dt);

            if (FwdBackSlider != null)
                FwdBackSlider.Value = 0;

            ReplayTimer.Stop();

            UpdatePlotResolutionTimer.Start();

            PlotCenterButton.IsChecked = false;

            NavPlotModel.SelectionVisible = Visibility.Hidden;

            //Center on boat if position available
            if (POS.IsValid())
            {
                Point p = map.LocationToViewportPoint(new Location(LAT.Val, LON.Val));
                map.TargetCenter = map.ViewportPointToLocation(new Point(p.X, p.Y));
            }

        }

        private void FwdBackSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (FwdBackSlider.Value != 0)
            {
                if (ReplayTimer.IsEnabled == false)
                    ReplayTimer.Start();
            }
            else
                ReplayTimer.Stop();
        }

        private void MainNavPlot_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed)
            {
                PanStartingPoint = MainNavPlot.Chart.ConvertToChartValues(e.GetPosition(MainNavPlot.Chart));

                NavPlotModel.SelectionVisible = Visibility.Visible;

                ClickTime = e.Timestamp;
                mouseHandlingMode = MouseHandlingMode.SelectingPlotRange;

                e.Handled = true;
            }

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                ClickTime = e.Timestamp;
                PanStartingPoint = e.GetPosition(MainNavPlot.Chart);
                mouseHandlingMode = MouseHandlingMode.PlotPanning;
            }

        }

        private void MainNavPlot_MouseMove(object sender, MouseEventArgs e)
        {
            var pos = e.GetPosition(MainNavPlot.Chart);
            var point = MainNavPlot.Chart.ConvertToChartValues(pos);

            NavPlotModel.CursorValue = point.X;

            if (mouseHandlingMode == MouseHandlingMode.SelectingPlotRange)
            {
                if ((e.Timestamp - ClickTime) > 250)
                {
                    NavPlotModel.SelectionFromValue = PanStartingPoint.X;
                    NavPlotModel.SelectionToValue = point.X;
                }
                e.Handled = true;
            }

            if (mouseHandlingMode == MouseHandlingMode.PlotPanning)
            {

                NavPlotModel.CursorVisible = Visibility.Hidden;

                double deltaX = PanStartingPoint.X - pos.X;

                var dx = ChartFunctions.FromPlotArea(deltaX, LiveCharts.AxisOrientation.X, MainNavPlot.Chart.Model, 0) -
                             ChartFunctions.FromPlotArea(0, LiveCharts.AxisOrientation.X, MainNavPlot.Chart.Model, 0);

                NavPlotModel.MinXAxisValue += dx;
                NavPlotModel.MaxXAxisValue += dx;

                PanStartingPoint = pos;

                PlayButton.IsChecked = false;

                if (PlotCenterButton.IsChecked == true)
                {
                    NavPlotModel.CurrentValue = (NavPlotModel.MinXAxisValue + NavPlotModel.MaxXAxisValue) / 2;

                    if (PlotPanTimer.IsEnabled == false)
                        PlotPanTimer.Start();
                }
            }
        }

        private void MainNavPlot_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var point = MainNavPlot.Chart.ConvertToChartValues(e.GetPosition(MainNavPlot.Chart));

            if (mouseHandlingMode == MouseHandlingMode.SelectingPlotRange)
            {
                if ((e.Timestamp - ClickTime) < 250)
                {
                    if (point.X > NavPlotModel.SelectionFromValue && point.X < NavPlotModel.SelectionToValue)
                    {
                        MainNavPlot.SelectionContextMenu.PlacementTarget = MainNavPlot;
                        MainNavPlot.SelectionContextMenu.IsOpen = true;
                    }

                    mouseHandlingMode = MouseHandlingMode.None;
                }
                else
                {
                    DateTime startTime = new DateTime((long)NavPlotModel.SelectionFromValue);
                    DateTime endTime = new DateTime((long)NavPlotModel.SelectionToValue);

                    UpdateTracks(startTime, endTime);

                    mouseHandlingMode = MouseHandlingMode.None;

                    PlayButton.IsChecked = false;

                    e.Handled = true;
                }
            }

            if (mouseHandlingMode == MouseHandlingMode.PlotPanning)
                if ((e.Timestamp - ClickTime) < 300)
                {

                    mouseHandlingMode = MouseHandlingMode.None;

                    PlayButton.IsChecked = false;

                    if (PlotCenterButton.IsChecked == true)
                    {
                        double deltaT = NavPlotModel.MaxXAxisValue - NavPlotModel.MinXAxisValue;

                        NavPlotModel.MaxXAxisValue = point.X + deltaT / 2;
                        NavPlotModel.MinXAxisValue = point.X - deltaT / 2;
                    }

                    NavPlotModel.CurrentValue = point.X;

                    if (PlotPanTimer.IsEnabled == false)
                        PlotPanTimer.Start();
                }
                else
                {
                    NavPlotModel.CursorValue = point.X;
                    NavPlotModel.CursorVisible = Visibility.Visible;

                    mouseHandlingMode = MouseHandlingMode.None;

                    UpdatePlotResolutionTimer.Start();

                }

        }

        private void MainNavPlot_MouseWheel(object sender, MouseWheelEventArgs e)
        {

            const double zoomFactor = 0.85;

            double zoomPos;
            double zoom;



            if (PlotCenterButton.IsChecked == true || PlayButton.IsChecked == true)
                zoomPos = NavPlotModel.CurrentValue;
            else
                zoomPos = NavPlotModel.CursorValue;

            double deltaT1 = zoomPos - NavPlotModel.MinXAxisValue;
            double deltaT2 = zoomPos - NavPlotModel.MaxXAxisValue;


            if (e.Delta > 0)
                zoom = zoomFactor;
            else
                zoom = 1 / zoomFactor;

            NavPlotModel.MinXAxisValue += deltaT1 * (1 - zoom);
            NavPlotModel.MaxXAxisValue += deltaT2 * (1 - zoom);

            if (PlotCenterButton.IsChecked == true)
            {
                var point = MainNavPlot.Chart.ConvertToChartValues(e.GetPosition(MainNavPlot.Chart));
                NavPlotModel.CursorValue = point.X;
            }

            UpdatePlotResolutionTimer.Start();
        }

        private void MainNavPlot_MouseEnter(object sender, MouseEventArgs e)
        {
            NavPlotModel.CursorVisible = Visibility.Visible;

        }

        private void MainNavPlot_MouseLeave(object sender, MouseEventArgs e)
        {
            NavPlotModel.CursorVisible = Visibility.Hidden;
        }

        private void PlotCenterButton_Checked(object sender, RoutedEventArgs e)
        {
            double deltaT = NavPlotModel.MaxXAxisValue - NavPlotModel.MinXAxisValue;

            NavPlotModel.MaxXAxisValue = NavPlotModel.CurrentValue + deltaT / 2;
            NavPlotModel.MinXAxisValue = NavPlotModel.CurrentValue - deltaT / 2;

            PlayButton.IsChecked = false;
        }

        private void MainPlotSelectionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MainPlotValues.Clear();
            UpdatePlotResolutionTimer.Start();
        }

        private void AuxPlotSelectionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AuxPlotValues.Clear();
            UpdatePlotResolutionTimer.Start();
        }

        private void InstrumentStackContextMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var mi = sender as MenuItem;
            var kvp = (KeyValuePair<string, UserControl>)(mi.DataContext);
            var uc = kvp.Value;

            if (uc != null && !InstrumentStackPanel.Children.Contains(uc))
                InstrumentStackPanel.Children.Add(uc);

            Properties.Settings.Default.InstrumentDisplayList = GetCurrentInstrumentDisplayList();
            Properties.Settings.Default.Save();
        }

        private string GetCurrentInstrumentDisplayList()
        {
            string s = "";

            foreach (UserControl x in InstrumentStackPanel.Children)
            {
                var key = InstrumentDisplays.FirstOrDefault(z => z.Value == x).Key;
                if (key != null)
                    s += key + ",";
            }
            return s;
        }

        private void InstrumentStackPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            PanStartingPoint = e.GetPosition(InstrumentStackPanel);

            hitTestUI = null;

            VisualTreeHelper.HitTest(InstrumentStackPanel, null,
                new HitTestResultCallback(MyHitTestResult),
                new PointHitTestParameters(PanStartingPoint));

            if (hitTestUI != null)
            {
                MovingInstrument = hitTestUI;
                PanStartingPoint = e.GetPosition(hitTestUI);
                Point pcanvas = e.GetPosition(InstrumentCanvas);
                DummyInstrument.Visibility = Visibility.Visible;
                DummyInstrument.Width = hitTestUI.ActualWidth;
                DummyInstrument.Height = hitTestUI.ActualHeight;
                DummyInstrument.SetValue(Canvas.LeftProperty, pcanvas.X - PanStartingPoint.X);
                DummyInstrument.SetValue(Canvas.TopProperty, pcanvas.Y - PanStartingPoint.Y);
                var x = MovingInstrument.DataContext as Instrument;
                DummyInstrumentText.Text = x.DisplayName;
                hitTestUI = null;
            }
        }

        public HitTestResultBehavior MyHitTestResult(HitTestResult result)
        {
            var hUI = result.VisualHit as FrameworkElement;

            if (hUI.GetType() == typeof(System.Windows.Controls.Border))
            {
                hitTestUI = ((hUI.Parent as FrameworkElement).Parent as FrameworkElement).Parent as FrameworkElement;
                return HitTestResultBehavior.Stop;
            }
            else
            {
                return HitTestResultBehavior.Continue;
            }
        }

        public HitTestResultBehavior MyHitTestResult1(HitTestResult result)
        {
            var hUI = result.VisualHit as FrameworkElement;

            if (hUI.GetType() == typeof(System.Windows.Controls.Border))
            {
                hitTestUI = ((hUI.Parent as FrameworkElement).Parent as FrameworkElement).Parent as FrameworkElement;
                return HitTestResultBehavior.Stop;
            }
            else
            if (hUI.GetType() == typeof(System.Windows.Controls.StackPanel))
            {
                hitTestUI = hUI;
                return HitTestResultBehavior.Stop;
            }
            else
            {
                return HitTestResultBehavior.Continue;
            }
        }

        private void InstrumentStackPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (MovingInstrument != null)
            {
                var newPos = e.GetPosition(InstrumentCanvas);
                DummyInstrument.SetValue(Canvas.LeftProperty, newPos.X - PanStartingPoint.X);
                DummyInstrument.SetValue(Canvas.TopProperty, newPos.Y - PanStartingPoint.Y);
            }

        }

        private void InstrumentStackPanel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (MovingInstrument!=null)
            {
                PanStartingPoint = e.GetPosition(InstrumentStackPanel);

                hitTestUI = null;

                VisualTreeHelper.HitTest(InstrumentStackPanel, null,
                    new HitTestResultCallback(MyHitTestResult1),
                    new PointHitTestParameters(PanStartingPoint));

                if (hitTestUI == null)
                    InstrumentStackPanel.Children.Remove(MovingInstrument);
                else
                if (hitTestUI.GetType() == typeof(System.Windows.Controls.StackPanel))
                {
                    InstrumentStackPanel.Children.Remove(MovingInstrument);
                    InstrumentStackPanel.Children.Add(MovingInstrument);
                }
                else
                if (MovingInstrument != hitTestUI)
                {
                    int j = InstrumentStackPanel.Children.IndexOf(hitTestUI);
                    InstrumentStackPanel.Children.Remove(MovingInstrument);
                    InstrumentStackPanel.Children.Insert(j, MovingInstrument);
                }

                DummyInstrument.Visibility = Visibility.Hidden;
                MovingInstrument = null;

                Properties.Settings.Default.InstrumentDisplayList = GetCurrentInstrumentDisplayList();
                Properties.Settings.Default.Save(); 
            }

        }

        #endregion

        #region Routing

        public List<Vertex> RoutingGridGenerate(Location StartLocation, Location EndLocation, int size, out Vertex StartVertex, out Vertex EndVertex)
        {
            size += 4; // Increase grid size two more row/cols before and after start and end vertex

            List<Vertex> VertexList = new List<Vertex>();
            Vertex[,] vArray = new Vertex[size, size];

            // Cartesian distance and angles ... just to build a regular grid           

            double dlat = EndLocation.Latitude - StartLocation.Latitude;
            double dlon = EndLocation.Longitude - StartLocation.Longitude;

            double dist = Math.Sqrt(dlat * dlat + dlon * dlon);
            double bearing = Math.Atan2(dlat, dlon) * 180 / Math.PI;

            double step = Math.Sqrt(2) / 2 * dist / (size - 5);

            // Create all vertex 
            for (int j = -2; j < size - 2; j++)
                for (int i = -2; i < size - 2; i++)
                {
                    Point l = new Point(i * step, j * step);

                    double sin = Math.Sin((bearing - 45) * Math.PI / 180);
                    double cos = Math.Cos((bearing - 45) * Math.PI / 180);

                    Point lr = new Point(StartLocation.Longitude + l.X * cos - l.Y * sin, StartLocation.Latitude + l.X * sin + l.Y * cos);

                    Vertex v = new Vertex(new Location(lr.Y, lr.X), DateTime.MaxValue);
                    vArray[i + 2, j + 2] = v;
                    VertexList.Add(v);
                }

            // Link Vertex to neighbors
            for (int j = 0; j < size; j++)
                for (int i = 0; i < size; i++)
                {
                    if (j < size - 2) vArray[i, j].Neighbors.Add(vArray[i, j + 1]);
                    if (j > 0) vArray[i, j].Neighbors.Add(vArray[i, j - 1]);

                    if (i < size - 2)
                    {
                        vArray[i, j].Neighbors.Add(vArray[i + 1, j]);
                        if (j < size - 2) vArray[i, j].Neighbors.Add(vArray[i + 1, j + 1]);
                        if (j < 0) vArray[i, j].Neighbors.Add(vArray[i + 1, j - 1]);
                        if (j < size - 3) vArray[i, j].Neighbors.Add(vArray[i + 1, j + 2]);
                        if (j > 1) vArray[i, j].Neighbors.Add(vArray[i + 1, j - 2]);
                        if (j < size - 4) vArray[i, j].Neighbors.Add(vArray[i + 1, j + 3]);
                        if (j > 2) vArray[i, j].Neighbors.Add(vArray[i + 1, j - 3]);
                    }

                    if (i > 0)
                    {
                        vArray[i, j].Neighbors.Add(vArray[i - 1, j]);
                        if (j < size - 2) vArray[i, j].Neighbors.Add(vArray[i - 1, j + 1]);
                        if (j > 0) vArray[i, j].Neighbors.Add(vArray[i - 1, j - 1]);
                        if (j > 1) vArray[i, j].Neighbors.Add(vArray[i - 1, j - 2]);
                        if (j < size - 3) vArray[i, j].Neighbors.Add(vArray[i - 1, j + 2]);
                        if (j < size - 4) vArray[i, j].Neighbors.Add(vArray[i - 1, j + 3]);
                        if (j > 2) vArray[i, j].Neighbors.Add(vArray[i - 1, j - 3]);
                    }

                    if (i < size - 3)
                    {
                        if (j < size - 2) vArray[i, j].Neighbors.Add(vArray[i + 2, j + 1]);
                        if (j > 0) vArray[i, j].Neighbors.Add(vArray[i + 2, j - 1]);
                        if (j < size - 4) vArray[i, j].Neighbors.Add(vArray[i + 2, j + 3]);
                        if (j > 2) vArray[i, j].Neighbors.Add(vArray[i + 2, j - 3]);
                    }

                    if (i > 1)
                    {
                        if (j > 0) vArray[i, j].Neighbors.Add(vArray[i - 2, j - 1]);
                        if (j < size - 2) vArray[i, j].Neighbors.Add(vArray[i - 2, j + 1]);
                        if (j < size - 4) vArray[i, j].Neighbors.Add(vArray[i - 2, j + 3]);
                        if (j > 2) vArray[i, j].Neighbors.Add(vArray[i - 2, j - 3]);
                    }

                    if (i < size - 4)
                    {
                        if (j < size - 2) vArray[i, j].Neighbors.Add(vArray[i + 3, j + 1]);
                        if (j < size - 3) vArray[i, j].Neighbors.Add(vArray[i + 3, j + 2]);
                        if (j > 0) vArray[i, j].Neighbors.Add(vArray[i + 3, j - 1]);
                        if (j > 1) vArray[i, j].Neighbors.Add(vArray[i + 3, j - 2]);
                    }

                    if (i > 2)
                    {
                        if (j < size - 2) vArray[i, j].Neighbors.Add(vArray[i - 3, j + 1]);
                        if (j < size - 3) vArray[i, j].Neighbors.Add(vArray[i - 3, j + 2]);
                        if (j > 0) vArray[i, j].Neighbors.Add(vArray[i - 3, j - 1]);
                        if (j > 1) vArray[i, j].Neighbors.Add(vArray[i - 3, j - 2]);
                    }
                }

            StartVertex = vArray[2, 2];
            EndVertex = vArray[size - 3, size - 3];
            return VertexList;
        }

        private Boolean VisitVertex(Vertex v)
        {

            DateTime t = v.Cost;

            // Current

            uvpair cuv = new uvpair();

            if (cgrib != null)
            {
                if (useCurrents)
                {
                    cuv = cgrib.GetCurrentInterpolated(v.Position.Latitude, v.Position.Longitude, t);
                    if (cuv != null)
                    {
                        v.DRIFT = Math.Sqrt(cuv.u * cuv.u + cuv.v * cuv.v) * 3600 / 1852;
                        v.SET = (Math.Atan2(cuv.u, cuv.v) * 180 / Math.PI + 360) % 360;
                    }
                }
                else
                {
                    cuv.u = 0;
                    cuv.v = 0;
                }
            }

            processedVertexCnt++;

            double processedPercentage = (double)processedVertexCnt / (double)totalVertex * 100;
            if (Math.Floor(processedPercentage) != Math.Floor(lastProcessedPercentage))
            {
                CalcRouteWorker.ReportProgress((int)processedPercentage);
                lastProcessedPercentage = processedPercentage;
            }

            //Wind
            uvpair wuv = wgrib.GetWindInterpolated(v.Position.Latitude, v.Position.Longitude, t);

            double twd;
            double tws;

            if (wuv != null)
            {
                twd = Math.Atan2(wuv.u, wuv.v) * 180 / Math.PI;
                twd = (twd + 180) % 360;
                tws = Math.Sqrt(wuv.u * wuv.u + wuv.v * wuv.v) * 3600 / 1852;
            }
            else
            {
                twd = 0;
                tws = 0;
            }

            v.TWD = twd;
            v.TWS = tws;

            foreach (Vertex vn in v.Neighbors)
            {
                if (!vn.Visited)
                {

                    if (cuv != null) // we've got a valid current measurment
                    {
                        double brg = CalcBearing(v.Position.Latitude, v.Position.Longitude, vn.Position.Latitude, vn.Position.Longitude);
                        double dst = CalcDistance(v.Position.Latitude, v.Position.Longitude, vn.Position.Latitude, vn.Position.Longitude) / 1852;

                        PolarPoint pr = new PolarPoint(); // target twa and sog to sail at tgtcog bearing under given current

                        if (v.DRIFT != 0)
                        {
                            pr = NavPolar.GetTargetBearing(tws, twd, brg, v.DRIFT, v.SET, perfAdj);
                        }
                        else
                        {
                            pr.TWA = twd - brg;
                            pr.SPD = NavPolar.GetTarget(pr.TWA, tws) * perfAdj;
                        }

                        if (pr != null && pr.SPD != 0)
                        {
                            DateTime newCost = t.AddHours(dst / pr.SPD);

                            if (vn.Cost > newCost)
                            {
                                vn.Cost = newCost;
                                vn.Previous = v;

                                vn.SPD = pr.SPD;
                                vn.TWA = (pr.TWA > 180) ? (pr.TWA - 360) : pr.TWA;
                                vn.BRG = brg;
                            }
                        }
                    }

                }
            }

            v.Visited = true;
            return true;
        }

        private Boolean CalcMinimumRoute(Vertex startV, Vertex endV, DateTime startTime, List<Vertex> routingGrid)
        {
            startV.Cost = startTime;

            Vertex vx = startV;

            while (vx != endV)
            {
                Boolean result = VisitVertex(vx);

                if (!result)
                    return false;    // Grib out of range.

                var notVisitedVertex = routingGrid.Where(x => x.Visited != true);
                DateTime minCost = notVisitedVertex.Min(x => x.Cost);
                vx = notVisitedVertex.First(x => x.Cost == minCost);
            }

            VisitVertex(endV);

            return true;
        }

        public IEnumerable<Vertex> GetMinimumRoute(Vertex startV, Vertex endV, List<Vertex> routingGrid)
        {
            Vertex v = endV;

            while (v != startV && v != null)
            {
                yield return v;
                v = v.Previous;
            }

            yield return v; // First vertex - has previous = null;
        }

        private void RouteCalcButton_Click(object sender, RoutedEventArgs e)
        {
            if (routeCalculationControl.RouteListCombo.SelectedItem != null
                && routeCalculationControl.RouteStartTimePicker.Value != null
                && wgrib != null
                && NavPolar.IsLoaded)
            {
                routeCalculationControl.CalculateRoute.IsEnabled = false;

                startTime = ((DateTime)routeCalculationControl.RouteStartTimePicker.Value).ToUniversalTime();
                perfAdj = (double)routeCalculationControl.PerformanceAdjust.Value;

                useCurrents = (bool)routeCalculationControl.UseCurrent.IsChecked;

                var r = (Route)routeCalculationControl.RouteListCombo.SelectedItem;

                sourceRouteLocations.Clear();
                sourceRouteLocations.Add(r.Legs[0].FromLocation);
                foreach (Leg lg in r.Legs)
                    sourceRouteLocations.Add(lg.ToLocation);
                CalcRouteWorker.RunWorkerAsync(sourceRouteLocations);
            }
        }

        void CalcRouteWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            List<Vertex> routeResult = new List<Vertex>();
            Vertex lastVertex = null;
            DateTime st = startTime;

            Boolean result = true;  // will return false if it fails to calculate route because grib is out of range

            const int gridSize = 80;

            var locs = e.Argument as List<Location>;

            totalVertex = (locs.Count() - 1) * gridSize * gridSize;
            processedVertexCnt = 0;

            for (int i = 0; i < locs.Count() - 1; i++)
            {
                Vertex startV, endV;
                List<Vertex> routingGrid = new List<Vertex>();

                routingGrid = RoutingGridGenerate(locs[i], locs[i + 1], gridSize, out startV, out endV);

                startV.Previous = lastVertex;

                result = CalcMinimumRoute(startV, endV, st, routingGrid);

                if (result == false)
                {
                    break;
                }

                var rl = GetMinimumRoute(startV, endV, routingGrid);
                routeResult.AddRange(rl);

                lastVertex = endV;  // This links leg result to previous leg
                st = endV.Cost;
            }

            e.Result = routeResult;
        }

        void CalcRouteWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            routeCalculationControl.Progress.Value = e.ProgressPercentage;
        }

        void CalcRouteWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            routeCalculationControl.Progress.Value = 0;
            routeCalculationControl.CalculateRoute.IsEnabled = true;
            routeCalculationControl.RouteReplaySlider.IsEnabled = true;
            replayBoat.BoatVisible = Visibility.Visible;

            var routeResult = e.Result as List<Vertex>;

            if (routeResult != null)
            {
                if (routeResult.Count() > 1)
                {
                    routeResult.Sort((p1, p2) => DateTime.Compare(p1.Cost, p2.Cost));

                    RoutingResult rr = new RoutingResult(routeResult);
                    Panel.SetZIndex(rr, 40);
                    routingResults.Add(rr);
                    rr.ID = "Result " + routingResults.Count().ToString("#");
                    map.Children.Add(rr);

                    routeCalculationControl.ResultCombo.SelectedItem = rr;
                }
                else
                {
                    routeCalculationControl.ResultText.Text = "Grib Out of Range";
                    System.Media.SystemSounds.Exclamation.Play();
                }
            }
        }

        private void RouteClearResultsButton_Click(object sender, RoutedEventArgs e)
        {
            routeCalculationControl.RouteReplaySlider.IsEnabled = false;
            routeCalculationControl.RouteReplaySlider.Value = 0;

            foreach (RoutingResult rr in routingResults)
                map.Children.Remove(rr);

            routingResults.Clear();
            routeCalculationControl.ResultText.Text = "";

            replayBoat.BoatVisible = Visibility.Hidden;

        }

        void ResultCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (RoutingResult rr in e.AddedItems)
            {
                rr.Select();
                routeCalculationControl.ResultText.Text = rr.vertexList[rr.vertexList.Count() - 1].Cost.ToLocalTime().ToString();
                routeCalculationControl.RouteReplaySlider.Maximum = rr.vertexReplayList.Count - 1;

                routeCalculationControl.RouteReplaySlider.Value = 0;

                replayBoat.Location = rr.vertexReplayList[0].Position;
                replayBoat.Heading = rr.vertexReplayList[0].BRG;
            }

            foreach (RoutingResult rr in e.RemovedItems)
            {
                rr.UnSelect();
            }

        }

        void RouteReplaySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

            var rr = routeCalculationControl.ResultCombo.SelectedItem as RoutingResult;

            Vertex v = rr.vertexReplayList[(int)e.NewValue];

            replayBoat.Location = v.Position;
            replayBoat.Heading = v.BRG;
            replayBoat.WindDirection = v.TWD;
            replayBoat.WindSpeed = v.TWS;
            replayBoat.CurrentDirection = v.SET;
            replayBoat.CurrentSpeed = v.DRIFT;
            replayBoat.BoatSpeed = v.SPD;
            replayBoat.Time = v.Cost.ToLocalTime();
        }

        #endregion

        #region MainWindow Updates

        private void UpdateNav()
        {
            if (PlayButton.IsChecked == true)
            {

                if (LAT.IsValid())
                {
                    boat.Location = new Location(LAT.Val, LON.Val);
                    boat.Course = COG.Val;
                    boat.BoatVisible = Visibility.Visible;

                    double heading = COG.Val;

                    if (HDT.IsValid())
                        heading = HDT.Val;

                    boat.Heading = heading;

                    NavPlotModel.CurrentValue = POS.GetLastVal(0).Time.Ticks;

                    double deltaT = NavPlotModel.MaxXAxisValue - NavPlotModel.MinXAxisValue;
                    double lim = NavPlotModel.MinXAxisValue + 0.8 * deltaT;

                    if (NavPlotModel.CurrentValue > lim)
                    {
                        NavPlotModel.MinXAxisValue = (NavPlotModel.CurrentValue - 0.8 * deltaT);
                        NavPlotModel.MaxXAxisValue = (NavPlotModel.CurrentValue + 0.2 * deltaT);
                    }

                    if (NavPlotModel.CurrentValue < NavPlotModel.MinXAxisValue)
                    {
                        NavPlotModel.MinXAxisValue = (NavPlotModel.CurrentValue);
                        NavPlotModel.MaxXAxisValue = (NavPlotModel.CurrentValue + deltaT);
                    }

                    if (mapOrientationMode == MapOrientationMode.CourseUp)
                    {
                        map.TargetHeading = -COG.Val;
                        boat.Course = 0;
                        boat.Heading = heading - COG.Val;
                    }

                }
                else
                {
                    boat.BoatVisible = Visibility.Hidden;
                }

                if (mapCenterMode == MapCenterMode.Centered)
                {
                    if (mapOrientationMode == MapOrientationMode.CourseUp)
                    {
                        Point p = map.LocationToViewportPoint(boat.Location);
                        map.TargetCenter = map.ViewportPointToLocation(new Point(p.X, p.Y - map.ActualHeight / 3));
                    }
                    else
                    {
                        Point p = map.LocationToViewportPoint(boat.Location);
                        map.TargetCenter = map.ViewportPointToLocation(new Point(p.X, p.Y));
                    }
                }

                if ((bool)MeasureButton.IsChecked && mouseHandlingMode != MouseHandlingMode.SettingMeasureStart)
                {
                    if (measureCenteredOnBoat)
                    {
                        measureRange.FromLocation = POS.Val;
                    }
                    measureRange.TWD = TWD.Val;
                    CalcMeasure();
                }

                if (newTrackPositionAvailable)
                {
                    //var level = Properties.Settings.Default.TrackResolution;                    
                    var level = NavPlotModel.Resolution;
                    if (POS.GetLastVal(level) != null)
                    {
                        track.AddNewLocation(POS.GetLastVal(level).Val, SOG.GetLastVal(level).Val, POS.GetLastVal(level).Time);
                        newTrackPositionAvailable = false;

                        // Update Plot

                        DateTime cTime = POS.GetLastVal(level).Time;
                        (double? mVal, double? aVal) = GetLastPloValue(level);

                        MainPlotValues.Add(new DateModel { DateTime = cTime, Value = mVal });
                        if (MainPlotValues.Count > NavPlotModel.MaxData)
                            MainPlotValues.RemoveAt(0);

                        AuxPlotValues.Add(new DateModel { DateTime = cTime, Value = aVal });
                        if (AuxPlotValues.Count > NavPlotModel.MaxData)
                            AuxPlotValues.RemoveAt(0);
                    }

                }

                if (TWD.IsValid())
                {
                    boat.WindDirection = TWD.Val;
                    if (mapOrientationMode == MapOrientationMode.CourseUp)
                        boat.WindDirection -= COG.Val;
                    boat.WindSpeed = TWS.Val;
                }

                if (SOG.IsValid())
                {
                    boat.BoatSpeed = SOG.Val;
                }

                if (PERF.IsValid())
                {
                    boat.BoatPerf = PERF.Val;
                }
                else
                {
                    boat.BoatPerf = 0;
                }

                if (DRIFT.IsValid())
                {
                    boat.CurrentDirection = SET.Val + 180;
                    if (mapOrientationMode == MapOrientationMode.CourseUp)
                        boat.CurrentDirection -= COG.Val;
                    boat.CurrentSpeed = DRIFT.Val;
                }

                if (Properties.Settings.Default.LaylinesCheck == true)
                {
                    if (ActiveMark != null)
                    {
                        StbLaylineTo.Visibility = Visibility.Visible;
                        PrtLaylineTo.Visibility = Visibility.Visible;
                        HitP.Visibility = Visibility.Visible;
                        HitS.Visibility = Visibility.Visible;

                        var l1 = new Location(WLAT.Val, WLON.Val);
                        var d = Math.Max(1800, 2 * 1852 * DST.Val);

                        double a1 = 0, a2 = 0;

                        double windir = 0;

                        if (TWD.IsValid())
                            windir = TWD.Val;

                        switch (sailingMode)
                        {
                            case SailingMode.Beating:
                                if (insideCourse)
                                {
                                    if (TGTTWA.IsValid())
                                    {
                                        if (TGTCOGs.IsValid())
                                        {
                                            a1 = 180 + TGTCOGs.Val;
                                            a2 = 180 + TGTCOGp.Val;
                                        }
                                        else
                                        {
                                            a1 = windir + 180 - TGTTWA.Val;
                                            a2 = windir + 180 + TGTTWA.Val;
                                        }
                                    }
                                    else
                                    {
                                        a1 = windir + 180 - 45;
                                        a2 = windir + 180 + 45;
                                    }
                                }
                                else
                                {
                                    a1 = 180 + BRG.Val;
                                    a2 = a1;
                                    d = 1852 * DST.Val;
                                }
                                break;

                            case SailingMode.Reaching:
                            case SailingMode.None:
                                a1 = 180 + BRG.Val;
                                a2 = a1;
                                d = 1852 * DST.Val;
                                break;

                            case SailingMode.Running:
                                if (TGTTWA.IsValid())
                                {
                                    if (TGTCOGs.IsValid())
                                    {
                                        a1 = 180 + TGTCOGs.Val;
                                        a2 = 180 + TGTCOGp.Val;
                                    }
                                    else
                                    {
                                        a1 = windir + 180 - TGTTWA.Val;
                                        a2 = windir + 180 + TGTTWA.Val;
                                    }
                                }
                                else
                                {
                                    a1 = windir + 180 - 170;
                                    a2 = windir + 180 + 170;
                                }
                                break;
                        }

                        double lat = 0, lon = 0;
                        CalcPosition(l1.Latitude, l1.Longitude, d, a1, ref lat, ref lon);
                        var l2 = new Location(lat, lon);

                        CalcPosition(l1.Latitude, l1.Longitude, d, a2, ref lat, ref lon);
                        var l3 = new Location(lat, lon);


                        StbLaylineTo.FromLocation = l1;
                        StbLaylineTo.ToLocation = l2;

                        PrtLaylineTo.FromLocation = l1;
                        PrtLaylineTo.ToLocation = l3;

                        CalcPosition(POS.Val.Latitude, POS.Val.Longitude, DSTLYLp.Val, TGTCOGp.Val, ref lat, ref lon);
                        HitPointPort.Location = new Location(lat, lon);
                        HitPointPort.Name = TTGLYLp.FormattedValue;

                        CalcPosition(POS.Val.Latitude, POS.Val.Longitude, DSTLYLs.Val, TGTCOGs.Val, ref lat, ref lon);
                        HitPointStbd.Location = new Location(lat, lon);
                        HitPointStbd.Name = TTGLYLs.FormattedValue;

                        if (!insideCourse)
                        {
                            HitP.Visibility = Visibility.Hidden;
                            HitS.Visibility = Visibility.Hidden;
                            PrtLaylineTo.Visibility = Visibility.Hidden;
                            // Leave Stbd green layline as reference - direct course marker
                        }

                    }
                    else
                    {
                        StbLaylineTo.Visibility = Visibility.Hidden;
                        PrtLaylineTo.Visibility = Visibility.Hidden;
                        HitP.Visibility = Visibility.Hidden;
                        HitS.Visibility = Visibility.Hidden;
                    }
                }
                else
                {
                    StbLaylineTo.Visibility = Visibility.Hidden;
                    PrtLaylineTo.Visibility = Visibility.Hidden;
                    HitP.Visibility = Visibility.Hidden;
                    HitS.Visibility = Visibility.Hidden;
                }

                if (Properties.Settings.Default.TargetBearingsCheck == true)
                {
                    StbBearingTarget.Visibility = Visibility.Visible;
                    PrtBearingTarget.Visibility = Visibility.Visible;

                    if (LAT.IsValid())
                    {
                        var l1 = boat.Location;
                        var d = Math.Max(1800, 2 * 1852 * DST.Val);

                        double a1 = 0, a2 = 0;

                        double windir = 0;

                        if (TWD.IsValid())
                            windir = TWD.Val;

                        switch (sailingMode)
                        {
                            case SailingMode.Beating:
                                if (TGTTWA.IsValid())
                                {
                                    if (TGTCOGs.IsValid())
                                    {
                                        a1 = TGTCOGs.Val;
                                        a2 = TGTCOGp.Val;
                                    }
                                    else
                                    {
                                        a1 = windir - TGTTWA.Val;
                                        a2 = windir + TGTTWA.Val;
                                    }
                                }
                                else
                                {
                                    a1 = windir - 45;
                                    a2 = windir + 45;
                                }
                                break;

                            case SailingMode.Reaching:
                                StbBearingTarget.Visibility = Visibility.Hidden;
                                PrtBearingTarget.Visibility = Visibility.Hidden;
                                break;

                            case SailingMode.Running:
                                if (TGTTWA.IsValid())
                                {
                                    if (TGTCOGs.IsValid())
                                    {
                                        a1 = TGTCOGs.Val;
                                        a2 = TGTCOGp.Val;
                                    }
                                    else
                                    {
                                        a1 = windir - TGTTWA.Val;
                                        a2 = windir + TGTTWA.Val;
                                    }
                                }
                                else
                                {
                                    a1 = windir - 170;
                                    a2 = windir + 170;
                                }
                                break;
                        }

                        double lat = 0, lon = 0;
                        CalcPosition(l1.Latitude, l1.Longitude, d, a1, ref lat, ref lon);
                        var l2 = new Location(lat, lon);

                        CalcPosition(l1.Latitude, l1.Longitude, d, a2, ref lat, ref lon);
                        var l3 = new Location(lat, lon);

                        StbBearingTarget.FromLocation = l1;
                        StbBearingTarget.ToLocation = l2;

                        PrtBearingTarget.FromLocation = l1;
                        PrtBearingTarget.ToLocation = l3;
                    }
                }
                else
                {
                    StbBearingTarget.Visibility = Visibility.Hidden;
                    PrtBearingTarget.Visibility = Visibility.Hidden;
                }
            }
            else
            {
                StbLaylineTo.Visibility = Visibility.Hidden;
                PrtLaylineTo.Visibility = Visibility.Hidden;
                HitP.Visibility = Visibility.Hidden;
                HitS.Visibility = Visibility.Hidden;
                StbBearingTarget.Visibility = Visibility.Hidden;
                PrtBearingTarget.Visibility = Visibility.Hidden;
            }
        }

        private void UpdateFleet(DateTime dt)
        {
            DateTime minDt = dt.AddSeconds(-60); // Short lookup range for position = t - 60 seg

            using (var context = new LionRiverDBContext())
            {
                // UPDATE TO LEVEL=0 FOR BETTER RESOLUTION !!!!

                var logEntry = (from x in context.Logs
                                where (x.level == 1 && (x.timestamp <= dt) && (x.timestamp > minDt))
                                orderby x.timestamp descending
                                select x).FirstOrDefault();

                if (logEntry != null)
                {
                    boat.Location = new Location(logEntry.LAT, logEntry.LON);
                    boat.Course = Convert.ToDouble(logEntry.COG);
                    boat.BoatSpeed = Convert.ToDouble(logEntry.SOG);
                    boat.Heading = Convert.ToDouble(logEntry.HDT);
                    boat.BoatPerf = Convert.ToDouble(logEntry.PERF);
                    boat.WindDirection = Convert.ToDouble(logEntry.TWD);
                    boat.WindSpeed = Convert.ToDouble(logEntry.TWS);
                    boat.CurrentDirection = Convert.ToDouble(logEntry.SET) + 180;
                    boat.CurrentSpeed = Convert.ToDouble(logEntry.DRIFT);

                    if (wgrib != null && Properties.Settings.Default.PredictedWindDirectionCheck)
                    {
                        double twd, tws;

                        uvpair wuv = wgrib.GetWindInterpolated(boat.Location.Latitude, boat.Location.Longitude, dt);
                        if (wuv != null)
                        {
                            twd = Math.Atan2(wuv.u, wuv.v) * 180 / Math.PI + 180;
                            tws = Math.Sqrt(wuv.u * wuv.u + wuv.v * wuv.v) * 3600 / 1852;

                            boat.PredictedWindDirection = twd;
                            boat.PredictedWindSpeed = tws;
                            boat.PredictedWindVisible = Visibility.Visible;
                        }
                        else
                            boat.PredictedWindVisible = Visibility.Hidden;
                    }
                    else
                        boat.PredictedWindVisible = Visibility.Hidden;

                    if (cgrib != null && Properties.Settings.Default.PredictedWindDirectionCheck)
                    {
                        double set, drift;

                        uvpair cuv = cgrib.GetCurrentInterpolated(boat.Location.Latitude, boat.Location.Longitude, dt);
                        if (cuv != null)
                        {
                            set = Math.Atan2(cuv.u, cuv.v) * 180 / Math.PI + 180;
                            drift = Math.Sqrt(cuv.u * cuv.u + cuv.v * cuv.v) * 3600 / 1852;

                            boat.PredictedCurrentDirection = set;
                            boat.PredictedCurrentSpeed = drift;
                            boat.PredictedCurrentVisible = Visibility.Visible;
                        }
                        else
                            boat.PredictedCurrentVisible = Visibility.Hidden;
                    }
                    else
                        boat.PredictedCurrentVisible = Visibility.Hidden;

                    boat.BoatVisible = Visibility.Visible;
                }
                else
                {
                    boat.BoatVisible = Visibility.Hidden;
                }

                foreach (Boat b in fleetBoats)
                {
                    var fTrackEntry =
                        (from x in context.FleetTracks
                         where x.Name == b.Name && x.timestamp <= dt
                         orderby x.timestamp descending
                         select x).Take(1).ToList();

                    if (fTrackEntry != null && fTrackEntry.Count() > 0)
                    {
                        var lastTrackEntry = fTrackEntry[0];

                        TimeSpan deltaT = dt - lastTrackEntry.timestamp;

                        if (deltaT > TimeSpan.FromMinutes(15)) 
                            deltaT = TimeSpan.FromMinutes(15);

                        // extrapolate Meters since last position up to 15min

                        var distance = lastTrackEntry.SOG * deltaT.TotalHours * 1852;

                        double lat = 0, lon = 0;

                        CalcPosition(lastTrackEntry.Latitude, lastTrackEntry.Longitude, distance, lastTrackEntry.COG, ref lat, ref lon);

                        b.Location = new Location(lat,lon);
                        b.Heading = lastTrackEntry.COG;
                        b.IsAvailable = true;
                    }
                    else
                    {
                        b.IsAvailable = false;
                    }

                }

                if (mapCenterMode == MapCenterMode.Centered)
                {
                    if (mapOrientationMode == MapOrientationMode.CourseUp)
                    {
                        Point p = map.LocationToViewportPoint(boat.Location);
                        map.TargetCenter = map.ViewportPointToLocation(new Point(p.X, p.Y - map.ActualHeight / 3));
                    }
                    else
                    {
                        Point p = map.LocationToViewportPoint(boat.Location);
                        map.TargetCenter = map.ViewportPointToLocation(new Point(p.X, p.Y));
                    }
                }
            }

        }

        private void UpdateTracks(DateTime startTime, DateTime endTime, int n = 0)
        {
            using (var context = new LionRiverDBContext())
            {

                map.Children.Remove(track);

                IQueryable<Log> logEntries;

                if (n == 0)
                {
                    logEntries = (from x in context.Logs
                                  where (x.level == NavPlotModel.Resolution && x.timestamp <= endTime && x.timestamp > startTime)
                                  select x);
                }
                else
                {
                    logEntries = (from x in context.Logs
                                  where (x.level == NavPlotModel.Resolution)
                                  orderby x.timestamp descending
                                  select x
                                        ).Take(n).OrderBy(y => y.timestamp);
                }

                track = new Track(logEntries.ToList(), NavPlotModel.Resolution, Properties.Settings.Default.SPDminVal,
                                        Properties.Settings.Default.SPDminIndex, Properties.Settings.Default.SPDmaxVal, Properties.Settings.Default.SPDmaxIndex);

                map.Children.Add(track);
                Panel.SetZIndex(track, 5);

                foreach (Track ft in fleetTracks)
                    map.Children.Remove(ft);
                fleetTracks.Clear();

                // Add up to 5 full Track.maxLenght
                var selCnt = fleetBoats.Count(x => x.IsSelected == true);
                var takeCnt = (selCnt != 0) ? Track.MaxLength * 5 / selCnt : 0;

                List<List<FleetTrack>> fTrackList = new List<List<FleetTrack>>();
                List<FleetTrack> fTrackEntries;

                foreach (Boat b in fleetBoats)
                {

                    if (b.IsSelected)
                    {
                        if (n == 0)
                        { // Include all, post-processing could decimate the list to accomodate takeCnt elements
                            fTrackEntries =
                                (from x in context.FleetTracks
                                 where x.Name == b.Name && x.timestamp <= endTime && x.timestamp > startTime
                                 orderby x.timestamp ascending
                                 select x).ToList();
                        }
                        else
                        { // Make sure endtime is included, begining could be trimmed off
                            fTrackEntries =
                                (from x in context.FleetTracks
                                 where x.Name == b.Name && x.timestamp <= endTime && x.timestamp > startTime
                                 orderby x.timestamp descending
                                 select x).Take(takeCnt).OrderBy(y => y.timestamp).ToList();
                        }

                        fTrackList.Add(fTrackEntries);
                    }
                }

                foreach (List<FleetTrack> fTe in fTrackList)
                {
                    double A = fTe.Count();
                    double B = takeCnt;
                    double decimator = A / (A - B);

                    List<FleetTrack> rfTe= new List<FleetTrack>();

                    if (n == 0 && decimator >= 1) // Decimate list
                    {
                        for (int j = 0; j < fTe.Count(); j++)
                        {
                            if ((int)(j % decimator) >= 1)
                                rfTe.Add(fTe[j]);
                        }
                    }
                    else
                    {
                        rfTe = fTe;
                    }

                    if (rfTe != null && rfTe.Count() > 0)
                    {

                        Track tr = new Track(rfTe, Properties.Settings.Default.SPDminVal, Properties.Settings.Default.SPDminIndex,
                                                Properties.Settings.Default.SPDmaxVal, Properties.Settings.Default.SPDmaxIndex);

                        tr.Visibility = Visibility.Visible;

                        fleetTracks.Add(tr);
                        map.Children.Add(tr);
                        Panel.SetZIndex(tr, 5);
                    }
                }
            }
        }

        private void UpdatePlot(double n, DateTime StartTime, DateTime EndTime)
        {
            using (var context = new LionRiverDBContext())
            {
                var logEntries = from x in context.Logs
                                 where (x.level == n && x.timestamp > StartTime && x.timestamp < EndTime)
                                 select x;

                PlotSelector mSelector =  MainPlotSelectionComboBox.SelectedItem as PlotSelector;

                List<DateModel> result = new List<DateModel>();

                if(mSelector!=null)
                {
                    switch(mSelector.Name)
                    {
                        case "SOG":
                            result = logEntries.Select(x => new DateModel { DateTime = x.timestamp, Value = x.SOG }).ToList();
                            break;

                        case "SPD":
                            result = logEntries.Select(x => new DateModel { DateTime = x.timestamp, Value = x.SPD }).ToList();

                            break;
                        case "TWD":
                            result = logEntries.Select(x => new DateModel { DateTime = x.timestamp, Value = (x.TWD + 360) % 360 }).ToList();

                            break;
                        case "TWS":
                            result = logEntries.Select(x => new DateModel { DateTime = x.timestamp, Value = x.TWS }).ToList();

                            break;
                        case "Drift":
                            result = logEntries.Select(x => new DateModel { DateTime = x.timestamp, Value = x.DRIFT }).ToList();
                            break;

                        case "Perf":
                            result = logEntries.Select(x => new DateModel { DateTime = x.timestamp, Value = (x.PERF * 100) }).ToList();
                            break; 

                        case "Depth":
                            result = logEntries.Select(x => new DateModel { DateTime = x.timestamp, Value = -x.DPT }).ToList();
                            break;

                        case "Fleet":
                            result = (from x in FleetActivityValues
                                      where x.DateTime > StartTime && x.DateTime < EndTime
                                      select x).ToList();
                            break;
                    }
                }

                if (result.Count() != 0)
                {
                    MainPlotValues.Clear();
                    MainPlotValues.AddRange(result);

                    NavPlotModel.MinY1AxisValue = mSelector.MinValue;
                    NavPlotModel.MaxY1AxisValue = mSelector.MaxValue;
                    NavPlotModel.Y1Formatter = mSelector.Formatter;
                }

                mSelector = AuxPlotSelectionComboBox.SelectedItem as PlotSelector;

                if (mSelector != null)
                {
                    switch (mSelector.Name)
                    {
                        case "SOG":
                            result = logEntries.Select(x => new DateModel { DateTime = x.timestamp, Value = x.SOG }).ToList();
                            break;

                        case "SPD":
                            result = logEntries.Select(x => new DateModel { DateTime = x.timestamp, Value = x.SPD }).ToList();

                            break;
                        case "TWD":
                            result = logEntries.Select(x => new DateModel { DateTime = x.timestamp, Value = (x.TWD + 360) % 360 }).ToList();

                            break;
                        case "TWS":
                            result = logEntries.Select(x => new DateModel { DateTime = x.timestamp, Value = x.TWS }).ToList();

                            break;
                        case "Drift":
                            result = logEntries.Select(x => new DateModel { DateTime = x.timestamp, Value = x.DRIFT }).ToList();
                            break;

                        case "Perf":
                            result = logEntries.Select(x => new DateModel { DateTime = x.timestamp, Value = (x.PERF * 100) }).ToList();
                            break;

                        case "Depth":
                            result = logEntries.Select(x => new DateModel { DateTime = x.timestamp, Value = -x.DPT }).ToList();
                            break;

                        case "Fleet":
                            result = (from x in FleetActivityValues
                                      where x.DateTime > StartTime && x.DateTime < EndTime
                                      select x).ToList();
                            break;
                    }
                }

                if (result.Count() != 0)
                {
                    AuxPlotValues.Clear();
                    AuxPlotValues.AddRange(result);

                    NavPlotModel.MinY2AxisValue = mSelector.MinValue;
                    NavPlotModel.MaxY2AxisValue = mSelector.MaxValue;
                    NavPlotModel.Y2Formatter = mSelector.Formatter;
                }

            }
        }

        private (double? mainV,double? auxV ) GetLastPloValue(int level)
        {
            PlotSelector mSelector = MainPlotSelectionComboBox.SelectedItem as PlotSelector;
            double? mResult = null;

            if (mSelector != null)
            {
                switch (mSelector.Name)
                {
                    case "SOG":
                        mResult = SOG.GetLastVal(level).Val;
                        break;

                    case "SPD":
                        mResult = SPD.GetLastVal(level).Val;
                        break;

                    case "TWD":
                        mResult = (TWD.GetLastVal(level).Val + 360) % 360;
                        break;

                    case "TWS":
                        mResult = TWS.GetLastVal(level).Val;
                        break;

                    case "Drift":
                        mResult = DRIFT.GetLastVal(level).Val;
                        break;

                    case "Perf":
                        mResult = PERF.GetLastVal(level).Val * 100;
                        break;

                    case "Depth":
                        mResult = -DPT.GetLastVal(level).Val;
                        break;
                }
            }

            mSelector = AuxPlotSelectionComboBox.SelectedItem as PlotSelector;
            double? aResult = null;

            if (mSelector != null)
            {
                switch (mSelector.Name)
                {
                    case "SOG":
                        aResult = SOG.GetLastVal(level).Val;
                        break;

                    case "SPD":
                        aResult = SPD.GetLastVal(level).Val;
                        break;

                    case "TWD":
                        aResult = (TWD.GetLastVal(level).Val + 360) % 360;
                        break;

                    case "TWS":
                        aResult = TWS.GetLastVal(level).Val;
                        break;

                    case "Drift":
                        aResult = DRIFT.GetLastVal(level).Val;
                        break;

                    case "Perf":
                        aResult = PERF.GetLastVal(level).Val * 100;
                        break;

                    case "Depth":
                        aResult = -DPT.GetLastVal(level).Val;
                        break;
                }
            }

            return (mResult, aResult);
        }

        #endregion

        #region MOB

        private void LatLonGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                ManOverBoard = true;

                MOB = new Mark()
                {
                    Location = new Location(LAT.Val, LON.Val),
                    Name = "MOB"
                };

                marksItemCollection.Add(MOB);

                ActiveRoute = null;
                ActiveLeg = null;
                ActiveMark = MOB;

                LatLonGrid.Background = Brushes.DarkRed;
                ContextMenuMOBClear.IsEnabled = true;
            }
        }

        private void ContextMenuMOBClear_Click(object sender, RoutedEventArgs e)
        {
            ManOverBoard = false;

            ActiveRoute = null;
            ActiveLeg = null;
            ActiveMark = null;

            foreach(Mark mk in marksItemCollection.ToList())
            {
                if(mk.Name.Substring(0,3)=="MOB")
                {
                    marksItemCollection.Remove(mk);
                }
            }

            LatLonGrid.Background = null;

        }
        #endregion

        #region Other

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            routeControl.Close();
            marksControl.Close();
            layerControl.Close();
            wlCourseSetupWindow.Close();

            CloseSerialPort1();
            CloseSerialPort2();
            CloseSerialPort3();
            CloseSerialPort4();

            Properties.Settings.Default.MapScale = map.ZoomLevel;
            Properties.Settings.Default.MapCenter = map.Center;
            Properties.Settings.Default.Save();

            if (logging)
                LogFile.Close();
        }

        private void MainWnd_StateChanged(object sender, EventArgs e)
        {
            switch (this.WindowState)
            {
                case System.Windows.WindowState.Maximized:
                    break;

                case System.Windows.WindowState.Minimized:
                    routeControl.Hide();
                    marksControl.Hide();
                    layerControl.Hide();
                    wlCourseSetupWindow.Hide();
                    break;

                case System.Windows.WindowState.Normal:
                    break;
            }
        }

        private Version GetRunningVersion()
        {
            try
            {
                return System.Deployment.Application.ApplicationDeployment.CurrentDeployment.CurrentVersion;
            }
            catch
            {
                return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            }
        }

        static public Color GetColor(double x, LinearGradientBrush br)
        {

            //Clip the input if before or after the max/min offset values
            double max = br.GradientStops.Max(n => n.Offset);
            if (x > max)
            {
                x = max;
            }
            double min = br.GradientStops.Min(n => n.Offset);
            if (x < min)
            {
                x = min;
            }

            //Find gradient stops that surround the input value
            GradientStop gs0 = br.GradientStops.Where(n => n.Offset <= x).OrderBy(n => n.Offset).Last();
            GradientStop gs1 = br.GradientStops.Where(n => n.Offset >= x).OrderBy(n => n.Offset).First();

            float y = 0f;
            if (gs0.Offset != gs1.Offset)
            {
                y = (float)((x - gs0.Offset) / (gs1.Offset - gs0.Offset));
            }

            //Interpolate color channels
            Color cx = new Color();
            if (br.ColorInterpolationMode == ColorInterpolationMode.ScRgbLinearInterpolation)
            {
                float aVal = (gs1.Color.ScA - gs0.Color.ScA) * y + gs0.Color.ScA;
                float rVal = (gs1.Color.ScR - gs0.Color.ScR) * y + gs0.Color.ScR;
                float gVal = (gs1.Color.ScG - gs0.Color.ScG) * y + gs0.Color.ScG;
                float bVal = (gs1.Color.ScB - gs0.Color.ScB) * y + gs0.Color.ScB;
                cx = Color.FromScRgb(aVal, rVal, gVal, bVal);
            }
            else
            {
                byte aVal = (byte)((gs1.Color.A - gs0.Color.A) * y + gs0.Color.A);
                byte rVal = (byte)((gs1.Color.R - gs0.Color.R) * y + gs0.Color.R);
                byte gVal = (byte)((gs1.Color.G - gs0.Color.G) * y + gs0.Color.G);
                byte bVal = (byte)((gs1.Color.B - gs0.Color.B) * y + gs0.Color.B);
                cx = Color.FromArgb(aVal, rVal, gVal, bVal);
            }
            return cx;
        }

        #endregion

    }
}
