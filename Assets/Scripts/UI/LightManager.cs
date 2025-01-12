using UnityEngine;
using System.Collections.Generic;

public class LightManager : MonoBehaviour
{
    public static LightManager Instance { get; private set; }
    private List<LightSource> lightSources = new List<LightSource>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Tìm tất cả các nguồn sáng trong scene
        LightSource[] sources = FindObjectsOfType<LightSource>();
        lightSources.AddRange(sources);
    }

    public bool IsPositionIlluminated(Vector2 position)
    {
        foreach (LightSource light in lightSources)
        {
            if (light.IsInLight(position))
            {
                // Kiểm tra xem có vật cản giữa nguồn sáng và vị trí không
                RaycastHit2D hit = Physics2D.Raycast(
                    light.transform.position,
                    position - (Vector2)light.transform.position,
                    Vector2.Distance(light.transform.position, position),
                    LayerMask.GetMask("Default", "Ground") // Thêm các layer cần thiết
                );

                if (hit.collider == null)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public void RegisterLight(LightSource light)
    {
        if (!lightSources.Contains(light))
        {
            lightSources.Add(light);
        }
    }

    public void UnregisterLight(LightSource light)
    {
        lightSources.Remove(light);
    }
}