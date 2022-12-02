using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Pavlo.MyDAL
{
    /// <summary>
    /// class represents a Waveform (several segments) saved by Lecroy WaveSurfer 400 series, WaveMaster 8 and etc. 
    /// standard oscill. output to file is expected
    /// </summary>
    public class FileLecroyWave : FileBaseDevice
    {

        private readonly System.Globalization.NumberStyles nStyle = System.Globalization.NumberStyles.AllowExponent | System.Globalization.NumberStyles.AllowLeadingSign | System.Globalization.NumberStyles.AllowDecimalPoint;
        private readonly System.Globalization.CultureInfo nCulture = System.Globalization.CultureInfo.InvariantCulture;
        private readonly int roughHeaderStringsCount;

        public FileLecroyWave(StreamReader inStr)
            : base(inStr)
        {
            roughHeaderStringsCount = 3;
            this.ChannelsCount = 1; //
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

            bool res = SearchForTimesData();
            if (res == false)
                return false;

            FillTimesTable();

            return true;
        }

        private bool ProcessHeaderString(string str)
        {
            if (str == null)
                return false;
            if (str.StartsWith("LECROY"))
            {
                return ProcessFirstHeaderString(str);
            }
            if (str.StartsWith("Segments"))
            {
                return Process2HeaderString(str);
            }
            if (str.StartsWith("Segment")&& (!str.StartsWith("Segments")))
            {
                return Process3HeaderString(str);
            }
            return false;
        }

        private bool Process2HeaderString(string str)
        {
            //get FramesCount
            string[] strSplitted = str.Split(Separator);
            int val = 0;
            bool result;
            result = int.TryParse(strSplitted[1], nStyle, nCulture, out val);
            this.FramesCount = val;
            
            //get SamplesCount
            bool result2;
            result2 = strSplitted[2].StartsWith("SegmentSize");
            bool result3;
            int segmentSize;
            result3 = int.TryParse(strSplitted[3], nStyle, nCulture, out segmentSize);
            this.SamplesCount = segmentSize;
            return result & result2 & result3;
        }

        private bool ProcessFirstHeaderString(string str)
        {
            if (!str.EndsWith("Waveform"))
                return false;

            this.Separator = str[str.Length - 1 - 8 /*Waveform=8chars*/ ];
            
            return true;
        }

        private bool Process3HeaderString(string str)
        {
            //nothing to extract or check
            return true;
        }

        /// <summary>
        /// Searching for data of the first segment to get dt and t0
        /// </summary>
        /// <returns>false - if file is unexpected file type</returns>
        private bool SearchForTimesData()
        {
            try
            {
                //assume that the stream is on triger data for each frame
                while (inputSR.ReadLine().StartsWith("#")) ;

                string str1 = inputSR.ReadLine();
                string str2 = inputSR.ReadLine();

                string[] strSplitted1 = str1.Split(Separator);
                string[] strSplitted2 = str2.Split(Separator);

                double time0, time1;

                double.TryParse(strSplitted1[0], nStyle, nCulture, out time0);
                double.TryParse(strSplitted2[0], nStyle, nCulture, out time1);


                this.t0 = time0;
                this.dt = time1 - time0;
            }
            catch
            {
                return false;
            }

            return true;
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
                    str=inputSR.ReadLine();
                } while(!str.StartsWith("#"));

                //stream position - triger data for each frame
                do
                {
                    str = inputSR.ReadLine();
                } while(str.StartsWith("#"));


                int linesCount = this.FramesCount * this.SamplesCount;
                this.Voltages = new double[this.ChannelsCount][][];
                Voltages[0] = new double[this.FramesCount][];//ChannelsCount=1!
                for (int i = 0; i < this.FramesCount; i++)
                {
                    Voltages[0][i] = new double[this.SamplesCount];
                    for (int j = 0; j < this.SamplesCount; j++)
                    {
                        strSplitted = inputSR.ReadLine().Split(this.Separator);
                        Voltages[0][i][j] = double.Parse(strSplitted[1].TrimStart(), nStyle, nCulture);
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
