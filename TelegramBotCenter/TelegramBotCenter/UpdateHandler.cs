using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBotCenter
{
    public class UpdateHandler : IUpdateHandler
    {

        private readonly ITelegramBotClient _botClient;
        private readonly ILogger<UpdateHandler> _logger;

        private const string recharge = $"{Emoji.Moneybag}充值";
        private const string withdraw = $"{Emoji.Money_With_Wings}提现";

        private const string contact = $"{Emoji.Telephone}联系我们";

        private const string banance = $"{Emoji.Currency_Exchange}查询余额";
        public UpdateHandler(ITelegramBotClient botClient, ILogger<UpdateHandler> logger)
        {
            _botClient = botClient;
            _logger = logger;
        }
        public async Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            _logger.LogInformation("HandleError: {ErrorMessage}", ErrorMessage);

            // Cooldown in case of network connection error
            if (exception is RequestException)
                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var handler = update switch
            {
                { Message: { } message } => BotOnMessageReceived(message, cancellationToken),
                { CallbackQuery: { } callbackQuery } => BotOnCallbackQueryReceived(callbackQuery, cancellationToken),
            };
            await handler;
        }

        private async Task BotOnCallbackQueryReceived(CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Received inline keyboard callback from: {CallbackQueryId}", callbackQuery.Id);

            await _botClient.AnswerCallbackQueryAsync(
                callbackQueryId: callbackQuery.Id,
                text: $"充值成功： {callbackQuery.Data}",
                cancellationToken: cancellationToken);

            await _botClient.SendTextMessageAsync(
                chatId: callbackQuery.Message!.Chat.Id,
                text: $"充值成功： {callbackQuery.Data}",
                cancellationToken: cancellationToken);
        }

        private async Task BotOnMessageReceived(Message message, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Receive message type: {MessageType}", message.Type);
            if (message.Text is not { } messageText)
                return;

            var action = messageText.Split(' ')[0] switch
            {
                "/start" => SendReplyKeyboard(_botClient, message, cancellationToken),
                recharge => SendInlineKeyboard(_botClient, message, cancellationToken),
                contact => SendInlineContactboard(_botClient, message, cancellationToken),
                _ => Usage(_botClient, message, cancellationToken)
            };
        }

        static async Task<Message> SendInlineContactboard(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            await botClient.SendChatActionAsync(
               chatId: message.Chat.Id,
               chatAction: ChatAction.Typing,
               cancellationToken: cancellationToken);

            // Simulate longer running task
            await Task.Delay(500, cancellationToken);

            InlineKeyboardMarkup inlineKeyboard = new(
                new[]
                {
                    new[]
                    {
                      InlineKeyboardButton.WithUrl("联系我们", "https://t.me/moximoxipeter"),
                    }
                });

            return await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "联系我们，一起打造我们的飞机娱乐！",
                replyMarkup: inlineKeyboard,
                cancellationToken: cancellationToken);
        }

        static async Task<Message> SendInlineKeyboard(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            await botClient.SendChatActionAsync(
                chatId: message.Chat.Id,
                chatAction: ChatAction.Typing,
                cancellationToken: cancellationToken);

            // Simulate longer running task666
            await Task.Delay(500, cancellationToken);

            InlineKeyboardMarkup inlineKeyboard = new(
                new[]
                {
                    // first row
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("100", "100"),
                        InlineKeyboardButton.WithCallbackData("200", "200"),
                    },
                    // second row
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("300", "300"),
                        InlineKeyboardButton.WithCallbackData("400", "400"),
                    },
                    new[]
                    {
                                 InlineKeyboardButton.WithUrl("联系我", "https://t.me/moximoxipeter"),
                    }
                });

            return await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "请选择充值金额",
                replyMarkup: inlineKeyboard,
                cancellationToken: cancellationToken);
        }
        static async Task<Message> Usage(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        static async Task<Message> SendReplyKeyboard(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            ReplyKeyboardMarkup replyKeyboardMarkup = new(
                new[]
                {
                        new KeyboardButton[] { recharge, withdraw },
                        new KeyboardButton[] { contact, banance },
                })
            {
                ResizeKeyboard = true
            };
            return await botClient.SendTextMessageAsync(
               chatId: message.Chat.Id,
               text: "Choose",
               replyMarkup: replyKeyboardMarkup,
               cancellationToken: cancellationToken);
        }

        static async Task<Message> SendFile(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            await botClient.SendChatActionAsync(
                message.Chat.Id,
                ChatAction.UploadPhoto,
                cancellationToken: cancellationToken);

            const string filePath = "Files/tux.png";
            await using FileStream fileStream = new(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var fileName = filePath.Split(Path.DirectorySeparatorChar).Last();

            return await botClient.SendPhotoAsync(
                chatId: message.Chat.Id,
                photo: new InputFile(fileStream, fileName),
                caption: "Nice Picture",
                cancellationToken: cancellationToken);
        }
    }
}
