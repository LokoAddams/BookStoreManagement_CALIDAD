using System.Reflection;
using MicroServiceSales.Infrastructure.DataBase;

namespace MicroServiceSales.Infrastructure.UnitTests;

public class DataBaseConnectionGetInstanceTests
{
    [Fact]
    public void GetInstance_Should_Create_Instance_On_First_Call()
    {
        ResetSingleton();

        var instance = DataBaseConnection.GetInstance("Host=localhost;Database=db1;Username=u;Password=p");

        Assert.NotNull(instance);
        Assert.Same(instance, GetCurrentSingleton());

        ResetSingleton();
    }

    [Fact]
    public void GetInstance_Should_Return_Existing_Instance_On_Subsequent_Calls()
    {
        ResetSingleton();
        var existing = CreateInstanceViaReflection("Host=localhost;Database=existing;Username=u;Password=p");
        SetSingleton(existing);

        var returned = DataBaseConnection.GetInstance("Host=localhost;Database=another;Username=u;Password=p");

        Assert.Same(existing, returned);

        ResetSingleton();
    }

    [Fact]
    public async Task GetInstance_Should_Return_Instance_Created_By_Other_Thread_In_Race_Scenario()
    {
        ResetSingleton();

        var padlock = GetPadlock();
        Monitor.Enter(padlock);
        try
        {
            var getInstanceTask = Task.Run(() =>
                DataBaseConnection.GetInstance("Host=localhost;Database=threadA;Username=u;Password=p"));

            Thread.Sleep(50);

            var existing = CreateInstanceViaReflection("Host=localhost;Database=threadB;Username=u;Password=p");
            SetSingleton(existing);

            Monitor.Exit(padlock);

            var returned = await getInstanceTask;
            Assert.Same(existing, returned);
        }
        finally
        {
            if (Monitor.IsEntered(padlock))
            {
                Monitor.Exit(padlock);
            }

            ResetSingleton();
        }
    }

    private static DataBaseConnection CreateInstanceViaReflection(string connectionString)
    {
        var ctor = typeof(DataBaseConnection).GetConstructor(
            BindingFlags.Instance | BindingFlags.NonPublic,
            binder: null,
            new[] { typeof(string) },
            modifiers: null);

        Assert.NotNull(ctor);

        return (DataBaseConnection)ctor.Invoke(new object[] { connectionString });
    }

    private static void ResetSingleton()
    {
        SetSingleton(null);
    }

    private static DataBaseConnection? GetCurrentSingleton()
    {
        var field = typeof(DataBaseConnection).GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic);
        Assert.NotNull(field);
        return (DataBaseConnection?)field.GetValue(null);
    }

    private static void SetSingleton(DataBaseConnection? instance)
    {
        var field = typeof(DataBaseConnection).GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic);
        Assert.NotNull(field);
        field.SetValue(null, instance);
    }

    private static object GetPadlock()
    {
        var field = typeof(DataBaseConnection).GetField("_padlock", BindingFlags.Static | BindingFlags.NonPublic);
        Assert.NotNull(field);

        var value = field.GetValue(null);
        Assert.NotNull(value);

        return value;
    }
}
