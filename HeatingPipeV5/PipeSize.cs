using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeatingPipeV5
{
    public  class PipeSize
    {
        public double InnerDiameter { get; set; }
        public double OuterDiameter { get; set; }
        public double Diameter { get; set; }

       

        public PipeSize(double diameter, double outerDiameter, double innerDiameter)
        {
            Diameter = diameter;
            OuterDiameter = outerDiameter;
            InnerDiameter = innerDiameter;
        }
        public static PipeSize FindBestMatch(PipeGOST3262Catalog sizes, double targetOuter, double targetInner, double maxTolerance = double.MaxValue)
        {
            if (sizes == null) throw new ArgumentNullException(nameof(sizes));

            PipeSize best = null;
            double bestScore = double.MaxValue;

            foreach (var s in sizes.Catalog)
            {
                // расстояние (можно изменить на взвешенное)
                double score = Math.Abs(s.OuterDiameter - targetOuter) + Math.Abs(s.InnerDiameter - targetInner);

                // альтернатива: use max deviation instead of sum:
                // double score = Math.Max(Math.Abs(s.OuterDiameter - targetOuter), Math.Abs(s.InnerDiameter - targetInner));

                if (score < bestScore)
                {
                    bestScore = score;
                    best = s;
                }
            }

            // если нужно применять предел допусков — проверяем:
           /* if (best != null)
            {
                double outerDiff = Math.Abs(best.OuterDiameter - targetOuter);
                double innerDiff = Math.Abs(best.InnerDiameter - targetInner);
                if (outerDiff > maxTolerance || innerDiff > maxTolerance)
                    return null; // ничего не подошло по допускам
            }*/

            return best;
        }

        public static PipeSize FindBestMatch(PipeGost10704Catalog sizes, double targetOuter, double targetInner, double maxTolerance = double.MaxValue)
        {
            if (sizes == null) throw new ArgumentNullException(nameof(sizes));

            PipeSize best = null;
            double bestScore = double.MaxValue;

            foreach (var s in sizes.Catalog)
            {
                // расстояние (можно изменить на взвешенное)
                double score = Math.Abs(s.OuterDiameter - targetOuter) + Math.Abs(s.InnerDiameter - targetInner);

                // альтернатива: use max deviation instead of sum:
                // double score = Math.Max(Math.Abs(s.OuterDiameter - targetOuter), Math.Abs(s.InnerDiameter - targetInner));

                if (score < bestScore)
                {
                    bestScore = score;
                    best = s;
                }
            }

           /* // если нужно применять предел допусков — проверяем:
            if (best != null)
            {
                double outerDiff = Math.Abs(best.OuterDiameter - targetOuter);
                double innerDiff = Math.Abs(best.InnerDiameter - targetInner);
                if (outerDiff > maxTolerance || innerDiff > maxTolerance)
                    return null; // ничего не подошло по допускам
            }*/

            return best;
        }

        public static PipeSize FindBestMatch(ValtecPexACatalog sizes, double targetOuter, double targetInner, double maxTolerance = double.MaxValue)
        {
            if (sizes == null) throw new ArgumentNullException(nameof(sizes));

            PipeSize best = null;
            double bestScore = double.MaxValue;

            foreach (var s in sizes.Catalog)
            {
                // расстояние (можно изменить на взвешенное)
                double score = Math.Abs(s.OuterDiameter - targetOuter) + Math.Abs(s.InnerDiameter - targetInner);

                // альтернатива: use max deviation instead of sum:
                // double score = Math.Max(Math.Abs(s.OuterDiameter - targetOuter), Math.Abs(s.InnerDiameter - targetInner));

                if (score < bestScore)
                {
                    bestScore = score;
                    best = s;
                }
            }

            // если нужно применять предел допусков — проверяем:
           /* if (best != null)
            {
                double outerDiff = Math.Abs(best.OuterDiameter - targetOuter);
                double innerDiff = Math.Abs(best.InnerDiameter - targetInner);
                if (outerDiff > maxTolerance || innerDiff > maxTolerance)
                    return null; // ничего не подошло по допускам
            }*/

            return best;
        }
    }
}
