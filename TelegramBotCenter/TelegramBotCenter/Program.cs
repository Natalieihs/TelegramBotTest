
using Telegram.Bot;
using TelegramBotCenter;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<BotConfiguration>(
 builder.Configuration.GetSection(BotConfiguration.Configuration));
// Add services to the container.

builder.Services.AddHttpClient("telegram_bot_client")
              .AddTypedClient<ITelegramBotClient>((httpClient, sp) =>
              {
                  BotConfiguration? botConfig = sp.GetConfiguration<BotConfiguration>();
                  TelegramBotClientOptions options = new(botConfig.BotToken);
                  return new TelegramBotClient(options, httpClient);
              });

builder.Services.AddScoped<UpdateHandler>();
builder.Services.AddScoped<IReceiverService,ReceiverService>();
builder.Services.AddHostedService<PollingService>();
var app = builder.Build();
app.Run();
