using Twitch;
using UnityEngine;

public class ValidCommand : MonoBehaviour
{
    [Tooltip("Configurações de comandos")]
    public CommandConfig commandConfig;

    [Tooltip("UI Manager")]
    public ControllerGameUI controllerGameUI;

    private int points = 0;
    private string chatCommandsText = "";


    public static ValidCommand instance;

    void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public void ValidChatCommand(string chatUserName, string command)
    {
        bool isValid = false;
        for (int i = 0; i < commandConfig.commandsMobs.Count; i++)
        {
            if(commandConfig.commandsMobs[i].Command.Equals(command))
            {
                BuildText(chatUserName, command);
                CommandSummon(commandConfig.commandsMobs[i].Mob, commandConfig.commandsMobs[i].EnemyCost, chatUserName);
                isValid = true;
            }
        }

        if(!isValid)
        {
            for(int i = 0; i < commandConfig.commandsFunctions.Count; i++)
            {
                if(commandConfig.commandsFunctions[i].Command.Equals(command))
                {
                    BuildText(chatUserName, command);
                    CommandAction(commandConfig.commandsFunctions[i].Function, chatUserName);
                }
            }
        }
    }

    public void BuildText(string chatUserName, string command)
    {
        chatCommandsText = $"{chatCommandsText} \n {chatUserName} : {command}";
        controllerGameUI.SetTextChatCommands(chatCommandsText);
    }

    public void CommandAction(CommandFunctionsOptions option, string chatUserName)
    {
        switch(option)
        {
            case CommandFunctionsOptions.energyFunction:
                SumEnergy();
                break;
            case CommandFunctionsOptions.randomMob:
                RandomMob(chatUserName);
                break;
            case CommandFunctionsOptions.voteFunction:
                // ...
                break;
        }
    }

    public void CommandSummon(GameObject mob, int valEnergy, string chatUserName)
    {
        if(ChatStatus.instance.energy >= valEnergy)
        {
            MobSpawn.instance.SpawnMob(mob, chatUserName);
            ChatStatus.instance.DecreaseEnergy(valEnergy);
            if(ChatStatus.instance.chatUsers.ContainsKey(chatUserName))
                ChatStatus.instance.chatUsers[chatUserName].SpawnedMonsters++;
            else
                ChatStatus.instance.chatUsers.Add(chatUserName, new ChatUser(chatUserName, 0, 1, 0));
            controllerGameUI.ChangeEnergyUI();
        }
    }

    public void RandomMob(string chatUserName)
    {
        int index = Random.Range(0, commandConfig.commandsMobs.Count);
        CommandSummon(commandConfig.commandsMobs[index].Mob, commandConfig.commandsMobs[index].EnemyCost, chatUserName);
    }

    public void SumEnergy()
    {
        points = ChatStatus.instance.energy;
        controllerGameUI.SetTextPoints(points.ToString());
        ChatStatus.instance.AddEnergy(1);
        controllerGameUI.ChangeEnergyUI();
    }

    /*public void ShearchCommand(List<T> list)
    {
        
    }*/
}
