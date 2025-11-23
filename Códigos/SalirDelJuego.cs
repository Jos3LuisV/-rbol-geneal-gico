using UnityEngine;

//Para cerrar totalmente el programa
public class SalirDelJuego : MonoBehaviour
{
    public void CerrarJuego()
    {
        Debug.Log("Cerrando juego...");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}

