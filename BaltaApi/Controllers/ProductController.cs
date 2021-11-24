using BaltaApi.Data;
using BaltaApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BaltaApi.Controllers
{
    [Route("v1/products")]
    public class ProductController : Controller
    {
        private readonly BaltaContext _context;

        public ProductController(BaltaContext context)
        {
            _context = context;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<List<Product>>> Get()
        {
            try
            {
                var Lista = await _context
                    .Products
                    .Include(x => x.Category)
                    .AsNoTracking()
                    .ToListAsync();

                return Ok(Lista);
            }
            catch (Exception e)
            {
                return BadRequest(e.InnerException.Message);
            }
        }

        [HttpGet]
        [Route("{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<Product>> GetById(int id)
        {
            try
            {
                var produto = await _context
                    .Products
                    .Include(x => x.Category)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (produto == null)
                    return NotFound(new { message = "Produto não encontrado" });

                return Ok(produto);
            }
            catch
            {
                return BadRequest();
            }
        }

        [HttpGet]
        [Route("categories/{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<List<Product>>> GetByCategorie(int id)
        {
            try
            {
                var List = await _context
                    .Products
                    .Include(x => x.Category)
                    .AsNoTracking()
                    .Where(x => x.CategoryId == id)
                    .ToListAsync();

                if (List == null)
                    return NotFound();

                return Ok(List);
            }
            catch
            {
                return BadRequest();
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Product>> Post
            ([FromBody] Product product)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                return Ok(product);
            }
            catch(Exception e)
            {
                return BadRequest(e.InnerException);
            }
        }

        [HttpPut]
        [Route("{id:int}")]
        [Authorize(Roles = "gerente")]
        public async Task<ActionResult<Product>> Put([FromBody] Product product, int id)
        {
            if (product.Id != id)
                return NotFound(new { message = "Produto não encontrado" });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                _context.Entry(product).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok(product);
            }
            catch (Exception)
            { 
                return BadRequest(new { message = "Não foi possível alterar o produto" }); 
            }
        }

        [HttpDelete]
        [Route("{id:int}")]
        [Authorize(Roles = "gerente")]
        public async Task<ActionResult<Product>> Delete(int id)
        {
            var obj = await _context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (obj == null)
                return NotFound(new { message = "Produto não encontrado" });

            try
            {
                _context.Products.Remove(obj);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Objeto removido com sucesso" });

            }
            catch (Exception)
            {
                return BadRequest("Não foi possível remover");
            }
        } 
    }
}
