// TriggerBot.cs
// Single-player / learning-only triggerbot example for Unity (C#)
// Usage: Attach to the Player or Camera in a Unity single-player project. Ensure weaponComponent has a public Fire() method.
// WARNING: For learning and single-player use only. Do not use to cheat in multiplayer games.

using System.Collections;
using UnityEngine;

/// <summary>
/// TriggerBot đơn giản cho mục đích học tập (single-player).
/// Gắn script này vào Player hoặc Camera. Cần tham chiếu tới Camera và Weapon (có phương thức Fire()).
/// Chỉ dùng offline / local để test.
/// </summary>
public class TriggerBot : MonoBehaviour
{
    [Header("References")]
    public Camera playerCamera;            // camera chính (nếu null sẽ cố lấy Camera.main)
    public MonoBehaviour weaponComponent;  // component chứa phương thức Fire() (ví dụ: Weapon script)

    [Header("Targeting")]
    public LayerMask targetMask;           // layer(s) cho target (ví dụ Enemy layer)
    public string targetTag = "Enemy";     // hoặc dùng tag
    public float maxDistance = 100f;

    [Header("Timing & Safety")]
    public bool enabledTriggerBot = true;
    public float minDelaySeconds = 0.05f;  // delay tối thiểu trước khi bắn
    public float maxDelaySeconds = 0.15f;  // delay tối đa (randomize)
    public float cooldownSeconds = 0.05f;  // thời gian giữa các lần trigger
    private bool canFire = true;

    void Reset()
    {
        playerCamera = Camera.main;
        minDelaySeconds = 0.05f;
        maxDelaySeconds = 0.15f;
        cooldownSeconds = 0.05f;
        maxDistance = 100f;
    }

    void Awake()
    {
        if (playerCamera == null) playerCamera = Camera.main;
    }

    void Update()
    {
        if (!enabledTriggerBot || !canFire || playerCamera == null || weaponComponent == null) return;

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f)); // tâm màn hình
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, maxDistance, targetMask))
        {
            // Kiểm tra tag (nếu dùng tag)
            if (!string.IsNullOrEmpty(targetTag) && !hit.collider.CompareTag(targetTag))
                return;

            // Optionally: kiểm tra nếu target có component health, isAlive, v.v.
            // Ví dụ: var hp = hit.collider.GetComponent<Health>(); if (hp == null || !hp.IsAlive) return;

            // Tất cả ok -> trigger fire with random delay
            StartCoroutine(FireWithDelay(Random.Range(minDelaySeconds, maxDelaySeconds)));
        }
    }

    IEnumerator FireWithDelay(float delaySeconds)
    {
        canFire = false;
        yield return new WaitForSeconds(delaySeconds);

        // Gọi phương thức Fire trên weaponComponent bằng reflection (giúp tương thích)
        var method = weaponComponent.GetType().GetMethod("Fire");
        if (method != null)
        {
            method.Invoke(weaponComponent, null);
        }
        else
        {
            Debug.LogWarning("[TriggerBot] Không tìm thấy phương thức Fire() trên weaponComponent.");
        }

        // cooldown
        yield return new WaitForSeconds(cooldownSeconds);
        canFire = true;
    }

    // Public API để bật/tắt
    public void ToggleTriggerBot(bool enabledState)
    {
        enabledTriggerBot = enabledState;
    }
}
