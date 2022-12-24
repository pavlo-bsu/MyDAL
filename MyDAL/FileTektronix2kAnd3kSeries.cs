using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pavlo.MyDAL
{
    /// <summary>
    /// class represents a Waveforms saved by Tektronix 2000 or 3000 series
    /// standard oscill. output to file is expected
    /// If there are some channels and math calculation in the file, then math calc. is ignored!
    /// </summary>
    public class FileTektronix2kAnd3kSeries:FileBaseDevice
    {

        private readonly System.Globalization.NumberStyles nStyle = System.Globalization.NumberStyles.AllowExponent | System.Globalization.NumberStyles.AllowLeadingSign | System.Globalization.NumberStyles.AllowDecimalPoint;
        private readonly System.Globalization.CultureInfo nCulture = System.Globalization.CultureInfo.InvariantCulture;
        private readonly int roughHeaderStringsCount;

        public FileTektronix2kAnd3kSeries(StreamReader inStr)
            : base(inStr)
        {
            roughHeaderStringsCount = 21;   //16 lines for Tektronix 2000 series. 21 lines for Tektronix 3000 series
            this.FramesCount = 1; //always single frame
        }

        #region header
        public override bool ProcessFileHeader()
        {
            for (int i = 0; i < roughHeaderStringsCount; i++)
            {
                if (ProcessHeaderString(inputSR.ReadLine()) == false)
                    return false;
            }

            var res = SearchFor_t0();
            if (!res)
                return false;

            res = CheckFileForSavingBetweenCursorsAndCorrectSamplesCount();
            if (res == false)
                return false;

            FillTimesTable();

            return true;
        }

        public bool SearchFor_t0()
        {
            //going to the first time data
            ResetInputStreamReader();
            string tmpStr;
            do
            { tmpStr = inputSR.ReadLine();}
            while (!tmpStr.StartsWith("TIME"));

            var strArr = inputSR.ReadLine().Split(this.Separator);

            double val = 0;
            bool result;
            result = double.TryParse(strArr[0], nStyle, nCulture, out val);
            this.t0 = val;

            return result;
        }

        private bool ProcessHeaderString(string str)
        {
            if (str == null)
                return false;
            if (str.StartsWith("Model"))
            {
                return ProcessFirstHeaderString(str);
            }
            if (str.StartsWith("Sample Interval"))
            {
                return Process7HeaderString(str);
            }
            if (str.StartsWith("Record Length"))
            {
                return Process9HeaderString(str);
            }
            if (str.StartsWith("TIME"))
            {
                return Process16HeaderString(str);
            }
            return true;
        }

        private bool Process7HeaderString(string str)
        {
            string[] strSplitted = str.Split(Separator);
            double val = 0;
            bool result;
            result = double.TryParse(strSplitted[1], nStyle, nCulture, out val);
            this.dt = val;
            
            return result;
        }

        private bool ProcessFirstHeaderString(string str)
        {
            if (str.Length<6)
                return false;

            this.Separator = str[5];

            return true;
        }

        private bool Process9HeaderString(string str)
        {
            string[] strSplitted = str.Split(Separator);
            int val = 0;
            bool result;
            result = int.TryParse(strSplitted[1], nStyle, nCulture, out val);

            if (result==false)
            {
                //Sample Interval value has a double type
                //for up-to-date version of Tektronix MDO 3 series
                double dVal = 0d;
                result = double.TryParse(strSplitted[1], nStyle, nCulture, out dVal);
                val = (int)dVal;
            }
            this.SamplesCount = val;

            return result;
        }

        private bool Process16HeaderString(string str)
        {
            string[] strSplitted = str.Split(Separator);
            int i;
            
            //check: if there is a math output in the file
            for (i = 0; i < strSplitted.Length; i++)
            {
                if (strSplitted[i] == string.Empty)
                    break;
            }

            this.ChannelsCount = i-1;

            return i > 1 ? true : false;
        }

        /// <summary>
        /// Check is file was saved "between cursors". At this case correct the samplesCount
        /// </summary>
        /// <returns>false - if file isunexpected file type</returns>
        private bool CheckFileForSavingBetweenCursorsAndCorrectSamplesCount()
        {
            //going to the voltages data
            ResetInputStreamReader();
            string tmpStr;
            do
            { 
                tmpStr = inputSR.ReadLine();
            }
            while (!tmpStr.StartsWith("TIME"));

            //count lines in the file
            int linesCount = 0;
            while (!string.IsNullOrEmpty(inputSR.ReadLine())) { linesCount++; }

            //check lines count value
            if (linesCount == SamplesCount * FramesCount)
            {//full range was saved
                return true;
            }
            else
            {//check file more closely
                if (linesCount > SamplesCount * FramesCount)
                    return false;

                if (linesCount % FramesCount > 0) //all frames must have the same size
                    return false;

                //set new SamplesCount
                SamplesCount = linesCount / FramesCount;
                return true;
            }
        }
        #endregion

        public override bool FillChannelVoltages()
        {
            ResetInputStreamReader();
            string str;
            string[] strSplitted;

            try
            {
                //rushing towards the voltages data

                //stream position - header
                do
                {
                    str = inputSR.ReadLine();
                } while (!str.StartsWith("TIME"));

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
                            Voltages[k][i][j] = double.Parse(strSplitted[k+1].TrimStart(), nStyle, nCulture);
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

            return inputSR.EndOfStream;
        }
    }
}
