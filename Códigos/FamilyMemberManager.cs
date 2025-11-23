using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static FamilyMemberManager;

[System.Serializable]
public class Location
{
    public string name; // Nombre de la ubicación
    public float latitude; // Coordenada X en el mapa
    public float longitude; // Coordenada Y en el mapa

    public Location(string name, float latitude, float longitude)
    {
        this.name = name;
        this.latitude = latitude;
        this.longitude = longitude;
    }
}

[System.Serializable]
public class FamilyMember
{
    public string name; // Nombre del familiar
    public Texture2D photo; // Foto del familiar
    public string idNumber; // Identificador único
    public Vector2 coordinates; // Posición en el mapa
    public DateTime birthDate; // Fecha de nacimiento
    public bool isAlive; // Estado vital
    public int age; // Edad del familiar
    public FamilyRole role; // Rol en la familia

    // Lista de conexiones
    public List<string> connectedMemberIds = new List<string>(); // IDs de familiares conectados

    public FamilyMember(string name, string idNumber, Vector2 coordinates, DateTime birthDate, int age, FamilyRole role, bool isAlive = true)
    {
        this.name = name;
        this.idNumber = idNumber;
        this.coordinates = coordinates;
        this.birthDate = birthDate;
        this.age = age;
        this.role = role;
        this.isAlive = isAlive;
    }

    public void AddConnection(string memberId) // Añade conexión con otro familiar
    {
        if (!connectedMemberIds.Contains(memberId) && memberId != idNumber)
        {
            connectedMemberIds.Add(memberId);
        }
    }
}

public class FamilyMemberManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField nameInput; // Campo para nombre
    public TMP_InputField idInput; // Campo para nombre
    public TMP_Dropdown locationDropdown; // Dropdown de ubicaciones
    public TMP_InputField birthDateInput; // Campo para fecha nacimiento
    public TMP_InputField ageInput; // Campo para edad
    public Toggle isAliveToggle; // Toggle para estado vital
    public RawImage photoPreview; // Preview de la foto
    public Button takePhotoButton; // Botón para tomar foto
    public Button saveButton; // Botón guardar
    public Button cancelButton; // Botón cancelar
    public Transform familyMembersContainer; // Contenedor de miembros en UI
    public GameObject familyMemberUIPrefab; // Prefab para mostrar miembros

    [Header("Zoom Settings")]
    public float zoomSpeed = 0.2f; // Velocidad del zoom
    public float minZoom = 0.5f; // Zoom mínimo
    public float maxZoom = 3.0f; // Zoom máximo
    private float currentZoom = 1.0f; // Zoom actual

    [Header("Distance Settings")]
    public bool showDistances = true; // Mostrar distancias
    private Dictionary<string, GameObject> distanceTexts = new Dictionary<string, GameObject>(); // Textos de distancia

    [Header("Pan Settings")]
    public bool enablePan = true; // Habilitar arrastre
    private bool isDragging = false; // Estado de arrastre
    private Vector2 dragStartPosition; // Posición inicial del arrastre
    private Vector2 mapStartPosition; // Posición inicial del mapa

    [Header("Pan Limits")]
    public bool enablePanLimits = true; // Límites de arrastre
    public float panLimitMargin = 50f; // Margen en píxeles

    [Header("Zoom Buttons")]
    public Button zoomInButton; // Botón acercar
    public Button zoomOutButton; // Botón alejar
    public Button resetZoomButton; // Botón alejar

    [Header("Statistics Button")]
    public Button statisticsButton; // Botón para estadísticas

    [Header("Map Reference")]
    public RectTransform mapRect; // Referencia al mapa

    [Header("File Browser Settings")]
    public string[] supportedImageExtensions = { "*.png", "*.jpg", "*.jpeg" }; // Extensiones de imagen soportadas

    [Header("Graph Settings")]
    public Color lineColor = Color.black; // Color de las líneas
    public float lineWidth = 4.0f; // Ancho de las líneas

    [Header("Predefined Locations")]
