using ElogioAPI.DTOs;
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
        /// Adiciona um novo elogio ao banco de dados.
        /// </summary>
        [HttpPost("elogio")]
        [ProducesResponseType(201, Type = typeof(Elogio))]
        public async Task<IActionResult> AdicionarElogio([FromBody] AdicionarElogioDto novoElogioDto)
        {
            var elogio = new Elogio
            {
                Texto = novoElogioDto.Texto,
                DataElogio = novoElogioDto.DataElogio
                // data_upload é preenchido automaticamente pelo banco
            };

            var resposta = await _supabase.From<Elogio>().Insert(elogio);
            var elogioCriado = resposta.Model;

            var resultadoDto = new ElogioResultadoDto
            {
                Id = elogioCriado.Id,
                Texto = elogioCriado.Texto,
                DataElogio = elogioCriado.DataElogio
                // Não incluímos as outras propriedades internas do BaseModel
            };

            // Agora, retornamos o DTO limpo no corpo da resposta
            return CreatedAtAction(nameof(GetElogioPorId), new { id = resultadoDto.Id }, resultadoDto);
        }

        /// <summary>
        /// Busca um elogio específico pelo ID.
        /// </summary>
        [HttpGet("elogio/{id}")]
        public async Task<IActionResult> GetElogioPorId(long id)
        {
            var resposta = await _supabase.From<Elogio>().Where(e => e.Id == id).Single();
            if (resposta == null) return NotFound();
            return Ok(new { id = resposta.Id, texto = resposta.Texto });
        }


        /// <summary>
        /// Adiciona uma nova foto, com data e link opcional para um elogio.
        /// </summary>
        [HttpPost("foto")]
        [ProducesResponseType(201, Type = typeof(Foto))]
        // VERSÃO NOVA E CORRIGIDA USANDO O DTO
        public async Task<IActionResult> AdicionarFoto([FromForm] AdicionarFotoDto fotoDto)
        {
            if (fotoDto.Arquivo == null || fotoDto.Arquivo.Length == 0)
                return BadRequest("Nenhum arquivo enviado.");

            // Gera um nome de arquivo único para evitar conflitos
            var nomeArquivoUnico = $"{Guid.NewGuid()}{Path.GetExtension(fotoDto.Arquivo.FileName)}";

            using var memoryStream = new MemoryStream();
            await fotoDto.Arquivo.CopyToAsync(memoryStream);
            var bytesDoArquivo = memoryStream.ToArray();

            // Envia para o Supabase Storage
            await _supabase.Storage.From("tetefotos").Upload(bytesDoArquivo, nomeArquivoUnico);

            // Salva a referência no banco de dados
            var registroFoto = new Foto
            {
                NomeArquivo = nomeArquivoUnico,
                DataFoto = fotoDto.DataFoto,
                ElogioLinkadoId = fotoDto.ElogioLinkadoId
            };

            var resposta = await _supabase.From<Foto>().Insert(registroFoto);
            var fotoCriada = resposta.Model;

            // Retornamos apenas os dados essenciais
            return Ok(new
            {
                id = fotoCriada.Id,
                nomeArquivo = fotoCriada.NomeArquivo,
                dataFoto = fotoCriada.DataFoto,
                elogioLinkadoId = fotoCriada.ElogioLinkadoId
            });
        }

        /// <summary>
        /// Busca elogios por um dia específico ou um intervalo de datas (máximo 7 dias).
        /// </summary>
        [HttpGet("elogios/buscar")]
        // Adicionamos um tipo de resposta explícito para o Swagger
        [ProducesResponseType(200, Type = typeof(List<ElogioResultadoDto>))]
        public async Task<IActionResult> BuscarElogiosPorData([FromQuery] DateTime dataInicio, [FromQuery] DateTime? dataFim)
        {
            if (dataFim.HasValue && (dataFim.Value - dataInicio).TotalDays > 7)
            {
                return BadRequest(new { message = "O intervalo máximo de busca é de 7 dias." });
            }

            var query = _supabase.From<Elogio>();

            if (dataFim.HasValue)
            {
                query = (Supabase.Interfaces.ISupabaseTable<Elogio, Supabase.Realtime.RealtimeChannel>)query.Where(e => e.DataElogio >= dataInicio && e.DataElogio <= dataFim.Value);
            }
            else
            {
                query = (Supabase.Interfaces.ISupabaseTable<Elogio, Supabase.Realtime.RealtimeChannel>)query.Where(e => e.DataElogio == dataInicio);
            }

            var resultado = await query.Get();

            // Usamos o DTO para criar uma lista com tipo definido
            var elogiosDto = resultado.Models.Select(e => new ElogioResultadoDto
            {
                Id = e.Id,
                Texto = e.Texto,
                DataElogio = e.DataElogio
            }).ToList();

            return Ok(elogiosDto);
        }

        /// <summary>
        /// Busca fotos por um dia específico ou um intervalo de datas (máximo 7 dias).
        /// </summary>
        [HttpGet("fotos/buscar")]
        // Adicionamos um tipo de resposta explícito para o Swagger
        [ProducesResponseType(200, Type = typeof(List<FotoResultadoDto>))]
        public async Task<IActionResult> BuscarFotosPorData([FromQuery] DateTime dataInicio, [FromQuery] DateTime? dataFim)
        {
            if (dataFim.HasValue && (dataFim.Value - dataInicio).TotalDays > 7)
            {
                return BadRequest(new { message = "O intervalo máximo de busca é de 7 dias." });
            }

            var query = _supabase.From<Foto>();

            if (dataFim.HasValue)
            {
                query = (Supabase.Interfaces.ISupabaseTable<Foto, Supabase.Realtime.RealtimeChannel>)query.Where(f => f.DataFoto >= dataInicio && f.DataFoto <= dataFim.Value);
            }
            else
            {
                query = (Supabase.Interfaces.ISupabaseTable<Foto, Supabase.Realtime.RealtimeChannel>)query.Where(f => f.DataFoto == dataInicio);
            }

            var resultado = await query.Get();

            // Usamos o DTO para criar uma lista com tipo definido
            var fotosDto = resultado.Models.Select(f => new FotoResultadoDto
            {
                Id = f.Id,
                NomeArquivo = f.NomeArquivo,
                DataFoto = f.DataFoto,
                ElogioLinkadoId = f.ElogioLinkadoId
            }).ToList();

            return Ok(fotosDto);
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

        /// <summary>
        /// Busca um arquivo de foto pelo seu nome.
        /// </summary>
        [HttpGet("foto/by-name/{nomeArquivo}")]
        public async Task<IActionResult> GetFotoPeloNome(string nomeArquivo)
        {
            try
            {
                var imagemBytes = await _supabase.Storage.From("tetefotos").Download(nomeArquivo, null);

                if (imagemBytes == null || imagemBytes.Length == 0)
                {
                    return NotFound($"Arquivo de imagem '{nomeArquivo}' não encontrado no Storage.");
                }

                var contentType = GetMimeType(nomeArquivo);
                return File(imagemBytes, contentType);
            }
            catch (Exception ex)
            {
                // Se estiver rodando localmente, o erro aparecerá no console do Visual Studio
                // Se estiver em produção, aparecerá no Logs Explorer do GCP
                Console.WriteLine($"Erro ao buscar foto por nome: {ex.Message}");
                return StatusCode(500, "Ocorreu um erro interno ao buscar a imagem.");
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