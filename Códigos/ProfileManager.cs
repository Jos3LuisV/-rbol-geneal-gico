using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;


//Muestra la información detallada de un miembro familiar en una pantalla de perfil.

public class ProfileManager : MonoBehaviour
{
    [Header("UI References")]
    public RawImage profilePhoto; // Muestra la foto del familiar
    public TMP_Text nameText; // Muestra el nombre del familiar
    public TMP_Text idText; // Muestra la cédula del familiar
    public TMP_Text ageText; // Muestra la edad del familiar
    public TMP_Text statusText; // Muestra si está vivo o fallecido
    public TMP_Text birthDateText; // Muestra la fecha de nacimiento
    public TMP_Text coordinatesText; // Muestra las coordenadas en el mapa
    public Button backButton; // Botón para regresar al árbol familiar

    private FamilyMember currentMember; // Almacena los datos del miembro actual

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

        LoadMemberData(); // Carga los datos del miembro al iniciar
    }

    public void ReturnToMainScene() // Regresa a la escena del árbol familiar
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

    private void LoadMemberData() // Carga y muestra los datos del miembro
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
                profilePhoto.texture = currentMember.photo; // Muestra la foto si existe
            }

            Debug.Log("Datos cargados correctamente");
        }
        else
        {
            Debug.LogError("No se encontraron datos del miembro");
        }
    }
}