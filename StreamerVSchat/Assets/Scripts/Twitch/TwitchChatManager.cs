using Twitch;
using Twitch.Connection;
using UnityEngine;
using Utils;

public class TwitchChatManager : SingletonMonoBehaviour<TwitchChatManager>
{
    [Tooltip("Configurações de comandos")]
    [SerializeField]
    private CommandConfig commandConfig;

    /*[Tooltip("UI Manager")]
    [SerializeField]
    private ControllerGameUI controllerGameUI;*/

    private void Start()
    {
        
    }


    private void Update()
    {
        ExecuteChatActions();
    }

    private void ExecuteChatActions()
    {
        (string chatUserName, string command) commandTuple = ReadChat.GetChat();
        if (ValidCommand.ValidChatCommand(commandConfig, commandTuple.command))
        {
            Debug.Log($"1");
            ExecuteCommand.CommandAction(commandConfig, commandTuple);
            Debug.Log($"2");
            ControllerGameUI.instance.SetTextChatCommands(commandTuple);
            Debug.Log($"3");
            ControllerGameUI.instance.ChangeEnergyUI();
        }
    }
}
