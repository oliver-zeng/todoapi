using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TodoApi.Models;

namespace TodoApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Consumes("application/json")]
    public class TodoController : ControllerBase
    {
        private readonly ILogger<TodoController> _logger;
        private readonly TodoApiDbContext _todoApiDbContext;

        public TodoController(ILogger<TodoController> logger, TodoApiDbContext todoApiDbContext)
        {
            _logger = logger;
            _todoApiDbContext = todoApiDbContext;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<TodoItem>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<TodoItem>>> GetAll()
        {
            _logger.LogInformation("Get All todo items");
            return await _todoApiDbContext.TodoItems
                .TagWith("Get All todo items")
                .ToListAsync();
        }

        [HttpGet("{id}", Name = "GetTodo")]
        [ProducesResponseType(typeof(TodoItem), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<TodoItem> GetById(int id)
        {
            _logger.LogInformation($"Get one todo item with id:{id}");
            var item = _todoApiDbContext.TodoItems
                .TagWith($"Get Todo with Id : {id}")
                .FirstOrDefault(todo => todo.Id == id);
            if (item == null)
            {
                _logger.LogInformation("Item not found !");
                return NotFound();
            }
            _logger.LogInformation("Found the item, returning it");
            return item;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Create(TodoItem todoItem)
        {
            _logger.LogInformation($"Creating todo with TodoItem - {todoItem}");
            var todoExists = await _todoApiDbContext.TodoItems
                .TagWith("Checking Description already exists.")
                .AnyAsync(t => t.Description == todoItem.Description);
            if (todoExists)
            {
                _logger.LogInformation($"Description already exists - returns a conflict.");
                return StatusCode(StatusCodes.Status409Conflict);
            }

            _todoApiDbContext.TodoItems.Add(todoItem);
            _todoApiDbContext.SaveChanges();
            _logger.LogInformation($"Saved to DB and now returns GetURL");
            return CreatedAtRoute("GetTodo", new { id = todoItem.Id }, todoItem);
        }


        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public IActionResult Update(int id, TodoItem todoItem)
        {
            _logger.LogInformation($"Updating todo");
            if (id != todoItem.Id)
            {
                _logger.LogInformation($"Ids not matching.");
                return BadRequest();
            }

            var todo = _todoApiDbContext.TodoItems.Find(id);
            if (todo == null)
            {
                _logger.LogInformation($"Todo not found !.");
                return NotFound();
            }

            todo.Description = todoItem.Description;
            todo.IsCompleted = todoItem.IsCompleted;

            _todoApiDbContext.SaveChanges();
            _logger.LogInformation($"Todo updated and saved to Database.");
            return NoContent();
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public IActionResult Delete(int id)
        {
            _logger.LogInformation($"Deleting a todo.");
            var todo = _todoApiDbContext.TodoItems.Find(id);
            if (todo == null)
            {
                _logger.LogInformation($"Todo not found.");
                return NotFound();
            }

            _todoApiDbContext.TodoItems.Remove(todo);
            _todoApiDbContext.SaveChanges();
            _logger.LogInformation($"Removed todo, updating Database.");
            return NoContent();
        }
    }
}