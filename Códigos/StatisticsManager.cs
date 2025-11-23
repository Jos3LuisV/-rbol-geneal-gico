using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;

//Calcula y muestra estadísticas de distancias entre familiares en el árbol genealógico.

public class StatisticsManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text farthestPairText; // Muestra el par de familiares más lejanos
    public TMP_Text closestPairText; // Muestra el par de familiares más cercanos
    public TMP_Text averageDistanceText; // Muestra la distancia promedio entre familiares
    public Button backButton; // Botón para regresar a la escena principal

    // Configuración del mapa para calcular distancias correctamente
    private float mapWidth = 1920f; // Configuración del mapa para calcular distancias correctamente
    private float mapHeight = 1080f; // Alto del canvas del mapa
    private float leftMargin = 650f; // Margen izquierdo del mapa
    private float topMargin = 200f; // Margen superior del mapa
    private float bottomMargin = 50f; // Margen inferior del mapa

    void Start()
    {
        Debug.Log("StatisticsManager iniciado");

        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(ReturnToMainScene); // Configura el botón de regreso
        }

        CalculateStatistics(); // Calcula las estadísticas al iniciar
    }

    public void ReturnToMainScene() // Regresa a la escena del juego principal
    {
        Debug.Log("Regresando a escena principal");
        SceneManager.LoadScene("Juego");
    }

    private void CalculateStatistics() // Calcula todas las estadísticas de distancias familiares
    {
        List<FamilyMember> allMembers = FamilyDataManager.GetAllFamilyMembers();

        if (allMembers.Count < 2)
        {
            SetErrorTexts("Se necesitan al menos 2 familiares"); // Error si no hay suficientes miembros
            return;
        }

        Debug.Log($"Calculando estadísticas para {allMembers.Count} familiares");

        var distances = CalculateAllDistances(allMembers); // Calcula distancias entre todos los pares

        if (distances.Count == 0)
        {
            SetErrorTexts("No hay conexiones entre familiares"); // Error si no hay conexiones
            return;
        }

        DisplayStatistics(distances); // Muestra las estadísticas calculadas
    }

    private void SetErrorTexts(string message) // Muestra mensajes de error en todos los textos
    {
        farthestPairText.text = message;
        closestPairText.text = message;
        averageDistanceText.text = message;
    }

    private void DisplayStatistics(List<FamilyDistance> distances) // Muestra las estadísticas en la UI
    {
        var farthestPair = distances.OrderByDescending(d => d.distance).First(); // Encuentra par más lejano
        var closestPair = distances.OrderBy(d => d.distance).First(); // Encuentra par más cercano
        float averageDistance = distances.Average(d => d.distance); // Calcula distancia promedio

        // Muestra los resultados formateados en la interfaz
        farthestPairText.text = $"<b>Familiares más separados:</b>\n" +
                               $"{farthestPair.member1.name} y {farthestPair.member2.name}\n" +
                               $"<color=green>Distancia: {farthestPair.distance:F0}Km</color>";

        closestPairText.text = $"<b>Familiares más cercanos:</b>\n" +
                              $"{closestPair.member1.name} y {closestPair.member2.name}\n" +
                              $"<color=green>Distancia: {closestPair.distance:F0}Km</color>";

        averageDistanceText.text = $"<b>Distancia promedio entre familiares:</b>\n" +
                                  $"<color=green>{averageDistance:F0}Km</color>";

        // Logs para debugging
        Debug.Log($"ESTADÍSTICAS CALCULADAS:");
        foreach (var dist in distances)
        {
            Debug.Log($"    {dist.member1.name} ↔ {dist.member2.name}: {dist.distance:F0}px");
        }
        Debug.Log($"    Más separados: {farthestPair.distance:F0}px");
        Debug.Log($"    Más cercanos: {closestPair.distance:F0}px");
        Debug.Log($"    Promedio: {averageDistance:F0}px");
    }

    private List<FamilyDistance> CalculateAllDistances(List<FamilyMember> members) // Calcula distancias entre todos los pares conectados
    {
        List<FamilyDistance> distances = new List<FamilyDistance>();
        HashSet<string> processedPairs = new HashSet<string>(); // Evita duplicados

        foreach (var member1 in members)
        {
            if (member1.connectedMemberIds == null) continue;

            foreach (string connectedId in member1.connectedMemberIds)
            {
                var member2 = members.Find(m => m.idNumber == connectedId);
                if (member2 != null)
                {
                    string pairKey = GetPairKey(member1.idNumber, member2.idNumber); // Clave única para el par

                    if (!processedPairs.Contains(pairKey)) // Solo procesa pares no calculados
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

    private string GetPairKey(string id1, string id2) // Genera clave única para un par de miembros
    {
        return id1.CompareTo(id2) < 0 ? $"{id1}_{id2}" : $"{id2}_{id1}";
    }

    private float CalculateDistance(FamilyMember member1, FamilyMember member2) // Calcula distancia entre dos miembros
    {
        Vector2 pos1 = ConvertCoordinatesToMapPosition(member1.coordinates);
        Vector2 pos2 = ConvertCoordinatesToMapPosition(member2.coordinates);

        float distance = Vector2.Distance(pos1, pos2);

        Debug.Log($"CALCULO: {member1.name}({member1.coordinates}) → {pos1} | " +
                 $"{member2.name}({member2.coordinates}) → {pos2} | " +
                 $"Distancia: {distance:F0}px");

        return distance;
    }

    private Vector2 ConvertCoordinatesToMapPosition(Vector2 coordinates) // Convierte coordenadas a posición en el mapa
    {
        // Rangos de coordenadas predefinidos
        float minLatitude = 35.5f;
        float maxLatitude = 150.75f;
        float minLongitude = 139.65f;
        float maxLongitude = 330.5f;

        // Normaliza coordenadas a porcentajes (0-1)
        float horizontalPercent = (coordinates.y - minLongitude) / (maxLongitude - minLongitude);
        float verticalPercent = (coordinates.x - minLatitude) / (maxLatitude - minLatitude);

        horizontalPercent = Mathf.Clamp01(horizontalPercent);
        verticalPercent = Mathf.Clamp01(verticalPercent);

        // Calcula posición en el canvas del mapa
        float usableWidth = mapWidth - (leftMargin + 0f);
        float usableHeight = mapHeight - (topMargin + bottomMargin);

        float xInMap = leftMargin + (horizontalPercent * usableWidth) - (mapWidth * 0.5f);
        float yInMap = topMargin + (verticalPercent * usableHeight) - (mapHeight * 0.5f);

        return new Vector2(xInMap, yInMap);
    }
}

[System.Serializable]
public class FamilyDistance // Representa la distancia entre dos familiares
{
    public FamilyMember member1; // Primer miembro del par
    public FamilyMember member2; // Segundo miembro del par
    public float distance; // Distancia entre ellos

    public FamilyDistance(FamilyMember m1, FamilyMember m2, float dist) // Constructor
    {
        member1 = m1;
        member2 = m2;
        distance = dist;
    }
}
