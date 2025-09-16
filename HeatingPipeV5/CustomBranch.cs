using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Xml.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace HeatingPipeV5
{
    public class CustomBranch
    {
        public Autodesk.Revit.DB.Document Document { get; set; }
        private static int _counter = 0;
        public int Number { get; set; }
        public int MiniLoopNumber { get; set; }
        int LoopNumber { get; set; }
        public int GroupNumber { get; set; }
        public double Pressure { get; set; }
        public double Length { get; set; }
        public string BranchNumber { get; set; }
        public string BranchMark { get; set; }
        //public string Direction { get; set; }
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
        public void CreateNewSupplyBranch(Document document, ElementId airterminal )
        {

            

            Number = _counter;
            MiniLoopNumber = _counter;
            Direction= "П";
            ElementId nextElement = null;

            CustomElement customElement = new CustomElement(document, airterminal);
            //customElement.MepSpace = ((document.GetElement(airterminal) as FamilyInstance).Space as SpatialElement).Number;
            customElement.MepSpace = (document.GetElement(airterminal) as FamilyInstance).Space.LookupParameter("ADSK_Номер квартиры").AsValueString();
            //
            BranchNumber = customElement.MepSpace;
            customElement.Direction = "П";
            //customElement.BranchMark = BranchNumber + "_" + customElement.Direction + "_" + Number.ToString();
            customElement.BranchMark = customElement.MepSpace + "_" + customElement.Direction + "_" + Number.ToString();
            customElement.MiniLoopNumber = MiniLoopNumber;

            Elements.Add(customElement);
            var nextsupplyelement = customElement.SupplyConnector.NextOwnerId;
            

            CustomElement customElementSup = new CustomElement(document, nextsupplyelement);
            try
            {
                customElementSup.MepSpace = ((document.GetElement(airterminal) as FamilyInstance).Space as SpatialElement).Number;
                BranchNumber = customElementSup.MepSpace;
            }
             catch (Exception ex)
            {
                string message = $"{airterminal} не имеет привязки к пространству. Проверьте наличие расчетной точки прибора";
                TaskDialog.Show("Нет привязки к пространстру", message);
            }
           
            customElementSup.Direction = "П";
            customElementSup.BranchMark = customElement.MepSpace + "_" + customElementSup.Direction + "_" + Number.ToString();
            customElementSup.MiniLoopNumber = MiniLoopNumber;
            do
            {

                Elements.Add(customElementSup);
                nextElement = customElementSup.NextElementId;
                if (customElementSup.OwnConnectors.Size > 3)
                {
                    nextElement = customElementSup.SupplyConnector.NextOwnerId;
                }
                customElementSup = new CustomElement(document, nextElement);
                customElementSup.IsSupply = true;
                customElementSup.Direction = "П";
                //customElementSup.BranchMark = BranchNumber + "_" + customElementSup.Direction + "_" + Number.ToString();
                customElementSup.BranchMark = customElement.MepSpace + "_" + customElementSup.Direction + "_" + Number.ToString();
                customElementSup.MiniLoopNumber = MiniLoopNumber;
            }
            while (nextElement != null);
            BranchNumber = Number.ToString();
            BranchMark= BranchNumber + "_" + customElement.Direction + "_" + Number.ToString();
            //_counter++;
        }

        public static List<CustomBranch> UpDown(List<CustomBranch> branch)
        {
            List<CustomBranch> newCollection = new List<CustomBranch>();
            for (int i= branch.Count-1; i>-1;i--)
            {
                newCollection.Add(branch[i]);
            }

            return newCollection;

        }
        public void CreateNewReturnBranch(Document document, ElementId airterminal)
        {
            Number = _counter;
            MiniLoopNumber = _counter;
            Direction = "О";
            ElementId nextElement = null;

            CustomElement customElement = new CustomElement(document, airterminal);
            //customElement.MepSpace = ((document.GetElement(airterminal) as FamilyInstance).Space as SpatialElement).Number;
            customElement.MepSpace = (document.GetElement(airterminal) as FamilyInstance).Space.LookupParameter("ADSK_Номер квартиры").AsValueString();
            BranchNumber = customElement.MepSpace;
            customElement.Direction = "О";
           
                customElement.BranchMark = customElement.MepSpace + "_" + customElement.Direction + "_" + Number.ToString();
            //customElement.BranchMark = BranchNumber + "_" + customElement.Direction + "_" + Number.ToString();
            customElement.MiniLoopNumber = MiniLoopNumber;

            Elements.Add(customElement);
           
            var nextreturnelement = customElement.ReturnConnector.NextOwnerId;

            CustomElement customElementRet = new CustomElement(document, nextreturnelement);
            if (customElementRet.ElementId.IntegerValue == 4080858)
            {
                var airt = airterminal;
            }
            //customElementRet.MepSpace = ((document.GetElement(airterminal) as FamilyInstance).Space as SpatialElement).Number;
            BranchNumber = customElement.MepSpace;
            customElementRet.Direction = "О";
            customElementRet.BranchMark = customElement.MepSpace + "_" + customElementRet.Direction + "_" + Number.ToString();
            customElementRet.MiniLoopNumber = MiniLoopNumber;
            do
            {
                if (customElementRet.ElementId.IntegerValue == 4080864)
                {
                    var airt = airterminal;
                }


                Elements.Add(customElementRet);


                nextElement = customElementRet.NextElementId;
                if (customElementRet.OwnConnectors.Size > 3)
                {
                    nextElement = customElementRet.ReturnConnector.NextOwnerId;
                }
                customElementRet = new CustomElement(document, nextElement);
                customElementRet.IsSupply = false;
                customElementRet.Direction = "О";
                customElementRet.BranchMark = customElement.MepSpace + "_" + customElementRet.Direction + "_" + Number.ToString();
                customElementRet.MiniLoopNumber = MiniLoopNumber;
            }
            while (nextElement != null);
            BranchNumber = Number.ToString();
            _counter++;

        }


        public void CreateNewBranch(Document document, ElementId airterminal)
        {

            
             Number = _counter;
             MiniLoopNumber = _counter;
             ElementId nextElement = null;

            CustomElement customElement = new CustomElement(document, airterminal);
            customElement.MepSpace = ((document.GetElement(airterminal) as FamilyInstance).Space as SpatialElement).Number;
            BranchNumber = customElement.MepSpace;
            customElement.Direction = "П";
            customElement.BranchMark = BranchNumber +"_"+ customElement.Direction + "_" + Number.ToString();
            customElement.MiniLoopNumber = MiniLoopNumber;



            Elements.Add(customElement);
            var nextsupplyelement = customElement.SupplyConnector.NextOwnerId;
            var nextreturnelement = customElement.ReturnConnector.NextOwnerId;
           
            CustomElement customElementSup = new CustomElement(document, nextsupplyelement);
            customElementSup.MepSpace = ((document.GetElement(airterminal) as FamilyInstance).Space as SpatialElement).Number;
            BranchNumber = customElementSup.MepSpace;
            customElementSup.Direction = "П";
            customElementSup.BranchMark = BranchNumber +"_"+ customElementSup.Direction + "_" + Number.ToString();
            customElementSup.MiniLoopNumber = MiniLoopNumber;
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
                customElementSup.MiniLoopNumber = MiniLoopNumber;
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
            customElementRet.MiniLoopNumber = MiniLoopNumber;
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
                customElementRet.MiniLoopNumber = MiniLoopNumber;
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
