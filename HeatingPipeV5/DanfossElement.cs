using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeatingPipeV5
{
    public class DanfossPipe
    {
        private IGrouping<int, CustomElement> group;

        public string HeatingSystem { get; set; }
        public string Type { get; set; }
        public string PipeType { get; set; }
        public string Riser { get; set; }
        public string Part { get; set; }
        public string DiameterNominal { get; set; }//
        public string Insulation { get; set; }
        public string InsulationThick { get; set; }
        public string PLc { get; set; }
        public string Length { get; set; } // считаем
        public string Ost { get; set; }
        public string Lvl { get; set; }
        public string Room { get; set; }
        public string Otv { get; set; } 
        public string Od { get; set; }
        public string Sos { get; set; }
        public string Comment { get; set; } //  указываем
        public string Symbol { get; set; } 
        public string Manufacturer { get; set; }
        public string Description { get; set; }
        public string Onach { get; set; }
        public string Okonech { get; set; }
        public string Ids { get; set; }

       /* public DanfossPipe(List<CustomElement> pipes )
        {

            DiameterNominal = pipes.First().DiameterNominal;
            double len = 0;
            foreach (var pipe in pipes)
            {
                len += pipe.Lenght;
            }
            Length = Convert.ToString(len);
            Comment = $"{pipes.First().BranchMark}" + "-" + $"{pipes.First().TrackNumber}";

                
                     
             
        }*/


        public DanfossPipe(IEnumerable<CustomElement> pipes)
        {
            var list = pipes.ToList();
            foreach (var el in pipes)
            {
                Ids += $"{el.ElementId}"+";";
            }
            DiameterNominal = list.First().DiameterNominal;
            double len = list.Sum(p => p.Lenght);
            Length = len.ToString();
            Comment = $"{list.First().BranchMark}-{list.First().TrackNumber}";
        }

    }
}

