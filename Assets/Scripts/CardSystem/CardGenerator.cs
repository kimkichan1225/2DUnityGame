using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 랜덤한 CombatPage 카드를 생성하는 유틸리티 클래스
/// </summary>
public static class CardGenerator
{
    // 코스트별 확률 테이블
    private static readonly (int cost, float probability)[] costProbabilities = new[]
    {
        (0, 0.30f), // 30%
        (1, 0.30f), // 30%
        (2, 0.25f), // 25%
        (3, 0.10f), // 10%
        (4, 0.05f)  // 5%
    };

    // Element(주사위) 개수 확률 테이블
    private static readonly (int count, float probability)[] diceProbabilities = new[]
    {
        (1, 0.70f), // 70%
        (2, 0.25f), // 25%
        (3, 0.05f)  // 5%
    };

    // 코스트별 공격 주사위 범위 (최소값, 최대값)
    private static readonly Dictionary<int, (int[] minValues, int[] maxValues)> attackDiceRanges = new()
    {
        { 0, (new[] {1, 2}, new[] {4, 5}) },
        { 1, (new[] {1, 2, 3}, new[] {5, 6, 7}) },
        { 2, (new[] {2, 3, 4, 5, 6, 7}, new[] {8, 9, 10, 11, 12}) },
        { 3, (new[] {4, 5, 6, 7, 8}, new[] {9, 10, 11, 12, 13, 14, 15}) },
        { 4, (new[] {10, 11, 12, 13, 14, 15}, new[] {20, 21, 22, 23, 24, 25, 26, 27}) }
    };

    // 코스트별 방어 주사위 범위
    private static readonly Dictionary<int, (int[] minValues, int[] maxValues)> defenseDiceRanges = new()
    {
        { 0, (new[] {1, 2, 3}, new[] {5, 6, 7, 8}) },
        { 1, (new[] {2, 3, 4, 5}, new[] {7, 8, 9, 10, 11}) },
        { 2, (new[] {4, 5, 6, 7, 8, 9, 10}, new[] {11, 12, 13, 14, 15}) },
        { 3, (new[] {6, 7, 8, 9, 10, 11, 12}, new[] {13, 14, 15, 16, 17, 18, 19, 20}) },
        { 4, (new[] {12, 13, 14, 15, 16, 17, 18}, new[] {22, 23, 24, 25, 26, 27, 28, 29, 30}) }
    };

    /// <summary>
    /// 랜덤한 공격 카드 생성
    /// </summary>
    public static CombatPage GenerateAttackCard(Sprite artwork = null)
    {
        CombatPage card = ScriptableObject.CreateInstance<CombatPage>();

        // 랜덤 코스트 결정
        int cost = GetRandomCost();
        card.lightCost = cost;

        // 랜덤 주사위 개수 결정
        int diceCount = GetRandomDiceCount();

        // 주사위 생성
        card.diceList = new List<CombatDice>();
        for (int i = 0; i < diceCount; i++)
        {
            var diceRange = attackDiceRanges[cost];
            int minValue = diceRange.minValues[Random.Range(0, diceRange.minValues.Length)];
            int maxValue = diceRange.maxValues[Random.Range(0, diceRange.maxValues.Length)];

            card.diceList.Add(new CombatDice
            {
                type = DiceType.Attack,
                minValue = minValue,
                maxValue = maxValue
            });
        }

        // 카드 이름 생성
        card.pageName = GenerateAttackCardName(cost, diceCount);

        // 일러스트 할당
        card.artwork = artwork;

        return card;
    }

    /// <summary>
    /// 랜덤한 방어 카드 생성
    /// </summary>
    public static CombatPage GenerateDefenseCard(Sprite artwork = null)
    {
        CombatPage card = ScriptableObject.CreateInstance<CombatPage>();

        // 랜덤 코스트 결정
        int cost = GetRandomCost();
        card.lightCost = cost;

        // 랜덤 주사위 개수 결정
        int diceCount = GetRandomDiceCount();

        // 주사위 생성
        card.diceList = new List<CombatDice>();
        for (int i = 0; i < diceCount; i++)
        {
            var diceRange = defenseDiceRanges[cost];
            int minValue = diceRange.minValues[Random.Range(0, diceRange.minValues.Length)];
            int maxValue = diceRange.maxValues[Random.Range(0, diceRange.maxValues.Length)];

            card.diceList.Add(new CombatDice
            {
                type = DiceType.Defense,
                minValue = minValue,
                maxValue = maxValue
            });
        }

        // 카드 이름 생성
        card.pageName = GenerateDefenseCardName(cost, diceCount);

        // 일러스트 할당
        card.artwork = artwork;

        return card;
    }

    /// <summary>
    /// 확률 테이블에 따라 랜덤 코스트 선택
    /// </summary>
    private static int GetRandomCost()
    {
        float roll = Random.value; // 0.0 ~ 1.0
        float cumulative = 0f;

        foreach (var (cost, probability) in costProbabilities)
        {
            cumulative += probability;
            if (roll <= cumulative)
            {
                return cost;
            }
        }

        return 2; // 기본값 (fallback)
    }

    /// <summary>
    /// 확률 테이블에 따라 랜덤 주사위 개수 선택
    /// </summary>
    private static int GetRandomDiceCount()
    {
        float roll = Random.value;
        float cumulative = 0f;

        foreach (var (count, probability) in diceProbabilities)
        {
            cumulative += probability;
            if (roll <= cumulative)
            {
                return count;
            }
        }

        return 1; // 기본값
    }

    /// <summary>
    /// 공격 카드 이름 생성
    /// </summary>
    private static string GenerateAttackCardName(int cost, int diceCount)
    {
        string[] attackNames = { "찌르기", "베기", "강타", "연타", "난타", "휘두르기", "돌진" };
        string[] prefixes = { "날카로운", "강렬한", "맹렬한", "신속한", "치명적인", "격렬한" };

        if (cost >= 3 || diceCount >= 2)
        {
            return $"{prefixes[Random.Range(0, prefixes.Length)]} {attackNames[Random.Range(0, attackNames.Length)]}";
        }

        return attackNames[Random.Range(0, attackNames.Length)];
    }

    /// <summary>
    /// 방어 카드 이름 생성
    /// </summary>
    private static string GenerateDefenseCardName(int cost, int diceCount)
    {
        string[] defenseNames = { "막기", "수비", "회피", "방패", "철벽", "반격", "카운터" };
        string[] prefixes = { "견고한", "완벽한", "신속한", "철저한", "탄탄한", "단단한" };

        if (cost >= 3 || diceCount >= 2)
        {
            return $"{prefixes[Random.Range(0, prefixes.Length)]} {defenseNames[Random.Range(0, defenseNames.Length)]}";
        }

        return defenseNames[Random.Range(0, defenseNames.Length)];
    }

    /// <summary>
    /// 코스트에 맞는 일러스트 자동 선택 (옵션)
    /// </summary>
    public static Sprite GetArtworkForCard(DiceType type, int cost, Sprite[] normalArtworks, Sprite[] highCostArtworks)
    {
        if (cost >= 4 && highCostArtworks != null && highCostArtworks.Length > 0)
        {
            int index = type == DiceType.Attack ? 0 : 1;
            return index < highCostArtworks.Length ? highCostArtworks[index] : null;
        }

        if (normalArtworks != null && normalArtworks.Length > 0)
        {
            int index = type == DiceType.Attack ? 0 : 1;
            return index < normalArtworks.Length ? normalArtworks[index] : null;
        }

        return null;
    }
}
