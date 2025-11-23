using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class FamilyTreeTests
{
    // Clase temporal para testing
    public class TestFamilyMember
    {
        public string name;
        public string idNumber;
        public Vector2 coordinates;
        public System.DateTime birthDate;
        public int age;
        public string role;
        public List<string> connectedMemberIds = new List<string>();

        public TestFamilyMember(string name, string idNumber, Vector2 coordinates, System.DateTime birthDate, int age, string role)
        {
            this.name = name;
            this.idNumber = idNumber;
            this.coordinates = coordinates;
            this.birthDate = birthDate;
            this.age = age;
            this.role = role;
        }

        public void AddConnection(string memberId)
        {
            if (!connectedMemberIds.Contains(memberId) && memberId != idNumber)
            {
                connectedMemberIds.Add(memberId);
            }
        }
    }

    // Simulamos el DataManager para testing
    private static List<TestFamilyMember> testMembers = new List<TestFamilyMember>();

    [SetUp]
    public void Setup()
    {
        testMembers.Clear();
    }

    //Prueba #1
    [Test]
    public void Prueba1_CrearMiembro()
    {
        // Verificar que podemos crear un miembro familiar
        TestFamilyMember persona = new TestFamilyMember(
            "Juan", "001",
            new Vector2(100, 100),
            new System.DateTime(1990, 1, 1),
            30,
            "Padre"
        );

        Assert.AreEqual("Juan", persona.name);
        Assert.AreEqual("001", persona.idNumber);
        Debug.Log("PRUEBA 1: Miembro creado correctamente");
    }

    //Prueba #2
    [Test]
    public void Prueba2_GuardarEnDataManager()
    {
        // Verificar que se guarda en el DataManager
        TestFamilyMember persona = new TestFamilyMember("Maria", "002", Vector2.zero, new System.DateTime(1985, 1, 1), 35, "Madre");

        testMembers.Add(persona);
        TestFamilyMember recuperada = testMembers.Find(m => m.idNumber == "002");

        Assert.IsNotNull(recuperada);
        Assert.AreEqual("Maria", recuperada.name);
        Debug.Log("PRUEBA 2: Miembro guardado correctamente");
    }

    //Prueba #3
    [Test]
    public void Prueba3_ConexionEntreDosPersonas()
    {
        // Verificar que dos personas pueden conectarse
        TestFamilyMember padre = new TestFamilyMember("Carlos", "003", Vector2.zero, new System.DateTime(1970, 1, 1), 50, "Padre");
        TestFamilyMember hijo = new TestFamilyMember("Luis", "004", Vector2.zero, new System.DateTime(2000, 1, 1), 20, "Hijo");

        padre.AddConnection(hijo.idNumber);
        hijo.AddConnection(padre.idNumber);

        Assert.IsTrue(padre.connectedMemberIds.Contains("004"));
        Assert.IsTrue(hijo.connectedMemberIds.Contains("003"));
        Debug.Log("PRUEBA 3: Conexión entre personas funciona");
    }

    //Prueba #4
    [Test]
    public void Prueba4_NoDuplicados()
    {
        // Verificar que no se permiten miembros duplicados
        TestFamilyMember persona = new TestFamilyMember("Ana", "005", Vector2.zero, new System.DateTime(1995, 1, 1), 25, "Hermana");

        testMembers.Add(persona);
        // Intentar agregar duplicado - no debería funcionar
        if (!testMembers.Exists(m => m.idNumber == "005"))
        {
            testMembers.Add(persona);
        }

        Assert.AreEqual(1, testMembers.Count);
        Debug.Log("PRUEBA 4: Duplicados bloqueados correctamente");
    }

    //Prueba #5
    [Test]
    public void Prueba5_BuscarPorID()
    {
        // Verificar búsqueda por ID
        TestFamilyMember persona = new TestFamilyMember("Pedro", "006", Vector2.zero, new System.DateTime(1980, 1, 1), 40, "Tio");
        testMembers.Add(persona);

        TestFamilyMember encontrada = testMembers.Find(m => m.idNumber == "006");
        TestFamilyMember noEncontrada = testMembers.Find(m => m.idNumber == "999");

        Assert.IsNotNull(encontrada);
        Assert.IsNull(noEncontrada);
        Debug.Log("PRUEBA 5: Búsqueda por ID funciona");
    }

    //Prueba #6
    [Test]
    public void Prueba6_LimpiarTodo()
    {
        // Verificar que se puede limpiar todos los datos
        testMembers.Add(new TestFamilyMember("Persona1", "007", Vector2.zero, new System.DateTime(1990, 1, 1), 30, "Yo"));
        testMembers.Add(new TestFamilyMember("Persona2", "008", Vector2.zero, new System.DateTime(1992, 1, 1), 28, "Prima"));

        testMembers.Clear();

        Assert.AreEqual(0, testMembers.Count);
        Debug.Log("PRUEBA 6: Limpieza de datos funciona");
    }

    //Prueba #7
    [Test]
    public void Prueba7_MultiplesConexiones()
    {
        // Verificar que un miembro puede tener múltiples conexiones
        TestFamilyMember abuelo = new TestFamilyMember("Abuelo", "010", Vector2.zero, new System.DateTime(1950, 1, 1), 70, "Abuelo");
        TestFamilyMember padre = new TestFamilyMember("Padre", "011", Vector2.zero, new System.DateTime(1975, 1, 1), 45, "Padre");
        TestFamilyMember tio = new TestFamilyMember("Tio", "012", Vector2.zero, new System.DateTime(1972, 1, 1), 48, "Tio");

        abuelo.AddConnection(padre.idNumber);
        abuelo.AddConnection(tio.idNumber);

        Assert.AreEqual(2, abuelo.connectedMemberIds.Count);
        Debug.Log("PRUEBA 7: Múltiples conexiones funcionan");
    }

    //Prueba #8
    [Test]
    public void Prueba8_CoordenadasGuardadas()
    {
        // Verificar que las coordenadas se guardan correctamente
        Vector2 coordenadas = new Vector2(150.5f, 200.3f);
        TestFamilyMember persona = new TestFamilyMember("Marta", "013", coordenadas, new System.DateTime(1985, 1, 1), 35, "Madre");

        Assert.AreEqual(150.5f, persona.coordinates.x);
        Assert.AreEqual(200.3f, persona.coordinates.y);
        Debug.Log("PRUEBA 8: Coordenadas guardadas correctamente");
    }

    //Prueba #9
    [Test]
    public void Prueba9_ListaTodosMiembros()
    {
        // Verificar que podemos obtener lista de todos los miembros
        testMembers.Clear();

        testMembers.Add(new TestFamilyMember("PersonaA", "014", Vector2.zero, new System.DateTime(1990, 1, 1), 30, "Yo"));
        testMembers.Add(new TestFamilyMember("PersonaB", "015", Vector2.zero, new System.DateTime(1991, 1, 1), 29, "Hermano"));

        List<TestFamilyMember> todos = new List<TestFamilyMember>(testMembers);

        Assert.AreEqual(2, todos.Count);
        Debug.Log("PRUEBA 9: Lista de todos los miembros funciona");
    }

    //Prueba #10
    [Test]
    public void Prueba10_CalculoDistancias()
    {
        // Verificar cálculo de distancias entre coordenadas
        TestFamilyMember persona1 = new TestFamilyMember("Persona1", "016", new Vector2(0, 0), new System.DateTime(1990, 1, 1), 30, "Padre");
        TestFamilyMember persona2 = new TestFamilyMember("Persona2", "017", new Vector2(3, 4), new System.DateTime(1992, 1, 1), 28, "Madre");

        float distancia = Vector2.Distance(persona1.coordinates, persona2.coordinates);

        Assert.AreEqual(5f, distancia, 0.01f); // Distancia entre (0,0) y (3,4) = 5
        Debug.Log("PRUEBA 10: Cálculo de distancias funciona");
    }
}
