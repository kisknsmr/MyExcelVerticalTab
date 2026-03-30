using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Forms;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Office.Interop.Excel;
using VerticalTabControlLib;

namespace ExcelVerticalTab;

public partial class WorkbookHandler(Workbook workbook) : ObservableObject, ITabUserControlViewModel<SheetHandler>, IDisposable
{
    public Workbook TargetWorkbook { get; } = workbook;
    public WindowMessageHandler MsgHandler { get; } = new(workbook.Application);
    public Func<bool>? ShouldTrackSheetState { get; set; }

    public ObservableCollection<SheetHandler> Items { get; set; } = [];

    [ObservableProperty]
    private SheetHandler? _selectedItem;

    private ICollectionView? _itemsView;
    public ICollectionView? ItemsView
    {
        get => _itemsView ??= CollectionViewSource.GetDefaultView(Items);
        set => SetProperty(ref _itemsView, value);
    }

    [ObservableProperty]
    private string? _inputToFilter;

    private SheetSortMode? _sortMode;
    private readonly Timer _sheetStateTimer = new() { Interval = 1000 };
    private string _lastSheetStateSignature = string.Empty;

    partial void OnSelectedItemChanged(SheetHandler? value) => OnSelectedSheetChanged(value);

    partial void OnInputToFilterChanged(string? value) => ExecuteFilter();

    private void ExecuteFilter()
    {
        if (string.IsNullOrWhiteSpace(InputToFilter))
        {
            if (ItemsView != null)
            {
                ItemsView.Filter = null;
                ItemsView.Refresh();
            }
            SelectFirst();
            return;
        }

        Predicate<object> filter = x =>
        {
            if (x is not SheetHandler s) return false;
            var source = s.Header ?? "";
            const CompareOptions options = CompareOptions.IgnoreCase | CompareOptions.IgnoreKanaType | CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreWidth;
            return CultureInfo.CurrentCulture.CompareInfo.IndexOf(source, InputToFilter, options) >= 0;
        };
        
        if (ItemsView != null)
        {
            ItemsView.Filter = filter;
        }
        
        SelectFirst();
    }

    private void SelectFirst()
    {
        if (SelectedItem != null) return;
        
        var first = ItemsView?.OfType<SheetHandler>().FirstOrDefault();
        if (first != null)
            SelectedItem = first;
    }

