using Lab1.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Lab1
{
    public class MainClass
    {
        public List<double> Numbers { get; set; }
        public double Alpha { get; set; }
        public int L { get; set; }
        public int K { get; set; }
        public List<double> AutocorrelationN { get; set; }
        public List<int> Tau { get; set; }
        public List<List<double>> AutocorrelationKoef { get; set; }

        public MainClass()
        {
            Numbers = new List<double>();
            AutocorrelationKoef = new List<List<double>>();
            Tau = new List<int>();
            Alpha = 0.05;
        }

        public void DefineConst()
        {
            L = Numbers.Count() / 2;
            K = Numbers.Count() / 3;

            for (int i = 0; i <= L; i++)
            {
                Tau.Add(i);
            }
        }

        public OpenFileResultModel OpenFile(string fileName)
        {
            var resultModel = new OpenFileResultModel();
            var line = "";

            var reader = new StreamReader(fileName);
            string numberToParse;
            double current;

            var ci = CultureInfo.InvariantCulture.Clone() as CultureInfo;
            ci.NumberFormat.NumberDecimalSeparator = ".";

            try
            {
                while ((line = reader.ReadLine()) != null)
                {
                    try
                    {
                        var doubleStrArr = line.Split(' ');
                        doubleStrArr = doubleStrArr.Where(str => str != "").ToArray();

                        for (int i = 0; i < doubleStrArr.Length; i++)
                        {
                            numberToParse = doubleStrArr[i];
                            current = double.Parse(numberToParse, ci);
                            if (current != 0)
                            Numbers.Add(current);
                        }
                        resultModel.Message = "";
                    }

                    catch (Exception exception)
                    {
                        resultModel.Message = "File read error+\n" + exception.Message;
                    }
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                resultModel.Message = "Error: Could not read file from disk. Original error: " + ex.Message;
            }
            resultModel.Numbers = Numbers;

            return resultModel;
        }

       

        public SignTestResultModel CriterionManna()
        {
            var resultModel = new SignTestResultModel();
            var y = new List<double>();
            double c = 0;

            for (int i = 0; i < Numbers.Count() - 1; i++)
            {
                for (int j = i + 1; j <= Numbers.Count(); j++)
                {
                    if (Numbers[i] < Numbers[j])
                    {
                        y.Add(1);
                        break;
                    }
                    else if (Numbers[i] == Numbers[j])
                    {
                        y.Add(0.5);
                        break;
                    }
                    else
                    {
                        y.Add(0);
                        break;
                    }
                }
                c += y[i];
            }
            var m = (Numbers.Count() * (Numbers.Count() - 1)) / 4.0;
            var d = ((Numbers.Count() * 2.0 + 5) * Numbers.Count() * (Numbers.Count() - 1)) / 72.0;
            var u = (c - m + 0.5) / Math.Sqrt(d);
            resultModel.S = u;
           

            if (Math.Abs(u) <= QuantNorm(1 - Alpha / 2))
            {
                resultModel.Conclusion = "Ряд випадковий";
             
            }
            else if (Math.Abs(u) < - QuantNorm(1 - Alpha / 2))
                resultModel.Conclusion = "Ряд имеет тенденцию к убыванию";
            else resultModel.Conclusion = "Ряд имеет тенденцию к возрастанию";

            return resultModel;
        }


        public SignTestResultModel SignTest()
        {
            var resultModel = new SignTestResultModel();

            var y = new List<int>();
            var c = 0;

            for (int i = 1; i < Numbers.Count() - 1; i++)
            {
                if ((Numbers[i + 1] <= Numbers[i]) && (Numbers[i - 1] <= Numbers[i]))
                {
                    y.Add(1);
                }
                else if ((Numbers[i + 1] >= Numbers[i]) && (Numbers[i - 1] >= Numbers[i]))
                {
                    y.Add(1);
                }
                else y.Add(0);

                c += y[i - 1];
            }

            var m = 2.0 * (Numbers.Count() - 2) / 3.0;
            var d = (16 * Numbers.Count() - 29) / 90.0;

            var s = (c - m) / Math.Sqrt(d);

            if (Math.Abs(s) <= QuantNorm(1 - Alpha / 2))
            {
                resultModel.Conclusion = "Ряд випадковий";
            }
            else resultModel.Conclusion = "Ряд циклічний";
            resultModel.S = s;
            resultModel.QuantNorm = QuantNorm(1 - Alpha / 2);

            return resultModel;
        }

        public List<double> Autocorrelation()
        {
            var N = Numbers.Count();

            var k = new List<double>();
            double sumk = 0;
            for (int i = 0; i < Tau.Count(); i++)
            {
                for (int j = 1; j < N - Tau[i]; j++)
                {
                    sumk += (Numbers[j] - M1(Tau[i])) * (Numbers[j + Tau[i]] - M2(Tau[i]));
                }
                k.Add(sumk / (N - Tau[i]));
                sumk = 0;
            }

            AutocorrelationN = new List<double>();
            for (int i = 0; i < Tau.Count(); i++)
            {
                AutocorrelationN.Add((N - Tau[i]) * k[i] / ((N - Tau[i] - 1) * Math.Sqrt(D1(Tau[i]) * D2(Tau[i]))));
            }

            AutocorrelationKoef.Add(AutocorrelationN);
            
            return AutocorrelationN;
        }

        public StationarityProcessResultModel AutocorrelationStationaryProcess()
        {
            var resultModel = new StationarityProcessResultModel();
            K = Numbers.Count() / 3;

            for (int i = 1; i < K; i++)
            {
                Numbers.RemoveAt(0);
                Autocorrelation();
            }
            resultModel.Numbers = AutocorrelationKoef;

            resultModel.Conslusion = new List<ProcessColumnResultModel>();
         
            for (int j = 1; j < Tau.Count(); j++)
            {
               var column = CreateColumn(j);
               var result = ProcessColumn(column, K, Tau[j]);
               resultModel.Conslusion.Add(result);
            }

            return resultModel;
        }

        public string IsStationarityProcess(List<ProcessColumnResultModel> conclusions)
        {
            foreach (var conclusion in conclusions)
            {
                if (!conclusion.IsStationarity)
                {
                    return "Процес не стационарный";
                }
            }

            return "Процес стационарный";
        }

        private List<double> CreateColumn(int index)
        {
            var column = new List<double>();
            for (int i = 0; i < AutocorrelationKoef.Count(); i++)
            {
                column.Add(AutocorrelationKoef[i][index]);
            }

            return column;
        }

        private ProcessColumnResultModel ProcessColumn(List<double> elements, int K, int tau)
        {
            var result = new ProcessColumnResultModel();
            double sum1 = 0;
            double sum2 = 0;
            double sum3 = 0;

            for (int k = 0; k < K; k++)
            {
                sum1 += (Numbers.Count() - k - 3) * Math.Pow(ZTK(elements[k]), 2);
                sum2 += (Numbers.Count() - k - 3) * ZTK(elements[k]);
                sum3 += Numbers.Count() - k - 3;
            }

            var hi = sum1 - Math.Pow(sum2, 2) / sum3;
            result.Statistics = hi;

            if (hi <= Pirson(1 - Alpha, K))
            {
                result.IsStationarity = true;
            }
            else
            {
                result.IsStationarity = false;
            }

            result.Pirson = Pirson(1 - Alpha, K);

            return result;
        }

        private double ZTK(double r)
        {
            var a = (1 + r) / (1 - r);
            var z = 1 / 2.0 * Math.Log(a);

            return z;
        }
        
        private double M1(int Tau)
        {
            double sum = 0;

            for (int i = 1; i < Numbers.Count() - Tau; i++)
            {
                sum += Numbers[i];
            }

            return sum / (Numbers.Count() - Tau);
        }

        private double M2(int Tau)
        {
            double sum = 0;

            for (int i = 1; i < Numbers.Count() - Tau; i++)
            {
                sum += Numbers[i + Tau];
            }

            return sum / (Numbers.Count() - Tau);
        }

        private double D1(int Tau)
        {
            double sumd1 = 0;

            for (int j = 1; j < Numbers.Count() - Tau; j++)
            {
                sumd1 += Math.Pow(Numbers[j] - M1(Tau), 2);
            }

            return sumd1 / (Numbers.Count() - Tau - 1);
        }

        private double D2(int Tau)
        {
            double sumd2 = 0;

            for (int j = 1; j < Numbers.Count() - Tau; j++)
            {
                sumd2 += Math.Pow(Numbers[j + Tau] - M2(Tau), 2);
            }

            return sumd2 / (Numbers.Count() - Tau - 1);
        }

        private double QuantNorm(double p)
        {
            double value;

            if (p > 0.5)
            {
                value = NormFunc(1 - p);
            }
            else
            {
                value = -NormFunc(p);
            }

            return value;
        }

        private double NormFunc(double param)
        {
            double fi,
                   t = Math.Sqrt(-2 * Math.Log(param)),
                   c0 = 2.515517,
                   c1 = 0.802853,
                   c2 = 0.010328,
                   d1 = 1.432788,
                   d2 = 0.1892659,
                   d3 = 0.001308;

            fi = t - (c0 + c1 * t + c2 * Math.Pow(t, 2)) / (1 + d1 * t + d2 * Math.Pow(t, 2) + Math.Pow(t, 3) * d3);

            return fi;
        }

        private double Pirson(double p, double v)
        {
            double value;

            value = v * Math.Pow((1 - 2 / (9 * v) + QuantNorm(p) * Math.Sqrt(2 / (9 * v))), 3);

            return value;
        }
    }
}