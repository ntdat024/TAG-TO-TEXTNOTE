using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using View = Autodesk.Revit.DB.View;

namespace OPENSOURCE
{
    [Transaction(TransactionMode.Manual)]
    public class TagToTextCmd : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            UIDocument uidoc = uiApp.ActiveUIDocument;
            Document doc = uidoc.Document;
            View activeView = doc.ActiveView;

            while (true)
            {
                try
                {
                    Reference picked = uidoc.Selection.PickObject(ObjectType.Element, new TagFilter(), "Pick Element Tags");
                    Element elem = doc.GetElement(picked);
                    
                    using (Transaction t = new Transaction(doc, " "))
                    {
                        t.Start();

                        XYZ point = new XYZ();
                        string content = "";

                        if (elem is SpatialElementTag sTag)
                        {
                            point = sTag.TagHeadPosition;
                            content = sTag.TagText;
                        }
                        else
                        {
                            var iTag = elem as IndependentTag;
                            point = iTag.TagHeadPosition;
                            content = iTag.TagText;
                        }

                        if (string.IsNullOrEmpty(content))
                        {
                            ElementId textNoteTypeId = doc.GetDefaultElementTypeId(ElementTypeGroup.TextNoteType);
                            TextNote textNote = TextNote.Create(doc, activeView.Id, point, content, textNoteTypeId);
                        }

                        t.Commit();
                    }
                }
                catch
                {
                    break;
                }
            }


            return Result.Succeeded;
        }
    }

    public class TagFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            if (elem is IndependentTag || elem is SpatialElementTag)
            {
                return true;
            }

            return false;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            throw new NotImplementedException();
        }
    }
}
