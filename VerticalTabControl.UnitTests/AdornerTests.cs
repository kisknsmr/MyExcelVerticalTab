using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VerticalTabControlLib;

namespace VerticalTabControl.UnitTests;

[TestClass]
public class AdornerTests
{
    [TestMethod]
    public void GetVisualChild_Index0_ReturnsCore()
    {
        // Arrange
        var adornedElement = new TextBlock();
        var core = new TextBlock();
        var adorner = new TestAdorner(adornedElement, core);

        // Act
        var result = adorner.GetVisualChildPublic(0);

        // Assert
        Assert.AreSame(core, result);
    }

    [TestMethod]
    public void VisualChildrenCount_Always_ReturnsOne()
    {
        // Arrange
        var adornedElement = new TextBlock();
        var core = new TextBlock();
        var adorner = new TestAdorner(adornedElement, core);

        // Act
        var result = adorner.VisualChildrenCountPublic;

        // Assert
        Assert.AreEqual(1, result);
    }

    [TestMethod]
    public void MeasureOverride_ValidConstraint_MeasuresCoreAndReturnsBaseResult()
    {
        // Arrange
        var adornedElement = new TextBlock();
        var core = new TextBlock();
        var adorner = new TestAdorner(adornedElement, core);
        var constraint = new Size(100, 200);

        // Act
        var result = adorner.MeasureOverridePublic(constraint);

        // Assert
        Assert.IsGreaterThanOrEqualTo(result.Width, 0.0);
        Assert.IsGreaterThanOrEqualTo(result.Height, 0.0);
    }

    [TestMethod]
    public void MeasureOverride_ZeroConstraint_MeasuresCoreAndReturnsBaseResult()
    {
        // Arrange
        var adornedElement = new TextBlock();
        var core = new TextBlock();
        var adorner = new TestAdorner(adornedElement, core);
        var constraint = new Size(0, 0);

        // Act
        var result = adorner.MeasureOverridePublic(constraint);

        // Assert
        Assert.IsGreaterThanOrEqualTo(result.Width, 0.0);
        Assert.IsGreaterThanOrEqualTo(result.Height, 0.0);
    }

    [TestMethod]
    public void ArrangeOverride_ValidFinalSize_ArrangesCoreAndReturnsBaseResult()
    {
        // Arrange
        var adornedElement = new TextBlock();
        var core = new TextBlock();
        var adorner = new TestAdorner(adornedElement, core);
        var finalSize = new Size(150, 250);

        // Act
        var result = adorner.ArrangeOverridePublic(finalSize);

        // Assert
        Assert.AreEqual(finalSize.Width, result.Width);
        Assert.AreEqual(finalSize.Height, result.Height);
    }

    [TestMethod]
    public void ArrangeOverride_ZeroSize_ArrangesCoreAndReturnsBaseResult()
    {
        // Arrange
        var adornedElement = new TextBlock();
        var core = new TextBlock();
        var adorner = new TestAdorner(adornedElement, core);
        var finalSize = new Size(0, 0);

        // Act
        var result = adorner.ArrangeOverridePublic(finalSize);

        // Assert
        Assert.AreEqual(finalSize.Width, result.Width);
        Assert.AreEqual(finalSize.Height, result.Height);
    }

    [TestMethod]
    public void Dispose_DisposingTrue_SetsDisposedValueTrue()
    {
        // Arrange
        var adornedElement = new TextBlock();
        var core = new TextBlock();
        var adorner = new TestAdornerWithDisposedFlag(adornedElement, core);

        // Act
        adorner.DisposePublic(true);

        // Assert
        Assert.IsTrue(adorner.IsDisposed);
    }

    [TestMethod]
    public void Dispose_DisposingFalse_SetsDisposedValueTrue()
    {
        // Arrange
        var adornedElement = new TextBlock();
        var core = new TextBlock();
        var adorner = new TestAdornerWithDisposedFlag(adornedElement, core);

        // Act
        adorner.DisposePublic(false);

        // Assert
        Assert.IsTrue(adorner.IsDisposed);
    }

