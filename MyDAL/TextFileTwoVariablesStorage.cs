using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pavlo.MyDAL
{
    /// <summary>
    /// represent a file with two variables values. First line is variable descriptions
    /// </summary>
    public class TextFileTwoVariablesStorage
    {
        private string _FileName;
        public string FileName
        {
            get => _FileName;
            protected set
            {
                _FileName = value;
            }
        }

        protected bool _IsStorageDamaged = false;
        /// <summary>
        /// true - if the storage is damaged, not available, etc.
        /// </summary>
        public bool IsStorageDamaged
        {
            get => _IsStorageDamaged;
            protected set
            {
                _IsStorageDamaged = value;
            }
        }

        //first variable description
        public string FirstVariableDescription
        { get; protected set; }

        //array with first variable values
        public double[] FirstVariableArray
        { get; protected set; }

        //second variable description
        public string SecondVariableDescription
        { get; protected set; }
        
        //array with second variable values
        public double[] SecondVariableArray
        { get; protected set; }

        /// <summary>
        /// EOL symbol
        /// </summary>
        public string EndOfLine
        {
            get; set;
        }

        /// <summary>
        /// separator symbol
        /// </summary>
        public char SeparatorSymbol
        {
            get; set;
        }

        protected readonly System.Globalization.NumberStyles nStyle = System.Globalization.NumberStyles.AllowExponent | System.Globalization.NumberStyles.AllowLeadingSign | System.Globalization.NumberStyles.AllowDecimalPoint;
        protected readonly System.Globalization.CultureInfo nCulture = System.Globalization.CultureInfo.InvariantCulture;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="FileName">file path</param>
        public TextFileTwoVariablesStorage(string fileName)
        {
            FileName = fileName;

            //set default values
            EndOfLine = "\r\n";
            SeparatorSymbol = ',';
        }

        /// <summary>
        /// Load data from storage to the arrays
        /// </summary>
        public virtual void Load ()
        {
            string tmp;
            string[] strSplitted;
            try
            {
                using (System.IO.FileStream fs = new System.IO.FileStream(this.FileName, System.IO.FileMode.Open))
                {
                    using (System.IO.StreamReader sr = new System.IO.StreamReader(fs, Encoding.UTF8))
                    {
                        //read header. Only one line!
                        tmp = sr.ReadLine();
                        strSplitted = tmp.Split(SeparatorSymbol);
                        //must be 2 values
                        if (strSplitted.Length != 2)
                            throw new IndexOutOfRangeException();
                        FirstVariableDescription = strSplitted[0];
                        SecondVariableDescription = strSplitted[1];

                        //reading the values
                        List<double> firstVariables = new List<double>();
                        List<double> secondVariables = new List<double>();

                        do
                        {
                            tmp = sr.ReadLine();
                            if (tmp == string.Empty)
                                continue;//it can be an extra line in the end of the file
                            strSplitted = tmp.Split(SeparatorSymbol);
                            //must be 2 values
                            if (strSplitted.Length != 2)
                                throw new IndexOutOfRangeException();

                            firstVariables.Add(double.Parse(strSplitted[0].TrimStart(), nStyle, nCulture));
                            secondVariables.Add(double.Parse(strSplitted[1].TrimStart(), nStyle, nCulture));
                        } while (!sr.EndOfStream);

                        FirstVariableArray = firstVariables.ToArray();
                        SecondVariableArray = secondVariables.ToArray();
                    }
                }
            }
            catch
            {
                this.IsStorageDamaged = true;
                throw;
            }
        }
    }
}
