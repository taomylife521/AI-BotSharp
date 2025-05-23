using BotSharp.Abstraction.Agents;
using BotSharp.Abstraction.Agents.Enums;

namespace BotSharp.Logger.Hooks;

public class VerboseLogHook : IContentGeneratingHook
{
    private readonly ConversationSetting _convSettings;
    private readonly ILogger<VerboseLogHook> _logger;
    private readonly IServiceProvider _services;

    public VerboseLogHook(
        ConversationSetting convSettings,
        IServiceProvider serivces,
        ILogger<VerboseLogHook> logger)
    {
        _convSettings = convSettings;
        _services = serivces;
        _logger = logger;
    }

    public async Task BeforeGenerating(Agent agent, List<RoleDialogModel> conversations)
    {
        if (!_convSettings.ShowVerboseLog) return;

        var dialog = conversations.LastOrDefault();
        if (dialog != null)
        {
            var log = $"{dialog.Role}: {dialog.Content} [msg_id: {dialog.MessageId}] ==>";
            _logger.LogDebug(log);
        }
        
        await Task.CompletedTask;
    }

    public async Task AfterGenerated(RoleDialogModel message, TokenStatsModel tokenStats)
    {
        if (!_convSettings.ShowVerboseLog || string.IsNullOrEmpty(tokenStats.Prompt)) return;

        var agentService = _services.GetRequiredService<IAgentService>();
        var agent = await agentService.GetAgent(message.CurrentAgentId);

        var log = message.Role == AgentRole.Function ?
                $"[{agent?.Name}]: {message.Indication} {message.FunctionName}({message.FunctionArgs})" :
                $"[{agent?.Name}]: {message.Content}" + $" <== [msg_id: {message.MessageId}]";

        _logger.LogDebug(tokenStats.Prompt);
        _logger.LogDebug(log);
    }
}
