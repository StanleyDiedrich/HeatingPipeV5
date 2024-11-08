using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeatingPipeV5
{
    public class CustomDensity
    {
        
            public double[,] Value { get; set; }

            // Constructor
            public CustomDensity()
            {
                Value = new double[,]
                {
            { 0, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 110, 120, 130, 140, 150, 160, 170, 180, 190 },
            { 999.8,999.6,998.2,995.6,992.2,988,983.2,977.7,971.8,965.3,958.3,951,943.1,934.8,926.1,916.9,907.4,897.3,886.9,876 }
                };
            }
            public double GetDensity(double t)
            {
                // Find index for temperature
                for (int i = 0; i < Value.GetLength(1) - 1; i++)
                {
                    // Check if the temperature matches
                    if (t == Value[0, i])
                    {
                        return Value[1, i]; // Return viscosity directly
                    }

                    // Check if the temperature is between two data points
                    if (t > Value[0, i] && t < Value[0, i + 1])
                    {
                        // Perform linear interpolation
                        double t1 = Value[0, i];
                        double t2 = Value[0, i + 1];
                        double v1 = Value[1, i];
                        double v2 = Value[1, i + 1];

                        // Linear interpolation formula
                        double interpolatedViscosity = v1 + (v2 - v1) * ((t - t1) / (t2 - t1));
                        return interpolatedViscosity;
                    }
                }

                // If temperature is outside the known range, return a default value (e.g., 0 or throw an exception)
                throw new ArgumentOutOfRangeException(nameof(t), $"Temperature {t} is outside the range of known viscosity values.");
            }
        
    }
}
