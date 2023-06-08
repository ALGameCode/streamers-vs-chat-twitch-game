using Twitch;

public static class ValidCommand
{
    public static bool ValidChatCommand(CommandConfig commandConfig, string command)
    {
        foreach (CommandMobs commandMobs in commandConfig.commandsMobs)
        {
            if (commandMobs.Command.Equals(command))
            {
                return true;
            }
        }
        
        return false;
    }
}
