using Twitch.Connection;
using UnityEngine;
using Utils;

namespace Twitch
{
    public class TwitchChatManager : SingletonMonoBehaviour<TwitchChatManager>
    {
        [Tooltip("Configurações de comandos")]
        [SerializeField] private CommandConfig commandConfig;
        private ControllerGameUI controllerGameUI;

        private void Start()
        {
            Initialize();
        }

        private void Update()
        {
            ExecuteChatActions();
        }

        /// <summary>
        /// Initializes the Twitch chat manager by building command dictionaries and finding the controller game UI.
        /// </summary>
        private void Initialize()
        {
            commandConfig.BuildCommandDictionaries(commandConfig.commandsMobs, commandConfig.commandsFunctions, out commandConfig.mobsCommandsDictionary, out commandConfig.functionsCommandsDictionary);
            controllerGameUI = FindObjectOfType<ControllerGameUI>();
        }

        /// <summary>
        /// Executes chat actions based on the received chat command.
        /// </summary>
        private void ExecuteChatActions()
        {
            (string chatUserName, string command) commandTuple = ReadChat.GetChat();
            if (IsInvalidCommand(commandTuple))
            {
                return;
            }

            if (IsValidCommand(commandTuple.command))
            {
                ExecuteCommandAction(commandTuple);
                UpdateEnergyHUD();
            }
        }

        /// <summary>
        /// Checks if the received command is invalid.
        /// </summary>
        /// <param name="commandTuple">The chat command tuple.</param>
        /// <returns>True if the command is invalid, false otherwise.</returns>
        private bool IsInvalidCommand((string chatUserName, string command) commandTuple)
        {
            return commandConfig == null || string.IsNullOrEmpty(commandTuple.command) || string.IsNullOrEmpty(commandTuple.chatUserName);
        }

        /// <summary>
        /// Checks if the received command is valid.
        /// </summary>
        /// <param name="command">The chat command.</param>
        /// <returns>True if the command is valid, false otherwise.</returns>
        private bool IsValidCommand(string command)
        {
            return ValidCommand.CheckCommandDictionaryFunctions(commandConfig, command);
        }

        /// <summary>
        /// Executes the action associated with the received chat command.
        /// </summary>
        /// <param name="commandTuple">The chat command tuple.</param>
        private void ExecuteCommandAction((string chatUserName, string command) commandTuple)
        {
            ExecuteCommand.CommandAction(commandConfig, commandTuple);
        }

        #region HUD_Update

        /// <summary>
        /// Updates the energy HUD with the current energy value.
        /// </summary>
        public void UpdateEnergyHUD()
        {
            controllerGameUI.SetTextPoints(ChatStatus.instance.energy.ToString());
            controllerGameUI.ChangeEnergyUI();
        }

        /// <summary>
        /// Updates the warning text in the chat HUD.
        /// </summary>
        /// <param name="warning">The warning message to display.</param>
        public void UpdateWarningTextChatHUD(string warning)
        {
            controllerGameUI.SetTextChatWarning(warning);
        }

        /// <summary>
        /// Updates the text in the chat HUD with the received chat command.
        /// </summary>
        /// <param name="commandTuple">The chat command tuple.</param>
        public void UpdateTextChatHUD((string chatUserName, string command) commandTuple)
        {
            controllerGameUI.SetTextChatCommands(commandTuple);
        }

        #endregion HUD_Update
    }
}