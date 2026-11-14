using UnityEngine;
using StillSky.Characters;

public class FallZone : MonoBehaviour
{
    [Tooltip("Characters below this world Y die.")]
    [SerializeField] private float killHeight = -15f;

    // Update is called once per frame
    void Update()
    {
        var all = Character.All;
        for(int i = all.Count - 1; i >= 0; i--)
        {
            Character c = all[i];
            if (c.IsAlive && c.transform.position.y < killHeight)
                c.Health.Kill(gameObject);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0.2f, 0.1f, 0.25f);
        Gizmos.DrawCube(new Vector3(transform.position.x, killHeight, transform.position.z), new Vector3(200f, 0.05f, 200f));
    }
}
