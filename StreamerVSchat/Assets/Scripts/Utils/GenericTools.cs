
namespace Utils
{
    public static class GenericTools
    {
        public static int GetRandomIndex(int maxExclusive)
        {
            return UnityEngine.Random.Range(0, maxExclusive);
        }

    }
}