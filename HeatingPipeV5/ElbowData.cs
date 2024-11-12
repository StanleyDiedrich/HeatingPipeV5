namespace HeatingPipeV5
{
    public class ElbowData
    {
        public double[,] Value { get; set; }
        public double LocRes { get; set; }
        public ElbowData()
        {
            Value = new double[,] {
                { 10,15,16,20,25,32,40,50},
                { 2,1.5,1.5,1.5,1,1,0.5,0.5}
            };


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

            // If no exact match is found, we should perform linear interpolation
           /* for (int i = 0; i < numRows - 1; i++)
            {
                double diameter1 = Value[0, i];
                double diameter2 = Value[0, i + 1];
                double res1 = Value[1, i];
                double res2 = Value[1, i + 1];

                // Check if the diameter falls within the current interval
                if (diameter >= diameter1 && diameter <= diameter2)
                {
                    // Perform linear interpolation
                    LocRes = res1 + (res2 - res1) * (diameter - diameter1) / (diameter2 - diameter1);
                    
                }
            }*/
            return LocRes;
        }
    }
}