using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using BaltaApi.Services;
using Microsoft.AspNetCore.Routing;
using BaltaApi.Models;
using BaltaApi.Data;
using Microsoft.EntityFrameworkCore;

namespace BaltaApi.Controllers
{
    [Route("v1/users")]
    public class UserController : Controller
    {
        private readonly BaltaContext _context;

        public UserController(BaltaContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "gerente")]
        public async Task<ActionResult<List<User>>> Get()
        {
            var lista = await _context.Users
                .AsNoTracking()
                .ToListAsync();

            if (lista == null)
                return NotFound(new { message = "Não foi encontrado nenhum aluno" });

            return Ok(lista);
        }

        [HttpGet]
        [Route("{id:int}")]
        [Authorize(Roles = "gerente")]
        public async Task<ActionResult<User>> GetById(int id)
        {
            try
            {
                var obj = await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (obj == null)
                    return NotFound(new { message = "objeto não encontrado" });

                return Ok(obj); 
            }
            catch (Exception)
            {
                return BadRequest(new { message = "Não foi possível retornar o usuário" });
            } 
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<dynamic>> Post(
            [FromBody] User model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                //Força o usuario a sempre ser um funcionário
                model.Role = "funcionário";

                _context.Users.Add(model);
                await _context.SaveChangesAsync();

                //Esconde a senha para não retorna-la para o usuário
                model.Password = "";

                return model;
            }
            catch (Exception)
            {
                return BadRequest(new { message = "Não foi possível criar o usuário" });
            }
        }

        [HttpPut]
        [Route("{id:int}")]
        [Authorize(Roles = "gerente")]
        public async Task<ActionResult<User>> Put([FromBody] User model, int id)
        {
            if (model.Id != id)
                return NotFound(new { message = "Usuário não encontrado"});

            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                _context.Entry(model).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok(model);
            }
            catch(Exception e)
            {
                return BadRequest(new { message = "Não foi possível alterar o usuário" });
            }
        }

        [HttpDelete]
        [Route("{id:int}")]
        [Authorize(Roles = "gerente")]

        public async Task<ActionResult<User>> Delete(int id)
        {
            var obj = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (obj == null)
                return NotFound(new { message = "Não foi possível retornar o usuário" });

            try
            {
                _context.Users.Remove(obj);
                await _context.SaveChangesAsync();

                return Ok(obj);
            }
            catch (Exception)
            {
                return BadRequest("Não foi possível remover");
            }
        } 

        [HttpPost]
        [Route("login")] 
        public async Task<ActionResult<dynamic>> Authenticate(
            [FromBody] User model)
        {
            var user = await _context.Users
                .AsNoTracking()
                .Where(x => x.Username == model.Username && x.Password == model.Password)
                .FirstOrDefaultAsync();

            if (user == null)
                return NotFound(new { message = "Usuário ou senha inválidos" });

            var token = TokenService.GenerateToken(user);

            //Esconde a senha
            user.Password = "";

            return new
            {
                user = user,
                token = token
            };
        }
    }
}
