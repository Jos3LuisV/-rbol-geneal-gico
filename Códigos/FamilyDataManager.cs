using System.Collections.Generic;
using UnityEngine;

//Guarda y organiza a los miembros de la familia
public static class FamilyDataManager
{
    private static List<FamilyMember> allFamilyMembers = new List<FamilyMember>(); // Almacena todos los miembros de la familia
    private static Dictionary<string, FamilyMember> familyMembersDict = new Dictionary<string, FamilyMember>(); // Acceso rápido por ID


    public static List<FamilyMember> GetAllFamilyMembers() // Devuelve copia de todos los miembros
    {
        return new List<FamilyMember>(allFamilyMembers);
    }

    public static void AddFamilyMember(FamilyMember member) // Añade miembro si no existe
    {
        if (!allFamilyMembers.Exists(m => m.idNumber == member.idNumber))
        {
            allFamilyMembers.Add(member);
            familyMembersDict[member.idNumber] = member;
            UnityEngine.Debug.Log($"Miembro guardado en DataManager: {member.name}");
        }
    }

    public static FamilyMember GetFamilyMemberById(string id) // Busca miembro por ID
    {
        if (familyMembersDict.ContainsKey(id))
            return familyMembersDict[id];
        return null;
    }

    public static void ClearAllData() // Limpia todos los datos
    {
        allFamilyMembers.Clear();
        familyMembersDict.Clear();
        UnityEngine.Debug.Log("Todos los datos de familia eliminados");
    }

    public static int GetMemberCount() // Cuenta total de miembros
    {
        return allFamilyMembers.Count;
    }
}
