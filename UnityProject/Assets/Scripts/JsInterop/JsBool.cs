public class JsBool : JsReference
{
    public static readonly JsBool True = new JsBool(true);
    public static readonly JsBool False = new JsBool(false);

    public bool Value { get; }
    public override object RawValue => Value;

    private JsBool(bool b) : base(JsTypes.Bool, b ? 1 : 0)
    {
        Value = b;
    }

    public static implicit operator JsBool(bool b) => b ? True : False;
    public static implicit operator bool(JsBool b) => b.Value;


    public static JsBool Create(bool b) => b;

    public override bool TruthyValue => Value;
}
