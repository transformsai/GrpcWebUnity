using System;
using System.Collections;
using System.Collections.Generic;

public class JsObject : JsReference
{
    public void SetProp(string key, JsReference value) => Runtime.SetProp(RefId, key, (int)value.TypeId, value.RefId);


    internal JsObject(double refId, JsTypes typeId = JsTypes.Object) : base(typeId, refId) { }
    public override object RawValue => this;
    public override bool TruthyValue => true;

    public static JsObject Create() => new JsObject(Runtime.MakeEmptyObject());

    public static JsObject Create(IDictionary dictionary)
    {
        var obj = Create();
        obj.Populate(dictionary);
        return obj;
    }

    public void Populate(IDictionary dictionary)
    {
        if (dictionary.Keys is not ICollection<string> keys) throw new Exception("Unsupported dictionary with non-string keys");

        foreach (var key in keys)
        {
            var rawValue = dictionary[key];
            var value = FromObject(rawValue);
            this.SetProp(key, value);
        }
    }
}
