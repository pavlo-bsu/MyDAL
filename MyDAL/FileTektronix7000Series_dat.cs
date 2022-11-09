using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Pavlo.MyDAL
{
    /// <summary>
    /// represents *.dat files recordered by Tektronix 7000 Series
    /// </summary>
    public class FileTektronix7000Series_dat:FileBaseDevice
    {

        private readonly System.Globalization.NumberStyles nStyle = System.Globalization.NumberStyles.AllowExponent | System.Globalization.NumberStyles.AllowLeadingSign | System.Globalization.NumberStyles.AllowDecimalPoint;
        private readonly System.Globalization.CultureInfo nCulture = System.Globalization.CultureInfo.InvariantCulture;
        private readonly int roughHeaderStringsCount;

        /// <summary>
        /// stream with input HEADER file
        /// </summary>
        protected readonly StreamReader inputHeaderSR;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inStr"></param>
        /// <param name="inStrHdr"></param>
        public FileTektronix7000Series_dat(StreamReader inStr, StreamReader inStrHdr)
            : base(inStr)
        {
            this.inputHeaderSR = inStrHdr;

            roughHeaderStringsCount = 6;
            this.ChannelsCount = 1; //FileTektronix7000Series makes a separated file for each channel
            this.FramesCount = 1; //start value for FramesCount. Then will be a search in the file header
        }

        #region header
        public override bool ProcessFileHeader()
        {
            for (int i = 0; i < roughHeaderStringsCount; i++)
            {
                if (ProcessHeaderString(i, inputHeaderSR.ReadLine()) == false)
                    return false;
            }

            var res = CheckFileForSavingBetweenCursorsAndCorrectSamplesCount();
            if (res == false)
                return false;

            FillTimesTable();

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="i">string number(starts from 0!)</param>
        /// <param name="str">string itself</param>
        /// <returns>false - wrong file</returns>
        private bool ProcessHeaderString(int i, string str)
        {
            if (str == null)
                return false;
            switch (i)
            {
                case 0: 
                    return ProcessFirstHeaderString(str);
                    break;
                case 1:
                    return Process2HeaderString(str);
                    break ;
                case 2:
                    return Process3HeaderString(str);
                    break;
                case 3:
                    return Process4HeaderString(str);
                    break;
                case 4:
                    return Process5HeaderString(str);
                    break;
                case 5:
                    return Process6HeaderString(str);
                    break;
                default:
                    //header file should consist of 5-6 strings
                    return false;

            }
        }
        private bool Process6HeaderString(string str)
        {
            int val = 0;
            bool result;
            result = int.TryParse(str, out val);
            this.FramesCount = val;
            return result;
        }

        private bool Process5HeaderString(string str)
        {
            double val = 0;
            bool result;
            result = double.TryParse(str, nStyle, nCulture, out val);
            this.t0 = val;
            return result;
        }

        private bool Process4HeaderString(string str)
        {
            //it should be a trigger data
            //just check the string contains double value
            double val = 0;
            bool result;
            result = double.TryParse(str, nStyle, nCulture, out val);
            return result;
        }

        private bool Process3HeaderString(string str)
        {
            //it should be a trigger data
            //just check the string contains int value
            int val = 0;
            bool result;
            result = int.TryParse(str, nStyle, nCulture, out val);
            return result;
        }

        private bool Process2HeaderString(string str)
        {
            double val = 0;
            bool result;
            result = double.TryParse(str, nStyle, nCulture, out val);
            this.dt = val;
            return result;
        }

        private bool ProcessFirstHeaderString(string str)
        {
            int val=0;
            bool result;
            result = int.TryParse(str, out val);
            this.SamplesCount = val;
            return result;
        }

        /// <summary>
        /// Check is file was saved "between cursors". At this case correct the samplesCount
        /// </summary>
        /// <returns>false - if file isunexpected file type</returns>
        private bool CheckFileForSavingBetweenCursorsAndCorrectSamplesCount()
        {
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
            string str;

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
                        str = inputSR.ReadLine();
                        Voltages[0][i][j] = double.Parse(str.TrimStart(),nStyle,nCulture);
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
