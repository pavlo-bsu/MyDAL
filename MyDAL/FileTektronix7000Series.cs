using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Pavlo.MyDAL
{
    public class FileTektronix7000Series:FileBaseDevice
    {

        private readonly System.Globalization.NumberStyles nStyle = System.Globalization.NumberStyles.AllowExponent | System.Globalization.NumberStyles.AllowLeadingSign | System.Globalization.NumberStyles.AllowDecimalPoint;
        private readonly System.Globalization.CultureInfo nCulture = System.Globalization.CultureInfo.InvariantCulture;
        private readonly int roughHeaderStringsCount;

        public FileTektronix7000Series(StreamReader inStr)
            : base(inStr)
        {
            roughHeaderStringsCount = 7;
            this.ChannelsCount = 1; //FileTektronix7000Series makes a separated file for each channel
            this.FramesCount = 1; //start value for FramesCount. Then will be a search in the file header
        }

        #region header
        public override bool ProcessFileHeader()
        {
            for (int i = 0; i < roughHeaderStringsCount; i++)
            {
                if (ProcessHeaderString(inputSR.ReadLine()) == false)
                    return false;
            }

            var res = CheckFileForSavingBetweenCursorsAndCorrectSamplesCount();
            if (res == false)
                return false;

            FillTimesTable();

            return true;
        }

        private bool ProcessHeaderString(string str)
        {
            if (str == null)
                return false;
            if (str.StartsWith("\"Record Length\""))
            {
                return ProcessFirstHeaderString(str);
            }
            if (str.StartsWith("\"Sample Interval\""))
            {
                return Process2HeaderString(str);
            }
            if (str.StartsWith("\"Horizontal Offset\""))
            {
                return Process6HeaderString(str);
            }
            if (str.StartsWith("\"FastFrame Count\""))
            {
                return Process7HeaderString(str);
            }
            if (str.StartsWith("\""))
            {
                return true;
            }
            if (str.StartsWith(this.Separator.ToString()))
            {
                return true;
            }
            return false;
        }

        private bool Process7HeaderString(string str)
        {
            string[] strSplitted = str.Split(Separator);
            int val = 0;
            bool result;
            result = int.TryParse(strSplitted[1], out val);
            this.FramesCount = val;
            return result;
        }

        private bool Process6HeaderString(string str)
        {
            string[] strSplitted = str.Split(Separator);
            double val = 0;
            bool result;
            result = double.TryParse(strSplitted[1], nStyle, nCulture, out val);
            this.t0 = val;
            return result;
        }

        private bool Process2HeaderString(string str)
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
            if (str.Length < 15+8+2)
                return false;
            char separator = str[15];
            if (separator == ',' || separator == '\t')
                this.Separator = separator;
            else return false;

            string[] strSplitted = str.Split(Separator);
            int val=0;
            bool result;
            result = int.TryParse(strSplitted[1], out val);
            this.SamplesCount = val;
            return result;
        }

        /// <summary>
        /// Check is file was saved "between cursors". At this case correct the samplesCount
        /// </summary>
        /// <returns>false - if file isunexpected file type</returns>
        private bool CheckFileForSavingBetweenCursorsAndCorrectSamplesCount()
        {
            ResetInputStreamReader();

            //count lines in the file
            int linesCount = 0;
            while (inputSR.ReadLine() != null) { linesCount++; }

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
            string[] strSplitted;

            try
            {
                int linesCount = this.FramesCount * this.SamplesCount;
                this.Voltages = new double[this.ChannelsCount][][];
                Voltages[0]=new double[this.FramesCount][];//ChannelsCount=1!
                for (int i = 0; i < this.FramesCount; i++)
                {
                    Voltages[0][i] = new double[this.SamplesCount];
                    for (int j = 0; j < this.SamplesCount; j++)
                    {
                        strSplitted = inputSR.ReadLine().Split(this.Separator);
                        Voltages[0][i][j] = double.Parse(strSplitted[4].TrimStart(),nStyle,nCulture);
                    }
                }
            }
            catch
            {
                return false;
            }
            return inputSR.EndOfStream;
        }
    }
}
