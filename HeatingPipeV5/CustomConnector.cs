using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;

namespace HeatingPipeV5
{
    public class CustomConnector
    {
        public ElementId OwnerId { get; set; }
        public Connector Connector { get; set; }
        public Domain Domain { get; set; }
        public FlowDirectionType DirectionType { get; set; }
        public ConnectorType ConnectorType { get; set; }
        public ElementId NextOwnerId { get; set; }
        public ElementId Neighbourg { get; set; }
        public double Flow { get; set; }
        public double Coefficient { get; set; }
        public bool IsSelected { get; set; }

        public double Diameter { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double Velocity { get; set; }
        public ConnectorProfileType Shape { get; set; }
        public ConnectorType Type { get; set; }

        public double EquiDiameter { get; set; }


        public double PressureDrop { get; set; }
        public double AInlet { get; set; }
        public double AOutlet { get; set; }
        public double Area { get; set; }
        public bool IsMainConnector { get; set; }
        public XYZ Origin { get; set; }
        public XYZ Vector { get; set; }
        public bool IsStraight { get; set; }
        List<CustomConnector> Connectors { get; set; } = new List<CustomConnector>();

        public CustomConnector(Autodesk.Revit.DB.Document document, ElementId elementId, Autodesk.Revit.DB.Plumbing.PipeSystemType pipeSystemType)
        {
            OwnerId = elementId;



        }

    }
}
