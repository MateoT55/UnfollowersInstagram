using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Text.Json;
using UnfollowersInstagram.Services;

namespace UnfollowersInstagram.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UnfollowersController: ControllerBase
    {
        private const long MaxRequestBytes = 10_000_000;

        private readonly UnfollowersService _unfollowersService;

        public UnfollowersController(UnfollowersService unfollowersService)
        { 
            this._unfollowersService = unfollowersService;
        }



        [HttpPost]
        [EnableRateLimiting("scan")]
        [RequestSizeLimit(MaxRequestBytes)]
        public async Task<IActionResult> Scan([FromForm] IFormFile seguidores, [FromForm] IFormFile seguidos)
        {
            if (seguidores == null || seguidos == null)
            {
                return BadRequest("Ambos archivos JSON son obligatorios");
            }


            if (string.IsNullOrWhiteSpace(seguidores.FileName) ||
                string.IsNullOrWhiteSpace(seguidos.FileName) ||
                !Path.GetExtension(seguidores.FileName).Equals(".json", StringComparison.OrdinalIgnoreCase) ||
                !Path.GetExtension(seguidos.FileName).Equals(".json", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Solo se permiten archivos JSON");
            }

            if (seguidores.Length == 0 || seguidos.Length == 0)
            {
                return BadRequest("Los archivos no pueden estar vacios");
            }


            try
            {
                var resultado = await _unfollowersService.ScanAsync(seguidores, seguidos);
                return Ok(resultado);
            }


            catch (JsonException)
            {
                // No se refleja ex.Message: evita filtrar detalles internos del parser.
                return BadRequest("Error al procesar el archivo JSON. Verifica que sean los archivos exportados de Instagram.");
            }
        }
    }
}
