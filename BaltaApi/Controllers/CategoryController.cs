using BaltaApi.Data;
using BaltaApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BaltaApi.Controllers
{
    //EndPoint == URL
    //https://localhost:5001/categories
    [Route("v1/categories")]
    public class CategoryController : ControllerBase
    {
        private readonly BaltaContext _context;

        public CategoryController(BaltaContext context)
        {
            _context = context;
        }

        [HttpGet]
        [AllowAnonymous]
        [ResponseCache(VaryByHeader = "User-Agent", Location = ResponseCacheLocation.Any, Duration = 30)] //Ativa o chace por metodo, duração de 30 minutos
        //[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]            //Desativa o cache para metodo especifico, quando esse é habilitado globalmente no Startup.cs
        public async Task<ActionResult<List<Category>>> Get()
        {
            try
            {
                var List = await _context.Categories.AsNoTracking().ToListAsync();

                return Ok(List);
            }
            catch
            {
                return BadRequest(new { message = "Não foi possível retornar nenhuma categoria" });
            }
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("{id:int}")]
        public async Task<ActionResult<Category>> GetById(int id)
        {
            try
            {
                var obj = await _context.Categories.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);

                if (obj == null)
                    return NotFound(new { message = "Id não encontrado" });
                return Ok(obj);
            }
            catch
            {
                return BadRequest(new { message = "Não foi possível retornar a categoria" });
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Category>> Post([FromBody] Category obj)
        {
            if (!ModelState.IsValid) //Retorna as mensagens de erro, caso não esteja de acordo com os DataAnotations
            {
                return BadRequest(ModelState);
            }
                
            try //Tenta gravar o objeto no banco, caso contrario entra no erro
            {
                _context.Categories.Add(obj);
                await _context.SaveChangesAsync();

                return Ok(obj);
            }
            catch
            {
                return BadRequest(new { message = "Não foi possível criar a categoria" });
            }
        }

        [HttpPut]
        [Route("{id:int}")]
        [Authorize(Roles = "gerente")]
        public async Task<ActionResult<Category>> Put(int id, [FromBody] Category category)
        {
            //Verifica se o Id informado é o mesmo do objeto
            if (id != category.Id)
                return NotFound(new { message = "Categoria nao encontrada" });

            //Se o modelo não for válido de acordo com meus data anotation
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                _context.Entry(category).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return Ok(category);
            }
            catch
            {
                return BadRequest(new { message = "Não foi possível alterar a categoria" });
            }
        }

        [HttpDelete]
        [Route("{id:int}")]
        [Authorize(Roles = "gerente")]
        public async Task<ActionResult<Category>> Delete(int id)
        {
            var obj = await _context.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (obj == null)
            {
                return NotFound(new { message = "Id não encontrado" });
            }

            try
            {
                _context.Categories.Remove(obj);
                await _context.SaveChangesAsync();

                return Ok(obj);
            }
            catch
            {
                return BadRequest("Não foi possível remover");
            }
        } 
    }
}
