using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

public class RankPage : MonoBehaviour
{
    [SerializeField] Transform contentRoot;
    [SerializeField] GameObject rowPrefab;

    StageResultList allData;

    private void Awake()
    {
        allData = StageResultSaver.LoadRank();
        RefreshRankList();
    }

    void RefreshRankList()
    {
        foreach (Transform child in contentRoot)
        {
            Destroy(child.gameObject);
        }

        var stageGroups = allData.results
            .GroupBy(r => r.stage)
            .OrderBy(g => g.Key);

        foreach (var stageGroup in stageGroups)
        {
            GameObject stageHeader = Instantiate(rowPrefab, contentRoot);
            TMP_Text stageHeaderText = stageHeader.GetComponentInChildren<TMP_Text>();
            stageHeaderText.text = $"=== Stage {stageGroup.Key} ===";
            stageHeaderText.fontStyle = FontStyles.Bold;

            var topScoresByPlayer = stageGroup
                .GroupBy(r => r.playerName)
                .Select(playerGroup => new {
                    playerName = playerGroup.Key,
                    bestScore = playerGroup.Max(r => r.score)
                })
                .OrderByDescending(x => x.bestScore)
                .ToList();

            for (int i = 0; i < topScoresByPlayer.Count; i++)
            {
                GameObject row = Instantiate(rowPrefab, contentRoot);
                TMP_Text rankText = row.GetComponentInChildren<TMP_Text>();
                rankText.text = $"  {i + 1}. {topScoresByPlayer[i].playerName} - {topScoresByPlayer[i].bestScore}";
            }

            GameObject spacer = Instantiate(rowPrefab, contentRoot);
            TMP_Text spacerText = spacer.GetComponentInChildren<TMP_Text>();
            spacerText.text = "";
        }
    }
}
