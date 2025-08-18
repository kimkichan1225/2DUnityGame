using UnityEngine;

public class MaceItem : MonoBehaviour
{
    public WeaponStats maceStats; // Inspector에서 할당

    public GameObject pickupEffectPrefab;
    public AudioClip pickupSound;
    public float rotationSpeed = 50f;
    public float floatAmplitude = 0.25f;
    public float floatFrequency = 1f;

    private Vector3 startPosition;
    private AudioSource audioSource;

    private void Start()
    {
        startPosition = transform.position;
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    private void Update()
    {
        // 1. 좌우로 빙글빙글 회전
        transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
        // 2. 위아래로 둥실둥실 이동
        Vector3 tempPosition = startPosition;
        tempPosition.y += Mathf.Sin(Time.time * Mathf.PI * floatFrequency) * floatAmplitude;
        transform.position = tempPosition;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // 1. 이펙트 생성
            if (pickupEffectPrefab != null)
            {
                GameObject effect = Instantiate(pickupEffectPrefab, transform.position, Quaternion.identity);
                Destroy(effect, 0.5f);
            }
            // 2. 소리 재생
            if (pickupSound != null)
            {
                audioSource.PlayOneShot(pickupSound);
            }
            // 3. 플레이어에 장착 알림 + 스탯 전달
            PlayerController playerController = other.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.EquipMace(maceStats);
            }
            // 4. 오브젝트 삭제
            Destroy(gameObject);
        }
    }
}
