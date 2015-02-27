namespace ImpalaToLinq
{
  using System;
  using System.Collections;
  using System.Collections.Generic;
  using System.Data.Common;
  using System.Linq;
  using System.Linq.Expressions;
  using System.Reflection;

  namespace ImpalaToLinq
  {
    public class ImpalaQueryable<TData> : IQueryable<TData>
    {
      public ImpalaQueryable(IQueryProvider provider)
      {
        if (provider == null)
        {
          throw new ArgumentNullException("provider");
        }
        Provider = provider;
        Expression = Expression.Constant(this);
      }

      public ImpalaQueryable(IQueryProvider provider, Expression expression)
      {
        if (provider == null)
        {
          throw new ArgumentNullException("provider");
        }
        if (expression == null)
        {
          throw new ArgumentNullException("expression");
        }
        if (!typeof(IQueryable<TData>).IsAssignableFrom(expression.Type))
        {
          throw new ArgumentOutOfRangeException("expression");
        }
        Provider = provider;
        Expression = expression;
      }

      public IEnumerator<TData> GetEnumerator()
      {
        return ((IEnumerable<TData>)Provider.Execute<TData>(Expression)).GetEnumerator();
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
        return GetEnumerator();
      }

      public Expression Expression { get; private set; }
      public Type ElementType { get { return typeof(TData); } }
      public IQueryProvider Provider { get; private set; }

      public override string ToString()
      {
        return ((QueryProvider)Provider).GetQuery(Expression);
      }
    }

    public abstract class QueryProvider : IQueryProvider
    {
      public IQueryable CreateQuery(Expression expression)
      {
        Type elementType = TypeSystem.GetElementType(expression.Type);
        try
        {
          return
            (IQueryable)
              Activator.CreateInstance(typeof(ImpalaQueryable<>).MakeGenericType(elementType),
                new object[] { this, expression });
        }
        catch (TargetInvocationException tie)
        {
          throw tie.InnerException;
        }
      }

      public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
      {
        return new ImpalaQueryable<TElement>(this, expression);
      }

      object IQueryProvider.Execute(Expression expression)
      {
        return Execute(expression);
      }

      TResult IQueryProvider.Execute<TResult>(Expression expression)
      {
        return (TResult)Execute(expression);
      }

      public abstract string GetQuery(Expression expression);
      public abstract object Execute(Expression expression);
    }

    internal static class TypeSystem
    {
      internal static Type GetElementType(Type seqType)
      {
        Type ienum = FindIEnumerable(seqType);
        if (ienum == null) return seqType;
        return ienum.GetGenericArguments()[0];
      }

      private static Type FindIEnumerable(Type seqType)
      {
        if (seqType == null || seqType == typeof(string))
          return null;
        if (seqType.IsArray)
          return typeof(IEnumerable<>).MakeGenericType(seqType.GetElementType());
        if (seqType.IsGenericType)
        {
          foreach (Type arg in seqType.GetGenericArguments())
          {
            Type ienum = typeof(IEnumerable<>).MakeGenericType(arg);
            if (ienum.IsAssignableFrom(seqType))
            {
              return ienum;
            }
          }
        }
        Type[] ifaces = seqType.GetInterfaces();
        if (ifaces != null && ifaces.Length > 0)
        {
          foreach (Type iface in ifaces)
          {
            Type ienum = FindIEnumerable(iface);
            if (ienum != null) return ienum;
          }
        }
        if (seqType.BaseType != null && seqType.BaseType != typeof(object))
        {
          return FindIEnumerable(seqType.BaseType);
        }
        return null;
      }
    }

    internal class ObjectReader<T> : IEnumerable<T>, IEnumerable where T : class, new()
    {
      private Enumerator enumerator;

      internal ObjectReader(DbDataReader reader)
      {
        enumerator = new Enumerator(reader);
      }

      public IEnumerator<T> GetEnumerator()
      {
        Enumerator e = enumerator;
        if (e == null)
        {
          throw new InvalidOperationException("Cannot enumerate more than once");
        }
        enumerator = null;
        return e;
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
        return GetEnumerator();
      }

      private class Enumerator : IEnumerator<T>, IEnumerator, IDisposable
      {
        private readonly FieldInfo[] fields;
        private readonly DbDataReader reader;
        private T current;
        private int[] fieldLookup;

        internal Enumerator(DbDataReader reader)
        {
          this.reader = reader;
          fields = typeof(T).GetFields();
        }

        public T Current
        {
          get { return current; }
        }

        object IEnumerator.Current
        {
          get { return current; }
        }

        public bool MoveNext()
        {
          if (!reader.Read()) return false;
          if (fieldLookup == null)
          {
            InitFieldLookup();
          }
          var instance = new T();
          for (int i = 0, n = fields.Length; i < n; i++)
          {
            int index = fieldLookup[i];
            if (index < 0) continue;
            var fi = fields[i];
            fi.SetValue(instance, reader.IsDBNull(index) ? null : reader.GetValue(index));
          }
          current = instance;
          return true;
        }

        public void Reset()
        {
        }

        public void Dispose()
        {
          reader.Dispose();
        }

        private void InitFieldLookup()
        {
          var map = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);
          for (int i = 0, n = reader.FieldCount; i < n; i++)
          {
            map.Add(reader.GetName(i), i);
          }
          fieldLookup = new int[fields.Length];
          for (int i = 0, n = fields.Length; i < n; i++)
          {
            int index;
            if (map.TryGetValue(fields[i].Name, out index))
            {
              fieldLookup[i] = index;
            }
            else
            {
              fieldLookup[i] = -1;
            }
          }
        }
      }
    }
  }
}
