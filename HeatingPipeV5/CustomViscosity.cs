using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeatingPipeV5
{
    public class Viscosity
    {
        public double[,] Value { get; set; }

        // Constructor
        public Viscosity()
        {
            Value = new double[,]
            {
            { 0, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 110, 120, 130, 140, 150, 160, 170, 180, 190 },
            { 1.79, 1.3, 1.0, 0.805, 0.659, 0.556, 0.479, 0.415, 0.366, 0.326, 0.295, 0.268, 0.244, 0.226, 0.212, 0.202, 0.19, 0.181, 0.173, 0.166 }
            };
        }
        public double GetViscosity(double t)
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
