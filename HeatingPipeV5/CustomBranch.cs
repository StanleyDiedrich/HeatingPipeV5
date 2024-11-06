using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Autodesk.Revit.DB;

namespace HeatingPipeV5
{
    public class CustomBranch
    {
        public Autodesk.Revit.DB.Document Document { get; set; }
        private static int _counter = 0;
        public int Number { get; set; }
        public double Pressure { get; set; }
        public double PBTot { get; set; }
        public List<CustomElement> Elements { get; set; } = new List<CustomElement>();
        public CustomBranch(Autodesk.Revit.DB.Document document, ElementId elementId)
        {
            Document = document;
            Number = _counter;
            _counter++;
        }
        public CustomBranch(Autodesk.Revit.DB.Document document)
        {
            Document = document;
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
            ElementId nextElement = null;
            CustomElement customElement = new CustomElement(document, airterminal);
            do
            {

                Elements.Add(customElement);
                nextElement = customElement.NextElementId;
                customElement = new CustomElement(document, nextElement);
            }
            while (nextElement != null);

        }





    }
}
