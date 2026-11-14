using UnityEngine;

namespace StillSky.Characters
{
    public enum Faction
    {
        Gloaming,
        Ember,
        HollowMoon,
        Crown,
        Neutral
    }

    [CreateAssetMenu(fileName = "NewCharacterStats", menuName = "Still Sky/Character Stats")]
    public class CharacterStats : ScriptableObject
    {
        public string displayName = "Unnamed";
        public Faction faction = Faction.Neutral;
        public float maxHealth = 100f;
        public float moveSpeed = 3.5f;
        public float damageMultiplier = 1f;
        public int emberReward;
    }
}