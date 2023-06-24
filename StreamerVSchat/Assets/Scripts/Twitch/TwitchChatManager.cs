using Twitch.Connection;
using UnityEngine;
using Utils;

namespace Twitch
{
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
            commandConfig.BuildCommandDictionaries(commandConfig.commandsMobs, commandConfig.commandsFunctions, out commandConfig.mobsCommandsDictionary, out commandConfig.functionsCommandsDictionary);
        }


        private void Update()
        {
            ExecuteChatActions();
        }

        private void ExecuteChatActions()
        {
            (string chatUserName, string command) commandTuple = ReadChat.GetChat();

            Debug.Log($"commandConfig: {commandConfig}, command {commandTuple.command}");
            if (commandConfig == null || string.IsNullOrEmpty(commandTuple.command) || string.IsNullOrEmpty(commandTuple.chatUserName))
            {
                return;
            }

            if (ValidCommand.CheckCommandDictionaryFunctions(commandConfig, commandTuple.command))
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
}