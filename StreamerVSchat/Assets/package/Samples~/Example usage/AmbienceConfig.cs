using UnityEngine;

[CreateAssetMenu(menuName="MyGame/Ambience Config")]
public class AmbienceConfig : ScriptableObject {

    public Catalog<string, AmbienceData> ambiences;

    public AmbienceConfig() =>
        ambiences = new Catalog<string, AmbienceData>(() => ref ambienceData, data => data.Key);

    [SerializeField]
    [CatalogData, AutomaticKeys, ValueLayout(value1Label="Name", value2Width=130f, value3Width=35f, value3Label="P")]
    private AmbienceData[] ambienceData;

    public AudioClip GetClip     (string key) => ambiences[key].Clip;
    public float     GetPitch    (string key) => ambiences[key].Pitch;
    public float     GetVolume   (string key) => ambiences[key].Volume;

    public bool      ContainsKey (string key) => ambiences.ContainsKey(key);

}

[System.Serializable]
public class AmbienceData {

    public string Key, DisplayName;
    [Range(0f, 1f)]
    public float Volume;
    public float Pitch;
    public AudioClip Clip;

}
