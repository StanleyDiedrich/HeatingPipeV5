using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeatingPipeV5
{
    public class CustomLoop
    {
        public List<CustomBranch> Collection { get; set; }
        public int LoopNumber { get; set; }
        public CustomBranch SupplyBranch { get; set; }
        public CustomBranch ReturnBranch { get; set; }


        public CustomLoop (CustomCollection collection)
        {
            Collection = collection.Collection;



        }
    }
}
