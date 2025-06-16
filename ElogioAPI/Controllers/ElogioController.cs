using ElogioAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Supabase;

namespace ElogioAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ElogioController : ControllerBase
    {
        private readonly Client _supabase;
        private static readonly Random _random = new();

        public ElogioController(Client supabase)
        {
            _supabase = supabase;
        }

        /// <summary>
        /// Busca um elogio aleatório no banco de dados.
        /// </summary>
        /// <returns>Um objeto JSON com os dados do elogio.</returns>
        [HttpGet("elogio")]
        [ProducesResponseType(200, Type = typeof(Elogio))]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetElogioAleatorio()
        {
            try
            {
                // 1. Busca um elogio aleatório
                var totalElogios = await _supabase.From<Elogio>().Count(Postgrest.Constants.CountType.Exact);
                if (totalElogios == 0)
                {
                    return NotFound(new { message = "Nenhum elogio encontrado." });
                }

                var elogioOffset = _random.Next(0, (int)totalElogios);
                var elogioResponse = await _supabase.From<Elogio>().Limit(1).Offset(elogioOffset).Get();
                var elogio = elogioResponse.Model;

                if (elogio == null)
                {
                    return NotFound(new { message = "Não foi possível buscar um elogio." });
                }

                // 2. Registra o log de uso para o elogio
                _ = Task.Run(async () =>
                {
                    var log = new LogUso
                    {
                        ElogioId = elogio.Id,
                        // FotoId será null, pois esta chamada é apenas para o elogio
                        DataUso = DateTime.UtcNow
                    };
                    await _supabase.From<LogUso>().Insert(log);
                });

                // 3. Retorna o elogio como JSON
                // 3. Retorna um NOVO objeto anônimo, limpo, apenas com os dados necessários
                return Ok(new
                {
                    id = elogio.Id,
                    texto = elogio.Texto
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Ocorreu um erro interno: {ex.Message}" });
            }
        }

        /// <summary>
        /// Busca uma foto aleatória e retorna o arquivo da imagem.
        /// </summary>
        /// <returns>O arquivo de imagem (ex: image/jpeg).</returns>
        [HttpGet("foto")]
        [ProducesResponseType(200, Type = typeof(FileContentResult))]
        [ProducesResponseType(404)]


        public async Task<IActionResult> GetFotoAleatoria()
        {
            try
            {
                // 1. Busca uma foto aleatória
                var totalFotos = await _supabase.From<Foto>().Count(Postgrest.Constants.CountType.Exact);
                if (totalFotos == 0)
                {
                    return NotFound(new { message = "Nenhuma foto encontrada." });
                }

                var fotoOffset = _random.Next(0, (int)totalFotos);
                var fotoResponse = await _supabase.From<Foto>().Limit(1).Offset(fotoOffset).Get();
                var foto = fotoResponse.Model;

                if (foto == null)
                {
                    return NotFound(new { message = "Não foi possível buscar uma foto." });
                }

                // 2. Baixa o arquivo da imagem do Supabase Storage
                var imagemBytes = await _supabase.Storage.From("tetefotos").Download(foto.NomeArquivo, null);

                if (imagemBytes == null || imagemBytes.Length == 0)
                {
                    return NotFound(new { message = $"Arquivo de imagem '{foto.NomeArquivo}' não encontrado no Storage." });
                }

                // 3. Registra o log de uso para a foto
                _ = Task.Run(async () =>
                {
                    var log = new LogUso
                    {
                        FotoId = foto.Id,
                        // ElogioId será null, pois esta chamada é apenas para a foto
                        DataUso = DateTime.UtcNow
                    };
                    await _supabase.From<LogUso>().Insert(log);
                });

                // 4. Retorna o arquivo da imagem
                var contentType = GetMimeType(foto.NomeArquivo);
                return File(imagemBytes, contentType);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Ocorreu um erro interno: {ex.Message}" });
            }
        }

        private string GetMimeType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                _ => "application/octet-stream",
            };
        }
    }
}