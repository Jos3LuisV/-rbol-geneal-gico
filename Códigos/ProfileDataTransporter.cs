using UnityEngine;


//Transporta datos de miembros entre escenas usando solo el ID como referencia.
public static class ProfileDataTransporter
{
    private static string memberId; // Almacena el ID del miembro entre escenas

    public static void SetMemberData(FamilyMember member) // Guarda el ID del miembro para transport
    {
        memberId = member.idNumber;
        UnityEngine.Debug.Log($"Transportando datos de: {member.name} (ID: {memberId})");
    }

    public static FamilyMember GetMemberData() // Recupera el miembro usando el ID guardado
    {
        if (!string.IsNullOrEmpty(memberId))
        {
            FamilyMember member = FamilyDataManager.GetFamilyMemberById(memberId);
            if (member != null)
            {
                UnityEngine.Debug.Log($"Recuperando datos de: {member.name}");
                return member;
            }
        }

        UnityEngine.Debug.LogError("No se pudo recuperar datos del miembro");
        return null;
    }
}