    [TestMethod]
    public void Dispose_CalledTwiceWithDisposingTrue_OnlyExecutesOnce()
    {
        // Arrange
        var adornedElement = new TextBlock();
        var core = new TextBlock();
        var adorner = new TestAdornerWithDisposeCounter(adornedElement, core);

        // Act
        adorner.DisposePublic(true);
        adorner.DisposePublic(true);

        // Assert
        Assert.AreEqual(1, adorner.DisposeCallCount);
    }

    [TestMethod]
    public void Dispose_CalledWithNullLayer_DoesNotThrow()
    {
        // Arrange
        var adornedElement = new TextBlock();
        var core = new TextBlock();
        var adorner = new TestAdorner(adornedElement, core);

        // Act & Assert - Should not throw
        adorner.DisposePublic(true);
    }

    [TestMethod]
    public void Dispose_CalledTwiceWithDisposingFalse_OnlyExecutesOnce()
    {
        // Arrange
        var adornedElement = new TextBlock();
        var core = new TextBlock();
        var adorner = new TestAdornerWithDisposeCounter(adornedElement, core);

        // Act
        adorner.DisposePublic(false);
        adorner.DisposePublic(false);

        // Assert
        Assert.AreEqual(1, adorner.DisposeCallCount);
    }

    [TestMethod]
    public void Dispose_MixedDisposingCalls_OnlyExecutesOnce()
    {
        // Arrange
        var adornedElement = new TextBlock();
        var core = new TextBlock();
        var adorner = new TestAdornerWithDisposeCounter(adornedElement, core);

        // Act
        adorner.DisposePublic(true);
        adorner.DisposePublic(false);

        // Assert
        Assert.AreEqual(1, adorner.DisposeCallCount);
    }

    private class TestAdorner : AdornerBase
    {
        public TestAdorner(UIElement adornedElement, UIElement core) 
            : base(adornedElement, core)
        {
        }

        public Visual GetVisualChildPublic(int index) => GetVisualChild(index);

        public int VisualChildrenCountPublic => VisualChildrenCount;

        public Size MeasureOverridePublic(Size constraint) => MeasureOverride(constraint);

        public Size ArrangeOverridePublic(Size finalSize) => ArrangeOverride(finalSize);

        public void DisposePublic(bool disposing) => Dispose(disposing);
    }

    private class TestAdornerWithDisposedFlag : AdornerBase
    {
        private bool _disposedValue;

        public TestAdornerWithDisposedFlag(UIElement adornedElement, UIElement core) 
            : base(adornedElement, core)
        {
        }

        public bool IsDisposed => _disposedValue;

        public void DisposePublic(bool disposing) => Dispose(disposing);

