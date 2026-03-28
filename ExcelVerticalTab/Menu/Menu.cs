using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Office = Microsoft.Office.Core;

namespace ExcelVerticalTab;

[ComVisible(true)]
public class Menu : Office.IRibbonExtensibility
{
    private Office.IRibbonUI? _ribbon;

    public string GetCustomUI(string ribbonID) => GetResourceText("ExcelVerticalTab.Menu.Menu.xml") ?? "";

    #region Ribbon Callbacks

    public void Ribbon_Load(Office.IRibbonUI ribbonUI) => _ribbon = ribbonUI;

    public void chkVisibility_Changed(Office.IRibbonControl control, bool isHide)
    {
        var pane = Globals.ThisAddIn.GetPaneForActiveWindow();
        if (pane == null) return;
        
        pane.Pane.Visible = !isHide;
    }

    public void cmdRefresh_Click(Office.IRibbonControl control)
    {
        var book = Globals.ThisAddIn.Application.ActiveWorkbook;
        if (book == null) return;

        Globals.ThisAddIn.OnActivate(book);            
    }

    public bool GetPanesVisibility(Office.IRibbonControl control)
    {
        var pane = Globals.ThisAddIn.GetPaneForActiveWindow();
        if (pane == null) return false;

        return !pane.Pane.Visible;
    }

    public void InvalidatePanesVisibility() => _ribbon?.InvalidateControl("chkVisibility");

    #endregion

    #region Helpers

    private static string? GetResourceText(string resourceName)
    {
        var asm = Assembly.GetExecutingAssembly();
        var resourceNames = asm.GetManifestResourceNames();
        foreach (var name in resourceNames)
        {
            if (string.Equals(resourceName, name, StringComparison.OrdinalIgnoreCase))
            {
                using var stream = asm.GetManifestResourceStream(name);
                if (stream == null) return null;
                using var reader = new StreamReader(stream);
                return reader.ReadToEnd();
            }
        }
        return null;
    }

    #endregion
}
