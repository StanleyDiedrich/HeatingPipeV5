using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeatingPipeV5
{
    public  class ModelsName
    {

        
            public string ModelName { get; set; }
            public bool IsSelected { get; set; }

            public ModelsName(string systemname)
            {
                ModelName = systemname;
            }
        
    }
}
