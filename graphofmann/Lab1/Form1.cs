using Lab1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace Lab1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            ClearChart();
            ClearTables();

            //считывание с файла
            var openFile = new OpenFileDialog();

            openFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog.RestoreDirectory = true;

            if (openFile.ShowDialog() == DialogResult.OK)
            {
                var mainClass = new MainClass();

                var resultModel = mainClass.OpenFile(openFile.FileName);
                if (resultModel.Message != "")
                {
                    MessageBox.Show(resultModel.Message);
                    return;
                }

                mainClass.DefineConst();

                Fill1DTable(resultModel.Numbers, dgNumbers);
                DrawSeries(resultModel.Numbers, chart1);

                var resultSignTest = mainClass.SignTest();
                FillConclusion(resultSignTest);

                var resultMannaTest = mainClass.CriterionManna();
                FillConclusionManna(resultMannaTest);

                var resultAutocorrelation = mainClass.Autocorrelation();
                DrawSeries(resultAutocorrelation, chart2);

                var resultStationarityProcess = mainClass.AutocorrelationStationaryProcess();
                FillTable(resultStationarityProcess);

               // var isStationarityProcess = mainClass.IsStationarityProcess(resultStationarityProcess.Conslusion);
               // FillConclusion(isStationarityProcess);
            }
        }

        private void Fill1DTable(List<double> numbers, DataGridView dg)
        {
            dg.RowCount = numbers.Count() != 0 ? numbers.Count() : 1;

            for (int i = 0; i < numbers.Count(); i++)
            {
                dg.Rows[i].HeaderCell.Value = (i + 1).ToString();
                dg.Rows[i].Cells[0].Value = numbers[i].ToString();
            }
        }

        private void FillTable(StationarityProcessResultModel resultModel)
        {
            dgTable.ColumnCount = resultModel.Numbers.Count() != 0 ? resultModel.Numbers[0].Count() : 1;
            dgTable.RowCount = resultModel.Numbers.Count() != 0 ? resultModel.Numbers.Count() : 1;
            dgTable.RowCount += 3;

            for (int i = 0; i < resultModel.Numbers.Count(); i++)
            {
                for (int j = 0; j < resultModel.Numbers[i].Count(); j++)
                {
                    dgTable.Rows[i].Cells[j].Value = Math.Round(resultModel.Numbers[i][j], 4).ToString();
                    dgTable.Columns[j].HeaderCell.Value = j.ToString();
                }
                dgTable.Rows[i].HeaderCell.Value = i.ToString();
            }
            
            for (int j = 0; j < resultModel.Conslusion.Count(); j++)
            {
                dgTable.Rows[dgTable.RowCount - 1].Cells[j + 1].Value = resultModel.Conslusion[j].IsStationarity;
                dgTable.Rows[dgTable.RowCount - 2].Cells[j + 1].Value = Math.Round(resultModel.Conslusion[j].Pirson, 4);
                dgTable.Rows[dgTable.RowCount - 3].Cells[j + 1].Value = Math.Round(resultModel.Conslusion[j].Statistics, 4);
            }
        }

        private void FillConclusion(SignTestResultModel resultModel)
        {
            label3.Text = "";
            label3.Text = Math.Round(resultModel.S, 4).ToString();

            label5.Text = "";
            label5.Text = Math.Round(resultModel.QuantNorm, 4).ToString();

            label6.Text = "";
            label6.Text = resultModel.Conclusion;
        }

        private void FillConclusionManna(SignTestResultModel resultModel)
        {
            label8.Text = "";
            label8.Text = resultModel.Conclusion;

            label9.Text = "";
            label9.Text = Math.Round(resultModel.S, 4).ToString();
        }

        private void DrawSeries(List<double> numbers, Chart chart)
        {
            for (int i = 0; i < numbers.Count(); i++)
            {
                chart.Series[0].Points.AddXY(i, numbers[i]);
            }
        }

        private void ClearTables()
        {
            dgNumbers.Rows.Clear();
        }

        private void ClearChart()
        {
            for (int i = 0; i < chart1.Series.Count(); i++)
            {
                chart1.Series[i].Points.Clear();
            }

            for (int i = 0; i < chart2.Series.Count(); i++)
            {
                chart2.Series[i].Points.Clear();
            }
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void chart1_Click(object sender, EventArgs e)
        {

        }

        private void label8_Click(object sender, EventArgs e)
        {

        }
    }
}
