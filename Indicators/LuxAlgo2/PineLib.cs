#region Assembly LuxAlgo Indicator, Version=1.0.0.1, Culture=neutral, PublicKeyToken=null
// C:\Users\sshrestha\Documents\NinjaTrader 8\bin\Custom\LuxAlgo - LiquidityVoidsFVG.dll
// Decompiled with ICSharpCode.Decompiler 8.1.1.7464
#endregion

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using NinjaTrader.Core;
using NinjaTrader.Gui;
using NinjaTrader.Gui.NinjaScript;
using NinjaTrader.Gui.Tools;
using NinjaTrader.NinjaScript.DrawingTools;

namespace NinjaTrader.NinjaScript.Indicators.LuxAlgo2
{
    public class PineLib
    {
        public class PineTime
        {
            protected NinjaScriptBase owner;

            public PineTime(NinjaScriptBase thisOwner)
            {
                owner = thisOwner;
            }

            public int ToUnix(DateTime dateTime)
            {
                return (int)(dateTime.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
            }

            public DateTime ToDateTime(int unixTime)
            {
                return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(unixTime);
            }
        }

        public class PineAlerts
        {
            protected NinjaScriptBase owner;

            public PineAlerts(NinjaScriptBase thisOwner)
            {
                owner = thisOwner;
            }

            public void DoAlert(string message, int id = 0, Priority priority = Priority.Medium)
            {
                if (IsNewCandleAlert(id))
                {
                    owner.Alert(id.ToString(), priority, message, Globals.InstallDir + "\\sounds\\Alert1.wav", 1, Brushes.SlateBlue, Brushes.White);
                }
            }

            private static bool IsNewCandleAlert(int index = 0, bool reset = false)
            {
                if (reset)
                {
                    SavedCandleTime = new DateTime[50];
                }

                if (SavedCandleTime[index] != ownerStatic.Time[0])
                {
                    SavedCandleTime[index] = ownerStatic.Time[0];
                    return true;
                }

                return false;
            }
        }

        public class PineMath
        {
            protected NinjaScriptBase owner;

            public double E => System.Math.E;

            public double PI => System.Math.PI;

            public double PHI => 1.6180339887498949;

            public double RPHI => 0.61803398874989479;

            public PineMath(NinjaScriptBase thisOwner)
            {
                owner = thisOwner;
            }

            public T MaxValue<T>(params T[] values) where T : IComparable<T>
            {
                if (values.Length == 0)
                {
                    throw new ArgumentException("At least one value must be provided.");
                }

                return values.Max();
            }

            public T MinValue<T>(params T[] values) where T : IComparable<T>
            {
                if (values.Length == 0)
                {
                    throw new ArgumentException("At least one value must be provided.");
                }

                return values.Min();
            }

            public double AverageValue<T>(params T[] numbers) where T : IComparable<T>
            {
                double[] array = System.Array.ConvertAll(numbers, (T x) => Convert.ToDouble(x));
                return array.Sum() / (double)array.Length;
            }

            public double Avg<T>(params T[] numbers) where T : IComparable<T>
            {
                double[] array = System.Array.ConvertAll(numbers, (T x) => Convert.ToDouble(x));
                return array.Sum() / (double)array.Length;
            }

            public T Abs<T>(T value) where T : IComparable<T>
            {
                if (typeof(T) == typeof(int))
                {
                    return (T)(object)System.Math.Abs((int)(object)value);
                }

                if (typeof(T) == typeof(long))
                {
                    return (T)(object)System.Math.Abs((long)(object)value);
                }

                if (typeof(T) == typeof(float))
                {
                    return (T)(object)System.Math.Abs((float)(object)value);
                }

                if (typeof(T) == typeof(double))
                {
                    return (T)(object)System.Math.Abs((double)(object)value);
                }

                if (typeof(T) == typeof(decimal))
                {
                    return (T)(object)System.Math.Abs((decimal)(object)value);
                }

                throw new ArgumentException("Invalid type for Abs function");
            }

            public T AbsoluteValue<T>(T value) where T : IComparable<T>
            {
                if (typeof(T) == typeof(int))
                {
                    return (T)(object)System.Math.Abs((int)(object)value);
                }

                if (typeof(T) == typeof(long))
                {
                    return (T)(object)System.Math.Abs((long)(object)value);
                }

                if (typeof(T) == typeof(float))
                {
                    return (T)(object)System.Math.Abs((float)(object)value);
                }

                if (typeof(T) == typeof(double))
                {
                    return (T)(object)System.Math.Abs((double)(object)value);
                }

                if (typeof(T) == typeof(decimal))
                {
                    return (T)(object)System.Math.Abs((decimal)(object)value);
                }

                throw new ArgumentException("Invalid type for Abs function");
            }

            public int Sum(ISeries<int> series, int period = 0)
            {
                if (period == 0)
                {
                    return series[0];
                }

                int num = 0;
                for (int i = 0; i < period; i++)
                {
                    num += series[i];
                }

                return num;
            }

            public double Sum(ISeries<double> series, int period = 0)
            {
                if (period == 0)
                {
                    return series[0];
                }

                double num = 0.0;
                for (int i = 0; i < period; i++)
                {
                    num += series[i];
                }

                return num;
            }

            public double RandomNumber(double min = 0.0, double max = 1.0, int seed = 0)
            {
                return new Random(seed).NextDouble() * (max - min) + min;
            }

            public double RandomNumber(double min = 0.0, double max = 1.0)
            {
                return new Random().NextDouble() * (max - min) + min;
            }

            public double QuadInterpolation(int x1, double y1, int x2, double y2, int x3, double y3, int x)
            {
                double num = ((y3 - y1) / (double)(x3 - x1) - (y2 - y1) / (double)(x2 - x1)) / (double)(x3 - x2);
                double num2 = (y2 - y1) / (double)(x2 - x1) - num * (double)(x2 + x1);
                double num3 = y1 - num * (double)x1 * (double)x1 - num2 * (double)x1;
                return num * (double)x * (double)x + num2 * (double)x + num3;
            }

            public double CubicInterpolation(int x1, double y1, int x2, double y2, int x3, double y3, int x4, double y4, int x)
            {
                double num = (y2 - y1) / (double)(x2 - x1);
                double num2 = ((y3 - y2) / (double)(x3 - x2) - num) / (double)(x3 - x1);
                double num3 = (((y4 - y3) / (double)(x4 - x3) - num2) / (double)(x4 - x1) - num) / (double)(x4 - x2);
                return y1 - num * (double)x1 - num2 * (double)x1 * (double)x2 - num3 * (double)x1 * (double)x2 * (double)x3 + num * (double)x + num2 * (double)x * (double)x + num3 * (double)x * (double)x * (double)x;
            }

            public double Interpolation(int[] x, double[] y, int x0)
            {
                double num = 0.0;
                for (int i = 0; i < x.Length; i++)
                {
                    double num2 = y[i];
                    for (int j = 0; j < x.Length; j++)
                    {
                        if (i != j)
                        {
                            num2 *= (double)((x0 - x[j]) / (x[i] - x[j]));
                        }
                    }

                    num += num2;
                }

                return num;
            }
        }

