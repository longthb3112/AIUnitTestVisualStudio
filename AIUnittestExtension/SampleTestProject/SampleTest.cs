using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Moq;

namespace SampleUnitTest
{
    public class SampleTest
    {
        [Fact]
        public void Sum_ValidInput_ReturnsCorrectSum()
        {
            // Arrange
            var mockTest = new Mock<ITest>();
            mockTest.Setup(x => x.PlusOne(2)).Returns(3);
            var TestClass = new Sample(mockTest.Object);
            // Act
            int result = TestClass.Sum(2, 3);
            // Assert
            Assert.Equal(8, result);
            mockTest.Verify(x => x.PlusOne(2), Times.Once());
        }

        [Fact]
        public void Sum_ValidInput_ReturnsCorrectSum_WithZero()
        {
            // Arrange
            var mockTest = new Mock<ITest>();
            mockTest.Setup(x => x.PlusOne(0)).Returns(1);
            var TestClass = new Sample(mockTest.Object);
            // Act
            int result = TestClass.Sum(0, 0);
            // Assert
            Assert.Equal(1, result);
            mockTest.Verify(x => x.PlusOne(0), Times.Once());
        }

        [Fact]
        public void Sum_ValidInput_ReturnsCorrectSum_WithNegative()
        {
            // Arrange
            var mockTest = new Mock<ITest>();
            mockTest.Setup(x => x.PlusOne(-2)).Returns(-1);
            var TestClass = new Sample(mockTest.Object);
            // Act
            int result = TestClass.Sum(-2, 3);
            // Assert
            Assert.Equal(0, result);
            mockTest.Verify(x => x.PlusOne(-2), Times.Once());
        }

        [Fact]
        public void Sum_TestIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            var TestClass = new Sample(null);
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => TestClass.Sum(1, 2));
        }
    }
}