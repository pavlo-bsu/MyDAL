using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pavlo.MyDAL
{
    /// <summary>
    /// The class represents a file that contains data from 1 frame(burst):
    /// 1srt column is time date (equidistant values),
    /// all other columns contain voltage values from different channels.
    /// File has header of 1 string with description of "time" column and "voltage" columns. Description shouldn't be a double or int value
    /// </summary>
    public class FileWithHeaderOf1str:FileBaseDevice
    {
        private readonly System.Globalization.NumberStyles nStyle = System.Globalization.NumberStyles.AllowExponent | System.Globalization.NumberStyles.AllowLeadingSign | System.Globalization.NumberStyles.AllowDecimalPoint;
        private readonly System.Globalization.CultureInfo nCulture = System.Globalization.CultureInfo.InvariantCulture;
        //private readonly int roughHeaderStringsCount;

        public FileWithHeaderOf1str(StreamReader inStr)
            : base(inStr)
        {
            this.FramesCount = 1; //always single frame
        }

        #region header
        public override bool ProcessFileHeader()
        {
            this.Separator = ',';

            if (ProcessFirstHeaderString(inputSR.ReadLine()) == false)
                return false;

            if (Process2ndString(inputSR.ReadLine()) == false)
                return false;

            if (Process3rdString(inputSR.ReadLine()) == false)
                return false;

            bool res = CheckAllTheRestLines();
            if (res == false)
                return false;

            FillTimesTable();

            return true;
        }

        private bool ProcessFirstHeaderString(string str)
        {
            string[] strSplitted = str.Split(Separator);

            if (strSplitted.Length < 2)
            {
                return false;
            }
            ChannelsCount = strSplitted.Length-1;

            double val = 0d;
            bool res = false;
            foreach (string cell in strSplitted)
            {
                res = double.TryParse(cell, nStyle, nCulture, out val);
                if (res) //Column description of a  shouldn't be a double or int value.
                    return false;
            }

            return true;
        }

        private bool Process2ndString(string str)
        {
            string[] strSplitted = str.Split(Separator);
            
            //check of cells count
            if(strSplitted.Length!=ChannelsCount+1)
                return false;

            double val = 0;
            bool result;
            result = double.TryParse(strSplitted[0], nStyle, nCulture, out val);
            this.t0 = val;

            return result;
        }

        private bool Process3rdString(string str)
        {
            string[] strSplitted = str.Split(Separator);

            //check of cells count
            if (strSplitted.Length != ChannelsCount + 1)
                return false;

            double val = 0;
            bool result;
            result = double.TryParse(strSplitted[0], nStyle, nCulture, out val);
            this.dt = val-t0;

            return result;
        }

        /// <summary>
        /// Calc SamplesCount
        /// Check of cells count for all the other lines of the file
        /// </summary>
        /// <returns>false - if file isunexpected file type</returns>
        private bool CheckAllTheRestLines()
        {
            int counter = 2; // 2strings already processed

            string tmpStr;

            while (!inputSR.EndOfStream)
            {
                tmpStr = inputSR.ReadLine();

                if (string.IsNullOrEmpty(tmpStr))
                {
                    break;
                }
                else
                {
                    counter++;

                    //check of cells count
                    string[] strSplitted = tmpStr.Split(Separator);
                    if (strSplitted.Length != ChannelsCount + 1)
                        return false;
                }
            }
            this.SamplesCount = counter;
            return true;
        }
        #endregion

        public override bool FillChannelVoltages()
        {
            ResetInputStreamReader();
            string[] strSplitted;

            try
            {
                //skip 1st string (header)
                inputSR.ReadLine();

                int linesCount = this.FramesCount * this.SamplesCount;

                //initiation of voltages array
                this.Voltages = new double[this.ChannelsCount][][];
                for (int i = 0; i < this.ChannelsCount; i++)
                {
                    Voltages[i] = new double[this.FramesCount][];
                    for (int j = 0; j < this.FramesCount; j++)
                    {
                        Voltages[i][j] = new double[this.SamplesCount];
                    }
                }

                //filling of voltages array
                for (int i = 0; i < this.FramesCount; i++)
                {
                    for (int j = 0; j < this.SamplesCount; j++)
                    {
                        strSplitted = inputSR.ReadLine().Split(this.Separator);
                        for (int k = 0; k < this.ChannelsCount; k++)
                        {
                            Voltages[k][i][j] = double.Parse(strSplitted[k + 1].TrimStart(), nStyle, nCulture);
                        }
                    }
                }
            }
            catch
            {
                return false;
            }

            //check the rest of the file
            if (inputSR.EndOfStream)
            {
                return true;

            }
            else
            {
                bool res1 = string.IsNullOrEmpty(inputSR.ReadLine());
                bool res2 = inputSR.EndOfStream;
                if (res1 & res2)
                    return true;
                else
                    return false;
            }

            //return inputSR.EndOfStream;
        }
    }
}