public List<Location> predefinedLocations = new List<Location> // Ubicaciones predefinidas
{
    new Location("San José, Costa Rica", 35.7f, 189.7f),
    new Location("Alajuela, Costa Rica", 85.65f, 150.6f),
    new Location("Cartago, Costa Rica", 40.68f, 150.65f),
    new Location("Heredia, Costa Rica", 70.72f, 239.55f),
    new Location("Guanacaste, Costa Rica", 100.6f, 190.75f),
    new Location("Puntarenas, Costa Rica", 140.75f, 239.58f),
    new Location("Limón, Costa Rica", 120.62f, 150.62f),
    new Location("Ciudad de México, México", 45.8f, 300.8f),
    new Location("Madrid, España", 120.55f, 330.5f),
    new Location("Buenos Aires, Argentina", 110.85f, 250.45f),
    new Location("Nueva York, USA", 35.5f, 235.85f),
    new Location("Tokio, Japón", 80f, 300.8f),
};
    private FamilyMember currentMember; // Miembro actual
    private List<FamilyMember> familyMembers = new List<FamilyMember>(); // Lista de miembros
    private Texture2D currentPhoto; // Foto actual
    private Dictionary<string, FamilyMember> familyMembersDict = new Dictionary<string, FamilyMember>(); // Diccionario de miembros
    private List<GameObject> connectionLines = new List<GameObject>(); // Líneas de conexión
    private Dictionary<string, GameObject> memberIcons = new Dictionary<string, GameObject>(); // Iconos de miembros

    void Start() // Inicialización
    {
        takePhotoButton.onClick.AddListener(SelectImageFile);
        saveButton.onClick.AddListener(SaveFamilyMember);
        cancelButton.onClick.AddListener(CancelEntry);
        isAliveToggle.isOn = false;

        // Configurar botones de zoom
        if (zoomInButton != null)
            zoomInButton.onClick.AddListener(ZoomIn);

        if (zoomOutButton != null)
            zoomOutButton.onClick.AddListener(ZoomOut);

        if (resetZoomButton != null)
            resetZoomButton.onClick.AddListener(ResetZoom);

        LoadFamilyData();

        familyMembersDict = new Dictionary<string, FamilyMember>();
        connectionLines = new List<GameObject>();

        InitializeLocationDropdown();
        InitializeFamilyRoleDropdown();

        // FORZAR cálculo de distancias al inicio
        StartCoroutine(InitializeDistances());

        if (statisticsButton != null)
        {
            statisticsButton.onClick.AddListener(OpenStatistics);
            Debug.Log("Botón de estadísticas configurado");
        }
    }

    private void OpenStatistics() // Abre escena de estadísticas
    {
        Debug.Log("Abriendo escena de estadísticas");
        SceneManager.LoadScene("Estadisticas");
    }
    private IEnumerator InitializeDistances() // Inicializa distancias
    {
        yield return new WaitForSeconds(0.5f);

        if (showDistances && familyMembers.Count > 0)
        {
            Debug.Log("Inicializando distancias...");
            ShowDistancesBetweenMembers();
        }

        VerifyConnections();
        ScheduleRedraw();
    }

    void Update() // Actualización por frame
    {
        HandlePan();
    }

    public enum FamilyRole // Roles familiares
    {
        Abuelo,
        Abuela,
        Padre,
        Madre,
        Hijo,
        Hija,
        Hermano,
        Hermana,
        Tio,
        Tia,
        Primo,
        Prima,
        Yo
    }

    [Header("Family Role System")]
    public TMP_Dropdown familyRoleDropdown; // Dropdown de roles

    private void InitializeFamilyRoleDropdown() // Inicializa dropdown de roles
    {
        if (familyRoleDropdown != null)
        {
            familyRoleDropdown.ClearOptions();
            List<string> roleOptions = new List<string>
        {
            "Parentesco",
            "Abuelo", "Abuela", "Padre", "Madre", "Hermano", "Hermana","Hijo","Hija",
            "Tío", "Tía", "Primo", "Prima", "Yo"
        };
            familyRoleDropdown.AddOptions(roleOptions);
            familyRoleDropdown.value = 0;
            
        }
    }

    private void HandlePan() // Maneja el arrastre del mapa
    {
        if (!enablePan || mapRect == null) return;

        Mouse mouse = Mouse.current;
        if (mouse == null) return;

        if (mouse.leftButton.wasPressedThisFrame)
        {
            isDragging = true;
            dragStartPosition = mouse.position.ReadValue();
            mapStartPosition = mapRect.anchoredPosition;
            Debug.Log("Iniciando arrastre");
        }

        if (isDragging && mouse.leftButton.isPressed)
        {
            Vector2 currentMousePos = mouse.position.ReadValue();
            Vector2 dragDelta = (currentMousePos - dragStartPosition);

            Vector2 newPosition = mapStartPosition + dragDelta;

            // Aplicar límites si están habilitados
            if (enablePanLimits)
            {
                newPosition = ApplyPanLimits(newPosition);
            }

            mapRect.anchoredPosition = newPosition;
        }

        if (mouse.leftButton.wasReleasedThisFrame && isDragging)
        {
            isDragging = false;
            Debug.Log("Terminando arrastre");
        }
    }

    private Vector2 ApplyPanLimits(Vector2 position) // Aplica límites al arrastre
    {
        if (mapRect == null) return position;

        RectTransform canvasRect = mapRect.parent as RectTransform;
        if (canvasRect == null) return position;

        float canvasWidth = canvasRect.rect.width;
        float canvasHeight = canvasRect.rect.height;

        // Calcular los límites basados en el zoom actual
        float mapWidth = mapRect.rect.width * currentZoom;
        float mapHeight = mapRect.rect.height * currentZoom;

        // Si el mapa es más pequeño que el canvas, centrarlo
        if (mapWidth <= canvasWidth)
        {
            position.x = 0;
        }
        else
        {
            float maxOffsetX = (mapWidth - canvasWidth) / 2f;
            position.x = Mathf.Clamp(position.x, -maxOffsetX, maxOffsetX);
        }

        if (mapHeight <= canvasHeight)
        {
            position.y = 0;
        }
        else
        {
            float maxOffsetY = (mapHeight - canvasHeight) / 2f;
            position.y = Mathf.Clamp(position.y, -maxOffsetY, maxOffsetY);
        }

        return position;
    }

    // Métodos de zoom
    public void ZoomIn() // Acerca el mapa
    {
        float oldZoom = currentZoom;
        currentZoom = Mathf.Clamp(currentZoom + zoomSpeed, minZoom, maxZoom);

        if (oldZoom != currentZoom)
        {
            ApplyZoom();
            Debug.Log($"Zoom In: {currentZoom}x");
        }
    }

    public void ZoomOut() // Aleja el mapa
    {
        float oldZoom = currentZoom;
        currentZoom = Mathf.Clamp(currentZoom - zoomSpeed, minZoom, maxZoom);

        if (oldZoom != currentZoom)
        {
            ApplyZoom();
            Debug.Log($"Zoom Out: {currentZoom}x");
        }
    }

    public void ResetZoom() // Resetea el zoom
    {
        currentZoom = 1.0f;
        mapRect.anchoredPosition = Vector2.zero;
        ApplyZoom();
        Debug.Log($"Zoom Reset: {currentZoom}x");
    }

    private void ApplyZoom() // Aplica el zoom al mapa
    {
        if (mapRect != null)
        {
            // Guardar la posición del mouse relativa al mapa antes del zoom
            Vector2 mousePosition = GetMousePositionInMap();

            // Aplicar el nuevo zoom
            mapRect.localScale = Vector3.one * currentZoom;

            // Recalcular la posición para mantener el punto bajo el mouse en la misma posición
            if (mousePosition != Vector2.zero)
            {
                Vector2 newMousePosition = GetMousePositionInMap();
                Vector2 positionDelta = mousePosition - newMousePosition;
                mapRect.anchoredPosition += positionDelta * currentZoom;
            }

            // Aplicar límites
            if (enablePanLimits)
            {
                mapRect.anchoredPosition = ApplyPanLimits(mapRect.anchoredPosition);
            }

            Debug.Log($"Zoom aplicado: {currentZoom}x - Posición: {mapRect.anchoredPosition}");
        }
    }

    private Vector2 GetMousePositionInMap() // Obtiene posición del mouse en el mapa
    {
        if (mapRect == null) return Vector2.zero;

        Mouse mouse = Mouse.current;
        if (mouse == null) return Vector2.zero;

        Vector2 mouseScreenPos = mouse.position.ReadValue();
        Vector2 localPoint;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            mapRect, mouseScreenPos, null, out localPoint);

        return localPoint;
    }

    private void LoadFamilyData() // Carga datos de familia
    {
        // Cargar miembros desde el DataManager
        familyMembers = FamilyDataManager.GetAllFamilyMembers();

        // Reconstruir el diccionario
        familyMembersDict.Clear();
        foreach (var member in familyMembers)
        {
            familyMembersDict[member.idNumber] = member;
        }

        UnityEngine.Debug.Log($"Datos cargados: {familyMembers.Count} miembros");

        // Recrear los iconos en el mapa
        RecreateMapIcons();

        Debug.Log("Datos cargados correctamente");
    }

    // Método para inicializar el dropdown
    private void InitializeLocationDropdown()
    {
        if (locationDropdown == null)
        {
            Debug.LogError("Location Dropdown no está asignado en el Inspector");
            return;
        }

        // Limpiar opciones existentes
        locationDropdown.ClearOptions();

        // Crear lista de opciones
        List<string> locationOptions = new List<string>();

        // Añadir opción por defecto
        locationOptions.Add("Seleccione el lugar de residencia");

        // Añadir todos los lugares predefinidos
        foreach (Location location in predefinedLocations)
        {
            locationOptions.Add(location.name);
        }

        // Añadir opciones al dropdown
        locationDropdown.AddOptions(locationOptions);

        // Configurar valor por defecto
        locationDropdown.value = 0;
        locationDropdown.RefreshShownValue();

        Debug.Log($"Dropdown inicializado con {predefinedLocations.Count} lugares");
    }

    public void SelectImageFile() // Seleccionar imagen
    {
        string extensions = "";
        foreach (string ext in supportedImageExtensions)
        {
            extensions += ext + ";";
        }
        extensions = extensions.TrimEnd(';');

        Debug.Log($"Buscando archivos con extensiones: {extensions}");

        string path = UnityEditor.EditorUtility.OpenFilePanel("Seleccionar imagen", "", extensions);

        if (!string.IsNullOrEmpty(path))
        {
            Debug.Log($"Archivo seleccionado: {path}");
            StartCoroutine(LoadImage(path));
        }
        else
        {
            Debug.Log("No se seleccionó ningún archivo");
        }
    }

    private IEnumerator LoadImage(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogError("El archivo no existe: " + filePath);
            yield break;
        }

        string fileExtension = Path.GetExtension(filePath).ToLower();
        Debug.Log($"Extensión del archivo: {fileExtension}");

        byte[] fileData = File.ReadAllBytes(filePath);
        Debug.Log($"Tamaño del archivo: {fileData.Length} bytes");

        Texture2D texture = new Texture2D(2, 2);

        try
        {
            bool loadSuccess = texture.LoadImage(fileData);

            if (loadSuccess)
            {
                Debug.Log($"Textura cargada exitosamente - Formato: {texture.format} - Tamaño: {texture.width}x{texture.height}");
                currentPhoto = texture;
                photoPreview.texture = currentPhoto;
                Debug.Log("Imagen cargada y asignada al preview: " + filePath);
            }
            else
            {
                Debug.LogError("Error: LoadImage() retornó false para el archivo: " + filePath);
                if (fileExtension == ".png")
                {
                    Debug.Log("Intentando método alternativo para PNG...");
                    texture = LoadPNG(filePath);
                    if (texture != null)
                    {
                        currentPhoto = texture;
                        photoPreview.texture = currentPhoto;
                        Debug.Log("PNG cargado con método alternativo");
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Excepción al cargar la imagen: {e.Message}");
        }

        yield return null;
    }

    private Texture2D LoadPNG(string filePath)
    {
        try
        {
            byte[] fileData = File.ReadAllBytes(filePath);
            Texture2D texture = new Texture2D(2, 2);
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;

            if (texture.LoadImage(fileData))
                return texture;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error en método alternativo PNG: {e.Message}");
        }
        return null;
    }

    public void SaveFamilyMember()
    {
        if (string.IsNullOrEmpty(nameInput.text) ||
            string.IsNullOrEmpty(idInput.text) ||
            string.IsNullOrEmpty(birthDateInput.text) ||
            string.IsNullOrEmpty(ageInput.text))
        {
            Debug.LogError("Por favor complete todos los campos obligatorios");
            return;
        }

        if (locationDropdown.value == 0)
        {
            Debug.LogError("Por favor seleccione un lugar de residencia");
            return;
        }

        if (familyRoleDropdown.value == 0)
        {
            Debug.LogError("Por favor seleccione un rol familiar");
            return;
        }

        try
        {
            // Obtener coordenadas y rol
            int selectedIndex = locationDropdown.value - 1;
            Location selectedLocation = predefinedLocations[selectedIndex];
            Vector2 coordinates = new Vector2(selectedLocation.latitude, selectedLocation.longitude);
            FamilyRole selectedRole = (FamilyRole)(familyRoleDropdown.value - 1);

            DateTime birthDate;
            if (!DateTime.TryParseExact(birthDateInput.text, "yyyy-MM-dd",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out birthDate))
            {
                Debug.LogError("Formato de fecha incorrecto. Use YYYY-MM-DD");
                return;
            }

            int age;
            if (!int.TryParse(ageInput.text, out age) || age < 0 || age > 150)
            {
                Debug.LogError("La edad debe ser un número entre 0 y 150");
                return;
            }

            bool isAlive = !isAliveToggle.isOn;

            // Crear miembro CON SU ROL
            currentMember = new FamilyMember(
                nameInput.text,
                idInput.text,
                coordinates,
                birthDate,
                age,
                selectedRole,
                isAlive
            );

            currentMember.photo = currentPhoto;

            // Guardar miembro
            familyMembers.Add(currentMember);
            familyMembersDict[currentMember.idNumber] = currentMember;
            FamilyDataManager.AddFamilyMember(currentMember);

            // CONEXIÓN SIMPLE Y DIRECTA
            ConnectNewMember(currentMember);

            AddFamilyMemberToUI(currentMember);
            PlaceOnMap(currentMember);
            DrawAllConnections();
            ClearForm();

            Debug.Log($" {currentMember.name} guardado como {selectedRole}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error al guardar miembro: {e.Message}\n{e.StackTrace}");
        }

        ScheduleRedraw();
    }


    private void ConnectNewMember(FamilyMember newMember) //Conectar los nodos
    {
        Debug.Log($"Conectando {newMember.name} como {newMember.role}");

        switch (newMember.role)
        {
            case FamilyRole.Abuelo:
            case FamilyRole.Abuela:
                // Conectar con padres, tíos
                ConnectToMembersWithRoles(newMember, new List<FamilyRole> {
                FamilyRole.Padre, FamilyRole.Madre, FamilyRole.Tio, FamilyRole.Tia
            });
                break;

            case FamilyRole.Padre:
            case FamilyRole.Madre:
                // Conectar con abuelos, hijos, hermanos, yo
                ConnectToMembersWithRoles(newMember, new List<FamilyRole> {
                FamilyRole.Abuelo, FamilyRole.Abuela
            });
                ConnectToMembersWithRoles(newMember, new List<FamilyRole> {
                FamilyRole.Hijo, FamilyRole.Hija, FamilyRole.Hermano, FamilyRole.Hermana, FamilyRole.Yo
            });
                break;

            case FamilyRole.Tio:
            case FamilyRole.Tia:
                // Conectar con abuelos, primos
                ConnectToMembersWithRoles(newMember, new List<FamilyRole> {
                FamilyRole.Abuelo, FamilyRole.Abuela
            });
                ConnectToMembersWithRoles(newMember, new List<FamilyRole> {
                FamilyRole.Primo, FamilyRole.Prima
            });
                break;

            case FamilyRole.Hijo:
            case FamilyRole.Hija:
            case FamilyRole.Hermano:
            case FamilyRole.Hermana:
            case FamilyRole.Yo:
                // Conectar con padres
                ConnectToMembersWithRoles(newMember, new List<FamilyRole> {
                FamilyRole.Padre, FamilyRole.Madre
            });
                break;

            case FamilyRole.Primo:
            case FamilyRole.Prima:
                // Conectar con tíos
                ConnectToMembersWithRoles(newMember, new List<FamilyRole> {
                FamilyRole.Tio, FamilyRole.Tia
            });
                break;
        }
    }

    private void ConnectToMembersWithRoles(FamilyMember member, List<FamilyRole> targetRoles)
    {
        foreach (var targetMember in familyMembers)
        {
            if (targetMember != member && targetRoles.Contains(targetMember.role))
            {
                CreateConnection(member, targetMember, $"{member.role}-{targetMember.role}");
            }
        }
    }

    private void CreateConnection(FamilyMember member1, FamilyMember member2, string connectionType)
    {
        if (member1 == null || member2 == null) return;
        if (member1.idNumber == member2.idNumber) return;

        bool connectionMade = false;

        if (!member1.connectedMemberIds.Contains(member2.idNumber))
        {
            member1.AddConnection(member2.idNumber);
            connectionMade = true;
        }

        if (!member2.connectedMemberIds.Contains(member1.idNumber))
        {
            member2.AddConnection(member1.idNumber);
            connectionMade = true;
        }

        if (connectionMade)
        {
            Debug.Log($"CONEXIÓN: {member1.name} ({member1.role}) ←→ {member2.name} ({member2.role})");
        }
    }


    private void RecreateMapIcons()
    {
        UnityEngine.Debug.Log("Recreando iconos en el mapa...");

        // Limpiar iconos existentes
        foreach (var icon in memberIcons.Values)
        {
            if (icon != null) Destroy(icon);
        }
        memberIcons.Clear();

        // Limpiar SOLO conexiones de líneas
        ClearConnections();

        // Crear nuevos iconos para cada miembro
        foreach (var member in familyMembers)
        {
            Vector2 mapPosition = ConvertCoordinatesToMapPosition(member.coordinates);
            CreateMapIcon(member, mapPosition);
        }

        // Redibujar conexiones DESPUÉS de crear todos los iconos
        DrawAllConnections();

        UnityEngine.Debug.Log($" {familyMembers.Count} iconos recreados, {distanceTexts.Count} distancias mantenidas");
    }

    private void VerifyConnections()
    {
        Debug.Log("=== VERIFICANDO CONEXIONES ===");

        int totalConnections = 0;
        foreach (var member in familyMembers)
        {
            foreach (string connectedId in member.connectedMemberIds)
            {
                if (familyMembersDict.ContainsKey(connectedId))
                {
                    totalConnections++;
                    Debug.Log($"Conexión válida: {member.name} ↔ {familyMembersDict[connectedId].name}");
                }
                else
                {
                    Debug.LogError($"Conexión inválida: {member.name} → ID: {connectedId}");
                }
            }
        }

        Debug.Log($"Total de conexiones válidas: {totalConnections}");
        Debug.Log("=== FIN VERIFICACIÓN ===");
    }

    private void DebugAllIconPositions()
    {
        Debug.Log("=== POSICIONES DE ICONOS ===");
        foreach (var kvp in memberIcons)
        {
            if (kvp.Value != null)
            {
                RectTransform rt = kvp.Value.GetComponent<RectTransform>();
                FamilyMember member = FamilyDataManager.GetFamilyMemberById(kvp.Key);
                if (member != null)
                {
                    Debug.Log($" {member.name}: {rt.anchoredPosition}");
                }
            }
        }
        Debug.Log("=== FIN POSICIONES ===");
    }

    // DIBUJAR CONEXIONES
    private void DrawAllConnections()
    {
        Debug.Log("Iniciando dibujo de conexiones...");

        // Limpiar SOLO conexiones de líneas, NO textos de distancia
        ClearConnections();

        int totalPossibleConnections = 0;
        int actualConnections = 0;
        HashSet<string> drawnConnections = new HashSet<string>();

        foreach (var member in familyMembers)
        {
            foreach (string connectedId in member.connectedMemberIds)
            {
                totalPossibleConnections++;
                string connectionKey = member.idNumber.CompareTo(connectedId) < 0
                    ? $"{member.idNumber}_{connectedId}"
                    : $"{connectedId}_{member.idNumber}";

                if (!drawnConnections.Contains(connectionKey) && familyMembersDict.ContainsKey(connectedId))
                {
                    FamilyMember connectedMember = familyMembersDict[connectedId];
                    DrawConnection(member, connectedMember);
                    drawnConnections.Add(connectionKey);
                    actualConnections++;
                }
            }
        }

        // LAS DISTANCIAS SE MANTIENEN - solo se actualizan si es necesario
        if (actualConnections > 0 && distanceTexts.Count == 0)
        {
            Debug.Log($"Mostrando distancias para {actualConnections} conexiones");
            ShowDistancesBetweenMembers();
        }

        Debug.Log($"Conexiones: {actualConnections}, Distancias: {distanceTexts.Count}");
    }

    private void DrawConnection(FamilyMember member1, FamilyMember member2)
    {
        string lineName = $"Line_{member1.idNumber}_{member2.idNumber}";

        // Verificar si la línea ya existe
        if (connectionLines.Exists(line => line != null && line.name == lineName))
            return;

        Debug.Log($"Dibujando línea: {member1.name} ↔ {member2.name}");

        // Obtener los objetos de iconos directamente
        GameObject icon1 = GetMemberIcon(member1.idNumber);
        GameObject icon2 = GetMemberIcon(member2.idNumber);

        if (icon1 == null || icon2 == null)
        {
            Debug.LogError($"No se encontraron iconos para: {member1.name} o {member2.name}");
            return;
        }

        // Obtener posiciones CENTRO de los iconos
        Vector2 pos1 = GetIconCenterPosition(icon1);
        Vector2 pos2 = GetIconCenterPosition(icon2);

        // Crear línea que conecte los centros
        GameObject lineObj = CreateUILine(pos1, pos2, lineName);
        connectionLines.Add(lineObj);

        Debug.Log($"Línea creada: {member1.name} ({pos1}) ↔ {member2.name} ({pos2})");
    }

    private Vector2 GetIconCenterPosition(GameObject icon)
    {
        if (icon == null) return Vector2.zero;

        RectTransform iconRT = icon.GetComponent<RectTransform>();
        // La posición anchoredPosition ya es el centro del icono debido a la configuración del pivot
        return iconRT.anchoredPosition;
    }

    private GameObject GetMemberIcon(string memberId)
    {
        if (memberIcons.ContainsKey(memberId))
            return memberIcons[memberId];
        return null;
    }

    private GameObject CreateUILine(Vector2 startPos, Vector2 endPos, string name)
    {
        GameObject lineObj = new GameObject(name);
        lineObj.transform.SetParent(mapRect);
        lineObj.transform.SetAsFirstSibling();

        Image lineImage = lineObj.AddComponent<Image>();
        lineImage.color = lineColor;

        RectTransform lineRT = lineObj.GetComponent<RectTransform>();

        // Calcular dirección y ángulo
        Vector2 direction = endPos - startPos;
        float distance = direction.magnitude;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // AJUSTE: Extender la línea un poco más para asegurar que llegue
        float extension = 230f;
        float extendedDistance = distance + extension;

        // Calcular nueva posición final extendida
        Vector2 extendedEndPos = startPos + direction.normalized * extendedDistance;

        // Configurar la línea extendida
        lineRT.anchoredPosition = startPos;
        lineRT.sizeDelta = new Vector2(extendedDistance, lineWidth);
        lineRT.pivot = new Vector2(0f, 0.5f);
        lineRT.rotation = Quaternion.Euler(0, 0, angle);

        Debug.Log($"Línea EXTENDIDA: {name} - Distancia original: {distance}, Extendida: {extendedDistance}");

        return lineObj;
    }

    private void ShowDistancesBetweenMembers()
    {
        // Verificar que el mapa esté listo
        if (mapRect == null)
        {
            Debug.LogError("mapRect es null - no se pueden mostrar distancias");
            return;
        }

        if (familyMembers.Count < 2)
        {
            Debug.Log("Necesitas al menos 2 miembros para mostrar distancias");
            return;
        }

        Debug.Log($"Calculando distancias entre {familyMembers.Count} miembros...");

        int distancesShown = 0;
        HashSet<string> processedPairs = new HashSet<string>();

        // Solo crear NUEVAS distancias para conexiones que no tengan texto
        foreach (var member in familyMembers)
        {
            if (member.connectedMemberIds.Count == 0) continue;

            foreach (string connectedId in member.connectedMemberIds)
            {
                string pairKey = member.idNumber.CompareTo(connectedId) < 0
                    ? $"{member.idNumber}_{connectedId}"
                    : $"{connectedId}_{member.idNumber}";

                // Solo crear si no existe ya
                if (!distanceTexts.ContainsKey(pairKey) && familyMembersDict.ContainsKey(connectedId))
                {
                    FamilyMember connectedMember = familyMembersDict[connectedId];
                    ShowDistanceBetween(member, connectedMember);
                    processedPairs.Add(pairKey);
                    distancesShown++;
                }
            }
        }

        if (distancesShown == 0 && distanceTexts.Count > 0)
        {
            Debug.Log($"Se mantienen {distanceTexts.Count} distancias existentes");
        }
        else if (distancesShown > 0)
        {
            Debug.Log($"Se mostraron {distancesShown} nuevas distancias. Total: {distanceTexts.Count}");
        }
    }

    private void ShowDistanceBetween(FamilyMember member1, FamilyMember member2)
    {
        // Obtener posiciones de los iconos
        GameObject icon1 = GetMemberIcon(member1.idNumber);
        GameObject icon2 = GetMemberIcon(member2.idNumber);

        if (icon1 == null || icon2 == null)
        {
            Debug.LogWarning($"No se encontraron iconos para {member1.name} o {member2.name}");
            return;
        }

        Vector2 pos1 = GetIconCenterPosition(icon1);
        Vector2 pos2 = GetIconCenterPosition(icon2);

        // Calcular distancia en píxeles
        float distance = Vector2.Distance(pos1, pos2);

        // Calcular posición media para el texto
        Vector2 midPoint = (pos1 + pos2) / 2f;

        Debug.Log($"Posiciones: {member1.name} ({pos1}), {member2.name} ({pos2})");
        Debug.Log($"Distancia calculada: {distance:F0}px, Punto medio: {midPoint}");

        // Crear o actualizar texto de distancia
        CreateDistanceText(member1, member2, midPoint, distance);
    }

    private void CreateDistanceText(FamilyMember member1, FamilyMember member2, Vector2 position, float distance)
    {
        string textId = $"{member1.idNumber}_{member2.idNumber}";

        // Si ya existe, ACTUALIZAR en lugar de destruir
        if (distanceTexts.ContainsKey(textId))
        {
            GameObject existingText = distanceTexts[textId];
            if (existingText != null)
            {
                // Actualizar posición y texto
                RectTransform textRT = existingText.GetComponent<RectTransform>();
                if (textRT != null)
                {
                    textRT.anchoredPosition = position;
                }

                Text textComponent = existingText.GetComponent<Text>();
                if (textComponent != null)
                {
                    textComponent.text = $"{distance:F0}px";
                    textComponent.fontSize = 24; // Tamaño aumentado
                }

                Debug.Log($"Texto de distancia actualizado: {distance:F0}px");
                return;
            }
            else
            {
                // El objeto fue destruido, remover del diccionario
                distanceTexts.Remove(textId);
            }
        }

        // Crear nuevo objeto de texto si no existe
        GameObject textObj = new GameObject($"Distance_{member1.name}_{member2.name}");

        RectTransform textRTNew = textObj.AddComponent<RectTransform>();
        textObj.transform.SetParent(mapRect);

        // TAMAÑO AUMENTADO del contenedor
        textRTNew.anchorMin = new Vector2(0.5f, 0.5f);
        textRTNew.anchorMax = new Vector2(0.5f, 0.5f);
        textRTNew.pivot = new Vector2(0.5f, 0.5f);
        textRTNew.sizeDelta = new Vector2(120, 50); // Más grande
        textRTNew.anchoredPosition = position;

        Text textComponentNew = textObj.AddComponent<Text>();
        textComponentNew.text = $"{distance:F0}Km";
        textComponentNew.color = Color.blue; // Color azul para mejor visibilidad
        textComponentNew.fontSize = 24; // TAMAÑO DE FUENTE GRANDE
        textComponentNew.alignment = TextAnchor.MiddleCenter;
        textComponentNew.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        textComponentNew.fontStyle = FontStyle.Bold;

        // OUTLINE MÁS GRUESO para mejor contraste
        Outline outline = textObj.AddComponent<Outline>();
        outline.effectColor = Color.white;
        outline.effectDistance = new Vector2(2, 2); // Más grueso

        // AÑADIR SOMBRA para aún más visibilidad
        Shadow shadow = textObj.AddComponent<Shadow>();
        shadow.effectColor = new Color(0, 0, 0, 0.5f);
        shadow.effectDistance = new Vector2(2, -2);

        distanceTexts[textId] = textObj;

        Debug.Log($"Nuevo texto de distancia GRANDE creado: {distance:F0}px");
    }

    private void DebugAllPositions()
    {
        Debug.Log("=== DEBUG DE POSICIONES ===");

        foreach (var member in familyMembers)
        {
            GameObject icon = GetMemberIcon(member.idNumber);
            if (icon != null)
            {
                RectTransform iconRT = icon.GetComponent<RectTransform>();
                Vector2 mapPos = ConvertCoordinatesToMapPosition(member.coordinates);
                Vector2 canvasPos = iconRT.anchoredPosition;

                Debug.Log($" {member.name}: " +
                         $"Coords: ({member.coordinates.x}, {member.coordinates.y}) → " +
                         $"Map: {mapPos} → " +
                         $"Canvas: {canvasPos} → " +
                         $"Icon Pos: {iconRT.anchoredPosition}");
            }
            else
            {
                Debug.LogWarning($"No hay icono para: {member.name}");
            }
        }
        Debug.Log("=== FIN DEBUG ===");
    }

    private void ClearConnections()
    {
        foreach (GameObject line in connectionLines)
        {
            if (line != null)
                Destroy(line);
        }
        connectionLines.Clear();
        Debug.Log("Todas las líneas eliminadas");
    }

    private void AddFamilyMemberToUI(FamilyMember member)
    {
        if (familyMemberUIPrefab && familyMembersContainer)
        {
            GameObject memberUI = Instantiate(familyMemberUIPrefab, familyMembersContainer);
            Text nameText = memberUI.GetComponentInChildren<Text>();
            if (nameText)
            {
                // Usar la lógica invertida
                string status = member.isAlive ? "Vivo" : "Fallecido";
                nameText.text = $"{member.name} ({member.age} años) - {status}";
            }
        }
    }

    private void PlaceOnMap(FamilyMember member)
    {
        Vector2 mapPosition = ConvertCoordinatesToMapPosition(member.coordinates);
        DebugMapPosition(member, member.coordinates, mapPosition);
        CreateMapIcon(member, mapPosition);
    }

    private Vector2 ConvertCoordinatesToMapPosition(Vector2 coordinates)
    {
        // USAR LOS RANGOS EXACTOS de tus coordenadas predefinidas
        float minLatitude = 35.5f;    // Nueva York (la más baja)
        float maxLatitude = 150.75f;  // Puntarenas (la más alta)  
        float minLongitude = 139.65f; // Cartago (la más a la izquierda)
        float maxLongitude = 330.5f;  // Madrid (la más a la derecha)

        Debug.Log($"Rangos: Lat [{minLatitude}-{maxLatitude}], Long [{minLongitude}-{maxLongitude}]");

        // Normalizar las coordenadas a porcentajes (0-1) dentro de tus rangos
        float horizontalPercent = (coordinates.y - minLongitude) / (maxLongitude - minLongitude);
        float verticalPercent = (coordinates.x - minLatitude) / (maxLatitude - minLatitude);

        // Asegurar que estén en el rango 0-1
        horizontalPercent = Mathf.Clamp01(horizontalPercent);
        verticalPercent = Mathf.Clamp01(verticalPercent);

        // Obtener el rectángulo del mapa
        Rect mapRectLocal = mapRect.rect;

        // AJUSTE FINO - más arriba
        float leftMargin = 650f;   // Derecha máxima
        float rightMargin = 0f;    // Sin margen derecho
        float topMargin = 200f;    // Ajusta este valor: ↑ más grande = más arriba
        float bottomMargin = 50f;  // Ajusta este valor: ↓ más pequeño = más arriba

        float usableWidth = mapRectLocal.width - (leftMargin + rightMargin);
        float usableHeight = mapRectLocal.height - (topMargin + bottomMargin);

        // Calcular posición con márgenes asimétricos
        float xInMap = leftMargin + (horizontalPercent * usableWidth) - (mapRectLocal.width * 0.5f);
        float yInMap = topMargin + (verticalPercent * usableHeight) - (mapRectLocal.height * 0.5f);

        Vector2 finalPosition = new Vector2(xInMap, yInMap);

        Debug.Log($" {coordinates} → [{horizontalPercent:F2}, {verticalPercent:F2}] → {finalPosition}");

        return finalPosition;
    }


    private void CreateMapIcon(FamilyMember member, Vector2 position)
    {
        GameObject iconContainer = new GameObject($"Icon_{member.name}");
        RectTransform containerRT = iconContainer.AddComponent<RectTransform>();
        iconContainer.transform.SetParent(mapRect);

        // Configuración CORREGIDA del RectTransform
        containerRT.anchorMin = new Vector2(0.5f, 0.5f);
        containerRT.anchorMax = new Vector2(0.5f, 0.5f);
        containerRT.pivot = new Vector2(0.5f, 0.5f);
        containerRT.sizeDelta = new Vector2(220, 225);
        containerRT.anchoredPosition = position; // Esta es la posición importante

        GameObject imageObj = new GameObject("Photo");
        Image img = imageObj.AddComponent<Image>();
        Button btn = imageObj.AddComponent<Button>();
        RectTransform imageRT = imageObj.GetComponent<RectTransform>();

        imageObj.transform.SetParent(iconContainer.transform);
        imageRT.anchorMin = new Vector2(0.5f, 1f);
        imageRT.anchorMax = new Vector2(0.5f, 1f);
        imageRT.pivot = new Vector2(0.5f, 1f);
        imageRT.sizeDelta = new Vector2(220, 220);
        imageRT.anchoredPosition = new Vector2(0, 0);

        GameObject nameBackgroundObj = new GameObject("NameBackground");
        Image nameBackground = nameBackgroundObj.AddComponent<Image>();
        RectTransform bgRT = nameBackgroundObj.GetComponent<RectTransform>();

        nameBackgroundObj.transform.SetParent(iconContainer.transform);
        bgRT.anchorMin = new Vector2(0.2f, 0f);
        bgRT.anchorMax = new Vector2(0.8f, 0f);
        bgRT.pivot = new Vector2(0.5f, 1f);
        bgRT.sizeDelta = new Vector2(0, 18);
        bgRT.anchoredPosition = new Vector2(0, -2);

        nameBackground.color = new Color(0f, 0f, 0f, 0.9f);

        GameObject textObj = new GameObject("NameLabel");
        Text textComponent = textObj.AddComponent<Text>();
        RectTransform textRT = textObj.GetComponent<RectTransform>();

        textObj.transform.SetParent(nameBackgroundObj.transform);
        textRT.anchorMin = new Vector2(0f, 0f);
        textRT.anchorMax = new Vector2(1f, 1f);
        textRT.pivot = new Vector2(0.5f, 0.5f);
        textRT.sizeDelta = Vector2.zero;
        textRT.anchoredPosition = Vector2.zero;

        textComponent.text = member.name;
        textComponent.color = Color.white;
        textComponent.alignment = TextAnchor.MiddleCenter;
        textComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        textComponent.fontSize = 10;
        textComponent.fontStyle = FontStyle.Bold;

        if (member.photo != null)
        {
            Sprite photoSprite = Sprite.Create(member.photo,
                new Rect(0, 0, member.photo.width, member.photo.height),
                new Vector2(0.5f, 0.5f));
            img.sprite = photoSprite;
            img.type = Image.Type.Simple;
            img.preserveAspect = true;
        }
        else
        {
            img.color = GetRandomColor();

            GameObject initialObj = new GameObject("Initial");
            Text initialText = initialObj.AddComponent<Text>();
            initialObj.transform.SetParent(imageObj.transform);
            initialText.text = member.name.Length > 0 ? member.name[0].ToString() : "?";
            initialText.color = Color.white;
            initialText.alignment = TextAnchor.MiddleCenter;
            initialText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            initialText.fontSize = 80;

            RectTransform initialRT = initialText.GetComponent<RectTransform>();
            initialRT.anchorMin = new Vector2(0, 0);
            initialRT.anchorMax = new Vector2(1, 1);
            initialRT.sizeDelta = Vector2.zero;
            initialRT.anchoredPosition = Vector2.zero;
        }

        btn.onClick.AddListener(() => OpenMemberProfile(member));
        AddHoverEffect(btn, img);

        memberIcons[member.idNumber] = iconContainer;

        // Debuggear después de crear cada icono
        DebugAllIconPositions();

        Debug.Log($"Icono creado para {member.name} - Nombre pegado a la imagen");
    }

    private IEnumerator RedrawConnectionsWithDelay()
    {
        yield return new WaitForEndOfFrame(); // Esperar un frame

        DebugAllPositions(); // Debuggear posiciones
        DrawAllConnections(); // Redibujar conexiones

        Debug.Log("Conexiones redibujadas con retraso");
    }

    // Llamar este método después de añadir miembros o recrear iconos
    public void ScheduleRedraw()
    {
        StartCoroutine(RedrawConnectionsWithDelay());
    }

    private Color GetRandomColor()
    {
        Color[] colors = new Color[] {
            Color.red, Color.blue, Color.green, Color.yellow,
            Color.cyan, Color.magenta, Color.gray
        };
        return colors[UnityEngine.Random.Range(0, colors.Length)];
    }

    private void OpenMemberProfile(FamilyMember member)
    {
        Debug.Log($"Abriendo perfil de: {member.name}");
        Debug.Log($"Estado actual - Distancias mostradas: {distanceTexts.Count}, Miembros: {familyMembers.Count}");

        // Usar el miembro del DataManager para asegurar consistencia
        FamilyMember dataManagerMember = FamilyDataManager.GetFamilyMemberById(member.idNumber);
        if (dataManagerMember != null)
        {
            ProfileDataTransporter.SetMemberData(dataManagerMember);
        }
        else
        {
            ProfileDataTransporter.SetMemberData(member);
        }

        UnityEngine.SceneManagement.SceneManager.LoadScene("Perfil");
    }

    private void AddHoverEffect(Button btn, Image img)
    {
        btn.transition = Selectable.Transition.ColorTint;
        ColorBlock colors = btn.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(0.8f, 0.8f, 1f);
        colors.pressedColor = new Color(0.6f, 0.6f, 1f);
        btn.colors = colors;
    }

    private void DebugMapPosition(FamilyMember member, Vector2 rawCoordinates, Vector2 mapPosition)
    {
        Debug.Log($"Miembro: {member.name}\n" +
                  $"Coordenadas reales: {rawCoordinates}\n" +
                  $"Posición en mapa: {mapPosition}\n" +
                  $"Tamaño del mapa: {mapRect.rect.width}x{mapRect.rect.height}\n" +
                  $"AnchoredPosition: {mapPosition}");
    }

    public void CancelEntry()
    {
        ClearForm();
        currentMember = null;
    }

    private void ClearForm()
    {
        nameInput.text = "";
        idInput.text = "";
        locationDropdown.value = 0;
        birthDateInput.text = "";
        ageInput.text = "";
        isAliveToggle.isOn = false;
        photoPreview.texture = null;
        currentPhoto = null;
    }
}