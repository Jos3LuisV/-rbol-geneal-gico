using UnityEngine;

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