        public class PineArray
        {
            protected NinjaScriptBase owner;

            public PineArray(NinjaScriptBase thisOwner)
            {
                owner = thisOwner;
            }

            public T PopElement<T>(ref T[] array)
            {
                if (array.Length == 0)
                {
                    return default(T);
                }

                T result = array[array.Length - 1];
                System.Array.Resize(ref array, array.Length - 1);
                return result;
            }

            public T ShiftElement<T>(ref T[] array)
            {
                if (array.Length == 0)
                {
                    return default(T);
                }

                T result = array[0];
                T[] array2 = new T[array.Length - 1];
                System.Array.Copy(array, 1, array2, 0, array.Length - 1);
                array = array2;
                return result;
            }

            public void UnshiftElement<T>(ref T[] array, T val)
            {
                array = array.Prepend(val).ToArray();
            }

            public int GetArraySize<T>(ref T[] array)
            {
                return array.Length;
            }

            public void PushElement<T>(ref T[] array, T val)
            {
                array = array.Append(val).ToArray();
            }

            public T GetElement<T>(ref T[] array, int index)
            {
                if (array.Length <= index)
                {
                    return default(T);
                }

                return array[index];
            }

            public void SetElement<T>(ref T[] array, int index, T value)
            {
                if (array.Length > index)
                {
                    array[index] = value;
                }
            }

            public void RemoveElement<T>(ref T[] myArray, int index)
            {
                if (index >= 0 && index < myArray.Length)
                {
                    for (int i = index; i < myArray.Length - 1; i++)
                    {
                        myArray[i] = myArray[i + 1];
                    }

                    System.Array.Resize(ref myArray, myArray.Length - 1);
                }
            }

            public void RemoveAt<T>(ref T[] myArray, int index)
            {
                RemoveElement(ref myArray, index);
            }

            public T[] SliceArray<T>(ref T[] array, int index_from, int index_to)
            {
                return array.Skip(index_from).Take(index_to - index_from).ToArray();
            }

            public T MaxArrayValue<T>(ref T[] array, int count = 0, int start = 0) where T : IComparable<T>
            {
                return array.Skip(start).Take((count > 0) ? count : (array.Length - start)).Max();
            }

            public T MinArrayValue<T>(ref T[] array, int count = 0, int start = 0) where T : IComparable<T>
            {
                return array.Skip(start).Take((count > 0) ? count : (array.Length - start)).Min();
            }

            public double Range(ref double[] array)
            {
                return MaxArrayValue(ref array) - MinArrayValue(ref array);
            }

            public double SumArrayElements<T>(ref T[] array, int count = 0, int start = 0) where T : IConvertible
            {
                return array.Skip(start).Take((count > 0) ? count : (array.Length - start)).Sum(delegate (T x)
                {
                    ref T reference = ref x;
                    T val = default(T);
                    if (val == null)
                    {
                        val = reference;
                        reference = ref val;
                    }

                    return reference.ToDouble(CultureInfo.InvariantCulture);
                });
            }

            public double AverageArrayElements<T>(ref T[] array, int count = 0, int start = 0) where T : IConvertible
            {
                return array.Skip(start).Take((count > 0) ? count : (array.Length - start)).Average(delegate (T x)
                {
                    ref T reference = ref x;
                    T val = default(T);
                    if (val == null)
                    {
                        val = reference;
                        reference = ref val;
                    }

                    return reference.ToDouble(CultureInfo.InvariantCulture);
                });
            }

            public double AverageArrayElements<T>(T[] array, int count = 0, int start = 0) where T : IConvertible
            {
                if (array.Length == 0)
                {
                    return 0.0;
                }

                return array.Skip(start).Take((count > 0) ? count : (array.Length - start)).Average(delegate (T x)
                {
                    ref T reference = ref x;
                    T val = default(T);
                    if (val == null)
                    {
                        val = reference;
                        reference = ref val;
                    }

                    return reference.ToDouble(CultureInfo.InvariantCulture);
                });
            }

            public T[] ConvertFromArguments<T>(params object[] args)
            {
                List<T> list = new List<T>();
                foreach (object obj in args)
                {
                    if (obj is T)
                    {
                        list.Add((T)obj);
                    }
                }

                return list.ToArray();
            }

            public void ClearArray<T>(ref T[] array)
            {
                array = new T[0];
            }

            public T[] CopyArray<T>(ref T[] array)
            {
                return array.ToArray();
            }

            public void InsertInArray<T>(ref T[] array, int index, params T[] values)
            {
                array = array.Take(index).Concat(values).Concat(array.Skip(index))
                    .ToArray();
            }

            public int IndexOfElement<T>(ref T[] array, T value)
            {
                return System.Array.IndexOf(array, value);
            }

            public int[] SortIndices<T>(ref T[] id, bool ascending = true)
            {
                if (ascending)
                {
                    return (from pair in id.Select((T value, int index) => new { value, index })
                            orderby pair.value
                            select pair.index).ToArray();
                }

                return (from pair in id.Select((T value, int index) => new { value, index })
                        orderby pair.value descending
                        select pair.index).ToArray();
            }

            public void Sort<T>(ref T[] id, bool ascending = true)
            {
                if (ascending)
                {
                    id = id.OrderBy((T x) => x).ToArray();
                }
                else
                {
                    id = id.OrderByDescending((T x) => x).ToArray();
                }
            }

            public double Covariance<T1, T2>(T1[] array1, T2[] array2, bool biased = true)
            {
                if (array1.Length != array2.Length)
                {
                    return double.NaN;
                }

                int num = array1.Length;
                double num2 = array1.Average((T1 x) => Convert.ToDouble(x));
                double num3 = array2.Average((T2 x) => Convert.ToDouble(x));
                double num4 = 0.0;
                for (int i = 0; i < array1.Length; i++)
                {
                    double num5 = Convert.ToDouble(array1[i]);
                    double num6 = Convert.ToDouble(array2[i]);
                    num4 += (num5 - num2) * (num6 - num3);
                }

                if (biased)
                {
                    return num4 / (double)num;
                }

                return num4 / (double)(num - 1);
            }

            public double Variance<T>(T[] array, bool biased = true)
            {
                int num = array.Length;
                double num2 = array.Average((T x) => Convert.ToDouble(x));
                double num3 = 0.0;
                for (int i = 0; i < array.Length; i++)
                {
                    num3 += System.Math.Pow(Convert.ToDouble(array[i]) - num2, 2.0);
                }

                if (biased)
                {
                    return num3 / (double)num;
                }

                return num3 / (double)(num - 1);
            }

            public double StdDev<T>(T[] array, bool biased = true)
            {
                return System.Math.Sqrt(Variance(array, biased));
            }

