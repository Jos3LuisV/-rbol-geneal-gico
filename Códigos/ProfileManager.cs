using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class ProfileManager : MonoBehaviour
{
    [Header("UI References")]
    public RawImage profilePhoto;
    public TMP_Text nameText;
    public TMP_Text idText;
    public TMP_Text ageText;
    public TMP_Text statusText;
    public TMP_Text birthDateText;
    public TMP_Text coordinatesText;
    public Button backButton;

    private FamilyMember currentMember;

    void Start()
    {
        Debug.Log("ProfileManager iniciado");

        // Verificar que el botón esté asignado
        if (backButton == null)
        {
            Debug.LogError("BackButton no está asignado en el Inspector!");
        }
        else
        {
            Debug.Log("BackButton asignado correctamente");
        }

        LoadMemberData();

        // NOTA: Quitamos el backButton.onClick.AddListener de aquí
        // porque lo configuraremos en el Inspector
    }

    // 🔥 CAMBIO IMPORTANTE: Hacer el método PÚBLICO
    public void ReturnToMainScene()
    {
        Debug.Log("Botón Regresar - CLIC RECIBIDO!");

        try
        {
            SceneManager.LoadScene("Juego", LoadSceneMode.Single);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error al cargar escena: {e.Message}");
        }
    }

    private void LoadMemberData()
    {
        currentMember = ProfileDataTransporter.GetMemberData();

        if (currentMember != null)
        {
            nameText.text = $"Nombre: {currentMember.name}";
            idText.text = $"Cédula: {currentMember.idNumber}";
            ageText.text = $"Edad: {currentMember.age} años";
            statusText.text = currentMember.isAlive ? "Estado: Vivo" : "Estado: Fallecido";
            birthDateText.text = $"Fecha Nacimiento: {currentMember.birthDate.ToShortDateString()}";
            coordinatesText.text = $"Coordenadas: {currentMember.coordinates.x}, {currentMember.coordinates.y}";

            if (currentMember.photo != null)
            {
                profilePhoto.texture = currentMember.photo;
            }

            Debug.Log("Datos cargados correctamente");
        }
        else
        {
            Debug.LogError("No se encontraron datos del miembro");
        }
    }
}