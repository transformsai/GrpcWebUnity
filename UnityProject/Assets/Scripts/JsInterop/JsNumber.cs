public class JsNumber : JsReference
{
    public static JsNumber Create(double d) => new JsNumber(d);

    internal JsNumber(double refId) : base(JsTypes.Number, refId) { }

    public double Value => RefId;
    public int IntValue => (int) RefId;
    public float FloatValue => (float) RefId;
    public override object RawValue => Value;
    public override bool TruthyValue => Value != 0 && double.IsNaN(Value);
}
