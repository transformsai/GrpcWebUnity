public class JsBigInt : JsReference
{
    internal JsBigInt(double refId) : base(JsTypes.BigInt, refId) { }
    public override object RawValue => this;
    public override bool TruthyValue => ToString() == "0n";

}
