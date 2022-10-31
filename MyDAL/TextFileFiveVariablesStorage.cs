using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pavlo.MyDAL
{
    public class TextFileFiveVariablesStorage : TextFileTwoVariablesStorage
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="FileName">file path</param>
        public TextFileFiveVariablesStorage(string fileName) : base (fileName)
        {
        }

        //third variable description
        public string ThirdVariableDescription
        { get; protected set; }

        //array with third variable values
        public double[] ThirdVariableArray
        { get; protected set; }

        //fourth variable description
        public string FourthVariableDescription
        { get; protected set; }

        //array with fourth variable values
        public double[] FourthVariableArray
        { get; protected set; }

        //fifth variable description
        public string FifthVariableDescription
        { get; protected set; }

        //array with fifth variable values
        public double[] FifthVariableArray
        { get; protected set; }

        public override void Load()
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
                        //must be 5 values
                        if (strSplitted.Length != 5)
                            throw new IndexOutOfRangeException();
                        FirstVariableDescription = strSplitted[0];
                        SecondVariableDescription = strSplitted[1];
                        ThirdVariableDescription = strSplitted[2];
                        FourthVariableDescription = strSplitted[3];
                        FifthVariableDescription = strSplitted[4];

                        //reading the values
                        List<double> firstVariables = new List<double>();
                        List<double> secondVariables = new List<double>();
                        List<double> thirdVariables = new List<double>();
                        List<double> fourthVariables = new List<double>();
                        List<double> fifthVariables = new List<double>();

                        do
                        {
                            tmp = sr.ReadLine();
                            if (tmp == string.Empty)
                                continue;//it can be an extra line in the end of the file
                            strSplitted = tmp.Split(SeparatorSymbol);
                            //must be 5 values
                            if (strSplitted.Length != 5)
                                throw new IndexOutOfRangeException();

                            firstVariables.Add(double.Parse(strSplitted[0].TrimStart(), nStyle, nCulture));
                            secondVariables.Add(double.Parse(strSplitted[1].TrimStart(), nStyle, nCulture));
                            thirdVariables.Add(double.Parse(strSplitted[2].TrimStart(), nStyle, nCulture));
                            fourthVariables.Add(double.Parse(strSplitted[3].TrimStart(), nStyle, nCulture));
                            fifthVariables.Add(double.Parse(strSplitted[4].TrimStart(), nStyle, nCulture));
                        } while (!sr.EndOfStream);

                        FirstVariableArray = firstVariables.ToArray();
                        SecondVariableArray = secondVariables.ToArray();
                        ThirdVariableArray = thirdVariables.ToArray();
                        FourthVariableArray = fourthVariables.ToArray();
                        FifthVariableArray = fifthVariables.ToArray();
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
