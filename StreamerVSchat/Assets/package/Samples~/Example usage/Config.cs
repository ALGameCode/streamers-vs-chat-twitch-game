using UnityEngine;
[CreateAssetMenu(menuName="MyGame/Config", fileName="Config.asset")]
public class Config : UnityEngine.ScriptableObject
{
    public SerializableDictionary<string, GameObject> Items;
}
