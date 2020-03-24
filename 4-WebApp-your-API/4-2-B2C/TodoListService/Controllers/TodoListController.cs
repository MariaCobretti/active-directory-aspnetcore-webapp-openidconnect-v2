/*
 The MIT License (MIT)

Copyright (c) 2018 Microsoft Corporation

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
 */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TodoListService.Infrastructure;
using TodoListService.Models;

namespace TodoListService.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class TodoListController : Controller
    {
        // In-memory TodoList
        private static readonly Dictionary<int, Todo> TodoStore = new Dictionary<int, Todo>();

        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IAuthorizationService _authorizationService;

        public TodoListController(IHttpContextAccessor contextAccessor, IAuthorizationService authorizationService)
        {
            _contextAccessor = contextAccessor;
            _authorizationService = authorizationService;

            // Pre-populate with sample data
            if (TodoStore.Count == 0)
            {
                TodoStore.Add(1, new Todo() { Id = 1, Owner = $"{this._contextAccessor.HttpContext.User.Identity.Name}", Title = "Pick up groceries", MoveInDate = "01.01.2020" });
                TodoStore.Add(2, new Todo() { Id = 2, Owner = $"{this._contextAccessor.HttpContext.User.Identity.Name}", Title = "Finish invoice report", MoveInDate = "02.01.2020" });
            }
        }

        // GET: api/values
        [HttpGet]
        [Authorize(Policy = "ReadScope")]
        [Authorize(Policy = "AdminOrDispatcher")]
        //[Authorize(Policy = Constants.AuthorizationPolicies.AssignmentToAdminGroupRequired)]
        //[Authorize(Policy = Constants.AuthorizationPolicies.AssignmentToDirectoryViewerRoleRequired)]
        public IEnumerable<Todo> Get()
        {
            var role = User.Claims;
            var admin = User.IsInRole("97e07631-02dc-49ed-ae6a-6fe93c68833c");
            string owner = User.Identity.Name;
            return TodoStore.Values.Where(x => x.Owner == owner);
        }

        // GET: api/values
        [HttpGet("{id}", Name = "Get")]
        //[Authorize(Policy = "ReadScope")]
        public async Task<Todo> Get(int id)
        {
            var todo = TodoStore.Values.FirstOrDefault(t => t.Id == id);
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, todo, "EditTaskPolicy");
            if (authorizationResult.Succeeded)
            {
                return todo;
            }
            else if (User.Identity.IsAuthenticated)
            {
                return null;
            }
            else
            {
                return null;
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "97e07631-02dc-49ed-ae6a-6fe93c68833c")]
        public void Delete(int id)
        {
            TodoStore.Remove(id);
        }

        // POST api/values
        [HttpPost]
        public IActionResult Post([FromBody] Todo todo)
        {
            int id = TodoStore.Values.OrderByDescending(x => x.Id).FirstOrDefault().Id + 1;
            Todo todonew = new Todo() { Id = id, Owner = HttpContext.User.Identity.Name, Title = todo.Title, MoveInDate = todo.MoveInDate };
            TodoStore.Add(id, todonew);

            return Ok(todo);
        }

        // PATCH api/values
        [HttpPatch("{id}")]
        public IActionResult Patch(int id, [FromBody] Todo todo)
        {
            if (id != todo.Id)
            {
                return NotFound();
            }

            if (TodoStore.Values.FirstOrDefault(x => x.Id == id) == null)
            {
                return NotFound();
            }

            TodoStore.Remove(id);
            TodoStore.Add(id, todo);

            return Ok(todo);
        }
    }
}