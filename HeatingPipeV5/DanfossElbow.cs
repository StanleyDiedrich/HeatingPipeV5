using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeatingPipeV5
{
    public class DanfossElbow
    {
        private List<CustomElement> list;

        public string Ids { get; set; }
        public int Count { get; set; }
        public string Mark { get; set; }



        
        public DanfossElbow(List<CustomElement> list)
        {
            this.list = list;
            Count = list.Count;
            foreach (var el in list)
            {
                Ids += $"{el.ElementId};";
            }
            Mark = list.First().BranchMark;
        }
    }
}
