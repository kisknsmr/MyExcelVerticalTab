using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Office.Interop.Excel;
using VerticalTabControlLib;

namespace ExcelVerticalTab;

public partial class WorkbookHandler(Workbook workbook) : ObservableObject, ITabUserControlViewModel<SheetHandler>, IDisposable
{
    public Workbook TargetWorkbook { get; } = workbook;
    public WindowMessageHandler MsgHandler { get; } = new(workbook.Application);

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

    public void Initialize()
    {
        TargetWorkbook.NewSheet += TargetWorkbook_NewSheet;
        TargetWorkbook.SheetBeforeDelete += TargetWorkbook_SheetBeforeDelete;
        TargetWorkbook.SheetActivate += TargetWorkbook_SheetActivate;
        TargetWorkbook.AfterSave += TargetWorkbook_AfterSave; // 保存後の名前変更に対応
        MsgHandler.RefreshRequired += MsgHandlerOnRefreshRequired;
    }

    private void RemoveHandler()
    {
        TargetWorkbook.NewSheet -= TargetWorkbook_NewSheet;
        TargetWorkbook.SheetBeforeDelete -= TargetWorkbook_SheetBeforeDelete;
        TargetWorkbook.SheetActivate -= TargetWorkbook_SheetActivate;
        TargetWorkbook.AfterSave -= TargetWorkbook_AfterSave;
        MsgHandler.RefreshRequired -= MsgHandlerOnRefreshRequired;
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

    public void Refresh_Required() => SyncWorksheets();

    public event EventHandler<EventArgs<SheetHandler>>? SelectedSheetChanged;

    private bool _suppressChanged;

    protected void OnSelectedSheetChanged(SheetHandler? sheetHandler)
    {
        if (_suppressChanged || sheetHandler == null) return;
        
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
