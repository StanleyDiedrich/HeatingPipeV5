using HeatingPipeV5;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeatingPipeV5
{
    public class PipeGOST3262Catalog
    {
        public List<PipeSize> Catalog { get; set; } = new List<PipeSize>();

        public PipeGOST3262Catalog()
        {
            Catalog.AddRange(
               new List<PipeSize>
                { 
                   new PipeSize (15, 21.3, 15.7),
                 new PipeSize(20, 26.8, 21.2 ),
                 new PipeSize(25, 33.5, 27.1), 
                  new PipeSize(32, 42.3, 35.9 ),
                     new PipeSize(40, 48, 41 ),
                new PipeSize(50, 60, 53 )});

        }
              
    }
    }

