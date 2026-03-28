using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using VerticalTabControlLib;

namespace VerticalTabControl.UnitTests
{
    [TestClass]
    public class InsertPositionAdornerTests
    {
        [TestMethod]
        public void Constructor_WhenCalled_InitializesComponent()
        {
            // Arrange & Act
            var adorner = new InsertPositionAdorner();

            // Assert
            Assert.IsNotNull(adorner);
        }

        [TestMethod]
        public void Constructor_WhenCalledMultipleTimes_CreatesDistinctInstances()
        {
            // Arrange & Act
            var adorner1 = new InsertPositionAdorner();
            var adorner2 = new InsertPositionAdorner();

            // Assert
            Assert.IsNotNull(adorner1);
            Assert.IsNotNull(adorner2);
            Assert.AreNotSame(adorner1, adorner2);
        }

        [TestMethod]
        public void Constructor_WhenCalled_DoesNotThrow()
        {
            // Arrange & Act
            Exception? exception = null;
            InsertPositionAdorner? adorner = null;

            try
            {
                adorner = new InsertPositionAdorner();
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsNull(exception);
            Assert.IsNotNull(adorner);
        }
    }
}
