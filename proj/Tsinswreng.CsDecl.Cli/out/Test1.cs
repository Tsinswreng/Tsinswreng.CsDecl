
namespace Test1{
public class SampleClass
{
    public int Field1;
    public string Field2;

    public SampleClass() {
        Field1 = 0;
        Field2 = "";
    }

    public SampleClass(int value) {
        Field1 = value;
        Field2 = value.ToString();
    }

    public int Property1 { get; set; }

    public string Property2 {
        get { return Field2; }
        set { Field2 = value; }
    }

    public void Method1() {
        Console.WriteLine("Method1");
    }

    public int Method2(int x, int y) {
        return x + y;
    }

    public event EventHandler MyEvent;

    public void RaiseEvent() {
        MyEvent?.Invoke(this, EventArgs.Empty);
    }
}

public enum SampleEnum {
    Value1 = 1,
    Value2 = 2,
    Value3 = 3
}

public partial class PartialClass {
    public void PartialMethod() {
        Console.WriteLine("Partial method");
    }
}
}
