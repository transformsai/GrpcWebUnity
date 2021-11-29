using System;
using System.Collections.Generic;

public abstract class JsReference : IDisposable, IJsValue
{
    private static readonly Dictionary<double, WeakReference<JsReference>> ReferenceMap = new Dictionary<double, WeakReference<JsReference>>();
    private JsValue? _ref;
    public JsValue RefValue => _ref ?? throw new ObjectDisposedException("Tried to accesss disposed reference value");
    
    internal JsReference(JsTypes typeId, double refId)
    {
        ReferenceMap.Add(refId, new WeakReference<JsReference>(this));
        _ref = new JsValue(typeId, refId, this);
    }

    public static bool TryGetRef(double refId, out JsReference jsReference)
    {
        jsReference = null;
        return ReferenceMap.TryGetValue(refId, out var weakRef) && weakRef.TryGetTarget(out jsReference);
    }

    public abstract object RawValue { get; }
    public abstract bool TruthyValue { get; }
    public virtual double NumberValue => Runtime.GetNumber(this);
    public JsValue GetProp(JsValue key) => RefValue.GetProp(key);
    public JsValue Invoke(string functionName, params JsValue[] values) =>
        RefValue.Invoke(functionName, values);
    public JsValue Invoke(string functionName, JsValue param1 = default, JsValue param2 = default, JsValue param3 = default) =>
        RefValue.Invoke(functionName, param1, param2, param3);

    public static implicit operator JsValue(JsReference reference) =>
        reference.RefValue;
    public string GetJsStringImpl() => Runtime.GetString(this);
    public override string ToString() => ReferenceEquals(RawValue, this) ? GetJsStringImpl() : RawValue?.ToString() ?? "";
    public object As(Type type) => RefValue.As(type);
    public T As<T>() => RefValue.As<T>();
    public JsValue EvaluateOnThis(string functionBody) => RefValue.EvaluateOnThis(functionBody);

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    ~JsReference() => Dispose(false);

    protected virtual void Dispose(bool isDisposing)
    {
        if (!_ref.HasValue) return;
        ReferenceMap.Remove(_ref.Value.Value);
        Runtime.GarbageCollect(this);
        _ref = null;
    }

}
