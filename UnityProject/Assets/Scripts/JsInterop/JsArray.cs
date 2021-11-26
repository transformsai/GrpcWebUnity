using System;
using System.Collections;
using System.Collections.Generic;

public class JsArray : JsObject, IList<JsReference>
{
    public static JsArray CreateEmpty() => new JsArray(Runtime.MakeArray());

    public static JsArray Create(IEnumerable list)
    {
        var arr = CreateEmpty();
        foreach (var obj in list) arr.Add(FromObject(obj));
        return arr;
    }


    internal JsArray(double refId) : base(refId, JsTypes.Array) { }

    public IEnumerator<JsReference> GetEnumerator()
    {
        for (var i = 0; i < Count; i++) yield return this[i];
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Add(JsReference item) => Invoke("push", item);

    public void Clear() => SetProp("length", 0);

    public bool Contains(JsReference item) => Invoke("includes", item) as JsBool ?? false;

    public void CopyTo(JsReference[] array, int index)
    {
        for (int i = 0; i < Count; i++) array[index++] = this[i];
    }

    public bool Remove(JsReference item)
    {
        var index = IndexOf(item);
        if (index < 0) return false;
        RemoveAt(index);
        return true;
    }

    public int Count => GetProp("length").As<int>();
    public bool IsReadOnly => false;
    public int IndexOf(JsReference item) => Invoke("indexOf", item).As<int>();
    public void Insert(int index, JsReference item) => Invoke("splice", index, 0, item);
    public void RemoveAt(int index) => Invoke("splice", index, 1);

    public JsReference this[int index]
    {
        get => FromRef(Runtime.GetArrayElement(RefId, index, out var typeId), typeId);
        set => Runtime.SetArrayElement(RefId, index, (int) value.TypeId, value.RefId);
    }
}
