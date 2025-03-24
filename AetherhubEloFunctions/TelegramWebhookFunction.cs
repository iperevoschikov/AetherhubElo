using Microsoft.Extensions.DependencyInjection;
using YandexCloudFunctions.Net.Sdk;

namespace AetherhubEloFunctions;

public class TelegramWebhookFunction : BaseFunctionHandler
{
    protected override void ConfigureServices(IServiceCollection serviceCollection)
    {
        throw new NotImplementedException();
    }

    public async Task<FunctionHandlerResponse> Handle(FunctionHandlerRequest request)
    {
        return FunctionHandlerResponses.Ok();
    }
}