    public void TryMoved(object? source, object? target)
    {
        if (source is not SheetHandler src) return;

        var dst = target as SheetHandler;
        if (src == dst) return;

        _suppressChanged = true;

        try
        {
            // Excel側のシート移動
            if (dst == null)
            {
                var last = Items.Last();
                if (src != last) src.TargetSheet.Move(After: last.TargetSheet);
            }
            else
            {
                src.TargetSheet.Move(Before: dst.TargetSheet);
            }

            // UI側の並び替え
            var srcIndex = Items.IndexOf(src);
            var dstIndex = dst == null ? Items.Count - 1 : Items.IndexOf(dst);
            
            if (srcIndex != -1 && dstIndex != -1 && srcIndex != dstIndex)
            {
                // ObservableCollection.Move(old, new) は、
                // newIndex が oldIndex より大きい場合、old項目が「削除」された後の位置を指定する必要がある
                if (srcIndex < dstIndex && dst != null)
                {
                    dstIndex--;
                }
                Items.Move(srcIndex, dstIndex);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error during move: {ex.Message}");
            SyncWorksheets();
        }
        finally
        {
            _suppressChanged = false;
        }
    }

    public string? TryRename(object? source, string? newName)
    {
        if (source is not SheetHandler sheetHandler) return "No sheet is selected.";

        var trimmed = newName?.Trim();
        if (string.IsNullOrWhiteSpace(trimmed)) return "Sheet name cannot be empty.";

        if (string.Equals(sheetHandler.Header, trimmed, StringComparison.Ordinal))
            return null;

        try
        {
            sheetHandler.TargetSheet.Name = trimmed;
            RefreshAfterMutation();
            return null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Rename error: {ex.Message}");
            return "The sheet name could not be changed. Check for duplicate names or invalid characters.";
        }
    }

    public string? TryDuplicate(IReadOnlyList<object> sources)
    {
        var selectedSheets = GetSelectedSheets(sources);
        if (selectedSheets.Count == 0) return "No sheet is selected.";

        try
        {
            foreach (var sheetHandler in selectedSheets.OrderBy(x => x.TargetSheet.Index))
            {
                sheetHandler.TargetSheet.Copy(After: sheetHandler.TargetSheet);
            }

            RefreshAfterMutation();
            return null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Duplicate error: {ex.Message}");
            return "The sheet could not be duplicated.";
        }
    }

    public string? TryDelete(IReadOnlyList<object> sources)
    {
        var selectedSheets = GetSelectedSheets(sources);
        if (selectedSheets.Count == 0) return "No sheet is selected.";
        if (TargetWorkbook.Worksheets.Count - selectedSheets.Count <= 0) return "At least one sheet must remain.";

        var app = TargetWorkbook.Application;
        var previousAlerts = app.DisplayAlerts;

        try
        {
            app.DisplayAlerts = false;

            foreach (var sheetHandler in selectedSheets.OrderByDescending(x => x.TargetSheet.Index))
            {
                sheetHandler.TargetSheet.Delete();
            }

            RefreshAfterMutation();
            return null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Delete error: {ex.Message}");
            return "The sheet could not be deleted.";
        }
        finally
        {
            app.DisplayAlerts = previousAlerts;
        }
    }

    public string? TrySetVisibility(IReadOnlyList<object> sources, SheetVisibilityMode mode)
    {
        var selectedSheets = GetSelectedSheets(sources);
        if (selectedSheets.Count == 0) return "No sheet is selected.";

        try
        {
            if (mode != SheetVisibilityMode.Visible)
            {
                var visibleCount = TargetWorkbook.Worksheets.Cast<Worksheet>()
                    .Count(x => x.Visible == XlSheetVisibility.xlSheetVisible);
                var visibleToHide = selectedSheets.Count(x => x.TargetSheet.Visible == XlSheetVisibility.xlSheetVisible);

                if (visibleCount - visibleToHide <= 0)
                    return "At least one visible sheet must remain.";
            }

            var targetVisibility = mode switch
            {
                SheetVisibilityMode.Visible => XlSheetVisibility.xlSheetVisible,
                SheetVisibilityMode.VeryHidden => XlSheetVisibility.xlSheetVeryHidden,
                _ => XlSheetVisibility.xlSheetHidden,
            };

            foreach (var sheetHandler in selectedSheets)
            {
                sheetHandler.TargetSheet.Visible = targetVisibility;
            }

            RefreshAfterMutation();
            return null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Hide/show error: {ex.Message}");
            return mode switch
            {
                SheetVisibilityMode.Visible => "The selected sheets could not be shown.",
                SheetVisibilityMode.VeryHidden => "The selected sheets could not be set to very hidden.",
                _ => "The selected sheets could not be hidden.",
            };
        }
    }

    public string? CreateSheet()
    {
        try
        {
            var last = (Worksheet)TargetWorkbook.Worksheets[TargetWorkbook.Worksheets.Count];
            TargetWorkbook.Worksheets.Add(After: last);
            RefreshAfterMutation();
            return null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Create sheet error: {ex.Message}");
            return "A new sheet could not be created.";
        }
    }

    public void ApplySort(SheetSortMode mode)
    {
        _sortMode = mode;

        try
        {
            var comparer = StringComparer.CurrentCultureIgnoreCase;
            var ordered = TargetWorkbook.Worksheets.Cast<Worksheet>()
                .OrderBy(x => x.Name, comparer)
                .ToList();

            if (mode == SheetSortMode.NameDescending)
            {
                ordered.Reverse();
            }

            for (var i = 0; i < ordered.Count; i++)
            {
                var currentAtPosition = (Worksheet)TargetWorkbook.Worksheets[i + 1];
                var desired = ordered[i];
                if (currentAtPosition == desired) continue;

                desired.Move(Before: currentAtPosition);
            }

            SyncWorksheets();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Sort error: {ex.Message}");
            SyncWorksheets();
        }
    }

    private void RefreshAfterMutation()
    {
        if (_sortMode is null)
        {
            SyncWorksheets();
            return;
        }

        ApplySort(_sortMode.Value);
    }

    private List<SheetHandler> GetSelectedSheets(IReadOnlyList<object> sources)
    {
        return sources
            .OfType<SheetHandler>()
            .Distinct()
            .ToList();
    }

    public void Initialize()
    {
        TargetWorkbook.NewSheet += TargetWorkbook_NewSheet;
        TargetWorkbook.SheetBeforeDelete += TargetWorkbook_SheetBeforeDelete;
        TargetWorkbook.SheetActivate += TargetWorkbook_SheetActivate;
        TargetWorkbook.AfterSave += TargetWorkbook_AfterSave; // 保存後の名前変更に対応
        MsgHandler.RefreshRequired += MsgHandlerOnRefreshRequired;
        _lastSheetStateSignature = CaptureSheetStateSignature();
        _sheetStateTimer.Tick += SheetStateTimerOnTick;
        _sheetStateTimer.Start();
    }

    private void RemoveHandler()
    {
        TargetWorkbook.NewSheet -= TargetWorkbook_NewSheet;
        TargetWorkbook.SheetBeforeDelete -= TargetWorkbook_SheetBeforeDelete;
        TargetWorkbook.SheetActivate -= TargetWorkbook_SheetActivate;
        TargetWorkbook.AfterSave -= TargetWorkbook_AfterSave;
        MsgHandler.RefreshRequired -= MsgHandlerOnRefreshRequired;
        _sheetStateTimer.Stop();
        _sheetStateTimer.Tick -= SheetStateTimerOnTick;
        _sheetStateTimer.Dispose();
    }

    private void TargetWorkbook_AfterSave(bool Success)
    {
        if (Success) SyncWorksheets(); // 名前変更（名前を付けて保存）の可能性があるため
    }

    public void SyncWorksheets()
    {
        if (_disposedValue) return;

        _suppressChanged = true;
        var currentSelected = SelectedItem?.Header;
        Items.Clear();
        
        foreach (var worksheet in TargetWorkbook.Worksheets.Cast<Worksheet>())
        {
            Items.Add(new SheetHandler(worksheet));    
        }

        SelectedItem = Items.FirstOrDefault(x => x.Header == currentSelected) ?? GetSheetHandler(TargetWorkbook.ActiveSheet);
        _lastSheetStateSignature = CaptureSheetStateSignature();
        _suppressChanged = false;
    }

    private SheetHandler? GetSheetHandler(Worksheet sheet) => Items.FirstOrDefault(x => x.TargetSheet == sheet);

    private void TargetWorkbook_NewSheet(object Sh) => SyncWorksheets();

    private void TargetWorkbook_SheetBeforeDelete(object Sh)
    {
        if (Sh is not Worksheet sheet) return;
        var handler = GetSheetHandler(sheet);
        if (handler != null) Items.Remove(handler);
    }

    private void TargetWorkbook_SheetActivate(object Sh)
    {
        if (Sh is not Worksheet sheet) return;
        SelectedItem = GetSheetHandler(sheet);
    }

    private void MsgHandlerOnRefreshRequired(object? sender, EventArgs eventArgs)
    {
        if (_suppressChanged) return;
        SyncWorksheets();
    }

    private void SheetStateTimerOnTick(object? sender, EventArgs e)
    {
        if (_disposedValue || _suppressChanged) return;
        if (ShouldTrackSheetState != null && !ShouldTrackSheetState()) return;

        var currentSignature = CaptureSheetStateSignature();
        if (currentSignature == _lastSheetStateSignature) return;

        SyncWorksheets();
    }

    private string CaptureSheetStateSignature()
    {
        try
        {
            return string.Join("|", TargetWorkbook.Worksheets.Cast<Worksheet>().Select(CreateSheetStateSignature));
        }
        catch
        {
            return _lastSheetStateSignature;
        }
    }

    private static string CreateSheetStateSignature(Worksheet sheet)
    {
        object? colorValue = null;

        try
        {
            colorValue = sheet.Tab.Color;
        }
        catch
        {
            // Ignore tab color lookup failures and rely on the previous snapshot.
        }

        return string.Format(
            CultureInfo.InvariantCulture,
            "{0}:{1}:{2}:{3}",
            sheet.Index,
            sheet.Name,
            (int)sheet.Visible,
            colorValue ?? "none");
    }

    public void Refresh_Required() => SyncWorksheets();

    public event EventHandler<EventArgs<SheetHandler>>? SelectedSheetChanged;

    private bool _suppressChanged;

    protected void OnSelectedSheetChanged(SheetHandler? sheetHandler)
    {
        if (_suppressChanged || sheetHandler == null) return;
        if (sheetHandler.IsHidden) return;
        
        try
        {
            sheetHandler.TargetSheet.Activate();
            SelectedSheetChanged?.Invoke(this, Helper.CreateEventArgs(sheetHandler));
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Activate error: {ex.Message}");
        }
    }

    private bool _disposedValue;

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                RemoveHandler();
            }
            MsgHandler.Dispose();
            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~WorkbookHandler() => Dispose(false);
}
