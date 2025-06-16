using Supabase;

var builder = WebApplication.CreateBuilder(args);

// Adiciona serviços ao contêiner.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- Início da Configuração do Supabase ---

// 1. Pega a URL e a Chave do appsettings.json
var supabaseUrl = builder.Configuration["Supabase:Url"];
var supabaseKey = builder.Configuration["Supabase:AnonKey"];

// 2. Registra o cliente Supabase como um serviço singleton
builder.Services.AddSingleton(provider => new Client(supabaseUrl, supabaseKey));

// --- Fim da Configuração do Supabase ---


// --- INÍCIO DA CONFIGURAÇÃO DO CORS (CÓDIGO ADICIONADO) ---
var myAllowSpecificOrigins = "_myAllowSpecificOrigins";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: myAllowSpecificOrigins,
                      policy =>
                      {
                          // Permite que qualquer origem, cabeçalho e método acesse a API.
                          // Ótimo para desenvolvimento.
                          policy.AllowAnyOrigin()
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                      });
});
// --- FIM DA CONFIGURAÇÃO DO CORS ---


var app = builder.Build();

// Configure o pipeline de requisições HTTP.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// --- USA A POLÍTICA DE CORS (CÓDIGO ADICIONADO) ---
// Esta linha deve vir antes de UseAuthorization
app.UseCors(myAllowSpecificOrigins);
// ---------------------------------------------------

app.UseAuthorization();

app.MapControllers();

app.Run();