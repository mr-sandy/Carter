﻿namespace Carter.Tests
{
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.TestHost;
    using Xunit;

    public class CarterModuleTests
    {
        public CarterModuleTests()
        {
            this.server = new TestServer(new WebHostBuilder()
                .ConfigureServices(x => { x.AddCarter(); })
                .Configure(x => x.UseCarter())
            );
            this.httpClient = this.server.CreateClient();
        }

        private readonly TestServer server;

        private readonly HttpClient httpClient;

        [Theory]
        [InlineData("/multiquerystring?id=1&id=2")]
        [InlineData("/multiquerystring?id=1,2")]
        public async Task Should_return_GET_requests_with_multiple_parsed_querystring(string url)
        {
            var response = await this.httpClient.GetAsync(url);

            var body = await response.Content.ReadAsStringAsync();

            Assert.Equal(200, (int)response.StatusCode);
            Assert.True(body.Contains("Managed to parse multiple ints 2"));
        }

        [Theory]
        [InlineData("/nullablemultiquerystring?id=1&id=2")]
        [InlineData("/nullablemultiquerystring?id=1,2")]
        public async Task Should_return_GET_requests_with_multiple_parsed_querystring_with_nullable_parameters(string url)
        {
            var response = await this.httpClient.GetAsync(url);

            var body = await response.Content.ReadAsStringAsync();

            Assert.Equal(200, (int)response.StatusCode);
            Assert.True(body.Contains("Managed to parse multiple Nullable<int>s 2"));
        }

        [Theory]
        [InlineData("/405test")]
        [InlineData("/405test/")]
        [InlineData("/405testwithslash")]
        [InlineData("/405testwithslash/")]
        public async Task Should_return_405_if_path_not_found_for_supplied_method(string path)
        {
            var response = await this.httpClient.PostAsync(path, new StringContent(""));

            Assert.Equal(405, (int)response.StatusCode);
        }

        [Theory]
        [InlineData("/parameterized/foo", "Beforeecho fooAfter")]
        [InlineData("/parameterized/bar", "Beforeecho barAfter")]
        [InlineData("/parameterized/e3c6af72-9cb7-4638-b3ea-4e4705f96cea", "Beforeecho e3c6af72-9cb7-4638-b3ea-4e4705f96ceaAfter")]
        [InlineData("/parameterized/911", "Beforeecho 911After")]
        public async Task Should_parameterized_route_work(string route, string content)
        {
            var result = await this.httpClient.GetAsync(route);

            Assert.Equal(200, (int)result.StatusCode);
            Assert.Equal(content, await result.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Should_handle_module_after_hook()
        {
            var response = await this.httpClient.GetAsync("/");
            var body = await response.Content.ReadAsStringAsync();
            Assert.True(body.Contains("After"));
        }

        [Fact]
        public async Task Should_handle_module_before_hook()
        {
            var response = await this.httpClient.GetAsync("/");
            var body = await response.Content.ReadAsStringAsync();
            Assert.True(body.Contains("Before"));
        }

        [Fact]
        public async Task Should_handle_short_circuit_response_in_before_hook()
        {
            var response = await this.httpClient.GetAsync("/noaccess");
            var body = await response.Content.ReadAsStringAsync();
            Assert.Equal("NoAccessBefore", body);
        }

        [Fact]
        public async Task Should_parameterized_route_return_405()
        {
            var result = await this.httpClient.PostAsync("/parameterized/foo", new StringContent(""));

            Assert.Equal(405, (int)result.StatusCode);
        }

        [Fact]
        public async Task Should_return_404_if_path_not_found()
        {
            var response = await this.httpClient.GetAsync("/flibbertygibbert");

            Assert.Equal(404, (int)response.StatusCode);
        }

        [Fact]
        public async Task Should_return_405_only_for_unconfigured_verbs()
        {
            var getResult = await this.httpClient.GetAsync("/405multiple");
            var postResult = await this.httpClient.PostAsync("/405multiple", new StringContent(""));
            var putResult = await this.httpClient.PutAsync("/405multiple", new StringContent(""));

            Assert.Equal(200, (int)getResult.StatusCode);
            Assert.Equal("Before405multiple-getAfter", await getResult.Content.ReadAsStringAsync());
            Assert.Equal(200, (int)postResult.StatusCode);
            Assert.Equal("Before405multiple-postAfter", await postResult.Content.ReadAsStringAsync());
            Assert.Equal(405, (int)putResult.StatusCode);
        }

        [Fact]
        public async Task Should_return_DELETE_requests()
        {
            var response = await this.httpClient.DeleteAsync("/");

            Assert.Equal(200, (int)response.StatusCode);
        }

        [Fact]
        public async Task Should_return_DELETE_requests_with_base_path()
        {
            var response = await this.httpClient.DeleteAsync("/test/");

            Assert.Equal(200, (int)response.StatusCode);
        }

        [Fact]
        public async Task Should_return_GET_requests()
        {
            var response = await this.httpClient.GetAsync("/");

            Assert.Equal(200, (int)response.StatusCode);
        }

        [Fact]
        public async Task Should_return_GET_requests_with_base_path()
        {
            var response = await this.httpClient.GetAsync("/test/");

            Assert.Equal(200, (int)response.StatusCode);
        }

        [Fact]
        public async Task Should_return_GET_requests_with_parsed_quersytring_with_default_value()
        {
            var response = await this.httpClient.GetAsync("querystringdefault");

            var body = await response.Content.ReadAsStringAsync();

            Assert.Equal(200, (int)response.StatusCode);
            Assert.True(body.Contains("Managed to parse default int 69"));
        }

        [Fact]
        public async Task Should_return_GET_requests_with_parsed_querystring()
        {
            const int idToTest = 69;
            var response = await this.httpClient.GetAsync($"/querystring?id={idToTest}");

            var body = await response.Content.ReadAsStringAsync();

            Assert.Equal(200, (int)response.StatusCode);
            Assert.True(body.Contains($"Managed to parse an int {idToTest}"));
        }

        [Fact]
        public async Task Should_return_GET_requests_with_parsed_querystring_with_nullable_parameter()
        {
            const int idToTest = 69;
            var response = await this.httpClient.GetAsync($"/nullablequerystring?id={idToTest}");

            var body = await response.Content.ReadAsStringAsync();

            Assert.Equal(200, (int)response.StatusCode);
            Assert.True(body.Contains($"Managed to parse a Nullable<int> {idToTest}"));
        }

        [Fact]
        public async Task Should_return_HEAD_requests()
        {
            var response = await this.httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, "/head"));

            Assert.Equal(200, (int)response.StatusCode);
        }

        [Fact]
        public async Task Should_return_HEAD_requests_for_defined_GET_routes()
        {
            var response = await this.httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, "/"));

            Assert.Equal(200, (int)response.StatusCode);
        }

        [Fact]
        public async Task Should_return_HEAD_requests_for_defined_GET_routes_with_base_path()
        {
            var response = await this.httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, "/test/"));

            Assert.Equal(200, (int)response.StatusCode);
        }

        [Fact]
        public async Task Should_return_HEAD_requests_with_base_path()
        {
            var response = await this.httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, "/test/head/"));

            Assert.Equal(200, (int)response.StatusCode);
        }

        [Fact]
        public async Task Should_return_OPTIONS_requests()
        {
            var response = await this.httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Options, "/"));

            Assert.Equal(200, (int)response.StatusCode);
        }

        [Fact]
        public async Task Should_return_OPTIONS_requests_with_base_path()
        {
            var response = await this.httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Options, "/test/"));

            Assert.Equal(200, (int)response.StatusCode);
        }

        [Fact]
        public async Task Should_return_PATCH_requests()
        {
            var response = await this.httpClient.SendAsync(new HttpRequestMessage(new HttpMethod("PATCH"), "/"));
            Assert.Equal(200, (int)response.StatusCode);
        }

        [Fact]
        public async Task Should_return_PATCH_requests_with_base_path()
        {
            var response = await this.httpClient.SendAsync(new HttpRequestMessage(new HttpMethod("PATCH"), "/test/"));
            Assert.Equal(200, (int)response.StatusCode);
        }

        [Fact]
        public async Task Should_return_POST_request_body_AsString()
        {
            const string content = "Hello";

            var response = await this.httpClient.PostAsync("/asstring", new StringContent(content));

            var body = await response.Content.ReadAsStringAsync();

            Assert.Equal(200, (int)response.StatusCode);
            Assert.True(body.Contains(content));
        }

        [Fact]
        public async Task Should_return_POST_request_body_AsStringAsync()
        {
            const string content = "Hello";

            var response = await this.httpClient.PostAsync("/asstringasync", new StringContent(content));

            var body = await response.Content.ReadAsStringAsync();

            Assert.Equal(200, (int)response.StatusCode);
            Assert.True(body.Contains(content));
        }

        [Fact]
        public async Task Should_return_POST_requests()
        {
            var response = await this.httpClient.PostAsync("/", new StringContent(""));

            Assert.Equal(200, (int)response.StatusCode);
        }

        [Fact]
        public async Task Should_return_POST_requests_with_base_path()
        {
            var response = await this.httpClient.PostAsync("/test/", new StringContent(""));

            Assert.Equal(200, (int)response.StatusCode);
        }

        [Fact]
        public async Task Should_return_PUT_requests()
        {
            var response = await this.httpClient.PutAsync("/", new StringContent(""));

            Assert.Equal(200, (int)response.StatusCode);
        }

        [Fact]
        public async Task Should_return_PUT_requests_with_base_path()
        {
            var response = await this.httpClient.PutAsync("/test/", new StringContent(""));

            Assert.Equal(200, (int)response.StatusCode);
        }
    }
}
