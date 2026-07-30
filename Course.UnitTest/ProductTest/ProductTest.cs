using Course.Domain.Entities;
using Course.Domain.Exceptions;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Course.UnitTest.ProductTest
{
    public class ProductTest
    {

        [Theory]
        [InlineData("Teclado", 10)]
        [InlineData("Mouse", 5)]
        public void DecreaseStock_WhenQuantityIsAvailable_ShouldUpdateStock(string producto, int cantidad) 
        {

            //Arrange
            var product = new Product(Guid.NewGuid(), producto, 100m, cantidad);

            //Act
            product.DecreaseStock(3);

            //Assert
            product.Stock.Should().Be(cantidad - 3);
        
        }



        [Fact]
        public void DecreaseStock_WhenQuantityExceedsStock_ShouldThrowDomainException() 
        {
            //Arrange
            var product = new Product(Guid.NewGuid(), "Teclado", 100m, 2);

            //Act
            var act = () => product.DecreaseStock(3);

            //Assert
            act.Should().Throw<InsufficientStockException>().WithMessage("*Solicitado: 3, disponible: 2*");

        }


    }
}
