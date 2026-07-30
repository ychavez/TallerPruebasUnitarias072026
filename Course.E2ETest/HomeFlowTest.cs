using Course.E2ETest.Infrastructure;
using Microsoft.Playwright;
using Microsoft.Playwright.Xunit;

namespace Course.E2ETest
{
    [Collection("HomeE2E")]
    public class HomeFlowTest : PageTest
    {
        private readonly string _homeUrl;

        public HomeFlowTest(WebAppFixture fixture)
        {
            _homeUrl = fixture.WebBaseUrl;
        }

        public override BrowserNewContextOptions ContextOptions()
        {
            return new BrowserNewContextOptions
            {
                RecordVideoDir = "videos/",
                RecordVideoSize = new RecordVideoSize { Width = 1280, Height = 720 },
                
            };
        }


        [Fact]
        public async Task HomeLoadsProductCatalogFromApi()
        {
            //Arrange
            await Page.GotoAsync(_homeUrl);

            //Act
            var productsTable = Page.Locator("[data-testid='products-table']");

            //Assert
            await Expect(productsTable).ToBeVisibleAsync();
            await Expect(Page.Locator("[data-testid='connection-status']")).ToContainTextAsync("Conectado");
        }

        [Fact]
        public async Task CreateOrderButtonIsDisabledWithoutQuantitySelected()
        {
            //Arrange
            await Page.GotoAsync(_homeUrl);
            await Expect(Page.Locator("[data-testid='products-table']")).ToBeVisibleAsync();

            //Act
            var createOrderButton = Page.Locator("[data-testid='create-order']");

            //Assert
            await Expect(createOrderButton).ToBeDisabledAsync();
        }

        [Fact]
        public async Task UserCanCreateOrderSuccessfully()
        {
            //Arrange
            await Page.GotoAsync(_homeUrl);
            await Expect(Page.Locator("[data-testid='products-table']")).ToBeVisibleAsync();

            //Act
            await Page.Locator("[data-testid='quantity-mechanical-keyboard']").FillAsync("2");
            await Page.Locator("[data-testid='create-order']").ClickAsync();

            //Assert
            await Expect(Page.Locator("[data-testid='message-success']")).ToContainTextAsync("Pedido creado correctamente.");
            // El FakePaymentGateway aprueba automaticamente pagos <= 1000, por lo que el pedido queda "Paid".
            await Expect(Page.Locator("[data-testid='order-status']")).ToContainTextAsync("Paid");
        }

        [Fact]
        public async Task UserCanLookupExistingOrder()
        {
            //Arrange
            await Page.GotoAsync(_homeUrl);
            await Expect(Page.Locator("[data-testid='products-table']")).ToBeVisibleAsync();
            await Page.Locator("[data-testid='quantity-ergonomic-mouse']").FillAsync("1");
            await Page.Locator("[data-testid='create-order']").ClickAsync();
            await Expect(Page.Locator("[data-testid='message-success']")).ToContainTextAsync("Pedido creado correctamente.");

            



            var createdOrderId = await Page.Locator("[data-testid='order-id']").InnerTextAsync();

            //Act
            await Page.Locator("[data-testid='lookup-order-id']").FillAsync(createdOrderId);
            await Page.Locator("[data-testid='lookup-order']").ClickAsync();

            //Assert
            await Expect(Page.Locator("[data-testid='message-success']")).ToContainTextAsync("Pedido consultado correctamente.");
            await Expect(Page.Locator("[data-testid='order-id']")).ToContainTextAsync(createdOrderId);
        }

        [Fact]
        public async Task LookupOrderFailsWithInvalidId()
        {
            //Arrange
            await Page.GotoAsync(_homeUrl);
            await Expect(Page.Locator("[data-testid='products-table']")).ToBeVisibleAsync();

            //Act
            await Page.Locator("[data-testid='lookup-order-id']").FillAsync("no-es-un-guid");
            await Page.Locator("[data-testid='lookup-order']").ClickAsync();

            //Assert
            await Expect(Page.Locator("[data-testid='message-error']")).ToContainTextAsync("El order id no es valido.");
        }

        [Fact]
        public async Task UserCanCancelCreatedOrder()
        {
            //Arrange
            await Page.GotoAsync(_homeUrl);
            await Expect(Page.Locator("[data-testid='products-table']")).ToBeVisibleAsync();
            await Page.Locator("[data-testid='quantity-developer-laptop']").FillAsync("1");
            await Page.Locator("[data-testid='create-order']").ClickAsync();
            await Expect(Page.Locator("[data-testid='message-success']")).ToContainTextAsync("Pedido creado correctamente.");

            //Act
            await Page.Locator("[data-testid='cancel-order']").ClickAsync();

            //Assert
            await Expect(Page.Locator("[data-testid='message-success']")).ToContainTextAsync("Pedido cancelado correctamente.");
            await Expect(Page.Locator("[data-testid='order-status']")).ToContainTextAsync("Cancel");
        }
    }
}
