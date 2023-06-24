using System;
using System.Collections.Generic;
using UnityEngine;

namespace Twitch
{
    /// <summary>
    /// Manages the configuration for commands
    /// </summary>
    [CreateAssetMenu(fileName = "New Command Config", menuName = "Custom/CreateCommandConfig")]
    public class CommandConfig : ScriptableObject
    {
        [Header("Configurações de Comandos:")]
        [Tooltip("Comandos para adicionar Mobs")]
        public List<CommandMobs> commandsMobs = new List<CommandMobs>();

        [Space(30)]
        [Tooltip("Comandos para executar funções")]
        public List<CommandFunctions> commandsFunctions = new List<CommandFunctions>();

        public Dictionary<string, CommandMobs> mobsCommandsDictionary = new Dictionary<string, CommandMobs>();
        public Dictionary<string, CommandFunctions> functionsCommandsDictionary = new Dictionary<string, CommandFunctions>();

        /// <summary>
        /// Builds dictionaries for command objects, using their command names as keys.
        /// </summary>
        /// <param name="mobs">List of CommandMobs objects.</param>
        /// <param name="functions">List of CommandFunctions objects.</param>
        /// <param name="mobsCommandsDictionary">Output dictionary for CommandMobs objects.</param>
        /// <param name="functionsCommandsDictionary">Output dictionary for CommandFunctions objects.</param>
        public void BuildCommandDictionaries(List<CommandMobs> mobs, List<CommandFunctions> functions, out Dictionary<string, CommandMobs> mobsCommandsDictionary, out Dictionary<string, CommandFunctions> functionsCommandsDictionary)
        {
            mobsCommandsDictionary = new Dictionary<string, CommandMobs>();
            functionsCommandsDictionary = new Dictionary<string, CommandFunctions>();

            foreach (CommandMobs mob in mobs)
            {
                mobsCommandsDictionary.Add(mob.Command, mob);
            }

            foreach (CommandFunctions function in functions)
            {
                functionsCommandsDictionary.Add(function.Command, function);
            }
        }
    }

    /// <summary>
    /// Represents a command for adding mobs
    /// </summary>
    [Serializable]
    public class CommandMobs
    {
        [SerializeField]
        [Tooltip("The command given by the chat to perform an action")]
        private string command = "";

        [SerializeField]
        [Tooltip("The number of summoners to summon this monster")]
        private int numSummoners = 1;

        [SerializeField]
        [Tooltip("Cost to summon the mob")]
        private int enemyCost = 1;

        [SerializeField]
        [Tooltip("Mob prefab")]
        private GameObject mob;

        public string Command
        {
            get { return command; }
            set { command = value; }
        }

        public int NumSummoners
        {
            get { return numSummoners; }
            set { numSummoners = value; }
        }

        public int EnemyCost
        {
            get { return enemyCost; }
            set { enemyCost = value; }
        }

        public GameObject Mob
        {
            get { return mob; }
            set { mob = value; }
        }
    }

    /// <summary>
    /// Represents a command for executing functions
    /// </summary>
    [Serializable]
    public class CommandFunctions
    {
        [SerializeField]
        [Tooltip("The command given by the chat to perform an action")]
        private string command = "";

        [SerializeField]
        [Tooltip("enum of executed function")]
        private CommandFunctionsOptions function = CommandFunctionsOptions.EnergyFunction;

        public string Command
        {
            get { return command; }
            set { command = value; }
        }

        public CommandFunctionsOptions Function
        {
            get { return function; }
            set { function = value; }
        }
    }

    /// <summary>
    /// Options for command functions
    /// </summary>
    public enum CommandFunctionsOptions
    {
        EnergyFunction,
        VoteFunction,
        RandomMobFunction,
        PlayGameFunction
    }
}