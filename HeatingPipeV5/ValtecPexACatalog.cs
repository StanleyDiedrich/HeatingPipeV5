using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeatingPipeV5
{
    public class ValtecPexACatalog
    {
        public List<PipeSize> Catalog { get; set; } = new List<PipeSize>();

        public ValtecPexACatalog()
        {
            Catalog.AddRange(
               new List<PipeSize>
                {
                   new PipeSize (16,16,11.6),
                 new PipeSize(20,20,14.4 ),
                 new PipeSize(25,25,18),
                  new PipeSize(32,32,23.2),
                    
               }
               );


        }
    }
}
