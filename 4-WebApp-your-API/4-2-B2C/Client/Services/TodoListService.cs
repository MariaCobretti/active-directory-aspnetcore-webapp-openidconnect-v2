// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Web;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TodoListService.Models;

namespace TodoListClient.Services
{
    public static class TodoListServiceExtensions
    {
        public static void AddTodoListService(this IServiceCollection services, IConfiguration configuration)
        {
            // https://docs.microsoft.com/en-us/dotnet/standard/microservices-architecture/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests
            services.AddHttpClient<ITodoListService, TodoListService>();
        }
    }

    /// <summary></summary>
    /// <seealso cref="TodoListClient.Services.ITodoListService" />
    public class TodoListService : ITodoListService
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly HttpClient _httpClient;
        private readonly string _TodoListScope = string.Empty;
        private readonly string _TodoListReadScope = string.Empty;
        private readonly string _TodoListBaseAddress = string.Empty;
        private readonly string _UserReadScope = "User.Read";
        private readonly string _DirectoryReadAllScope = "Directory.Read.All";
        private readonly ITokenAcquisition _tokenAcquisition;

        // having this dependency here is not so nice
        private readonly IMSGraphService _graphService;
        private readonly IAutomatedGraphClient _automatedGraphClient;

        public TodoListService(ITokenAcquisition tokenAcquisition, HttpClient httpClient, IConfiguration configuration, IHttpContextAccessor contextAccessor, IMSGraphService graphService, IAutomatedGraphClient automatedGraphClient)
        {
            _httpClient = httpClient;
            _tokenAcquisition = tokenAcquisition;
            _contextAccessor = contextAccessor;
            _graphService = graphService;
            _automatedGraphClient = automatedGraphClient;
            _TodoListScope = configuration["TodoList:TodoListScope"];
            _TodoListReadScope = configuration["TodoList:ReadScope"];
            _TodoListBaseAddress = configuration["TodoList:TodoListBaseAddress"];
        }

        public async Task<Todo> AddAsync(Todo todo)
        {
            await PrepareAuthenticatedClient();

            var jsonRequest = JsonConvert.SerializeObject(todo);
            var jsoncontent = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            var response = await this._httpClient.PostAsync($"{ _TodoListBaseAddress}/api/todolist", jsoncontent);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = await response.Content.ReadAsStringAsync();
                todo = JsonConvert.DeserializeObject<Todo>(content);

                return todo;
            }

            throw new HttpRequestException($"Invalid status code in the HttpResponseMessage: {response.StatusCode}.");
        }

        public async Task DeleteAsync(int id)
        {
            await PrepareAuthenticatedClient();

            var response = await this._httpClient.DeleteAsync($"{ _TodoListBaseAddress}/api/todolist/{id}");

            if (response.StatusCode == HttpStatusCode.OK)
            {
                return;
            }

            throw new HttpRequestException($"Invalid status code in the HttpResponseMessage: {response.StatusCode}.");
        }

        public async Task<Todo> EditAsync(Todo todo)
        {
            await PrepareAuthenticatedClient();

            var jsonRequest = JsonConvert.SerializeObject(todo);
            var jsoncontent = new StringContent(jsonRequest, Encoding.UTF8, "application/json-patch+json");

            var response = await _httpClient.PatchAsync($"{ _TodoListBaseAddress}/api/todolist/{todo.Id}", jsoncontent);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = await response.Content.ReadAsStringAsync();
                todo = JsonConvert.DeserializeObject<Todo>(content);

                return todo;
            }

            throw new HttpRequestException($"Invalid status code in the HttpResponseMessage: {response.StatusCode}.");
        }

        public async Task<IEnumerable<Todo>> GetAsync()
        {
            await PrepareAuthenticatedClient();
            
            var response = await _httpClient.GetAsync($"{ _TodoListBaseAddress}/api/todolist");
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = await response.Content.ReadAsStringAsync();
                IEnumerable<Todo> todolist = JsonConvert.DeserializeObject<IEnumerable<Todo>>(content);

                return todolist;
            }

            throw new HttpRequestException($"Invalid status code in the HttpResponseMessage: {response.StatusCode}.");
        }

        private async Task PrepareAuthenticatedClient()
        {
            
            var groups = await _automatedGraphClient.GetCurrentUsersGroupsAsync("a53c99d6-40a9-4f1f-b503-c16eb537cc2a");
            _contextAccessor.HttpContext.User.Identities.FirstOrDefault()?.AddClaims(groups.Select(group => new Claim(ClaimTypes.Role, group.DisplayName, ClaimValueTypes.String)));

            //var accessToken = await _tokenAcquisition.GetAccessTokenForUserAsync(new[] { _TodoListScope, _TodoListReadScope });

            var accessToken = await _tokenAcquisition.GetAccessTokenForUserAsync(new[] { _TodoListScope, _TodoListReadScope });
            //var graphAccessToken = await _tokenAcquisition.GetAccessTokenForUserAsync(new[] { _UserReadScope, _DirectoryReadAllScope });

            Debug.WriteLine($"access token-{accessToken}");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<Todo> GetAsync(int id)
        {
            await PrepareAuthenticatedClient();

            var response = await _httpClient.GetAsync($"{ _TodoListBaseAddress}/api/todolist/{id}");
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = await response.Content.ReadAsStringAsync();
                Todo todo = JsonConvert.DeserializeObject<Todo>(content);

                return todo;
            }

            throw new HttpRequestException($"Invalid status code in the HttpResponseMessage: {response.StatusCode}.");
        }
    }
}