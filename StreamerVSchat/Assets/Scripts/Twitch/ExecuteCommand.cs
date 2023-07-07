using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace Twitch
{
    public static class ExecuteCommand
    {
        private const int CHAT_ENERGY_RECOVER = 1;

        /// <summary>
        /// Executes the specified command action based on the given command configuration and command tuple.
        /// </summary>
        /// <param name="commandConfig">The command configuration.</param>
        /// <param name="commandTuple">The tuple containing the chat user name and command.</param>
        public static void CommandAction(CommandConfig commandConfig, (string chatUserName, string command) commandTuple)
        {
            string chatUserName = commandTuple.chatUserName;
            string command = commandTuple.command;

            if (commandConfig.functionsCommandsDictionary.ContainsKey(command))
            {
                CommandFunctionsOptions functionOption = commandConfig.functionsCommandsDictionary[command].Function;
                ExecuteCommandAction(commandConfig, functionOption, chatUserName);
            }
            else
            {
                GameObject mob = commandConfig.mobsCommandsDictionary[command].Mob;
                int enemyCost = commandConfig.mobsCommandsDictionary[command].EnemyCost;
                CommandSummon(mob, enemyCost, chatUserName);
            }            
        }

        /// <summary>
        /// Executes the corresponding action based on the specified option.
        /// </summary>
        /// <param name="commandConfig">The command configuration.</param>
        /// <param name="option">The command function option.</param>
        /// <param name="chatUserName">The chat user name.</param>
        private static void ExecuteCommandAction(CommandConfig commandConfig, CommandFunctionsOptions option, string chatUserName)
        {
            Debug.Log("ExecuteCommandAction");

            switch (option)
            {
                case CommandFunctionsOptions.EnergyFunction:
                    SumEnergy(chatUserName);
                    break;
                case CommandFunctionsOptions.RandomMobFunction:
                    RandomMob(commandConfig, chatUserName);
                    break;
                case CommandFunctionsOptions.VoteFunction:
                    ExecuteVoting(chatUserName);
                    break;
                case CommandFunctionsOptions.PlayGameFunction:
                    NewChatPlayer(chatUserName);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Adds a new chat player.
        /// </summary>
        /// <param name="chatUserName">The username of the new chat player.</param>
        private static void NewChatPlayer(string chatUserName)
        {
            ChatStatus.instance.AddNewChatUser(chatUserName);
        }

        /// <summary>
        /// Increases the energy for the specified chat user and updates the UI.
        /// </summary>
        /// <param name="chatUserName">The username of the chat user.</param>
        private static void SumEnergy(string chatUserName)
        {
            if (!VerifyChatUser(chatUserName))
            {
                return;
            }

            int currentEnergy = ChatStatus.instance.energy;
            ChatStatus.instance.AddEnergy(CHAT_ENERGY_RECOVER);

            ControllerGameUI.instance.SetTextPoints(currentEnergy.ToString());
            ControllerGameUI.instance.ChangeEnergyUI();
        }

        /// <summary>
        /// TODO: ...
        /// </summary>
        /// <param name="chatUserName"></param>
        private static void ExecuteVoting(string chatUserName)
        {
            if (!VerifyChatUser(chatUserName))
            {
                return;
            }
            // TODO: ... 
        }

        /// <summary>
        /// Executes a random mob summoning command based on the provided CommandConfig and chat user name.
        /// </summary>
        /// <param name="commandConfig">The CommandConfig object containing the mob commands.</param>
        /// <param name="chatUserName">The name of the chat user.</param>
        private static void RandomMob(CommandConfig commandConfig, string chatUserName)
        {
            if (!VerifyChatUser(chatUserName))
            {
                return;
            }

            int index = GenericTools.GetRandomIndex(commandConfig.commandsMobs.Count);
            CommandMobs commandMobs = GetMobIndex(commandConfig.commandsMobs, index);
            CommandSummon(commandMobs.Mob, commandMobs.EnemyCost, chatUserName);
        }

        /// <summary>
        /// Retrieves a specific CommandMobs object from the given list based on the provided index.
        /// </summary>
        /// <param name="mobs">The list of CommandMobs objects.</param>
        /// <param name="index">The index of the CommandMobs object to retrieve.</param>
        /// <returns>The CommandMobs object at the specified index.</returns>
        private static CommandMobs GetMobIndex(List<CommandMobs> mobs, int index)
        {
            return mobs[index];
        }

        /// <summary>
        /// Summons a specified mob if the chat user is valid and has enough energy.
        /// </summary>
        /// <param name="mob">The mob to summon.</param>
        /// <param name="valEnergy">The energy cost required for summoning the mob.</param>
        /// <param name="chatUserName">The name of the chat user.</param>
        private static void CommandSummon(GameObject mob, int valEnergy, string chatUserName)
        {
            if (!VerifyChatUser(chatUserName))
            {
                return;
            }

            if (ChatStatus.instance.energy < valEnergy)
            {
                return;
            }
            
            MobSpawn.instance.SpawnMob(mob, chatUserName);
            ChatStatus.instance.DecreaseEnergy(valEnergy);

            if (ChatStatus.instance.chatUsers.ContainsKey(chatUserName))
            {
                ChatStatus.instance.chatUsers[chatUserName].SpawnedMonsters++;
            }
        }

        /// <summary>
        /// Verifies the presence of a chat user by checking if the provided username exists in the chatUsers dictionary.
        /// </summary>
        /// <param name="chatUserName">The username of the chat user to verify.</param>
        /// <returns>True if the chat user exists, false otherwise.</returns>
        private static bool VerifyChatUser(string chatUserName)
        {
            return ChatStatus.instance.chatUsers.ContainsKey(chatUserName);
        }
    }
}