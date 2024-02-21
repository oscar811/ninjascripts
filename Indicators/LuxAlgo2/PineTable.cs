using NinjaTrader.Core;
using NinjaTrader.Gui.Chart;
using NinjaTrader.NinjaScript;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NinjaTrader.Custom.Indicators.LuxAlgo2
{
    public class PineTable
    {
        public enum TextHorizontal
        {
            AlignLeft,
            AlignCenter,
            AlignRight
        }

        public enum TextVertical
        {
            AlignTop,
            AlignCenter,
            AlignBottom
        }

        private struct MergeStruct
        {
            public int StartColumn;

            public int StartRow;

            public int EndColumn;

            public int EndRow;

            public bool Merged;

            public bool IsFirst;

            public int MergedRows;

            public int MergedColumns;

            public float MergedWidth;

            public float MergedHeight;
        }

        private struct RowStruct
        {
            public string Text;

            public int Width;

            public int Height;

            public Color TextColor;

            public Color BgColor;

            public TextHorizontal TextHalign;

            public TextVertical TextValign;

            public int TextSize;

            public TextFormat textFormat;

            public TextLayout textLayout;

            public MergeStruct MergeData;
        }

        private struct columnStruct
        {
            public RowStruct[] Row;

            public int ColumnWidth;

            public bool ColumnExist;
        }

        protected NinjaScriptBase owner;

        private columnStruct[] Column;

        private int[] RowHeight;

        private bool[] RowExist;

        private Color BgColor;

        private Color FrameColor;

        private Color BorderColor;

        private float TableWidth { get; set; }

        private float TableHeight { get; set; }

        private int Rows { get; set; }

        private int Columns { get; set; }

        private int ExistingRows { get; set; }

        private int ExistingColumns { get; set; }

        private float FrameWidth { get; set; }

        private float BorderWidth { get; set; }

        private LuxTablePosition TablePosition { get; set; }

        private int EmptySize { get; set; }

        public PineTable(NinjaScriptBase owner)
        {
            this.owner = owner;
            TableWidth = 0f;
            TableHeight = 0f;
            Rows = 0;
            Columns = 0;
            ExistingRows = 0;
            ExistingColumns = 0;
            FrameWidth = 0f;
            BorderWidth = 0f;
            TablePosition = LuxTablePosition.TopLeft;
        }

        public void New(LuxTablePosition TablePosition, int Columns, int Rows, Color? BgColor = null, Color? FrameColor = null, int FrameWidth = 0, Color? BorderColor = null, int BorderWidth = 0, int TextSize = 12, int EmptySize = 10)
        {
            this.TablePosition = TablePosition;
            this.Columns = Columns;
            this.Rows = Rows;
            this.FrameWidth = FrameWidth;
            this.BorderWidth = BorderWidth;
            TableWidth = 0f;
            TableHeight = 0f;
            this.BgColor = BgColor ?? Color.Transparent;
            this.FrameColor = FrameColor ?? Color.Transparent;
            this.BorderColor = BorderColor ?? Color.Transparent;
            this.EmptySize = EmptySize;
            RowHeight = new int[Rows];
            RowExist = new bool[Rows];
            for (int i = 0; i < Rows; i++)
            {
                RowHeight[i] = 0;
                RowExist[i] = false;
            }

            Column = new columnStruct[Columns];
            for (int j = 0; j < Columns; j++)
            {
                Column[j].Row = new RowStruct[Rows];
                Column[j].ColumnWidth = 0;
                Column[j].ColumnExist = false;
                for (int k = 0; k < Rows; k++)
                {
                    Column[j].Row[k].Text = "";
                    Column[j].Row[k].Width = 0;
                    Column[j].Row[k].Height = 0;
                    Column[j].Row[k].TextColor = Color.White;
                    Column[j].Row[k].BgColor = this.BgColor;
                    Column[j].Row[k].TextHalign = TextHorizontal.AlignCenter;
                    Column[j].Row[k].TextValign = TextVertical.AlignCenter;
                    Column[j].Row[k].TextSize = TextSize;
                    Column[j].Row[k].textFormat = null;
                    Column[j].Row[k].textLayout = null;
                    Column[j].Row[k].MergeData.Merged = false;
                }
            }
        }

        public void MergeCells(int StartColumn, int StartRow, int EndColumn, int EndRow)
        {
            if (StartColumn < 0 || StartColumn >= Columns || StartRow < 0 || StartRow >= Rows)
            {
                owner.Print("Error: merge: StartColumn or StartRow out of range");
                return;
            }

            if (EndColumn < 0 || EndColumn >= Columns || EndRow < 0 || EndRow >= Rows)
            {
                owner.Print("Error: merge: EndColumn or EndRow out of range");
                return;
            }

            if (StartColumn > EndColumn || StartRow > EndRow)
            {
                owner.Print("Error: merge: StartColumn or StartRow is greater than EndColumn or EndRow");
                return;
            }

            Clear(StartColumn, StartRow, EndColumn, EndRow);
            Column[StartColumn].ColumnExist = true;
            Column[StartColumn].Row[StartRow].MergeData.IsFirst = true;
            for (int i = StartRow; i <= EndRow; i++)
            {
                RowExist[i] = true;
            }

            for (int j = StartColumn; j <= EndColumn; j++)
            {
                for (int k = StartRow; k <= EndRow; k++)
                {
                    Column[j].Row[k].MergeData.StartColumn = StartColumn;
                    Column[j].Row[k].MergeData.StartRow = StartRow;
                    Column[j].Row[k].MergeData.EndColumn = EndColumn;
                    Column[j].Row[k].MergeData.EndRow = EndRow;
                    Column[j].Row[k].MergeData.Merged = true;
                    Column[j].Row[k].MergeData.MergedColumns = EndColumn - StartColumn + 1;
                    Column[j].Row[k].MergeData.MergedRows = EndRow - StartRow + 1;
                }
            }
        }

        public void SetCell(int column, int row, string Text = "", int Width = 0, int Height = 0, Color? TextColor = null, TextHorizontal TextHalign = TextHorizontal.AlignCenter, TextVertical TextValign = TextVertical.AlignCenter, int TextSize = 12, Color? BgColor = null)
        {
            if (column < 0 || column >= Columns || row < 0 || row >= Rows)
            {
                owner.Print("Error: cell: column (" + column + "/" + Columns + ") or row (" + row + "/" + Rows + ") out of range");
            }
            else
            {
                if (Column[column].Row[row].MergeData.Merged && (column != Column[column].Row[row].MergeData.StartColumn || row != Column[column].Row[row].MergeData.StartRow))
                {
                    return;
                }

                Column[column].ColumnExist = true;
                RowExist[row] = true;
                if (Column[column].Row[row].Text != Text || Column[column].Row[row].TextSize != TextSize)
                {
                    if (Column[column].Row[row].TextSize != TextSize || Column[column].Row[row].textFormat == null)
                    {
                        Column[column].Row[row].TextSize = TextSize;
                        if (Column[column].Row[row].textFormat != null)
                        {
                            Column[column].Row[row].textFormat.Dispose();
                        }

                        Column[column].Row[row].textFormat = new TextFormat(Globals.DirectWriteFactory, "Arial", TextSize);
                    }

                    Column[column].Row[row].Text = Text;
                    if (Column[column].Row[row].textLayout != null)
                    {
                        Column[column].Row[row].textLayout.Dispose();
                    }

                    Column[column].Row[row].textLayout = new TextLayout(Globals.DirectWriteFactory, Text, Column[column].Row[row].textFormat, 1000f, 1000f);
                    Column[column].Row[row].Height = (int)Column[column].Row[row].textLayout.Metrics.Height * 2;
                    Column[column].Row[row].Width = (int)Column[column].Row[row].textLayout.Metrics.Width + Column[column].Row[row].Height / 2;
                    if (Column[column].Row[row].Width == 0)
                    {
                        Column[column].Row[row].Width = EmptySize;
                    }

                    if (Column[column].Row[row].Height == 0)
                    {
                        Column[column].Row[row].Height = EmptySize;
                    }
                }

                if (Text == "" || Text == null)
                {
                    Column[column].Row[row].Width = EmptySize;
                    Column[column].Row[row].Height = EmptySize;
                }

                int num = 1;
                int num2 = 1;
                if (Column[column].Row[row].MergeData.Merged)
                {
                    num2 = Column[column].Row[row].MergeData.MergedColumns;
                    num = Column[column].Row[row].MergeData.MergedRows;
                }

                Column[column].Row[row].Width = Math.Max(Column[column].Row[row].Width, Width) / num2;
                Column[column].Row[row].Height = Math.Max(Column[column].Row[row].Height, Height) / num;
                Column[column].Row[row].TextColor = TextColor ?? Color.White;
                Column[column].Row[row].BgColor = BgColor ?? this.BgColor;
                Column[column].Row[row].TextHalign = TextHalign;
                Column[column].Row[row].TextValign = TextValign;
                if (!Column[column].Row[row].MergeData.Merged)
                {
                    return;
                }

                for (int i = Column[column].Row[row].MergeData.StartColumn; i <= Column[column].Row[row].MergeData.EndColumn; i++)
                {
                    for (int j = Column[column].Row[row].MergeData.StartRow; j <= Column[column].Row[row].MergeData.EndRow; j++)
                    {
                        Column[i].Row[j].Width = Column[column].Row[row].Width;
                        Column[i].Row[j].Height = Column[column].Row[row].Height;
                    }
                }
            }
        }

        public void Clear(int StartColumn, int StartRow, int EndColumn = -1, int EndRow = -1)
        {
            if (StartColumn < 0 || StartColumn >= Columns || StartRow < 0 || StartRow >= Rows)
            {
                owner.Print("Error: clear: StartColumn (" + StartColumn + "/" + Columns + ") or StartRow (" + StartRow + "/" + Rows + ") out of range");
                return;
            }

            if (EndColumn == -1)
            {
                EndColumn = StartColumn;
            }

            if (EndRow == -1)
            {
                EndRow = StartRow;
            }

            if (EndColumn < 0 || EndColumn >= Columns || EndRow < 0 || EndRow >= Rows)
            {
                owner.Print("Error: clear: EndColumn (" + EndColumn + "/" + Columns + ") or EndRow (" + EndRow + "/" + Rows + ") out of range");
                return;
            }

            for (int i = StartColumn; i <= EndColumn; i++)
            {
                for (int j = StartRow; j <= EndRow; j++)
                {
                    Column[i].Row[j].Text = "";
                    Column[i].Row[j].Width = 0;
                    Column[i].Row[j].Height = 0;
                    Column[i].Row[j].TextColor = Color.White;
                    Column[i].Row[j].BgColor = BgColor;
                    Column[i].Row[j].TextHalign = TextHorizontal.AlignCenter;
                    Column[i].Row[j].TextValign = TextVertical.AlignCenter;
                    Column[i].Row[j].TextSize = 0;
                    if (Column[i].Row[j].textFormat != null)
                    {
                        Column[i].Row[j].textFormat.Dispose();
                    }

                    if (Column[i].Row[j].textLayout != null)
                    {
                        Column[i].Row[j].textLayout.Dispose();
                    }

                    Column[i].Row[j].textFormat = null;
                    Column[i].Row[j].textLayout = null;
                    if (!Column[i].Row[j].MergeData.Merged)
                    {
                        continue;
                    }

                    for (int k = Column[i].Row[j].MergeData.StartColumn; k <= Column[i].Row[j].MergeData.EndColumn; k++)
                    {
                        for (int l = Column[i].Row[j].MergeData.StartRow; l <= Column[i].Row[j].MergeData.EndRow; l++)
                        {
                            Column[k].Row[l].MergeData.Merged = false;
                            Column[k].Row[l].MergeData.StartColumn = 0;
                            Column[k].Row[l].MergeData.StartRow = 0;
                            Column[k].Row[l].MergeData.EndColumn = 0;
                            Column[k].Row[l].MergeData.EndRow = 0;
                        }
                    }
                }
            }

            for (int m = 0; m < Columns; m++)
            {
                Column[m].ColumnExist = false;
                for (int n = 0; n < Rows; n++)
                {
                    if (Column[m].Row[n].Width > 0)
                    {
                        Column[m].ColumnExist = true;
                    }
                }
            }

            for (int num = 0; num < Rows; num++)
            {
                RowExist[num] = false;
                for (int num2 = 0; num2 < Columns; num2++)
                {
                    if (Column[num2].Row[num].Width > 0)
                    {
                        RowExist[num] = true;
                    }
                }
            }
        }

        public void Calculate()
        {
            TableWidth = 0f;
            TableHeight = 0f;
            for (int i = 0; i < Columns; i++)
            {
                ExistingColumns += (Column[i].ColumnExist ? 1 : 0);
                Column[i].ColumnWidth = 0;
                for (int j = 0; j < Rows; j++)
                {
                    Column[i].ColumnWidth = Math.Max(Column[i].ColumnWidth, Column[i].Row[j].Width);
                }

                TableWidth += Column[i].ColumnWidth;
            }

            for (int k = 0; k < Rows; k++)
            {
                ExistingRows += (RowExist[k] ? 1 : 0);
                RowHeight[k] = 0;
                for (int l = 0; l < Columns; l++)
                {
                    RowHeight[k] = Math.Max(RowHeight[k], Column[l].Row[k].Height);
                }

                TableHeight += RowHeight[k];
            }

            int num = 0;
            for (int m = 0; m < Columns; m++)
            {
                for (int n = 0; n < Rows; n++)
                {
                    if (!Column[m].Row[n].MergeData.IsFirst)
                    {
                        continue;
                    }

                    Column[m].Row[n].MergeData.MergedWidth = 0f;
                    for (int num2 = Column[m].Row[n].MergeData.StartColumn; num2 <= Column[m].Row[n].MergeData.EndColumn; num2++)
                    {
                        if (Column[num2].ColumnExist)
                        {
                            Column[m].Row[n].MergeData.MergedWidth += (float)Column[num2].ColumnWidth + ((num % 2 == 1) ? BorderWidth : 0f);
                            num++;
                        }
                    }

                    Column[m].Row[n].MergeData.MergedHeight = 0f;
                    for (int num3 = Column[m].Row[n].MergeData.StartRow; num3 <= Column[m].Row[n].MergeData.EndRow; num3++)
                    {
                        Column[m].Row[n].MergeData.MergedHeight += (float)RowHeight[num3] + ((num3 % 2 == 1) ? BorderWidth : 0f);
                    }
                }
            }

            TableWidth += BorderWidth / 2f * (float)ExistingColumns;
            TableHeight += BorderWidth / 2f * (float)ExistingRows;
        }

        public void Draw(RenderTarget renderTarget, ChartControl chartControl, ChartScale chartScale)
        {
            Vector2 location = GetLocation(chartControl, chartScale);
            for (int i = 0; i < Columns; i++)
            {
                if (!Column[i].ColumnExist)
                {
                    continue;
                }

                float x = location.X;
                for (int j = 0; j < Rows; j++)
                {
                    if (!RowExist[j] || (Column[i].Row[j].MergeData.Merged && (i != Column[i].Row[j].MergeData.StartColumn || j != Column[i].Row[j].MergeData.StartRow)))
                    {
                        continue;
                    }

                    float num = x + sumColumnWidths(i);
                    float num2 = location.Y + sumRowHeights(j);
                    int num3 = 1;
                    int num4 = 1;
                    float num5 = 0f;
                    float num6 = 0f;
                    _ = Column[i].Row[j].MergeData.Merged;
                    float num7 = ((!Column[i].Row[j].MergeData.Merged) ? ((float)(Column[i].ColumnWidth * num3) - num5) : Column[i].Row[j].MergeData.MergedWidth);
                    float num8 = ((!Column[i].Row[j].MergeData.Merged) ? ((float)(RowHeight[j] * num4) - num6) : Column[i].Row[j].MergeData.MergedHeight);
                    using (SolidColorBrush brush = new SolidColorBrush(renderTarget, Column[i].Row[j].BgColor))
                    {
                        RectangleF rect = new RectangleF(num, num2, num7, num8);
                        renderTarget.FillRectangle(rect, brush);
                    }

                    using (SolidColorBrush brush2 = new SolidColorBrush(renderTarget, BorderColor))
                    {
                        RectangleF rect2 = new RectangleF(num, num2, num7, num8);
                        renderTarget.DrawRectangle(rect2, brush2, BorderWidth);
                    }

                    if (Column[i].Row[j].Text != null && Column[i].Row[j].Text != "")
                    {
                        using SolidColorBrush defaultForegroundBrush = new SolidColorBrush(renderTarget, Column[i].Row[j].TextColor);
                        float num9 = ((Column[i].Row[j].TextHalign == TextHorizontal.AlignLeft) ? 1f : ((Column[i].Row[j].TextHalign == TextHorizontal.AlignCenter) ? (num7 / 2f - Column[i].Row[j].textLayout.Metrics.Width / 2f - 1f) : (num7 - Column[i].Row[j].textLayout.Metrics.Width - 1f)));
                        float num10 = ((Column[i].Row[j].TextValign == TextVertical.AlignTop) ? 1f : ((Column[i].Row[j].TextValign == TextVertical.AlignCenter) ? (num8 / 2f - Column[i].Row[j].textLayout.Metrics.Height / 2f - 1f) : (num8 - Column[i].Row[j].textLayout.Metrics.Height - 1f)));
                        float x2 = num + num9;
                        float y = num2 + num10;
                        renderTarget.DrawTextLayout(new Vector2(x2, y), Column[i].Row[j].textLayout, defaultForegroundBrush);
                    }
                }
            }

            using SolidColorBrush brush3 = new SolidColorBrush(renderTarget, FrameColor);
            RectangleF rect3 = new RectangleF(location.X - BorderWidth / 4f, location.Y - BorderWidth / 4f, TableWidth, TableHeight);
            renderTarget.DrawRectangle(rect3, brush3, FrameWidth);
        }

        private float sumColumnWidths(int endIndex)
        {
            float num = 0f;
            for (int i = 0; i < endIndex; i++)
            {
                if (Column[i].ColumnExist)
                {
                    num += (float)Column[i].ColumnWidth + BorderWidth / 2f;
                }
            }

            return num;
        }

        private float sumRowHeights(int endIndex)
        {
            float num = 0f;
            for (int i = 0; i < endIndex; i++)
            {
                if (RowExist[i])
                {
                    num += (float)RowHeight[i] + BorderWidth / 2f;
                }
            }

            return num;
        }

        private Vector2 GetLocation(ChartControl chartControl, ChartScale chartScale)
        {
            float num = 15f;
            float num2 = 15f;
            int panelIndex = chartScale.PanelIndex;
            float num3 = chartControl.ChartPanels[panelIndex].Y;
            float num4 = chartControl.ChartPanels[panelIndex].X;
            return TablePosition switch
            {
                LuxTablePosition.TopLeft => new Vector2(num4 + num2, num3 + num),
                LuxTablePosition.TopCenter => new Vector2(num4 + (float)chartScale.Width / 2f - TableWidth / 2f - num2 / 2f, num3 + num),
                LuxTablePosition.TopRight => new Vector2(num4 + (float)chartScale.Width - TableWidth - num2, num3 + num),
                LuxTablePosition.MiddleLeft => new Vector2(num4 + num2, num3 + ((float)chartScale.Height / 2f - TableHeight / 2f)),
                LuxTablePosition.MiddleCenter => new Vector2(num4 + (float)chartScale.Width / 2f - TableWidth / 2f - num2 / 2f, num3 + ((float)chartScale.Height / 2f - TableHeight / 2f)),
                LuxTablePosition.MiddleRight => new Vector2(num4 + (float)chartScale.Width - TableWidth - num2, num3 + ((float)chartScale.Height / 2f - TableHeight / 2f)),
                LuxTablePosition.BottomLeft => new Vector2(num4 + num2, num3 + ((float)chartScale.Height - TableHeight - num)),
                LuxTablePosition.BottomCenter => new Vector2(num4 + (float)chartScale.Width / 2f - TableWidth / 2f - num2 / 2f, num3 + ((float)chartScale.Height - TableHeight - num)),
                LuxTablePosition.BottomRight => new Vector2(num4 + (float)chartScale.Width - TableWidth - num2, num3 + ((float)chartScale.Height - TableHeight - num)),
                _ => new Vector2(num4 + num2, num3 + num),
            };
        }
    }
}
