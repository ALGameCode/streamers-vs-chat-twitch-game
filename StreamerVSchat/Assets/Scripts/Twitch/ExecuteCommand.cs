using Twitch;
using UnityEngine;

public static class ExecuteCommand
{

    public static void CommandAction(CommandConfig commandConfig, (string chatUserName, string command) commandTuple)
    {
        string chatUserName = commandTuple.chatUserName;
        string command = commandTuple.command;

        if (TryExecuteCommandFunctions(commandConfig, chatUserName, command))
        {
            return;
        }

        TryExecuteCommandMobs(commandConfig, chatUserName, command);
    }

    private static bool TryExecuteCommandMobs(CommandConfig commandConfig, string chatUserName, string command)
    {
        foreach (CommandMobs commandMobs in commandConfig.commandsMobs)
        {
            if (commandMobs.Command.Equals(command))
            {
                CommandSummon(commandMobs.Mob, commandMobs.EnemyCost, chatUserName);
                return true;
            }
        }

        return false;
    }

    private static bool TryExecuteCommandFunctions(CommandConfig commandConfig, string chatUserName, string command)
    {
        foreach (CommandFunctions commandFunction in commandConfig.commandsFunctions)
        {
            if (commandFunction.Command.Equals(command))
            {
                //CommandAction(commandConfig, commandFunction.Function, chatUserName);
                return true;
            }
        }
        return false;
    }

    public static void CommandSummon(GameObject mob, int valEnergy, string chatUserName)
    {
        if (ChatStatus.instance.energy >= valEnergy)
        {
            MobSpawn.instance.SpawnMob(mob, chatUserName);
            ChatStatus.instance.DecreaseEnergy(valEnergy);
            if (ChatStatus.instance.chatUsers.ContainsKey(chatUserName))
            {
                ChatStatus.instance.chatUsers[chatUserName].SpawnedMonsters++;
            }
            else
            {
                ChatStatus.instance.chatUsers.Add(chatUserName, new ChatUser(chatUserName, 0, 1, 0));
            }
        }
    }

    public static void CommandAction(CommandConfig commandConfig, CommandFunctionsOptions option, string chatUserName)
    {
        switch (option)
        {
            case CommandFunctionsOptions.EnergyFunction:
                //SumEnergy();
                break;
            case CommandFunctionsOptions.RandomMob:
                RandomMob(commandConfig, chatUserName);
                break;
            case CommandFunctionsOptions.VoteFunction:
                // ...
                break;
        }
    }

    public static string BuildText(string chatCommandsText, string chatUserName, string command)
    {
        return $"{chatCommandsText} \n {chatUserName} : {command}";
    }

    public static void RandomMob(CommandConfig commandConfig, string chatUserName)
    {
        int index = Random.Range(0, commandConfig.commandsMobs.Count);
        CommandSummon(commandConfig.commandsMobs[index].Mob, commandConfig.commandsMobs[index].EnemyCost, chatUserName);
    }

    /*public void SumEnergy()
    {
        points = ChatStatus.instance.energy;
        controllerGameUI.SetTextPoints(points.ToString());
        ChatStatus.instance.AddEnergy(1);
        controllerGameUI.ChangeEnergyUI();
    }*/

    /*public void ShearchCommand(List<T> list)
    {
        
    }*/
}
