namespace Functions.Infrastructure.Features;

public static class EventTargets
{
    // characters
    public static readonly string CharactersApi = "characters-api";
    public static readonly string CharactersDb = "characters-persistence";
    
    // players
    public static readonly string PlayersApi = "players-api";
    public static readonly string PlayersDb = "players-persistence";
    
    // sessions
    public static readonly string SessionsApi = "sessions-api";
    public static readonly string SessionsDb = "sessions-persistence";
    
    public static readonly string GuildsApi = "guilds-api";
    public static readonly string GuildsDb  = "guilds-persistence";
    public static readonly string GuildsQueryHandler  = "guilds-api-query-handler";
    
    public static readonly string DiscordResponder = "discord-responder";
    public static readonly string DiscordWatchdog  = "discord-watchdog";
    public static readonly string DiscordWatchdogQueryHandler  = "discord-watchdog-query-handler";
}