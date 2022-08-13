using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "New Command Config", menuName = "CreateCommandConfig")]
public class CommandConfig : ScriptableObject
{
    [Header("Configurações de Comandos:")]
    [Tooltip("Comandos para adicionar Mobs")] public List<CommandMobs> commandsMobs = new List<CommandMobs>();
    [Space(30)]
    [Tooltip("Comandos para executar funções")] public List<CommandFunctions> commandsFunctions = new List<CommandFunctions>();
    
}

[System.Serializable] 
public class CommandMobs
{ 
    [SerializeField] public string command = "";
    [SerializeField] public int numSummoners = 1;
    [SerializeField] public int enemyCost = 1;
    [SerializeField] public GameObject mob;
}

[Serializable] 
public class CommandFunctions
{
    [SerializeField] public string command = "";
    [SerializeField] public CommandFunctionsOptions function = CommandFunctionsOptions.energyFunction;
}

public enum CommandFunctionsOptions {energyFunction, voteFunction, randomMob}