
using System;
using UnityEngine;

namespace Twitch
{
    /// <summary>
    /// Utility class for validating chat commands.
    /// </summary>
    public static class ValidCommand
    {
        /// <summary>
        /// Checks if a command exists in the command dictionary of mobs and functions.
        /// </summary>
        /// <param name="commandConfig">The command configuration object containing the dictionaries.</param>
        /// <param name="command">The command to be checked.</param>
        /// <returns>True if the command exists in the dictionaries, false otherwise.</returns>
        public static bool CheckCommandDictionaryFunctions(CommandConfig commandConfig, string command)
        {
            if (commandConfig.mobsCommandsDictionary.ContainsKey(command))
            {
                return true;
            }
            else if(commandConfig.functionsCommandsDictionary.ContainsKey(command))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Validates the chat command by checking if it exists in the command configuration.
        /// </summary>
        /// <param name="commandConfig">The command configuration.</param>
        /// <param name="command">The chat command to validate.</param>
        /// <returns>True if the command is valid, false otherwise.</returns>
        public static bool ValidChatCommand(CommandConfig commandConfig, string command)
        {
            return CheckCommandMobs(commandConfig, command) || CheckCommandFunctions(commandConfig, command);
        }

        /// <summary>
        /// Checks if the chat command exists in the list of command mobs.
        /// </summary>
        /// <param name="commandConfig">The command configuration.</param>
        /// <param name="command">The chat command to check.</param>
        /// <returns>True if the command exists in the command mobs, false otherwise.</returns>
        public static bool CheckCommandMobs(CommandConfig commandConfig, string command)
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

        /// <summary>
        /// Checks if the chat command exists in the list of command functions.
        /// </summary>
        /// <param name="commandConfig">The command configuration.</param>
        /// <param name="command">The chat command to check.</param>
        /// <returns>True if the command exists in the command functions, false otherwise.</returns>
        public static bool CheckCommandFunctions(CommandConfig commandConfig, string command)
        {
            foreach (CommandFunctions commandFunction in commandConfig.commandsFunctions)
            {
                if (commandFunction.Command.Equals(command))
                {
                    return true;
                }
            }
            return false;
        }
    }
}