            public double PercentileLinearInterpolation(double[] array, double percentage)
            {
                if (array.Length == 0)
                {
                    return double.NaN;
                }

                if (percentage < 0.0 || percentage > 100.0)
                {
                    return double.NaN;
                }

                if (percentage == 100.0)
                {
                    return array.Max();
                }

                if (percentage == 0.0)
                {
                    return array.Min();
                }

                double[] array2 = array.OrderBy((double x) => x).ToArray();
                double num = (double)(array2.Length + 1) * percentage / 100.0;
                int num2 = (int)num;
                double num3 = num - (double)num2;
                if (num2 == 0)
                {
                    return array2[0];
                }

                if (num2 == array2.Length)
                {
                    return array2[array2.Length - 1];
                }

                return array2[num2 - 1] + num3 * (array2[num2] - array2[num2 - 1]);
            }

            public double Median(double[] array)
            {
                if (array.Length == 0)
                {
                    return 0.0;
                }

                double[] array2 = array.OrderBy((double x) => x).ToArray();
                if (array2.Length % 2 == 0)
                {
                    return (array2[array2.Length / 2 - 1] + array2[array2.Length / 2]) / 2.0;
                }

                return array2[array2.Length / 2];
            }
        }

        public class TechnicalAnalysis
        {
            protected NinjaScriptBase owner;

            public TechnicalAnalysis(NinjaScriptBase thisOwner)
            {
                owner = thisOwner;
            }

            public double PivotHigh(ISeries<double> source, int leftbars, int rightbars, int offset = 0)
            {
                if (owner.CurrentBar < leftbars + rightbars)
                {
                    return double.NaN;
                }

                rightbars += offset;
                double num = source[rightbars];
                double num2 = source[offset];
                for (int i = offset; i <= leftbars + rightbars; i++)
                {
                    num2 = ((i > rightbars) ? ((owner.Instrument.MasterInstrument.Compare(num2, source[i]) == 1) ? num2 : (num2 + 1.0)) : System.Math.Max(num2, source[i]));
                }

                if (num2 != num)
                {
                    return double.NaN;
                }

                return num;
            }

            public double PivotLow(ISeries<double> source, int leftbars, int rightbars, int offset = 0)
            {
                if (owner.CurrentBar < leftbars + rightbars)
                {
                    return double.NaN;
                }

                rightbars += offset;
                double num = source[rightbars];
                double num2 = source[offset];
                for (int i = offset; i <= leftbars + rightbars; i++)
                {
                    num2 = ((i > rightbars) ? ((owner.Instrument.MasterInstrument.Compare(num2, source[i]) == -1) ? num2 : (num2 - 1.0)) : System.Math.Min(num2, source[i]));
                }

                if (num2 != num)
                {
                    return double.NaN;
                }

                return num;
            }

            public double PivotHigh(int leftbars, int rightbars, int offset = 0)
            {
                if (owner.CurrentBar < leftbars + rightbars)
                {
                    return double.NaN;
                }

                rightbars += offset;
                double num = owner.High[rightbars];
                double num2 = owner.High[offset];
                for (int i = offset; i <= leftbars + rightbars; i++)
                {
                    num2 = ((i > rightbars) ? ((owner.Instrument.MasterInstrument.Compare(num2, owner.High[i]) == 1) ? num2 : (num2 + 1.0)) : System.Math.Max(num2, owner.High[i]));
                }

                if (num2 != num)
                {
                    return double.NaN;
                }

                return num;
            }

            public double PivotLow(int leftbars, int rightbars, int offset = 0)
            {
                if (owner.CurrentBar < leftbars + rightbars)
                {
                    return double.NaN;
                }

                rightbars += offset;
                double num = owner.Low[rightbars];
                double num2 = owner.Low[offset];
                for (int i = offset; i <= leftbars + rightbars; i++)
                {
                    num2 = ((i > rightbars) ? ((owner.Instrument.MasterInstrument.Compare(num2, owner.Low[i]) == -1) ? num2 : (num2 - 1.0)) : System.Math.Min(num2, owner.Low[i]));
                }

                if (num2 != num)
                {
                    return double.NaN;
                }

                return num;
            }

            public double Change(ISeries<double> source, int length = 0, int offset = 0)
            {
                return source[offset] - source[1 + offset + length];
            }

            public bool Cross(ISeries<double> source1, ISeries<double> source2, int index = 0, int offset1 = 0, int offset2 = 0)
            {
                return IsCross(CrossEnum.Any, source1, source2, index, offset1, offset2);
            }

            public bool Cross(ISeries<double> source, double value, int index = 0, int offset1 = 0)
            {
                return IsCross(CrossEnum.Any, source, value, index, offset1);
            }

            public bool CrossUnder(ISeries<double> source1, ISeries<double> source2, int index = 0, int offset1 = 0, int offset2 = 0)
            {
                return IsCross(CrossEnum.Down, source1, source2, index, offset1, offset2);
            }

            public bool CrossUnder(ISeries<double> source, double value, int index = 0, int offset1 = 0)
            {
                return IsCross(CrossEnum.Down, source, value, index, offset1);
            }

            public bool CrossOver(ISeries<double> source1, ISeries<double> source2, int index = 0, int offset1 = 0, int offset2 = 0)
            {
                return IsCross(CrossEnum.Up, source1, source2, index, offset1, offset2);
            }

            public bool CrossOver(ISeries<double> source, double value, int index = 0, int offset1 = 0)
            {
                return IsCross(CrossEnum.Up, source, value, index, offset1);
            }

            public bool IsCross(CrossEnum dir, ISeries<double> array1, ISeries<double> array2, int i = 0, int offset1 = 0, int offset2 = 0)
            {
                bool result = false;
                if ((dir == CrossEnum.Up || dir == CrossEnum.Any) && array1[i + offset1].CompareTo(array2[i + offset2]) > 0 && array1[i + 1 + offset1].CompareTo(array2[i + 1 + offset2]) <= 0)
                {
                    result = true;
                }

                if ((dir == CrossEnum.Down || dir == CrossEnum.Any) && array1[i + offset1].CompareTo(array2[i + offset2]) < 0 && array1[i + 1 + offset1].CompareTo(array2[i + 1 + offset2]) >= 0)
                {
                    result = true;
                }

                return result;
            }

            public bool IsCross<T2>(CrossEnum dir, ISeries<double> array1, T2 value, int i = 0, int offset1 = 0)
            {
                bool result = false;
                if ((dir == CrossEnum.Up || dir == CrossEnum.Any) && array1[i + offset1].CompareTo(value) > 0 && array1[i + 1 + offset1].CompareTo(value) <= 0)
                {
                    result = true;
                }

                if ((dir == CrossEnum.Down || dir == CrossEnum.Any) && array1[i + offset1].CompareTo(value) < 0 && array1[i + 1 + offset1].CompareTo(value) >= 0)
                {
                    result = true;
                }

                return result;
            }

