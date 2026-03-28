using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace VerticalTabControlLib;

public class CustomListBox : ListBox
{
    public static readonly DependencyProperty EnableSortByDragAndDropProperty = DependencyProperty.Register(
        "EnableSortByDragAndDrop", typeof(bool), typeof(CustomListBox), new PropertyMetadata(default(bool)));

    public bool EnableSortByDragAndDrop
    {
        get => (bool)GetValue(EnableSortByDragAndDropProperty);
        set => SetValue(EnableSortByDragAndDropProperty, value);
    }

    protected override DependencyObject GetContainerForItemOverride() => new CustomListBoxItem();

    protected override bool IsItemItsOwnContainerOverride(object item) => item is CustomListBoxItem;

    private FrameworkElement? _targetContainer;
    private object? _dragData;
    private Point _startPosition;
    private DragAdorner? _dragAdorner;
    private InsertionAdorner? _insertionAdorner;

    private FrameworkElement? GetContainer(FrameworkElement? originalSource)
    {
        return originalSource == null ? null : ContainerFromElement(originalSource) as FrameworkElement;
    }
    
    protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnPreviewMouseLeftButtonDown(e);
        if (!EnableSortByDragAndDrop) return;

        _targetContainer = GetContainer(e.OriginalSource as FrameworkElement);
        if (_targetContainer == null) return;
        
        _startPosition = PointToScreen(e.GetPosition(_targetContainer));
    }

    protected override void OnPreviewMouseMove(MouseEventArgs e)
    {
        base.OnPreviewMouseMove(e);
        if (!EnableSortByDragAndDrop || _targetContainer == null) return;
        
        var data = _targetContainer.DataContext;
        if (data == null) return;

        if (_targetContainer is ListBoxItem { IsSelected: false })
            return;

        var currentPosition = PointToScreen(e.GetPosition(_targetContainer));
        var delta = _startPosition - currentPosition;
        if (!delta.IsEnoughMoveForDrag()) return;

        _dragData = data;
        _dragAdorner ??= DragAdorner.Create(this, _targetContainer, _startPosition);
        _dragAdorner.SetOffset(currentPosition.X, currentPosition.Y);

        DragDrop.DoDragDrop(this, data, DragDropEffects.Move);

        ResetDragAndDropParameter();
    }

    protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
    {
        base.OnPreviewMouseUp(e);
        ResetDragAndDropParameter();
    }

    private void ResetDragAndDropParameter()
    {
        _targetContainer = null;
        _dragData = null;
        _startPosition = default;

        _dragAdorner?.Dispose();
        _dragAdorner = null;

        _insertionAdorner?.Dispose();
        _insertionAdorner = null;
    }

    protected override void OnPreviewDragEnter(DragEventArgs e)
    {
        base.OnPreviewDragEnter(e);

        var isBottom = false;
        var entered = GetContainer(e.OriginalSource as FrameworkElement);
        if (entered == null)
        {
            entered = ItemContainerGenerator.ContainerFromIndex(Items.Count - 1) as FrameworkElement;
            isBottom = true;
        }

        if (entered != null)
        {
            _insertionAdorner = InsertionAdorner.Create(entered, isBottom);
        }
    }

    protected override void OnPreviewDragOver(DragEventArgs e)
    {
        base.OnPreviewDragOver(e);
        var currentPosition = PointToScreen(e.GetPosition(this));
        _dragAdorner?.SetOffset(currentPosition.X, currentPosition.Y);
    }

    protected override void OnPreviewDragLeave(DragEventArgs e)
    {
        base.OnPreviewDragLeave(e);
        _insertionAdorner?.Dispose();
        _insertionAdorner = null;
    }

    protected override void OnPreviewDrop(DragEventArgs e)
    {
        base.OnPreviewDrop(e);
        var dropped = GetContainer(e.OriginalSource as FrameworkElement)?.DataContext;
        OnTryMoved(_dragData, dropped);
    }

    protected void OnTryMoved(object? source, object? target)
    {
        if (source == null || source == target) return;
        TryMoved?.Invoke(this, new TryMoveEventArgs(source, target!));
    }

    public event EventHandler<TryMoveEventArgs>? TryMoved;
}

public class CustomListBoxItem : ListBoxItem
{
    protected override void OnMouseEnter(MouseEventArgs e)
    {
        var parent = ItemsControl.ItemsControlFromItemContainer(this);
        if (parent?.IsMouseCaptured == true)
            parent.ReleaseMouseCapture();

        base.OnMouseEnter(e);
    }
}

public static class DragDropHelper
{
    public static bool IsEnoughMoveForDrag(this Vector delta)
    {
        return Math.Abs(delta.X) > SystemParameters.MinimumHorizontalDragDistance ||
               Math.Abs(delta.Y) > SystemParameters.MinimumVerticalDragDistance;
    }
}

public class TryMoveEventArgs(object source, object target) : EventArgs
{
    public object Source { get; } = source;
    public object Target { get; } = target;
}
