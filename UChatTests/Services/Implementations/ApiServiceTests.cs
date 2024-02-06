using Microsoft.VisualStudio.TestTools.UnitTesting;
using UChat.Services.Implementations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using UChat.Services.Interfaces;
using UChat.ViewModels;
using Windows.Storage.Streams;

namespace UChat.Services.Implementations.Tests
{
    [TestClass()]
    public class ApiServiceTests
    {
        [TestMethod()]
        public async Task SendRequestAsyncTest()
        {
            // Arrange
            var secureHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            secureHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .Throws(new HttpRequestException("SSL certificate error"));

            var insecureHandlerMock = new Mock<HttpMessageHandler>();
            insecureHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent("{'key':'value'}")
                });

            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            httpClientFactoryMock.Setup(_ => _.CreateClient("SecureHttpClient")).Returns(new HttpClient(secureHandlerMock.Object));
            httpClientFactoryMock.Setup(_ => _.CreateClient("InsecureHttpClient")).Returns(new HttpClient(insecureHandlerMock.Object));

            var mockSettings = new Mock<ISettings>();
            mockSettings.Setup(s => s.ApiUrl).Returns("https://174.34.109.19:10051/api/v2.0/dictation");
            mockSettings.Setup(s => s.AcceptInsecureConnection).Returns(true);
            mockSettings.Setup(s => s.Timeout).Returns(10);
            mockSettings.Setup(s => s.HttpClientName).Returns("InsecureHttpClient");
            var settingsViewModel = new SettingsViewModel(mockSettings.Object);

            var apiService = new ApiService(httpClientFactoryMock.Object, mockSettings.Object);

            // Act
            Exception? exception = null;
            try
            {
                var mockBuffer = new Mock<IBuffer>();
                // Setup the mock buffer as needed
                string responseJson = await apiService.SendRequestAsync(mockBuffer.Object, "testfile");
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsNull(exception);
        }
    }
}