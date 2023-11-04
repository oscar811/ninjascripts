#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators.TradeSaber
{
    public class MultiSeriesHL : Indicator
    {
        private double my1MinLow;
        private double my5MinLow;
        private double my15MinLow;
        private double my30MinLow;
        private double my60MinLow;
        private double my240MinLow;
        private double myDailyLow;
        private double myWeeklyLow;

        private double my1MinHigh;
        private double my5MinHigh;
        private double my15MinHigh;
        private double my30MinHigh;
        private double my60MinHigh;
        private double my240MinHigh;
        private double myDailyHigh;
        private double myWeeklyHigh;

        private double my1MinMedian;
        private double my5MinMedian;
        private double my15MinMedian;
        private double my30MinMedian;
        private double my60MinMedian;
        private double my240MinMedian;
        private double myDailyMedian;
        private double myWeeklyMedian;


        private int barsCurrent;

        private int barsAtLine1;
        private int barsAtLine5;
        private int barsAtLine15;
        private int barsAtLine30;
        private int barsAtLine60;
        private int barsAtLine240;
        private int barsAtLineDaily;
        private int barsAtLineWeekly;

        #region TradeSaber Social

        private string author = "TradeSaber(Dre)";
        private string version = "Version 1.2.0 // January 2023";

        private string youtube = "https://youtu.be/krEhUfAxW8Y";
        private string discord = "https://discord.gg/2YU9GDme8j";
        private string tradeSaber = "https://tradesaber.com/";

        private bool showSocials;

        private bool youtubeButtonClicked;
        private bool discordButtonClicked;
        private bool tradeSaberButtonClicked;

        private System.Windows.Controls.Button youtubeButton;
        private System.Windows.Controls.Button discordButton;
        private System.Windows.Controls.Button tradeSaberButton;


        private System.Windows.Controls.Grid myGrid29;

        #endregion

        private CurrentDayOHL myCurrentDayOHL;

        private double currentOHLHigh;
        private double currentOHLLow;
        private double currentOHLMedian;

        private double currentWeekMedian;
        private DateTime currentDate = Core.Globals.MinDate;
        private double currentWeekHigh = double.MinValue;
        private double currentWeekLow = double.MaxValue;
        private DateTime lastDate = Core.Globals.MinDate;
        private Data.SessionIterator sessionIterator;

        private DayOfWeek startWeekFromDay = DayOfWeek.Monday;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"Enter the description for your new custom Indicator here.";
                Name = "MultiSeriesHL";
                Calculate = Calculate.OnBarClose;
                IsOverlay = true;
                DisplayInDataBox = true;
                DrawOnPricePanel = true;
                DrawHorizontalGridLines = true;
                DrawVerticalGridLines = true;
                PaintPriceMarkers = true;
                ScaleJustification = NinjaTrader.Gui.Chart.ScaleJustification.Right;
                ArePlotsConfigurable = false;
                //Disable this property if your indicator requires custom values that cumulate with each new market data event. 
                //See Help Guide for additional information.
                IsSuspendedWhileInactive = true;


                //Levels
                min1 = false;
                min5 = false;
                min15 = false;
                min30 = false;
                min60 = true;
                min240 = true;
                minDaily = true;
                currentDaily = true;
                minWeekly = false;
                currentWeekly = true;

                //Colors
                min1Color = Brushes.Red;
                min5Color = Brushes.Orange;
                min15Color = Brushes.Blue;
                min30Color = Brushes.Pink;
                min60Color = Brushes.Purple;
                min240Color = Brushes.LimeGreen;
                minDailyColor = Brushes.White;
                currentDailyColor = Brushes.DarkRed;
                minWeeklyColor = Brushes.Goldenrod;
                currentWeeklyColor = Brushes.Tomato;

                //DashStyle
                min1Dash = DashStyleHelper.Dash;
                min5Dash = DashStyleHelper.Dash;
                min15Dash = DashStyleHelper.Dash;
                min30Dash = DashStyleHelper.Dash;
                min60Dash = DashStyleHelper.Dash;
                min240Dash = DashStyleHelper.Dash;
                minDailyDash = DashStyleHelper.Dash;
                currentDailyDash = DashStyleHelper.Dash;
                minWeeklyDash = DashStyleHelper.Dash;
                currentWeeklyDash = DashStyleHelper.Dash;

                //Line Width
                min1Width = 2;
                min5Width = 2;
                min15Width = 2;
                min30Width = 2;
                min60Width = 2;
                min240Width = 2;
                minDailyWidth = 2;
                currentDailyWidth = 2;
                minWeeklyWidth = 2;
                currentWeeklyWidth = 2;

                //Show Median Levels
                min1ShowMedian = false;
                min5ShowMedian = false;
                min15ShowMedian = false;
                min30ShowMedian = false;
                min60ShowMedian = false;
                min240ShowMedian = false;
                minDailyShowMedian = false;
                currentDailyShowMedian = false;
                minWeeklyShowMedian = false;
                currentWeeklyShowMedian = false;

                //Show Labels
                min1ShowLabel = true;
                min5ShowLabel = true;
                min15ShowLabel = true;
                min30ShowLabel = true;
                min60ShowLabel = true;
                min240ShowLabel = true;
                minDailyShowLabel = true;
                currentDailyShowLabel = true;
                minWeeklyShowLabel = true;
                currentWeeklyShowLabel = true;

                showSocials = true;

                StartWeekFromDay = DayOfWeek.Monday;
            }
            else if (State == State.Configure)
            {
                //[0][0] = current data series
                AddDataSeries(Data.BarsPeriodType.Minute, 1); //[1][0]
                AddDataSeries(Data.BarsPeriodType.Minute, 5); //[2][0]	
                AddDataSeries(Data.BarsPeriodType.Minute, 15); //[3][0]
                AddDataSeries(Data.BarsPeriodType.Minute, 30); //[4][0]
                AddDataSeries(Data.BarsPeriodType.Minute, 60); //[5][0]
                AddDataSeries(Data.BarsPeriodType.Minute, 240); //[6][0]
                AddDataSeries(Data.BarsPeriodType.Day, 1); //[7][0]
                AddDataSeries(Data.BarsPeriodType.Week, 1); //[8][0]

                currentDate = Core.Globals.MinDate;

                currentWeekHigh = double.MinValue;
                currentWeekLow = double.MaxValue;
                lastDate = Core.Globals.MinDate;
            }

            else if (State == State.DataLoaded)
            {
                ClearOutputWindow();

                sessionIterator = new Data.SessionIterator(Bars);
            }


            else if (State == State.Historical)
            {
                if (!Bars.BarsType.IsIntraday)
                {
                    if (currentDaily)
                    {
                        Draw.TextFixed(this, "CurrentDayErr", "Current Day High/Low Only works on Intraday\n", TextPosition.BottomRight);
                    }

                    if (currentWeekly)
                    {
                        Draw.TextFixed(this, "CurrentWeekErr", "Current Week High/Low Only works on Intraday", TextPosition.BottomRight);
                    }
                }

                #region TradeSaber Socials

                if (showSocials)
                {
                    if (UserControlCollection.Contains(myGrid29))
                        return;

                    Dispatcher.InvokeAsync((() =>
                    {
                        myGrid29 = new System.Windows.Controls.Grid
                        {
                            Name = "MyCustomGrid",
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Bottom
                        };

                        System.Windows.Controls.ColumnDefinition column1 = new System.Windows.Controls.ColumnDefinition();
                        System.Windows.Controls.ColumnDefinition column2 = new System.Windows.Controls.ColumnDefinition();
                        System.Windows.Controls.ColumnDefinition column3 = new System.Windows.Controls.ColumnDefinition();

                        myGrid29.ColumnDefinitions.Add(column1);
                        myGrid29.ColumnDefinitions.Add(column2);
                        myGrid29.ColumnDefinitions.Add(column3);

                        youtubeButton = new System.Windows.Controls.Button
                        {
                            Name = "YoutubeButton",
                            Content = "Youtube",
                            Foreground = Brushes.White,
                            Background = Brushes.Red
                        };

                        discordButton = new System.Windows.Controls.Button
                        {
                            Name = "DiscordButton",
                            Content = "Discord",
                            Foreground = Brushes.White,
                            Background = Brushes.RoyalBlue
                        };

                        tradeSaberButton = new System.Windows.Controls.Button
                        {
                            Name = "TradeSaberButton",
                            Content = "TradeSaber",
                            Foreground = Brushes.White,
                            Background = Brushes.DarkOrange
                        };

                        youtubeButton.Click += OnButtonClick;
                        discordButton.Click += OnButtonClick;
                        tradeSaberButton.Click += OnButtonClick;

                        System.Windows.Controls.Grid.SetColumn(youtubeButton, 0);
                        System.Windows.Controls.Grid.SetColumn(discordButton, 1);
                        System.Windows.Controls.Grid.SetColumn(tradeSaberButton, 2);

                        myGrid29.Children.Add(youtubeButton);
                        myGrid29.Children.Add(discordButton);
                        myGrid29.Children.Add(tradeSaberButton);

                        UserControlCollection.Add(myGrid29);
                    }));
                }
                #endregion

                else if (State == State.Terminated)
                {
                    #region Terminate TradeSaber Socials

                    if (showSocials)
                    {
                        Dispatcher.InvokeAsync((() =>
                        {
                            if (myGrid29 != null)
                            {
                                if (youtubeButton != null)
                                {
                                    myGrid29.Children.Remove(youtubeButton);
                                    youtubeButton.Click -= OnButtonClick;
                                    youtubeButton = null;
                                }

                                if (discordButton != null)
                                {
                                    myGrid29.Children.Remove(discordButton);
                                    discordButton.Click -= OnButtonClick;
                                    discordButton = null;
                                }

                                if (tradeSaberButton != null)
                                {
                                    myGrid29.Children.Remove(tradeSaberButton);
                                    tradeSaberButton.Click -= OnButtonClick;
                                    tradeSaberButton = null;
                                }
                            }
                        }));
                    }
                    #endregion
                }
            }


        }

        protected override void OnBarUpdate()
        {

            #region Calculate Levels

            if (CurrentBars[0] < 1 || CurrentBars[1] < 1 || CurrentBars[2] < 1 || CurrentBars[3] < 1 || CurrentBars[4] < 1 || CurrentBars[5] < 1 || CurrentBars[6] < 1)// || CurrentBars[7] < BarsRequiredToPlot) 
                return;

            if (BarsInProgress == 0)
            {
                barsCurrent = CurrentBars[0];

                #region Current Daily

                if (currentDaily)
                {
                    currentOHLHigh = CurrentDayOHL().CurrentHigh[0];
                    currentOHLLow = CurrentDayOHL().CurrentLow[0];

                    currentOHLMedian = (currentOHLHigh + currentOHLLow) / 2;
                }

                #endregion

                #region Current Weekly

                if (currentWeekly)
                {
                    if (!Bars.BarsType.IsIntraday)
                        return;

                    lastDate = currentDate;

                    if (sessionIterator.GetTradingDay(Time[0]).DayOfWeek == startWeekFromDay)
                    {
                        currentDate = sessionIterator.GetTradingDay(Time[0]);
                    }

                    if (lastDate != currentDate)
                    {
                        currentWeekHigh = High[0];
                        currentWeekLow = Low[0];
                    }

                    currentWeekHigh = Math.Max(currentWeekHigh, High[0]);
                    currentWeekLow = Math.Min(currentWeekLow, Low[0]);

                    currentWeekMedian = (currentWeekHigh + currentWeekLow) / 2;
                }

                #endregion
            }

            #region 1 Min

            if (BarsInProgress == 1 && CurrentBars[1] > 0)
            {
                my1MinHigh = Highs[1][0];
                my1MinLow = Lows[1][0];
                my1MinMedian = Medians[1][0];

                barsAtLine1 = CurrentBars[0];
            }

            #endregion

            #region 5 Min

            if (BarsInProgress == 2 && CurrentBars[2] > 0)
            {
                my5MinHigh = Highs[2][0];
                my5MinLow = Lows[2][0];
                my5MinMedian = Medians[2][0];

                barsAtLine5 = CurrentBars[0];
            }

            #endregion

            #region 15 Min

            if (BarsInProgress == 3 && CurrentBars[3] > 0)
            {
                my15MinHigh = Highs[3][0];
                my15MinLow = Lows[3][0];
                my15MinMedian = Medians[3][0];

                barsAtLine15 = CurrentBars[0];
            }

            #endregion

            #region 30 Min

            if (BarsInProgress == 4 && CurrentBars[4] > 0)
            {
                my30MinHigh = Highs[4][0];
                my30MinLow = Lows[4][0];
                my30MinMedian = Medians[4][0];

                barsAtLine30 = CurrentBars[0];
            }

            #endregion

            #region 60 Min

            if (BarsInProgress == 5 && CurrentBars[5] > 0)
            {
                my60MinHigh = Highs[5][0];
                my60MinLow = Lows[5][0];
                my60MinMedian = Medians[5][0];

                barsAtLine60 = CurrentBars[0];
            }

            #endregion

            #region 240 Min

            if (BarsInProgress == 6 && CurrentBars[6] > 0)
            {
                my240MinHigh = Highs[6][0];
                my240MinLow = Lows[6][0];
                my240MinMedian = Medians[6][0];

                barsAtLine240 = CurrentBars[0];
            }

            #endregion

            #region Previous Daily

            if (BarsInProgress == 7)
            {
                myDailyHigh = Highs[7][0];
                myDailyLow = Lows[7][0];
                myDailyMedian = Medians[7][0];

                barsAtLineDaily = CurrentBars[0];
            }

            #endregion

            #region Previous Weekly

            if (BarsInProgress == 8)
            {
                myWeeklyHigh = Highs[8][0];
                myWeeklyLow = Lows[8][0];
                myWeeklyMedian = Medians[8][0];

                barsAtLineWeekly = CurrentBars[0];
            }

            #endregion

            #endregion

            #region Draw Lines

            NinjaTrader.Gui.Tools.SimpleFont myFont = new NinjaTrader.Gui.Tools.SimpleFont("Font", 12) { Size = 15, Bold = true };

            #region Previous 1

            if (min1)
            {
                Draw.Line(this, "1High", false, (barsCurrent - barsAtLine1), my1MinHigh, -1, my1MinHigh, min1Color, min1Dash, min1Width);
                Draw.Line(this, "1Low", false, (barsCurrent - barsAtLine1), my1MinLow, -1, my1MinLow, min1Color, min1Dash, min1Width);

                if (min1ShowLabel)
                {
                    Draw.Text(this, "1HighText", false, "Previous 1 High: " + my1MinHigh, -5, my1MinHigh, 5, min1Color, myFont, TextAlignment.Left, Brushes.Transparent, null, 1);
                    Draw.Text(this, "1LowText", false, "Previous 1 Low: " + my1MinLow, -5, my1MinLow, 5, min1Color, myFont, TextAlignment.Left, Brushes.Transparent, null, 1);
                }

                if (min1ShowMedian)
                {
                    Draw.Line(this, "1Median", false, (barsCurrent - barsAtLine1), my1MinMedian, -1, my1MinMedian, min1Color, min1Dash, min1Width);

                    if (min1ShowLabel)
                    {
                        Draw.Text(this, "1MedianText", false, "Previous 1 Median: " + my1MinMedian, -5, my1MinMedian, 5, min1Color, myFont, TextAlignment.Left, Brushes.Transparent, null, 1);
                    }
                }
            }

            #endregion

            #region Previous 5

            if (min5)
            {
                Draw.Line(this, "5High", false, (barsCurrent - barsAtLine5), my5MinHigh, -1, my5MinHigh, min5Color, min5Dash, min5Width);
                Draw.Line(this, "5Low", false, (barsCurrent - barsAtLine5), my5MinLow, -1, my5MinLow, min5Color, min5Dash, min5Width);

                if (min5ShowLabel)
                {
                    Draw.Text(this, "5HighText", false, "Previous 5 High: " + my5MinHigh, -5, my5MinHigh, 5, min5Color, myFont, TextAlignment.Left, Brushes.Transparent, null, 1);
                    Draw.Text(this, "5LowText", false, "Previous 5 Low: " + my5MinLow, -5, my5MinLow, 5, min5Color, myFont, TextAlignment.Left, Brushes.Transparent, null, 1);
                }

                if (min5ShowMedian)
                {
                    Draw.Line(this, "5Median", false, (barsCurrent - barsAtLine5), my5MinMedian, -1, my5MinMedian, min5Color, min5Dash, min5Width);

                    if (min5ShowLabel)
                    {
                        Draw.Text(this, "5MedianText", false, "Previous 5 Median: " + my5MinMedian, -5, my5MinMedian, 5, min5Color, myFont, TextAlignment.Left, Brushes.Transparent, null, 1);
                    }
                }
            }

            #endregion

            #region Previous 15

            if (min15)
            {
                Draw.Line(this, "15High", false, (barsCurrent - barsAtLine15), my15MinHigh, -1, my15MinHigh, min15Color, min15Dash, min15Width);
                Draw.Line(this, "15Low", false, (barsCurrent - barsAtLine15), my15MinLow, -1, my15MinLow, min15Color, min15Dash, min15Width);

                if (min15ShowLabel)
                {
                    Draw.Text(this, "15HighText", false, "Previous 15 High: " + my15MinHigh, -5, my15MinHigh, 5, min15Color, myFont, TextAlignment.Left, Brushes.Transparent, null, 1);
                    Draw.Text(this, "15LowText", false, "Previous 15 Low: " + my15MinLow, -5, my15MinLow, 5, min15Color, myFont, TextAlignment.Left, Brushes.Transparent, null, 1);
                }

                if (min15ShowMedian)
                {
                    Draw.Line(this, "15Median", false, (barsCurrent - barsAtLine15), my15MinMedian, -1, my15MinMedian, min15Color, min15Dash, min15Width);

                    if (min15ShowLabel)
                    {
                        Draw.Text(this, "15MedianText", false, "Previous 15 Median: " + my15MinMedian, -5, my15MinMedian, 5, min15Color, myFont, TextAlignment.Left, Brushes.Transparent, null, 1);
                    }
                }
            }

            #endregion

            #region Previous 30

            if (min30)
            {
                Draw.Line(this, "30High", false, (barsCurrent - barsAtLine30), my30MinHigh, -1, my30MinHigh, min30Color, min30Dash, min30Width);
                Draw.Line(this, "30Low", false, (barsCurrent - barsAtLine30), my30MinLow, -1, my30MinLow, min30Color, min30Dash, min30Width);

                if (min30ShowLabel)
                {
                    Draw.Text(this, "30HighText", false, "Previous 30 High: " + my30MinHigh, -5, my30MinHigh, 5, min30Color, myFont, TextAlignment.Left, Brushes.Transparent, null, 1);
                    Draw.Text(this, "30LowText", false, "Previous 30 Low: " + my30MinLow, -5, my30MinLow, 5, min30Color, myFont, TextAlignment.Left, Brushes.Transparent, null, 1);
                }

                if (min30ShowMedian)
                {
                    Draw.Line(this, "30Median", false, (barsCurrent - barsAtLine30), my30MinMedian, -1, my30MinMedian, min30Color, min30Dash, min30Width);

                    if (min30ShowLabel)
                    {
                        Draw.Text(this, "30MedianText", false, "Previous 30 Median: " + my30MinMedian, -5, my30MinMedian, 5, min30Color, myFont, TextAlignment.Left, Brushes.Transparent, null, 1);
                    }
                }
            }

            #endregion

            #region Previous 60

            if (min60)
            {
                Draw.Line(this, "60High", false, (barsCurrent - barsAtLine60), my60MinHigh, -1, my60MinHigh, min60Color, min60Dash, min60Width);
                Draw.Line(this, "60Low", false, (barsCurrent - barsAtLine60), my60MinLow, -1, my60MinLow, min60Color, min60Dash, min60Width);

                if (min60ShowLabel)
                {
                    Draw.Text(this, "60HighText", false, "Previous 60 High: " + my60MinHigh, -5, my60MinHigh, 5, min60Color, myFont, TextAlignment.Left, Brushes.Transparent, null, 1);
                    Draw.Text(this, "60LowText", false, "Previous 60 Low: " + my60MinLow, -5, my60MinLow, 5, min60Color, myFont, TextAlignment.Left, Brushes.Transparent, null, 1);
                }
                if (min60ShowMedian)
                {
                    Draw.Line(this, "60Median", false, (barsCurrent - barsAtLine60), my60MinMedian, -1, my60MinMedian, min60Color, min60Dash, min60Width);

                    if (min60ShowLabel)
                    {
                        Draw.Text(this, "60MedianText", false, "Previous 60 Median: " + my60MinMedian, -5, my60MinMedian, 5, min60Color, myFont, TextAlignment.Left, Brushes.Transparent, null, 1);
                    }
                }
            }

            #endregion

            #region Previous 240

            if (min240)
            {
                Draw.Line(this, "240High", false, (barsCurrent - barsAtLine240), my240MinHigh, -1, my240MinHigh, min240Color, min240Dash, min240Width);
                Draw.Line(this, "240Low", false, (barsCurrent - barsAtLine240), my240MinLow, -1, my240MinLow, min240Color, min240Dash, min240Width);

                if (min240ShowLabel)
                {
                    Draw.Text(this, "240HighText", false, "Previous 240 High: " + my240MinHigh, -5, my240MinHigh, 5, min240Color, myFont, TextAlignment.Left, Brushes.Transparent, null, 1);
                    Draw.Text(this, "240LowText", false, "Previous 240 Low: " + my240MinLow, -5, my240MinLow, 5, min240Color, myFont, TextAlignment.Left, Brushes.Transparent, null, 1);
                }

                if (min240ShowMedian)
                {
                    Draw.Line(this, "240Median", false, (barsCurrent - barsAtLine240), my240MinMedian, -1, my240MinMedian, min240Color, min240Dash, min240Width);

                    if (min240ShowLabel)
                    {
                        Draw.Text(this, "240MedianText", false, "Previous 240 Median: " + my240MinMedian, -5, my240MinMedian, 5, min240Color, myFont, TextAlignment.Left, Brushes.Transparent, null, 1);
                    }
                }
            }

            #endregion

            #region Previous Daily

            if (minDaily)
            {
                Draw.Line(this, "DailyHigh", false, (barsCurrent - barsAtLineDaily), myDailyHigh, -1, myDailyHigh, minDailyColor, minDailyDash, minDailyWidth);
                Draw.Line(this, "DailyLow", false, (barsCurrent - barsAtLineDaily), myDailyLow, -1, myDailyLow, minDailyColor, minDailyDash, minDailyWidth);

                if (minDailyShowLabel)
                {
                    Draw.Text(this, "DailyHighText", false, "Previous Daily High: " + myDailyHigh, -5, myDailyHigh, 5, minDailyColor, myFont, TextAlignment.Left, Brushes.Transparent, null, 1);
                    Draw.Text(this, "DailyLowText", false, "Previous Daily Low: " + myDailyLow, -5, myDailyLow, 5, minDailyColor, myFont, TextAlignment.Left, Brushes.Transparent, null, 1);
                }

                if (minDailyShowMedian)
                {
                    Draw.Line(this, "DailyMedian", false, (barsCurrent - barsAtLineDaily), myDailyMedian, -1, myDailyMedian, minDailyColor, minDailyDash, minDailyWidth);

                    if (minDailyShowLabel)
                    {
                        Draw.Text(this, "DailyMedianText", false, "Previous Daily Median: " + myDailyMedian, -5, myDailyMedian, 5, minDailyColor, myFont, TextAlignment.Left, Brushes.Transparent, null, 1);
                    }
                }
            }

            #endregion

            #region Current Daily

            if (currentDaily)
            {
                Draw.Line(this, "CurrentDailyHigh", false, (barsCurrent - barsAtLineDaily), currentOHLHigh, -1, currentOHLHigh, currentDailyColor, currentDailyDash, currentDailyWidth);
                Draw.Line(this, "CurrentDailyLow", false, (barsCurrent - barsAtLineDaily), currentOHLLow, -1, currentOHLLow, currentDailyColor, currentDailyDash, currentDailyWidth);

                if (currentDailyShowLabel)
                {
                    Draw.Text(this, "CurrentDailyHighText", false, "Current Daily High: " + currentOHLHigh, -5, currentOHLHigh, 5, currentDailyColor, myFont, TextAlignment.Left, Brushes.Transparent, null, 1);
                    Draw.Text(this, "CurrentDailyLowText", false, "Current Daily Low: " + currentOHLLow, -5, currentOHLLow, 5, currentDailyColor, myFont, TextAlignment.Left, Brushes.Transparent, null, 1);
                }
                if (currentDailyShowMedian)
                {
                    Draw.Line(this, "CurrentDailyMedian", false, (barsCurrent - barsAtLineDaily), currentOHLMedian, -1, currentOHLMedian, currentDailyColor, currentDailyDash, currentDailyWidth);

                    if (currentDailyShowLabel)
                    {
                        Draw.Text(this, "CurrentDailyMedianText", false, "Current Daily Median: " + currentOHLMedian, -5, currentOHLMedian, 5, currentDailyColor, myFont, TextAlignment.Left, Brushes.Transparent, null, 1);
                    }
                }
            }

            #endregion

            #region Previous Weekly

            if (minWeekly)
            {
                Draw.Line(this, "WeeklyHigh", false, (barsCurrent - barsAtLineWeekly), myWeeklyHigh, -1, myWeeklyHigh, minWeeklyColor, minWeeklyDash, minWeeklyWidth);
                Draw.Line(this, "WeeklyLow", false, (barsCurrent - barsAtLineWeekly), myWeeklyLow, -1, myWeeklyLow, minWeeklyColor, minWeeklyDash, minWeeklyWidth);

                if (minWeeklyShowLabel)
                {
                    Draw.Text(this, "WeeklyHighText", false, "Previous Weekly High: " + myWeeklyHigh, -5, myWeeklyHigh, 5, minWeeklyColor, myFont, TextAlignment.Left, Brushes.Transparent, null, 1);
                    Draw.Text(this, "WeeklyLowText", false, "Previous Weekly Low: " + myWeeklyLow, -5, myWeeklyLow, 5, minWeeklyColor, myFont, TextAlignment.Left, Brushes.Transparent, null, 1);
                }

                if (minWeeklyShowMedian)
                {
                    Draw.Line(this, "WeeklyMedian", false, (barsCurrent - barsAtLineWeekly), myWeeklyMedian, -1, myWeeklyMedian, minWeeklyColor, minWeeklyDash, minWeeklyWidth);

                    if (minWeeklyShowLabel)
                    {
                        Draw.Text(this, "WeeklyMedianText", false, "Previous Weekly Median: " + myWeeklyMedian, -5, myWeeklyMedian, 5, minWeeklyColor, myFont, TextAlignment.Left, Brushes.Transparent, null, 1);
                    }
                }
            }

            #endregion

            #region Current Weekly

            if (currentWeekly)
            {
                Draw.Line(this, "CurrentWeeklyHigh", false, (barsCurrent - barsAtLineWeekly), currentWeekHigh, -1, currentWeekHigh, currentWeeklyColor, currentWeeklyDash, currentWeeklyWidth);
                Draw.Line(this, "CurrentWeeklyLow", false, (barsCurrent - barsAtLineWeekly), currentWeekLow, -1, currentWeekLow, currentWeeklyColor, currentWeeklyDash, currentWeeklyWidth);

                if (currentWeeklyShowLabel)
                {
                    Draw.Text(this, "CurrentWeeklyHighText", false, "Current Weekly High: " + currentWeekHigh, -5, currentWeekHigh, 5, currentWeeklyColor, myFont, TextAlignment.Left, Brushes.Transparent, null, 1);
                    Draw.Text(this, "CurrentWeeklyLowText", false, "Current Weekly Low: " + currentWeekLow, -5, currentWeekLow, 5, currentWeeklyColor, myFont, TextAlignment.Left, Brushes.Transparent, null, 1);
                }
                if (currentWeeklyShowMedian)
                {
                    Draw.Line(this, "CurrentWeeklyMedian", false, (barsCurrent - barsAtLineWeekly), currentWeekMedian, -1, currentWeekMedian, currentWeeklyColor, currentWeeklyDash, currentWeeklyWidth);

                    if (currentWeeklyShowLabel)
                    {
                        Draw.Text(this, "CurrentWeeklyMedianText", false, "Current Weekly Median: " + currentWeekMedian, -5, currentWeekMedian, 5, currentWeeklyColor, myFont, TextAlignment.Left, Brushes.Transparent, null, 1);
                    }
                }
            }

            #endregion

            #endregion

        }

        #region Button Click Event

        private void OnButtonClick(object sender, RoutedEventArgs rea)
        {
            System.Windows.Controls.Button button = sender as System.Windows.Controls.Button;

            #region TradeSaber Socials

            if (showSocials)
            {
                if (button == youtubeButton && button.Name == "YoutubeButton" && button.Content == "Youtube")
                {
                    System.Diagnostics.Process.Start(youtube);
                    return;
                }

                if (button == discordButton && button.Name == "DiscordButton" && button.Content == "Discord")
                {
                    System.Diagnostics.Process.Start(discord);
                    return;
                }

                if (button == tradeSaberButton && button.Name == "TradeSaberButton" && button.Content == "TradeSaber")
                {
                    System.Diagnostics.Process.Start(tradeSaber);
                    return;
                }
            }

            #endregion
        }

        #endregion


        #region Properties

        #region 00. Display Levels

        [NinjaScriptProperty]
        [Display(Name = "1 Minute", Order = 1, GroupName = "00. Display Levels")]
        public bool min1
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "5 Minute", Order = 2, GroupName = "00. Display Levels")]
        public bool min5
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "15 Minute", Order = 3, GroupName = "00. Display Levels")]
        public bool min15
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "30 Minute", Order = 4, GroupName = "00. Display Levels")]
        public bool min30
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "60 Minute", Order = 5, GroupName = "00. Display Levels")]
        public bool min60
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "240 Minute/4 Hour", Order = 6, GroupName = "00. Display Levels")]
        public bool min240
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Previous Daily Levels", Order = 7, GroupName = "00. Display Levels")]
        public bool minDaily
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Current Daily Levels", Order = 8, GroupName = "00. Display Levels")]
        public bool currentDaily
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Previous Weekly Levels", Order = 9, GroupName = "00. Display Levels")]
        public bool minWeekly
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Current Weekly Levels", Order = 10, GroupName = "00. Display Levels")]
        public bool currentWeekly
        { get; set; }

        #endregion

        #region 01. 1min Level Customize

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(ResourceType = typeof(Custom.Resource), Name = "1 Minute Color", GroupName = "01. Customize 1 Min Lines", Order = 1)]
        public Brush min1Color
        { get; set; }

        [Browsable(false)]
        public string min1ColorSerializable
        {
            get { return Serialize.BrushToString(min1Color); }
            set { min1Color = Serialize.StringToBrush(value); }
        }

        [NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "1 Minute Dash Style", GroupName = "01. Customize 1 Min Lines", Order = 2)]
        public DashStyleHelper min1Dash
        { get; set; }

        [NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "1 Minute Line Width", GroupName = "01. Customize 1 Min Lines", Order = 3)]
        public int min1Width
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Show 1 Min Median Levels", Order = 4, GroupName = "01. Customize 1 Min Lines")]
        public bool min1ShowMedian
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Show 1 Min Labels", Order = 5, GroupName = "01. Customize 1 Min Lines")]
        public bool min1ShowLabel
        { get; set; }

        #endregion

        #region 02. 5min Level Customize

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(ResourceType = typeof(Custom.Resource), Name = "5 Minute Color", GroupName = "02. Customize 5 Min Lines", Order = 0)]
        public Brush min5Color
        { get; set; }

        [Browsable(false)]
        public string min5ColorSerializable
        {
            get { return Serialize.BrushToString(min5Color); }
            set { min5Color = Serialize.StringToBrush(value); }
        }

        [NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "5 Minute Dash Style", GroupName = "02. Customize 5 Min Lines", Order = 2)]
        public DashStyleHelper min5Dash
        { get; set; }

        [NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "5 Minute Line Width", GroupName = "02. Customize 5 Min Lines", Order = 3)]
        public int min5Width
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Show 5 Min Median Levels", Order = 4, GroupName = "02. Customize 5 Min Lines")]
        public bool min5ShowMedian
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Show 5 Min Labels", Order = 5, GroupName = "02. Customize 5 Min Lines")]
        public bool min5ShowLabel
        { get; set; }

        #endregion

        #region 03. 15min Level Customize

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(ResourceType = typeof(Custom.Resource), Name = "15 Minute Color", GroupName = "03. Customize 15 Min Lines", Order = 0)]
        public Brush min15Color
        { get; set; }

        [Browsable(false)]
        public string min15ColorSerializable
        {
            get { return Serialize.BrushToString(min15Color); }
            set { min15Color = Serialize.StringToBrush(value); }
        }

        [NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "15 Minute Dash Style", GroupName = "03. Customize 15 Min Lines", Order = 2)]
        public DashStyleHelper min15Dash
        { get; set; }

        [NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "15 Minute Line Width", GroupName = "03. Customize 15 Min Lines", Order = 3)]
        public int min15Width
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Show 15 Min Median Levels", Order = 4, GroupName = "03. Customize 15 Min Lines")]
        public bool min15ShowMedian
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Show 15 Min Labels", Order = 5, GroupName = "03. Customize 15 Min Lines")]
        public bool min15ShowLabel
        { get; set; }

        #endregion

        #region 04. 30min Level Customize

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(ResourceType = typeof(Custom.Resource), Name = "30 Minute Color", GroupName = "04. Customize 30 Min Lines", Order = 0)]
        public Brush min30Color
        { get; set; }

        [Browsable(false)]
        public string min30ColorSerializable
        {
            get { return Serialize.BrushToString(min30Color); }
            set { min30Color = Serialize.StringToBrush(value); }
        }

        [NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "30 Minute Dash Style", GroupName = "04. Customize 30 Min Lines", Order = 2)]
        public DashStyleHelper min30Dash
        { get; set; }

        [NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "30 Minute Line Width", GroupName = "04. Customize 30 Min Lines", Order = 3)]
        public int min30Width
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Show 30 Min Median Levels", Order = 4, GroupName = "04. Customize 30 Min Lines")]
        public bool min30ShowMedian
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Show 30 Min Labels", Order = 5, GroupName = "04. Customize 30 Min Lines")]
        public bool min30ShowLabel
        { get; set; }

        #endregion

        #region 05. 60min Level Customize

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(ResourceType = typeof(Custom.Resource), Name = "60 Minute Color", GroupName = "05. Customize 60 Min Lines", Order = 0)]
        public Brush min60Color
        { get; set; }

        [Browsable(false)]
        public string min60ColorSerializable
        {
            get { return Serialize.BrushToString(min60Color); }
            set { min60Color = Serialize.StringToBrush(value); }
        }

        [NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "60 Minute Dash Style", GroupName = "05. Customize 60 Min Lines", Order = 2)]
        public DashStyleHelper min60Dash
        { get; set; }

        [NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "60 Minute Line Width", GroupName = "05. Customize 60 Min Lines", Order = 3)]
        public int min60Width
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Show 60 Min Median Levels", Order = 4, GroupName = "05. Customize 60 Min Lines")]
        public bool min60ShowMedian
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Show 60 Min Labels", Order = 5, GroupName = "05. Customize 60 Min Lines")]
        public bool min60ShowLabel
        { get; set; }

        #endregion

        #region 06. 240min Level Customize

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(ResourceType = typeof(Custom.Resource), Name = "240 Minute Color", GroupName = "06. Customize 240 Min Lines", Order = 0)]
        public Brush min240Color
        { get; set; }

        [Browsable(false)]
        public string min240ColorSerializable
        {
            get { return Serialize.BrushToString(min240Color); }
            set { min240Color = Serialize.StringToBrush(value); }
        }

        [NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "240 Minute Dash Style", GroupName = "06. Customize 240 Min Lines", Order = 2)]
        public DashStyleHelper min240Dash
        { get; set; }

        [NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "240 Minute Line Width", GroupName = "06. Customize 240 Min Lines", Order = 3)]
        public int min240Width
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Show 240 Min Median Levels", Order = 4, GroupName = "06. Customize 240 Min Lines")]
        public bool min240ShowMedian
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Show 240 Min Labels", Order = 5, GroupName = "06. Customize 240 Min Lines")]
        public bool min240ShowLabel
        { get; set; }

        #endregion

        #region 07. Previous Daily Level Customize

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(ResourceType = typeof(Custom.Resource), Name = "Previous Daily Color", GroupName = "07. Customize Previous Daily Lines", Order = 0)]
        public Brush minDailyColor
        { get; set; }

        [Browsable(false)]
        public string minDailyColorSerializable
        {
            get { return Serialize.BrushToString(minDailyColor); }
            set { minDailyColor = Serialize.StringToBrush(value); }
        }

        [NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "Previous Daily Dash Style", GroupName = "07. Customize Previous Daily Lines", Order = 2)]
        public DashStyleHelper minDailyDash
        { get; set; }

        [NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "Previous Daily Minute Line Width", GroupName = "07. Customize Previous Daily Lines", Order = 3)]
        public int minDailyWidth
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Show Previous Daily Median Levels", Order = 4, GroupName = "07. Customize Previous Daily Lines")]
        public bool minDailyShowMedian
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Show Previous Daily Labels", Order = 5, GroupName = "07. Customize Previous Daily Lines")]
        public bool minDailyShowLabel
        { get; set; }

        #endregion

        #region 08. Current Daily Level Customize

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(ResourceType = typeof(Custom.Resource), Name = "Current Daily Color", GroupName = "08. Customize Current Daily Lines", Order = 0)]
        public Brush currentDailyColor
        { get; set; }

        [Browsable(false)]
        public string currentDailyColorSerializable
        {
            get { return Serialize.BrushToString(currentDailyColor); }
            set { currentDailyColor = Serialize.StringToBrush(value); }
        }

        [NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "Current Daily Dash Style", GroupName = "08. Customize Current Daily Lines", Order = 2)]
        public DashStyleHelper currentDailyDash
        { get; set; }

        [NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "Current Daily Minute Line Width", GroupName = "08. Customize Current Daily Lines", Order = 3)]
        public int currentDailyWidth
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Show Current Daily Median Levels", Order = 4, GroupName = "08. Customize Current Daily Lines")]
        public bool currentDailyShowMedian
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Show Current Daily Labels", Order = 5, GroupName = "08. Customize Current Daily Lines")]
        public bool currentDailyShowLabel
        { get; set; }

        #endregion

        #region 09. Previous Weekly Level Customize

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(ResourceType = typeof(Custom.Resource), Name = "Previous Weekly Color", GroupName = "09. Customize Previous Weekly Lines", Order = 0)]
        public Brush minWeeklyColor
        { get; set; }

        [Browsable(false)]
        public string minWeeklyColorSerializable
        {
            get { return Serialize.BrushToString(minWeeklyColor); }
            set { minWeeklyColor = Serialize.StringToBrush(value); }
        }

        [NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "Previous Weekly Dash Style", GroupName = "09. Customize Previous Weekly Lines", Order = 2)]
        public DashStyleHelper minWeeklyDash
        { get; set; }

        [NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "Previous Weekly Minute Line Width", GroupName = "09. Customize Previous Weekly Lines", Order = 3)]
        public int minWeeklyWidth
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Show Previous Weekly Median Levels", Order = 4, GroupName = "09. Customize Previous Weekly Lines")]
        public bool minWeeklyShowMedian
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Show Previous Weekly Labels", Order = 5, GroupName = "09. Customize Previous Weekly Lines")]
        public bool minWeeklyShowLabel
        { get; set; }

        #endregion

        #region 10. Current Weekly Level Customize

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(ResourceType = typeof(Custom.Resource), Name = "Current Weekly Color", GroupName = "10. Customize Current Weekly Lines", Order = 0)]
        public Brush currentWeeklyColor
        { get; set; }

        [Browsable(false)]
        public string currentWeeklyColorSerializable
        {
            get { return Serialize.BrushToString(currentWeeklyColor); }
            set { currentWeeklyColor = Serialize.StringToBrush(value); }
        }

        [NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "Current Weekly Dash Style", GroupName = "10. Customize Current Weekly Lines", Order = 2)]
        public DashStyleHelper currentWeeklyDash
        { get; set; }

        [NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "Current Weekly Minute Line Width", GroupName = "10. Customize Current Weekly Lines", Order = 3)]
        public int currentWeeklyWidth
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Show Current Weekly Median Levels", Order = 4, GroupName = "10. Customize Current Weekly Lines")]
        public bool currentWeeklyShowMedian
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Show Current Weekly Labels", Order = 5, GroupName = "10. Customize Current Weekly Lines")]
        public bool currentWeeklyShowLabel
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Start Week Day: ", Order = 6, GroupName = "10. Customize Current Weekly Lines")]
        public DayOfWeek StartWeekFromDay
        {
            get { return startWeekFromDay; }
            set { startWeekFromDay = value; }
        }

        #endregion

        #region 29. TradeSaber Socials

        [NinjaScriptProperty]
        [Display(Name = "Show Social Media Buttons", Description = "", Order = 0, GroupName = "29. TradeSaber Socials")]
        public bool ShowSocials
        {
            get { return showSocials; }
            set { showSocials = (value); }
        }

        [NinjaScriptProperty]
        [Display(Name = "Explanation Video", Order = 1, GroupName = "29. TradeSaber Socials")]
        public string Youtube
        {
            get { return youtube; }
            set { youtube = (value); }
        }

        [NinjaScriptProperty]
        [Display(Name = "Discord Link", Order = 2, GroupName = "29. TradeSaber Socials")]
        public string Discord
        {
            get { return discord; }
            set { discord = (value); }
        }

        [NinjaScriptProperty]
        [Display(Name = "TradeSaber Link", Order = 3, GroupName = "29. TradeSaber Socials")]
        public string TradeSaber
        {
            get { return tradeSaber; }
            set { tradeSaber = (value); }
        }

        [NinjaScriptProperty]
        [ReadOnly(true)]
        [Display(Name = "Author", GroupName = "29. TradeSaber Socials", Order = 4)]
        public string Author
        {
            get { return author; }
            set { author = (value); }
        }

        [NinjaScriptProperty]
        [ReadOnly(true)]
        [Display(Name = "Version", GroupName = "29. TradeSaber Socials", Order = 5)]
        public string Version
        {
            get { return version; }
            set { version = (value); }
        }

        #endregion

        #endregion

    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private TradeSaber.MultiSeriesHL[] cacheMultiSeriesHL;
		public TradeSaber.MultiSeriesHL MultiSeriesHL(bool min1, bool min5, bool min15, bool min30, bool min60, bool min240, bool minDaily, bool currentDaily, bool minWeekly, bool currentWeekly, Brush min1Color, DashStyleHelper min1Dash, int min1Width, bool min1ShowMedian, bool min1ShowLabel, Brush min5Color, DashStyleHelper min5Dash, int min5Width, bool min5ShowMedian, bool min5ShowLabel, Brush min15Color, DashStyleHelper min15Dash, int min15Width, bool min15ShowMedian, bool min15ShowLabel, Brush min30Color, DashStyleHelper min30Dash, int min30Width, bool min30ShowMedian, bool min30ShowLabel, Brush min60Color, DashStyleHelper min60Dash, int min60Width, bool min60ShowMedian, bool min60ShowLabel, Brush min240Color, DashStyleHelper min240Dash, int min240Width, bool min240ShowMedian, bool min240ShowLabel, Brush minDailyColor, DashStyleHelper minDailyDash, int minDailyWidth, bool minDailyShowMedian, bool minDailyShowLabel, Brush currentDailyColor, DashStyleHelper currentDailyDash, int currentDailyWidth, bool currentDailyShowMedian, bool currentDailyShowLabel, Brush minWeeklyColor, DashStyleHelper minWeeklyDash, int minWeeklyWidth, bool minWeeklyShowMedian, bool minWeeklyShowLabel, Brush currentWeeklyColor, DashStyleHelper currentWeeklyDash, int currentWeeklyWidth, bool currentWeeklyShowMedian, bool currentWeeklyShowLabel, DayOfWeek startWeekFromDay, bool showSocials, string youtube, string discord, string tradeSaber, string author, string version)
		{
			return MultiSeriesHL(Input, min1, min5, min15, min30, min60, min240, minDaily, currentDaily, minWeekly, currentWeekly, min1Color, min1Dash, min1Width, min1ShowMedian, min1ShowLabel, min5Color, min5Dash, min5Width, min5ShowMedian, min5ShowLabel, min15Color, min15Dash, min15Width, min15ShowMedian, min15ShowLabel, min30Color, min30Dash, min30Width, min30ShowMedian, min30ShowLabel, min60Color, min60Dash, min60Width, min60ShowMedian, min60ShowLabel, min240Color, min240Dash, min240Width, min240ShowMedian, min240ShowLabel, minDailyColor, minDailyDash, minDailyWidth, minDailyShowMedian, minDailyShowLabel, currentDailyColor, currentDailyDash, currentDailyWidth, currentDailyShowMedian, currentDailyShowLabel, minWeeklyColor, minWeeklyDash, minWeeklyWidth, minWeeklyShowMedian, minWeeklyShowLabel, currentWeeklyColor, currentWeeklyDash, currentWeeklyWidth, currentWeeklyShowMedian, currentWeeklyShowLabel, startWeekFromDay, showSocials, youtube, discord, tradeSaber, author, version);
		}

		public TradeSaber.MultiSeriesHL MultiSeriesHL(ISeries<double> input, bool min1, bool min5, bool min15, bool min30, bool min60, bool min240, bool minDaily, bool currentDaily, bool minWeekly, bool currentWeekly, Brush min1Color, DashStyleHelper min1Dash, int min1Width, bool min1ShowMedian, bool min1ShowLabel, Brush min5Color, DashStyleHelper min5Dash, int min5Width, bool min5ShowMedian, bool min5ShowLabel, Brush min15Color, DashStyleHelper min15Dash, int min15Width, bool min15ShowMedian, bool min15ShowLabel, Brush min30Color, DashStyleHelper min30Dash, int min30Width, bool min30ShowMedian, bool min30ShowLabel, Brush min60Color, DashStyleHelper min60Dash, int min60Width, bool min60ShowMedian, bool min60ShowLabel, Brush min240Color, DashStyleHelper min240Dash, int min240Width, bool min240ShowMedian, bool min240ShowLabel, Brush minDailyColor, DashStyleHelper minDailyDash, int minDailyWidth, bool minDailyShowMedian, bool minDailyShowLabel, Brush currentDailyColor, DashStyleHelper currentDailyDash, int currentDailyWidth, bool currentDailyShowMedian, bool currentDailyShowLabel, Brush minWeeklyColor, DashStyleHelper minWeeklyDash, int minWeeklyWidth, bool minWeeklyShowMedian, bool minWeeklyShowLabel, Brush currentWeeklyColor, DashStyleHelper currentWeeklyDash, int currentWeeklyWidth, bool currentWeeklyShowMedian, bool currentWeeklyShowLabel, DayOfWeek startWeekFromDay, bool showSocials, string youtube, string discord, string tradeSaber, string author, string version)
		{
			if (cacheMultiSeriesHL != null)
				for (int idx = 0; idx < cacheMultiSeriesHL.Length; idx++)
					if (cacheMultiSeriesHL[idx] != null && cacheMultiSeriesHL[idx].min1 == min1 && cacheMultiSeriesHL[idx].min5 == min5 && cacheMultiSeriesHL[idx].min15 == min15 && cacheMultiSeriesHL[idx].min30 == min30 && cacheMultiSeriesHL[idx].min60 == min60 && cacheMultiSeriesHL[idx].min240 == min240 && cacheMultiSeriesHL[idx].minDaily == minDaily && cacheMultiSeriesHL[idx].currentDaily == currentDaily && cacheMultiSeriesHL[idx].minWeekly == minWeekly && cacheMultiSeriesHL[idx].currentWeekly == currentWeekly && cacheMultiSeriesHL[idx].min1Color == min1Color && cacheMultiSeriesHL[idx].min1Dash == min1Dash && cacheMultiSeriesHL[idx].min1Width == min1Width && cacheMultiSeriesHL[idx].min1ShowMedian == min1ShowMedian && cacheMultiSeriesHL[idx].min1ShowLabel == min1ShowLabel && cacheMultiSeriesHL[idx].min5Color == min5Color && cacheMultiSeriesHL[idx].min5Dash == min5Dash && cacheMultiSeriesHL[idx].min5Width == min5Width && cacheMultiSeriesHL[idx].min5ShowMedian == min5ShowMedian && cacheMultiSeriesHL[idx].min5ShowLabel == min5ShowLabel && cacheMultiSeriesHL[idx].min15Color == min15Color && cacheMultiSeriesHL[idx].min15Dash == min15Dash && cacheMultiSeriesHL[idx].min15Width == min15Width && cacheMultiSeriesHL[idx].min15ShowMedian == min15ShowMedian && cacheMultiSeriesHL[idx].min15ShowLabel == min15ShowLabel && cacheMultiSeriesHL[idx].min30Color == min30Color && cacheMultiSeriesHL[idx].min30Dash == min30Dash && cacheMultiSeriesHL[idx].min30Width == min30Width && cacheMultiSeriesHL[idx].min30ShowMedian == min30ShowMedian && cacheMultiSeriesHL[idx].min30ShowLabel == min30ShowLabel && cacheMultiSeriesHL[idx].min60Color == min60Color && cacheMultiSeriesHL[idx].min60Dash == min60Dash && cacheMultiSeriesHL[idx].min60Width == min60Width && cacheMultiSeriesHL[idx].min60ShowMedian == min60ShowMedian && cacheMultiSeriesHL[idx].min60ShowLabel == min60ShowLabel && cacheMultiSeriesHL[idx].min240Color == min240Color && cacheMultiSeriesHL[idx].min240Dash == min240Dash && cacheMultiSeriesHL[idx].min240Width == min240Width && cacheMultiSeriesHL[idx].min240ShowMedian == min240ShowMedian && cacheMultiSeriesHL[idx].min240ShowLabel == min240ShowLabel && cacheMultiSeriesHL[idx].minDailyColor == minDailyColor && cacheMultiSeriesHL[idx].minDailyDash == minDailyDash && cacheMultiSeriesHL[idx].minDailyWidth == minDailyWidth && cacheMultiSeriesHL[idx].minDailyShowMedian == minDailyShowMedian && cacheMultiSeriesHL[idx].minDailyShowLabel == minDailyShowLabel && cacheMultiSeriesHL[idx].currentDailyColor == currentDailyColor && cacheMultiSeriesHL[idx].currentDailyDash == currentDailyDash && cacheMultiSeriesHL[idx].currentDailyWidth == currentDailyWidth && cacheMultiSeriesHL[idx].currentDailyShowMedian == currentDailyShowMedian && cacheMultiSeriesHL[idx].currentDailyShowLabel == currentDailyShowLabel && cacheMultiSeriesHL[idx].minWeeklyColor == minWeeklyColor && cacheMultiSeriesHL[idx].minWeeklyDash == minWeeklyDash && cacheMultiSeriesHL[idx].minWeeklyWidth == minWeeklyWidth && cacheMultiSeriesHL[idx].minWeeklyShowMedian == minWeeklyShowMedian && cacheMultiSeriesHL[idx].minWeeklyShowLabel == minWeeklyShowLabel && cacheMultiSeriesHL[idx].currentWeeklyColor == currentWeeklyColor && cacheMultiSeriesHL[idx].currentWeeklyDash == currentWeeklyDash && cacheMultiSeriesHL[idx].currentWeeklyWidth == currentWeeklyWidth && cacheMultiSeriesHL[idx].currentWeeklyShowMedian == currentWeeklyShowMedian && cacheMultiSeriesHL[idx].currentWeeklyShowLabel == currentWeeklyShowLabel && cacheMultiSeriesHL[idx].StartWeekFromDay == startWeekFromDay && cacheMultiSeriesHL[idx].ShowSocials == showSocials && cacheMultiSeriesHL[idx].Youtube == youtube && cacheMultiSeriesHL[idx].Discord == discord && cacheMultiSeriesHL[idx].TradeSaber == tradeSaber && cacheMultiSeriesHL[idx].Author == author && cacheMultiSeriesHL[idx].Version == version && cacheMultiSeriesHL[idx].EqualsInput(input))
						return cacheMultiSeriesHL[idx];
			return CacheIndicator<TradeSaber.MultiSeriesHL>(new TradeSaber.MultiSeriesHL(){ min1 = min1, min5 = min5, min15 = min15, min30 = min30, min60 = min60, min240 = min240, minDaily = minDaily, currentDaily = currentDaily, minWeekly = minWeekly, currentWeekly = currentWeekly, min1Color = min1Color, min1Dash = min1Dash, min1Width = min1Width, min1ShowMedian = min1ShowMedian, min1ShowLabel = min1ShowLabel, min5Color = min5Color, min5Dash = min5Dash, min5Width = min5Width, min5ShowMedian = min5ShowMedian, min5ShowLabel = min5ShowLabel, min15Color = min15Color, min15Dash = min15Dash, min15Width = min15Width, min15ShowMedian = min15ShowMedian, min15ShowLabel = min15ShowLabel, min30Color = min30Color, min30Dash = min30Dash, min30Width = min30Width, min30ShowMedian = min30ShowMedian, min30ShowLabel = min30ShowLabel, min60Color = min60Color, min60Dash = min60Dash, min60Width = min60Width, min60ShowMedian = min60ShowMedian, min60ShowLabel = min60ShowLabel, min240Color = min240Color, min240Dash = min240Dash, min240Width = min240Width, min240ShowMedian = min240ShowMedian, min240ShowLabel = min240ShowLabel, minDailyColor = minDailyColor, minDailyDash = minDailyDash, minDailyWidth = minDailyWidth, minDailyShowMedian = minDailyShowMedian, minDailyShowLabel = minDailyShowLabel, currentDailyColor = currentDailyColor, currentDailyDash = currentDailyDash, currentDailyWidth = currentDailyWidth, currentDailyShowMedian = currentDailyShowMedian, currentDailyShowLabel = currentDailyShowLabel, minWeeklyColor = minWeeklyColor, minWeeklyDash = minWeeklyDash, minWeeklyWidth = minWeeklyWidth, minWeeklyShowMedian = minWeeklyShowMedian, minWeeklyShowLabel = minWeeklyShowLabel, currentWeeklyColor = currentWeeklyColor, currentWeeklyDash = currentWeeklyDash, currentWeeklyWidth = currentWeeklyWidth, currentWeeklyShowMedian = currentWeeklyShowMedian, currentWeeklyShowLabel = currentWeeklyShowLabel, StartWeekFromDay = startWeekFromDay, ShowSocials = showSocials, Youtube = youtube, Discord = discord, TradeSaber = tradeSaber, Author = author, Version = version }, input, ref cacheMultiSeriesHL);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.TradeSaber.MultiSeriesHL MultiSeriesHL(bool min1, bool min5, bool min15, bool min30, bool min60, bool min240, bool minDaily, bool currentDaily, bool minWeekly, bool currentWeekly, Brush min1Color, DashStyleHelper min1Dash, int min1Width, bool min1ShowMedian, bool min1ShowLabel, Brush min5Color, DashStyleHelper min5Dash, int min5Width, bool min5ShowMedian, bool min5ShowLabel, Brush min15Color, DashStyleHelper min15Dash, int min15Width, bool min15ShowMedian, bool min15ShowLabel, Brush min30Color, DashStyleHelper min30Dash, int min30Width, bool min30ShowMedian, bool min30ShowLabel, Brush min60Color, DashStyleHelper min60Dash, int min60Width, bool min60ShowMedian, bool min60ShowLabel, Brush min240Color, DashStyleHelper min240Dash, int min240Width, bool min240ShowMedian, bool min240ShowLabel, Brush minDailyColor, DashStyleHelper minDailyDash, int minDailyWidth, bool minDailyShowMedian, bool minDailyShowLabel, Brush currentDailyColor, DashStyleHelper currentDailyDash, int currentDailyWidth, bool currentDailyShowMedian, bool currentDailyShowLabel, Brush minWeeklyColor, DashStyleHelper minWeeklyDash, int minWeeklyWidth, bool minWeeklyShowMedian, bool minWeeklyShowLabel, Brush currentWeeklyColor, DashStyleHelper currentWeeklyDash, int currentWeeklyWidth, bool currentWeeklyShowMedian, bool currentWeeklyShowLabel, DayOfWeek startWeekFromDay, bool showSocials, string youtube, string discord, string tradeSaber, string author, string version)
		{
			return indicator.MultiSeriesHL(Input, min1, min5, min15, min30, min60, min240, minDaily, currentDaily, minWeekly, currentWeekly, min1Color, min1Dash, min1Width, min1ShowMedian, min1ShowLabel, min5Color, min5Dash, min5Width, min5ShowMedian, min5ShowLabel, min15Color, min15Dash, min15Width, min15ShowMedian, min15ShowLabel, min30Color, min30Dash, min30Width, min30ShowMedian, min30ShowLabel, min60Color, min60Dash, min60Width, min60ShowMedian, min60ShowLabel, min240Color, min240Dash, min240Width, min240ShowMedian, min240ShowLabel, minDailyColor, minDailyDash, minDailyWidth, minDailyShowMedian, minDailyShowLabel, currentDailyColor, currentDailyDash, currentDailyWidth, currentDailyShowMedian, currentDailyShowLabel, minWeeklyColor, minWeeklyDash, minWeeklyWidth, minWeeklyShowMedian, minWeeklyShowLabel, currentWeeklyColor, currentWeeklyDash, currentWeeklyWidth, currentWeeklyShowMedian, currentWeeklyShowLabel, startWeekFromDay, showSocials, youtube, discord, tradeSaber, author, version);
		}

		public Indicators.TradeSaber.MultiSeriesHL MultiSeriesHL(ISeries<double> input , bool min1, bool min5, bool min15, bool min30, bool min60, bool min240, bool minDaily, bool currentDaily, bool minWeekly, bool currentWeekly, Brush min1Color, DashStyleHelper min1Dash, int min1Width, bool min1ShowMedian, bool min1ShowLabel, Brush min5Color, DashStyleHelper min5Dash, int min5Width, bool min5ShowMedian, bool min5ShowLabel, Brush min15Color, DashStyleHelper min15Dash, int min15Width, bool min15ShowMedian, bool min15ShowLabel, Brush min30Color, DashStyleHelper min30Dash, int min30Width, bool min30ShowMedian, bool min30ShowLabel, Brush min60Color, DashStyleHelper min60Dash, int min60Width, bool min60ShowMedian, bool min60ShowLabel, Brush min240Color, DashStyleHelper min240Dash, int min240Width, bool min240ShowMedian, bool min240ShowLabel, Brush minDailyColor, DashStyleHelper minDailyDash, int minDailyWidth, bool minDailyShowMedian, bool minDailyShowLabel, Brush currentDailyColor, DashStyleHelper currentDailyDash, int currentDailyWidth, bool currentDailyShowMedian, bool currentDailyShowLabel, Brush minWeeklyColor, DashStyleHelper minWeeklyDash, int minWeeklyWidth, bool minWeeklyShowMedian, bool minWeeklyShowLabel, Brush currentWeeklyColor, DashStyleHelper currentWeeklyDash, int currentWeeklyWidth, bool currentWeeklyShowMedian, bool currentWeeklyShowLabel, DayOfWeek startWeekFromDay, bool showSocials, string youtube, string discord, string tradeSaber, string author, string version)
		{
			return indicator.MultiSeriesHL(input, min1, min5, min15, min30, min60, min240, minDaily, currentDaily, minWeekly, currentWeekly, min1Color, min1Dash, min1Width, min1ShowMedian, min1ShowLabel, min5Color, min5Dash, min5Width, min5ShowMedian, min5ShowLabel, min15Color, min15Dash, min15Width, min15ShowMedian, min15ShowLabel, min30Color, min30Dash, min30Width, min30ShowMedian, min30ShowLabel, min60Color, min60Dash, min60Width, min60ShowMedian, min60ShowLabel, min240Color, min240Dash, min240Width, min240ShowMedian, min240ShowLabel, minDailyColor, minDailyDash, minDailyWidth, minDailyShowMedian, minDailyShowLabel, currentDailyColor, currentDailyDash, currentDailyWidth, currentDailyShowMedian, currentDailyShowLabel, minWeeklyColor, minWeeklyDash, minWeeklyWidth, minWeeklyShowMedian, minWeeklyShowLabel, currentWeeklyColor, currentWeeklyDash, currentWeeklyWidth, currentWeeklyShowMedian, currentWeeklyShowLabel, startWeekFromDay, showSocials, youtube, discord, tradeSaber, author, version);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.TradeSaber.MultiSeriesHL MultiSeriesHL(bool min1, bool min5, bool min15, bool min30, bool min60, bool min240, bool minDaily, bool currentDaily, bool minWeekly, bool currentWeekly, Brush min1Color, DashStyleHelper min1Dash, int min1Width, bool min1ShowMedian, bool min1ShowLabel, Brush min5Color, DashStyleHelper min5Dash, int min5Width, bool min5ShowMedian, bool min5ShowLabel, Brush min15Color, DashStyleHelper min15Dash, int min15Width, bool min15ShowMedian, bool min15ShowLabel, Brush min30Color, DashStyleHelper min30Dash, int min30Width, bool min30ShowMedian, bool min30ShowLabel, Brush min60Color, DashStyleHelper min60Dash, int min60Width, bool min60ShowMedian, bool min60ShowLabel, Brush min240Color, DashStyleHelper min240Dash, int min240Width, bool min240ShowMedian, bool min240ShowLabel, Brush minDailyColor, DashStyleHelper minDailyDash, int minDailyWidth, bool minDailyShowMedian, bool minDailyShowLabel, Brush currentDailyColor, DashStyleHelper currentDailyDash, int currentDailyWidth, bool currentDailyShowMedian, bool currentDailyShowLabel, Brush minWeeklyColor, DashStyleHelper minWeeklyDash, int minWeeklyWidth, bool minWeeklyShowMedian, bool minWeeklyShowLabel, Brush currentWeeklyColor, DashStyleHelper currentWeeklyDash, int currentWeeklyWidth, bool currentWeeklyShowMedian, bool currentWeeklyShowLabel, DayOfWeek startWeekFromDay, bool showSocials, string youtube, string discord, string tradeSaber, string author, string version)
		{
			return indicator.MultiSeriesHL(Input, min1, min5, min15, min30, min60, min240, minDaily, currentDaily, minWeekly, currentWeekly, min1Color, min1Dash, min1Width, min1ShowMedian, min1ShowLabel, min5Color, min5Dash, min5Width, min5ShowMedian, min5ShowLabel, min15Color, min15Dash, min15Width, min15ShowMedian, min15ShowLabel, min30Color, min30Dash, min30Width, min30ShowMedian, min30ShowLabel, min60Color, min60Dash, min60Width, min60ShowMedian, min60ShowLabel, min240Color, min240Dash, min240Width, min240ShowMedian, min240ShowLabel, minDailyColor, minDailyDash, minDailyWidth, minDailyShowMedian, minDailyShowLabel, currentDailyColor, currentDailyDash, currentDailyWidth, currentDailyShowMedian, currentDailyShowLabel, minWeeklyColor, minWeeklyDash, minWeeklyWidth, minWeeklyShowMedian, minWeeklyShowLabel, currentWeeklyColor, currentWeeklyDash, currentWeeklyWidth, currentWeeklyShowMedian, currentWeeklyShowLabel, startWeekFromDay, showSocials, youtube, discord, tradeSaber, author, version);
		}

		public Indicators.TradeSaber.MultiSeriesHL MultiSeriesHL(ISeries<double> input , bool min1, bool min5, bool min15, bool min30, bool min60, bool min240, bool minDaily, bool currentDaily, bool minWeekly, bool currentWeekly, Brush min1Color, DashStyleHelper min1Dash, int min1Width, bool min1ShowMedian, bool min1ShowLabel, Brush min5Color, DashStyleHelper min5Dash, int min5Width, bool min5ShowMedian, bool min5ShowLabel, Brush min15Color, DashStyleHelper min15Dash, int min15Width, bool min15ShowMedian, bool min15ShowLabel, Brush min30Color, DashStyleHelper min30Dash, int min30Width, bool min30ShowMedian, bool min30ShowLabel, Brush min60Color, DashStyleHelper min60Dash, int min60Width, bool min60ShowMedian, bool min60ShowLabel, Brush min240Color, DashStyleHelper min240Dash, int min240Width, bool min240ShowMedian, bool min240ShowLabel, Brush minDailyColor, DashStyleHelper minDailyDash, int minDailyWidth, bool minDailyShowMedian, bool minDailyShowLabel, Brush currentDailyColor, DashStyleHelper currentDailyDash, int currentDailyWidth, bool currentDailyShowMedian, bool currentDailyShowLabel, Brush minWeeklyColor, DashStyleHelper minWeeklyDash, int minWeeklyWidth, bool minWeeklyShowMedian, bool minWeeklyShowLabel, Brush currentWeeklyColor, DashStyleHelper currentWeeklyDash, int currentWeeklyWidth, bool currentWeeklyShowMedian, bool currentWeeklyShowLabel, DayOfWeek startWeekFromDay, bool showSocials, string youtube, string discord, string tradeSaber, string author, string version)
		{
			return indicator.MultiSeriesHL(input, min1, min5, min15, min30, min60, min240, minDaily, currentDaily, minWeekly, currentWeekly, min1Color, min1Dash, min1Width, min1ShowMedian, min1ShowLabel, min5Color, min5Dash, min5Width, min5ShowMedian, min5ShowLabel, min15Color, min15Dash, min15Width, min15ShowMedian, min15ShowLabel, min30Color, min30Dash, min30Width, min30ShowMedian, min30ShowLabel, min60Color, min60Dash, min60Width, min60ShowMedian, min60ShowLabel, min240Color, min240Dash, min240Width, min240ShowMedian, min240ShowLabel, minDailyColor, minDailyDash, minDailyWidth, minDailyShowMedian, minDailyShowLabel, currentDailyColor, currentDailyDash, currentDailyWidth, currentDailyShowMedian, currentDailyShowLabel, minWeeklyColor, minWeeklyDash, minWeeklyWidth, minWeeklyShowMedian, minWeeklyShowLabel, currentWeeklyColor, currentWeeklyDash, currentWeeklyWidth, currentWeeklyShowMedian, currentWeeklyShowLabel, startWeekFromDay, showSocials, youtube, discord, tradeSaber, author, version);
		}
	}
}

#endregion
