using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Pavlo.MyDAL
{
    public abstract class FileBaseDevice
    {
        /// <summary>
        /// stream with input file
        /// </summary>
        protected readonly StreamReader inputSR;

        private char _separator;
        protected char Separator
        {
            get { return _separator; }
            set { _separator = value; }
        }

        private double _t0;
        public double t0
        {
            get { return _t0; }
            protected set { _t0 = value; }
        }

        private double _dt;
        public double dt
        {
            get { return _dt; }
            protected set { _dt = value; }
        }

        private double[] _times;
        public double[] Times
        {
            get { return _times; }
            protected set { _times = value; }
        }

        private int _channelsCount;
        public int ChannelsCount
        {
            get { return _channelsCount; }
            protected set { _channelsCount = value; }
        }

        private int _framesCount;
        public int FramesCount
        {
            get { return _framesCount; }
            protected set { _framesCount = value; }
        }

        private int _samplesCount;
        public int SamplesCount
        {
            get {return _samplesCount; }
            protected set
            { _samplesCount = value; }
        }

        /// <summary>
        /// first index - channel
        /// second index - frame
        /// third index - sample
        /// </summary>
        private double[][][] _voltages;
        public double[][][] Voltages
        {
            get { return _voltages; }
            protected set { _voltages = value; }
        }

        /// <summary>
        /// has inputfile infinity value (no difference + or -)
        /// </summary>
        private bool _infValueDetected = false;

        public bool InfValueDetected
        {
            get { return _infValueDetected; }
            protected set { _infValueDetected = value; }
        }

        protected virtual void FillTimesTable()
        {
            Times = new double[SamplesCount];
            for (int i = 0; i < SamplesCount; i++)
                Times[i] = t0 + dt * i;
        }

        public FileBaseDevice(StreamReader inStr)
        {
            this.inputSR = inStr;
        }

        public abstract bool FillChannelVoltages();

        public abstract bool ProcessFileHeader();
                
        /// <summary>
        /// set input stream to start position
        /// </summary>
        public virtual void ResetInputStreamReader()
        {
            inputSR.BaseStream.Position = 0;
            inputSR.DiscardBufferedData();
        }

        /// <summary>
        /// Emulate the current file as a csv-file recorded by Tektronix 7000 Series
        /// </summary>
        /// <param name="outSW">output stream for a file to be written</param>
        /// <param name="channel">channel of the current file to be written</param>
        public virtual void EmulateTek70000CSVfile(StreamWriter outSW, int channel)
        {
            //SamplesCount should be large enough
            if (SamplesCount < 10)
                throw new ArgumentOutOfRangeException("SamplesCount");

            string mainvaluesPreffix = ",,,";

            //'.' is number decimal separator for all double values in the emulated file
            System.Globalization.NumberFormatInfo nfi = new System.Globalization.NumberFormatInfo();
            nfi.NumberDecimalSeparator = ".";

            //format for double values
            string dFormat = "e08";

            //preparing header of the file
            string firstHStr = @"""Record Length"",{0},""Points"",";
            firstHStr = string.Format(firstHStr, SamplesCount);
            string secondHStr = @"""Sample Interval"",{0:e08},s,";
            secondHStr = string.Format(nfi, secondHStr, dt);
            string thirdHStr = @"""Trigger Point"",{0},""Samples"",";
            //in general we don't know the trigger settings
            thirdHStr = string.Format(nfi, thirdHStr, "");
            //in general we don't know the trigger settings
            string fourthHStr = @"""Trigger Time"",{0},s,";
            fourthHStr = string.Format(nfi, fourthHStr, "");
            string fifthHStr = @""""",,,";
            string sixthHStr = @"""Horizontal Offset"",{0:e08},s,";
            sixthHStr = string.Format(nfi, sixthHStr, Times[0]);
            string seventhHStr = @"""FastFrame Count"",{0},""Frames"",";
            seventhHStr = String.Format(seventhHStr, FramesCount);

            string[] fileHeader = { firstHStr,secondHStr,thirdHStr,fourthHStr,fifthHStr,sixthHStr,seventhHStr};

            //WRITE THE DATA

                //header with data of first frame
            StringBuilder tmp= new StringBuilder(100);
            for (int i = 0; i < fileHeader.Length; i++)
            {
                tmp.Clear();
                tmp.Append(fileHeader[i]);
                tmp.Append(Times[i].ToString(dFormat, nfi));
                tmp.Append(',');
                tmp.Append(Voltages[channel][0][i].ToString(dFormat, nfi));
                outSW.WriteLine(tmp.ToString());
            }

                //all remain data of 1st frame
            for (int i = fileHeader.Length; i < SamplesCount; i++)
            {
                tmp.Clear();
                tmp.Append(mainvaluesPreffix);
                tmp.Append(Times[i].ToString(dFormat, nfi));
                tmp.Append(',');
                tmp.Append(Voltages[channel][0][i].ToString(dFormat, nfi));
                outSW.WriteLine(tmp.ToString());
            }

                //all other frames
            for (int j = 1; j < FramesCount; j++)
            {
                for (int i = 0; i < SamplesCount; i++)
                {
                    tmp.Clear();
                    tmp.Append(mainvaluesPreffix);
                    tmp.Append(Times[i].ToString(dFormat, nfi));
                    tmp.Append(',');
                    tmp.Append(Voltages[channel][j][i].ToString(dFormat, nfi));
                    outSW.WriteLine(tmp.ToString());
                }
            }
        }
    }
}
