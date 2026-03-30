using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace VerticalTabControlLib;

/// <summary>
/// MyCanvas.xaml の相互作用ロジック
/// </summary>
public partial class TabUserControl : UserControl
{
    private SheetSortMode? _currentSortMode;

    public TabUserControl()
    {
        InitializeComponent();
        UpdateSortButtonTooltip();
    }

    private void CmdRefresh_OnClick(object sender, RoutedEventArgs e)
    {
        var vm = DataContext as ITabUserControlViewModel;
        vm?.Refresh_Required();
    }

    private void LstTab_OnTryMoved(object sender, TryMoveEventArgs e)
    {
        var vm = DataContext as ITabUserControlViewModel;
        vm?.TryMoved(e.Source, e.Target);
    }

    private void LstTab_OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.F2) return;

        BeginRenameSelected();
        e.Handled = true;
    }

    private void BeginRenameSelected()
    {
        var vm = DataContext as ITabUserControlViewModel;
        var selectedItems = GetSelectedItems();
        if (vm == null || selectedItems.Count == 0) return;

        if (selectedItems.Count > 1)
        {
            MessageBox.Show("Rename works with a single selected sheet.", "Rename Sheet", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var target = selectedItems[0];

        var currentName = target.ToString() ?? "";
        var newName = ShowRenameDialog(currentName);
        if (newName == null) return;

        var error = vm.TryRename(target, newName);
        if (!string.IsNullOrEmpty(error))
        {
            MessageBox.Show(error, "Rename Sheet", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void CmdNew_OnClick(object sender, RoutedEventArgs e)
    {
        var vm = DataContext as ITabUserControlViewModel;
        var error = vm?.CreateSheet();
        ShowErrorIfNeeded(error, "New Sheet");
    }

    private void DuplicateMenuItem_OnClick(object sender, RoutedEventArgs e)
    {
        var vm = DataContext as ITabUserControlViewModel;
        var error = vm?.TryDuplicate(GetSelectedItems());
        ShowErrorIfNeeded(error, "Duplicate Sheet");
    }

    private void DeleteMenuItem_OnClick(object sender, RoutedEventArgs e)
    {
        var vm = DataContext as ITabUserControlViewModel;
        var targets = GetSelectedItems();
        if (vm == null || targets.Count == 0) return;

        var result = MessageBox.Show(
            targets.Count == 1 ? "Delete the selected sheet?" : $"Delete {targets.Count} selected sheets?",
            "Delete Sheet",
            MessageBoxButton.OKCancel,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.OK) return;

        var error = vm.TryDelete(targets);
        ShowErrorIfNeeded(error, "Delete Sheet");
    }

    private void HideMenuItem_OnClick(object sender, RoutedEventArgs e)
    {
        var vm = DataContext as ITabUserControlViewModel;
        var error = vm?.TrySetVisibility(GetSelectedItems(), SheetVisibilityMode.Hidden);
        ShowErrorIfNeeded(error, "Hide Sheet");
    }

    private void VeryHideMenuItem_OnClick(object sender, RoutedEventArgs e)
    {
        var vm = DataContext as ITabUserControlViewModel;
        var error = vm?.TrySetVisibility(GetSelectedItems(), SheetVisibilityMode.VeryHidden);
        ShowErrorIfNeeded(error, "Very Hidden");
    }

    private void ShowMenuItem_OnClick(object sender, RoutedEventArgs e)
    {
        var vm = DataContext as ITabUserControlViewModel;
        var error = vm?.TrySetVisibility(GetSelectedItems(), SheetVisibilityMode.Visible);
        ShowErrorIfNeeded(error, "Show Sheet");
    }

    private void RenameMenuItem_OnClick(object sender, RoutedEventArgs e) => BeginRenameSelected();

    private void TabContextMenu_OnOpened(object sender, RoutedEventArgs e)
    {
        var selectedItems = GetSelectedItems();
        var states = selectedItems.OfType<SheetItemState>().ToList();
        var hasSelection = selectedItems.Count > 0;
        var isSingleSelection = selectedItems.Count == 1;

        mnuRename.IsEnabled = isSingleSelection;
        mnuDuplicate.IsEnabled = hasSelection;
        mnuDelete.IsEnabled = hasSelection;
        mnuHide.IsEnabled = states.Any(x => x.VisibilityMode != SheetVisibilityMode.Hidden);
        mnuVeryHide.IsEnabled = states.Any(x => x.VisibilityMode != SheetVisibilityMode.VeryHidden);
        mnuShow.IsEnabled = states.Any(x => x.VisibilityMode != SheetVisibilityMode.Visible);
    }

    private void CmdSortMenu_OnClick(object sender, RoutedEventArgs e)
    {
        ctxSort.PlacementTarget = cmdSortMenu;
        ctxSort.IsOpen = true;
    }

    private void SortContextMenu_OnOpened(object sender, RoutedEventArgs e)
    {
        mnuSortNameAscending.IsChecked = _currentSortMode == SheetSortMode.NameAscending;
        mnuSortNameDescending.IsChecked = _currentSortMode == SheetSortMode.NameDescending;
    }

    private void SortNameAscendingMenuItem_OnClick(object sender, RoutedEventArgs e) => ApplySort(SheetSortMode.NameAscending);

    private void SortNameDescendingMenuItem_OnClick(object sender, RoutedEventArgs e) => ApplySort(SheetSortMode.NameDescending);

    private void ApplySort(SheetSortMode mode)
    {
        if (_currentSortMode == mode) return;

        var result = MessageBox.Show(
            "This will reorder the actual Excel sheet tabs and cannot automatically restore the previous order.\n\nContinue?",
            "Sort Sheets",
            MessageBoxButton.OKCancel,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.OK) return;

        _currentSortMode = mode;
        UpdateSortButtonTooltip();

        var vm = DataContext as ITabUserControlViewModel;
        vm?.ApplySort(mode);
    }

    private void UpdateSortButtonTooltip()
    {
        cmdSortMenu.ToolTip = _currentSortMode is null
            ? "Sort sheets"
            : $"Sort: {GetSortLabel(_currentSortMode.Value)}";
    }

    private static string GetSortLabel(SheetSortMode mode) => mode switch
    {
        SheetSortMode.NameAscending => "Name A-Z",
        SheetSortMode.NameDescending => "Name Z-A",
        _ => "Name A-Z",
    };

    private void ShowErrorIfNeeded(string? error, string title)
    {
        if (string.IsNullOrEmpty(error)) return;

        MessageBox.Show(error, title, MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    private IReadOnlyList<object> GetSelectedItems()
    {
        return lstTab.SelectedItems.Cast<object>().ToArray();
    }

    private string? ShowRenameDialog(string currentName)
    {
        var textBox = new TextBox
        {
            Text = currentName,
            MinWidth = 280,
            Margin = new Thickness(0, 0, 0, 12),
        };

        var okButton = new Button
        {
            Content = "OK",
            Width = 80,
            IsDefault = true,
            Margin = new Thickness(0, 0, 8, 0),
        };

        var cancelButton = new Button
        {
            Content = "Cancel",
            Width = 80,
            IsCancel = true,
        };

        var buttons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Children = { okButton, cancelButton },
        };

        var panel = new StackPanel
        {
            Margin = new Thickness(16),
            Children =
            {
                new TextBlock
                {
                    Text = "Enter a new sheet name.",
                    Margin = new Thickness(0, 0, 0, 8),
                },
                textBox,
                buttons,
            },
        };

        var dialog = new Window
        {
            Title = "Rename Sheet",
            Content = panel,
            SizeToContent = SizeToContent.WidthAndHeight,
            ResizeMode = ResizeMode.NoResize,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            ShowInTaskbar = false,
            Owner = Window.GetWindow(this),
        };

        string? result = null;
        okButton.Click += (_, _) =>
        {
            result = textBox.Text;
            dialog.DialogResult = true;
        };

        if (dialog.ShowDialog() != true) return null;

        return result;
    }
}

public interface ITabUserControlViewModel
{
    void Refresh_Required();

    string? InputToFilter { get; set; }

    void TryMoved(object? source, object? target);

    string? TryRename(object? source, string? newName);

    string? TryDuplicate(IReadOnlyList<object> sources);

    string? TryDelete(IReadOnlyList<object> sources);

    string? TrySetVisibility(IReadOnlyList<object> sources, SheetVisibilityMode mode);

    string? CreateSheet();

    void ApplySort(SheetSortMode mode);
}

public interface ITabUserControlViewModel<T> : ITabUserControlViewModel
{
    ObservableCollection<T> Items { get; set; }
    ICollectionView? ItemsView { get; set; }
    T? SelectedItem { get; set; }
}

internal class TabUserControlViewModelMock : ITabUserControlViewModel<object>
{
    public ObservableCollection<object> Items { get; set; } = [];

    private ICollectionView? _itemsView;

    public ICollectionView? ItemsView
    {
        get => _itemsView ??= CollectionViewSource.GetDefaultView(Items);
        set => _itemsView = value;
    }

    public object? SelectedItem { get; set; }

    public string? InputToFilter
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }

    public void Refresh_Required() => throw new NotImplementedException();

    public void TryMoved(object? source, object? target) => throw new NotImplementedException();

    public string? TryRename(object? source, string? newName) => throw new NotImplementedException();

    public string? TryDuplicate(IReadOnlyList<object> sources) => throw new NotImplementedException();

    public string? TryDelete(IReadOnlyList<object> sources) => throw new NotImplementedException();

    public string? TrySetVisibility(IReadOnlyList<object> sources, SheetVisibilityMode mode) => throw new NotImplementedException();

    public string? CreateSheet() => throw new NotImplementedException();

    public void ApplySort(SheetSortMode mode) => throw new NotImplementedException();
}

public enum SheetSortMode
{
    NameAscending,
    NameDescending,
}

public enum SheetVisibilityMode
{
    Visible,
    Hidden,
    VeryHidden,
}

public interface SheetItemState
{
    bool IsHidden { get; }
    SheetVisibilityMode VisibilityMode { get; }
    string VisibilityLabel { get; }
}