            public double ValueWhen(ISeries<double> c_condition, ISeries<double> c_source, int c_occurrence)
            {
                int num = 0;
                for (int i = 0; i < owner.CurrentBar - 1; i++)
                {
                    if (!double.IsNaN(c_condition[i]) && c_condition[i] != 0.0)
                    {
                        if (num == c_occurrence)
                        {
                            return c_source[i];
                        }

                        num++;
                    }
                }

                return double.NaN;
            }

            public T2 ValueWhen<T2>(ISeries<double> c_condition, double value, ISeries<T2> c_source, int c_occurrence, T2 DefVal)
            {
                int num = 0;
                double num2 = 1E-10;
                for (int i = 0; i < owner.CurrentBar - 1; i++)
                {
                    if (System.Math.Abs(c_condition[i] - value) < num2)
                    {
                        if (num == c_occurrence)
                        {
                            return c_source[i];
                        }

                        num++;
                    }
                }

                return DefVal;
            }

            public T2 ValueWhen<T2>(ISeries<int> c_condition, int value, ISeries<T2> c_source, int c_occurrence, T2 DefVal)
            {
                int num = 0;
                double num2 = 1E-10;
                for (int i = 0; i < owner.CurrentBar - 1; i++)
                {
                    if ((double)System.Math.Abs(c_condition[i] - value) < num2)
                    {
                        if (num == c_occurrence)
                        {
                            return c_source[i];
                        }

                        num++;
                    }
                }

                return DefVal;
            }

            public T2 ValueWhen<T2>(ISeries<bool> c_condition, ISeries<T2> c_source, int c_occurrence, T2 DefVal)
            {
                int num = 0;
                for (int i = 0; i < owner.CurrentBar - 1; i++)
                {
                    if (c_condition[i])
                    {
                        if (num == c_occurrence)
                        {
                            return c_source[i];
                        }

                        num++;
                    }
                }

                return DefVal;
            }

            public int mom(ISeries<int> source, int length = 1, int offset = 0)
            {
                return source[offset] - source[offset + length];
            }

            public double mom(ISeries<double> source, int length = 1, int offset = 0)
            {
                return source[offset] - source[offset + length];
            }

            public bool isZero(double val, double eps = 0.0)
            {
                return System.Math.Abs(val) <= eps;
            }

            public double GetValueByShift(double x1, double y1, double x2, double y2, double x)
            {
                double num = (y1 - y2) / (x1 - x2);
                double num2 = y1 - num * x1;
                return num * x + num2;
            }

            public bool Falling(ISeries<double> source, int length = 1)
            {
                if (owner.CurrentBar < length)
                {
                    return false;
                }

                for (int i = 0; i < length; i++)
                {
                    if (source[i] >= source[i + 1])
                    {
                        return false;
                    }
                }

                return true;
            }

