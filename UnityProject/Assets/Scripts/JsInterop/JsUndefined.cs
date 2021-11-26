public class JsUndefined : JsReference
{
    public static JsUndefined Instance = new JsUndefined();
    private JsUndefined() : base(JsTypes.Undefined, 0) { }

    public override object RawValue => this;
    public override bool TruthyValue => false;

}
