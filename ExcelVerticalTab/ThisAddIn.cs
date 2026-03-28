using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using ExcelVerticalTab.Controls;
using Microsoft.Office.Tools;
using Excel = Microsoft.Office.Interop.Excel;
using Office = Microsoft.Office.Core;

namespace ExcelVerticalTab;

public partial class ThisAddIn
{
    // キーはHWND（ウィンドウハンドル）。
    private readonly ConcurrentDictionary<int, PaneAndControl> _panes = new();

    private Menu? RibbonMenu { get; set; }

    private void ThisAddIn_Startup(object sender, EventArgs e)
    {
        Application.WorkbookActivate += Application_WorkbookActivate;
        // WindowDeactivate もトリガーとして使用（ウィンドウが閉じた後の掃除用）
        Application.WindowDeactivate += Application_WindowDeactivate;
    }

    private void Application_WindowDeactivate(Excel.Workbook wb, Excel.Window wn) => PrunePanes();

    private void Application_WorkbookActivate(Excel.Workbook wb) => OnActivate(wb);

    public void OnActivate(Excel.Workbook wb)
    {
        // If Save As recreates the host window, the stale HWND is pruned here and a fresh pane is created.
        PrunePanes();

        var window = Application.ActiveWindow;
        if (window == null) return;

        var hwnd = window.Hwnd;
        
        // 1. ペイン（入れ物）の確保
        if (!_panes.TryGetValue(hwnd, out var paneControl))
        {
            paneControl = CreatePane(window);
            _panes[hwnd] = paneControl;
        }

        // 2. ハンドラ（中身）の整合性チェックと差し替え
        var currentHandler = paneControl.Control.CurrentHandler;
        if (currentHandler == null || !WorkbookContainsWindow(currentHandler.TargetWorkbook, hwnd))
        {
            // 古いハンドラがある場合は破棄（イベント購読解除）
            currentHandler?.Dispose();

            var newHandler = new WorkbookHandler(wb);
            newHandler.Initialize();
            paneControl.Control.AssignWorkbookHandler(newHandler);
            currentHandler = newHandler;
        }
        
        currentHandler.SyncWorksheets();
        RibbonMenu?.InvalidatePanesVisibility();
    }

    /// <summary>
    /// すでに存在しないHWNDに紐付いているペインを掃除する
    /// </summary>
    private void PrunePanes()
    {
        // 現在Excelが認識している全ウィンドウのHWNDを取得
        HashSet<int> activeHwnds;
        try
        {
            activeHwnds = GetWindowHandles(Application.Windows);
        }
        catch { return; }

        var deadHwnds = _panes.Keys.Where(h => !activeHwnds.Contains(h)).ToList();
        
        foreach (var hwnd in deadHwnds)
        {
            if (_panes.TryRemove(hwnd, out var paneControl))
            {
                CleanUpPaneAndControl(paneControl);
            }
        }
    }

    private PaneAndControl CreatePane(Excel.Window window)
    {
        var control = new VerticalTabHost();
        control.Initialize();

        var pane = CustomTaskPanes.Add(control, "VTab", window);
        pane.DockPosition = Office.MsoCTPDockPosition.msoCTPDockPositionLeft;
        pane.Width = 100;
        pane.Visible = true;

        pane.VisibleChanged += Pane_VisibleChanged;

        return new PaneAndControl(pane, control);
    }

    private void Pane_VisibleChanged(object? sender, EventArgs e) => RibbonMenu?.InvalidatePanesVisibility();

    private bool WorkbookContainsWindow(Excel.Workbook? workbook, int hwnd)
    {
        if (workbook == null) return false;
        try
        {
            // FullName (Path + Name) で比較することで、別フォルダの同名ブックを識別
            return GetWindowHandles(workbook.Windows).Contains(hwnd);
        }
        catch { return false; }
    }

    private static HashSet<int> GetWindowHandles(Excel.Windows windows)
    {
        var hwnds = new HashSet<int>();
        try
        {
            var count = windows.Count;
            for (var index = 1; index <= count; index++)
            {
                Excel.Window? window = null;
                try
                {
                    window = windows[index];
                    hwnds.Add(window.Hwnd);
                }
                finally
                {
                    ReleaseComObject(window);
                }
            }
        }
        finally
        {
            ReleaseComObject(windows);
        }

        return hwnds;
    }

    private static void ReleaseComObject(object? comObject)
    {
        if (comObject != null && Marshal.IsComObject(comObject))
        {
            Marshal.ReleaseComObject(comObject);
        }
    }

    private void ThisAddIn_Shutdown(object sender, EventArgs e)
    {
        Application.WorkbookActivate -= Application_WorkbookActivate;
        Application.WindowDeactivate -= Application_WindowDeactivate;

        foreach (var x in _panes.Values)
        {
            CleanUpPaneAndControl(x);
        }
        _panes.Clear();
    }

    private void CleanUpPaneAndControl(PaneAndControl target)
    {
        target.Control.CurrentHandler?.Dispose();
        try
        {
            target.Pane.VisibleChanged -= Pane_VisibleChanged;
            CustomTaskPanes.Remove(target.Pane);
        }
        catch (ObjectDisposedException) { /* Ignore */ }
        catch (Exception ex) { Debug.WriteLine($"Cleanup error: {ex.Message}"); }
    }

    protected override Office.IRibbonExtensibility CreateRibbonExtensibilityObject()
    {
        RibbonMenu = new Menu();
        return RibbonMenu;
    }

    #region VSTO generated code
    private void InternalStartup()
    {
        Startup += ThisAddIn_Startup;
        Shutdown += ThisAddIn_Shutdown;
    }
    #endregion

    public IEnumerable<PaneAndControl> Panes => _panes.Values;
    
    public PaneAndControl? GetPaneForActiveWindow()
    {
        var window = Application.ActiveWindow;
        if (window == null) return null;

        return _panes.GetValueOrDefault(window.Hwnd);
    }
}

public class PaneAndControl(CustomTaskPane pane, VerticalTabHost control)
{
    public CustomTaskPane Pane { get; } = pane;
    public VerticalTabHost Control { get; } = control;
}