            public bool Rising(ISeries<double> source, int length = 1)
            {
                if (owner.CurrentBar < length)
                {
                    return false;
                }

                for (int i = 0; i < length; i++)
                {
                    if (source[i] <= source[i + 1])
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        public class PineColors
        {
            protected NinjaScriptBase owner;

            public PineColors(NinjaScriptBase thisOwner)
            {
                owner = thisOwner;
            }

            public Color FromGradient(double c_value, double bot_value, double top_value, Color bot_col, Color top_col)
            {
                Color result = default(Color);
                if (c_value <= bot_value)
                {
                    return bot_col;
                }

                if (c_value >= top_value)
                {
                    return top_col;
                }

                double num = top_value - bot_value;
                double num2 = c_value - bot_value;
                double num3 = ((num2 != 0.0) ? (num2 / num) : 0.0);
                result.A = (byte)((double)(int)bot_col.A + num3 * (double)(top_col.A - bot_col.A));
                result.R = (byte)((double)(int)bot_col.R + num3 * (double)(top_col.R - bot_col.R));
                result.G = (byte)((double)(int)bot_col.G + num3 * (double)(top_col.G - bot_col.G));
                result.B = (byte)((double)(int)bot_col.B + num3 * (double)(top_col.B - bot_col.B));
                return result;
            }

            public int GetCss(double c_value, double bot_value, double top_value)
            {
                c_value = System.Math.Max(System.Math.Min(c_value, top_value), bot_value);
                double num = top_value - bot_value;
                double num2 = c_value - bot_value;
                if (num2 == 0.0)
                {
                    return 0;
                }

                return (int)(num2 / num * 100.0);
            }

            public Color FromBrush(Brush myCustomBrush, double myOpacity = 100.0)
            {
                myOpacity = 255.0 * (System.Math.Min(System.Math.Max(myOpacity, 0.0), 100.0) / 100.0);
                SolidColorBrush solidColorBrush = myCustomBrush as SolidColorBrush;
                solidColorBrush.Freeze();
                return System.Windows.Media.Color.FromArgb((byte)myOpacity, solidColorBrush.Color.R, solidColorBrush.Color.G, solidColorBrush.Color.B);
            }

            public Brush FromPlotDefined(int i)
            {
                return owner.Plots[i].Pen.Brush;
            }

            public void CreateGradientArray(ref Brush[] GradientBrushes, Color dnCol, Color upCol)
            {
                GradientBrushes = new Brush[101];
                for (int i = 0; i <= 100; i++)
                {
                    Color color = FromGradient(i, 0.0, 100.0, dnCol, upCol);
                    GradientBrushes[i] = new SolidColorBrush(color);
                    GradientBrushes[i].Freeze();
                }
            }

            public void CreateGradientArray3(ref Brush[] GradientBrushes, Color dnCol, Color midCol, Color upCol, double percent = 50.0)
            {
                GradientBrushes = new Brush[101];
                percent = ((percent > 99.0) ? 99.0 : ((percent < 1.0) ? 1.0 : percent));
                for (int i = 0; i <= 100; i++)
                {
                    Color color = (((double)i > percent) ? FromGradient(i, percent, 100.0, midCol, upCol) : FromGradient(i, 0.0, percent, dnCol, midCol));
                    GradientBrushes[i] = new SolidColorBrush(color);
                    GradientBrushes[i].Freeze();
                }
            }

            public void CreateOpacityGradientArray(ref Brush[] GradientBrushes, Brush useBrush)
            {
                GradientBrushes = new Brush[101];
                for (int i = 0; i <= 100; i++)
                {
                    GradientBrushes[i] = new SolidColorBrush(FromBrush(useBrush, i));
                    GradientBrushes[i].Freeze();
                }
            }
        }

        public class PineLine
        {
            protected NinjaScriptBase owner;

            private int objectCount;

            public DashStyleHelper style_solid;

            public DashStyleHelper style_dashed;

            public DashStyleHelper style_dotted;

            public PineLine(NinjaScriptBase thisOwner)
            {
                owner = thisOwner;
                objectCount = 0;
                style_solid = DashStyleHelper.Solid;
                style_dashed = DashStyleHelper.Dash;
                style_dotted = DashStyleHelper.Dot;
            }

            public NinjaTrader.NinjaScript.DrawingTools.Line New(int x1 = 0, double y1 = 0.0, int x2 = 0, double y2 = 0.0, Brush color = null, DashStyleHelper style = DashStyleHelper.Solid, int width = 1, bool isAutoScale = false)
            {
                if (objectCount > MaxObjectsLimit)
                {
                    objectCount = 0;
                }

                color = color ?? Brushes.Transparent;
                return Draw.Line(owner, "[LuxAlgo] PineLib Line " + ++objectCount, isAutoScale, owner.Time.GetValueAt(x1), y1, owner.Time.GetValueAt(x2), y2, color, style, width);
            }

            public NinjaTrader.NinjaScript.DrawingTools.Line New(DateTime x1, double y1 = 0.0, DateTime x2 = default(DateTime), double y2 = 0.0, Brush color = null, DashStyleHelper style = DashStyleHelper.Solid, int width = 1, bool isAutoScale = false)
            {
                if (objectCount > MaxObjectsLimit)
                {
                    objectCount = 0;
                }

                color = color ?? Brushes.Transparent;
                return Draw.Line(owner, "[LuxAlgo] PineLib Line " + ++objectCount, isAutoScale, x1, y1, x2, y2, color, style, width);
            }

            public void Delete(ref NinjaTrader.NinjaScript.DrawingTools.Line line)
            {
                FixPineObject(ref line);
                renderBase.RemoveDrawObject(line.Tag);
            }

            public void Delete(NinjaTrader.NinjaScript.DrawingTools.Line line)
            {
                renderBase.RemoveDrawObject(line.Tag);
            }

            public void DeleteAll()
            {
                foreach (IDrawingTool item in drawObjects.ToList())
                {
                    if (item is NinjaTrader.NinjaScript.DrawingTools.Line && item.DrawnBy == owner)
                    {
                        renderBase.RemoveDrawObject(item.Tag);
                    }
                }
            }

            public void SetX1(ref NinjaTrader.NinjaScript.DrawingTools.Line line, int x)
            {
                FixPineObject(ref line);
                line.StartAnchor.SlotIndex = x;
            }

            public void SetX1(ref NinjaTrader.NinjaScript.DrawingTools.Line line, DateTime x)
            {
                FixPineObject(ref line);
                line.StartAnchor.Time = x;
            }

            public void SetY1(ref NinjaTrader.NinjaScript.DrawingTools.Line line, double y)
            {
                FixPineObject(ref line);
                line.StartAnchor.Price = y;
            }

            public void SetX2(ref NinjaTrader.NinjaScript.DrawingTools.Line line, int x)
            {
                FixPineObject(ref line);
                line.EndAnchor.SlotIndex = x;
            }

            public void SetX2(ref NinjaTrader.NinjaScript.DrawingTools.Line line, DateTime x)
            {
                FixPineObject(ref line);
                line.EndAnchor.Time = x;
            }

            public void SetY2(ref NinjaTrader.NinjaScript.DrawingTools.Line line, double y)
            {
                FixPineObject(ref line);
                line.EndAnchor.Price = y;
            }

            public void SetXY1(ref NinjaTrader.NinjaScript.DrawingTools.Line line, int x, double y)
            {
                FixPineObject(ref line);
                line.StartAnchor.SlotIndex = x;
                line.StartAnchor.Price = y;
            }

            public void SetXY1(ref NinjaTrader.NinjaScript.DrawingTools.Line line, DateTime x, double y)
            {
                FixPineObject(ref line);
                line.StartAnchor.Time = x;
                line.StartAnchor.Price = y;
            }

            public void SetXY2(ref NinjaTrader.NinjaScript.DrawingTools.Line line, int x, double y)
            {
                FixPineObject(ref line);
                line.EndAnchor.SlotIndex = x;
                line.EndAnchor.Price = y;
            }

            public void SetXY2(ref NinjaTrader.NinjaScript.DrawingTools.Line line, DateTime x, double y)
            {
                FixPineObject(ref line);
                line.EndAnchor.Time = x;
                line.EndAnchor.Price = y;
            }

            public void SetStyle(ref NinjaTrader.NinjaScript.DrawingTools.Line line, DashStyleHelper style)
            {
                FixPineObject(ref line);
                line.Stroke.DashStyleHelper = style;
            }

            public void SetColor(ref NinjaTrader.NinjaScript.DrawingTools.Line line, Brush color)
            {
                FixPineObject(ref line);
                line.Stroke.Brush = color;
            }

            public int GetX1(ref NinjaTrader.NinjaScript.DrawingTools.Line line)
            {
                FixPineObject(ref line);
                return (int)line.StartAnchor.SlotIndex;
            }

            public DateTime GetX1Time(ref NinjaTrader.NinjaScript.DrawingTools.Line line)
            {
                FixPineObject(ref line);
                return line.StartAnchor.Time;
            }

            public double GetY1(ref NinjaTrader.NinjaScript.DrawingTools.Line line)
            {
                FixPineObject(ref line);
                return line.StartAnchor.Price;
            }

            public int GetX2(ref NinjaTrader.NinjaScript.DrawingTools.Line line)
            {
                FixPineObject(ref line);
                return (int)line.EndAnchor.SlotIndex;
            }

            public DateTime GetX2Time(ref NinjaTrader.NinjaScript.DrawingTools.Line line)
            {
                FixPineObject(ref line);
                return line.EndAnchor.Time;
            }

            public double GetY2(ref NinjaTrader.NinjaScript.DrawingTools.Line line)
            {
                FixPineObject(ref line);
                return line.EndAnchor.Price;
            }

            public double GetPrice(ref NinjaTrader.NinjaScript.DrawingTools.Line line, int index = 0)
            {
                FixPineObject(ref line);
                double slotIndex = line.StartAnchor.SlotIndex;
                double price = line.StartAnchor.Price;
                double slotIndex2 = line.EndAnchor.SlotIndex;
                return (line.EndAnchor.Price - price) / (slotIndex2 - slotIndex) * ((double)index - slotIndex) + price;
            }
        }

        public class PineLabel
        {
            protected NinjaScriptBase owner;

            private int objectCount;

            public int style_label_down;

            public int style_label_up;

            public PineLabel(NinjaScriptBase thisOwner)
            {
                owner = thisOwner;
                objectCount = 0;
                style_label_down = 1;
                style_label_up = -1;
            }

            public Text New(int x1 = 0, double y1 = 0.0, string text = "", Brush color = null, Brush colorOutline = null, int opacity = 0, int style = 1, Brush textcolor = null, SimpleFont font = null, TextAlignment textalign = TextAlignment.Center, int offset = 0)
            {
                if (objectCount > MaxObjectsLimit)
                {
                    objectCount = 0;
                }

                color = color ?? Brushes.Transparent;
                colorOutline = colorOutline ?? Brushes.Transparent;
                textcolor = textcolor ?? Brushes.Transparent;
                font = font ?? new SimpleFont("Arial", 12);
                return Draw.Text(owner, "[LuxAlgo] PineLib Label " + ++objectCount, isAutoScale: false, text, owner.Time.GetValueAt(x1), y1, style * offset, textcolor, font, textalign, colorOutline, color, opacity);
            }

            public Text New(DateTime x1, double y1 = 0.0, string text = "", Brush color = null, Brush colorOutline = null, int opacity = 0, int style = 1, Brush textcolor = null, SimpleFont font = null, TextAlignment textalign = TextAlignment.Center, int offset = 0)
            {
                if (objectCount > MaxObjectsLimit)
                {
                    objectCount = 0;
                }

                color = color ?? Brushes.Transparent;
                colorOutline = colorOutline ?? Brushes.Transparent;
                textcolor = textcolor ?? Brushes.Transparent;
                font = font ?? new SimpleFont("Arial", 12);
                return Draw.Text(owner, "[LuxAlgo] PineLib Label " + ++objectCount, isAutoScale: false, text, x1, y1, style * offset, textcolor, font, textalign, colorOutline, color, opacity);
            }

            public void Delete(ref Text label)
            {
                FixPineObject(ref label);
                renderBase.RemoveDrawObject(label.Tag);
            }

            public void Delete(Text label)
            {
                renderBase.RemoveDrawObject(label.Tag);
            }

            public void DeleteAll()
            {
                foreach (IDrawingTool item in drawObjects.ToList())
                {
                    if (item is Text && item.DrawnBy == owner)
                    {
                        renderBase.RemoveDrawObject(item.Tag);
                    }
                }
            }

            public void SetText(ref Text label, string text)
            {
                FixPineObject(ref label);
                label.DisplayText = text;
            }

            public string GetText(ref Text label)
            {
                FixPineObject(ref label);
                return label.DisplayText;
            }

            public void SetX(ref Text label, int x)
            {
                FixPineObject(ref label);
                label.Anchor.SlotIndex = x;
            }

            public void SetX(ref Text label, DateTime x)
            {
                FixPineObject(ref label);
                label.Anchor.Time = x;
            }

            public void SetY(ref Text label, double y)
            {
                FixPineObject(ref label);
                label.Anchor.Price = y;
            }

            public void SetXY(ref Text label, int x, double y)
            {
                FixPineObject(ref label);
                label.Anchor.SlotIndex = x;
                label.Anchor.Price = y;
            }

            public void SetXY(ref Text label, DateTime x, double y)
            {
                FixPineObject(ref label);
                label.Anchor.Time = x;
                label.Anchor.Price = y;
            }

            public int GetX(ref Text label)
            {
                FixPineObject(ref label);
                return (int)label.Anchor.SlotIndex;
            }

            public DateTime GetXTime(ref Text label)
            {
                FixPineObject(ref label);
                return label.Anchor.Time;
            }

            public double GetY(ref Text label)
            {
                FixPineObject(ref label);
                return label.Anchor.Price;
            }

            public void SetTextColor(ref Text label, Brush color)
            {
                FixPineObject(ref label);
                label.TextBrush = color;
            }

            public void SetStyle(ref Text label, int style, int offset = 15)
            {
                FixPineObject(ref label);
                label.YPixelOffset = style * offset;
            }

            public void SetAlignment(ref Text label, TextAlignment align)
            {
                FixPineObject(ref label);
                label.Alignment = align;
            }

            public void SetSize(ref Text label, int size)
            {
                FixPineObject(ref label);
                label.Font.Size = size;
            }

            public void SetFont(ref Text label, SimpleFont font)
            {
                FixPineObject(ref label);
                label.Font = font;
            }
        }

        public enum textHorizontal
        {
            align_left,
            align_center,
            align_right
        }

        public enum textVertical
        {
            align_top,
            align_center,
            align_bottom
        }

        public class PineBox
        {
            protected NinjaScriptBase owner;

            private int objectCount;

            public DashStyleHelper style_solid;

            public DashStyleHelper style_dashed;

            public DashStyleHelper style_dotted;

            public PineBox(NinjaScriptBase thisOwner)
            {
                owner = thisOwner;
                objectCount = 0;
                style_solid = DashStyleHelper.Solid;
                style_dashed = DashStyleHelper.Dash;
                style_dotted = DashStyleHelper.Dot;
            }

            public Rectangle New(int left = 0, double top = 0.0, int right = 0, double bottom = 0.0, Brush border_color = null, int border_width = 1, DashStyleHelper border_style = DashStyleHelper.Solid, Brush bgcolor = null, int opacity = 20)
            {
                if (objectCount > MaxObjectsLimit)
                {
                    objectCount = 0;
                }

                border_color = border_color ?? Brushes.Transparent;
                bgcolor = bgcolor ?? Brushes.Transparent;
                Rectangle rectangle = Draw.Rectangle(owner, "[LuxAlgo] PineLib Box " + ++objectCount, isAutoScale: false, owner.Time.GetValueAt(left), top, owner.Time.GetValueAt(right), bottom, border_color, bgcolor, opacity);
                rectangle.OutlineStroke.DashStyleHelper = border_style;
                rectangle.OutlineStroke.Width = border_width;
                return rectangle;
            }

            public Rectangle New(DateTime left, double top = 0.0, DateTime right = default(DateTime), double bottom = 0.0, Brush border_color = null, int border_width = 1, DashStyleHelper border_style = DashStyleHelper.Solid, Brush bgcolor = null, int opacity = 20)
            {
                if (objectCount > MaxObjectsLimit)
                {
                    objectCount = 0;
                }

                border_color = border_color ?? Brushes.Transparent;
                bgcolor = bgcolor ?? Brushes.Transparent;
                Rectangle rectangle = Draw.Rectangle(owner, "[LuxAlgo] PineLib Box " + ++objectCount, isAutoScale: false, left, top, right, bottom, border_color, bgcolor, opacity);
                rectangle.OutlineStroke.DashStyleHelper = border_style;
                rectangle.OutlineStroke.Width = border_width;
                return rectangle;
            }

            public void Delete(ref Rectangle box)
            {
                FixPineObject(ref box);
                renderBase.RemoveDrawObject(box.Tag);
            }

            public void Delete(Rectangle box)
            {
                renderBase.RemoveDrawObject(box.Tag);
            }

            public void DeleteAll()
            {
                foreach (IDrawingTool item in drawObjects.ToList())
                {
                    if (item is Rectangle && item.DrawnBy == owner)
                    {
                        renderBase.RemoveDrawObject(item.Tag);
                    }
                }
            }

            public void SetLeft(ref Rectangle box, int x)
            {
                FixPineObject(ref box);
                box.StartAnchor.SlotIndex = x;
            }

            public void SetLeft(ref Rectangle box, DateTime x)
            {
                FixPineObject(ref box);
                box.StartAnchor.Time = x;
            }

            public void SetTop(ref Rectangle box, double y)
            {
                FixPineObject(ref box);
                box.StartAnchor.Price = y;
            }

            public void SetRight(ref Rectangle box, int x)
            {
                FixPineObject(ref box);
                box.EndAnchor.SlotIndex = x;
            }

            public void SetRight(ref Rectangle box, DateTime x)
            {
                FixPineObject(ref box);
                box.EndAnchor.Time = x;
            }

            public void SetBottom(ref Rectangle box, double y)
            {
                FixPineObject(ref box);
                box.EndAnchor.Price = y;
            }

            public void SetLeftTop(ref Rectangle box, int x, double y)
            {
                FixPineObject(ref box);
                box.StartAnchor.SlotIndex = x;
                box.StartAnchor.Price = y;
            }

            public void SetLeftTop(ref Rectangle box, DateTime x, double y)
            {
                FixPineObject(ref box);
                box.StartAnchor.Time = x;
                box.StartAnchor.Price = y;
            }

            public void SetRightBottom(ref Rectangle box, int x, double y)
            {
                FixPineObject(ref box);
                box.EndAnchor.SlotIndex = x;
                box.EndAnchor.Price = y;
            }

            public void SetRightBottom(ref Rectangle box, DateTime x, double y)
            {
                FixPineObject(ref box);
                box.EndAnchor.Time = x;
                box.EndAnchor.Price = y;
            }

            public int GetLeft(ref Rectangle box)
            {
                FixPineObject(ref box);
                return (int)box.StartAnchor.SlotIndex;
            }

            public DateTime GetLeftTime(ref Rectangle box)
            {
                FixPineObject(ref box);
                return box.StartAnchor.Time;
            }

            public double GetTop(ref Rectangle box)
            {
                FixPineObject(ref box);
                return box.StartAnchor.Price;
            }

            public int GetRight(ref Rectangle box)
            {
                FixPineObject(ref box);
                return (int)box.EndAnchor.SlotIndex;
            }

            public DateTime GetRightTime(ref Rectangle box)
            {
                FixPineObject(ref box);
                return box.EndAnchor.Time;
            }

            public double GetBottom(ref Rectangle box)
            {
                FixPineObject(ref box);
                return box.EndAnchor.Price;
            }

            public void SetBgColor(ref Rectangle box, Brush color, int opacity = -1)
            {
                FixPineObject(ref box);
                box.AreaBrush = color;
                if (opacity > -1)
                {
                    box.AreaOpacity = opacity;
                }
            }

            public void SetOpacity(ref Rectangle box, int opacity)
            {
                FixPineObject(ref box);
                box.AreaOpacity = opacity;
            }

            public void SetBorderColor(ref Rectangle box, Brush color)
            {
                FixPineObject(ref box);
                box.OutlineStroke.Brush = color;
            }

            public void SetBorderWidth(ref Rectangle box, int width)
            {
                FixPineObject(ref box);
                box.OutlineStroke.Width = width;
            }

            public void SetBorderStyle(ref Rectangle box, DashStyleHelper style)
            {
                FixPineObject(ref box);
                box.OutlineStroke.DashStyleHelper = style;
            }
        }

        public class PineBarState
        {
            protected NinjaScriptBase owner;

            public bool IsLast => owner.CurrentBar == owner.Bars.Count - 1 - ((owner.Calculate == Calculate.OnBarClose) ? 1 : 0);

            public bool IsFirst => owner.CurrentBar == 0;

            public PineBarState(NinjaScriptBase thisOwner)
            {
                owner = thisOwner;
            }
        }

        public class PineStr
        {
            protected NinjaScriptBase owner;

            public PineStr(NinjaScriptBase thisOwner)
            {
                owner = thisOwner;
            }

            public bool Contains(string cSource, string str)
            {
                return cSource.Contains(str);
            }

            public bool EndsWith(string cSource, string str)
            {
                return cSource.EndsWith(str);
            }

            public bool StartsWith(string cSource, string str)
            {
                return cSource.StartsWith(str);
            }

            public int Length(string cSource)
            {
                return cSource.Length;
            }

            public string ToLower(string cSource)
            {
                return cSource.ToLower();
            }

            public string ToUpper(string cSource)
            {
                return cSource.ToUpper();
            }

            public int Pos(string cSource, string str)
            {
                return cSource.IndexOf(str);
            }

            public string ReplaceAll(string cSource, string target, string replacement)
            {
                return cSource.Replace(target, replacement);
            }

            public string Substring(string cSource, int beginPos)
            {
                return cSource.Substring(beginPos);
            }

            public string Substring(string cSource, int beginPos, int endPos)
            {
                return cSource.Substring(beginPos, endPos - beginPos);
            }

            public string ToString<T>(T value, string formatString = "")
            {
                double num = Convert.ToDouble(value);
                switch (formatString)
                {
                    case "mintick":
                        return num.ToString("F" + CultureInfo.CurrentCulture.NumberFormat.NumberDecimalDigits);
                    case "percent":
                        return $"{num * 100.0:0.00}%";
                    case "volume":
                        {
                            double num2 = System.Math.Round(num);
                            if (System.Math.Abs(num2) > 1000000000.0)
                            {
                                num2 /= 1000000.0;
                                return $"{num2:0.000}B";
                            }

                            if (System.Math.Abs(num2) > 1000000.0)
                            {
                                num2 /= 1000.0;
                                return $"{num2:0.000}M";
                            }

                            if (System.Math.Abs(num2) > 1000.0)
                            {
                                return $"{num2:0.000}K";
                            }

                            return $"{num2:0.000}";
                        }
                    default:
                        if (formatString.Contains("."))
                        {
                            return num.ToString("F" + formatString.Split('.')[1].Length);
                        }

                        return num.ToString();
                }
            }

            public void Split(string cSource, string cSeparator, out string[] cResult)
            {
                cResult = cSource.Split(new string[1] { cSeparator }, StringSplitOptions.None);
            }

            public double ToNumber(string cSource)
            {
                return double.Parse(cSource);
            }
        }

        public TechnicalAnalysis TA;

        public PineColors Color;

        public PineBarState BarState;

        public PineArray Array;

        public PineMath Math;

        public PineLine Line;

        public PineLabel Label;

        public PineBox Box;

        public PineStr Str;

        public PineAlerts Alerts;

        public PineTime Time;

        protected NinjaScriptBase owner;

        protected static NinjaScriptBase ownerStatic;

        protected static IndicatorRenderBase renderBase;

        protected static IDrawObjects drawObjects;

        protected static DateTime[] SavedCandleTime;

        protected static int MaxObjectsLimit;

        public PineLib(NinjaScriptBase thisOwner, IndicatorRenderBase RenderBase, IDrawObjects thisdrawObjects, int maxObjectsLimit = 5000)
        {
            owner = thisOwner;
            ownerStatic = owner;
            renderBase = RenderBase;
            drawObjects = thisdrawObjects;
            TA = new TechnicalAnalysis(owner);
            Color = new PineColors(owner);
            BarState = new PineBarState(owner);
            Array = new PineArray(owner);
            Math = new PineMath(owner);
            Line = new PineLine(owner);
            Label = new PineLabel(owner);
            Box = new PineBox(owner);
            Str = new PineStr(owner);
            Alerts = new PineAlerts(owner);
            Time = new PineTime(owner);
            SavedCandleTime = new DateTime[50];
            MaxObjectsLimit = System.Math.Max(maxObjectsLimit, 1);
        }

        public string FormatNumber(double number)
        {
            if (number >= 1000000000.0)
            {
                return (number / 1000000000.0).ToString("0.000B");
            }

            if (number >= 1000000.0)
            {
                return (number / 1000000.0).ToString("0.000M");
            }

            if (number >= 1000.0)
            {
                return (number / 1000.0).ToString("0.000K");
            }

            return number.ToString();
        }

        public bool IsTimeframeChanged(int BarsIndex)
        {
            return owner.BarsArray[BarsIndex].GetBar(owner.Time[0]) != owner.BarsArray[BarsIndex].GetBar(owner.Time[1]);
        }

        public bool IsTimeframeStart(int BarsIndex, int CandleIndex = 0)
        {
            for (int i = 1; i <= BarsIndex; i++)
            {
                if (owner.Time[0] < owner.Times[i].GetValueAt(CandleIndex))
                {
                    return false;
                }
            }

            return true;
        }

        public double FixNaN(ISeries<double> source, double value = double.NaN)
        {
            if (double.IsNaN(value))
            {
                return source[1];
            }

            return value;
        }

        public double Nz(double source, double value = 0.0)
        {
            if (double.IsNaN(source))
            {
                return value;
            }

            return source;
        }

        public static bool IsNewCandle(int index = 0, bool reset = false)
        {
            if (reset)
            {
                SavedCandleTime = new DateTime[50];
            }

            if (SavedCandleTime[index] != ownerStatic.Time[0])
            {
                SavedCandleTime[index] = ownerStatic.Time[0];
                return true;
            }

            return false;
        }

        public static void FixPineObject(ref NinjaTrader.NinjaScript.DrawingTools.Line obj)
        {
            if (obj.State == State.Active)
            {
                return;
            }

            foreach (IDrawingTool item in drawObjects.ToList())
            {
                if (item is NinjaTrader.NinjaScript.DrawingTools.Line && item.DrawnBy == obj.DrawnBy && item.Tag == obj.Tag)
                {
                    obj = (NinjaTrader.NinjaScript.DrawingTools.Line)item;
                    break;
                }
            }
        }

        public static void FixPineObject(ref Text obj)
        {
            if (obj.State == State.Active)
            {
                return;
            }

            foreach (DrawingTool item in drawObjects.ToList())
            {
                if (item is Text && item.DrawnBy == obj.DrawnBy && item.Tag == obj.Tag)
                {
                    obj = (Text)item;
                    break;
                }
            }
        }

        public static void FixPineObject(ref Rectangle obj)
        {
            if (obj.State == State.Active)
            {
                return;
            }

            foreach (IDrawingTool item in drawObjects.ToList())
            {
                if (item is Rectangle && item.DrawnBy == obj.DrawnBy && item.Tag == obj.Tag)
                {
                    obj = (Rectangle)item;
                    break;
                }
            }
        }
    }
}
#if false // Decompilation log
'23' items in cache
------------------
Resolve: 'mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Found single assembly: 'mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Load from: 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319\mscorlib.dll'
------------------
Resolve: 'System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Found single assembly: 'System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Load from: 'C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\System.dll'
------------------
Resolve: 'NinjaTrader.Gui, Version=8.1.2.0, Culture=neutral, PublicKeyToken=null'
Found single assembly: 'NinjaTrader.Gui, Version=8.1.2.1, Culture=neutral, PublicKeyToken=null'
WARN: Version mismatch. Expected: '8.1.2.0', Got: '8.1.2.1'
Load from: 'C:\Program Files\NinjaTrader 8\bin\NinjaTrader.Gui.dll'
------------------
Resolve: 'NinjaTrader.Core, Version=8.1.2.0, Culture=neutral, PublicKeyToken=null'
Found single assembly: 'NinjaTrader.Core, Version=8.1.2.1, Culture=neutral, PublicKeyToken=null'
WARN: Version mismatch. Expected: '8.1.2.0', Got: '8.1.2.1'
Load from: 'C:\Program Files\NinjaTrader 8\bin\NinjaTrader.Core.dll'
------------------
Resolve: 'WindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35'
Found single assembly: 'WindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35'
Load from: 'C:\Windows\Microsoft.NET\Framework\v4.0.30319\WPF\WindowsBase.dll'
------------------
Resolve: 'SharpDX.Direct2D1, Version=2.6.3.0, Culture=neutral, PublicKeyToken=b4dcf0f35e5521f1'
Found single assembly: 'SharpDX.Direct2D1, Version=2.6.3.0, Culture=neutral, PublicKeyToken=b4dcf0f35e5521f1'
Load from: 'C:\Program Files\NinjaTrader 8\bin\SharpDX.Direct2D1.dll'
------------------
Resolve: 'PresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35'
Found single assembly: 'PresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35'
Load from: 'C:\Windows\Microsoft.NET\Framework\v4.0.30319\WPF\PresentationCore.dll'
------------------
Resolve: 'SharpDX, Version=2.6.3.0, Culture=neutral, PublicKeyToken=b4dcf0f35e5521f1'
Found single assembly: 'SharpDX, Version=2.6.3.0, Culture=neutral, PublicKeyToken=b4dcf0f35e5521f1'
Load from: 'C:\Program Files\NinjaTrader 8\bin\SharpDX.dll'
------------------
Resolve: 'System.ComponentModel.DataAnnotations, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35'
Found single assembly: 'System.ComponentModel.DataAnnotations, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35'
Load from: 'C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\System.ComponentModel.DataAnnotations.dll'
------------------
Resolve: 'System.Xml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Found single assembly: 'System.Xml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Load from: 'C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\System.Xml.dll'
------------------
Resolve: 'PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35'
Found single assembly: 'PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35'
Load from: 'C:\Windows\Microsoft.NET\Framework\v4.0.30319\WPF\PresentationFramework.dll'
------------------
Resolve: 'NinjaTrader.Vendor, Version=8.1.2.0, Culture=neutral, PublicKeyToken=null'
Found single assembly: 'NinjaTrader.Vendor, Version=8.1.2.1, Culture=neutral, PublicKeyToken=null'
WARN: Version mismatch. Expected: '8.1.2.0', Got: '8.1.2.1'
Load from: 'C:\Users\sshrestha\Documents\NinjaTrader 8\bin\Custom\NinjaTrader.Vendor.dll'
------------------
Resolve: 'System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Found single assembly: 'System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Load from: 'C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\System.Core.dll'
------------------
Resolve: 'System.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Found single assembly: 'System.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Load from: 'C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\System.Xaml.dll'
#endif
