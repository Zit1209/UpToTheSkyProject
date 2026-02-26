using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(BoxCollider))]
public class TrapZone : MonoBehaviour
{
    [Header("Trap Settings")]
    [SerializeField] private GameObject trapPrefab;
    [SerializeField] private List<Transform> spawnPoints = new List<Transform>();
    [SerializeField] private float spawnInterval = 2f;
    
    [Header("Player Interaction")]
    [SerializeField] private float knockbackForce = 10f;
    [SerializeField] private float knockbackDuration = 1f;
    [SerializeField] private string playerTag = "Player";
    
    private BoxCollider triggerZone;
    private bool playerInZone = false;
    private Coroutine spawnCoroutine;

    private void Awake()
    {
        triggerZone = GetComponent<BoxCollider>();
        triggerZone.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInZone = true;
            StartSpawning();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInZone = false;
            StopSpawning();
        }
    }

    private void StartSpawning()
    {
        if (spawnCoroutine == null)
        {
            spawnCoroutine = StartCoroutine(SpawnTrapsCoroutine());
        }
    }

    private void StopSpawning()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }

    private IEnumerator SpawnTrapsCoroutine()
    {
        while (playerInZone)
        {
            SpawnTrap();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnTrap()
    {
        if (spawnPoints.Count == 0 || trapPrefab == null)
            return;

        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];
        
        if (spawnPoint != null)
        {
            GameObject trap = Instantiate(trapPrefab, spawnPoint.position, spawnPoint.rotation);
            
            FallingTrap trapComponent = trap.GetComponent<FallingTrap>();
            if (trapComponent == null)
            {
                trapComponent = trap.AddComponent<FallingTrap>();
            }
            
            trapComponent.Initialize(knockbackForce, knockbackDuration, playerTag);
        }
    }

    private void OnDestroy()
    {
        StopSpawning();
    }
}
public class FallingTrap : MonoBehaviour
{
    private float knockbackForce;
    private float knockbackDuration;
    private string playerTag;
    private bool hasHitPlayer = false;

    public void Initialize(float knockback, float duration, string tag)
    {
        knockbackForce = knockback;
        knockbackDuration = duration;
        playerTag = tag;

        // KHÔNG tạo collider / rigidbody
        // Chỉ đảm bảo collider đang hoạt động
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogError($"❌ {name} không có Collider!");
            enabled = false;
            return;
        }

        col.isTrigger = false;

        Destroy(gameObject, 10f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hasHitPlayer)
            return;

        // ===== HIT PLAYER =====
        if (collision.gameObject.CompareTag(playerTag))
        {
            hasHitPlayer = true;
            HandlePlayerHit(collision.gameObject);
            return;
        }

        // ===== HIT GROUND (CÁCH 3) =====
        if (IsGroundCollision(collision))
        {
            Destroy(gameObject, 0.5f);
        }
    }

    /// <summary>
    /// Kiểm tra có phải va chạm mặt đất không (dựa vào normal)
    /// </summary>
    private bool IsGroundCollision(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            // normal.y lớn → bề mặt hướng lên
            if (contact.normal.y > 0.5f)
            {
                return true;
            }
        }
        return false;
    }

    private void HandlePlayerHit(GameObject player)
    {
        PlayerMovement playerController = player.GetComponent<PlayerMovement>();
        if (playerController != null)
        {
            Vector3 dir = (player.transform.position - transform.position).normalized;
            dir.y = 0.3f;
            dir.Normalize();

            playerController.EnterKnockbackState(dir, knockbackForce, knockbackDuration);
        }

        Destroy(gameObject, 0.1f);
    }
}