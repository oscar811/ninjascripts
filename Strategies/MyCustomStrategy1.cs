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
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Strategies in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies
{
	public class MyCustomStrategy1 : Strategy
	{
		private NinjaTrader.NinjaScript.Indicators.LuxAlgo2.SuperTrendAIClustering2 SuperTrendAIClustering21;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Strategy here.";
				Name										= "MyCustomStrategy1";
				Calculate									= Calculate.OnBarClose;
				EntriesPerDirection							= 1;
				EntryHandling								= EntryHandling.AllEntries;
				IsExitOnSessionCloseStrategy				= true;
				ExitOnSessionCloseSeconds					= 30;
				IsFillLimitOnTouch							= false;
				MaximumBarsLookBack							= MaximumBarsLookBack.TwoHundredFiftySix;
				OrderFillResolution							= OrderFillResolution.Standard;
				Slippage									= 0;
				StartBehavior								= StartBehavior.WaitUntilFlat;
				TimeInForce									= TimeInForce.Gtc;
				TraceOrders									= false;
				RealtimeErrorHandling						= RealtimeErrorHandling.StopCancelClose;
				StopTargetHandling							= StopTargetHandling.PerEntryExecution;
				BarsRequiredToTrade							= 20;
				// Disable this property for performance gains in Strategy Analyzer optimizations
				// See the Help Guide for additional information
				IsInstantiatedOnEachOptimizationIteration	= true;
			}
			else if (State == State.Configure)
			{
			}
			else if (State == State.DataLoaded)
			{				
				SuperTrendAIClustering21				= SuperTrendAIClustering2(Close, 10, 1, 5, 0.5, 10, LuxSTAIFromCluster.Best, 1000, 10000, Brushes.Crimson, Brushes.Teal, true, false, LuxTablePosition.TopRight, 12);
				SuperTrendAIClustering21.Plots[0].Brush = Brushes.DodgerBlue;
				SuperTrendAIClustering21.Plots[1].Brush = Brushes.DodgerBlue;
				AddChartIndicator(SuperTrendAIClustering21);
				SetParabolicStop(@"1", CalculationMode.Currency, 0, false, 1, 0, 0);
			}
		}

		protected override void OnBarUpdate()
		{
			if (BarsInProgress != 0) 
				return;

			if (CurrentBars[0] < 1)
				return;

			 // Set 1
			if (Close[0] >= SuperTrendAIClustering21[0])
			{
				EnterLong(Convert.ToInt32(DefaultQuantity), "");
			}
			
		}
	}
}

