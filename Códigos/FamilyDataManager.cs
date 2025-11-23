using System.Collections.Generic;
using UnityEngine;

public static class FamilyDataManager
{
    private static List<FamilyMember> allFamilyMembers = new List<FamilyMember>();
    private static Dictionary<string, FamilyMember> familyMembersDict = new Dictionary<string, FamilyMember>();
    

    public static List<FamilyMember> GetAllFamilyMembers()
    {
        return new List<FamilyMember>(allFamilyMembers);
    }

    public static void AddFamilyMember(FamilyMember member)
    {
        if (!allFamilyMembers.Exists(m => m.idNumber == member.idNumber))
        {
            allFamilyMembers.Add(member);
            familyMembersDict[member.idNumber] = member;
            UnityEngine.Debug.Log($"Miembro guardado en DataManager: {member.name}");
        }
    }

    public static FamilyMember GetFamilyMemberById(string id)
    {
        if (familyMembersDict.ContainsKey(id))
            return familyMembersDict[id];
        return null;
    }

    public static void ClearAllData()
    {
        allFamilyMembers.Clear();
        familyMembersDict.Clear();
        UnityEngine.Debug.Log("Todos los datos de familia eliminados");
    }

    public static int GetMemberCount()
    {
        return allFamilyMembers.Count;
    }
}
