﻿using System;
using System.Collections.Generic;
using System.Linq;
using Revit.Elements;
using RevitServices.Persistence;
using RevitServices.Transactions;

namespace archilab.Revit.Views
{
    /// <summary>
    /// Methods and properties typically associated with View Sets in Revit
    /// </summary>
    public class ViewSets
    {
        internal ViewSets()
        {
        }

        /// <summary>
        /// Creates new View Set by name and List of Views.
        /// </summary>
        /// <param name="views">List of Views.</param>
        /// <param name="name">Name of the View Set.</param>
        /// <param name="replace">Override existing View Set with same name.</param>
        /// <returns>View Sheet Set.</returns>
        public static Element ByViewsName(List<Element> views, string name, bool replace = false)
        {
            if (views == null || !views.Any()) throw new ArgumentException(nameof(views));
            if (string.IsNullOrEmpty(name)) throw new ArgumentException(nameof(name));

            var doc = DocumentManager.Instance.CurrentDBDocument;

            var viewSet = new Autodesk.Revit.DB.ViewSet();
            foreach (var v in views)
            {
                viewSet.Insert((Autodesk.Revit.DB.View) v.InternalElement);
            }

            var printManager = doc.PrintManager;
            printManager.PrintRange = Autodesk.Revit.DB.PrintRange.Select;

            var settings = printManager.ViewSheetSetting;
            settings.CurrentViewSheetSet.Views = viewSet;

            TransactionManager.Instance.EnsureInTransaction(doc);

            try
            {
                settings.SaveAs(name);
            }
            catch (Autodesk.Revit.Exceptions.InvalidOperationException)
            {
                if (replace)
                {
                    try
                    {
                        settings.Delete();
                        doc.Regenerate();

                        settings.CurrentViewSheetSet.Views = viewSet;
                        settings.SaveAs(name);
                    }
                    catch (Autodesk.Revit.Exceptions.InvalidOperationException)
                    {
                        throw new Exception("Could not replace existing View Set.");
                    }
                }
            }

            TransactionManager.Instance.TransactionTaskDone();

            var set = settings.CurrentViewSheetSet as Autodesk.Revit.DB.ViewSheetSet;
            return set?.ToDSType(true);
        }
    }
}
