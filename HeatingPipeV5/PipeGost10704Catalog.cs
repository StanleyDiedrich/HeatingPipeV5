using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeatingPipeV5
{
    public class PipeGost10704Catalog
    {
        public List<PipeSize> Catalog { get; set; } = new List<PipeSize>();

        public PipeGost10704Catalog()
        {
            Catalog.AddRange(
               new List<PipeSize>
                {
                   new PipeSize (50,57,50),
                 new PipeSize(65,76,69 ),
                 new PipeSize(80,89,82),
                  new PipeSize(100,108,100),
                     new PipeSize(125,133,125 ),
                new PipeSize(150,159,150 ),
                new PipeSize(200, 219, 200),
                new PipeSize(250,273,250),
                new PipeSize(300,325,300),
                new PipeSize(350,377,350),
                new PipeSize(400,426,400)
               }
               );


        }
    }
}