        protected override void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                _disposedValue = true;
            }
        }
    }

    private class TestAdornerWithDisposeCounter : AdornerBase
    {
        private bool _disposedValue;

        public TestAdornerWithDisposeCounter(UIElement adornedElement, UIElement core) 
            : base(adornedElement, core)
        {
        }

        public int DisposeCallCount { get; private set; }

        public void DisposePublic(bool disposing) => Dispose(disposing);

        protected override void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                DisposeCallCount++;
                _disposedValue = true;
            }
        }
    }

    [TestMethod]
    public void Dispose_Called_CallsDisposeTrue()
    {
        // Arrange
        var adornedElement = new TextBlock();
        var core = new TextBlock();
        var adorner = new TestAdornerWithDisposeCounter(adornedElement, core);

        // Act
        adorner.Dispose();

        // Assert
        Assert.AreEqual(1, adorner.DisposeCallCount);
    }

    [TestMethod]
    public void Dispose_CalledTwice_OnlyDisposesOnce()
    {
        // Arrange
        var adornedElement = new TextBlock();
        var core = new TextBlock();
        var adorner = new TestAdornerWithDisposeCounter(adornedElement, core);

        // Act
        adorner.Dispose();
        adorner.Dispose();

        // Assert
        Assert.AreEqual(1, adorner.DisposeCallCount);
    }

    [TestMethod]
    public void SetOffset_WithPositiveValues_SetsOffsetCorrectly()
    {
        // Arrange
        var adornedElement = new Button();
        var dragTarget = new Button { Content = "Test", Width = 100, Height = 50 };
        dragTarget.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        dragTarget.Arrange(new Rect(new Point(0, 0), dragTarget.DesiredSize));
        var startPosition = new Point(10, 20);
        var adorner = DragAdorner.Create(adornedElement, dragTarget, startPosition);

        // Act
        adorner.SetOffset(30, 40);

        // Assert
        Assert.AreEqual(20.0, adorner.OffsetX);
        Assert.AreEqual(20.0, adorner.OffsetY);
    }

    [TestMethod]
    public void SetOffset_WithNegativeValues_SetsOffsetCorrectly()
    {
        // Arrange
        var adornedElement = new Button();
        var dragTarget = new Button { Content = "Test", Width = 100, Height = 50 };
        dragTarget.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        dragTarget.Arrange(new Rect(new Point(0, 0), dragTarget.DesiredSize));
        var startPosition = new Point(50, 60);
        var adorner = DragAdorner.Create(adornedElement, dragTarget, startPosition);

        // Act
        adorner.SetOffset(20, 30);

        // Assert
        Assert.AreEqual(-30.0, adorner.OffsetX);
        Assert.AreEqual(-30.0, adorner.OffsetY);
    }

    [TestMethod]
    public void SetOffset_WithZeroValues_SetsOffsetCorrectly()
    {
        // Arrange
        var adornedElement = new Button();
        var dragTarget = new Button { Content = "Test", Width = 100, Height = 50 };
        dragTarget.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        dragTarget.Arrange(new Rect(new Point(0, 0), dragTarget.DesiredSize));
        var startPosition = new Point(10, 20);
        var adorner = DragAdorner.Create(adornedElement, dragTarget, startPosition);

        // Act
        adorner.SetOffset(0, 0);

        // Assert
        Assert.AreEqual(-10.0, adorner.OffsetX);
        Assert.AreEqual(-20.0, adorner.OffsetY);
    }

    [TestMethod]
    public void SetOffset_CalledMultipleTimes_UpdatesOffsetEachTime()
    {
        // Arrange
        var adornedElement = new Button();
        var dragTarget = new Button { Content = "Test", Width = 100, Height = 50 };
        dragTarget.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        dragTarget.Arrange(new Rect(new Point(0, 0), dragTarget.DesiredSize));
        var startPosition = new Point(10, 20);
        var adorner = DragAdorner.Create(adornedElement, dragTarget, startPosition);

        // Act
        adorner.SetOffset(30, 40);
        adorner.SetOffset(50, 60);

        // Assert
        Assert.AreEqual(40.0, adorner.OffsetX);
        Assert.AreEqual(40.0, adorner.OffsetY);
    }

    [TestMethod]
    public void GetDesiredTransform_WithNullTransform_ReturnsTransformGroup()
    {
        // Arrange
        var adornedElement = new Button();
        var dragTarget = new Button { Content = "Test", Width = 100, Height = 50 };
        dragTarget.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        dragTarget.Arrange(new Rect(new Point(0, 0), dragTarget.DesiredSize));
        var startPosition = new Point(10, 20);
        var adorner = DragAdorner.Create(adornedElement, dragTarget, startPosition);
        adorner.SetOffset(30, 40);

        // Act
        var result = adorner.GetDesiredTransform(null!);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType<GeneralTransformGroup>(result);
    }

    [TestMethod]
    public void GetDesiredTransform_WithTranslateTransform_ReturnsTransformGroup()
    {
        // Arrange
        var adornedElement = new Button();
        var dragTarget = new Button { Content = "Test", Width = 100, Height = 50 };
        dragTarget.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        dragTarget.Arrange(new Rect(new Point(0, 0), dragTarget.DesiredSize));
        var startPosition = new Point(10, 20);
        var adorner = DragAdorner.Create(adornedElement, dragTarget, startPosition);
        adorner.SetOffset(30, 40);
        var inputTransform = new TranslateTransform(5, 10);

        // Act
        var result = adorner.GetDesiredTransform(inputTransform);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType<GeneralTransformGroup>(result);
    }

    [TestMethod]
    public void GetDesiredTransform_WithZeroOffset_ReturnsTransformGroup()
    {
        // Arrange
        var adornedElement = new Button();
        var dragTarget = new Button { Content = "Test", Width = 100, Height = 50 };
        dragTarget.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        dragTarget.Arrange(new Rect(new Point(0, 0), dragTarget.DesiredSize));
        var startPosition = new Point(0, 0);
        var adorner = DragAdorner.Create(adornedElement, dragTarget, startPosition);

        // Act
        var result = adorner.GetDesiredTransform(null!);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType<GeneralTransformGroup>(result);
    }

    [TestMethod]
    public void Create_WithValidParameters_ReturnsNonNullDragAdorner()
    {
        // Arrange
        var adornedElement = new Button();
        var dragTarget = new Button { Content = "Test", Width = 100, Height = 50 };
        dragTarget.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        dragTarget.Arrange(new Rect(new Point(0, 0), dragTarget.DesiredSize));
        var startPosition = new Point(10, 20);

        // Act
        var result = DragAdorner.Create(adornedElement, dragTarget, startPosition);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType<DragAdorner>(result);
    }

    [TestMethod]
    public void Create_WithValidParameters_SetsStartPosition()
    {
        // Arrange
        var adornedElement = new Button();
        var dragTarget = new Button { Content = "Test", Width = 100, Height = 50 };
        dragTarget.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        dragTarget.Arrange(new Rect(new Point(0, 0), dragTarget.DesiredSize));
        var startPosition = new Point(10, 20);

        // Act
        var result = DragAdorner.Create(adornedElement, dragTarget, startPosition);

        // Assert
        Assert.AreEqual(0.0, result.OffsetX);
        Assert.AreEqual(0.0, result.OffsetY);
    }

    [TestMethod]
    public void Create_WithZeroStartPosition_ReturnsValidAdorner()
    {
        // Arrange
        var adornedElement = new Button();
        var dragTarget = new Button { Content = "Test", Width = 100, Height = 50 };
        dragTarget.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        dragTarget.Arrange(new Rect(new Point(0, 0), dragTarget.DesiredSize));
        var startPosition = new Point(0, 0);

        // Act
        var result = DragAdorner.Create(adornedElement, dragTarget, startPosition);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0.0, result.OffsetX);
        Assert.AreEqual(0.0, result.OffsetY);
    }

    [TestMethod]
    public void Create_WithNegativeStartPosition_ReturnsValidAdorner()
    {
        // Arrange
        var adornedElement = new Button();
        var dragTarget = new Button { Content = "Test", Width = 100, Height = 50 };
        dragTarget.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        dragTarget.Arrange(new Rect(new Point(0, 0), dragTarget.DesiredSize));
        var startPosition = new Point(-10, -20);

        // Act
        var result = DragAdorner.Create(adornedElement, dragTarget, startPosition);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0.0, result.OffsetX);
        Assert.AreEqual(0.0, result.OffsetY);
    }

    [TestMethod]
    public void InsertionAdorner_Create_WithIsBottomTrue_ReturnsNonNullAdorner()
    {
        // Arrange
        var adornedElement = new TextBlock();
        var isBottom = true;

        // Act
        var result = InsertionAdorner.Create(adornedElement, isBottom);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType<InsertionAdorner>(result);
    }

    [TestMethod]
    public void InsertionAdorner_Create_WithIsBottomTrue_SetsVerticalAlignmentToBottom()
    {
        // Arrange
        var adornedElement = new TextBlock();
        var isBottom = true;

        // Act
        var result = InsertionAdorner.Create(adornedElement, isBottom);

        // Assert
        Assert.IsNotNull(result.Control);
        Assert.AreEqual(VerticalAlignment.Bottom, result.Control.VerticalAlignment);
    }

    [TestMethod]
    public void InsertionAdorner_Create_WithIsBottomFalse_SetsVerticalAlignmentToTop()
    {
        // Arrange
        var adornedElement = new TextBlock();
        var isBottom = false;

        // Act
        var result = InsertionAdorner.Create(adornedElement, isBottom);

        // Assert
        Assert.IsNotNull(result.Control);
        Assert.AreEqual(VerticalAlignment.Top, result.Control.VerticalAlignment);
    }

    [TestMethod]
    public void InsertionAdorner_Create_SetsHorizontalAlignmentToStretch()
    {
        // Arrange
        var adornedElement = new TextBlock();
        var isBottom = true;

        // Act
        var result = InsertionAdorner.Create(adornedElement, isBottom);

        // Assert
        Assert.IsNotNull(result.Control);
        Assert.AreEqual(HorizontalAlignment.Stretch, result.Control.HorizontalAlignment);
    }
}
