using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int baseMaxHealth = 100; // 기본 최대 체력
    public int maxHealth = 100;     // 실제 최대 체력 (무기 효과 반영)
    private int currentHealth;      // 현재 체력 (private)

    [Header("UI")]
    public Slider healthSlider;     // 체력바 UI (Slider)
    public Text healthText;         // 체력 숫자 UI 텍스트

    [Header("Animator")]
    public Animator animator;       // 플레이어의 애니메이터 (Hurt, Die 등 애니메이션 제어)

    [Header("Events")]
    public UnityEvent onPlayerDeath; // 사망 시 실행될 이벤트 (예: 게임 오버 UI 호출 등)

    [Header("Invincibility")]
    public float invincibilityDuration = 1.0f; // 데미지 후 기본 무적 지속 시간
    private bool isInvincible = false;         // 현재 무적 상태인지 여부

    [Header("Audio")]
    [SerializeField] private AudioClip damageSound; // 데미지 사운드 클립
    private AudioSource audioSource;                // 사운드 재생을 위한 컴포넌트

    private bool isDead = false;                   // 사망 상태인지 여부

    private void Start()
    {
        // 게임 시작 시 현재 체력을 최대 체력으로 설정
        currentHealth = maxHealth;

        // AudioSource 초기화
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // 체력 슬라이더 초기화
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        // 체력 텍스트 초기화
        UpdateHealthUI();
    }

    // 무기 효과로 최대 체력 변경 (bonusHealth 적용)
    public void ApplyBonusHealth(int bonusHealth)
    {
        // 이전 최대 체력 저장
        int previousMaxHealth = maxHealth;
        // 새로운 최대 체력 계산
        maxHealth = baseMaxHealth + bonusHealth;
        // 현재 체력 비율 유지
        float healthPercent = (float)currentHealth / previousMaxHealth;
        currentHealth = Mathf.RoundToInt(maxHealth * healthPercent);
        // 체력 클램핑
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        // 체력 슬라이더 최대값 업데이트
        if (healthSlider != null)
            healthSlider.maxValue = maxHealth;
        // UI 업데이트
        UpdateHealthUI();
    }

    // 플레이어가 데미지를 받을 때 호출
    public void TakeDamage(int amount)
    {
        // 이미 죽었거나 무적 상태라면 데미지 무시
        if (isDead || isInvincible) return;

        // 체력 감소 및 최소 0으로 제한
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // 데미지 사운드 재생
        if (damageSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(damageSound);
        }

        // UI 업데이트
        UpdateHealthUI();

        // 체력이 0 이하이면 죽음 처리
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // 아직 살아있다면 피격 애니메이션 재생
            animator.SetTrigger("isHurt");
        }

        // 무적 상태 시작 (깜빡임 활성화)
        StartCoroutine(StartInvincibility(blink: true));
    }

    // 플레이어 체력 회복
    public void Heal(int amount)
    {
        // 죽었으면 회복 불가
        if (isDead) return;

        // 체력 회복 및 클램핑
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // UI 업데이트
        UpdateHealthUI();
    }

    // 체력 UI(슬라이더 + 숫자 텍스트) 업데이트
    private void UpdateHealthUI()
    {
        // 체력 슬라이더 업데이트
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }

        // 체력 텍스트 업데이트
        if (healthText != null)
        {
            healthText.text = $"{currentHealth} / {maxHealth}"; // 예: 75 / 100
        }
    }

    // 플레이어가 죽었을 때 실행되는 함수
    private void Die()
    {
        // 사망 상태로 전환
        isDead = true;

        // 디버그 메시지 출력
        Debug.Log("Player has died.");

        // 사망 애니메이션 실행
        animator.SetTrigger("isDead");

        // 사망 이벤트 발생
        onPlayerDeath?.Invoke();
    }

    // 무적 상태를 유지하는 코루틴
    public IEnumerator StartInvincibility(float duration = -1f, bool blink = true)
    {
        // 무적 상태 활성화
        isInvincible = true;

        // 지속 시간 설정: 전달된 duration이 -1f면 기본 invincibilityDuration 사용
        float invincibilityTime = duration < 0f ? invincibilityDuration : duration;

        // 깜빡임 효과 (blink 파라미터가 true일 때만 실행)
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        float blinkInterval = 0.1f; // 깜빡임 간격
        float elapsedTime = 0f;

        if (blink && spriteRenderer != null)
        {
            while (elapsedTime < invincibilityTime)
            {
                // 스프라이트 렌더러 활성화/비활성화 토글
                spriteRenderer.enabled = !spriteRenderer.enabled;
                elapsedTime += blinkInterval;
                yield return new WaitForSeconds(blinkInterval);
            }
            // 무적 종료 후 스프라이트 렌더러 복구
            spriteRenderer.enabled = true;
        }
        else
        {
            // 깜빡임 비활성화 시 단순히 시간 대기
            yield return new WaitForSeconds(invincibilityTime);
        }

        // 무적 상태 비활성화
        isInvincible = false;
    }

    // 외부에서 현재 체력을 가져올 수 있도록 하는 getter
    public int GetCurrentHealth() => currentHealth;

    // 플레이어 체력을 초기화하고 상태를 리셋 (부활 시 사용 가능)
    public void ResetHealth()
    {
        // 사망 상태 해제
        isDead = false;
        // 체력을 최대 체력으로 설정
        currentHealth = maxHealth;

        // UI 업데이트
        UpdateHealthUI();
    }
}