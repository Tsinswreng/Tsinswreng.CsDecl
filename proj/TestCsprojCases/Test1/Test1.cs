using System;
using System.Collections.Generic;
using i32 = System.Int32;
namespace Test1;
using System.Linq;
using u32 = System.UInt32;

/// <summary>
///
/// </summary>
[Obsolete("BBB")]
public class SampleClass {// test
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

	[Obsolete("AAA")]
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
