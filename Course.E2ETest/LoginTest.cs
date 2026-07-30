using Microsoft.Playwright.Xunit;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Course.E2ETest
{
    public class LoginTest : PageTest
    {

        private const string LoginUrl = "https://practicetestautomation.com/practice-test-login/";

        [Fact]
        public async Task UserCanLoginSuccesfully()
        {
            //Arrange
            await Page.GotoAsync(LoginUrl);

            //Act
            await Page.Locator("#username").FillAsync("student");
            await Page.Locator("#password").FillAsync("Password123");
            await Page.Locator("#submit").ClickAsync();


            //Assert
            await Expect(Page).ToHaveURLAsync(new Regex("logged-in-successfully"));

        }

        [Fact]
        public async Task LoginFailsWithInvalidUsername()
        {
            //Arrange
            await Page.GotoAsync(LoginUrl);

            //Act
            await Page.Locator("#username").FillAsync("incorrectUser");
            await Page.Locator("#password").FillAsync("Password123");
            await Page.Locator("#submit").ClickAsync();

            //Assert
            await Expect(Page).ToHaveURLAsync(new Regex("practice-test-login"));
            await Expect(Page.Locator("#error")).ToBeVisibleAsync();
            await Expect(Page.Locator("#error")).ToContainTextAsync("Your username is invalid!");
        }

        [Fact]
        public async Task LoginFailsWithInvalidPassword()
        {
            //Arrange
            await Page.GotoAsync(LoginUrl);

            //Act
            await Page.Locator("#username").FillAsync("student");
            await Page.Locator("#password").FillAsync("wrongPassword");
            await Page.Locator("#submit").ClickAsync();

            //Assert
            await Expect(Page).ToHaveURLAsync(new Regex("practice-test-login"));
            await Expect(Page.Locator("#error")).ToBeVisibleAsync();
            await Expect(Page.Locator("#error")).ToContainTextAsync("Your password is invalid!");
        }

        [Fact]
        public async Task LoginFailsWithEmptyFields()
        {
            //Arrange
            await Page.GotoAsync(LoginUrl);

            //Act
            await Page.Locator("#submit").ClickAsync();

            //Assert
            await Expect(Page).ToHaveURLAsync(new Regex("practice-test-login"));
            await Expect(Page.Locator("#error")).ToBeVisibleAsync();
            await Expect(Page.Locator("#error")).ToContainTextAsync("Your username is invalid!");
        }

        [Fact]
        public async Task SuccessfulLoginShowsWelcomeMessage()
        {
            //Arrange
            await Page.GotoAsync(LoginUrl);

            //Act
            await Page.Locator("#username").FillAsync("student");
            await Page.Locator("#password").FillAsync("Password123");
            await Page.Locator("#submit").ClickAsync();

            //Assert
            await Expect(Page).ToHaveURLAsync(new Regex("logged-in-successfully"));
            await Expect(Page.Locator("h1")).ToContainTextAsync("Logged In Successfully");
            await Expect(Page.Locator(".post-content")).ToContainTextAsync("Congratulations");
        }

    }
}
