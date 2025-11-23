using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;

public class StatisticsManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text farthestPairText;
    public TMP_Text closestPairText;
    public TMP_Text averageDistanceText;
    public Button backButton;

    // ✅ VARIABLES PARA COINCIDIR CON FamilyMemberManager
    private float mapWidth = 1920f;  // Ajusta según tu Canvas real
    private float mapHeight = 1080f; // Ajusta según tu Canvas real
    private float leftMargin = 650f;
    private float topMargin = 200f;
    private float bottomMargin = 50f;

    void Start()
    {
        Debug.Log("StatisticsManager iniciado");

        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(ReturnToMainScene);
        }

        CalculateStatistics();
    }

    public void ReturnToMainScene()
    {
        Debug.Log("Regresando a escena principal");
        SceneManager.LoadScene("Juego");
    }

    private void CalculateStatistics()
    {
        List<FamilyMember> allMembers = FamilyDataManager.GetAllFamilyMembers();

        if (allMembers.Count < 2)
        {
            SetErrorTexts("Se necesitan al menos 2 familiares");
            return;
        }

        Debug.Log($"Calculando estadísticas para {allMembers.Count} familiares");

        var distances = CalculateAllDistances(allMembers);

        if (distances.Count == 0)
        {
            SetErrorTexts("No hay conexiones entre familiares");
            return;
        }

        DisplayStatistics(distances);
    }

    private void SetErrorTexts(string message)
    {
        farthestPairText.text = message;
        closestPairText.text = message;
        averageDistanceText.text = message;
    }

    private void DisplayStatistics(List<FamilyDistance> distances)
    {
        var farthestPair = distances.OrderByDescending(d => d.distance).First();
        var closestPair = distances.OrderBy(d => d.distance).First();
        float averageDistance = distances.Average(d => d.distance);

        // ✅ TEXTO MEJORADO Y MÁS DESCRIPTIVO
        farthestPairText.text = $"<b>Familiares más separados:</b>\n" +
                               $"{farthestPair.member1.name} y {farthestPair.member2.name}\n" +
                               $"<color=green>Distancia: {farthestPair.distance:F0}Km</color>";

        closestPairText.text = $"<b>Familiares más cercanos:</b>\n" +
                              $"{closestPair.member1.name} y {closestPair.member2.name}\n" +
                              $"<color=green>Distancia: {closestPair.distance:F0}Km</color>";

        averageDistanceText.text = $"<b>Distancia promedio entre familiares:</b>\n" +
                                  $"<color=green>{averageDistance:F0}Km</color>";

        // ✅ DEBUG DETALLADO para verificar
        Debug.Log($"ESTADÍSTICAS CALCULADAS:");
        foreach (var dist in distances)
        {
            Debug.Log($"    {dist.member1.name} ↔ {dist.member2.name}: {dist.distance:F0}px");
        }
        Debug.Log($"    Más separados: {farthestPair.distance:F0}px");
        Debug.Log($"    Más cercanos: {closestPair.distance:F0}px");
        Debug.Log($"    Promedio: {averageDistance:F0}px");
    }

    private List<FamilyDistance> CalculateAllDistances(List<FamilyMember> members)
    {
        List<FamilyDistance> distances = new List<FamilyDistance>();
        HashSet<string> processedPairs = new HashSet<string>();

        foreach (var member1 in members)
        {
            if (member1.connectedMemberIds == null) continue;

            foreach (string connectedId in member1.connectedMemberIds)
            {
                var member2 = members.Find(m => m.idNumber == connectedId);
                if (member2 != null)
                {
                    string pairKey = GetPairKey(member1.idNumber, member2.idNumber);

                    if (!processedPairs.Contains(pairKey))
                    {
                        float distance = CalculateDistance(member1, member2);
                        distances.Add(new FamilyDistance(member1, member2, distance));
                        processedPairs.Add(pairKey);
                    }
                }
            }
        }

        return distances;
    }

    private string GetPairKey(string id1, string id2)
    {
        return id1.CompareTo(id2) < 0 ? $"{id1}_{id2}" : $"{id2}_{id1}";
    }

    private float CalculateDistance(FamilyMember member1, FamilyMember member2)
    {
        Vector2 pos1 = ConvertCoordinatesToMapPosition(member1.coordinates);
        Vector2 pos2 = ConvertCoordinatesToMapPosition(member2.coordinates);

        float distance = Vector2.Distance(pos1, pos2);

        Debug.Log($"CALCULO: {member1.name}({member1.coordinates}) → {pos1} | " +
                 $"{member2.name}({member2.coordinates}) → {pos2} | " +
                 $"Distancia: {distance:F0}px");

        return distance;
    }

    private Vector2 ConvertCoordinatesToMapPosition(Vector2 coordinates)
    {
        float minLatitude = 35.5f;
        float maxLatitude = 150.75f;
        float minLongitude = 139.65f;
        float maxLongitude = 330.5f;

        float horizontalPercent = (coordinates.y - minLongitude) / (maxLongitude - minLongitude);
        float verticalPercent = (coordinates.x - minLatitude) / (maxLatitude - minLatitude);

        horizontalPercent = Mathf.Clamp01(horizontalPercent);
        verticalPercent = Mathf.Clamp01(verticalPercent);

        float usableWidth = mapWidth - (leftMargin + 0f);
        float usableHeight = mapHeight - (topMargin + bottomMargin);

        float xInMap = leftMargin + (horizontalPercent * usableWidth) - (mapWidth * 0.5f);
        float yInMap = topMargin + (verticalPercent * usableHeight) - (mapHeight * 0.5f);

        return new Vector2(xInMap, yInMap);
    }
}

[System.Serializable]
public class FamilyDistance
{
    public FamilyMember member1;
    public FamilyMember member2;
    public float distance;

    public FamilyDistance(FamilyMember m1, FamilyMember m2, float dist)
    {
        member1 = m1;
        member2 = m2;
        distance = dist;
    }
}
