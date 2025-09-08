using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Xml.Linq;
using Autodesk.Revit.DB;

namespace HeatingPipeV5
{
    public class CustomBranch
    {
        public Autodesk.Revit.DB.Document Document { get; set; }
        private static int _counter = 0;
        public int Number { get; set; }
        public int GroupNumber { get; set; }
        public double Pressure { get; set; }
        public double Length { get; set; }
        public string BranchNumber { get; set; }
        public string Direction { get; set; }
        public double RelPressure { get; set; }
        public double LTot { get; set; }
        public double PBTot { get; set; }
        public List<CustomElement> Elements { get; set; } = new List<CustomElement>();
        public CustomBranch(Autodesk.Revit.DB.Document document, ElementId elementId)
        {
            Document = document;
            
           /* Number = _counter;
            _counter++;*/
        }
        public CustomBranch(Autodesk.Revit.DB.Document document)
        {
            Document = document;
           /* Number = _counter;
            _counter++;*/
        }
        public void Add(CustomElement customElement)
        {
            if (customElement != null)
            {
                // Находим индекс узла с таким же ElementId
                var existingNodeIndex = Elements.FindIndex(n => n.ElementId == customElement.ElementId);

                if (existingNodeIndex >= 0)
                {
                    // Если найден, заменяем существующий узел
                    Elements[existingNodeIndex] = customElement;
                }
                else
                {
                    // Если не найден, добавляем новый узел
                    Elements.Add(customElement);
                }
            }
        }

        public void AddSpecial(CustomElement customElement)
        {
            if (customElement!=null)
            {
                var existingNodeIndex = Elements.FindIndex(n => n.ElementId == customElement.ElementId);
                if (existingNodeIndex==0)
                {
                    Elements.Add(customElement);
                }
            }
        }
        public void Remove(CustomElement customElement)
        {
            // Находим индекс узла с указанным ElementId
            var nodeIndex = Elements.FindIndex(n => n.ElementId == customElement.ElementId);

            if (nodeIndex >= 0)
            {
                // Если найден, удаляем узел
                Elements.RemoveAt(nodeIndex);
            }
        }
        public void AddRange(CustomBranch branch)
        {
            if (branch != null)
            {
                foreach (var node in branch.Elements)
                {
                    Add(node); // Использует метод Add, который уже включает логику уникальности
                }
            }
        }

        public void CreateNewBranch(Document document, ElementId airterminal)
        {

            
                Number = _counter;
           
            ElementId nextElement = null;

            CustomElement customElement = new CustomElement(document, airterminal);
            customElement.MepSpace = ((document.GetElement(airterminal) as FamilyInstance).Space as SpatialElement).Number;
            BranchNumber = customElement.MepSpace;
            customElement.Direction = "П";
            customElement.BranchMark = BranchNumber +"_"+ customElement.Direction + "_" + Number.ToString();




            Elements.Add(customElement);
            var nextsupplyelement = customElement.SupplyConnector.NextOwnerId;
            var nextreturnelement = customElement.ReturnConnector.NextOwnerId;
           
            CustomElement customElementSup = new CustomElement(document, nextsupplyelement);
            customElementSup.MepSpace = ((document.GetElement(airterminal) as FamilyInstance).Space as SpatialElement).Number;
            BranchNumber = customElementSup.MepSpace;
            customElementSup.Direction = "П";
            customElementSup.BranchMark = BranchNumber +"_"+ customElementSup.Direction + "_" + Number.ToString();

            do
            {
                
                Elements.Add(customElementSup);
                nextElement = customElementSup.NextElementId;
                if (customElementSup.OwnConnectors.Size>3)
                {
                    nextElement = customElementSup.SupplyConnector.NextOwnerId;
                }
                customElementSup = new CustomElement(document, nextElement);
                customElementSup.IsSupply = true;
                customElementSup.Direction = "П";
                customElementSup.BranchMark = BranchNumber+"_" +customElementSup.Direction+"_"+Number.ToString();
            }
            while (nextElement != null);



            
            CustomElement customElementRet = new CustomElement(document, nextreturnelement);
            if (customElementRet.ElementId.IntegerValue == 4080858)
            {
                var airt = airterminal;
            }
            //customElementRet.MepSpace = ((document.GetElement(airterminal) as FamilyInstance).Space as SpatialElement).Number;
            BranchNumber = customElementSup.MepSpace;
            customElementRet.Direction = "О";
            customElementRet.BranchMark = customElement.MepSpace +"_"+ customElementRet.Direction + "_" + Number.ToString();
            do
            {
                if (customElementRet.ElementId.IntegerValue == 4080864)
                {
                    var airt = airterminal;
                }
                

                Elements.Add(customElementRet);

               
                nextElement = customElementRet.NextElementId;
                if (customElementRet.OwnConnectors.Size>3)
                {
                    nextElement = customElementRet.ReturnConnector.NextOwnerId;
                }
                customElementRet = new CustomElement(document, nextElement);
                customElementRet.IsSupply = false;
                customElementRet.Direction = "О";
                customElementRet.BranchMark = customElement.MepSpace + "_" + customElementRet.Direction + "_" + Number.ToString();
            }
            while (nextElement != null);
            BranchNumber = Number. ToString();
            _counter++;
        }

        public List<ElementId> ShowElements ()
        {
            List<ElementId> result = new List<ElementId>();
            foreach (var el in Elements)
            {
               
                    result.Add(el.ElementId);
                
            }
            return result;
        }




    }
}
