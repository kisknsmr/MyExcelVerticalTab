using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;

namespace VerticalTabControlLib;

public abstract class AdornerBase(UIElement adornedElement, UIElement core) : Adorner(adornedElement), IDisposable
{
    private AdornerLayer? Layer { get; } = AdornerLayer.GetAdornerLayer(adornedElement);

    protected UIElement Core { get; } = core;

    static AdornerBase()
    {
    }

    protected override Visual GetVisualChild(int index) => Core;

    protected override int VisualChildrenCount => 1;

    protected override Size MeasureOverride(Size constraint)
    {
        Core.Measure(constraint);
        return base.MeasureOverride(constraint);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        Core.Arrange(new Rect(finalSize));
        return base.ArrangeOverride(finalSize);
    }

    #region IDisposable Support
    private bool _disposedValue;

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                Layer?.Remove(this);
            }
            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    #endregion
}

public class DragAdorner : AdornerBase
{
    private const double ThisOpacity = 0.7;

    private Point StartPosition { get; set; }

    private DragAdorner(UIElement adornedElement, UIElement core) : base(adornedElement, core)
    {
    }

    public double OffsetX { get; private set; }
    public double OffsetY { get; private set; }

    public void SetOffset(double x, double y)
    {
        OffsetX = x - StartPosition.X;
        OffsetY = y - StartPosition.Y;

        var layer = Parent as AdornerLayer;
        layer?.Update(AdornedElement);
    }

    public override GeneralTransform? GetDesiredTransform(GeneralTransform transform)
    {
        var transformGroup = new GeneralTransformGroup();
        var baseObj = base.GetDesiredTransform(transform);
        if (baseObj != null)
            transformGroup.Children.Add(baseObj);

        transformGroup.Children.Add(new TranslateTransform(OffsetX, OffsetY));

        return transformGroup;
    }

    public static DragAdorner Create(UIElement adornedElement, UIElement dragTarget, Point startPosition)
    {
        var bounds = VisualTreeHelper.GetDescendantBounds(dragTarget);
        var ghost = new Rectangle
        {
            Width = bounds.Width,
            Height = bounds.Height,
            Fill = new VisualBrush(dragTarget) { Opacity = ThisOpacity },
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
        };

        var adorner = new DragAdorner(adornedElement, ghost);
        adorner.StartPosition = startPosition;
        return adorner;
    }
}

public class InsertionAdorner : AdornerBase
{
    private InsertionAdorner(UIElement adornedElement, UIElement core, InsertPositionAdorner control) 
        : base(adornedElement, core)
    {
        Control = control;
    }

    public InsertPositionAdorner Control { get; }

    public static InsertionAdorner Create(UIElement adornedElement, bool isBottom)
    {
        var control = new InsertPositionAdorner();
        control.SetValue(HorizontalAlignmentProperty, HorizontalAlignment.Stretch);
        control.SetValue(VerticalAlignmentProperty, isBottom ? VerticalAlignment.Bottom : VerticalAlignment.Top);

        return new InsertionAdorner(adornedElement, control, control);
    }
}
