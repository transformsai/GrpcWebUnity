public class JsNull : JsReference
{
    public static JsNull Instance = new JsNull();
    private JsNull() : base(JsTypes.Null, 0) { }

    public override object RawValue => null;
    public override bool TruthyValue => false;
}
