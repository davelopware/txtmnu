/*
 * Copyright 2007 Davelopware Ltd
 * 
 * http://www.davelopware.com/txtmnu/
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *     http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software distributed
 * under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR
 * CONDITIONS OF ANY KIND, either express or implied. See the License for the
 * specific language governing permissions and limitations under the License.
 *
 */
using System;
using System.Collections;
using System.Collections.Generic;

namespace Davelopware.TxtMnu
{
	/// <summary>
	/// Typed collection of IMenuEntry instances.
	/// </summary>
	public class MenuEntries : ICollection, IEnumerable
	{
		private ArrayList _entries = new ArrayList();

		#region constructors

		public MenuEntries()
		{
		}

		#endregion

		#region IList style methods but strongly typed whereever possible

		public IMenuEntry this[int index]
		{
			get { return (IMenuEntry)_entries[index]; }
			set { _entries[index] = value; }
		}

		public IMenuEntry this[string key]
		{
			get
			{
				foreach (IMenuEntry entry in _entries)
				{
					if (entry.KeyCompare(key))
						return entry;
				}
				return null;
			}
		}

		public void RemoveAt(int index)
		{
			_entries.RemoveAt(index);
		}

		public void Insert(int index, IMenuEntry value)
		{
			_entries.Insert(index, value);
		}

		public void Remove(IMenuEntry value)
		{
			_entries.Remove(value);
		}

		public bool Contains(IMenuEntry value)
		{
			return _entries.Contains(value);
		}

		public void Clear()
		{
			_entries.Clear();
		}

		public int IndexOf(IMenuEntry value)
		{
			return _entries.IndexOf(value);
		}

		public IMenuEntry Add(IMenuEntry value)
		{
			if (!_entries.Contains(value))
				_entries.Add(value);
			return value;
		}

		#endregion

		#region ICollection Members

		public bool IsSynchronized
		{
			get
			{
				return _entries.IsSynchronized;
			}
		}

		public int Count
		{
			get
			{
				return _entries.Count;
			}
		}

		public void CopyTo(Array array, int index)
		{
			_entries.CopyTo(array, index);
		}

		public object SyncRoot
		{
			get
			{
				return _entries.SyncRoot;
			}
		}

		#endregion

		#region IEnumerable Members

		public IEnumerator GetEnumerator()
		{
			return _entries.GetEnumerator();
		}

		#endregion

		public List<T> AllOfType<T>()
        {
			List<T> items = new List<T>();
			foreach(IMenuEntry me in _entries)
            {
				if (typeof(T).IsInstanceOfType(me))
                {
					items.Add((T)me);
                }
            }
			return items;
        }
	}
}
