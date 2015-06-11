using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dotissi.Cache
{
    public class ConditionalWeakTable
    {
        private readonly Dictionary<object, int> _table;
        private int _capacity = 4;

        public ConditionalWeakTable()
        {
            this._table = new Dictionary<object, int>();
        }

        public void Add(object key, int value)
        {
            CleanupDeadReferences();
            this._table.Add(CreateWeakKey(key), value);
        }

        public bool Remove(object key)
        {
            return this._table.Remove(key);
        }

        public bool TryGetValue(object key, out int value)
        {
            return this._table.TryGetValue(key, out value);
        }

        private void CleanupDeadReferences()
        {
            if (this._table.Count < _capacity)
            {
                return;
            }

            object[] deadKeys = this._table.Keys
          .Where(weakRef => !((EquivalentWeakReference)weakRef).IsAlive).ToArray();

            foreach (var deadKey in deadKeys)
            {
                this._table.Remove(deadKey);
            }

            if (this._table.Count >= _capacity)
            {
                _capacity *= 2;
            }
        }

        private static object CreateWeakKey(object key)
        {
            return new EquivalentWeakReference(key);
        }

        private class EquivalentWeakReference
        {
            private readonly WeakReference _weakReference;
            private readonly int _hashCode;

            public EquivalentWeakReference(object obj)
            {
                this._hashCode = obj.GetHashCode();
                this._weakReference = new WeakReference(obj);
            }

            public bool IsAlive
            {
                get
                {
                    return this._weakReference.IsAlive;
                }
            }

            public override bool Equals(object obj)
            {
                EquivalentWeakReference weakRef = obj as EquivalentWeakReference;

                if (weakRef != null)
                {
                    obj = weakRef._weakReference.Target;
                }

                if (obj == null)
                {
                    return base.Equals(weakRef);
                }

                return object.Equals(this._weakReference.Target, obj);
            }

            public override int GetHashCode()
            {
                return this._hashCode;
            }
        }
    }
}
