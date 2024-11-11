using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using Autodesk.Revit.DB.Plumbing;

namespace HeatingPipeV5
{
    public class TeeData
    {
        public double[,] Value { get; set; }
        public double LocRes { get; set; }
        public bool IsStraight { get; set; }
        public PipeSystemType SystemType { get; set; }
       
        public TeeData(PipeSystemType pipeSystemType, bool isStraight, double diameter )
        {
            SystemType = pipeSystemType;
            if (SystemType == PipeSystemType.SupplyHydronic && IsStraight == true)
            {
                Value = new double[,]
                {
                    {10,15,16,20,25,32,40,50,65,80,90,100,125,125 },
                    { 1,1,1,1,1,1,1,1,1,1,1,1,1,1}
                };
               
            }
            else if (SystemType == PipeSystemType.SupplyHydronic && IsStraight == false)
            {
                Value = new double[,]
                {
                    {10,15,16,20,25,32,40,50,65,80,90,100,125,125 },
                    { 1.5,1.5,1.5,1.5,1.5,1.5,1.5,1.5,1.5,1.5,1.5,1.5,1.5,1.5}
                };
                
            }
            else if (SystemType==PipeSystemType.ReturnHydronic && IsStraight == true)
            {
                Value = new double[,]
                {
                    {10,15,16,20,25,32,40,50,65,80,90,100,125,125 },
                    { 1,1,1,1,1,1,1,1,1,1,1,1,1,1}
                };
                
            }
            else if (SystemType == PipeSystemType.ReturnHydronic && IsStraight == false)
            {
                Value = new double[,]
                {
                    {10,15,16,20,25,32,40,50,65,80,90,100,125,125 },
                    { 3,3,3,3,3,3,3,3,3,3,3,3,3,3}
                };
               
            }

           
        }
        public double GetLocRes(double diameter)
        {
            // Get the number of entries
            int numRows = Value.GetLength(1);

            // Check for exact match
            for (int i = 0; i < numRows; i++)
            {
                if (Value[0, i] == diameter)
                {
                    LocRes = Value[1, i];

                }
            }


            return LocRes;
        }
    }
}
