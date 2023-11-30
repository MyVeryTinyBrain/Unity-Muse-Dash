using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

public sealed class Ref<T>
{
    private Func<T> getter;
    private Action<T> setter;
    public Ref(Func<T> getter, Action<T> setter)
    {
        this.getter = getter;
        this.setter = setter;
    }
    public T Value
    {
        get { return getter(); }
        set { setter(value); }
    }
}

public enum FieldAccessType
{
    // 언제나 노출
    Always,
    // 절대 노출하지 않음
    Never,
    // 조건에 따라 노출
    Condition,
}

public class DepthFieldInfo
{
    DepthFieldInfo _parent;
    FieldInfo _fieldInfo;

    public FieldInfo fieldInfo => _fieldInfo;
    public DepthFieldInfo parent => _parent;

    public DepthFieldInfo(FieldInfo fieldInfo, DepthFieldInfo parent)
    {
        _fieldInfo = fieldInfo;
        _parent = parent;
    }

    public DepthFieldInfo(FieldInfo fieldInfo) : this(fieldInfo, null) { }

    public object GetValue<T>(T rootData)
    {
        Stack<DepthFieldInfo> s = new Stack<DepthFieldInfo>();
        DepthFieldInfo currentDepth = this;
        while (currentDepth != null)
        {
            s.Push(currentDepth);
            currentDepth = currentDepth.parent;
        }
        object currentObject = rootData;
        while (s.Count > 0)
        {
            DepthFieldInfo depth = s.Pop();
            currentObject = depth.fieldInfo.GetValue(currentObject);
        }
        return currentObject;
    }

    public void SetValue<T>(Ref<T> refRootData, object value)
    {
        object originRootData = refRootData.Value;
        Stack<DepthFieldInfo> s = new Stack<DepthFieldInfo>();
        DepthFieldInfo currentDepth = this;
        while (currentDepth != null)
        {
            s.Push(currentDepth);
            currentDepth = currentDepth.parent;
        }
        Queue<Temp> q = new Queue<Temp>();
        object currentObject = originRootData;
        while (s.Count > 1)
        {
            DepthFieldInfo top = s.Pop();

            Temp temp = new Temp() { childFieldInfo = top.fieldInfo, parentObject = currentObject };
            currentObject = top.fieldInfo.GetValue(currentObject);
            temp.childObject = currentObject;

            q.Enqueue(temp);
        }
        this.fieldInfo.SetValue(currentObject, value);
        while (q.Count > 0)
        {
            Temp front = q.Dequeue();
            front.childFieldInfo.SetValue(front.parentObject, front.childObject);
        }
        refRootData.Value = (T)originRootData;
    }

    struct Temp
    {
        public object parentObject;
        public FieldInfo childFieldInfo;
        public object childObject;
    }

