using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Text;

namespace Pavlo.MyDAL.Tests
{
    [TestClass]
    public class FileTektronix7000Series_datUnitTest
    {
        /// <summary>
        /// Compare data from *.dat and *.csv files from Tektronix 7000Series.
        /// </summary>
        [TestMethod]
        public void TestFileTektronix7000Series_dat_CheckWithCSVTekFile()
        {
            string csvFileName = @"..\..\DataFiles\Tek70000example.csv";

            string headerDatFileName = @"..\..\DataFiles\Tek70000example_hdr.dat";
            string datFileName = @"..\..\DataFiles\Tek70000example.dat";

            //permissible delta for signals comparison
            double deltaVoltage = 4e-11;
            //fruction of dt for calculation of permissible delta for times
            double deltaTimeFractionOfdt = 1d/1000000;//permissible delta for times comparison will be calculated later

            //reading csv Tektronix file
            FileBaseDevice csvTekFile = null;
            using (FileStream dataFileFS = new FileStream(csvFileName, FileMode.Open))
            {
                using (StreamReader dataFileSR = new StreamReader(dataFileFS, System.Text.Encoding.ASCII))
                {
                    //search of fileType
                    csvTekFile = new FileTektronix7000Series(dataFileSR);
                    bool res = csvTekFile.ProcessFileHeader();
                    if (!res)
                    {
                        csvTekFile = null;
                    }
                    if (csvTekFile == null)
                        throw new Exception();

                    // dataFile type is identified
                    //reading the voltage data
                    csvTekFile.FillChannelVoltages();
                }
            }

            //reading *.dat Tektronix files
            FileTektronix7000Series_dat datTekFile;
            using (FileStream dataFileFS = new FileStream(datFileName, FileMode.Open))
            using (FileStream headerDataFileFS = new FileStream(headerDatFileName, FileMode.Open))
            {
                using (StreamReader dataFileSR = new StreamReader(dataFileFS, System.Text.Encoding.ASCII))
                using (StreamReader headerDataFileSR = new StreamReader(headerDataFileFS, System.Text.Encoding.ASCII))
                {
                    datTekFile = new FileTektronix7000Series_dat(dataFileSR, headerDataFileSR);
                    bool res = datTekFile.ProcessFileHeader();
                    if (!res)
                    {
                        datTekFile = null;
                    }
                    if (datTekFile == null)
                        throw new Exception();

                    //reading the voltage data
                    datTekFile.FillChannelVoltages();
                }
            }

            //permissible delta for times comparison
            double deltaTime = csvTekFile.dt * deltaTimeFractionOfdt;

            //COMPARE THE TIMES ARRAY
            for (int i = 0; i < csvTekFile.Times.Length; i++)
            {
                Assert.AreEqual(csvTekFile.Times[i], datTekFile.Times[i], deltaTime);
            }
            
            //COMPARE THE CHANNELS COUNT
            Assert.AreEqual(csvTekFile.Voltages.Length, datTekFile.Voltages.Length);

            //COMPARE THE VOLTAGES ARRAY
            for (int i = 0; i < datTekFile.FramesCount; i++)
                for (int j = 0; j < datTekFile.Voltages[0][i].Length; j++)
                {
                    Assert.AreEqual(csvTekFile.Voltages[0][i][j], datTekFile.Voltages[0][i][j], deltaVoltage);
                }
        }
    }
}
