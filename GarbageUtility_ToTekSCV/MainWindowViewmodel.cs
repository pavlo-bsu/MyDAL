using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OxyPlot;

namespace Pavlo.MyDAL.GarbageUtility_ToTekSCV
{
    /// <summary>
    /// The class contains both viewmodel and model.
    /// Logic of utylity:
    /// 1. Open a file with waveform
    /// 2. Show the first frame(signal) on a window (oxyplot is used)
    /// 3. Convert file mentioned at step#1 to *.csv file (like recoded by Tektronix 7000 series)
    /// 
    /// All file pathes and etc. are hardcoded in the class. 
    /// I.e. a GARBAGE UTILITY )) for launch it only couple of times.
    /// </summary>
    public class MainWindowViewmodel
    {
        public MainWindowViewmodel()
        {

            #region workWithFiles

            /*
            //work with *.dat files

            string headerDatFileName = @"D:\file_hdr.dat";
            string datFileName = @"D:\file.dat";

            FileTektronix7000Series_dat file;

            using (FileStream dataFileFS = new FileStream(datFileName, FileMode.Open))
            using (FileStream headerDataFileFS = new FileStream(headerDatFileName, FileMode.Open))
            {
                using (StreamReader dataFileSR = new StreamReader(dataFileFS, System.Text.Encoding.ASCII))
                using (StreamReader headerDataFileSR = new StreamReader(headerDataFileFS, System.Text.Encoding.ASCII))
                {
                    file = new FileTektronix7000Series_dat(dataFileSR, headerDataFileSR);
            */

            string datFileName = @"D:\Tek3k.csv";

            FileTektronix2kAnd3kSeries file;

            using (FileStream dataFileFS = new FileStream(datFileName, FileMode.Open))
            {
                using (StreamReader dataFileSR = new StreamReader(dataFileFS, System.Text.Encoding.ASCII))
                {
                    file = new FileTektronix2kAnd3kSeries(dataFileSR);
                    bool res = file.ProcessFileHeader();
                    if (!res)
                    {
                        file = null;
                    }
                    if (file == null)
                        throw new Exception();

                    //reading the voltage data
                    file.FillChannelVoltages();

                }
            }


            #endregion

            FillTheOXYPlotModel(file);
            SaveTheEmulation(file);
        }

        /// <summary>
        /// Conver file to *.csv file (like recoded by Tektronix 7000 series)
        /// </summary>
        /// <param name="file"></param>
        private static void SaveTheEmulation(FileBaseDevice file)
        {
            //save the file
            string outFileName = @"d:\Tek7k.csv";
            using (FileStream outFS = new FileStream(outFileName, FileMode.Create, FileAccess.Write, FileShare.None))
            using (StreamWriter outSW = new StreamWriter(outFS, Encoding.ASCII))
            {
                file.EmulateTek70000CSVfile(outSW, 0);
            }
        }

        private void FillTheOXYPlotModel(FileBaseDevice file)
        {
            //number of frame to show
            int frame = 0;

            //number of channel to show
            int channel = 0;

            TheModel = new PlotModel();
            TheModel.Title = $"frame #{frame}";

            //create series 
            var series = new OxyPlot.Series.LineSeries();

            for (int i = 0; i < file.SamplesCount; i++)
            {
                series.Points.Add(new DataPoint(file.Times[i], file.Voltages[channel][frame][i]));
            }
            TheModel.Series.Add(series);
        }

        public PlotModel TheModel { get; private set; }
    }
}
