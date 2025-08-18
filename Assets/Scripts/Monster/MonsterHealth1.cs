using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MonsterHealth1 : MonoBehaviour
{
    public int maxHealth = 300; // 최대 체력
    public int defense = 0;     // 방어력
    private int currentHealth;  // 현재 체력
    private Animator animator;  // 애니메이터 컴포넌트
    private AudioSource audioSource; // 사운드 재생을 위한 컴포넌트
    private bool isDead = false; // 사망 상태 플래그
    public bool IsDead => isDead;   // 외부에서 사망 상태 확인용 프로퍼티

    public Slider healthBar;          // 체력바 UI
    public GameObject damageTextPrefab; // 데미지 텍스트 프리팹
    public Vector3 damageTextOffset = new Vector3(0, 1f, 0); // 데미지 텍스트 표시 오프셋
    public Vector3 healthBarOffset = new Vector3(0, 1.5f, 0); // 체력바 표시 오프셋

    [SerializeField] private AudioClip deathSound; // 사망 사운드 클립
    [SerializeField] private AudioClip hitSound;   // 피격 사운드 클립
    [SerializeField] private float deathAnimationDuration = 1f; // 사망 애니메이션 지속 시간 (초)

    private Canvas healthBarCanvas; // 체력바가 속한 캔버스
    private RectTransform healthBarRect; // 체력바의 RectTransform

    private void Start()
    {
        currentHealth = maxHealth; // 초기 체력 설정
        animator = GetComponent<Animator>(); // 애니메이터 초기화
        audioSource = GetComponent<AudioSource>(); // AudioSource 초기화

        // AudioSource가 없으면 추가
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth; // 체력바 최대값 설정
            healthBar.value = currentHealth; // 초기 체력바 값 설정
            healthBar.gameObject.SetActive(false); // 처음에는 체력바 숨김

            // 체력바의 RectTransform과 부모 캔버스 초기화
            healthBarRect = healthBar.GetComponent<RectTransform>();
            healthBarCanvas = healthBar.GetComponentInParent<Canvas>();
        }
    }

    private void LateUpdate()
    {
        if (healthBar != null && healthBar.gameObject.activeSelf)
        {
            // 체력바 위치를 몬스터 위치에 동기화 (월드 공간 기준)
            Vector3 worldPos = transform.position + healthBarOffset;
            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
            healthBarRect.position = screenPos;

            // 체력바의 스케일과 회전을 고정
            healthBarRect.localScale = Vector3.one; // 스케일 고정
            healthBarRect.rotation = Quaternion.identity; // 회전 고정
        }
    }

    // Sword 데미지 처리 메서드
    public void TakeDamageSword(int amount, int attackType)
    {
        if (isDead) return;

        float damageMultiplier = Random.Range(0.85f, 1.15f);
        int finalDamage;

        if (attackType == 1)
        {
            finalDamage = Mathf.RoundToInt(((amount - defense) * damageMultiplier) / 2f);
        }
        else
        {
            finalDamage = Mathf.RoundToInt((amount - defense) * damageMultiplier);
        }

        finalDamage = Mathf.Max(finalDamage, 1);
        ApplyDamage(finalDamage);
    }

    // Mace 데미지 처리 메서드
    public void TakeDamageMace(int amount, int attackType)
    {
        if (isDead) return;

        float damageMultiplier = Random.Range(0.85f, 1.15f);
        int finalDamage;

        if (attackType == 1)
        {
            finalDamage = Mathf.RoundToInt((amount - defense) * damageMultiplier);
        }
        else
        {
            finalDamage = Mathf.RoundToInt(((amount - defense) * damageMultiplier) * 1.1f);
        }

        finalDamage = Mathf.Max(finalDamage, 1);
        ApplyDamage(finalDamage);
    }

    // Lance 데미지 처리 메서드
    public void TakeDamageLance(int amount, int attackType)
    {
        if (isDead) return;

        float damageMultiplier = Random.Range(0.85f, 1.15f);
        int finalDamage;

        if (attackType == 1)
        {
            finalDamage = Mathf.RoundToInt(((amount - defense) * damageMultiplier) / 2f);
        }
        else
        {
            finalDamage = Mathf.RoundToInt(((amount - defense) * damageMultiplier) / 3f);
        }

        finalDamage = Mathf.Max(finalDamage, 1);
        ApplyDamage(finalDamage);
    }

    private void ApplyDamage(int finalDamage)
    {
        currentHealth -= finalDamage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (hitSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hitSound);
        }

        if (damageTextPrefab != null)
        {
            Vector3 spawnPosition = transform.position + damageTextOffset;
            GameObject damageTextObj = Instantiate(damageTextPrefab, spawnPosition, Quaternion.identity, GameObject.Find("DamageTextCanvas").transform);
            DamageText damageText = damageTextObj.GetComponent<DamageText>();
            if (damageText != null)
                damageText.SetDamage(finalDamage);
        }

        if (animator != null)
            animator.SetTrigger("isHurt");

        if (healthBar != null)
        {
            healthBar.value = currentHealth;
            healthBar.gameObject.SetActive(true); // 공격받으면 체력바 표시
        }

        if (currentHealth <= 0)
            Die();
    }

    private void Die()
    {
        if (isDead) return;

        isDead = true;

        if (animator != null)
            animator.SetTrigger("isDead");

        if (deathSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(deathSound);
        }

        if (healthBar != null)
            healthBar.gameObject.SetActive(false);

        StartCoroutine(DestroyAfterAnimation());
    }

    private IEnumerator DestroyAfterAnimation()
    {
        yield return new WaitForSeconds(deathAnimationDuration);
        Debug.Log("허수아비가 쓰러졌습니다.");
        Destroy(gameObject);
    }
}