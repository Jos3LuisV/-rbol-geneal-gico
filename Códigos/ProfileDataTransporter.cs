using UnityEngine;

public static class ProfileDataTransporter
{
    private static string memberId;

    public static void SetMemberData(FamilyMember member)
    {
        memberId = member.idNumber;
        UnityEngine.Debug.Log($"Transportando datos de: {member.name} (ID: {memberId})");
    }

    public static FamilyMember GetMemberData()
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
