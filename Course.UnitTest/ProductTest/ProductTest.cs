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

        [Fact]
        public void DecreaseStock_WhenQuantityIsAvailable_ShouldUpdateStock() 
        {

            //Arrange
            var product = new Product(Guid.NewGuid(), "Teclado", 100m, 10);

            //Act
            product.DecreaseStock(3);

            //Assert
            product.Stock.Should().Be(7);
        
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
