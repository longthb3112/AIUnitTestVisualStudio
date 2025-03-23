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
        public void Sum_ValidInputs_ReturnsCorrectSum()
        {
            // Arrange
            var mockTest = new Mock<ITest>();
            mockTest.Setup(m => m.PlusOne(It.IsAny<int>())).Returns(1);
            var sample = new Sample(mockTest.Object);
            int a = 2;
            int b = 3;
            // Act
            var result = sample.Sum(a, b);
            // Assert
            Assert.Equal(6, result); // 2 + 3 + 1 = 6
        }

        [Fact]
        public void Sum_NullTest_ThrowsArgumentNullException()
        {
            // Arrange
            var sample = new Sample(null);
            int a = 2;
            int b = 3;
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => sample.Sum(a, b));
            Assert.NotNull(exception);
        }

        [Fact]
        public void Sum_ZeroInputs_ReturnsCorrectSum()
        {
            // Arrange
            var mockTest = new Mock<ITest>();
            mockTest.Setup(m => m.PlusOne(It.IsAny<int>())).Returns(1);
            var sample = new Sample(mockTest.Object);
            int a = 0;
            int b = 0;
            // Act
            var result = sample.Sum(a, b);
            // Assert
            Assert.Equal(1, result); // 0 + 0 + 1 = 1
        }

        [Fact]
        public void Sum_NegativeInputs_ReturnsCorrectSum()
        {
            // Arrange
            var mockTest = new Mock<ITest>();
            mockTest.Setup(m => m.PlusOne(It.IsAny<int>())).Returns(1);
            var sample = new Sample(mockTest.Object);
            int a = -2;
            int b = -3;
            // Act
            var result = sample.Sum(a, b);
            // Assert
            Assert.Equal(-4, result); // -2 + -3 + 1 = -4
        }
    }
}