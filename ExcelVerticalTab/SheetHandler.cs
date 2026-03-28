using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Office.Interop.Excel;

namespace ExcelVerticalTab;

public partial class SheetHandler : ObservableObject
{
    public SheetHandler(Worksheet sheet)
    {
        TargetSheet = sheet;
        _header = TargetSheet.Name;
    }

    public Worksheet TargetSheet { get; }

    [ObservableProperty]
    private string _header;

    public override string ToString() => Header;
}
