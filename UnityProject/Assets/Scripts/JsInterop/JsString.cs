public class JsString : JsReference
{
    private string _stringCache;

    public string Value => _stringCache ?? (_stringCache = GetJsStringImpl());

    public static JsString Create(string s) => new JsString(Runtime.MakeString(s));

    internal JsString(double refId, string initialValue = null) : base(JsTypes.String, refId) { }
    public override object RawValue => Value;
    public override bool TruthyValue => !string.IsNullOrEmpty(Value);
}
