using System.Data;
using UnityEngine;

namespace Twitch
{
    public static class ExecuteCommand
    {

        public static void CommandAction(CommandConfig commandConfig, (string chatUserName, string command) commandTuple)
        {
            string chatUserName = commandTuple.chatUserName;
            string command = commandTuple.command;
            Debug.Log("CommandAction");
            if (commandConfig.functionsCommandsDictionary.ContainsKey(command))
            {
                ExecuteCommandAction(commandConfig, commandConfig.functionsCommandsDictionary[command].Function, chatUserName);
            }
            else
            {
                CommandSummon(commandConfig.mobsCommandsDictionary[command].Mob, commandConfig.mobsCommandsDictionary[command].EnemyCost, chatUserName);
            }            
        }

        public static void ExecuteCommandAction(CommandConfig commandConfig, CommandFunctionsOptions option, string chatUserName)
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
            }
        }

        public static void NewChatPlayer(string chatUserName)
        {
            Debug.Log("NewChatPlayer");
            ChatStatus.instance.AddNewChatUser(chatUserName);
        }

        public static void SumEnergy(string chatUserName)
        {
            if (!VerifyChatUser(chatUserName))
            {
                return;
            }
            int points = ChatStatus.instance.energy;
            ControllerGameUI.instance.SetTextPoints(points.ToString());
            ChatStatus.instance.AddEnergy(1);
            ControllerGameUI.instance.ChangeEnergyUI();
        }

        public static void ExecuteVoting(string chatUserName)
        {
            if (!VerifyChatUser(chatUserName))
            {
                return;
            }
            // TODO: ... 
        }

        public static void RandomMob(CommandConfig commandConfig, string chatUserName)
        {
            if (!VerifyChatUser(chatUserName))
            {
                return;
            }
            int index = Random.Range(0, commandConfig.commandsMobs.Count);
            CommandSummon(commandConfig.commandsMobs[index].Mob, commandConfig.commandsMobs[index].EnemyCost, chatUserName);
        }

        public static void CommandSummon(GameObject mob, int valEnergy, string chatUserName)
        {
            if (!VerifyChatUser(chatUserName))
            {
                return;
            }
            Debug.Log("CommandSummon");
            if (ChatStatus.instance.energy >= valEnergy)
            {
                MobSpawn.instance.SpawnMob(mob, chatUserName);
                ChatStatus.instance.DecreaseEnergy(valEnergy);
                if (ChatStatus.instance.chatUsers.ContainsKey(chatUserName))
                {
                    ChatStatus.instance.chatUsers[chatUserName].SpawnedMonsters++;
                }
            }
        }

        public static string BuildText(string chatCommandsText, string chatUserName, string command)
        {
            return $"{chatCommandsText} \n {chatUserName} : {command}";
        }

        private static bool VerifyChatUser(string chatUserName)
        {
            if (!ChatStatus.instance.chatUsers.ContainsKey(chatUserName))
            {
                return false;
            }

            return true;
        }
    }
}