using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TransporterLib;
using Hef.Math;

namespace SimpleCalcSource
{
    class SourceMath
    {
        public Transporter demoTransporter;
        private Interpreter sourceInterpretter;

        public SourceMath(Transporter demoTransporter)
        {
            if (demoTransporter == null)
                throw new NullReferenceException("Transporter is null");
            sourceInterpretter = new Interpreter();

            this.demoTransporter = demoTransporter;
        }

        public void SendArg(object arg)
        {
            try
            {
                demoTransporter.SendObject(arg);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public double CalculateArg(string arg)
        {
            return sourceInterpretter.Calculate(arg);
        }

        public double CalcFuncFab(string funcFab, double argA, double argB)
        {
            sourceInterpretter.SetVar("a", argA);
            sourceInterpretter.SetVar("b", argB);

            return sourceInterpretter.Calculate(funcFab);
        }

    }
}