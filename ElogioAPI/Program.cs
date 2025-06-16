using Supabase;

var builder = WebApplication.CreateBuilder(args);

// Adiciona servi�os ao cont�iner.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- In�cio da Configura��o do Supabase ---

// 1. Pega a URL e a Chave do appsettings.json
var supabaseUrl = builder.Configuration["Supabase:Url"];
var supabaseKey = builder.Configuration["Supabase:AnonKey"];

// 2. Registra o cliente Supabase como um servi�o singleton
builder.Services.AddSingleton(provider => new Client(supabaseUrl, supabaseKey));

// --- Fim da Configura��o do Supabase ---


// --- IN�CIO DA CONFIGURA��O DO CORS (C�DIGO ADICIONADO) ---
var myAllowSpecificOrigins = "_myAllowSpecificOrigins";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: myAllowSpecificOrigins,
                      policy =>
                      {
                          // Permite que qualquer origem, cabe�alho e m�todo acesse a API.
                          // �timo para desenvolvimento.
                          policy.AllowAnyOrigin()
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                      });
});
// --- FIM DA CONFIGURA��O DO CORS ---


var app = builder.Build();

// Configure o pipeline de requisi��es HTTP.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// --- USA A POL�TICA DE CORS (C�DIGO ADICIONADO) ---
// Esta linha deve vir antes de UseAuthorization
app.UseCors(myAllowSpecificOrigins);
// ---------------------------------------------------

app.UseAuthorization();

app.MapControllers();

app.Run();