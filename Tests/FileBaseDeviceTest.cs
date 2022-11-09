using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Text;

namespace Pavlo.MyDAL.Tests
{
    [TestClass]
    public class FileBaseDeviceUnitTest
    {
        /// <summary>
        /// Input: true TekFile. Create new file by EmulateTek70000CSVfile(). Compare the data (time and voltage) in this two files.
        /// </summary>
        [TestMethod]
        public void TestEmulateTek70000CSVfile_CheckWithRealTekFile()
        {
            string inFileName = @"..\..\DataFiles\Tek70000example.csv";
            string outFileName = @"..\..\DataFiles\emulTmp.csv";
            //permissible delta for signals comparison
            double deltaVoltage = 1e-12;

            //reading true Tektronix file
            FileBaseDevice trueTekFile = null;
            using (FileStream dataFileFS = new FileStream(inFileName, FileMode.Open))
            {
                using (StreamReader dataFileSR = new StreamReader(dataFileFS, System.Text.Encoding.ASCII))
                {
                    //search of fileType
                    trueTekFile = new FileTektronix7000Series(dataFileSR);
                    bool res = trueTekFile.ProcessFileHeader();
                    if (!res)
                    {
                        trueTekFile = null;
                    }
                    if (trueTekFile == null)
                        throw new Exception();

                    // dataFile type is identified
                    //reading the voltage data
                    trueTekFile.FillChannelVoltages();
                }
            }


            using (FileStream outFS = new FileStream(outFileName, FileMode.Create, FileAccess.Write, FileShare.None))
            using (StreamWriter outSW = new StreamWriter(outFS, Encoding.ASCII))
            {
                trueTekFile.EmulateTek70000CSVfile(outSW, 0);
            }

            //reading the EMULATED file
            FileBaseDevice emulatedDataFileBaseDevice = null;
            using (FileStream dataFileFS = new FileStream(outFileName, FileMode.Open))
            {
                using (StreamReader dataFileSR = new StreamReader(dataFileFS, System.Text.Encoding.ASCII))
                {
                    //search of fileType
                    emulatedDataFileBaseDevice = new FileTektronix7000Series(dataFileSR);
                    bool res = emulatedDataFileBaseDevice.ProcessFileHeader();
                    if (!res)
                    {
                        emulatedDataFileBaseDevice = null;
                    }
                    if (emulatedDataFileBaseDevice == null)
                        throw new Exception();

                    // dataFile type is identified
                    //reading the voltage data
                    emulatedDataFileBaseDevice.FillChannelVoltages();
                }
            }
            //permissible delta for signals comparison
            double deltaTime = trueTekFile.dt / 1000000;

            //COMPARE THE TIMES ARRAY
            for (int i = 0; i < trueTekFile.Times.Length; i++)
            {
                Assert.AreEqual(trueTekFile.Times[i], emulatedDataFileBaseDevice.Times[i], deltaTime);
            }

            //COMPARE THE CHANNELS COUNT
            Assert.AreEqual(trueTekFile.Voltages.Length, emulatedDataFileBaseDevice.Voltages.Length);

            //COMPARE THE VOLTAGES ARRAY
            for (int i = 0; i < emulatedDataFileBaseDevice.FramesCount; i++)
                for (int j = 0; j < emulatedDataFileBaseDevice.Voltages[0][i].Length; j++)
                {
                    Assert.AreEqual(trueTekFile.Voltages[0][i][j], emulatedDataFileBaseDevice.Voltages[0][i][j], deltaVoltage);
                }
        }
    }
}
