using Autodesk.Revit.DB;

namespace HeatingPipeV5
{
    public  class DanfossRoom
    {
        public string Source { get; set; }
        public string Symbol { get; set; }
        public string Area { get; set; }
        public string TempInt { get; set; }
        public string TempOut { get; set; }
        public string HeatLoad { get; set; }
        public string HeatAuto { get; set; }

        public string Area1P { get; set; }
        public string ColdLoad { get; set; }
        public string RoomTermostate { get; set; }
        public string Description { get; set; }
        public string Comment { get; set; }
        public string Zone { get; set; }

        public DanfossRoom (Element mep_room)
        {
            
           
                Symbol = mep_room.LookupParameter("ADSK_Номер квартиры").AsString();

            try
            {
                Zone = mep_room.LookupParameter("ADSK_Зона").AsString();
                if (!Zone.Equals(""))
                {
                    Symbol = Symbol + " ЗОНА " + Zone;
                }
                else
                {
                    Symbol = Symbol;
                }
            }
            catch
            {

            }
            try
            {
                Area = "";
                TempInt = mep_room.LookupParameter("ADSK_Температура в помещении").AsValueString().Split()[0];
                TempOut = "";
                HeatLoad = mep_room.get_Parameter(BuiltInParameter.ROOM_DESIGN_HEATING_LOAD_PARAM).AsValueString().Split()[0];
                HeatAuto = "";
                Area1P = "";
                ColdLoad = mep_room.get_Parameter(BuiltInParameter.ROOM_DESIGN_COOLING_LOAD_PARAM).AsValueString().Split()[0];
                RoomTermostate = "";
                Description = "";
                Comment = "";
            }
               
            
            catch
            {

            }
                

        }
    }
}