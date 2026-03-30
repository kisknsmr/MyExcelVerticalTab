using System.Collections.Generic;
using System.Drawing;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Office.Interop.Excel;
using VerticalTabControlLib;
using DrawingColor = System.Drawing.Color;
using MediaBrush = System.Windows.Media.Brush;
using MediaBrushes = System.Windows.Media.Brushes;

namespace ExcelVerticalTab;

public partial class SheetHandler : ObservableObject, SheetItemState
{
    public SheetHandler(Worksheet sheet)
    {
        TargetSheet = sheet;
        UpdateFromSheet();
    }

    public Worksheet TargetSheet { get; }

    [ObservableProperty]
    private string _header = string.Empty;

    [ObservableProperty]
    private bool _isHidden;

    [ObservableProperty]
    private SheetVisibilityMode _visibilityMode;

    [ObservableProperty]
    private string _visibilityLabel = string.Empty;

    [ObservableProperty]
    private MediaBrush _tabColorBrush = MediaBrushes.Transparent;

    public void UpdateFromSheet()
    {
        Header = TargetSheet.Name;
        VisibilityMode = GetVisibilityMode(TargetSheet.Visible);
        IsHidden = VisibilityMode != SheetVisibilityMode.Visible;
        VisibilityLabel = VisibilityMode switch
        {
            SheetVisibilityMode.Hidden => "hidden",
            SheetVisibilityMode.VeryHidden => "very hidden",
            _ => string.Empty,
        };
        TabColorBrush = CreateTabColorBrush();
    }

    private static SheetVisibilityMode GetVisibilityMode(XlSheetVisibility visibility)
    {
        return visibility switch
        {
            XlSheetVisibility.xlSheetVisible => SheetVisibilityMode.Visible,
            XlSheetVisibility.xlSheetVeryHidden => SheetVisibilityMode.VeryHidden,
            _ => SheetVisibilityMode.Hidden,
        };
    }

    private MediaBrush CreateTabColorBrush()
    {
        try
        {
            var colorValue = TargetSheet.Tab.Color;
            if (colorValue == null) return MediaBrushes.Transparent;

            var oleColor = System.Convert.ToInt32(colorValue);
            if (oleColor <= 0) return MediaBrushes.Transparent;

            var drawingColor = ColorTranslator.FromOle(oleColor);
            if (drawingColor.A == 0) return MediaBrushes.Transparent;

            var brush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(drawingColor.A, drawingColor.R, drawingColor.G, drawingColor.B));
            brush.Freeze();
            return brush;
        }
        catch
        {
            return MediaBrushes.Transparent;
        }
    }

    public override string ToString() => Header;
}
