using BaltaApi.Data;
using BaltaApi.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BaltaApi.Controllers
{
    [Route("v1")]
    public class HomeController : Controller
    {
        private readonly BaltaContext _context;
        public HomeController(BaltaContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<dynamic>> Get()
        {
            var funcionario = new User
            { 
                Id = 1, 
                Username = "Deivid", 
                Password = "qwe123", 
                Role = "funcionario" 
            };

            var gerente = new User 
            { 
                Id = 2, 
                Username = "Junin", 
                Password = "qwe123asdzxc", 
                Role = "gerente" 
            };

            var categoria = new Category
            {
                Id = 1,
                Title = "Informática"
            };

            var produto = new Product
            {
                Id = 1,
                Title = "Mouse",
                Category = categoria,
                Price = 299,
                Description = "Mouse gamer"
            };

            _context.Users.AddRange(funcionario, gerente);
            _context.Categories.Add(categoria);
            _context.Products.Add(produto);

            await _context.SaveChangesAsync();

            return Ok(new { message = "Dados configurados" });
        }
    }
}