#region Wizard settings, neither change nor remove
/*@
<?xml version="1.0"?>
<ScriptProperties xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
  <Calculate>OnBarClose</Calculate>
  <ConditionalActions>
    <ConditionalAction>
      <Actions>
        <WizardAction>
          <IsExpanded>false</IsExpanded>
          <IsSelected>true</IsSelected>
          <Name>Enter long position</Name>
          <OffsetType>Arithmetic</OffsetType>
          <ActionProperties>
            <DashStyle>Solid</DashStyle>
            <DivideTimePrice>false</DivideTimePrice>
            <Id />
            <File />
            <IsAutoScale>false</IsAutoScale>
            <IsSimulatedStop>false</IsSimulatedStop>
            <IsStop>false</IsStop>
            <LogLevel>Information</LogLevel>
            <Mode>Currency</Mode>
            <OffsetType>Currency</OffsetType>
            <Priority>Medium</Priority>
            <Quantity>
              <DefaultValue>0</DefaultValue>
              <IsInt>true</IsInt>
              <BindingValue xsi:type="xsd:string">DefaultQuantity</BindingValue>
              <DynamicValue>
                <IsExpanded>false</IsExpanded>
                <IsSelected>false</IsSelected>
                <Name>Default order quantity</Name>
                <OffsetType>Arithmetic</OffsetType>
                <AssignedCommand>
                  <Command>DefaultQuantity</Command>
                  <Parameters />
                </AssignedCommand>
                <BarsAgo>0</BarsAgo>
                <CurrencyType>Currency</CurrencyType>
                <Date>2024-02-22T22:12:47.275987</Date>
                <DayOfWeek>Sunday</DayOfWeek>
                <EndBar>0</EndBar>
                <ForceSeriesIndex>false</ForceSeriesIndex>
                <LookBackPeriod>0</LookBackPeriod>
                <MarketPosition>Long</MarketPosition>
                <Period>0</Period>
                <ReturnType>Number</ReturnType>
                <StartBar>0</StartBar>
                <State>Undefined</State>
                <Time>0001-01-01T00:00:00</Time>
              </DynamicValue>
              <IsLiteral>false</IsLiteral>
              <LiveValue xsi:type="xsd:string">DefaultQuantity</LiveValue>
            </Quantity>
            <ServiceName />
            <ScreenshotPath />
            <SoundLocation />
            <Tag>
              <SeparatorCharacter> </SeparatorCharacter>
              <Strings>
                <NinjaScriptString>
                  <Index>0</Index>
                  <StringValue>Set Enter long position</StringValue>
                </NinjaScriptString>
              </Strings>
            </Tag>
            <TextPosition>BottomLeft</TextPosition>
            <VariableDateTime>2024-02-22T22:12:47.275987</VariableDateTime>
            <VariableBool>false</VariableBool>
          </ActionProperties>
          <ActionType>Enter</ActionType>
          <Command>
            <Command>EnterLong</Command>
            <Parameters>
              <string>quantity</string>
              <string>signalName</string>
            </Parameters>
          </Command>
        </WizardAction>
      </Actions>
      <AnyOrAll>All</AnyOrAll>
      <Conditions>
        <WizardConditionGroup>
          <AnyOrAll>Any</AnyOrAll>
          <Conditions>
            <WizardCondition>
              <LeftItem xsi:type="WizardConditionItem">
                <IsExpanded>false</IsExpanded>
                <IsSelected>true</IsSelected>
                <Name>Close</Name>
                <OffsetType>Arithmetic</OffsetType>
                <AssignedCommand>
                  <Command>{0}</Command>
                  <Parameters>
                    <string>Series1</string>
                    <string>BarsAgo</string>
                    <string>OffsetBuilder</string>
                  </Parameters>
                </AssignedCommand>
                <BarsAgo>0</BarsAgo>
                <CurrencyType>Currency</CurrencyType>
                <Date>2024-02-22T22:07:53.8477542</Date>
                <DayOfWeek>Sunday</DayOfWeek>
                <EndBar>0</EndBar>
                <ForceSeriesIndex>false</ForceSeriesIndex>
                <LookBackPeriod>0</LookBackPeriod>
                <MarketPosition>Long</MarketPosition>
                <Period>0</Period>
                <ReturnType>Series</ReturnType>
                <StartBar>0</StartBar>
                <State>Undefined</State>
                <Time>0001-01-01T00:00:00</Time>
              </LeftItem>
              <Lookback>1</Lookback>
              <Operator>GreaterEqual</Operator>
              <RightItem xsi:type="WizardConditionItem">
                <IsExpanded>false</IsExpanded>
                <IsSelected>true</IsSelected>
                <Name>SuperTrendAI2</Name>
                <OffsetType>Arithmetic</OffsetType>
                <AssignedCommand>
                  <Command>SuperTrendAIClustering2</Command>
                  <Parameters>
                    <string>AssociatedIndicator</string>
                    <string>BarsAgo</string>
                    <string>OffsetBuilder</string>
                  </Parameters>
                </AssignedCommand>
                <AssociatedIndicator>
                  <AcceptableSeries>Indicator DataSeries CustomSeries DefaultSeries</AcceptableSeries>
                  <CustomProperties>
                    <item>
                      <key>
                        <string>length</string>
                      </key>
                      <value>
                        <anyType xsi:type="NumberBuilder">
                          <LiveValue xsi:type="xsd:string">10</LiveValue>
                          <BindingValue xsi:type="xsd:string">10</BindingValue>
                          <DefaultValue>0</DefaultValue>
                          <IsInt>true</IsInt>
                          <IsLiteral>true</IsLiteral>
                        </anyType>
                      </value>
                    </item>
                    <item>
                      <key>
                        <string>minMult</string>
                      </key>
                      <value>
                        <anyType xsi:type="NumberBuilder">
                          <LiveValue xsi:type="xsd:string">1</LiveValue>
                          <BindingValue xsi:type="xsd:string">1</BindingValue>
                          <DefaultValue>0</DefaultValue>
                          <IsInt>true</IsInt>
                          <IsLiteral>true</IsLiteral>
                        </anyType>
                      </value>
                    </item>
                    <item>
                      <key>
                        <string>maxMult</string>
                      </key>
                      <value>
                        <anyType xsi:type="NumberBuilder">
                          <LiveValue xsi:type="xsd:string">5</LiveValue>
                          <BindingValue xsi:type="xsd:string">5</BindingValue>
                          <DefaultValue>0</DefaultValue>
                          <IsInt>true</IsInt>
                          <IsLiteral>true</IsLiteral>
                        </anyType>
                      </value>
                    </item>
                    <item>
                      <key>
                        <string>step</string>
                      </key>
                      <value>
                        <anyType xsi:type="NumberBuilder">
                          <LiveValue xsi:type="xsd:string">0.5</LiveValue>
                          <BindingValue xsi:type="xsd:string">0.5</BindingValue>
                          <DefaultValue>0</DefaultValue>
                          <IsInt>false</IsInt>
                          <IsLiteral>true</IsLiteral>
                        </anyType>
                      </value>
                    </item>
                    <item>
                      <key>
                        <string>perfAlpha</string>
                      </key>
                      <value>
                        <anyType xsi:type="NumberBuilder">
                          <LiveValue xsi:type="xsd:string">10</LiveValue>
                          <BindingValue xsi:type="xsd:string">10</BindingValue>
                          <DefaultValue>0</DefaultValue>
                          <IsInt>false</IsInt>
                          <IsLiteral>true</IsLiteral>
                        </anyType>
                      </value>
                    </item>
                    <item>
                      <key>
                        <string>fromCluster</string>
                      </key>
                      <value>
                        <anyType xsi:type="xsd:string">Best</anyType>
                      </value>
                    </item>
                    <item>
                      <key>
                        <string>maxIter</string>
                      </key>
                      <value>
                        <anyType xsi:type="NumberBuilder">
                          <LiveValue xsi:type="xsd:string">1000</LiveValue>
                          <BindingValue xsi:type="xsd:string">1000</BindingValue>
                          <DefaultValue>0</DefaultValue>
                          <IsInt>true</IsInt>
                          <IsLiteral>true</IsLiteral>
                        </anyType>
                      </value>
                    </item>
                    <item>
                      <key>
                        <string>maxData</string>
                      </key>
                      <value>
                        <anyType xsi:type="NumberBuilder">
                          <LiveValue xsi:type="xsd:string">10000</LiveValue>
                          <BindingValue xsi:type="xsd:string">10000</BindingValue>
                          <DefaultValue>0</DefaultValue>
                          <IsInt>true</IsInt>
                          <IsLiteral>true</IsLiteral>
                        </anyType>
                      </value>
                    </item>
                    <item>
                      <key>
                        <string>bearCss</string>
                      </key>
                      <value>
                        <anyType xsi:type="SolidColorBrush">
                          <Opacity>1</Opacity>
                          <Transform xsi:type="MatrixTransform">
                            <Matrix>
                              <M11>1</M11>
                              <M12>0</M12>
                              <M21>0</M21>
                              <M22>1</M22>
                              <OffsetX>0</OffsetX>
                              <OffsetY>0</OffsetY>
                            </Matrix>
                          </Transform>
                          <RelativeTransform xsi:type="MatrixTransform">
                            <Matrix>
                              <M11>1</M11>
                              <M12>0</M12>
                              <M21>0</M21>
                              <M22>1</M22>
                              <OffsetX>0</OffsetX>
                              <OffsetY>0</OffsetY>
                            </Matrix>
                          </RelativeTransform>
                          <Color>
                            <A>255</A>
                            <R>220</R>
                            <G>20</G>
                            <B>60</B>
                            <ScA>1</ScA>
                            <ScR>0.715693533</ScR>
                            <ScG>0.00699541066</ScG>
                            <ScB>0.045186203</ScB>
                          </Color>
                        </anyType>
                      </value>
                    </item>
                    <item>
                      <key>
                        <string>bullCss</string>
                      </key>
                      <value>
                        <anyType xsi:type="SolidColorBrush">
                          <Opacity>1</Opacity>
                          <Transform xsi:type="MatrixTransform">
                            <Matrix>
                              <M11>1</M11>
                              <M12>0</M12>
                              <M21>0</M21>
                              <M22>1</M22>
                              <OffsetX>0</OffsetX>
                              <OffsetY>0</OffsetY>
                            </Matrix>
                          </Transform>
                          <RelativeTransform xsi:type="MatrixTransform">
                            <Matrix>
                              <M11>1</M11>
                              <M12>0</M12>
                              <M21>0</M21>
                              <M22>1</M22>
                              <OffsetX>0</OffsetX>
                              <OffsetY>0</OffsetY>
                            </Matrix>
                          </RelativeTransform>
                          <Color>
                            <A>255</A>
                            <R>0</R>
                            <G>128</G>
                            <B>128</B>
                            <ScA>1</ScA>
                            <ScR>0</ScR>
                            <ScG>0.215860531</ScG>
                            <ScB>0.215860531</ScB>
                          </Color>
                        </anyType>
                      </value>
                    </item>
                    <item>
                      <key>
                        <string>amaBearCss</string>
                      </key>
                      <value>
                        <anyType xsi:type="SolidColorBrush">
                          <Opacity>1</Opacity>
                          <Transform xsi:type="MatrixTransform">
                            <Matrix>
                              <M11>1</M11>
                              <M12>0</M12>
                              <M21>0</M21>
                              <M22>1</M22>
                              <OffsetX>0</OffsetX>
                              <OffsetY>0</OffsetY>
                            </Matrix>
                          </Transform>
                          <RelativeTransform xsi:type="MatrixTransform">
                            <Matrix>
                              <M11>1</M11>
                              <M12>0</M12>
                              <M21>0</M21>
                              <M22>1</M22>
                              <OffsetX>0</OffsetX>
                              <OffsetY>0</OffsetY>
                            </Matrix>
                          </RelativeTransform>
                          <Color>
                            <A>255</A>
                            <R>220</R>
                            <G>20</G>
                            <B>60</B>
                            <ScA>1</ScA>
                            <ScR>0.715693533</ScR>
                            <ScG>0.00699541066</ScG>
                            <ScB>0.045186203</ScB>
                          </Color>
                        </anyType>
                      </value>
                    </item>
                    <item>
                      <key>
                        <string>amaBullCss</string>
                      </key>
                      <value>
                        <anyType xsi:type="SolidColorBrush">
                          <Opacity>1</Opacity>
                          <Transform xsi:type="MatrixTransform">
                            <Matrix>
                              <M11>1</M11>
                              <M12>0</M12>
                              <M21>0</M21>
                              <M22>1</M22>
                              <OffsetX>0</OffsetX>
                              <OffsetY>0</OffsetY>
                            </Matrix>
                          </Transform>
                          <RelativeTransform xsi:type="MatrixTransform">
                            <Matrix>
                              <M11>1</M11>
                              <M12>0</M12>
                              <M21>0</M21>
                              <M22>1</M22>
                              <OffsetX>0</OffsetX>
                              <OffsetY>0</OffsetY>
                            </Matrix>
                          </RelativeTransform>
                          <Color>
                            <A>255</A>
                            <R>0</R>
                            <G>128</G>
                            <B>128</B>
                            <ScA>1</ScA>
                            <ScR>0</ScR>
                            <ScG>0.215860531</ScG>
                            <ScB>0.215860531</ScB>
                          </Color>
                        </anyType>
                      </value>
                    </item>
                    <item>
                      <key>
                        <string>showSignals</string>
                      </key>
                      <value>
                        <anyType xsi:type="xsd:boolean">true</anyType>
                      </value>
                    </item>
                    <item>
                      <key>
                        <string>showDash</string>
                      </key>
                      <value>
                        <anyType xsi:type="xsd:boolean">false</anyType>
                      </value>
                    </item>
                    <item>
                      <key>
                        <string>dashLoc</string>
                      </key>
                      <value>
                        <anyType xsi:type="xsd:string">TopRight</anyType>
                      </value>
                    </item>
                    <item>
                      <key>
                        <string>textSize</string>
                      </key>
                      <value>
                        <anyType xsi:type="NumberBuilder">
                          <LiveValue xsi:type="xsd:string">12</LiveValue>
                          <BindingValue xsi:type="xsd:string">12</BindingValue>
                          <DefaultValue>0</DefaultValue>
                          <IsInt>true</IsInt>
                          <IsLiteral>true</IsLiteral>
                        </anyType>
                      </value>
                    </item>
                  </CustomProperties>
                  <IndicatorHolder>
                    <IndicatorName>SuperTrendAIClustering2</IndicatorName>
                    <Plots>
                      <Plot>
                        <IsOpacityVisible>false</IsOpacityVisible>
                        <BrushSerialize>&lt;SolidColorBrush xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"&gt;#FF1E90FF&lt;/SolidColorBrush&gt;</BrushSerialize>
                        <DashStyleHelper>Solid</DashStyleHelper>
                        <Opacity>100</Opacity>
                        <Width>1</Width>
                        <AutoWidth>false</AutoWidth>
                        <Max>1.7976931348623157E+308</Max>
                        <Min>-1.7976931348623157E+308</Min>
                        <Name>Trailing Stop</Name>
                        <PlotStyle>Line</PlotStyle>
                      </Plot>
                      <Plot>
                        <IsOpacityVisible>false</IsOpacityVisible>
                        <BrushSerialize>&lt;SolidColorBrush xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"&gt;#FF1E90FF&lt;/SolidColorBrush&gt;</BrushSerialize>
                        <DashStyleHelper>Solid</DashStyleHelper>
                        <Opacity>100</Opacity>
                        <Width>1</Width>
                        <AutoWidth>false</AutoWidth>
                        <Max>1.7976931348623157E+308</Max>
                        <Min>-1.7976931348623157E+308</Min>
                        <Name>Trailing Stop AMA</Name>
                        <PlotStyle>Line</PlotStyle>
                      </Plot>
                    </Plots>
                  </IndicatorHolder>
                  <IsExplicitlyNamed>false</IsExplicitlyNamed>
                  <IsPriceTypeLocked>false</IsPriceTypeLocked>
                  <PlotOnChart>true</PlotOnChart>
                  <PriceType>Close</PriceType>
                  <SeriesType>Indicator</SeriesType>
                  <SelectedPlot>ts</SelectedPlot>
                </AssociatedIndicator>
                <BarsAgo>0</BarsAgo>
                <CurrencyType>Currency</CurrencyType>
                <Date>2024-02-22T22:07:53.855733</Date>
                <DayOfWeek>Sunday</DayOfWeek>
                <EndBar>0</EndBar>
                <ForceSeriesIndex>false</ForceSeriesIndex>
                <LookBackPeriod>0</LookBackPeriod>
                <MarketPosition>Long</MarketPosition>
                <Period>0</Period>
                <ReturnType>Series</ReturnType>
                <StartBar>0</StartBar>
                <State>Undefined</State>
                <Time>0001-01-01T00:00:00</Time>
              </RightItem>
            </WizardCondition>
          </Conditions>
          <IsGroup>false</IsGroup>
          <DisplayName>Default input[0] &gt;= SuperTrendAIClustering2(10, 1, 5, 0.5, 10, LuxSTAIFromCluster.Best, 1000, 10000, Brushes.Crimson, Brushes.Teal, Brushes.Crimson, Brushes.Teal, true, false, LuxTablePosition.TopRight, 12)[0]</DisplayName>
        </WizardConditionGroup>
      </Conditions>
      <SetName>Set 1</SetName>
      <SetNumber>1</SetNumber>
    </ConditionalAction>
  </ConditionalActions>
  <CustomSeries />
  <DataSeries />
  <Description>Enter the description for your new custom Strategy here.</Description>
  <DisplayInDataBox>true</DisplayInDataBox>
  <DrawHorizontalGridLines>true</DrawHorizontalGridLines>
  <DrawOnPricePanel>true</DrawOnPricePanel>
  <DrawVerticalGridLines>true</DrawVerticalGridLines>
  <EntriesPerDirection>1</EntriesPerDirection>
  <EntryHandling>AllEntries</EntryHandling>
  <ExitOnSessionClose>true</ExitOnSessionClose>
  <ExitOnSessionCloseSeconds>30</ExitOnSessionCloseSeconds>
  <FillLimitOrdersOnTouch>false</FillLimitOrdersOnTouch>
  <InputParameters />
  <IsTradingHoursBreakLineVisible>true</IsTradingHoursBreakLineVisible>
  <IsInstantiatedOnEachOptimizationIteration>true</IsInstantiatedOnEachOptimizationIteration>
  <MaximumBarsLookBack>TwoHundredFiftySix</MaximumBarsLookBack>
  <MinimumBarsRequired>20</MinimumBarsRequired>
  <OrderFillResolution>Standard</OrderFillResolution>
  <OrderFillResolutionValue>1</OrderFillResolutionValue>
  <OrderFillResolutionType>Minute</OrderFillResolutionType>
  <OverlayOnPrice>false</OverlayOnPrice>
  <PaintPriceMarkers>true</PaintPriceMarkers>
  <PlotParameters />
  <RealTimeErrorHandling>StopCancelClose</RealTimeErrorHandling>
  <ScaleJustification>Right</ScaleJustification>
  <ScriptType>Strategy</ScriptType>
  <Slippage>0</Slippage>
  <StartBehavior>WaitUntilFlat</StartBehavior>
  <StopsAndTargets>
    <WizardAction>
      <IsExpanded>false</IsExpanded>
      <IsSelected>true</IsSelected>
      <Name>Parabolic stop</Name>
      <OffsetType>Arithmetic</OffsetType>
      <ActionProperties>
        <Acceleration>
          <DefaultValue>0</DefaultValue>
          <IsInt>false</IsInt>
          <BindingValue xsi:type="xsd:string">1</BindingValue>
          <IsLiteral>true</IsLiteral>
          <LiveValue xsi:type="xsd:string">1</LiveValue>
        </Acceleration>
        <AccelerationStep>
          <DefaultValue>0</DefaultValue>
          <IsInt>false</IsInt>
          <BindingValue xsi:type="xsd:string">0</BindingValue>
          <IsLiteral>true</IsLiteral>
          <LiveValue xsi:type="xsd:string">0</LiveValue>
        </AccelerationStep>
        <DashStyle>Solid</DashStyle>
        <DivideTimePrice>false</DivideTimePrice>
        <Id />
        <File />
        <FromEntrySignal>
          <SeparatorCharacter> </SeparatorCharacter>
          <Strings>
            <NinjaScriptString>
              <Index>0</Index>
              <StringValue>1</StringValue>
            </NinjaScriptString>
          </Strings>
        </FromEntrySignal>
        <IsAutoScale>false</IsAutoScale>
        <IsSimulatedStop>false</IsSimulatedStop>
        <IsStop>false</IsStop>
        <LogLevel>Information</LogLevel>
        <Mode>Currency</Mode>
        <OffsetType>Currency</OffsetType>
        <Priority>Medium</Priority>
        <Quantity>
          <DefaultValue>0</DefaultValue>
          <IsInt>true</IsInt>
          <BindingValue xsi:type="xsd:string">DefaultQuantity</BindingValue>
          <DynamicValue>
            <IsExpanded>false</IsExpanded>
            <IsSelected>false</IsSelected>
            <Name>Default order quantity</Name>
            <OffsetType>Arithmetic</OffsetType>
            <AssignedCommand>
              <Command>DefaultQuantity</Command>
              <Parameters />
            </AssignedCommand>
            <BarsAgo>0</BarsAgo>
            <CurrencyType>Currency</CurrencyType>
            <Date>2024-02-22T22:13:16.9010946</Date>
            <DayOfWeek>Sunday</DayOfWeek>
            <EndBar>0</EndBar>
            <ForceSeriesIndex>false</ForceSeriesIndex>
            <LookBackPeriod>0</LookBackPeriod>
            <MarketPosition>Long</MarketPosition>
            <Period>0</Period>
            <ReturnType>Number</ReturnType>
            <StartBar>0</StartBar>
            <State>Undefined</State>
            <Time>0001-01-01T00:00:00</Time>
          </DynamicValue>
          <IsLiteral>false</IsLiteral>
          <LiveValue xsi:type="xsd:string">DefaultQuantity</LiveValue>
        </Quantity>
        <ServiceName />
        <ScreenshotPath />
        <SoundLocation />
        <Tag>
          <SeparatorCharacter> </SeparatorCharacter>
          <Strings>
            <NinjaScriptString>
              <Index>0</Index>
              <StringValue>Set Parabolic stop</StringValue>
            </NinjaScriptString>
          </Strings>
        </Tag>
        <TextPosition>BottomLeft</TextPosition>
        <VariableDateTime>2024-02-22T22:13:16.9010946</VariableDateTime>
        <VariableBool>false</VariableBool>
      </ActionProperties>
      <ActionType>Misc</ActionType>
      <Command>
        <Command>SetParabolicStop</Command>
        <Parameters>
          <string>fromEntrySignal</string>
          <string>mode</string>
          <string>value</string>
          <string>isSimulatedStop</string>
          <string>acceleration</string>
          <string>accelerationMax</string>
          <string>accelerationStep</string>
        </Parameters>
      </Command>
    </WizardAction>
  </StopsAndTargets>
  <StopTargetHandling>PerEntryExecution</StopTargetHandling>
  <TimeInForce>Gtc</TimeInForce>
  <TraceOrders>false</TraceOrders>
  <UseOnAddTradeEvent>false</UseOnAddTradeEvent>
  <UseOnAuthorizeAccountEvent>false</UseOnAuthorizeAccountEvent>
  <UseAccountItemUpdate>false</UseAccountItemUpdate>
  <UseOnCalculatePerformanceValuesEvent>true</UseOnCalculatePerformanceValuesEvent>
  <UseOnConnectionEvent>false</UseOnConnectionEvent>
  <UseOnDataPointEvent>true</UseOnDataPointEvent>
  <UseOnFundamentalDataEvent>false</UseOnFundamentalDataEvent>
  <UseOnExecutionEvent>false</UseOnExecutionEvent>
  <UseOnMouseDown>true</UseOnMouseDown>
  <UseOnMouseMove>true</UseOnMouseMove>
  <UseOnMouseUp>true</UseOnMouseUp>
  <UseOnMarketDataEvent>false</UseOnMarketDataEvent>
  <UseOnMarketDepthEvent>false</UseOnMarketDepthEvent>
  <UseOnMergePerformanceMetricEvent>false</UseOnMergePerformanceMetricEvent>
  <UseOnNextDataPointEvent>true</UseOnNextDataPointEvent>
  <UseOnNextInstrumentEvent>true</UseOnNextInstrumentEvent>
  <UseOnOptimizeEvent>true</UseOnOptimizeEvent>
  <UseOnOrderUpdateEvent>false</UseOnOrderUpdateEvent>
  <UseOnPositionUpdateEvent>false</UseOnPositionUpdateEvent>
  <UseOnRenderEvent>true</UseOnRenderEvent>
  <UseOnRestoreValuesEvent>false</UseOnRestoreValuesEvent>
  <UseOnShareEvent>true</UseOnShareEvent>
  <UseOnWindowCreatedEvent>false</UseOnWindowCreatedEvent>
  <UseOnWindowDestroyedEvent>false</UseOnWindowDestroyedEvent>
  <Variables />
  <Name>MyCustomStrategy1</Name>
</ScriptProperties>
@*/
#endregion
