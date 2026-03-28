using System;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using VerticalTabControlLib;

namespace ExcelVerticalTab.Controls;

public partial class VerticalTabHost : UserControl
{
    public VerticalTabHost()
    {
        InitializeComponent();
        TabControl = new TabUserControl();
    }

    public TabUserControl TabControl { get; }

    public WorkbookHandler? CurrentHandler { get; private set; }

    private void VerticalTabHost_Load(object? sender, EventArgs e)
    {
        var host = new ElementHost
        {
            Dock = DockStyle.Fill,
            Child = TabControl,
        };
        
        Controls.Add(host);
    }

    public void Initialize()
    {
    }

    public void AssignWorkbookHandler(WorkbookHandler handler)
    {
        CurrentHandler = handler;
        TabControl.DataContext = handler;
    }
}
