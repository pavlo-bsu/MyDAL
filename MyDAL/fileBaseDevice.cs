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
    }
}