    public void SetValue<T>(ref T rootData, object value)
    {
        object originRootData = rootData;

        Stack<DepthFieldInfo> s = new Stack<DepthFieldInfo>();
        DepthFieldInfo currentDepth = this;
        while (currentDepth != null)
        {
            s.Push(currentDepth);
            currentDepth = currentDepth.parent;
        }

        Queue<Temp> q = new Queue<Temp>();
        object currentObject = originRootData;
        while (s.Count > 1)
        {
            DepthFieldInfo top = s.Pop();

            Temp temp = new Temp() { childFieldInfo = top.fieldInfo, parentObject = currentObject };
            currentObject = top.fieldInfo.GetValue(currentObject);
            temp.childObject = currentObject;

            q.Enqueue(temp);
        }

        this.fieldInfo.SetValue(currentObject, value);

        while(q.Count > 0)
        {
            Temp front = q.Dequeue();
            front.childFieldInfo.SetValue(front.parentObject, front.childObject);
        }

        rootData = (T)originRootData;
    }
}

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public class FieldAccessAttribute : PropertyAttribute
{
    Type _type;
    string _name;
    object _value;
    FieldAccessType _access;

    public Type type => _type;
    public string name => _name;
    public object value => _value;
    public FieldAccessType access => _access;

    /// <summary>
    /// 해당 필드가 언제나 접근 가능하거나, 언제나 접근 불가능하도록 설정합니다.
    /// </summary>
    /// <param name="access"></param>
    public FieldAccessAttribute(bool access)
    {
        if (access)
        {
            _access = FieldAccessType.Always;
        }
        else
        {
            _access = FieldAccessType.Never;
        }
    }

    /// <summary>
    /// 해당 필드가 기입한 타입의 이름을 가진 어느 필드의 값과 같을 때만 접근 가능하도록 설정합니다.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="name"></param>
    /// <param name="value"></param>
    public FieldAccessAttribute(Type type, string name, object value)
    {
        _type = type;
        _name = name;
        _value = value;
        _access = FieldAccessType.Condition;
    }

    /// <summary>
    /// 해당 필드가 접근 가능한 필드인지 확인합니다.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data">해당 필드가 포함되어 있는 객체입니다.</param>
    /// <param name="fieldInfo">해당 필드의 정보입니다.</param>
    /// <returns></returns>
    public static bool IsAccessibleField<T>(T data, FieldInfo fieldInfo)
    {
        List<FieldAccessAttribute> attributes = fieldInfo.GetCustomAttributes<FieldAccessAttribute>().ToList();

        // 해당 필드에 속성이 부착되어 있지 않다면 접근할 수 없습니다.
        if (attributes.Count == 0)
        {
            return false;
        }

        // 여러개의 속성이 부착된 경우에는 OR 연산처럼 하나라도 만족하면 접근 가능합니다.
        FieldInfo[] fieldInfos = data.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
        foreach (FieldAccessAttribute attribute in attributes)
        {
            switch (attribute.access)
            {
                // 접근 가능 설정인 경우에는 언제나 접근할 수 있습니다.
                case FieldAccessType.Always:
                return true;
                // 접근 불가 설정인 경우에는 다른 속성을 탐색합니다.
                case FieldAccessType.Never:
                break;
                case FieldAccessType.Condition:
                {
                    FieldInfo[] matches = Array.FindAll(fieldInfos, (x) =>
                    {
                        return
                        x.FieldType == attribute.type &&
                        x.Name == attribute.name;
                    });
                    // 타입과 이름이 같은 필드가 없는 경우에는 다른 속성을 탐색합니다.
                    if (matches.Count() == 0)
                    {
                        break;
                    }
                    // 타입과 이름이 같은 필드의 값이 기입된 값과 같은 것이 하나라도 있다면 접근할 수 있습니다.
                    foreach (FieldInfo match in matches)
                    {
                        object matchValue = match.GetValue(data);
                        if (matchValue.Equals(attribute.value))
                        {
                            return true;
                        }
                    }
                }
                // 타입 매칭에 실패하면 다른 조건을 탐색합니다.
                break;
            }
        }
        // 조건이 모두 실패하면 해당 필드는 접근 불가능합니다.
        return false;
    }

    /// <summary>
    /// 객체에서 접근 가능한 필드 정보들을 추출합니다.<para>
    /// 이 메소드는 객체내의 다른 객체 내부의 정보는 추출하지 않습니다.</para>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data">필드 정보들을 추출할 객체입니다.</param>
    /// <returns></returns>
    public static List<FieldInfo> GetAccessibleFieldInfos<T>(T data)
    {
        Type type = data.GetType();
        FieldInfo[] fieldInfos = type.GetFields(BindingFlags.Public | BindingFlags.Instance);

        List<FieldInfo> accessibles = new List<FieldInfo>();
        foreach (FieldInfo fieldInfo in fieldInfos)
        {
            if (FieldAccessAttribute.IsAccessibleField(data, fieldInfo))
            {
                accessibles.Add(fieldInfo);
            }
        }

        return accessibles;
    }

    /// <summary>
    /// 객체에서 접근 가능한 필드 정보들을 추출합니다.<para>
    /// 이 메소드는 객체내의 다른 객체 내부의 정보 또한 추출합니다.</para>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data">필드 정보들을 추출할 객체입니다.</param>
    /// <returns></returns>
    public static List<DepthFieldInfo> GetAccessibleFieldDepthInfos<T>(T data)
    {
        List<DepthFieldInfo> depthFieldInfos = new List<DepthFieldInfo>();
        Queue<DepthFieldInfo> q = new Queue<DepthFieldInfo>();

        void Search(List<FieldInfo> fieldInfos, DepthFieldInfo parent)
        {
            foreach (FieldInfo fieldInfo in fieldInfos)
            {
                DepthFieldInfo depth = new DepthFieldInfo(fieldInfo, parent);
                depthFieldInfos.Add(depth);

                bool isStruct = fieldInfo.FieldType.IsValueType && !fieldInfo.FieldType.IsPrimitive;
                bool isClass = fieldInfo.FieldType.IsClass;
                if (isStruct || isClass)
                {
                    q.Enqueue(depth);
                }
            }
        }
        Search(GetAccessibleFieldInfos(data), null);
        while (q.Count > 0)
        {
            DepthFieldInfo front = q.Dequeue();
            object value = front.GetValue(data);
            List<FieldInfo> infos = GetAccessibleFieldInfos(value);
            Search(infos, front);
        }
        return depthFieldInfos;
    }
}
